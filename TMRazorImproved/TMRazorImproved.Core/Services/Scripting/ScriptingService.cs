using IronPython.Hosting;
using IronPython.Runtime.Exceptions;
using Microsoft.Scripting.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Threading;
using System.Threading.Tasks;
using TMRazorImproved.Core.Services.Scripting.Api;
using TMRazorImproved.Core.Services.Scripting.Engines;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Enums;

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
    public sealed class ScriptingService : IScriptingService, IDisposable
    {
        // Numero di statement Python tra un check IsCancelled e l'altro nel trace handler.
        // Valore 50: latenza max ~50×line_time (< 1ms per macro tipiche UO).
        // Aumentare per meno overhead su loop pesanti; diminuire per reattività maggiore.
        private const int TraceInterval = 50;

        // Preamble iniettato prima dello script utente. Responsabile di:
        //   1. Redirigere sys.stdout/stderr → i nostri TextWriter.
        //   2. Sovrascrivere sys.modules['time'] per intercettare time.sleep().
        //   3. Installare sys.settrace con check periodico ogni TraceInterval statement.
        // __stdout__, __stderr__, __cancel__, Misc sono scope variables iniettate in C#.
        private const string TracePreamble = @"
import sys as _sys_

# --- 1. Redirect output ---
_sys_.stdout = __stdout__
_sys_.stderr = __stderr__

# --- 2. Override time.sleep() → Misc.Pause() (cancellation-aware) ---
# Sostituisce sys.modules['time'] con un wrapper.
# 'import time; time.sleep(x)' e 'from time import sleep; sleep(x)' vengono
# intercettati. Tutti gli altri attributi del modulo time sono inoltrati
# all'originale via __getattr__.
import time as _time_orig_
class _SafeTime_:
    def sleep(self, secs): Misc.Pause(int(secs * 1000))
    def __getattr__(self, name): return getattr(_time_orig_, name)
_sys_.modules['time'] = _SafeTime_()
del _SafeTime_, _time_orig_

# --- 3. sys.settrace con check periodico ---
# _t_ viene chiamato per ogni riga Python (overhead di frame inevitabile),
# ma IsCancelled (cross-boundary DLR call) è letto solo ogni _INTERVAL_ righe.
_INTERVAL_ = __trace_interval__
def _make_tracer_(_c_):
    _n_ = [0]
    def _t_(frame, event, arg):
        _n_[0] += 1
        if _n_[0] >= _INTERVAL_:
            _n_[0] = 0
            if _c_.IsCancelled:
                raise SystemExit('Script stopped by user')
        return _t_
    return _t_

_sys_.settrace(_make_tracer_(__cancel__))
del _make_tracer_, _INTERVAL_, _sys_
";

        // Rimuove il trace handler a fine esecuzione.
        private const string TraceCleanup = "import sys as _s_; _s_.settrace(None); del _s_";

        private readonly IWorldService _world;
        private readonly IPacketService _packetService;
        private readonly ITargetingService _targetingService;
        private readonly IJournalService _journalService;
        private readonly ISkillsService _skillsService;
        private readonly IConfigService _config;
        private readonly ILogger<ScriptingService> _logger;

        private volatile bool _isRunning;
        private string? _currentScriptName;
        private CancellationTokenSource? _activeCts;

        // Thread che sta eseguendo lo script — usato da StopAsync per Thread.Interrupt().
        // volatile per visibilità cross-thread senza lock.
        private volatile Thread? _scriptThread;

        // Un solo script alla volta
        private readonly SemaphoreSlim _executionLock = new(1, 1);

        public bool IsRunning => _isRunning;
        public string? CurrentScriptName => _currentScriptName;

        public event Action<string>? OutputReceived;
        public event Action<string>? ErrorReceived;
        public event Action<ScriptCompletionInfo>? ScriptCompleted;

        public ScriptingService(
            IWorldService world, 
            IPacketService packetService, 
            ITargetingService targetingService, 
            IJournalService journalService,
            ISkillsService skillsService,
            IConfigService config,
            ILogger<ScriptingService> logger)
        {
            _world = world;
            _packetService = packetService;
            _targetingService = targetingService;
            _journalService = journalService;
            _skillsService = skillsService;
            _config = config;
            _logger = logger;
        }

        public IEnumerable<string> GetLoadedScripts()
        {
            var dir = _config.Global.ScriptsPath;
            if (string.IsNullOrEmpty(dir) || !System.IO.Directory.Exists(dir)) return Enumerable.Empty<string>();
            
            return System.IO.Directory.GetFiles(dir, "*.*", System.IO.SearchOption.AllDirectories)
                .Where(f => f.EndsWith(".py") || f.EndsWith(".uos") || f.EndsWith(".cs"))
                .Select(f => System.IO.Path.GetFileName(f));
        }

        public async Task RunScript(string name)
        {
            var dir = _config.Global.ScriptsPath;
            if (string.IsNullOrEmpty(dir) || !System.IO.Directory.Exists(dir)) return;

            var files = System.IO.Directory.GetFiles(dir, name, System.IO.SearchOption.AllDirectories);
            if (files.Length == 0) return;

            var path = files[0];
            var code = await System.IO.File.ReadAllTextAsync(path);
            var lang = path.EndsWith(".py") ? ScriptLanguage.Python :
                       path.EndsWith(".uos") ? ScriptLanguage.UOSteam : ScriptLanguage.CSharp;

            await RunAsync(code, lang, name);
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
                                   CancellationToken externalToken = default)
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

            _logger.LogInformation("Starting script: {ScriptName} [{Language}]", scriptName, language);

            // Crea un CTS combinato: può essere cancellato dall'esterno o da StopAsync()
            var cts = CancellationTokenSource.CreateLinkedTokenSource(externalToken);
            _activeCts = cts;
            _currentScriptName = scriptName;
            _isRunning = true;
            var start = DateTime.UtcNow;

            try
            {
                await Task.Run(() => 
                {
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
            var miscApi    = new MiscApi(_world, cancelCtrl, line => OutputReceived?.Invoke(line));

            scope.SetVariable("__stdout__",        stdout);
            scope.SetVariable("__stderr__",        stderr);
            scope.SetVariable("__cancel__",        cancelCtrl);
            scope.SetVariable("__trace_interval__", TraceInterval);
            scope.SetVariable("Misc",              miscApi);
            scope.SetVariable("Items",   new ItemsApi(_world, _packetService, cancelCtrl));
            scope.SetVariable("Mobiles", new MobilesApi(_world, cancelCtrl));
            scope.SetVariable("Player",  new PlayerApi(_world, _packetService, _targetingService, cancelCtrl));
            scope.SetVariable("Journal", new JournalApi(_journalService, cancelCtrl));
            scope.SetVariable("Gumps",   new GumpsApi(_world, _packetService, cancelCtrl));
            scope.SetVariable("Target",  new TargetApi(_targetingService, cancelCtrl));
            scope.SetVariable("Skills",  new SkillsApi(_skillsService, _packetService, cancelCtrl));
            scope.SetVariable("Spells",  new SpellsApi(_packetService, cancelCtrl));
            scope.SetVariable("Statics", new StaticsApi(cancelCtrl));
            scope.SetVariable("Friend",  new FriendApi(cancelCtrl));
            scope.SetVariable("Filters", new FiltersApi(cancelCtrl));

            engine.Execute(TracePreamble, scope);

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
                _scriptThread = null;
                try { engine.Execute(TraceCleanup, scope); } catch { }
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
            var miscApi    = new MiscApi(_world, cancelCtrl, line => OutputReceived?.Invoke(line));
            var itemsApi   = new ItemsApi(_world, _packetService, cancelCtrl);
            var mobilesApi = new MobilesApi(_world, cancelCtrl);
            var playerApi  = new PlayerApi(_world, _packetService, _targetingService, cancelCtrl);
            
            var interpreter = new UOSteamInterpreter(
                miscApi, 
                playerApi, 
                itemsApi, 
                mobilesApi, 
                cancelCtrl, 
                line => OutputReceived?.Invoke(line));

            interpreter.Execute(code);
        }

        // ------------------------------------------------------------------
        // Esecuzione C# (Roslyn)
        // ------------------------------------------------------------------

        private void ExecuteCSharpInternal(string code, string scriptName, CancellationTokenSource cts)
        {
            _scriptThread = Thread.CurrentThread;
            _logger.LogDebug("Starting Roslyn C# script execution: {ScriptName}", scriptName);

            var cancelCtrl = new ScriptCancellationController(cts.Token);
            var globals = new ScriptGlobals
            {
                Player   = new PlayerApi(_world, _packetService, _targetingService, cancelCtrl),
                Items    = new ItemsApi(_world, _packetService, cancelCtrl),
                Mobiles  = new MobilesApi(_world, cancelCtrl),
                Misc     = new MiscApi(_world, cancelCtrl, line => OutputReceived?.Invoke(line)),
                Journal  = new JournalApi(_journalService, cancelCtrl),
                Gumps    = new GumpsApi(_world, _packetService, cancelCtrl),
                Target   = new TargetApi(_targetingService, cancelCtrl),
                Skills   = new SkillsApi(_skillsService, _packetService, cancelCtrl),
                Spells   = new SpellsApi(_packetService, cancelCtrl),
                Statics  = new StaticsApi(cancelCtrl),
                Friend   = new FriendApi(cancelCtrl),
                Filters  = new FiltersApi(cancelCtrl)
            };

            var options = ScriptOptions.Default
                .WithReferences(
                    typeof(ScriptGlobals).Assembly,
                    typeof(ScriptLanguage).Assembly,
                    typeof(object).Assembly,
                    typeof(System.Linq.Enumerable).Assembly,
                    typeof(System.Collections.Generic.List<>).Assembly)
                .WithImports(
                    "System",
                    "System.Collections.Generic",
                    "System.Linq",
                    "System.Threading.Tasks",
                    "TMRazorImproved.Core.Services.Scripting.Api",
                    "TMRazorImproved.Shared.Models");

            try
            {
                // Esecuzione sincrona del task asincrono Roslyn all'interno del thread worker dello script
                CSharpScript.RunAsync(code, options, globals, typeof(ScriptGlobals), cts.Token).Wait();
            }
            catch (AggregateException aggEx)
            {
                // Roslyn wrappa le eccezioni in AggregateException
                if (aggEx.InnerException != null)
                    throw aggEx.InnerException;
                throw;
            }
            finally
            {
                _scriptThread = null;
            }
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private static string FormatException(Exception ex)
        {
            // Le eccezioni IronPython contengono spesso informazioni Python nella Message
            return $"[Errore Script] {ex.GetType().Name}: {ex.Message}";
        }

        public void Dispose()
        {
            _activeCts?.Cancel();
            _executionLock.Dispose();
        }
    }
}
