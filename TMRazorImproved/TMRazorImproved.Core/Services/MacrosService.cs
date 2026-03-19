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
        private readonly IJournalService? _journalService;
        private readonly IOrganizerService? _organizerService;
        private readonly ILogger<MacrosService> _logger;
        private readonly ConditionEvaluator _conditionEvaluator;

        // Alias locali definiti via SETALIAS/REMOVEALIAS
        private readonly Dictionary<string, uint> _aliases = new(StringComparer.OrdinalIgnoreCase);

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
            ILogger<MacrosService> logger,
            IJournalService? journalService = null,
            IOrganizerService? organizerService = null,
            ISkillsService? skillsService = null)
        {
            _config = config;
            _packetService = packetService;
            _worldService = worldService;
            _targetingService = targetingService;
            _logger = logger;
            _journalService = journalService;
            _organizerService = organizerService;
            _conditionEvaluator = new ConditionEvaluator(worldService, skillsService, journalService, targetingService);
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
        /// Valuta una condizione testuale delegando al ConditionEvaluator.
        /// Supporta 9 categorie: PlayerStats, PlayerStatus, Skill, Find, Count,
        /// InRange, TargetExists, InJournal, BuffExists (+ prefisso NOT).
        /// </summary>
        private bool EvaluateCondition(string condition)
            => _conditionEvaluator.Evaluate(condition);

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

                case "SETABILITY":
                {
                    int ability = args?.ToLowerInvariant() switch
                    {
                        "primary" => 1,
                        "secondary" => 2,
                        "clear" => 0,
                        _ => int.TryParse(args, out int n) ? n : 0
                    };
                    uint serial = _worldService.Player?.Serial ?? 0;
                    if (serial != 0)
                    {
                        byte[] pkt = new byte[9];
                        pkt[0] = 0xD7;
                        pkt[1] = 0x00; pkt[2] = 0x09;
                        System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(3), serial);
                        pkt[7] = 0x00; pkt[8] = (byte)ability;
                        _packetService.SendToServer(pkt);
                    }
                    break;
                }

                case "ATTACK":
                {
                    uint atkSerial = ResolveAttackTarget(args);
                    if (atkSerial != 0)
                        _packetService.SendToServer(PacketBuilder.Attack(atkSerial));
                    break;
                }

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

                // ── TASK-04.1 — WARMODE ────────────────────────────────────────────────────────
                case "WARMODE":
                {
                    bool enable;
                    if (args.Equals("toggle", StringComparison.OrdinalIgnoreCase))
                        enable = !(_worldService.Player?.WarMode ?? false);
                    else
                        enable = args.Equals("on", StringComparison.OrdinalIgnoreCase);
                    _packetService.SendToServer(new byte[] { 0x72, (byte)(enable ? 0x01 : 0x00), 0x00, 0x32, 0x00 });
                    break;
                }

                // ── TASK-04.2 — FLY / LAND ─────────────────────────────────────────────────────
                case "FLY":
                case "LAND":
                    // 0xBF sub 0x32: toggle volo
                    _packetService.SendToServer(new byte[] { 0xBF, 0x00, 0x05, 0x00, 0x32 });
                    break;

                // ── TASK-04.3 — RESYNC ─────────────────────────────────────────────────────────
                case "RESYNC":
                    _packetService.SendToServer(new byte[] { 0x22, 0xFF, 0x00 });
                    break;

                // ── TASK-04.4 — CLEARJOURNAL ───────────────────────────────────────────────────
                case "CLEARJOURNAL":
                    _journalService?.Clear();
                    break;

                // ── TASK-04.5 — MOVEITEM / PICKUP / DROP ───────────────────────────────────────
                case "MOVEITEM":
                {
                    var parts = args.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 2) break;
                    uint mvSerial = ResolveSerial(parts[0]);
                    uint mvDest   = ResolveSerial(parts[1]);
                    ushort mvAmt  = parts.Length > 2 && ushort.TryParse(parts[2], out ushort pa) ? pa : (ushort)1;
                    _packetService.SendToServer(PacketBuilder.LiftItem(mvSerial, mvAmt));
                    await Task.Delay(100, token);
                    _packetService.SendToServer(PacketBuilder.DropToContainer(mvSerial, mvDest));
                    break;
                }

                case "PICKUP":
                {
                    var parts = args.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 1) break;
                    uint puSerial  = ResolveSerial(parts[0]);
                    ushort puAmt   = parts.Length > 1 && ushort.TryParse(parts[1], out ushort pa) ? pa : (ushort)1;
                    uint backpack  = _worldService.Player?.Backpack?.Serial ?? 0;
                    if (backpack == 0) break;
                    _packetService.SendToServer(PacketBuilder.LiftItem(puSerial, puAmt));
                    await Task.Delay(100, token);
                    _packetService.SendToServer(PacketBuilder.DropToContainer(puSerial, backpack));
                    break;
                }

                case "DROP":
                {
                    var parts = args.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 4) break;
                    uint drSerial = ResolveSerial(parts[0]);
                    if (!ushort.TryParse(parts[1], out ushort drX) ||
                        !ushort.TryParse(parts[2], out ushort drY) ||
                        !short.TryParse(parts[3], out short drZ)) break;
                    _packetService.SendToServer(PacketBuilder.LiftItem(drSerial));
                    await Task.Delay(100, token);
                    _packetService.SendToServer(PacketBuilder.DropToWorld(drSerial, drX, drY, drZ));
                    break;
                }

                // ── TASK-04.6 — EMOTE / INVOKEVIRTUE / RENAMEMOBILE ───────────────────────────
                case "EMOTE":
                    _packetService.SendToServer(PacketBuilder.UnicodeSpeech(args, 0x03, 0x024));
                    break;

                case "INVOKEVIRTUE":
                {
                    byte virtueId = args.Trim().ToLowerInvariant() switch
                    {
                        "honor"        => 1, "sacrifice"    => 2, "valor"       => 3,
                        "compassion"   => 4, "honesty"      => 5, "humility"    => 6,
                        "justice"      => 7, "spirituality" => 8, _ => 0
                    };
                    if (virtueId != 0)
                        _packetService.SendToServer(new byte[] { 0xB2, virtueId });
                    break;
                }

                case "RENAMEMOBILE":
                {
                    int idx = args.IndexOf(' ');
                    if (idx < 0) break;
                    uint rnSerial = ResolveSerial(args[..idx].Trim());
                    string rnName = args[(idx + 1)..].Trim();
                    // Pacchetto 0x75: cmd(1) serial(4) name(30, null-terminated)
                    var pkt = new byte[35];
                    pkt[0] = 0x75;
                    System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(1), rnSerial);
                    var nameBytes = System.Text.Encoding.ASCII.GetBytes(rnName);
                    Array.Copy(nameBytes, 0, pkt, 5, Math.Min(nameBytes.Length, 29));
                    _packetService.SendToServer(pkt);
                    break;
                }

                // ── TASK-04.7 — USECONTEXTMENU / WAITFORGUMP ──────────────────────────────────
                case "USECONTEXTMENU":
                {
                    var parts = args.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 2) break;
                    uint cmSerial = ResolveSerial(parts[0]);
                    if (!ushort.TryParse(parts[1], out ushort cmEntry)) break;
                    // Request: 0xBF sub 0x13 — richiede il menu contestuale
                    var req = new byte[9];
                    req[0] = 0xBF;
                    System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(req.AsSpan(1), 9);
                    System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(req.AsSpan(3), 0x13);
                    System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(req.AsSpan(5), cmSerial);
                    _packetService.SendToServer(req);
                    await Task.Delay(150, token);
                    // Response: 0xBF sub 0x15 — sceglie la voce di menu
                    var resp = new byte[11];
                    resp[0] = 0xBF;
                    System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(resp.AsSpan(1), 11);
                    System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(resp.AsSpan(3), 0x15);
                    System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(resp.AsSpan(5), cmSerial);
                    System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(resp.AsSpan(9), cmEntry);
                    _packetService.SendToServer(resp);
                    break;
                }

                case "WAITFORGUMP":
                {
                    var parts = args.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    uint wfgTypeId = parts.Length > 0 && uint.TryParse(parts[0], System.Globalization.NumberStyles.HexNumber, null, out uint tid) ? tid : 0;
                    int wfgTimeout = parts.Length > 1 && int.TryParse(parts[1], out int wt) ? wt : 5000;
                    var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                    Action<byte[]> gumpHandler = data =>
                    {
                        // 0xB0: cmd(1) len(2) serial(4) typeId(4) ...
                        if (data.Length > 12)
                        {
                            uint t = System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(7));
                            if (wfgTypeId == 0 || t == wfgTypeId) tcs.TrySetResult(true);
                        }
                    };
                    _packetService.RegisterViewer(PacketPath.ServerToClient, 0xB0, gumpHandler);
                    try
                    {
                        using var linked = CancellationTokenSource.CreateLinkedTokenSource(token);
                        linked.CancelAfter(wfgTimeout);
                        await tcs.Task.WaitAsync(linked.Token);
                    }
                    catch (OperationCanceledException) { }
                    finally { _packetService.UnregisterViewer(PacketPath.ServerToClient, 0xB0, gumpHandler); }
                    break;
                }

                // ── TASK-04.8 — ARMDISARM / BANDAGE / TARGETRESOURCE / USEPOTIONTYPE ──────────
                case "ARMDISARM":
                {
                    var player = _worldService.Player;
                    if (player == null) break;
                    const byte mainHand = (byte)TMRazorImproved.Shared.Enums.Layer.RightHand;
                    const byte twoHanded = (byte)TMRazorImproved.Shared.Enums.Layer.LeftHand;
                    var weapon = _worldService.Items.FirstOrDefault(i => i.Container == player.Serial && (i.Layer == mainHand || i.Layer == twoHanded));
                    var backpack = player.Backpack;
                    if (weapon != null && backpack != null)
                    {
                        _packetService.SendToServer(PacketBuilder.LiftItem(weapon.Serial, weapon.Amount));
                        await Task.Delay(100, token);
                        _packetService.SendToServer(PacketBuilder.DropToContainer(weapon.Serial, backpack.Serial));
                    }
                    break;
                }

                case "BANDAGE":
                {
                    var bpContainer = _worldService.Player?.Backpack;
                    if (bpContainer == null) break;
                    var bandage = _worldService.Items.FirstOrDefault(i => i.Container == bpContainer.Serial && i.Graphic == 0x0E21);
                    if (bandage == null) break;
                    uint bTarget = string.IsNullOrEmpty(args)
                        ? _worldService.Player?.Serial ?? 0
                        : ResolveSerial(args.Trim());
                    _packetService.SendToServer(PacketBuilder.DoubleClick(bandage.Serial));
                    await Task.Delay(100, token);
                    _targetingService.SendTarget(bTarget);
                    break;
                }

                case "TARGETRESOURCE":
                {
                    var parts = args.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 2) break;
                    uint trSerial = ResolveSerial(parts[0]);
                    int resourceType = parts[1].StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                        ? Convert.ToInt32(parts[1], 16)
                        : int.TryParse(parts[1], out int ri) ? ri : 0;
                    _packetService.SendToServer(PacketBuilder.TargetByResource(trSerial, resourceType));
                    break;
                }

                case "USEPOTIONTYPE":
                {
                    var potionGraphics = new Dictionary<string, ushort>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["heal"]      = 0x0F0C, ["cure"]      = 0x0F07, ["refresh"]   = 0x0F0B,
                        ["agility"]   = 0x0F08, ["strength"]  = 0x0F09, ["explosion"] = 0x0F0D,
                        ["poison"]    = 0x0F0A, ["nightsight"]= 0x0F06
                    };
                    if (!potionGraphics.TryGetValue(args.Trim(), out ushort potGraphic)) break;
                    var bp = _worldService.Player?.Backpack;
                    if (bp == null) break;
                    var potion = _worldService.Items.FirstOrDefault(i => i.Container == bp.Serial && i.Graphic == potGraphic);
                    if (potion != null)
                        _packetService.SendToServer(PacketBuilder.DoubleClick(potion.Serial));
                    break;
                }

                // ── TASK-04.9 — SETALIAS / REMOVEALIAS / PROMPTRESPONSE / RUNORGANIZER ────────
                case "SETALIAS":
                {
                    int idx = args.IndexOf(' ');
                    if (idx < 0) break;
                    string aliasName = args[..idx].Trim();
                    uint aliasSerial = ResolveSerial(args[(idx + 1)..].Trim());
                    _aliases[aliasName] = aliasSerial;
                    break;
                }

                case "REMOVEALIAS":
                    _aliases.Remove(args.Trim());
                    break;

                case "PROMPTRESPONSE":
                    _targetingService.SendPrompt(args);
                    break;

                case "RUNORGANIZER":
                    _organizerService?.Start();
                    break;

                // ── TASK-010 — Menu classici UO (0x7C/0x7D) ──────────────────────────────────
                case "WAITFORMENU":
                {
                    int wmTimeout = int.TryParse(args.Trim(), out int wmt) ? wmt : 5000;
                    await Task.Run(() => MenuStore.WaitForMenu(wmTimeout), token);
                    break;
                }

                case "MENURESPONSE":
                {
                    var menu = MenuStore.Get();
                    if (menu == null) break;
                    string menuSearch = args.Trim().Trim('"', '\'');
                    var menuItem = menu.Items.FirstOrDefault(i =>
                        i.Name.Contains(menuSearch, StringComparison.OrdinalIgnoreCase));
                    if (menuItem != null)
                    {
                        _packetService.SendToServer(PacketBuilder.MenuResponse(
                            menu.Serial, menu.MenuId,
                            (ushort)menuItem.Index, menuItem.Graphic, menuItem.Hue));
                        MenuStore.Clear();
                    }
                    break;
                }

                default:
                    _logger.LogWarning("Unknown macro action: {Action}", action);
                    break;
            }
        }

        /// <summary>
        /// Risolve un argomento serial: prima controlla gli alias locali,
        /// poi prova a parsare come numero decimale o esadecimale (prefisso 0x).
        /// </summary>
        private uint ResolveSerial(string s)
        {
            if (_aliases.TryGetValue(s, out uint aliasValue)) return aliasValue;
            if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                return Convert.ToUInt32(s, 16);
            return uint.TryParse(s, out uint v) ? v : 0;
        }

        // -------------------------------------------------------------------------
        // Recording
        // -------------------------------------------------------------------------

        public void StartRecording(string? name = null)
        {
            if (_isPlaying || _isRecording) return;
            _isRecording = true;
            ActiveMacro = name ?? $"macro_{DateTime.Now:yyyyMMdd_HHmmss}";
            lock (_recordingBuffer)
            {
                _recordingBuffer.Clear();
            }
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

            // TASK-04.1 recording: WarMode 0x72
            Action<byte[]> onWarMode = data =>
            {
                if (data.Length >= 2)
                    lock (_recordingBuffer) _recordingBuffer.Add($"WARMODE {(data[1] != 0 ? "on" : "off")}");
            };
            // TASK-04.2 recording: FLY 0xBF sub 0x32
            Action<byte[]> onExtendedC2S = data =>
            {
                if (data.Length >= 5 && data[3] == 0x00 && data[4] == 0x32)
                    lock (_recordingBuffer) _recordingBuffer.Add(_worldService.Player?.Flying ?? false ? "FLY" : "LAND");
            };

            _packetService.RegisterViewer(PacketPath.ClientToServer, 0x06, onDoubleClick);
            _packetService.RegisterViewer(PacketPath.ClientToServer, 0x09, onSingleClick);
            _packetService.RegisterViewer(PacketPath.ClientToServer, 0xAD, onSpeech);
            _packetService.RegisterViewer(PacketPath.ClientToServer, 0x6C, onTarget);
            _packetService.RegisterViewer(PacketPath.ClientToServer, 0x12, onTextCmd);
            _packetService.RegisterViewer(PacketPath.ClientToServer, 0x05, onAttack);
            _packetService.RegisterViewer(PacketPath.ClientToServer, 0x72, onWarMode);
            _packetService.RegisterViewer(PacketPath.ClientToServer, 0xBF, onExtendedC2S);

            _recordingUnsubscribers.Add(() => _packetService.UnregisterViewer(PacketPath.ClientToServer, 0x06, onDoubleClick));
            _recordingUnsubscribers.Add(() => _packetService.UnregisterViewer(PacketPath.ClientToServer, 0x09, onSingleClick));
            _recordingUnsubscribers.Add(() => _packetService.UnregisterViewer(PacketPath.ClientToServer, 0xAD, onSpeech));
            _recordingUnsubscribers.Add(() => _packetService.UnregisterViewer(PacketPath.ClientToServer, 0x6C, onTarget));
            _recordingUnsubscribers.Add(() => _packetService.UnregisterViewer(PacketPath.ClientToServer, 0x12, onTextCmd));
            _recordingUnsubscribers.Add(() => _packetService.UnregisterViewer(PacketPath.ClientToServer, 0x05, onAttack));
            _recordingUnsubscribers.Add(() => _packetService.UnregisterViewer(PacketPath.ClientToServer, 0x72, onWarMode));
            _recordingUnsubscribers.Add(() => _packetService.UnregisterViewer(PacketPath.ClientToServer, 0xBF, onExtendedC2S));

            _logger.LogInformation("Recording macro '{Name}' started", name);
        }

        // -------------------------------------------------------------------------
        // Stop / Save / Load / Delete / Rename
        // -------------------------------------------------------------------------

        public void StopRecording()
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

        public void SetAlias(string name, uint serial) => _aliases[name] = serial;
        public void RemoveAlias(string name) => _aliases.Remove(name);

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

        /// <summary>
        /// Risolve l'argomento del comando ATTACK in un serial da attaccare.
        /// Sintassi supportate:
        ///   ATTACK nearest [notoriety]    — mobile più vicino, filtro notoriety opzionale
        ///   ATTACK farthest [notoriety]   — mobile più lontano
        ///   ATTACK bytype 0xXXXX          — mobile con quel graphic ID
        ///   ATTACK 0xXXXXXXXX             — serial diretto
        /// Notoriety valori: enemy(4), criminal(3), gray(3), innocent(1), murderer(6)
        /// </summary>
        private uint ResolveAttackTarget(string args)
        {
            if (string.IsNullOrWhiteSpace(args)) return 0;

            var player = _worldService.Player;
            if (player == null) return 0;

            var parts = args.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string mode = parts[0].ToUpperInvariant();

            if (mode == "NEAREST" || mode == "FARTHEST")
            {
                byte? notorietyFilter = parts.Length > 1 ? ParseNotoriety(parts[1]) : null;

                var candidates = _worldService.Mobiles
                    .Where(m => m.Serial != player.Serial && !m.IsGhost)
                    .Where(m => notorietyFilter == null || m.Notoriety == notorietyFilter.Value);

                Mobile? chosen = mode == "NEAREST"
                    ? candidates.OrderBy(m => m.DistanceTo(player)).FirstOrDefault()
                    : candidates.OrderByDescending(m => m.DistanceTo(player)).FirstOrDefault();

                return chosen?.Serial ?? 0;
            }

            if (mode == "BYTYPE" && parts.Length > 1)
            {
                // Supporta sia "0x0190" che il numero decimale
                ushort graphic = parts[1].StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                    ? Convert.ToUInt16(parts[1], 16)
                    : ushort.TryParse(parts[1], out ushort g) ? g : (ushort)0;

                if (graphic == 0) return 0;

                return _worldService.Mobiles
                    .Where(m => m.Graphic == graphic && !m.IsGhost)
                    .OrderBy(m => m.DistanceTo(player))
                    .FirstOrDefault()?.Serial ?? 0;
            }

            // Fallback: serial diretto (hex o decimale)
            if (args.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                if (uint.TryParse(args[2..], System.Globalization.NumberStyles.HexNumber, null, out uint hexSerial))
                    return hexSerial;
            }

            uint.TryParse(args, out uint serial);
            return serial;
        }

        private static byte? ParseNotoriety(string token) => token.ToLowerInvariant() switch
        {
            "innocent" => 1,
            "friend"   => 2,
            "gray"     => 3,
            "criminal" => 3,
            "enemy"    => 4,
            "murderer" => 5,
            "attackable" => 6,
            _          => null
        };

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
