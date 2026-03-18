using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models.Config;
using System.Buffers.Binary;
using TMRazorImproved.Core.Utilities;

namespace TMRazorImproved.Core.Services
{
    public class RestockService : AgentServiceBase, IRestockService
    {
        private readonly IPacketService _packetService;
        private readonly IWorldService _worldService;
        private readonly IDragDropCoordinator _dragDropCoordinator;
        private readonly ILogger<RestockService> _logger;

        public event Action? OnComplete;

        public RestockService(
            IPacketService packetService,
            IConfigService configService,
            IWorldService worldService,
            IDragDropCoordinator dragDropCoordinator,
            IHotkeyService hotkeyService,
            ILogger<RestockService> logger) : base(configService)
        {
            _packetService = packetService;
            _worldService = worldService;
            _dragDropCoordinator = dragDropCoordinator;
            _logger = logger;

            hotkeyService.RegisterAction("Restock: Start", () => Start());
            hotkeyService.RegisterAction("Restock: Stop", () => _ = StopAsync());
        }

        public void ChangeList(string listName)
        {
            var profile = _configService.CurrentProfile;
            if (profile != null)
            {
                profile.ActiveRestockList = listName;
                _logger.LogInformation("Restock list changed to: {ListName}", listName);
            }
        }

        private RestockConfig? GetActiveConfig()
        {
            return GetActiveConfig(p => p.RestockLists, p => p.ActiveRestockList);
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

                    await MoveItemAsync(item.Serial, (ushort)toMove, destination);
                    needed -= toMove;

                    await Task.Delay(Math.Max(100, config.Delay), token);
                }
            }

            _logger.LogInformation("Restock agent loop completed");
            OnComplete?.Invoke();
            await StopAsync();
        }

        private async Task<bool> MoveItemAsync(uint serial, ushort amount, uint targetContainer)
        {
            return await _dragDropCoordinator.RequestDragDrop(serial, targetContainer, amount);
        }
    }
}
