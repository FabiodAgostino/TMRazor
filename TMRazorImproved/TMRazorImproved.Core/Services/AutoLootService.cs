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
    public class AutoLootService : AgentServiceBase, IAutoLootService, IRecipient<ContainerContentMessage>, IRecipient<ContainerItemAddedMessage>
    {
        private readonly IPacketService _packetService;
        private readonly IConfigService _configService;
        private readonly IWorldService _worldService;
        private readonly ILogger<AutoLootService> _logger;
        private readonly IMessenger _messenger;

        private readonly ConcurrentQueue<uint> _lootQueue = new();
        // FIX BUG-C03: HashSet<uint> non thread-safe → ConcurrentDictionary<uint,byte>
        private readonly ConcurrentDictionary<uint, byte> _processedSerials = new();

        public AutoLootService(
            IPacketService packetService, 
            IConfigService configService,
            IWorldService worldService,
            IMessenger messenger,
            IHotkeyService hotkeyService,
            ILogger<AutoLootService> logger)
        {
            _packetService = packetService;
            _configService = configService;
            _worldService = worldService;
            _messenger = messenger;
            _logger = logger;

            _messenger.Register<ContainerContentMessage>(this);
            _messenger.Register<ContainerItemAddedMessage>(this);

            hotkeyService.RegisterAction("AutoLoot Start", () => Start());
            hotkeyService.RegisterAction("AutoLoot Stop", () => _ = StopAsync());
            hotkeyService.RegisterAction("AutoLoot Toggle", () => { if (IsRunning) _ = StopAsync(); else Start(); });
        }

        private AutoLootConfig GetActiveConfig()
        {
            var profile = _configService.CurrentProfile;
            if (profile == null) return null;
            return profile.AutoLootLists.FirstOrDefault(l => l.Name == profile.ActiveAutoLootList) 
                   ?? profile.AutoLootLists.FirstOrDefault();
        }

        public void Receive(ContainerContentMessage message)
        {
            var config = GetActiveConfig();
            if (config == null || !config.Enabled || !IsRunning) return;

            foreach (var item in message.Value.Items)
            {
                bool shouldLoot = config.ItemList.Any(li => li.IsEnabled && li.Graphic == item.Graphic);
                if (shouldLoot && !_processedSerials.ContainsKey(item.Serial))
                {
                    if (config.NoOpenCorpse && IsCorpse(message.Value.ContainerSerial))
                        continue;

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
            if (item == null) return;

            bool shouldLoot = config.ItemList.Any(li => li.IsEnabled && li.Graphic == item.Graphic);
            if (shouldLoot && !_processedSerials.ContainsKey(item.Serial))
            {
                if (config.NoOpenCorpse && IsCorpse(message.Value.ContainerSerial))
                    return;

                _lootQueue.Enqueue(item.Serial);
                _processedSerials.TryAdd(item.Serial, 0);
            }
        }

        private bool IsCorpse(uint serial)
        {
            // In UO i serial dei corpse iniziano solitamente con 0x40000000 e hanno un range specifico
            // o possiamo interpellare il WorldService per il tipo di oggetto
            var item = _worldService.FindItem(serial);
            return item?.Graphic == 0x2006;
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
                        _logger.LogDebug("Looting item 0x{Serial:X}", serial);
                        MoveItem(serial, targetContainer);
                        
                        await Task.Delay(Math.Max(100, config.Delay), token); 
                    }
                }
                else
                {
                    await Task.Delay(100, token);
                }
            }
        }

        private void MoveItem(uint serial, uint targetContainer)
        {
            _packetService.SendToServer(PacketBuilder.LiftItem(serial));
            _packetService.SendToServer(PacketBuilder.DropToContainer(serial, targetContainer));
        }

        protected override void OnStopped()
        {
            _logger.LogInformation("AutoLoot agent stopped");
            while (_lootQueue.TryDequeue(out _)) { }
        }
    }
}
