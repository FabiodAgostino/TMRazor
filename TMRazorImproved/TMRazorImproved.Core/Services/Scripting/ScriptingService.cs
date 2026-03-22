using IronPython.Hosting;
using IronPython.Runtime.Exceptions;
using Microsoft.Scripting.Hosting;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Mvvm.Messaging;
using TMRazorImproved.Core.Services.Scripting.Engines;
using System;
using System.Threading;
using System.Threading.Tasks;
using TMRazorImproved.Core.Services.Scripting.Api;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Enums;
using TMRazorImproved.Shared.Messages;
using System.IO;
using System.Collections.Generic;

namespace TMRazorImproved.Core.Services.Scripting
{
    /// <summary>
    /// Implementazione di IScriptingService basata su IronPython 3.4.x.
    ///
    /// ===========================================================================
    /// ARCHITETTURA DI CANCELLAZIONE A DUE LIVELLI (Critica per .NET 10)
    /// ===========================================================================
    ///
    /// LIVELLO 1 — Thread.Interrupt() (overhead zero in esecuzione normale)
    /// -----------------------------------------------------------------------
    /// StopAsync() chiama Thread.Interrupt() sul thread che esegue lo script.
    /// In .NET 5+, Thread.Interrupt() lancia ThreadInterruptedException quando
    /// il thread si trova in uno stato di attesa nativo (Thread.Sleep, Monitor.Wait,
    /// WaitHandle.WaitOne, ecc.). Non richiede nessun check overhead durante
    /// l'esecuzione normale.
    /// Cattura: qualsiasi blocking native call (Thread.Sleep, time.sleep se non
    ///          intercettato, socket wait, ecc.).
    /// LIMITE: non interrompe un loop Python puro che non chiama mai codice nativo
    ///         bloccante. Per quello serve il Livello 2.
    ///
    /// LIVELLO 2 — sys.settrace con check periodico ogni TRACE_INTERVAL statement
    /// -----------------------------------------------------------------------
    /// Intercetta puri loop Python CPU-bound. L'overhead rispetto alla versione
    /// per-statement è ridotto: la chiamata a _t_ è ancora present ad ogni riga
    /// (frame overhead inevitabile), ma il costoso cross-boundary IsCancelled
    /// check (volatile read C#→Python via DLR) avviene solo ogni TRACE_INTERVAL
    /// statement. Con TRACE_INTERVAL=50 la latenza massima di cancellazione è
    /// ~50 × avg_line_time (tipicamente < 1 ms per macro UO).
    ///
    /// OVERRIDE time.sleep()
    /// -----------------------------------------------------------------------
    /// Il preamble sostituisce sys.modules['time'] con un wrapper che reindirizza
    /// time.sleep() → Misc.Pause(). Questo intercetta il pattern più comune di
    /// "escape bloccante" involontario: l'utente che importa time e chiama
    /// time.sleep() credendo di fare una pausa normale.
    /// Misc.Pause usa ThrowIfCancelled() a ogni slice da 10ms → cancellazione
    /// quasi istantanea anche durante attese lunghe.
    ///
    /// LIVELLO 0 — Misc.Pause / API checks (complementare)
    /// -----------------------------------------------------------------------
    /// Tutti i metodi API (Pause, WaitFor, FindAll*, ecc.) chiamano
    /// _cancel.ThrowIfCancelled() in ogni iterazione del loro loop interno.
    /// Cattura: attese gestite dalle nostre API → cancellazione istantanea.
    ///
    /// ===========================================================================
    /// COMPATIBILITÀ IRONPYTHON su .NET 10
    /// ===========================================================================
    /// IronPython 3.4.2: TFM dichiarato net8.0, NuGet usa backward-compat su net10.
    /// JIT mode (non NativeAOT) → compatibile.
    ///
    /// RISCHI NOTI e MITIGAZIONI:
    /// - Proprietà C# non-virtual → AttributeError DLR binder.
    ///   FIX: tutte le API class usano 'public virtual'.
    /// - ReadOnlySpan<T>/Span<T> nelle API → ArgumentException DLR.
    ///   FIX: API espongono solo tipi primitivi (int, string, bool, uint).
    /// - Import concorrenti non thread-safe (IronPython issue #1826).
    ///   FIX: un engine per esecuzione + SemaphoreSlim(1,1).
    ///
    /// ===========================================================================
    /// </summary>
    public sealed class ScriptingService : IScriptingService, IRecipient<LoginCompleteMessage>, IDisposable
    {
        // Numero di statement Python tra un check IsCancelled e l'altro nel trace handler.
        // Valore 50: latenza max ~50×line_time (< 1ms per macro tipiche UO).
        // Aumentare per meno overhead su loop pesanti; diminuire per reattività maggiore.
        private const int TraceInterval = 50;

