using System;
using System.Collections.Generic;
using System.Linq;
using System.Buffers.Binary;
using Microsoft.Extensions.Logging;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;
using TMRazorImproved.Shared.Models.Config;
using TMRazorImproved.Shared.Enums;

namespace TMRazorImproved.Core.Services
{
    public class TargetingService : ITargetingService
    {
        private readonly IPacketService _packetService;
        private readonly IConfigService _configService;
        private readonly IWorldService _worldService;
        private readonly IFriendsService _friendsService;
        private readonly ILogger<TargetingService> _logger;

        // BUG-P1-01 FIX: _lastTarget e _hasPrompt acceduti da packet thread e UI thread →
        // devono essere volatile per garantire visibilità cross-thread senza lock overhead.
        private volatile uint _lastTarget;
        // _targetQueue e _queueIndex: modificati da hotkey callbacks (qualsiasi thread) →
        // accessi protetti da _queueLock.
        private List<uint> _targetQueue = new();
        private int _queueIndex = -1;
        private readonly object _queueLock = new();
        private volatile bool _hasPrompt;
        private volatile bool _hasTargetCursor;
        private volatile uint _pendingCursorId;
        // FIX P1-02: tracking serial/promptId dal pacchetto 0x9A S2C
        private volatile uint _pendingPromptSerial;
        private volatile uint _pendingPromptId;

        public uint LastTarget
        {
            get => _lastTarget;   // volatile read
            set => _lastTarget = value;  // volatile write
        }

        public bool HasPrompt => _hasPrompt;  // volatile read
        public bool HasTargetCursor => _hasTargetCursor;
        public uint PendingCursorId => _pendingCursorId;
        public uint PendingPromptSerial => _pendingPromptSerial;
        public uint PendingPromptId => _pendingPromptId;

        public event Action<uint>? TargetCursorRequested;
        public event Action<uint>? TargetReceived;
        public event Action<bool>? PromptChanged;

        public TargetingService(
            IPacketService packetService, 
            IConfigService configService,
            IWorldService worldService,
            IHotkeyService hotkeyService,
            IFriendsService friendsService,
            ILogger<TargetingService> logger)
        {
            _packetService = packetService;
            _configService = configService;
            _worldService = worldService;
            _friendsService = friendsService;
            _logger = logger;

            // Registrazione Handlers Pacchetti Server->Client (richiesta di target)
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x6C, HandleServerTargetCursor);
            // Registrazione Handlers Pacchetti Client->Server (risposta al target)
            _packetService.RegisterViewer(PacketPath.ClientToServer, 0x6C, HandleTargetResponse);
            // FIX P1-02: tracking serial/promptId dal pacchetto 0x9A S2C (ASCII Prompt)
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x9A, HandlePromptFromServer);

            // Registrazione Hotkeys
            hotkeyService.RegisterAction("Target Next", () => TargetNext());
            hotkeyService.RegisterAction("Target Closest", () => TargetClosest());
            hotkeyService.RegisterAction("Target Self", () => TargetSelf());
            hotkeyService.RegisterAction("Last Target", () => { if (_lastTarget != 0) SendTarget(_lastTarget); });
            hotkeyService.RegisterAction("Clear Target", () => Clear());
        }

        private void HandleServerTargetCursor(byte[] data)
        {
            // Pacchetto 0x6C S2C: il server chiede al client di selezionare un target
            // [0] 0x6C  [1] targetType  [2-5] cursorId  [6] cursorType
            if (data.Length < 7) return;

            uint cursorId = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(2));
            _pendingCursorId = cursorId;
            _hasTargetCursor = true;
            _logger.LogDebug("Target Cursor Received from Server: cursorId=0x{CursorId:X}", cursorId);
            TargetCursorRequested?.Invoke(cursorId);
        }

        private void HandlePromptFromServer(byte[] data)
        {
            // Pacchetto 0x9A S2C: cmd(1) len(2) serial(4) promptId(4) type(4) = 15 byte minimo
            if (data.Length < 15) return;

            _pendingPromptSerial = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(3));
            _pendingPromptId = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(7));
            _logger.LogDebug("Prompt Received from Server: serial=0x{Serial:X} promptId=0x{PromptId:X}", _pendingPromptSerial, _pendingPromptId);
            SetPrompt(true);
        }

        private void HandleTargetResponse(byte[] data)
        {
            // Pacchetto 0x6C C2S: il client risponde al server con il target selezionato
            // [0] 0x6C  [1] type  [2-5] cursorId  [6] action  [7-10] serial
            if (data.Length < 11) return;

            uint serial = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(7));
            byte action = data[6];

            if (action == 0 && serial != 0)
            {
                _lastTarget = serial;
                _logger.LogDebug("Target Response Received: 0x{Serial:X}", serial);
                TargetReceived?.Invoke(serial);
            }
        }

        public void ClearTargetCursor()
        {
            _hasTargetCursor = false;
            _pendingCursorId = 0;
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
            if (_worldService.Player != null)
            {
                SendTarget(_worldService.Player.Serial);
                _logger.LogDebug("Target Self");
            }
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

        public async Task<uint> AcquireTargetAsync()
        {
            var tcs = new TaskCompletionSource<uint>();

            void OnTarget(uint serial)
            {
                tcs.TrySetResult(serial);
            }

            TargetReceived += OnTarget;
            RequestTarget();

            try
            {
                // Timeout dopo 30 secondi se l'utente non seleziona nulla
                var delayTask = Task.Delay(TimeSpan.FromSeconds(30));
                var completedTask = await Task.WhenAny(tcs.Task, delayTask);

                if (completedTask == delayTask)
                    return 0;

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
            // Pacchetto 0x9A C2S (ASCII Prompt Response): cmd(1) len(2) serial(4) promptId(4) type(4) text(var ASCII null-term)
            // FIX P1-02: usa serial e promptId tracciati dal 0x9A S2C ricevuto in HandlePromptFromServer
            byte[] textBytes = System.Text.Encoding.ASCII.GetBytes(text);
            byte[] packet = new byte[15 + textBytes.Length + 1];

            packet[0] = 0x9A;
            BinaryPrimitives.WriteUInt16BigEndian(packet.AsSpan(1), (ushort)packet.Length);
            BinaryPrimitives.WriteUInt32BigEndian(packet.AsSpan(3), _pendingPromptSerial);
            BinaryPrimitives.WriteUInt32BigEndian(packet.AsSpan(7), _pendingPromptId);
            // type(4) a [11..14] = 0 (normal text response — already 0 from new byte[])
            textBytes.CopyTo(packet, 15);
            packet[packet.Length - 1] = 0x00; // Null terminator

            _pendingPromptSerial = 0;
            _pendingPromptId = 0;

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
