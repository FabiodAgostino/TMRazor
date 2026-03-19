using System;
using System.Collections.Generic;
using System.Linq;
using System.Buffers.Binary;
using Microsoft.Extensions.Logging;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;
using TMRazorImproved.Shared.Models.Config;
using TMRazorImproved.Shared.Enums;
using TMRazorImproved.Core.Utilities;

namespace TMRazorImproved.Core.Services
{
    public class TargetingService : ITargetingService
    {
        private readonly IPacketService _packetService;
        private readonly IConfigService _configService;
        private readonly IWorldService _worldService;
        private readonly IFriendsService _friendsService;
        private readonly ITargetFilterService _targetFilterService;
        private readonly ILogger<TargetingService> _logger;

        // BUG-P1-01 FIX: _lastTarget e _hasPrompt acceduti da packet thread e UI thread →
        // devono essere volatile per garantire visibilità cross-thread senza lock overhead.
        private volatile uint _lastTarget;

        // 039-A: Tipi di target separati per harm/bene/ground
        private volatile uint _lastHarmTarget;
        private volatile uint _lastBeneTarget;
        private ushort _lastGroundX;
        private ushort _lastGroundY;
        private volatile sbyte _lastGroundZ;

        // 039-B: Tipo di cursore persistente (non viene azzerato da ClearTargetCursor)
        // per permettere a HandleTargetResponse di categorizzare il target
        private volatile byte _lastActiveCursorType;

        // 039-C: Coda azione differita — un'azione in attesa del prossimo cursore
        private Action? _deferredTargetAction;
        private readonly object _deferredLock = new();

        // _targetQueue e _queueIndex: modificati da hotkey callbacks (qualsiasi thread) →
        // accessi protetti da _queueLock.
        private List<uint> _targetQueue = new();
        private int _queueIndex = -1;
        private readonly object _queueLock = new();
        private volatile bool _hasPrompt;
        private volatile bool _hasTargetCursor;
        private volatile uint _pendingCursorId;
        private volatile byte _pendingCursorType;
        // FIX P1-02: tracking serial/promptId dal pacchetto 0x9A/0xC2 S2C
        private volatile uint _pendingPromptSerial;
        private volatile uint _pendingPromptId;
        private volatile uint _pendingPromptType;
        private volatile bool _pendingPromptIsUnicode;

        public uint LastTarget
        {
            get => _lastTarget;
            set => _lastTarget = value;
        }

        // 039-A: proprietà per i target tipizzati
        public uint LastHarmTarget
        {
            get => _lastHarmTarget;
            set => _lastHarmTarget = value;
        }

        public uint LastBeneTarget
        {
            get => _lastBeneTarget;
            set => _lastBeneTarget = value;
        }

        public ushort LastGroundX => _lastGroundX;
        public ushort LastGroundY => _lastGroundY;
        public sbyte LastGroundZ => _lastGroundZ;

        public bool HasPrompt => _hasPrompt;
        public bool HasTargetCursor => _hasTargetCursor;
        public uint PendingCursorId => _pendingCursorId;
        public byte PendingCursorType => _pendingCursorType;
        public uint PendingPromptSerial => _pendingPromptSerial;
        public uint PendingPromptId => _pendingPromptId;
        public bool PendingPromptIsUnicode => _pendingPromptIsUnicode;

        public event Action<uint>? TargetCursorRequested;
        public event Action<TargetInfo>? TargetReceived;
        public event Action<bool>? PromptChanged;