        private volatile bool _isSuspended;
        public bool IsSuspended => _isSuspended;

        public void Suspend() => _isSuspended = true;
        public void Resume() => _isSuspended = false;

        // Preamble iniettato prima dello script utente. Responsabile di:
        //   1. Redirigere sys.stdout/stderr → i nostri TextWriter.
        //   2. Sovrascrivere sys.modules['time'] per intercettare time.sleep().
        //   3. Installare sys.settrace con check periodico ogni TraceInterval statement.
        // __stdout__, __stderr__, __cancel__, Misc sono scope variables iniettate in C#.
        private const string TracePreamble = @"
import sys as _sys_

_sys_.stdout = __stdout__
_sys_.stderr = __stderr__

import time as _time_orig_
class _SafeTime_:
    def sleep(self, secs): Misc.Pause(int(secs * 1000))
    def __getattr__(self, name): return getattr(_time_orig_, name)
_sys_.modules['time'] = _SafeTime_()
del _SafeTime_, _time_orig_

def _make_tracer_(_c_, _interval_, _svc_, _dbg_):
    _n_ = [0]
    def _t_(frame, event, arg):
        if event == 'line' and _dbg_ is not None:
            if _dbg_.ShouldPause(frame.f_lineno):
                _dbg_.PauseHere()
                while _dbg_.IsPaused and not _c_.IsCancelled:
                    Misc.Pause(50)
        _n_[0] += 1
        if _n_[0] >= _interval_:
            _n_[0] = 0
            while _svc_.IsSuspended and not _c_.IsCancelled:
                Misc.Pause(50)
            if _c_.IsCancelled:
                raise SystemExit('Script stopped by user')
        return _t_
    return _t_

_sys_.settrace(_make_tracer_(__cancel__, __trace_interval__, __script_svc__, __debug__))
del _make_tracer_, _sys_
";

        // Rimuove il trace handler a fine esecuzione.
        private const string TraceCleanup = "import sys as _s_; _s_.settrace(None); del _s_";

        private readonly IWorldService _world;
        private readonly IPacketService _packetService;
        private readonly IClientInteropService _interopService;
        private readonly ITargetingService _targetingService;
        private readonly IJournalService _journalService;
        private readonly ISkillsService _skillsService;
        private readonly IFriendsService _friendsService;
        private readonly IHotkeyService _hotkeyService;
        private readonly IConfigService _config;
        private readonly IMessenger _messenger;
        private readonly ILogger<ScriptingService> _logger;
        private readonly ILoggerFactory _loggerFactory;

        private volatile bool _isRunning;
        private string? _currentScriptName;
        private CancellationTokenSource? _activeCts;
        private readonly System.Collections.Concurrent.ConcurrentDictionary<string, CancellationTokenSource> _activeScripts = new(StringComparer.OrdinalIgnoreCase);

        // Thread che sta eseguendo lo script — usato da StopAsync per Thread.Interrupt().
        // volatile per visibilità cross-thread senza lock.
        private volatile Thread? _scriptThread;

        // Engine e scope IronPython attivi durante l'esecuzione di uno script Python.
        // Usati da CallPythonFunction per richiamare funzioni Python da C#.
        private volatile Microsoft.Scripting.Hosting.ScriptEngine? _activePythonEngine;
        private volatile Microsoft.Scripting.Hosting.ScriptScope? _activePythonScope;

        // Un solo script alla volta
        private readonly SemaphoreSlim _executionLock = new(1, 1);

        // Debug controller — persiste tra esecuzioni (breakpoint rimangono), ma stato pausa/step resettato
        private readonly ScriptDebugController _debugController = new();

        public bool IsRunning => _isRunning;
        public string? CurrentScriptName => _currentScriptName;

        // Debug interface
        public bool IsPaused => _debugController.IsPaused;
        public int CurrentDebugLine => _debugController.CurrentLine;
        public event Action<int>? DebugPaused;
        public void SetBreakpoint(int line) => _debugController.SetBreakpoint(line);
        public void ClearBreakpoint(int line) => _debugController.ClearBreakpoint(line);
        public void ClearAllBreakpoints() => _debugController.ClearAll();
        public IEnumerable<int> GetBreakpoints() => _debugController.Breakpoints;
        public void ContinueExecution() => _debugController.Continue();
        public void StepInto() => _debugController.StepInto();

        // FR-079: Inspector Debug API
        public IReadOnlyDictionary<string, object> GetSharedScriptData()
            => new System.Collections.Generic.Dictionary<string, object>(MiscApi.SharedValues);

