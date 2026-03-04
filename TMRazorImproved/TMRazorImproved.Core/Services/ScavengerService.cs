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
    public class ScavengerService : AgentServiceBase, IScavengerService, IRecipient<WorldItemMessage>
    {
        private readonly IPacketService _packetService;
        private readonly IConfigService _configService;
        private readonly IWorldService _worldService;
        private readonly ILogger<ScavengerService> _logger;
        private readonly IMessenger _messenger;

        private readonly ConcurrentQueue<uint> _scavengeQueue = new();
        // FIX BUG-C03: HashSet<uint> non thread-safe → ConcurrentDictionary<uint,byte>
        private readonly ConcurrentDictionary<uint, byte> _processedSerials = new();

        public ScavengerService(
            IPacketService packetService, 
            IConfigService configService,
            IWorldService worldService,
            IMessenger messenger,
            IHotkeyService hotkeyService,
            ILogger<ScavengerService> logger)
        {
            _packetService = packetService;
            _configService = configService;
            _worldService = worldService;
            _messenger = messenger;
            _logger = logger;

            _messenger.Register<WorldItemMessage>(this);

            hotkeyService.RegisterAction("Scavenger Start", () => Start());
            hotkeyService.RegisterAction("Scavenger Stop", () => _ = StopAsync());
            hotkeyService.RegisterAction("Scavenger Toggle", () => { if (IsRunning) _ = StopAsync(); else Start(); });
        }

        private ScavengerConfig GetActiveConfig()
        {
            var profile = _configService.CurrentProfile;
            if (profile == null) return null;
            return profile.ScavengerLists.FirstOrDefault(l => l.Name == profile.ActiveScavengerList) 
                   ?? profile.ScavengerLists.FirstOrDefault();
        }

        public void Receive(WorldItemMessage message)
        {
            var config = GetActiveConfig();
            if (config == null || !config.Enabled || !IsRunning) return;

            var item = message.Value;
            bool shouldScavenge = config.ItemList.Count == 0 || config.ItemList.Any(i => i.IsEnabled && i.Graphic == item.Graphic);

            if (shouldScavenge && !_processedSerials.ContainsKey(item.Serial))
            {
                if (_worldService.Player != null)
                {
                    int dist = GetDistance(_worldService.Player.X, _worldService.Player.Y, item.X, item.Y);
                    if (dist <= config.Range)
                    {
                        _scavengeQueue.Enqueue(item.Serial);
                        _processedSerials.TryAdd(item.Serial, 0);
                    }
                }
            }
        }

        private int GetDistance(int x1, int y1, int x2, int y2)
        {
            return Math.Max(Math.Abs(x1 - x2), Math.Abs(y1 - y2));
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
                        _logger.LogDebug("Scavenging item 0x{Serial:X}", serial);
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
            _logger.LogInformation("Scavenger agent stopped");
            while (_scavengeQueue.TryDequeue(out _)) { }
        }
    }
}
