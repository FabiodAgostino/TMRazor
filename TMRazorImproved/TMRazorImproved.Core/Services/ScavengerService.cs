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
using TMRazorImproved.Core.Utilities;

namespace TMRazorImproved.Core.Services
{
    public class ScavengerService : AgentServiceBase, IScavengerService, IRecipient<WorldItemMessage>, IRecipient<LoginCompleteMessage>
    {
        private readonly IPacketService _packetService;
        private readonly IWorldService _worldService;
        private readonly IDragDropCoordinator _dragDropCoordinator;
        private readonly ILogger<ScavengerService> _logger;
        private readonly IMessenger _messenger;

        private readonly ConcurrentQueue<uint> _scavengeQueue = new();
        // FIX BUG-C03: HashSet<uint> non thread-safe → ConcurrentDictionary<uint,byte>
        private readonly ConcurrentDictionary<uint, byte> _processedSerials = new();

        public ScavengerService(
            IPacketService packetService, 
            IConfigService configService,
            IWorldService worldService,
            IDragDropCoordinator dragDropCoordinator,
            IMessenger messenger,
            IHotkeyService hotkeyService,
            ILogger<ScavengerService> logger) : base(configService)
        {
            _packetService = packetService;
            _worldService = worldService;
            _dragDropCoordinator = dragDropCoordinator;
            _messenger = messenger;
            _logger = logger;

            _messenger.Register<WorldItemMessage>(this);
            _messenger.Register<LoginCompleteMessage>(this);

            hotkeyService.RegisterAction("Scavenger Start", () => Start());
            hotkeyService.RegisterAction("Scavenger Stop", () => _ = StopAsync());
            hotkeyService.RegisterAction("Scavenger Toggle", () => { if (IsRunning) _ = StopAsync(); else Start(); });
        }

        public void ChangeList(string listName)
        {
            var profile = _configService.CurrentProfile;
            if (profile != null)
            {
                profile.ActiveScavengerList = listName;
                _logger.LogInformation("Scavenger list changed to: {ListName}", listName);
            }
        }

        // BUG-P1-04 FIX: return type non-nullable con possibile return null → CS8603
        private ScavengerConfig? GetActiveConfig()
        {
            return GetActiveConfig(p => p.ScavengerLists, p => p.ActiveScavengerList);
        }

        // FR-052: AutoStart on login — enable the active list if AutoStart == true
        public void Receive(LoginCompleteMessage message)
        {
            var config = GetActiveConfig();
            if (config?.AutoStart == true)
            {
                config.Enabled = true;
                _logger.LogInformation("Scavenger: AutoStart attivato per lista '{Name}'", config.Name);
            }
        }

        public void Receive(WorldItemMessage message)
        {
            var config = GetActiveConfig();
            if (config == null || !config.Enabled || !IsRunning) return;

            var item = message.Value;
            if (_processedSerials.ContainsKey(item.Serial)) return;

            // Se la lista è vuota, raccoglie tutto (comportamento legacy Scavenger)
            // Se non è vuota, verifica graphic e proprietà
            LootItem? lootItemConfig = null;
            if (config.ItemList.Count > 0)
            {
                lootItemConfig = config.ItemList.FirstOrDefault(li => li.IsEnabled && li.Graphic == item.Graphic);
                if (lootItemConfig == null) return;
                
                // Verifica proprietà se presenti filtri
                if (lootItemConfig.PropertyFilters.Any() && !MatchProperties(item, lootItemConfig.PropertyFilters))
                    return;
            }

            if (_worldService.Player != null)
            {
                int dist = _worldService.Player.DistanceTo(item);
                if (dist <= config.Range)
                {
                    _scavengeQueue.Enqueue(item.Serial);
                    _processedSerials.TryAdd(item.Serial, 0);
                }
            }
        }

        protected override async Task AgentLoopAsync(CancellationToken token)
        {
            _logger.LogInformation("Scavenger agent loop started");
            _processedSerials.Clear();

            while (!token.IsCancellationRequested)
            {
                var config = GetActiveConfig();
                if (config == null || !config.Enabled)
                {
                    await Task.Delay(500, token);
                    continue;
                }

                if (_scavengeQueue.TryDequeue(out uint serial))
                {
                    uint targetContainer = config.Container;

                    if (targetContainer != 0)
                    {
                        var item = _worldService.FindItem(serial);
                        if (item == null) continue;

                        // FR-046: skip items that are "Locked Down" (house items)
                        if (item.Properties != null &&
                            item.Properties.Any(p => p.IndexOf("Locked Down", StringComparison.OrdinalIgnoreCase) >= 0))
                        {
                            _logger.LogDebug("Scavenger: skipping locked-down item 0x{Serial:X}", serial);
                            continue;
                        }

                        ushort amount = item.Amount;
                        _logger.LogDebug("Scavenging item 0x{Serial:X} (Amount: {Amount})", serial, amount);
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

        // FR-045: one-shot scavenge pass — drains the current queue into the target container
        public void RunOnce()
        {
            var config = GetActiveConfig();
            if (config == null) return;

            uint targetContainer = config.Container != 0 ? config.Container
                : _worldService.Player?.Backpack?.Serial ?? 0;
            if (targetContainer == 0) return;

            _ = Task.Run(async () =>
            {
                while (_scavengeQueue.TryDequeue(out uint serial))
                {
                    var item = _worldService.FindItem(serial);
                    if (item == null) continue;
                    if (item.Properties != null &&
                        item.Properties.Any(p => p.IndexOf("Locked Down", StringComparison.OrdinalIgnoreCase) >= 0))
                        continue;
                    await MoveItemAsync(serial, item.Amount, targetContainer);
                    await Task.Delay(Math.Max(100, config.Delay));
                }
            });
        }

        // FR-045: returns the active scavenger bag serial
        public uint GetScavengerBag()
        {
            var config = GetActiveConfig();
            if (config != null && config.Container != 0)
            {
                if (_worldService.FindItem(config.Container) != null) return config.Container;
            }
            return _worldService.Player?.Backpack?.Serial ?? 0;
        }

        // FR-045: clears processed serials and pending queue
        public void ResetIgnore()
        {
            _processedSerials.Clear();
            while (_scavengeQueue.TryDequeue(out _)) { }
        }

        private async Task<bool> MoveItemAsync(uint serial, ushort amount, uint targetContainer)
        {
            return await _dragDropCoordinator.RequestDragDrop(serial, targetContainer, amount);
        }

        protected override void OnStopped()
        {
            _logger.LogInformation("Scavenger agent stopped");
            while (_scavengeQueue.TryDequeue(out _)) { }
        }
    }
}