        public IReadOnlyList<ScriptTimerInfo> GetActiveTimers()
        {
            var result = new System.Collections.Generic.List<ScriptTimerInfo>();
            foreach (var (name, timer) in TimerApi.Timers)
                result.Add(new ScriptTimerInfo(name, timer.Interval, Math.Max(0, timer.TimeLeft), timer.Enabled));
            return result;
        }

        public event Action<string?>? ScriptsChanged;
        public event Action<string>? OutputReceived;
        public event Action<string>? ErrorReceived;
        public event Action<ScriptCompletionInfo>? ScriptCompleted;

        private FileSystemWatcher? _scriptsWatcher;

        private readonly IAutoLootService _autoLoot;
        private readonly IScavengerService _scavenger;
        private readonly IOrganizerService _organizer;
        private readonly IBandageHealService _bandageHeal;
        private readonly IDressService _dress;
        private readonly IRestockService _restock;
        private readonly IVendorService _vendor;
        private readonly ISecureTradeService _secureTrade;
        private readonly ISoundService _sound;
        private readonly IMacrosService _macros;
        private readonly IPathFindingService _pathfinding;
        private readonly ICounterService _counter;
        private readonly IDPSMeterService _dpsMeter;
        private readonly IPacketLoggerService _packetLogger;
        private readonly IMultiService _multiService;
        private readonly IDragDropCoordinator? _dragDropCoordinator;

        public ScriptingService(
            IWorldService world,
            IPacketService packetService,
            IClientInteropService interopService,
            ITargetingService targetingService,
            IJournalService journalService,
            ISkillsService skillsService,
            IFriendsService friendsService,
            IHotkeyService hotkeyService,
            IConfigService config,
            IAutoLootService autoLoot,
            IScavengerService scavenger,
            IOrganizerService organizer,
            IBandageHealService bandageHeal,
            IDressService dress,
            IRestockService restock,
            IVendorService vendor,
            ISecureTradeService secureTrade,
            ISoundService sound,
            IMacrosService macros,
            IPathFindingService pathfinding,
            ICounterService counter,
            IDPSMeterService dpsMeter,
            IPacketLoggerService packetLogger,
            IMultiService multiService,
            IMessenger messenger,
            ILogger<ScriptingService> logger,
            ILoggerFactory loggerFactory,
            IDragDropCoordinator? dragDropCoordinator = null)
        {
            _world = world;
            _packetService = packetService;
            _interopService = interopService;
            _targetingService = targetingService;
            _journalService = journalService;
            _skillsService = skillsService;
            _friendsService = friendsService;
            _hotkeyService = hotkeyService;
            _config = config;
            _autoLoot = autoLoot;
            _scavenger = scavenger;
            _organizer = organizer;
            _bandageHeal = bandageHeal;
            _dress = dress;
            _restock = restock;
            _vendor = vendor;
            _secureTrade = secureTrade;
            _sound = sound;
            _macros = macros;
            _pathfinding = pathfinding;
            _counter = counter;
            _dpsMeter = dpsMeter;
            _packetLogger = packetLogger;
            _multiService = multiService;
            _messenger = messenger;
            _logger = logger;
            _loggerFactory = loggerFactory;
            _dragDropCoordinator = dragDropCoordinator;

            messenger.RegisterAll(this);
            InitScriptsWatcher(_config.Global.ScriptsPath);

            // Propaga l'evento DebugPaused dal controller all'interfaccia pubblica
            _debugController.DebugPaused += line => DebugPaused?.Invoke(line);
        }

        private void InitScriptsWatcher(string? path)
        {
            _scriptsWatcher?.Dispose();
            _scriptsWatcher = null;

            if (string.IsNullOrWhiteSpace(path) || !System.IO.Directory.Exists(path)) return;

            var w = new FileSystemWatcher(path)
            {
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                Filter = "*.*",
                EnableRaisingEvents = true
            };

            w.Created += (_, e) => ScriptsChanged?.Invoke(e.FullPath);
            w.Deleted += (_, e) => ScriptsChanged?.Invoke(e.FullPath);
            w.Renamed += (_, e) => ScriptsChanged?.Invoke(e.FullPath);
            w.Changed += (_, e) => ScriptsChanged?.Invoke(e.FullPath);

            _scriptsWatcher = w;
            _logger.LogDebug("FileSystemWatcher attivo su: {Path}", path);
        }

        public IEnumerable<string> GetLoadedScripts()
        {
            var dir = _config.Global.ScriptsPath;
            if (string.IsNullOrEmpty(dir) || !System.IO.Directory.Exists(dir)) return Enumerable.Empty<string>();
            
            return System.IO.Directory.GetFiles(dir, "*.*", System.IO.SearchOption.AllDirectories)
                .Where(f => f.EndsWith(".py") || f.EndsWith(".uos") || f.EndsWith(".cs"))
                .Select(f => System.IO.Path.GetFileName(f));
        }

