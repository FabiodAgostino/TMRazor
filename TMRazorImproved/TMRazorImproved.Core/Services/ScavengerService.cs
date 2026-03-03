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

namespace TMRazorImproved.Core.Services
{
    public class ScavengerService : AgentServiceBase, IScavengerService, IRecipient<UOPacketMessage>
    {
        private readonly IPacketService _packetService;
        private readonly IConfigService _configService;
        private readonly IWorldService _worldService;
        private readonly ILogger<ScavengerService> _logger;
        private readonly IMessenger _messenger;

        private readonly ConcurrentQueue<uint> _scavengeQueue = new();
        private readonly HashSet<uint> _processedSerials = new();

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

            _messenger.Register<UOPacketMessage>(this);

            hotkeyService.RegisterAction("Scavenger Start", () => Start());
            hotkeyService.RegisterAction("Scavenger Stop", () => _ = StopAsync());
            hotkeyService.RegisterAction("Scavenger Toggle", () => { if (IsRunning) _ = StopAsync(); else Start(); });
        }

        public void Receive(UOPacketMessage message)
        {
            if (!IsRunning) return;
            if (message.Path != Shared.Enums.PacketPath.ServerToClient) return;

            byte[] data = message.Value.Data;
            if (data.Length == 0) return;

            if (data[0] == 0x1A) // World Item
            {
                HandleWorldItem(data);
            }
        }

        private void HandleWorldItem(byte[] data)
        {
            var reader = new UOBufferReader(data);
            reader.ReadByte(); // 0x1A
            reader.ReadUInt16(); // length
            uint serial = reader.ReadUInt32() & 0x7FFFFFFF;
            ushort graphic = (ushort)(reader.ReadUInt16() & 0x7FFF);

            if ((serial & 0x80000000) != 0)
                reader.ReadUInt16(); // amount

            ushort x = reader.ReadUInt16();
            ushort y = reader.ReadUInt16();
            sbyte z = reader.ReadSByte();

            var config = _configService.CurrentProfile.Scavenger;
            var lootList = new HashSet<uint>(config.ItemList);

            if (lootList.Contains(graphic) && !_processedSerials.Contains(serial))
            {
                if (_worldService.Player != null)
                {
                    int dist = GetDistance(_worldService.Player.X, _worldService.Player.Y, x & 0x7FFF, y);
                    if (dist <= config.Range)
                    {
                        _scavengeQueue.Enqueue(serial);
                        _processedSerials.Add(serial);
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
                if (_scavengeQueue.TryDequeue(out uint serial))
                {
                    var config = _configService.CurrentProfile.Scavenger;
                    uint targetContainer = config.Container;

                    if (targetContainer != 0)
                    {
                        _logger.LogDebug("Scavenging item 0x{Serial:X}", serial);
                        MoveItem(serial, targetContainer);
                        await Task.Delay(600, token); 
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
            // 1. Lift Request (0x07)
            byte[] lift = new byte[7];
            lift[0] = 0x07;
            BinaryPrimitives.WriteUInt32BigEndian(lift.AsSpan(1), serial);
            BinaryPrimitives.WriteUInt16BigEndian(lift.AsSpan(5), 1);
            _packetService.SendToServer(lift);

            // 2. Drop Request (0x08)
            byte[] drop = new byte[15];
            drop[0] = 0x08;
            BinaryPrimitives.WriteUInt32BigEndian(drop.AsSpan(1), serial);
            BinaryPrimitives.WriteUInt16BigEndian(drop.AsSpan(5), 0xFFFF);
            BinaryPrimitives.WriteUInt16BigEndian(drop.AsSpan(7), 0xFFFF);
            drop[9] = 0;
            BinaryPrimitives.WriteUInt32BigEndian(drop.AsSpan(11), targetContainer);
            _packetService.SendToServer(drop);
        }

        protected override void OnStopped()
        {
            _logger.LogInformation("Scavenger agent stopped");
            while (_scavengeQueue.TryDequeue(out _)) { }
        }
    }
}
