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
        private readonly IWeaponService _weaponService;
        private readonly ILogger<DressService> _logger;
        
        private readonly ConcurrentQueue<ActionTask> _actionQueue = new();
        // FR-041: track whether current op is dress (true) or undress (false)
        private volatile bool _isDressingNow;

        private record ActionTask(uint Serial, byte Layer, bool IsDress);

        public DressService(
            IPacketService packetService, 
            IConfigService configService,
            IWorldService worldService,
            IHotkeyService hotkeyService,
            IWeaponService weaponService,
            ILogger<DressService> logger) : base(configService)
        {
            _packetService = packetService;
            _worldService = worldService;
            _weaponService = weaponService;
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

            _isDressingNow = true;

            // FR-040: UO3D batch equip — send a single EquipItemMacro packet if Use3D is enabled
            if (list.Use3D && list.LayerItems.Count > 0)
            {
                var serials = list.LayerItems.Values.ToList();
                _packetService.SendToServer(PacketBuilder.EquipItemMacro(serials));
                _logger.LogDebug("Dress (UO3D): sent EquipItemMacro for {Count} items", serials.Count);
                return;
            }

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

            _isDressingNow = false;

            // FR-040: UO3D batch undress — send a single UnEquipItemMacro packet if Use3D is enabled
            if (list.Use3D && list.LayerItems.Count > 0)
            {
                var layers = list.LayerItems.Keys.ToList();
                _packetService.SendToServer(PacketBuilder.UnEquipItemMacro(layers));
                _logger.LogDebug("Undress (UO3D): sent UnEquipItemMacro for {Count} layers", layers.Count);
                return;
            }

            foreach (var kvp in list.LayerItems)
            {
                _actionQueue.Enqueue(new ActionTask(kvp.Value, kvp.Key, false));
            }

            if (!IsRunning) Start();
        }

        // FR-039: captures current player equipment into the active dress list
        public void ReadPlayerDress()
        {
            var player = _worldService.Player;
            if (player == null)
            {
                _logger.LogWarning("ReadPlayerDress: player not logged in");
                return;
            }

            var config = GetActiveConfig();
            if (config == null) return;

            config.LayerItems.Clear();

            // layers 1..0x1A cover all equip slots; skip layer 0 (unequipped/on ground)
            var equippedItems = _worldService.Items
                .Where(i => i.Container == player.Serial && i.Layer > 0 && i.Layer <= 0x1A);

            foreach (var item in equippedItems)
            {
                config.LayerItems[item.Layer] = item.Serial;
            }

            _logger.LogInformation("ReadPlayerDress: captured {Count} items into list '{List}'",
                config.LayerItems.Count, config.Name);
        }

        // FR-041: separate dress/undress status tracking
        public bool DressStatus() => IsRunning && _isDressingNow;
        public bool UnDressStatus() => IsRunning && !_isDressingNow;

        protected override async Task AgentLoopAsync(CancellationToken token)
        {
            _logger.LogInformation("Dress agent loop started");

            while (!token.IsCancellationRequested)
            {
                if (_actionQueue.TryDequeue(out var task))
                {
                    if (task.IsDress)
                    {
                        var item = _worldService.FindItem(task.Serial);
                        if (item != null)
                        {
                            // Risoluzione conflitti 2-mani
                            if (_weaponService.IsTwoHanded(item.Graphic))
                            {
                                // Se l'item è a 2 mani, rimuovi sia RightHand che LeftHand prima
                                await UnequipLayer(0x01, token); // RightHand
                                await UnequipLayer(0x02, token); // LeftHand
                            }
                            else if (task.Layer == 0x02) // LeftHand (Scudo o arma 1-mano)
                            {
                                // Se vogliamo equipaggiare qualcosa in LeftHand, 
                                // controlliamo se c'è un'arma a 2 mani in RightHand
                                var rightItem = GetItemOnLayer(0x01);
                                if (rightItem != null && _weaponService.IsTwoHanded(rightItem.Graphic))
                                {
                                    await UnequipLayer(0x01, token);
                                }
                            }
                            else if (task.Layer == 0x01) // RightHand
                            {
                                // Se equipaggiamo in RightHand, controlliamo se c'è già qualcosa
                                // (WearItem di solito fallisce se lo slot è occupato)
                                await UnequipLayer(0x01, token);
                            }

                            EquipItem(task.Serial, task.Layer);
                        }
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

        private async Task UnequipLayer(byte layer, CancellationToken token)
        {
            var item = GetItemOnLayer(layer);
            if (item != null)
            {
                UnequipItem(item.Serial);
                await Task.Delay(600, token);
            }
        }

        private Item? GetItemOnLayer(byte layer)
        {
            return _worldService.Items.FirstOrDefault(i => i.Container == _worldService.Player?.Serial && i.Layer == layer);
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