        public async Task RunScript(string name, bool loop = false)
        {
            var dir = _config.Global.ScriptsPath;
            if (string.IsNullOrEmpty(dir) || !System.IO.Directory.Exists(dir)) return;

            var files = System.IO.Directory.GetFiles(dir, name, System.IO.SearchOption.AllDirectories);
            if (files.Length == 0) return;

            var path = files[0];
            var code = await System.IO.File.ReadAllTextAsync(path);
            var lang = path.EndsWith(".py") ? ScriptLanguage.Python :
                       path.EndsWith(".uos") ? ScriptLanguage.UOSteam : ScriptLanguage.CSharp;

            await RunAsync(code, lang, name, default, loop);
        }

        public IEnumerable<string> ValidateScript(string code, ScriptLanguage language)
        {
            var warnings = new List<string>();
            if (string.IsNullOrWhiteSpace(code)) return warnings;

            if (language == ScriptLanguage.Python)
            {
                string[] dangerousKeywords = { "import os", "import subprocess", "import shutil", "eval(", "exec(", "open(", "socket" };
                foreach (var kw in dangerousKeywords)
                {
                    if (code.Contains(kw, StringComparison.OrdinalIgnoreCase))
                    {
                        warnings.Add($"ATTENZIONE: Lo script contiene chiamate potenzialmente pericolose: '{kw}'.");
                    }
                }
            }
            else if (language == ScriptLanguage.CSharp)
            {
                string[] dangerousKeywords = { "System.IO", "Process.", "Registry.", "Socket", "HttpClient" };
                foreach (var kw in dangerousKeywords)
                {
                    if (code.Contains(kw, StringComparison.OrdinalIgnoreCase))
                    {
                        warnings.Add($"ATTENZIONE: Lo script C# utilizza API di sistema sensibili: '{kw}'.");
                    }
                }
            }

            return warnings;
        }

        // ------------------------------------------------------------------
        // Esecuzione pubblica
        // ------------------------------------------------------------------

        public async Task RunAsync(string code, ScriptLanguage language = ScriptLanguage.Python,
                                   string scriptName = "unnamed",
                                   CancellationToken externalToken = default,
                                   bool loop = false)
        {
            // Rifiuta se uno script è già in esecuzione (non-blocking check)
            if (!await _executionLock.WaitAsync(0))
            {
                _logger.LogWarning("Execution rejected: script '{CurrentScript}' is already running.", _currentScriptName);
                ErrorReceived?.Invoke(
                    $"[ScriptingService] Script '{_currentScriptName}' già in esecuzione. " +
                    "Chiamare StopAsync() prima di avviarne un altro.");
                return;
            }

            _logger.LogInformation("Starting script: {ScriptName} [{Language}] loop={Loop}", scriptName, language, loop);

            // Crea un CTS combinato: può essere cancellato dall'esterno o da StopAsync()
            var cts = CancellationTokenSource.CreateLinkedTokenSource(externalToken);
            _activeCts = cts;
            _currentScriptName = scriptName;
            _isRunning = true;
            _activeScripts[scriptName] = cts;
            var start = DateTime.UtcNow;

            try
            {
                await Task.Run(() =>
                {
                    do
                    {
                        cts.Token.ThrowIfCancellationRequested();
                        switch (language)
                        {
                            case ScriptLanguage.Python:
                                ExecutePythonInternal(code, scriptName, cts);
                                break;
                            case ScriptLanguage.UOSteam:
                                ExecuteUOSteamInternal(code, scriptName, cts);
                                break;
                            case ScriptLanguage.CSharp:
                                ExecuteCSharpInternal(code, scriptName, cts);
                                break;
                            default:
                                throw new NotSupportedException($"Linguaggio {language} non supportato.");
                        }
                    }
                    while (loop && !cts.Token.IsCancellationRequested);
                }, externalToken);

                _logger.LogInformation("Script '{ScriptName}' completed successfully in {Duration}", scriptName, DateTime.UtcNow - start);
                ScriptCompleted?.Invoke(new ScriptCompletionInfo(
                    scriptName, false, null, DateTime.UtcNow - start));
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Script '{ScriptName}' was cancelled after {Duration}", scriptName, DateTime.UtcNow - start);
                ScriptCompleted?.Invoke(new ScriptCompletionInfo(
                    scriptName, true, null, DateTime.UtcNow - start));
            }
            catch (ThreadInterruptedException)
            {
                _logger.LogInformation("Script '{ScriptName}' was interrupted after {Duration}", scriptName, DateTime.UtcNow - start);
                ScriptCompleted?.Invoke(new ScriptCompletionInfo(
                    scriptName, true, null, DateTime.UtcNow - start));
            }
            catch (Exception ex)
            {
                var msg = FormatException(ex);
                _logger.LogError(ex, "Error executing script '{ScriptName}': {Message}", scriptName, msg);
                ErrorReceived?.Invoke(msg);
                ScriptCompleted?.Invoke(new ScriptCompletionInfo(
                    scriptName, false, ex, DateTime.UtcNow - start));
            }
            finally
            {
                _isRunning = false;
                _currentScriptName = null;
                _activeCts = null;
                _activeScripts.TryRemove(scriptName, out _);
                cts.Dispose();
                
                try { _executionLock.Release(); }
                catch (SemaphoreFullException) { }
            }
        }

