using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Mvvm.Messaging;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;
using TMRazorImproved.Shared.Models.Config;
using TMRazorImproved.Shared.Messages;
using System.Buffers.Binary;
using System.Text.RegularExpressions;
using TMRazorImproved.Core.Utilities;

namespace TMRazorImproved.Core.Services
{
    public class AutoLootService : AgentServiceBase, IAutoLootService, IRecipient<ContainerContentMessage>, IRecipient<ContainerItemAddedMessage>
    {
        private readonly IPacketService _packetService;
        private readonly IWorldService _worldService;
        private readonly IDragDropCoordinator _dragDropCoordinator;
        private readonly ILogger<AutoLootService> _logger;
        private readonly IMessenger _messenger;

        private readonly ConcurrentQueue<uint> _lootQueue = new();
        // FIX BUG-C03: HashSet<uint> non thread-safe → ConcurrentDictionary<uint,byte>
        private readonly ConcurrentDictionary<uint, byte> _processedSerials = new();

        public AutoLootService(
            IPacketService packetService, 
            IConfigService configService,
            IWorldService worldService,
            IDragDropCoordinator dragDropCoordinator,
            IMessenger messenger,
            IHotkeyService hotkeyService,
            ILogger<AutoLootService> logger) : base(configService)
        {
            _packetService = packetService;
            _worldService = worldService;
            _dragDropCoordinator = dragDropCoordinator;
            _messenger = messenger;
            _logger = logger;

            _messenger.Register<ContainerContentMessage>(this);
            _messenger.Register<ContainerItemAddedMessage>(this);

            hotkeyService.RegisterAction("AutoLoot Start", () => Start());
            hotkeyService.RegisterAction("AutoLoot Stop", () => _ = StopAsync());
            hotkeyService.RegisterAction("AutoLoot Toggle", () => { if (IsRunning) _ = StopAsync(); else Start(); });
        }

        public void ChangeList(string listName)
        {
            var profile = _configService.CurrentProfile;
            if (profile != null)
            {
                profile.ActiveAutoLootList = listName;
                _logger.LogInformation("AutoLoot list changed to: {ListName}", listName);
            }
        }

        // FR-030: RunOnce — one-shot loot pass using the specified list
        public void RunOnce(string listName, int msDelay)
        {
            var profile = _configService.CurrentProfile;
            var config = profile?.AutoLootLists.FirstOrDefault(l => l.Name == listName);
            if (config == null)
            {
                _logger.LogWarning("AutoLoot.RunOnce: list '{ListName}' not found", listName);
                return;
            }

            _ = Task.Run(async () =>
            {
                uint targetContainer = config.Container;
                if (targetContainer == 0 && _worldService.Player?.Backpack != null)
                    targetContainer = _worldService.Player.Backpack.Serial;

                if (targetContainer == 0) return;

                while (_lootQueue.TryDequeue(out uint serial))
                {
                    var item = _worldService.FindItem(serial);
                    ushort amount = item?.Amount ?? 1;
                    await MoveItemAsync(serial, amount, targetContainer);
                    await Task.Delay(Math.Max(100, msDelay));
                }
            });
        }

        // FR-031: SetNoOpenCorpse — temporarily toggle the NoOpenCorpse flag, returns old value
        public bool SetNoOpenCorpse(bool noOpen)
        {
            var config = GetActiveConfig();
            if (config == null) return false;
            bool old = config.NoOpenCorpse;
            config.NoOpenCorpse = noOpen;
            return old;
        }

        // FR-032: GetList — returns the item list for the named AutoLoot config
        public List<LootItem> GetList(string listName)
        {
            var profile = _configService.CurrentProfile;
            var config = profile?.AutoLootLists.FirstOrDefault(l => l.Name == listName);
            return config?.ItemList ?? new List<LootItem>();
        }

        // FR-032: GetLootBag — returns the serial of the active loot bag
        public uint GetLootBag()
        {
            var config = GetActiveConfig();
            if (config != null && config.Container != 0)
            {
                var bag = _worldService.FindItem(config.Container);
                if (bag != null) return config.Container;
            }
            return _worldService.Player?.Backpack?.Serial ?? 0;
        }

        // FR-032: ResetIgnore — clears processed serials and pending loot queue
        public void ResetIgnore()
        {
            _processedSerials.Clear();
            while (_lootQueue.TryDequeue(out _)) { }
        }

        // BUG-P1-04 FIX: return type non-nullable con possibile return null → CS8603
        private AutoLootConfig? GetActiveConfig()
        {
            return GetActiveConfig(p => p.AutoLootLists, p => p.ActiveAutoLootList);
        }

