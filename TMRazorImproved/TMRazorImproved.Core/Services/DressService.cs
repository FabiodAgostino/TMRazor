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
using TMRazorImproved.Core.Utilities;

namespace TMRazorImproved.Core.Services
{
    public class DressService : AgentServiceBase, IDressService
    {
        private readonly IPacketService _packetService;
        private readonly IWorldService _worldService;
        private readonly ILogger<DressService> _logger;
        
        private readonly ConcurrentQueue<ActionTask> _actionQueue = new();

        private record ActionTask(uint Serial, byte Layer, bool IsDress);

        public DressService(
            IPacketService packetService, 
            IConfigService configService,
            IWorldService worldService,
            IHotkeyService hotkeyService,
            ILogger<DressService> logger) : base(configService)
        {
            _packetService = packetService;
            _worldService = worldService;
            _logger = logger;

            // In TMRazor le hotkey sono dinamiche per ogni lista.
            // Per ora registriamo azioni generiche che verranno mappate dalla UI
            hotkeyService.RegisterAction("Dress Current", () => Dress("Default"));
            hotkeyService.RegisterAction("Undress Current", () => Undress("Default"));
        }

        public void ChangeList(string listName)
        {
            var profile = _configService.CurrentProfile;
            if (profile != null)
            {
                profile.ActiveDressList = listName;
                _logger.LogInformation("Dress list changed to: {ListName}", listName);
            }
        }

        private DressList? GetActiveConfig()
        {
            return GetActiveConfig(p => p.DressLists, p => p.ActiveDressList);
        }

        public void DressUp()
        {
            var config = GetActiveConfig();
            if (config != null)
            {
                Dress(config.Name);
            }
        }

        public void Undress()
        {
            var config = GetActiveConfig();
            if (config != null)
            {
                Undress(config.Name);
            }
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
            uint playerSerial = _worldService.Player?.Serial ?? 0;
            _packetService.SendToServer(PacketBuilder.LiftItem(serial));
            _packetService.SendToServer(PacketBuilder.WearItem(serial, layer, playerSerial));
            _logger.LogDebug("Equipping item 0x{Serial:X} to layer {Layer}", serial, layer);
        }

        private void UnequipItem(uint serial)
        {
            var player = _worldService.Player;
            if (player == null) return;

            uint backpackSerial = player.Backpack?.Serial ?? 0;
            if (backpackSerial == 0)
            {
                _logger.LogWarning("Undress failed: Backpack serial not found.");
                return;
            }

            _packetService.SendToServer(PacketBuilder.LiftItem(serial));
            _packetService.SendToServer(PacketBuilder.DropToContainer(serial, backpackSerial));
            _logger.LogDebug("Unequipping item 0x{Serial:X} to backpack 0x{Backpack:X}", serial, backpackSerial);
        }

        protected override void OnStopped()
        {
            _logger.LogInformation("Dress agent stopped");
            while (_actionQueue.TryDequeue(out _)) { }
        }
    }
}