        public async Task StopAsync()
        {
            var cts = _activeCts;
            if (cts == null || !_isRunning) return;

            _logger.LogInformation("Stopping current script: {ScriptName}", _currentScriptName);

            // Segnala la cancellazione al CTS (letto da Python trace e Roslyn token)
            cts.Cancel();

            // Interrupt per blocchi nativi (.NET Wait, Sleep, ecc.)
            _scriptThread?.Interrupt();

            // Attende fino a 5s che il task termini
            bool acquired = await _executionLock.WaitAsync(TimeSpan.FromSeconds(5));

            if (acquired)
            {
                _executionLock.Release();
                _logger.LogDebug("Script stopped normally within timeout.");
            }
            else
            {
                _logger.LogWarning("Script '{ScriptName}' failed to stop within 5s and is now a zombie.", _currentScriptName);
                _isRunning = false;
                _currentScriptName = null;
                _activeCts = null;

                ErrorReceived?.Invoke(
                    "[ScriptingService] Lo script non ha risposto entro 5 secondi. " +
                    "Lo stato è stato resettato. Lo script zombie terminerà al prossimo check di cancellazione.");

                ScriptCompleted?.Invoke(new ScriptCompletionInfo(
                    _currentScriptName ?? "zombie", true, null, TimeSpan.FromSeconds(5)));
            }
        }

        public void StopScript(string name)
        {
            if (_activeScripts.TryGetValue(name, out var cts))
            {
                _logger.LogInformation("Stopping script by name: {ScriptName}", name);
                try
                {
                    cts.Cancel();
                }
                catch (ObjectDisposedException) { }
                
                if (string.Equals(_currentScriptName, name, StringComparison.OrdinalIgnoreCase))
                {
                    _scriptThread?.Interrupt();
                }
            }
        }

        // ------------------------------------------------------------------
        // Autostart (triggered on LoginCompleteMessage)
        // ------------------------------------------------------------------

        /// <summary>Riceve il messaggio di login completato e avvia gli script marcati come autostart.</summary>
        public void Receive(LoginCompleteMessage message) => AutoStartScripts();

        /// <summary>
        /// Avvia tutti gli script nel profilo corrente marcati con AutoStart = true.
        /// Se lo script ha anche Loop = true, viene eseguito in ciclo continuo.
        /// </summary>
        public void AutoStartScripts()
        {
            var scripts = _config.CurrentProfile.Scripts;
            if (scripts == null || scripts.Count == 0) return;

            foreach (var cfg in scripts)
            {
                if (!cfg.AutoStart || string.IsNullOrWhiteSpace(cfg.Name)) continue;

                _logger.LogInformation("AutoStart: avvio script '{Name}' (loop={Loop})", cfg.Name, cfg.Loop);
                // Fire-and-forget: ogni script gira in background indipendentemente
                _ = Task.Run(() => RunScript(cfg.Name, cfg.Loop));
            }
        }

        // ------------------------------------------------------------------
        // Preload (warm-up compilazione Roslyn per script C# marcati Preload=true)
        // ------------------------------------------------------------------

        /// <summary>
        /// Pre-compila in background tutti gli script C# marcati con Preload=true nel profilo corrente.
        /// Non esegue gli script; serve solo a riscaldare il JIT Roslyn per la prima esecuzione.
        /// </summary>
        public async Task PreloadScripts()
        {
            var scripts = _config.CurrentProfile.Scripts;
            if (scripts == null || scripts.Count == 0) return;

            var dir = _config.Global.ScriptsPath;
            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) return;

            var engine = new CSharpScriptEngine(
                line => _logger.LogDebug("[Preload] {Line}", line),
                line => _logger.LogWarning("[Preload] {Line}", line));

            foreach (var cfg in scripts)
            {
                if (!cfg.Preload || string.IsNullOrWhiteSpace(cfg.Name)) continue;
                if (!cfg.Name.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)) continue;

                var files = Directory.GetFiles(dir, cfg.Name, SearchOption.AllDirectories);
                if (files.Length == 0)
                {
                    _logger.LogWarning("[Preload] Script '{Name}' non trovato in {Dir}", cfg.Name, dir);
                    continue;
                }

