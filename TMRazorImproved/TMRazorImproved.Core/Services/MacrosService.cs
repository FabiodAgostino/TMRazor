using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TMRazorImproved.Core.Utilities;
using TMRazorImproved.Shared.Enums;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.Core.Services
{
    public class MacrosService : IMacrosService
    {
        private readonly string _macrosPath;
        private readonly IConfigService _config;
        private readonly IPacketService _packetService;
        private readonly IWorldService _worldService;
        private readonly ITargetingService _targetingService;
        private readonly ILogger<MacrosService> _logger;

        // FIX P0-04: cattura il SynchronizationContext del thread UI al momento della costruzione
        // (App.xaml.cs esegue il costruttore sul thread UI) per poter aggiornare MacroList
        // in modo thread-safe da thread background.
        private readonly SynchronizationContext? _uiContext;

        public ObservableCollection<string> MacroList { get; } = new();

        // FIX P1-03: volatile per visibilità cross-thread (packet thread ↔ UI thread).
        private volatile bool _isRecording;
        private volatile bool _isPlaying;

        public bool IsRecording => _isRecording;
        public bool IsPlaying => _isPlaying;
        public string? ActiveMacro { get; private set; }

        private CancellationTokenSource? _playCts;

        // FIX BUG-P1-05: buffer di registrazione macro
        private readonly List<string> _recordingBuffer = new();
        private readonly List<Action> _recordingUnsubscribers = new();

        public MacrosService(
            IConfigService config,
            IPacketService packetService,
            IWorldService worldService,
            ITargetingService targetingService,
            ILogger<MacrosService> logger)
        {
            _config = config;
            _packetService = packetService;
            _worldService = worldService;
            _targetingService = targetingService;
            _logger = logger;
            _macrosPath = Path.Combine(AppContext.BaseDirectory, "Macros");
            // FIX P0-04: cattura il contesto UI. Se siamo già sul thread UI (App startup), sarà
            // il WPF SynchronizationContext; se null (unit test), le add vengono eseguite in-thread.
            _uiContext = SynchronizationContext.Current;

            if (!Directory.Exists(_macrosPath))
                Directory.CreateDirectory(_macrosPath);
        }

        public void LoadMacros()
        {
            MacroList.Clear();
            if (Directory.Exists(_macrosPath))
            {
                foreach (var file in Directory.GetFiles(_macrosPath, "*.macro"))
                    MacroList.Add(Path.GetFileNameWithoutExtension(file));
            }
        }

        public void Play(string name)
        {
            if (_isPlaying || _isRecording) return;
            _isPlaying = true;
            ActiveMacro = name;

            var steps = GetSteps(name);
            _playCts = new CancellationTokenSource();

            _ = Task.Run(async () =>
            {
                try
                {
                    await ExecuteWithControlFlowAsync(steps, _playCts.Token);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Macro {Name} cancelled.", name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error playing macro {Name}", name);
                }
                finally
                {
                    _isPlaying = false;
                    ActiveMacro = null;
                }
            });
        }

        // -------------------------------------------------------------------------
        // Sprint Fix-3: Control-flow execution engine
        // Supporta: IF/ELSEIF/ELSE/ENDIF  WHILE/ENDWHILE  FOR/ENDFOR
        // -------------------------------------------------------------------------

        /// <summary>
        /// Esegue la lista di step con supporto a control flow.
        /// Pass 1: BuildJumpTables costruisce le tabelle di salto in O(n).
        /// Pass 2: esecuzione a program-counter; lo stack ifBranchTaken traccia i blocchi IF.
        /// </summary>
        private async Task ExecuteWithControlFlowAsync(List<MacroStep> steps, CancellationToken token)
        {
            var (condFalse, skipToEndif, loopBack) = BuildJumpTables(steps);
            var forCounters    = new Dictionary<int, int>(); // FOR idx → iterazioni rimanenti
            var ifBranchTaken  = new Stack<bool>();           // true = un ramo di questo IF è già stato eseguito

            int pc = 0;
            while (pc < steps.Count && !token.IsCancellationRequested)
            {
                var step = steps[pc];
                if (!step.IsEnabled) { pc++; continue; }

                var cmd = step.Command.Trim();
                if (string.IsNullOrEmpty(cmd) || cmd.StartsWith("//") || cmd.StartsWith("#")) { pc++; continue; }

                var parts = cmd.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                string action = parts[0].ToUpperInvariant();
                string args   = parts.Length > 1 ? parts[1].Trim() : "";

                switch (action)
                {
                    // ---- IF ----
                    case "IF":
                    {
                        bool cond = EvaluateCondition(args);
                        ifBranchTaken.Push(cond);
                        if (!cond && condFalse.TryGetValue(pc, out int falseDest))
                            pc = falseDest;
                        else
                            pc++;
                        break;
                    }

                    // ---- ELSEIF ----
                    case "ELSEIF":
                    {
                        bool alreadyExecuted = ifBranchTaken.Count > 0 && ifBranchTaken.Peek();
                        if (alreadyExecuted)
                        {
                            // Ramo precedente vero → salta a ENDIF
                            if (skipToEndif.TryGetValue(pc, out int endifDest)) pc = endifDest;
                            else pc++;
                        }
                        else
                        {
                            bool cond = EvaluateCondition(args);
                            if (cond)
                            {
                                if (ifBranchTaken.Count > 0) { ifBranchTaken.Pop(); ifBranchTaken.Push(true); }
                                pc++;
                            }
                            else if (condFalse.TryGetValue(pc, out int falseDest))
                                pc = falseDest;
                            else
                                pc++;
                        }
                        break;
                    }

                    // ---- ELSE ----
                    case "ELSE":
                    {
                        bool alreadyExecuted = ifBranchTaken.Count > 0 && ifBranchTaken.Peek();
                        if (alreadyExecuted)
                        {
                            if (skipToEndif.TryGetValue(pc, out int endifDest)) pc = endifDest;
                            else pc++;
                        }
                        else
                        {
                            if (ifBranchTaken.Count > 0) { ifBranchTaken.Pop(); ifBranchTaken.Push(true); }
                            pc++;
                        }
                        break;
                    }

                    // ---- ENDIF ----
                    case "ENDIF":
                        if (ifBranchTaken.Count > 0) ifBranchTaken.Pop();
                        pc++;
                        break;

                    // ---- WHILE ----
                    case "WHILE":
                    {
                        bool cond = EvaluateCondition(args);
                        if (!cond && condFalse.TryGetValue(pc, out int falseDest))
                            pc = falseDest;
                        else
                            pc++;
                        break;
                    }

                    // ---- ENDWHILE ----
                    case "ENDWHILE":
                        pc = loopBack.TryGetValue(pc, out int whileDest) ? whileDest : pc + 1;
                        break;

                    // ---- FOR n ----
                    case "FOR":
                    {
                        if (!forCounters.ContainsKey(pc))
                        {
                            // Prima visita: inizializza contatore
                            if (int.TryParse(args, out int n) && n > 0) { forCounters[pc] = n; pc++; }
                            else if (condFalse.TryGetValue(pc, out int skipDest)) pc = skipDest;
                            else pc++;
                        }
                        else
                        {
                            // Revisita da ENDFOR: decrementa
                            forCounters[pc]--;
                            if (forCounters[pc] <= 0)
                            {
                                forCounters.Remove(pc);
                                pc = condFalse.TryGetValue(pc, out int doneDest) ? doneDest : pc + 1;
                            }
                            else
                                pc++;
                        }
                        break;
                    }

                    // ---- ENDFOR ----
                    case "ENDFOR":
                        pc = loopBack.TryGetValue(pc, out int forDest) ? forDest : pc + 1;
                        break;

                    // ---- Azioni normali ----
                    default:
                        await ExecuteActionAsync(action, args, token);
                        pc++;
                        break;
                }
            }
        }

        /// <summary>
        /// Costruisce tre tabelle in un unico scan O(n):
        ///   condFalse[i]   = dove saltare quando la condizione a i è falsa (IF/ELSEIF/WHILE/FOR)
        ///   skipToEndif[i] = ENDIF corrispondente (per IF/ELSEIF/ELSE: usato dopo ramo vero)
        ///   loopBack[i]    = FOR/WHILE corrispondente (per ENDFOR/ENDWHILE)
        /// </summary>
        private static (Dictionary<int, int> condFalse, Dictionary<int, int> skipToEndif, Dictionary<int, int> loopBack)
            BuildJumpTables(List<MacroStep> steps)
        {
            var condFalse   = new Dictionary<int, int>();
            var skipToEndif = new Dictionary<int, int>();
            var loopBack    = new Dictionary<int, int>();

            var ifBlockStack = new Stack<List<int>>(); // lista IF/ELSEIF/ELSE per il blocco corrente
            var whileStack   = new Stack<int>();
            var forStack     = new Stack<int>();

            for (int i = 0; i < steps.Count; i++)
            {
                var raw = steps[i].Command.Trim();
                if (string.IsNullOrEmpty(raw)) continue;

                var parts = raw.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                string c = parts[0].ToUpperInvariant();

                switch (c)
                {
                    case "IF":
                        ifBlockStack.Push(new List<int> { i });
                        break;

                    case "ELSEIF":
                        if (ifBlockStack.Count > 0)
                        {
                            var block = ifBlockStack.Peek();
                            condFalse[block[^1]] = i; // precedente IF/ELSEIF → questo ELSEIF
                            block.Add(i);
                        }
                        break;

                    case "ELSE":
                        if (ifBlockStack.Count > 0)
                        {
                            var block = ifBlockStack.Peek();
                            condFalse[block[^1]] = i;
                            block.Add(i);
                        }
                        break;

                    case "ENDIF":
                        if (ifBlockStack.Count > 0)
                        {
                            var block = ifBlockStack.Pop();
                            foreach (var idx in block)
                            {
                                if (!condFalse.ContainsKey(idx)) condFalse[idx] = i; // nessun ELSE
                                skipToEndif[idx] = i;
                            }
                        }
                        break;

                    case "WHILE":
                        whileStack.Push(i);
                        break;

                    case "ENDWHILE":
                        if (whileStack.Count > 0)
                        {
                            int w = whileStack.Pop();
                            condFalse[w] = i + 1;
                            loopBack[i]  = w;
                        }
                        break;

                    case "FOR":
                        forStack.Push(i);
                        break;

                    case "ENDFOR":
                        if (forStack.Count > 0)
                        {
                            int f = forStack.Pop();
                            condFalse[f] = i + 1;
                            loopBack[i]  = f;
                        }
                        break;
                }
            }

            return (condFalse, skipToEndif, loopBack);
        }

        /// <summary>
        /// Valuta una condizione testuale rispetto allo stato corrente del player.
        /// Supporta: POISONED, HIDDEN, WARMODE, DEAD (+ NOT prefisso),
        ///           HP/MANA/STAM/STR/DEX/INT/WEIGHT con operatori &lt; &gt; &lt;= &gt;= == !=
        /// </summary>
        private bool EvaluateCondition(string condition)
        {
            if (string.IsNullOrWhiteSpace(condition)) return true;

            var cond   = condition.Trim();
            bool negate = false;
            if (cond.StartsWith("NOT ", StringComparison.OrdinalIgnoreCase))
            {
                negate = true;
                cond = cond.Substring(4).Trim();
            }

            var player = _worldService.Player;
            bool result;

            switch (cond.ToUpperInvariant())
            {
                case "POISONED": result = player?.IsPoisoned ?? false; break;
                case "HIDDEN":   result = player?.IsHidden   ?? false; break;
                case "WARMODE":  result = player?.WarMode    ?? false; break;
                case "DEAD":     result = player?.Hits == 0;           break;
                default:
                {
                    // Formato: PROPERTY OPERATOR VALUE  (es. "HP < 80")
                    var tokens = cond.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
                    if (tokens.Length < 3 || player == null) { result = false; break; }

                    if (!int.TryParse(tokens[2], out int val)) { result = false; break; }

                    int pv = tokens[0].ToUpperInvariant() switch
                    {
                        "HP" or "HITS"       => player.Hits,
                        "MAXHP" or "MAXHITS" => (int)player.HitsMax,
                        "MANA"               => player.Mana,
                        "MAXMANA"            => (int)player.ManaMax,
                        "STAM" or "STAMINA"  => player.Stam,
                        "MAXSTAM"            => (int)player.StamMax,
                        "STR"                => player.Str,
                        "DEX"                => player.Dex,
                        "INT"                => player.Int,
                        "WEIGHT"             => player.Weight,
                        _                    => -1
                    };

                    if (pv == -1) { result = false; break; }

                    result = tokens[1] switch
                    {
                        "<"         => pv < val,
                        ">"         => pv > val,
                        "<="        => pv <= val,
                        ">="        => pv >= val,
                        "==" or "=" => pv == val,
                        "!="        => pv != val,
                        _           => false
                    };
                    break;
                }
            }

            return negate ? !result : result;
        }

        private async Task ExecuteActionAsync(string action, string args, CancellationToken token)
        {
            switch (action)
            {
                case "PAUSE":
                case "WAIT":
                    await Task.Delay(int.TryParse(args, out int ms) ? ms : 500, token);
                    break;

                case "SAY":
                case "MSG":
                    _packetService.SendToServer(PacketBuilder.UnicodeSpeech(args));
                    await Task.Delay(100, token);
                    break;

                case "DOUBLECLICK":
                case "DCLICK":
                    if (uint.TryParse(args, out uint dcSerial))
                        _packetService.SendToServer(PacketBuilder.DoubleClick(dcSerial));
                    break;

                case "SINGLECLICK":
                    if (uint.TryParse(args, out uint scSerial))
                        _packetService.SendToServer(PacketBuilder.SingleClick(scSerial));
                    break;

                case "TARGET":
                    if (uint.TryParse(args, out uint tSerial))
                    {
                        uint cursorId = _targetingService.PendingCursorId;
                        _targetingService.ClearTargetCursor();
                        _packetService.SendToServer(PacketBuilder.TargetObject(tSerial, cursorId));
                    }
                    break;

                case "CAST":
                    if (int.TryParse(args, out int spellId))
                        _packetService.SendToServer(PacketBuilder.CastSpell(spellId));
                    break;

                case "USESKILL":
                    if (int.TryParse(args, out int skillId))
                        _packetService.SendToServer(PacketBuilder.UseSkill(skillId));
                    break;

                case "ATTACK":
                    if (uint.TryParse(args, out uint atkSerial))
                        _packetService.SendToServer(PacketBuilder.Attack(atkSerial));
                    break;

                // FIX BUG-P2-03: attende il vero S2C 0x6C dal server
                case "WAITFORTARGET":
                {
                    int timeout = int.TryParse(args, out int wftMs) ? wftMs : 5000;
                    var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                    Action<byte[]> handler = _ => tcs.TrySetResult(true);
                    _packetService.RegisterViewer(PacketPath.ServerToClient, 0x6C, handler);
                    try
                    {
                        using var linked = CancellationTokenSource.CreateLinkedTokenSource(token);
                        linked.CancelAfter(timeout);
                        await tcs.Task.WaitAsync(linked.Token);
                    }
                    catch (OperationCanceledException) { }
                    finally { _packetService.UnregisterViewer(PacketPath.ServerToClient, 0x6C, handler); }
                    break;
                }

                // Sprint Fix-4: USETYPE <graphic> — trova il primo item con quel graphic nel backpack e lo usa
                case "USETYPE":
                {
                    if (!int.TryParse(args, out int useTypeGraphic)) break;
                    var bp = _worldService.Player?.Backpack;
                    TMRazorImproved.Shared.Models.Item? found = null;
                    if (bp != null)
                        found = _worldService.GetItemsInContainer(bp.Serial)
                            .FirstOrDefault(i => i.Graphic == useTypeGraphic);
                    if (found == null)
                        found = _worldService.Items.FirstOrDefault(i => i.Graphic == useTypeGraphic);
                    if (found != null)
                    {
                        _logger.LogDebug("USETYPE graphic=0x{G:X} → serial=0x{S:X}", useTypeGraphic, found.Serial);
                        _packetService.SendToServer(PacketBuilder.DoubleClick(found.Serial));
                    }
                    else
                        _logger.LogWarning("USETYPE: no item found with graphic 0x{G:X}", useTypeGraphic);
                    break;
                }

                // Sprint Fix-4: EQUIPITEM <serial> <layer> — lift + wear
                case "EQUIPITEM":
                {
                    var ep = args.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                    if (ep.Length == 2
                        && uint.TryParse(ep[0], out uint equipSerial)
                        && byte.TryParse(ep[1], out byte equipLayer))
                    {
                        uint playerSerial = _worldService.Player?.Serial ?? 0;
                        _logger.LogDebug("EQUIPITEM serial=0x{S:X} layer={L} player=0x{P:X}", equipSerial, equipLayer, playerSerial);
                        _packetService.SendToServer(PacketBuilder.LiftItem(equipSerial));
                        await Task.Delay(50, token);
                        _packetService.SendToServer(PacketBuilder.WearItem(equipSerial, equipLayer, playerSerial));
                    }
                    else
                        _logger.LogWarning("EQUIPITEM: formato non valido. Atteso: EQUIPITEM <serial> <layer>");
                    break;
                }

                // Sprint Fix-4: MOUNT <serial?> — double-click sul mount
                case "MOUNT":
                {
                    if (uint.TryParse(args, out uint mountSerial) && mountSerial != 0)
                    {
                        _logger.LogDebug("MOUNT serial=0x{S:X}", mountSerial);
                        _packetService.SendToServer(PacketBuilder.DoubleClick(mountSerial));
                    }
                    else
                    {
                        var player = _worldService.Player;
                        if (player != null)
                        {
                            var nearestMobile = _worldService.Mobiles
                                .Where(m => m.Serial != player.Serial && m.DistanceTo(player) <= 2)
                                .OrderBy(m => m.DistanceTo(player))
                                .FirstOrDefault();
                            if (nearestMobile != null)
                            {
                                _logger.LogDebug("MOUNT nearest mobile=0x{S:X}", nearestMobile.Serial);
                                _packetService.SendToServer(PacketBuilder.DoubleClick(nearestMobile.Serial));
                            }
                        }
                    }
                    break;
                }

                // Sprint Fix-4: DISMOUNT — double-click sull'item nel riding layer (layer 25)
                case "DISMOUNT":
                {
                    var player = _worldService.Player;
                    if (player != null)
                    {
                        const byte ridingLayer = 0x19; // layer 25
                        var mountItem = _worldService.Items
                            .FirstOrDefault(i => i.Container == player.Serial && i.Layer == ridingLayer);
                        if (mountItem != null)
                        {
                            _logger.LogDebug("DISMOUNT via riding layer item 0x{S:X}", mountItem.Serial);
                            _packetService.SendToServer(PacketBuilder.DoubleClick(mountItem.Serial));
                        }
                        else
                        {
                            _logger.LogDebug("DISMOUNT: no riding layer item, double-clicking player serial");
                            _packetService.SendToServer(PacketBuilder.DoubleClick(player.Serial));
                        }
                    }
                    break;
                }

                // Sprint Fix-3: RESPONDGUMP <serial> <typeId> <buttonId>
                case "RESPONDGUMP":
                {
                    var t = args.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
                    if (t.Length == 3
                        && uint.TryParse(t[0], out uint gs)
                        && uint.TryParse(t[1], out uint gt)
                        && int.TryParse(t[2], out int gb))
                    {
                        _packetService.SendToServer(PacketBuilder.RespondGump(gs, gt, gb));
                        _logger.LogDebug("RespondGump serial=0x{S:X} typeId=0x{T:X} button={B}", gs, gt, gb);
                    }
                    else
                        _logger.LogWarning("RESPONDGUMP: formato non valido. Atteso: RESPONDGUMP <serial> <typeId> <buttonId>");
                    break;
                }

                default:
                    _logger.LogWarning("Unknown macro action: {Action}", action);
                    break;
            }
        }

        // -------------------------------------------------------------------------
        // Recording
        // -------------------------------------------------------------------------

        public void Record(string name)
        {
            if (_isPlaying || _isRecording) return;
            _isRecording = true;
            ActiveMacro = name;
            _recordingBuffer.Clear();
            _recordingUnsubscribers.Clear();

            // FIX BUG-P1-05: viewer C2S per catturare le azioni del giocatore
            Action<byte[]> onDoubleClick = data =>
            {
                if (data.Length >= 5)
                {
                    uint serial = System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(1));
                    lock (_recordingBuffer) _recordingBuffer.Add($"DOUBLECLICK {serial}");
                }
            };
            Action<byte[]> onSingleClick = data =>
            {
                if (data.Length >= 5)
                {
                    uint serial = System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(1));
                    lock (_recordingBuffer) _recordingBuffer.Add($"SINGLECLICK {serial}");
                }
            };
            Action<byte[]> onSpeech = data =>
            {
                if (data.Length > 13)
                {
                    try
                    {
                        string text = System.Text.Encoding.BigEndianUnicode.GetString(data, 12, data.Length - 14).TrimEnd('\0');
                        if (!string.IsNullOrWhiteSpace(text))
                            lock (_recordingBuffer) _recordingBuffer.Add($"SAY {text}");
                    }
                    catch { }
                }
            };
            Action<byte[]> onTarget = data =>
            {
                if (data.Length >= 11 && data[6] == 0x00)
                {
                    uint serial = System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(7));
                    lock (_recordingBuffer) _recordingBuffer.Add($"TARGET {serial}");
                }
            };
            Action<byte[]> onTextCmd = data =>
            {
                if (data.Length > 5)
                {
                    byte cmdType = data[3];
                    try
                    {
                        string text = System.Text.Encoding.ASCII.GetString(data, 4, data.Length - 5).TrimEnd('\0');
                        if (cmdType == 0x56 || cmdType == 0x27)
                        {
                            if (int.TryParse(text, out int sid))
                                lock (_recordingBuffer) _recordingBuffer.Add($"CAST {sid}");
                        }
                        else if (cmdType == 0x24)
                        {
                            var p = text.Split(' ');
                            if (p.Length > 0 && int.TryParse(p[0], out int skid))
                                lock (_recordingBuffer) _recordingBuffer.Add($"USESKILL {skid}");
                        }
                    }
                    catch { }
                }
            };
            Action<byte[]> onAttack = data =>
            {
                if (data.Length >= 5)
                {
                    uint serial = System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(1));
                    lock (_recordingBuffer) _recordingBuffer.Add($"ATTACK {serial}");
                }
            };

            _packetService.RegisterViewer(PacketPath.ClientToServer, 0x06, onDoubleClick);
            _packetService.RegisterViewer(PacketPath.ClientToServer, 0x09, onSingleClick);
            _packetService.RegisterViewer(PacketPath.ClientToServer, 0xAD, onSpeech);
            _packetService.RegisterViewer(PacketPath.ClientToServer, 0x6C, onTarget);
            _packetService.RegisterViewer(PacketPath.ClientToServer, 0x12, onTextCmd);
            _packetService.RegisterViewer(PacketPath.ClientToServer, 0x05, onAttack);

            _recordingUnsubscribers.Add(() => _packetService.UnregisterViewer(PacketPath.ClientToServer, 0x06, onDoubleClick));
            _recordingUnsubscribers.Add(() => _packetService.UnregisterViewer(PacketPath.ClientToServer, 0x09, onSingleClick));
            _recordingUnsubscribers.Add(() => _packetService.UnregisterViewer(PacketPath.ClientToServer, 0xAD, onSpeech));
            _recordingUnsubscribers.Add(() => _packetService.UnregisterViewer(PacketPath.ClientToServer, 0x6C, onTarget));
            _recordingUnsubscribers.Add(() => _packetService.UnregisterViewer(PacketPath.ClientToServer, 0x12, onTextCmd));
            _recordingUnsubscribers.Add(() => _packetService.UnregisterViewer(PacketPath.ClientToServer, 0x05, onAttack));

            _logger.LogInformation("Recording macro '{Name}' started", name);
        }

        // -------------------------------------------------------------------------
        // Stop / Save / Load / Delete / Rename
        // -------------------------------------------------------------------------

        public void Stop()
        {
            if (_isPlaying && _playCts != null)
                _playCts.Cancel();

            if (_isRecording)
            {
                foreach (var unsub in _recordingUnsubscribers) unsub();
                _recordingUnsubscribers.Clear();

                List<string> captured;
                lock (_recordingBuffer)
                {
                    captured = new List<string>(_recordingBuffer);
                    _recordingBuffer.Clear();
                }
                if (!string.IsNullOrEmpty(ActiveMacro) && captured.Count > 0)
                {
                    var steps = captured.Select(cmd => new MacroStep(cmd, cmd)).ToList();
                    Save(ActiveMacro, steps);
                    _logger.LogInformation("Macro '{Name}' saved with {Count} steps", ActiveMacro, steps.Count);
                }
            }

            _isPlaying = false;
            _isRecording = false;
            ActiveMacro = null;
        }

        public void Save(string name, List<MacroStep> steps)
        {
            var path = Path.Combine(_macrosPath, $"{name}.macro");
            File.WriteAllLines(path, steps.Select(s => s.Command));
            // FIX P0-04: MacroList è ObservableCollection — va modificata sul thread UI.
            // Se _uiContext è null (test), aggiunge direttamente (nessun WPF).
            if (!MacroList.Contains(name))
            {
                if (_uiContext != null)
                    _uiContext.Post(_ => MacroList.Add(name), null);
                else
                    MacroList.Add(name);
            }
        }

        public List<MacroStep> GetSteps(string name)
        {
            var steps = new List<MacroStep>();
            var path = Path.Combine(_macrosPath, $"{name}.macro");

            if (File.Exists(path))
            {
                foreach (var line in File.ReadAllLines(path))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    steps.Add(new MacroStep(line, line));
                }
            }

            return steps;
        }

        public void Delete(string name)
        {
            var path = Path.Combine(_macrosPath, $"{name}.macro");
            if (File.Exists(path))
            {
                File.Delete(path);
                // FIX P0-04: thread-safe remove
                void DoRemove() => MacroList.Remove(name);
                if (_uiContext != null) _uiContext.Post(_ => DoRemove(), null);
                else DoRemove();
            }
        }

        public void Rename(string oldName, string newName)
        {
            var oldPath = Path.Combine(_macrosPath, $"{oldName}.macro");
            var newPath = Path.Combine(_macrosPath, $"{newName}.macro");
            if (File.Exists(oldPath))
            {
                File.Move(oldPath, newPath);
                // FIX P0-04: thread-safe rename in collection
                void DoRename()
                {
                    var index = MacroList.IndexOf(oldName);
                    if (index != -1) MacroList[index] = newName;
                }
                if (_uiContext != null) _uiContext.Post(_ => DoRename(), null);
                else DoRename();
            }
        }
    }
}