        public TargetingService(
            IPacketService packetService, 
            IConfigService configService,
            IWorldService worldService,
            IHotkeyService hotkeyService,
            IFriendsService friendsService,
            ITargetFilterService targetFilterService,
            ILogger<TargetingService> logger)
        {
            _packetService = packetService;
            _configService = configService;
            _worldService = worldService;
            _friendsService = friendsService;
            _targetFilterService = targetFilterService;
            _logger = logger;

            // Registrazione Handlers Pacchetti Server->Client (richiesta di target)
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x6C, HandleServerTargetCursor);
            // Registrazione Handlers Pacchetti Client->Server (risposta al target)
            _packetService.RegisterViewer(PacketPath.ClientToServer, 0x6C, HandleTargetResponse);
            // FIX P1-02: tracking serial/promptId dal pacchetto 0x9A S2C (ASCII Prompt)
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x9A, HandlePromptFromServer);
            // TASK-011: Unicode Prompt (0xC2 S2C)
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0xC2, HandleUnicodePromptFromServer);

            // Registrazione Hotkeys
            hotkeyService.RegisterAction("Target Next", () => TargetNext());
            hotkeyService.RegisterAction("Target Closest", () => TargetClosest());
            hotkeyService.RegisterAction("Target Self", () => TargetSelf());
            // 039-B/C: usa DoLastTarget per smart targeting + deferred queue
            hotkeyService.RegisterAction("Last Target", () => DoLastTarget());
            hotkeyService.RegisterAction("Clear Target", () => Clear());
        }

        private void HandleServerTargetCursor(byte[] data)
        {
            // Pacchetto 0x6C S2C: il server chiede al client di selezionare un target
            // [0] 0x6C  [1] targetType  [2-5] cursorId  [6] cursorType
            if (data.Length < 7) return;

            uint cursorId = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(2));
            _pendingCursorId = cursorId;
            _pendingCursorType = data[6];
            // 039-B: salva tipo cursore in campo persistente (non viene azzerato da ClearTargetCursor)
            _lastActiveCursorType = data[6];
            _hasTargetCursor = true;
            _logger.LogDebug("Target Cursor Received from Server: cursorId=0x{CursorId:X}, type={CursorType}", cursorId, _pendingCursorType);
            TargetCursorRequested?.Invoke(cursorId);

            // 039-C: esegui azione differita se presente
            Action? deferred;
            lock (_deferredLock)
            {
                deferred = _deferredTargetAction;
                _deferredTargetAction = null;
            }
            deferred?.Invoke();
        }

        private void HandlePromptFromServer(byte[] data)
        {
            // Pacchetto 0x9A S2C: cmd(1) len(2) serial(4) promptId(4) type(4) = 15 byte minimo
            if (data.Length < 15) return;

            _pendingPromptSerial = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(3));
            _pendingPromptId = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(7));
            _pendingPromptType = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(11));
            _pendingPromptIsUnicode = false;
            _logger.LogDebug("ASCII Prompt Received: serial=0x{Serial:X} promptId=0x{PromptId:X}", _pendingPromptSerial, _pendingPromptId);
            SetPrompt(true);
        }

        private void HandleUnicodePromptFromServer(byte[] data)
        {
            // Pacchetto 0xC2 S2C: cmd(1) len(2) serial(4) promptId(4) type(4)
            if (data.Length < 15) return;

            _pendingPromptSerial = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(3));
            _pendingPromptId = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(7));
            _pendingPromptType = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(11));
            _pendingPromptIsUnicode = true;
            _logger.LogDebug("Unicode Prompt Received: serial=0x{Serial:X} promptId=0x{PromptId:X}", _pendingPromptSerial, _pendingPromptId);
            SetPrompt(true);
        }

        private void HandleTargetResponse(byte[] data)
        {
            // Pacchetto 0x6C C2S: il client risponde al server con il target selezionato
            // [0] 0x6C  [1] type  [2-5] cursorId  [6] action  [7-10] serial  [11-12] x  [13-14] y  [15] z  [16-17] graphic
            if (data.Length < 19) return;

            uint serial = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(7));
            byte action = data[6];

            if (action == 0)
            {
                ushort x = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(11));
                ushort y = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(13));
                sbyte z = (sbyte)data[15];
                ushort graphic = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(16));

                if (serial != 0)
                {
                    // 039-A: aggiorna _lastTarget sempre + harm/bene in base al tipo cursore
                    _lastTarget = serial;
                    if (_lastActiveCursorType == 0x01)
                        _lastHarmTarget = serial;
                    else if (_lastActiveCursorType == 0x02)
                        _lastBeneTarget = serial;
                }
                else
                {
                    // 039-A: target a terra (locazione)
                    _lastGroundX = x;
                    _lastGroundY = y;
                    _lastGroundZ = z;
                }

                var info = new TargetInfo
                {
                    Serial = serial,
                    X = x,
                    Y = y,
                    Z = z,
                    Graphic = graphic
                };

                _logger.LogDebug("Target Response Received: Serial=0x{Serial:X} X={X} Y={Y} Z={Z} Graphic={Graphic}", serial, info.X, info.Y, info.Z, info.Graphic);
                TargetReceived?.Invoke(info);
            }
        }

        public void ClearTargetCursor()
        {
            _hasTargetCursor = false;
            _pendingCursorId = 0;
            _pendingCursorType = 0;
        }

        public void RequestTarget()
        {
            // Invia il pacchetto 0x6C (Target Cursor) al client
            byte[] packet = new byte[19];
            packet[0] = 0x6C;
            packet[1] = 0x01; // Object mode (di solito per inspector vogliamo oggetti)
            BinaryPrimitives.WriteUInt32BigEndian(packet.AsSpan(2), 0xDEADC0DE); // Unique ID per questo cursore
            packet[6] = 0x00; // Action: Request
            
            _logger.LogDebug("Requesting target cursor from client");
            _packetService.SendToClient(packet);
        }

        public void RequestLocationTarget()
        {
            // Invia il pacchetto 0x6C (Target Cursor) al client in modalità locazione
            byte[] packet = new byte[19];
            packet[0] = 0x6C;
            packet[1] = 0x00; // Location mode (0x00 = ground/land)
            BinaryPrimitives.WriteUInt32BigEndian(packet.AsSpan(2), 0xDEADC0DE); 
            packet[6] = 0x00; // Action: Request
            
            _logger.LogDebug("Requesting location target cursor from client");
            _packetService.SendToClient(packet);
        }

        public void TargetNext()
        {
            var validTargets = GetValidTargets();
            if (validTargets.Count == 0) return;

            lock (_queueLock)
            {
                var validSerials = validTargets.Select(t => t.Serial).ToList();
                if (_targetQueue.SequenceEqual(validSerials))
                {
                    _queueIndex = (_queueIndex + 1) % _targetQueue.Count;
                }
                else
                {
                    _targetQueue = validSerials;
                    _queueIndex = 0;
                }

                uint targetSerial = _targetQueue[_queueIndex];
                SetLastTarget(targetSerial);
                _logger.LogDebug("Target Next: 0x{Serial:X}", targetSerial);
            }
        }

        public void TargetClosest()
        {
            var validTargets = GetValidTargets();
            var closest = validTargets.OrderBy(t => GetDistanceToPlayer(t)).FirstOrDefault();
            
            if (closest != null)
            {
                SetLastTarget(closest.Serial);
                _logger.LogDebug("Target Closest: 0x{Serial:X} ({Name})", closest.Serial, closest.Name);
            }
        }

        public void TargetSelf()
        {
            if (_worldService.Player == null) return;

            if (!_hasTargetCursor)
            {
                // 039-C: cursor non attivo → accoda per quando arriverà
                lock (_deferredLock)
                    _deferredTargetAction = () => TargetSelf();
                _logger.LogDebug("Target Self queued (no cursor)");
                return;
            }

            SendTarget(_worldService.Player.Serial);
            _logger.LogDebug("Target Self");
        }

        /// <summary>
        /// 039-B/C: Smart Last Target — sceglie harm/bene/neutral in base al tipo di cursore attivo.
        /// Se il cursore non è attivo, accoda l'azione per il prossimo cursore.
        /// </summary>
        public void DoLastTarget()
        {
            if (!_hasTargetCursor)
            {
                // 039-C: cursor non attivo → accoda per quando arriverà
                lock (_deferredLock)
                    _deferredTargetAction = () => DoLastTarget();
                _logger.LogDebug("Last Target queued (no cursor)");
                return;
            }

            var config = _configService.CurrentProfile?.Targeting;

            // 039-B: seleziona il target appropriato in base al tipo di cursore
            uint target = _lastTarget;
            if (config?.SmartLastTarget == true)
            {
                if (_pendingCursorType == 0x01 && _lastHarmTarget != 0)
                    target = _lastHarmTarget;
                else if (_pendingCursorType == 0x02 && _lastBeneTarget != 0)
                    target = _lastBeneTarget;
            }

            if (target == 0)
            {
                _logger.LogDebug("Last Target: no target set");
                return;
            }

            // 039-E: range check configurabile
            if (config?.RangeCheckEnabled == true)
            {
                var mobile = _worldService.Mobiles.FirstOrDefault(m => m.Serial == target);
                if (mobile != null)
                {
                    int distance = GetDistanceToPlayer(mobile);
                    if (distance > config.MaxRange)
                    {
                        _logger.LogDebug("Last Target out of range ({Distance} > {Max})", distance, config.MaxRange);
                        return;
                    }
                }
            }

            // 039-D: blocca heal su target avvelenato
            if (_pendingCursorType == 0x02 && config?.BlockHealPoisoned == true)
            {
                var mobile = _worldService.Mobiles.FirstOrDefault(m => m.Serial == target);
                if (mobile?.IsPoisoned == true)
                {
                    _logger.LogDebug("Blocked healing poisoned target 0x{Serial:X}", target);
                    return;
                }
            }

            _logger.LogDebug("Last Target: 0x{Serial:X} (cursorType={Type})", target, _pendingCursorType);
            SendTarget(target);
        }

        public void Clear()
        {
            _lastTarget = 0;
            lock (_queueLock) { _targetQueue.Clear(); _queueIndex = -1; }
            _logger.LogDebug("Target Cleared");
        }

        public void SendTarget(uint serial)
        {
            SendTarget(serial, 0, 0, 0, 0);
        }

        public void SendTarget(uint serial, ushort x, ushort y, sbyte z, ushort graphic)
        {
            // Invia il pacchetto 0x6C (Target) al server usando il cursorId pendente
            uint cursorId = _pendingCursorId;
            ClearTargetCursor();

            byte[] packet = new byte[19];
            packet[0] = 0x6C;
            packet[1] = (serial == 0) ? (byte)0x00 : (byte)0x01; // 0=Loc, 1=Obj
            BinaryPrimitives.WriteUInt32BigEndian(packet.AsSpan(2), cursorId);
            packet[6] = 0x00; // Action: Target
            BinaryPrimitives.WriteUInt32BigEndian(packet.AsSpan(7), serial);
            BinaryPrimitives.WriteUInt16BigEndian(packet.AsSpan(11), x);
            BinaryPrimitives.WriteUInt16BigEndian(packet.AsSpan(13), y);
            packet[15] = (byte)z;
            BinaryPrimitives.WriteUInt16BigEndian(packet.AsSpan(16), graphic);

            _packetService.SendToServer(packet);
        }

        public void CancelTarget()
        {
            uint cursorId = _pendingCursorId;
            ClearTargetCursor();

            byte[] packet = new byte[19];
            packet[0] = 0x6C;
            BinaryPrimitives.WriteUInt32BigEndian(packet.AsSpan(2), cursorId);
            packet[6] = 0x01; // Action: Cancel
            _packetService.SendToServer(packet);
        }

        public async Task<TargetInfo> AcquireTargetAsync()
        {
            var tcs = new TaskCompletionSource<TargetInfo>();

            void OnTarget(TargetInfo info)
            {
                tcs.TrySetResult(info);
            }

            TargetReceived += OnTarget;
            RequestTarget();

            try
            {
                // Timeout dopo 30 secondi se l'utente non seleziona nulla
                var delayTask = Task.Delay(TimeSpan.FromSeconds(30));
                var completedTask = await Task.WhenAny(tcs.Task, delayTask);

                if (completedTask == delayTask)
                    return default;

                return await tcs.Task;
            }
            finally
            {
                TargetReceived -= OnTarget;
            }
        }

        public void SetLastTarget(uint serial)
        {
            _lastTarget = serial;
            // In TMRazor classico, settare il last target spesso invia anche il target fisico 
            // se c'è un cursore attivo nel client. Per ora lo settiamo e basta.
        }

        public void SetPrompt(bool hasPrompt)
        {
            if (_hasPrompt != hasPrompt)
            {
                _hasPrompt = hasPrompt;
                _logger.LogDebug("Prompt State Changed: {State}", hasPrompt);
                PromptChanged?.Invoke(hasPrompt);
            }
        }

        public void SendPrompt(string text)
        {
            if (_pendingPromptSerial == 0) return;

            byte[] packet;
            if (_pendingPromptIsUnicode)
            {
                // Pacchetto 0xC2 C2S (Unicode Prompt Response)
                packet = PacketBuilder.UnicodePromptResponse(_pendingPromptSerial, _pendingPromptId, _pendingPromptType, text);
            }
            else
            {
                // Pacchetto 0x9A C2S (ASCII Prompt Response)
                packet = PacketBuilder.PromptResponse(_pendingPromptSerial, _pendingPromptId, _pendingPromptType, text);
            }

            _pendingPromptSerial = 0;
            _pendingPromptId = 0;
            _pendingPromptType = 0;

            _packetService.SendToServer(packet);
            SetPrompt(false);
        }

        private List<Mobile> GetValidTargets()
        {
            if (_worldService.Player == null) return new List<Mobile>();

            var profile = _configService.CurrentProfile;
            if (profile == null) return new List<Mobile>();

            var config = profile.Targeting;

            return _worldService.Mobiles
                .Where(m => m.Serial != _worldService.Player.Serial)
                .Where(m => GetDistanceToPlayer(m) <= config.Range)
                .Where(m => 
                {
                    // Filtro Target Esclusi (TargetFilterManager legacy)
                    if (_targetFilterService.IsFiltered(m.Serial)) return false;

                    // Filtro Amici
                    if (_friendsService.IsFriend(m.Serial)) return config.TargetFriends;
                    
                    // Filtro Innocenti (Blu)
                    if (m.Notoriety == 1) return config.TargetInnocents;

                    // Altri filtri basati su notorietà possono essere aggiunti qui
                    return true;
                })
                .ToList();
        }

        private int GetDistanceToPlayer(Mobile m)
        {
            if (_worldService.Player == null) return 999;
            return Math.Max(Math.Abs(_worldService.Player.X - m.X), Math.Abs(_worldService.Player.Y - m.Y));
        }
    }
}