                var path = files[0];
                _logger.LogInformation("[Preload] Pre-compilazione: {Name}", cfg.Name);

                await Task.Run(() =>
                {
                    try
                    {
                        var code = File.ReadAllText(path);
                        var scriptDir = Path.GetDirectoryName(path);
                        var error = engine.Precompile(code, scriptDir);
                        if (error != null)
                            _logger.LogWarning("[Preload] Errori in '{Name}': {Error}", cfg.Name, error);
                        else
                            _logger.LogInformation("[Preload] '{Name}' compilato con successo.", cfg.Name);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "[Preload] Fallita pre-compilazione di '{Name}'", cfg.Name);
                    }
                });
            }
        }

        // ------------------------------------------------------------------
        // Esecuzione Python (IronPython)
        // ------------------------------------------------------------------

        private void ExecutePythonInternal(string code, string scriptName, CancellationTokenSource cts)
        {
            _scriptThread = Thread.CurrentThread;
            _logger.LogDebug("Creating new IronPython engine for script: {ScriptName}", scriptName);
            
            var engine = IronPython.Hosting.Python.CreateEngine();
            var scope  = engine.CreateScope();

            var stdout = new ScriptOutputWriter(line => OutputReceived?.Invoke(line));
            var stderr = new ScriptOutputWriter(line => ErrorReceived?.Invoke(line));

            var cancelCtrl = new ScriptCancellationController(cts.Token);
            var miscApi    = new MiscApi(_world, _packetService, _interopService, cancelCtrl, line => OutputReceived?.Invoke(line), _targetingService, hotkeyService: _hotkeyService);

            // Reset stato pausa/step (mantiene i breakpoint impostati prima dell'esecuzione)
            _debugController.Continue();

            scope.SetVariable("__stdout__",        stdout);
            scope.SetVariable("__stderr__",        stderr);
            scope.SetVariable("__cancel__",        cancelCtrl);
            scope.SetVariable("__trace_interval__", TraceInterval);
            scope.SetVariable("__script_svc__",     this);
            scope.SetVariable("__debug__",          _debugController);
            scope.SetVariable("Misc",              miscApi);
            var itemsApi = new ItemsApi(_world, _packetService, _targetingService, cancelCtrl, _loggerFactory.CreateLogger<ItemsApi>(), _messenger, null, _dragDropCoordinator);
            var mobilesApi = new MobilesApi(_world, _friendsService, _packetService, _targetingService, cancelCtrl, _loggerFactory.CreateLogger<MobilesApi>());
            var staticsApi = new StaticsApi(cancelCtrl, _multiService);
            
            scope.SetVariable("Items",   itemsApi);
            scope.SetVariable("Mobiles", mobilesApi);
            scope.SetVariable("Player",  new PlayerApi(_world, _packetService, _targetingService, _skillsService, cancelCtrl, _interopService, logger: _loggerFactory.CreateLogger<PlayerApi>()));
            scope.SetVariable("Journal", new JournalApi(_journalService, cancelCtrl));
            scope.SetVariable("Gump",   new GumpsApi(_world, _packetService, cancelCtrl, _messenger));
            scope.SetVariable("Target",  new TargetApi(_targetingService, _world, _config, _packetService, cancelCtrl));
            scope.SetVariable("Skills",  new SkillsApi(_skillsService, _packetService, cancelCtrl));
            scope.SetVariable("Spells",  new SpellsApi(_world, _packetService, cancelCtrl, _targetingService, _messenger, _loggerFactory.CreateLogger<SpellsApi>()));
            scope.SetVariable("Statics", staticsApi);
            scope.SetVariable("Friend",  new FriendApi(_friendsService, _targetingService, _world, cancelCtrl));
            scope.SetVariable("Filters", new FiltersApi(_config, cancelCtrl));
            scope.SetVariable("Timer",   new TimerApi(cancelCtrl, miscApi));
            scope.SetVariable("SpecialMoves", new SpecialMovesApi(_world, _packetService, cancelCtrl));
            scope.SetVariable("Sound", new SoundApi(_sound, _world, _targetingService, cancelCtrl, _packetService));
            scope.SetVariable("Hotkey", new HotkeyApi(_hotkeyService, _config, cancelCtrl));
            scope.SetVariable("Trade",  new TradeApi(_secureTrade, cancelCtrl));
            scope.SetVariable("CUO",    new CuoApi(_packetService, _interopService, _world, cancelCtrl, _loggerFactory.CreateLogger<CuoApi>()));

            // Agents
            scope.SetVariable("AutoLoot",    new AutoLootApi(_autoLoot, cancelCtrl));
            scope.SetVariable("Dress",       new DressApi(_dress, cancelCtrl));
            scope.SetVariable("Scavenger",   new ScavengerApi(_scavenger, cancelCtrl));
            scope.SetVariable("Restock",     new RestockApi(_restock, cancelCtrl));
            scope.SetVariable("Organizer",   new OrganizerApi(_organizer, cancelCtrl));
            scope.SetVariable("BandageHeal", new BandageHealApi(_bandageHeal, cancelCtrl));
            scope.SetVariable("PathFinding",  new PathFindingApi(_pathfinding, _world, _packetService, cancelCtrl));
            scope.SetVariable("Counter",      new CounterApi(_counter, cancelCtrl));
            scope.SetVariable("DPSMeter",     new DPSMeterApi(_dpsMeter, cancelCtrl));
            scope.SetVariable("PacketLogger", new PacketLoggerApi(_packetLogger, cancelCtrl));

            engine.Execute(TracePreamble, scope);

            // Esponi engine/scope per CallPythonFunction (callback C# -> Python)
            _activePythonEngine = engine;
            _activePythonScope  = scope;

            try
            {
                engine.Execute(code, scope);
            }
            catch (IronPython.Runtime.Exceptions.SystemExitException)
            {
                throw new OperationCanceledException("Script interrotto via sys.settrace.");
            }
            finally
            {
                _activePythonEngine = null;
                _activePythonScope  = null;
                _scriptThread = null;
                try { engine.Execute(TraceCleanup, scope); } catch { }
                // FIX BUG-C02: dispose esplicito del runtime IronPython per evitare memory leak
                try { engine.Runtime.Shutdown(); } catch { }
                stdout.Dispose();
                stderr.Dispose();
            }
        }

        // ------------------------------------------------------------------
        // Esecuzione UOSteam
        // ------------------------------------------------------------------

        private void ExecuteUOSteamInternal(string code, string scriptName, CancellationTokenSource cts)
        {
            _scriptThread = Thread.CurrentThread;

            var cancelCtrl = new ScriptCancellationController(cts.Token);
            var miscApi    = new MiscApi(_world, _packetService, _interopService, cancelCtrl, line => OutputReceived?.Invoke(line), _targetingService, hotkeyService: _hotkeyService);
            var itemsApi   = new ItemsApi(_world, _packetService, _targetingService, cancelCtrl, _loggerFactory.CreateLogger<ItemsApi>(), _messenger, null, _dragDropCoordinator);
            var mobilesApi = new MobilesApi(_world, _friendsService, _packetService, _targetingService, cancelCtrl, _loggerFactory.CreateLogger<MobilesApi>());
            var playerApi  = new PlayerApi(_world, _packetService, _targetingService, _skillsService, cancelCtrl, _interopService, logger: _loggerFactory.CreateLogger<PlayerApi>(), config: _config);
            var journalApi = new JournalApi(_journalService, cancelCtrl);
            var targetApi  = new TargetApi(_targetingService, _world, _config, _packetService, cancelCtrl);
            var skillsApi  = new SkillsApi(_skillsService, _packetService, cancelCtrl);
            var gumpsApi   = new GumpsApi(_world, _packetService, cancelCtrl, _messenger);
            var autoLootApi = new AutoLootApi(_autoLoot, cancelCtrl);
            var dressApi = new DressApi(_dress, cancelCtrl);
            var scavengerApi = new ScavengerApi(_scavenger, cancelCtrl);
            var restockApi = new RestockApi(_restock, cancelCtrl);
            var organizerApi = new OrganizerApi(_organizer, cancelCtrl);
            var bandageHealApi = new BandageHealApi(_bandageHeal, cancelCtrl);
            var hotkeyApi = new HotkeyApi(_hotkeyService, _config, cancelCtrl);
            var vendorApi = new VendorApi(_vendor, cancelCtrl);

            var interpreter = new UOSteamInterpreter(
                miscApi,
                playerApi,
                itemsApi,
                mobilesApi,
                journalApi,
                targetApi,
                skillsApi,
                gumpsApi,
                autoLootApi,
                dressApi,
                scavengerApi,
                restockApi,
                organizerApi,
                bandageHealApi,
                hotkeyApi,
                vendorApi,
                _friendsService,
                _macros,
                _world,
                cancelCtrl,
                line => OutputReceived?.Invoke(line));
            interpreter.Execute(code);
        }
        // ------------------------------------------------------------------
        // Esecuzione C# (Roslyn) — delegata a CSharpScriptEngine
        // ------------------------------------------------------------------

        private void ExecuteCSharpInternal(string code, string scriptName, CancellationTokenSource cts)
        {
            _scriptThread = Thread.CurrentThread;
            _logger.LogDebug("Starting Roslyn C# script execution: {ScriptName}", scriptName);

            var cancelCtrl = new ScriptCancellationController(cts.Token);
            var misc = new MiscApi(_world, _packetService, _interopService, cancelCtrl, line => OutputReceived?.Invoke(line), _targetingService, hotkeyService: _hotkeyService);
            var itemsApi = new ItemsApi(_world, _packetService, _targetingService, cancelCtrl, _loggerFactory.CreateLogger<ItemsApi>(), _messenger, null, _dragDropCoordinator);
            var mobilesApi = new MobilesApi(_world, _friendsService, _packetService, _targetingService, cancelCtrl, _loggerFactory.CreateLogger<MobilesApi>());
            var staticsApi = new StaticsApi(cancelCtrl, _multiService);
            var globals = new ScriptGlobals
            {
                Player      = new PlayerApi(_world, _packetService, _targetingService, _skillsService, cancelCtrl, _interopService, logger: _loggerFactory.CreateLogger<PlayerApi>()),
                Items       = itemsApi,
                Mobiles     = mobilesApi,
                Misc        = misc,
                Journal     = new JournalApi(_journalService, cancelCtrl),
                Gump        = new GumpsApi(_world, _packetService, cancelCtrl, _messenger),
                Target      = new TargetApi(_targetingService, _world, _config, _packetService, cancelCtrl),
                Skills      = new SkillsApi(_skillsService, _packetService, cancelCtrl),                Spells      = new SpellsApi(_world, _packetService, cancelCtrl, _targetingService, _messenger, _loggerFactory.CreateLogger<SpellsApi>()),
                Statics     = staticsApi,
                Friend      = new FriendApi(_friendsService, _targetingService, _world, cancelCtrl),
                Filters     = new FiltersApi(_config, cancelCtrl),
                Timer       = new TimerApi(cancelCtrl, misc),
                SpecialMoves = new SpecialMovesApi(_world, _packetService, cancelCtrl),
                Sound       = new SoundApi(_sound, _world, _targetingService, cancelCtrl, _packetService),
                Hotkey      = new HotkeyApi(_hotkeyService, _config, cancelCtrl),
                Trade       = new TradeApi(_secureTrade, cancelCtrl),
                CUO         = new CuoApi(_packetService, _interopService, _world, cancelCtrl, _loggerFactory.CreateLogger<CuoApi>()),
                AutoLoot    = new AutoLootApi(_autoLoot, cancelCtrl),
                Dress       = new DressApi(_dress, cancelCtrl),
                Scavenger   = new ScavengerApi(_scavenger, cancelCtrl),
                Restock     = new RestockApi(_restock, cancelCtrl),
                Organizer   = new OrganizerApi(_organizer, cancelCtrl),
                BandageHeal  = new BandageHealApi(_bandageHeal, cancelCtrl),
                PathFinding   = new PathFindingApi(_pathfinding, _world, _packetService, cancelCtrl),
                Counter       = new CounterApi(_counter, cancelCtrl),
                DPSMeter      = new DPSMeterApi(_dpsMeter, cancelCtrl),
                PacketLogger  = new PacketLoggerApi(_packetLogger, cancelCtrl),
                // Espone il token direttamente agli script per cancellazione cooperativa:
                // ScriptToken.ThrowIfCancellationRequested() in qualsiasi loop dello script.
                ScriptToken = cts.Token
            };

            var engine = new CSharpScriptEngine(
                line => OutputReceived?.Invoke(line),
                line => ErrorReceived?.Invoke(line));

            // Usa la ScriptsPath come directory base per la risoluzione di //#import e //#assembly.
            // Se scriptName è un percorso assoluto, usa la sua directory.
            var scriptDir = System.IO.Path.IsPathRooted(scriptName) && System.IO.File.Exists(scriptName)
                ? System.IO.Path.GetDirectoryName(scriptName)
                : (_config?.Global?.ScriptsPath ?? System.IO.Directory.GetCurrentDirectory());

            try
            {
                engine.Execute(code, globals, scriptDir);
            }
            finally
            {
                _scriptThread = null;
            }
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        /// <inheritdoc/>
        public object? CallPythonFunction(string functionName, params object[] args)
        {
            var engine = _activePythonEngine;
            var scope  = _activePythonScope;
            if (engine == null || scope == null) return null;

            try
            {
                if (!scope.ContainsVariable(functionName)) return null;
                var func = scope.GetVariable(functionName);
                return engine.Operations.Invoke(func, args);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "CallPythonFunction('{FunctionName}') failed.", functionName);
                return null;
            }
        }

        private static string FormatException(Exception ex)
        {
            // Le eccezioni IronPython contengono spesso informazioni Python nella Message
            return $"[Errore Script] {ex.GetType().Name}: {ex.Message}";
        }

        public void Dispose()
        {
            _activeCts?.Cancel();
            _executionLock.Dispose();
            _scriptsWatcher?.Dispose();
        }
    }
}
