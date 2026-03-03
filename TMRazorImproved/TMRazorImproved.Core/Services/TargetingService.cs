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
        private readonly ILogger<TargetingService> _logger;

        private uint _lastTarget;
        private List<uint> _targetQueue = new();
        private int _queueIndex = -1;

        public uint LastTarget 
        { 
            get => _lastTarget; 
            set => _lastTarget = value; 
        }

        public event Action<uint>? TargetReceived;

        public TargetingService(
            IPacketService packetService, 
            IConfigService configService,
            IWorldService worldService,
            IHotkeyService hotkeyService,
            ILogger<TargetingService> logger)
        {
            _packetService = packetService;
            _configService = configService;
            _worldService = worldService;
            _logger = logger;

            // Registrazione Handlers Pacchetti Client->Server
            _packetService.RegisterViewer(PacketPath.ClientToServer, 0x6C, HandleTargetResponse);

            // Registrazione Hotkeys
            hotkeyService.RegisterAction("Target Next", () => TargetNext());
            hotkeyService.RegisterAction("Target Closest", () => TargetClosest());
            hotkeyService.RegisterAction("Target Self", () => TargetSelf());
            hotkeyService.RegisterAction("Last Target", () => { if (_lastTarget != 0) SendTarget(_lastTarget); });
            hotkeyService.RegisterAction("Clear Target", () => Clear());
        }

        private void HandleTargetResponse(byte[] data)
        {
            if (data.Length < 11) return;
            
            // Pacchetto 0x6C (Client to Server)
            // [0] 0x6C
            // [1] type (0=location, 1=object)
            // [2-5] cursor id
            // [6] action (0=target, 1=cancel)
            // [7-10] serial
            
            uint serial = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(7));
            byte action = data[6];

            if (action == 0 && serial != 0)
            {
                _lastTarget = serial;
                _logger.LogDebug("Target Response Received: 0x{Serial:X}", serial);
                
                // Notifichiamo gli iscritti (es. l'Inspector)
                TargetReceived?.Invoke(serial);
            }
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

            // Gestione del ciclo della coda
            if (_targetQueue.SequenceEqual(validTargets.Select(t => t.Serial)))
            {
                _queueIndex = (_queueIndex + 1) % _targetQueue.Count;
            }
            else
            {
                _targetQueue = validTargets.Select(t => t.Serial).ToList();
                _queueIndex = 0;
            }

            uint targetSerial = _targetQueue[_queueIndex];
            SetLastTarget(targetSerial);
            _logger.LogDebug("Target Next: 0x{Serial:X}", targetSerial);
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
            _targetQueue.Clear();
            _queueIndex = -1;
            _logger.LogDebug("Target Cleared");
        }

        public void SendTarget(uint serial)
        {
            // Invia il pacchetto 0x6C (Target) al server
            byte[] packet = new byte[19];
            packet[0] = 0x6C;
            packet[1] = 0x01; // Object
            BinaryPrimitives.WriteUInt32BigEndian(packet.AsSpan(2), 0); // Cursor ID
            packet[6] = 0x00; // Action: Target
            BinaryPrimitives.WriteUInt32BigEndian(packet.AsSpan(7), serial);
            BinaryPrimitives.WriteUInt16BigEndian(packet.AsSpan(11), 0); // X
            BinaryPrimitives.WriteUInt16BigEndian(packet.AsSpan(13), 0); // Y
            packet[15] = 0; // Z
            BinaryPrimitives.WriteUInt16BigEndian(packet.AsSpan(16), 0); // Graphic (ItemID)

            _packetService.SendToServer(packet);
        }

        public void SetLastTarget(uint serial)
        {
            _lastTarget = serial;
            // In TMRazor classico, settare il last target spesso invia anche il target fisico 
            // se c'è un cursore attivo nel client. Per ora lo settiamo e basta.
        }

        private List<Mobile> GetValidTargets()
        {
            if (_worldService.Player == null) return new List<Mobile>();

            var config = _configService.CurrentProfile.Targeting;
            var friends = _configService.CurrentProfile.Friends;

            return _worldService.Mobiles
                .Where(m => m.Serial != _worldService.Player.Serial)
                .Where(m => GetDistanceToPlayer(m) <= config.Range)
                .Where(m => 
                {
                    // Filtro Amici
                    if (friends.Contains(m.Serial)) return config.TargetFriends;
                    
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
