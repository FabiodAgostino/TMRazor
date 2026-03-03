using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models.Config;
using System.Buffers.Binary;

namespace TMRazorImproved.Core.Services
{
    public class RestockService : AgentServiceBase, IRestockService
    {
        private readonly IPacketService _packetService;
        private readonly IConfigService _configService;
        private readonly IWorldService _worldService;
        private readonly ILogger<RestockService> _logger;

        public event Action? OnComplete;

        public RestockService(
            IPacketService packetService,
            IConfigService configService,
            IWorldService worldService,
            IHotkeyService hotkeyService,
            ILogger<RestockService> logger)
        {
            _packetService = packetService;
            _configService = configService;
            _worldService = worldService;
            _logger = logger;

            hotkeyService.RegisterAction("Restock: Start", () => Start());
            hotkeyService.RegisterAction("Restock: Stop", () => _ = StopAsync());
        }

        private RestockConfig? GetActiveConfig()
        {
            var profile = _configService.CurrentProfile;
            if (profile == null) return null;
            return profile.RestockLists.FirstOrDefault(l => l.Name == profile.ActiveRestockList) 
                   ?? profile.RestockLists.FirstOrDefault();
        }

        protected override async Task AgentLoopAsync(CancellationToken token)
        {
            _logger.LogInformation("Restock agent loop started");

            var config = GetActiveConfig();
            if (config == null || config.Source == 0)
            {
                _logger.LogWarning("Restock config not valid or source not set");
                await StopAsync();
                return;
            }

            uint destination = config.Destination;
            if (destination == 0 && _worldService.Player != null && _worldService.Player.Backpack != null)
                destination = _worldService.Player.Backpack.Serial;

            if (destination == 0)
            {
                _logger.LogWarning("Restock destination not set");
                await StopAsync();
                return;
            }

            // Recupera gli item dal container sorgente
            var sourceContainer = _worldService.FindItem(config.Source);
            if (sourceContainer == null)
            {
                _logger.LogWarning("Source container not found in world");
                await StopAsync();
                return;
            }

            foreach (var restockItem in config.ItemList)
            {
                if (token.IsCancellationRequested) break;

                // Calcola quanti ne abbiamo già nel backpack
                int currentAmount = _worldService.GetItemsInContainer(destination)
                    .Where(i => i.Graphic == restockItem.Graphic)
                    .Sum(i => i.Amount);

                int needed = restockItem.Amount - currentAmount;
                if (needed <= 0) continue;

                // Cerca l'item nel sorgente
                var foundItems = _worldService.GetItemsInContainer(config.Source)
                    .Where(i => i.Graphic == restockItem.Graphic)
                    .ToList();

                foreach (var item in foundItems)
                {
                    if (needed <= 0 || token.IsCancellationRequested) break;

                    int toMove = Math.Min(item.Amount, needed);
                    _logger.LogInformation("Restocking {Amount} of item 0x{Graphic:X} (needed: {Needed})", toMove, item.Graphic, needed);

                    MoveItem(item.Serial, (ushort)toMove, destination);
                    needed -= toMove;

                    await Task.Delay(Math.Max(100, config.Delay), token);
                }
            }

            _logger.LogInformation("Restock agent loop completed");
            OnComplete?.Invoke();
            await StopAsync();
        }

        private void MoveItem(uint serial, ushort amount, uint targetContainer)
        {
            // Lift
            byte[] lift = new byte[7];
            lift[0] = 0x07;
            BinaryPrimitives.WriteUInt32BigEndian(lift.AsSpan(1), serial);
            BinaryPrimitives.WriteUInt16BigEndian(lift.AsSpan(5), amount);
            _packetService.SendToServer(lift);

            // Drop
            byte[] drop = new byte[15];
            drop[0] = 0x08;
            BinaryPrimitives.WriteUInt32BigEndian(drop.AsSpan(1), serial);
            BinaryPrimitives.WriteUInt16BigEndian(drop.AsSpan(5), 0xFFFF);
            BinaryPrimitives.WriteUInt16BigEndian(drop.AsSpan(7), 0xFFFF);
            drop[9] = 0;
            BinaryPrimitives.WriteUInt32BigEndian(drop.AsSpan(11), targetContainer);
            _packetService.SendToServer(drop);
        }
    }
}
