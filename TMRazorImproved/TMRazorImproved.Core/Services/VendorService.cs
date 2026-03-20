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
        private readonly IWorldService _worldService;
        private readonly ILogger<VendorService> _logger;
        private readonly IMessenger _messenger;
        // FR-047: cache of last vendor buy list (Price info from 0x74)
        private IReadOnlyList<(uint Price, string Name)> _lastBuyItems = Array.Empty<(uint, string)>();

        public VendorService(
            IPacketService packetService, 
            IConfigService configService,
            IWorldService worldService,
            IMessenger messenger,
            ILogger<VendorService> logger) : base(configService)
        {
            _packetService = packetService;
            _worldService = worldService;
            _messenger = messenger;
            _logger = logger;

            _messenger.Register<VendorBuyMessage>(this);
            _messenger.Register<VendorSellMessage>(this);
        }

        private VendorConfig? GetActiveConfig()
        {
            return GetActiveConfig(p => p.VendorLists, p => p.ActiveVendorList);
        }

        public void Receive(VendorBuyMessage message)
        {
            var config = GetActiveConfig();
            if (config == null || !config.BuyEnabled) return;

            _logger.LogInformation("Vendor Buy Menu detected for vendor 0x{VendorSerial:X}", message.Value.VendorSerial);

            // FR-047: cache price info from 0x74 for BuyList() script API
            _lastBuyItems = message.Value.Items;

            // FIX BUG-P1-03: Il server invia sempre 0x3C (container content) PRIMA di 0x74 (buy window).
            uint vendorContainerSerial = _worldService.LastOpenedContainer;
            if (vendorContainerSerial == 0)
            {
                _logger.LogWarning("VendorBuy: LastOpenedContainer is 0. Buy aborted.");
                return;
            }

            var vendorItems = _worldService.GetItemsInContainer(vendorContainerSerial).ToList();
            if (vendorItems.Count == 0)
            {
                _logger.LogWarning("VendorBuy: No items in vendor container 0x{Container:X}", vendorContainerSerial);
                return;
            }

            var toBuy = new List<(uint Serial, ushort Amount)>();

            foreach (var buyReq in config.BuyList)
            {
                if (!buyReq.IsEnabled) continue;

                var match = vendorItems.FirstOrDefault(i => i.Graphic == buyReq.Graphic);
                if (match == null)
                {
                    _logger.LogDebug("VendorBuy: item 0x{Graphic:X} not in vendor inventory", buyReq.Graphic);
                    continue;
                }

                // FR-048: CompareName — skip if item name doesn't match config
                if (config.CompareName && !string.IsNullOrEmpty(buyReq.Name))
                {
                    if (match.Name == null || !match.Name.Contains(buyReq.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogDebug("VendorBuy: name mismatch for 0x{Graphic:X}: '{Actual}' vs '{Expected}'",
                            buyReq.Graphic, match.Name, buyReq.Name);
                        continue;
                    }
                }

                // FR-049: MaxBuyPrice — skip if unit price exceeds limit
                if (config.MaxBuyPrice > 0)
                {
                    var priceEntry = _lastBuyItems.FirstOrDefault(p =>
                        p.Name.Equals(match.Name ?? string.Empty, StringComparison.OrdinalIgnoreCase));
                    if (priceEntry.Price > (uint)config.MaxBuyPrice)
                    {
                        _logger.LogDebug("VendorBuy: price {Price} exceeds MaxBuyPrice {Max} for 0x{Graphic:X}",
                            priceEntry.Price, config.MaxBuyPrice, buyReq.Graphic);
                        continue;
                    }
                }

                int desiredQty = buyReq.Amount > 0 ? buyReq.Amount : 1;

                // FR-048: CompleteAmount — deduct items already in backpack
                if (config.CompleteAmount && _worldService.Player?.Backpack != null)
                {
                    int already = _worldService.GetItemsInContainer(_worldService.Player.Backpack.Serial)
                        .Where(i => i.Graphic == buyReq.Graphic)
                        .Sum(i => i.Amount);
                    desiredQty = Math.Max(0, desiredQty - already);
                }

                if (desiredQty <= 0) continue;

                ushort qty = (ushort)Math.Min(desiredQty, match.Amount);
                toBuy.Add((match.Serial, qty));
                _logger.LogDebug("VendorBuy: queued 0x{Graphic:X} x{Amount} (serial 0x{Serial:X})", buyReq.Graphic, qty, match.Serial);
            }

            if (toBuy.Count > 0)
            {
                ExecuteBuy(message.Value.VendorSerial, toBuy);
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
                ExecuteSell(message.Value.VendorSerial, toSell);
            }
        }

        public void ExecuteBuy(uint vendorSerial, List<(uint Serial, ushort Amount)> items)
        {
            int len = 8 + (items.Count * 7);
            byte[] data = new byte[len];
            data[0] = 0x3B;
            BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(1), (ushort)len);
            BinaryPrimitives.WriteUInt32BigEndian(data.AsSpan(3), vendorSerial);
            data[7] = 0x02; // flag (0x02 indicates list)
            int offset = 8;
            foreach (var item in items)
            {
                data[offset] = 0x1A; // layer usually 0x1A for buy list
                BinaryPrimitives.WriteUInt32BigEndian(data.AsSpan(offset + 1), item.Serial);
                BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(offset + 5), item.Amount);
                offset += 7;
            }
            _packetService.SendToServer(data);
            
            _logger.LogInformation("Sent Buy Response for vendor 0x{Vendor:X}, {Count} items.", vendorSerial, items.Count);
        }

        public void ExecuteSell(uint vendorSerial, List<(uint Serial, ushort Amount)> items)
        {
            int len = 9 + (items.Count * 6);
            byte[] data = new byte[len];
            data[0] = 0x9F;
            BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(1), (ushort)len);
            BinaryPrimitives.WriteUInt32BigEndian(data.AsSpan(3), vendorSerial);
            BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(7), (ushort)items.Count);
            int offset = 9;
            foreach (var item in items)
            {
                BinaryPrimitives.WriteUInt32BigEndian(data.AsSpan(offset), item.Serial);
                BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(offset + 4), item.Amount);
                offset += 6;
            }
            _packetService.SendToServer(data);

            _logger.LogInformation("Sent Sell Response for vendor 0x{Vendor:X}, {Count} items.", vendorSerial, items.Count);
        }

        // FR-047: script-driven buy by graphic ID
        public void Buy(uint vendorSerial, int itemID, int amount, int maxPrice = 0)
        {
            var containerSerial = _worldService.LastOpenedContainer;
            var match = _worldService.GetItemsInContainer(containerSerial)
                .FirstOrDefault(i => i.Graphic == (ushort)itemID);
            if (match == null)
            {
                _logger.LogWarning("Vendor.Buy: item 0x{ID:X} not in vendor container 0x{Container:X}", itemID, containerSerial);
                return;
            }
            if (maxPrice > 0)
            {
                var priceEntry = _lastBuyItems.FirstOrDefault(p =>
                    p.Name.Equals(match.Name ?? string.Empty, StringComparison.OrdinalIgnoreCase));
                if (priceEntry.Price > (uint)maxPrice)
                {
                    _logger.LogDebug("Vendor.Buy: price {Price} > maxPrice {Max}", priceEntry.Price, maxPrice);
                    return;
                }
            }
            ushort qty = (ushort)Math.Min(amount > 0 ? amount : 1, match.Amount);
            ExecuteBuy(vendorSerial, new List<(uint, ushort)> { (match.Serial, qty) });
        }

        // FR-047: script-driven buy by item name
        public void Buy(uint vendorSerial, string itemName, int amount, int maxPrice = 0)
        {
            var containerSerial = _worldService.LastOpenedContainer;
            var match = _worldService.GetItemsInContainer(containerSerial)
                .FirstOrDefault(i => (i.Name ?? string.Empty).Contains(itemName, StringComparison.OrdinalIgnoreCase));
            if (match == null)
            {
                _logger.LogWarning("Vendor.Buy: item '{Name}' not in vendor container 0x{Container:X}", itemName, containerSerial);
                return;
            }
            if (maxPrice > 0)
            {
                var priceEntry = _lastBuyItems.FirstOrDefault(p =>
                    p.Name.Contains(itemName, StringComparison.OrdinalIgnoreCase));
                if (priceEntry.Price > (uint)maxPrice)
                {
                    _logger.LogDebug("Vendor.Buy: price {Price} > maxPrice {Max}", priceEntry.Price, maxPrice);
                    return;
                }
            }
            ushort qty = (ushort)Math.Min(amount > 0 ? amount : 1, match.Amount);
            ExecuteBuy(vendorSerial, new List<(uint, ushort)> { (match.Serial, qty) });
        }

        // FR-047: returns the last-seen vendor items with price info
        public List<(string Name, int Graphic, uint Price)> BuyList(uint vendorSerial)
        {
            var containerSerial = _worldService.LastOpenedContainer;
            var containerItems = _worldService.GetItemsInContainer(containerSerial).ToList();
            return containerItems.Select(i =>
            {
                var priceEntry = _lastBuyItems.FirstOrDefault(p =>
                    p.Name.Equals(i.Name ?? string.Empty, StringComparison.OrdinalIgnoreCase));
                return (i.Name ?? string.Empty, (int)i.Graphic, priceEntry.Price);
            }).ToList();
        }

        public void SetBuyList(string listName)
        {
            var profile = _configService.CurrentProfile;
            if (profile != null && !string.IsNullOrEmpty(listName))
            {
                profile.ActiveVendorList = listName;
            }
            var config = GetActiveConfig();
            if (config != null)
            {
                config.BuyEnabled = true;
            }
        }

        public void SetSellList(string listName)
        {
            var profile = _configService.CurrentProfile;
            if (profile != null && !string.IsNullOrEmpty(listName))
            {
                profile.ActiveVendorList = listName;
            }
            var config = GetActiveConfig();
            if (config != null)
            {
                config.SellEnabled = true;
            }
        }

        public void ClearBuyList()
        {
            var config = GetActiveConfig();
            if (config != null)
            {
                config.BuyList.Clear();
            }
        }

        public void ClearSellList()
        {
            var config = GetActiveConfig();
            if (config != null)
            {
                config.SellList.Clear();
            }
        }

        protected override async System.Threading.Tasks.Task AgentLoopAsync(System.Threading.CancellationToken token)
        {
            await System.Threading.Tasks.Task.Delay(-1, token);
        }
    }
}