        public void Receive(ContainerContentMessage message)
        {
            var config = GetActiveConfig();
            if (config == null || !config.Enabled || !IsRunning) return;

            foreach (var item in message.Value.Items)
            {
                if (_processedSerials.ContainsKey(item.Serial)) continue;

                var lootItemConfig = config.ItemList.FirstOrDefault(li => li.IsEnabled && li.Graphic == item.Graphic);
                if (lootItemConfig != null)
                {
                    if (config.NoOpenCorpse && IsCorpse(message.Value.ContainerSerial))
                        continue;

                    // Verifica proprietà se presenti filtri
                    if (lootItemConfig.PropertyFilters.Any())
                    {
                        var worldItem = _worldService.FindItem(item.Serial);
                        if (worldItem == null || !MatchProperties(worldItem, lootItemConfig.PropertyFilters))
                            continue;
                    }

                    _lootQueue.Enqueue(item.Serial);
                    _processedSerials.TryAdd(item.Serial, 0);
                }
            }
        }

        public void Receive(ContainerItemAddedMessage message)
        {
            var config = GetActiveConfig();
            if (config == null || !config.Enabled || !IsRunning) return;

            var item = _worldService.FindItem(message.Value.ItemSerial);
            if (item == null || _processedSerials.ContainsKey(item.Serial)) return;

            var lootItemConfig = config.ItemList.FirstOrDefault(li => li.IsEnabled && li.Graphic == item.Graphic);
            if (lootItemConfig != null)
            {
                if (config.NoOpenCorpse && IsCorpse(message.Value.ContainerSerial))
                    return;

                // Verifica proprietà se presenti filtri
                if (lootItemConfig.PropertyFilters.Any() && !MatchProperties(item, lootItemConfig.PropertyFilters))
                    return;

                _lootQueue.Enqueue(item.Serial);
                _processedSerials.TryAdd(item.Serial, 0);
            }
        }

        // FR-033: Detects direct corpses AND shared/instanced loot containers placed near a corpse
        // (UO Dreams / OSI instanced loot: server places a container within 3 tiles of the corpse)
        private bool IsCorpse(uint serial)
        {
            var item = _worldService.FindItem(serial);
            if (item == null) return false;

            // Direct corpse (graphic 0x2006)
            if (item.Graphic == 0x2006) return true;

            // Shared loot container: any container within 3 tiles of a known corpse
            return _worldService.Items.Any(i =>
                i.Graphic == 0x2006 &&
                Math.Abs(i.X - item.X) <= 3 &&
                Math.Abs(i.Y - item.Y) <= 3);
        }

        protected override async Task AgentLoopAsync(CancellationToken token)
        {
            _logger.LogInformation("AutoLoot agent loop started");
            _processedSerials.Clear(); // ConcurrentDictionary.Clear() è thread-safe

            while (!token.IsCancellationRequested)
            {
                var config = GetActiveConfig();
                if (config == null || !config.Enabled)
                {
                    await Task.Delay(500, token);
                    continue;
                }

                if (_worldService.Player != null && !config.AllowHidden && _worldService.Player.IsHidden)
                {
                    await Task.Delay(500, token);
                    continue;
                }

                if (_lootQueue.TryDequeue(out uint serial))
                {
                    uint targetContainer = config.Container;
                    
                    // Verifica range
                    var item = _worldService.FindItem(serial);
                    if (item != null && config.MaxRange > 0)
                    {
                        var dist = _worldService.Player?.DistanceTo(item) ?? 100;
                        if (dist > config.MaxRange)
                        {
                            _logger.LogDebug("Item 0x{Serial:X} out of range ({Dist} > {MaxRange}), will retry when closer", serial, dist, config.MaxRange);
                            _processedSerials.TryRemove(serial, out _);
                            continue;
                        }
                    }

                    if (targetContainer == 0 && _worldService.Player != null && _worldService.Player.Backpack != null)
                    {
                        targetContainer = _worldService.Player.Backpack.Serial;
                    }

                    if (targetContainer != 0)
                    {
                        ushort amount = item?.Amount ?? 1;
                        _logger.LogDebug("Looting item 0x{Serial:X} (Amount: {Amount})", serial, amount);
                        bool success = await MoveItemAsync(serial, amount, targetContainer);
                        
                        await Task.Delay(Math.Max(100, config.Delay), token); 
                    }
                }
                else
                {
                    await Task.Delay(100, token);
                }
            }
        }

        private async Task<bool> MoveItemAsync(uint serial, ushort amount, uint targetContainer)
        {
            return await _dragDropCoordinator.RequestDragDrop(serial, targetContainer, amount);
        }

        protected override void OnStopped()
        {
            _logger.LogInformation("AutoLoot agent stopped");
            while (_lootQueue.TryDequeue(out _)) { }
        }
    }
}
