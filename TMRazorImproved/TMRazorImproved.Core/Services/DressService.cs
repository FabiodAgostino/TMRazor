using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;
using TMRazorImproved.Shared.Models.Config;
using System.Buffers.Binary;

namespace TMRazorImproved.Core.Services
{
    public class DressService : AgentServiceBase, IDressService
    {
        private readonly IPacketService _packetService;
        private readonly IConfigService _configService;
        private readonly IWorldService _worldService;
        private readonly ILogger<DressService> _logger;
        
        private readonly ConcurrentQueue<ActionTask> _actionQueue = new();

        private record ActionTask(uint Serial, byte Layer, bool IsDress);

        public DressService(
            IPacketService packetService, 
            IConfigService configService,
            IWorldService worldService,
            IHotkeyService hotkeyService,
            ILogger<DressService> logger)
        {
            _packetService = packetService;
            _configService = configService;
            _worldService = worldService;
            _logger = logger;

            // In TMRazor le hotkey sono dinamiche per ogni lista.
            // Per ora registriamo azioni generiche che verranno mappate dalla UI
            hotkeyService.RegisterAction("Dress Current", () => Dress("Default"));
            hotkeyService.RegisterAction("Undress Current", () => Undress("Default"));
        }

        public void Dress(string listName)
        {
            var list = _configService.CurrentProfile.DressLists.FirstOrDefault(l => l.Name == listName);
            if (list == null) return;

            foreach (var kvp in list.LayerItems)
            {
                _actionQueue.Enqueue(new ActionTask(kvp.Value, kvp.Key, true));
            }

            if (!IsRunning) Start();
        }

        public void Undress(string listName)
        {
            var list = _configService.CurrentProfile.DressLists.FirstOrDefault(l => l.Name == listName);
            if (list == null) return;

            foreach (var kvp in list.LayerItems)
            {
                _actionQueue.Enqueue(new ActionTask(kvp.Value, kvp.Key, false));
            }

            if (!IsRunning) Start();
        }

        protected override async Task AgentLoopAsync(CancellationToken token)
        {
            _logger.LogInformation("Dress agent loop started");

            while (!token.IsCancellationRequested)
            {
                if (_actionQueue.TryDequeue(out var task))
                {
                    if (task.IsDress)
                    {
                        EquipItem(task.Serial, task.Layer);
                    }
                    else
                    {
                        UnequipItem(task.Serial);
                    }

                    // Delay tipico tra un cambio e l'altro per evitare blocchi del server
                    await Task.Delay(600, token);
                }
                else
                {
                    // Se la coda è vuota, l'agente ha finito il suo compito per ora
                    break; 
                }
            }
        }

        private void EquipItem(uint serial, byte layer)
        {
            // 1. Solleviamo l'oggetto (0x07)
            byte[] lift = new byte[7];
            lift[0] = 0x07;
            BinaryPrimitives.WriteUInt32BigEndian(lift.AsSpan(1), serial);
            BinaryPrimitives.WriteUInt16BigEndian(lift.AsSpan(5), 1);
            _packetService.SendToServer(lift);

            // 2. WearItem Request (0x13): cmd(1) itemSerial(4) layer(1) mobileSerial(4)
            // FIX BUG-C01: 0x05 era Attack Request, il corretto pacchetto di equip è 0x13
            byte[] equip = new byte[10];
            equip[0] = 0x13;
            BinaryPrimitives.WriteUInt32BigEndian(equip.AsSpan(1), serial);
            equip[5] = layer;
            BinaryPrimitives.WriteUInt32BigEndian(equip.AsSpan(6), _worldService.Player?.Serial ?? 0);
            _packetService.SendToServer(equip);

            _logger.LogDebug("Equipping item 0x{Serial:X} to layer {Layer}", serial, layer);
        }

        private void UnequipItem(uint serial)
        {
            var player = _worldService.Player;
            if (player == null) return;

            // In UO undress significa sollevare l'oggetto e rimetterlo nel backpack
            // Cerchiamo il seriale del backpack (layer 21)
            uint backpackSerial = player.Backpack?.Serial ?? 0;
            if (backpackSerial == 0)
            {
                _logger.LogWarning("Undress failed: Backpack serial not found.");
                return;
            }

            // 1. Lift Request (0x07)
            byte[] lift = new byte[7];
            lift[0] = 0x07;
            BinaryPrimitives.WriteUInt32BigEndian(lift.AsSpan(1), serial);
            BinaryPrimitives.WriteUInt16BigEndian(lift.AsSpan(5), 1);
            _packetService.SendToServer(lift);

            // 2. Drop Request (0x08) verso il backpack
            byte[] drop = new byte[15];
            drop[0] = 0x08;
            BinaryPrimitives.WriteUInt32BigEndian(drop.AsSpan(1), serial);
            BinaryPrimitives.WriteUInt16BigEndian(drop.AsSpan(5), 0xFFFF);
            BinaryPrimitives.WriteUInt16BigEndian(drop.AsSpan(7), 0xFFFF);
            drop[9] = 0;
            BinaryPrimitives.WriteUInt32BigEndian(drop.AsSpan(11), backpackSerial);
            _packetService.SendToServer(drop);

            _logger.LogDebug("Unequipping item 0x{Serial:X} to backpack 0x{Backpack:X}", serial, backpackSerial);
        }

        protected override void OnStopped()
        {
            _logger.LogInformation("Dress agent stopped");
            while (_actionQueue.TryDequeue(out _)) { }
        }
    }
}
