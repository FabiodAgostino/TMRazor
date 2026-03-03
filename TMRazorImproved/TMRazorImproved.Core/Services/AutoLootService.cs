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
    public class AutoLootService : AgentServiceBase, IAutoLootService, IRecipient<UOPacketMessage>
    {
        private readonly IPacketService _packetService;
        private readonly IConfigService _configService;
        private readonly IWorldService _worldService;
        private readonly ILogger<AutoLootService> _logger;
        private readonly IMessenger _messenger;

        private readonly ConcurrentQueue<uint> _lootQueue = new();
        private readonly HashSet<uint> _processedSerials = new();

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

            _messenger.Register<UOPacketMessage>(this);

            hotkeyService.RegisterAction("AutoLoot Start", () => Start());
            hotkeyService.RegisterAction("AutoLoot Stop", () => _ = StopAsync());
            hotkeyService.RegisterAction("AutoLoot Toggle", () => { if (IsRunning) _ = StopAsync(); else Start(); });
        }

        public void Receive(UOPacketMessage message)
        {
            if (!IsRunning) return;
            if (message.Path != Shared.Enums.PacketPath.ServerToClient) return;

            byte[] data = message.Value.Data;
            if (data.Length == 0) return;

            if (data[0] == 0x3C) // Container Content
            {
                HandleContainerContent(data);
            }
            else if (data[0] == 0x25) // Container Item (Single)
            {
                HandleSingleItem(data);
            }
        }

        private void HandleContainerContent(byte[] data)
        {
            var reader = new UOBufferReader(data);
            reader.ReadByte(); // 0x3C
            reader.ReadUInt16(); // length
            ushort count = reader.ReadUInt16();

            var config = _configService.CurrentProfile.AutoLoot;
            var lootList = new HashSet<uint>(config.ItemList);

            for (int i = 0; i < count; i++)
            {
                uint serial = reader.ReadUInt32();
                ushort graphic = reader.ReadUInt16();
                reader.ReadByte(); // 0
                ushort amount = reader.ReadUInt16();
                ushort x = reader.ReadUInt16();
                ushort y = reader.ReadUInt16();
                uint containerSerial = reader.ReadUInt32();
                ushort hue = reader.ReadUInt16();

                if (lootList.Contains(graphic) && !_processedSerials.Contains(serial))
                {
                    _lootQueue.Enqueue(serial);
                    _processedSerials.Add(serial);
                }
            }
        }

        private void HandleSingleItem(byte[] data)
        {
            var reader = new UOBufferReader(data);
            reader.ReadByte(); // 0x25
            uint serial = reader.ReadUInt32();
            ushort graphic = reader.ReadUInt16();
            reader.ReadByte(); // 0
            ushort amount = reader.ReadUInt16();
            ushort x = reader.ReadUInt16();
            ushort y = reader.ReadUInt16();
            uint containerSerial = reader.ReadUInt32();
            ushort hue = reader.ReadUInt16();

            var config = _configService.CurrentProfile.AutoLoot;
            if (config.ItemList.Contains(graphic) && !_processedSerials.Contains(serial))
            {
                _lootQueue.Enqueue(serial);
                _processedSerials.Add(serial);
            }
        }

        protected override async Task AgentLoopAsync(CancellationToken token)
        {
            _logger.LogInformation("AutoLoot agent loop started");
            _processedSerials.Clear();

            while (!token.IsCancellationRequested)
            {
                if (_lootQueue.TryDequeue(out uint serial))
                {
                    var config = _configService.CurrentProfile.AutoLoot;
                    uint targetContainer = config.Container;

                    // Se non è impostato un container, usa il backpack del player
                    if (targetContainer == 0 && _worldService.Player != null)
                    {
                        // In UO il backpack è solitamente un item con layer specifico o serial noto
                        // Per ora assumiamo che l'utente debba settarlo o usiamo un fallback
                    }

                    if (targetContainer != 0)
                    {
                        _logger.LogDebug("Looting item 0x{Serial:X}", serial);
                        MoveItem(serial, targetContainer);
                        
                        // Delay per simulare il tempo di trascinamento e non floodare il server
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
            BinaryPrimitives.WriteUInt16BigEndian(lift.AsSpan(5), 1); // Amount 1 (TODO: gestire stack)
            _packetService.SendToServer(lift);

            // 2. Drop Request (0x08)
            byte[] drop = new byte[15];
            drop[0] = 0x08;
            BinaryPrimitives.WriteUInt32BigEndian(drop.AsSpan(1), serial);
            BinaryPrimitives.WriteUInt16BigEndian(drop.AsSpan(5), 0xFFFF); // X
            BinaryPrimitives.WriteUInt16BigEndian(drop.AsSpan(7), 0xFFFF); // Y
            drop[9] = 0; // Z
            BinaryPrimitives.WriteUInt32BigEndian(drop.AsSpan(11), targetContainer);
            _packetService.SendToServer(drop);
        }

        protected override void OnStopped()
        {
            _logger.LogInformation("AutoLoot agent stopped");
            while (_lootQueue.TryDequeue(out _)) { }
        }
    }
}
