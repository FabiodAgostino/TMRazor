using System;
using System.Collections.Generic;
using System.Linq;
using System.Buffers.Binary;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Mvvm.Messaging;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;
using TMRazorImproved.Shared.Models.Config;
using TMRazorImproved.Shared.Messages;

namespace TMRazorImproved.Core.Services
{
    public class VendorService : AgentServiceBase, IVendorService, IRecipient<VendorBuyMessage>, IRecipient<VendorSellMessage>
    {
        private readonly IPacketService _packetService;
        private readonly IConfigService _configService;
        private readonly IWorldService _worldService;
        private readonly ILogger<VendorService> _logger;
        private readonly IMessenger _messenger;

        public VendorService(
            IPacketService packetService, 
            IConfigService configService,
            IWorldService worldService,
            IMessenger messenger,
            ILogger<VendorService> logger)
        {
            _packetService = packetService;
            _configService = configService;
            _worldService = worldService;
            _messenger = messenger;
            _logger = logger;

            _messenger.Register<VendorBuyMessage>(this);
            _messenger.Register<VendorSellMessage>(this);
        }

        private VendorConfig GetActiveConfig()
        {
            var profile = _configService.CurrentProfile;
            if (profile == null) return null;
            return profile.VendorLists.FirstOrDefault(l => l.Name == profile.ActiveVendorList) 
                   ?? profile.VendorLists.FirstOrDefault();
        }

        public void Receive(VendorBuyMessage message)
        {
            var config = GetActiveConfig();
            if (config == null || !config.BuyEnabled) return;

            _logger.LogInformation("Vendor Buy Menu detected for vendor 0x{VendorSerial:X}", message.Value.VendorSerial);

            // FIX BUG-P1-03: Il server invia sempre 0x3C (container content) PRIMA di 0x74 (buy window).
            // WorldService.LastOpenedContainer contiene il serial del container aperto più di recente,
            // che corrisponde al contenitore buy del vendor. I suoi item (con Serial e Graphic reali)
            // sono già stati aggiunti al WorldService dalla ContainerContentMessage.
            uint vendorContainerSerial = _worldService.LastOpenedContainer;
            if (vendorContainerSerial == 0)
            {
                _logger.LogWarning("VendorBuy: LastOpenedContainer is 0, cannot determine vendor container serial. Buy aborted.");
                return;
            }

            var vendorItems = _worldService.GetItemsInContainer(vendorContainerSerial).ToList();
            if (vendorItems.Count == 0)
            {
                _logger.LogWarning("VendorBuy: No items found in vendor container 0x{Container:X}", vendorContainerSerial);
                return;
            }

            var toBuy = new List<(uint Serial, ushort Amount)>();

            foreach (var buyReq in config.BuyList)
            {
                if (!buyReq.IsEnabled) continue;

                // Cerca per Graphic nell'inventario reale del vendor (con serial corretti da 0x3C)
                var match = vendorItems.FirstOrDefault(i => i.Graphic == buyReq.Graphic);
                if (match != null)
                {
                    ushort qty = (ushort)Math.Min(buyReq.Amount > 0 ? buyReq.Amount : 1, match.Amount);
                    toBuy.Add((match.Serial, qty));
                    _logger.LogDebug("VendorBuy: queued 0x{Graphic:X} x{Amount} (serial 0x{Serial:X})", buyReq.Graphic, qty, match.Serial);
                }
                else
                {
                    _logger.LogDebug("VendorBuy: item 0x{Graphic:X} not found in vendor inventory", buyReq.Graphic);
                }
            }

            if (toBuy.Count > 0)
            {
                SendBuyResponse(message.Value.VendorSerial, toBuy);
            }
        }

        public void Receive(VendorSellMessage message)
        {
            var config = GetActiveConfig();
            if (config == null || !config.SellEnabled) return;

            _logger.LogInformation("Vendor Sell Menu detected, generating response...");

            var toSell = new List<(uint Serial, ushort Amount)>();

            foreach (var vendorItem in message.Value.Items)
            {
                var match = config.SellList.FirstOrDefault(l => l.IsEnabled && l.Graphic == vendorItem.Graphic);
                if (match != null)
                {
                    toSell.Add((vendorItem.Serial, vendorItem.Amount));
                }
            }

            if (toSell.Count > 0)
            {
                SendSellResponse(message.Value.VendorSerial, toSell);
            }
        }

        private void SendBuyResponse(uint vendorSerial, List<(uint Serial, ushort Amount)> toBuy)
        {
            int len = 8 + (toBuy.Count * 7);
            byte[] data = new byte[len];
            data[0] = 0x3B;
            BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(1), (ushort)len);
            BinaryPrimitives.WriteUInt32BigEndian(data.AsSpan(3), vendorSerial);
            data[7] = 0x02; // flag (0x02 indicates list)
            int offset = 8;
            foreach (var item in toBuy)
            {
                data[offset] = 0x1A; // layer usually 0x1A for buy list
                BinaryPrimitives.WriteUInt32BigEndian(data.AsSpan(offset + 1), item.Serial);
                BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(offset + 5), item.Amount);
                offset += 7;
            }
            _packetService.SendToServer(data);
        }

        private void SendSellResponse(uint vendorSerial, List<(uint Serial, ushort Amount)> toSell)
        {
            int len = 9 + (toSell.Count * 6);
            byte[] data = new byte[len];
            data[0] = 0x9F;
            BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(1), (ushort)len);
            BinaryPrimitives.WriteUInt32BigEndian(data.AsSpan(3), vendorSerial);
            BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(7), (ushort)toSell.Count);
            int offset = 9;
            foreach (var item in toSell)
            {
                BinaryPrimitives.WriteUInt32BigEndian(data.AsSpan(offset), item.Serial);
                BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(offset + 4), item.Amount);
                offset += 6;
            }
            _packetService.SendToServer(data);
        }

        protected override async System.Threading.Tasks.Task AgentLoopAsync(System.Threading.CancellationToken token)
        {
            await System.Threading.Tasks.Task.Delay(-1, token);
        }
    }
}
