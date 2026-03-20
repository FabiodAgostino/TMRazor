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

                // FR-044: include color filter in backpack check
                int currentAmount = _worldService.GetItemsInContainer(destination)
                    .Where(i => i.Graphic == restockItem.Graphic
                             && (restockItem.Color == -1 || i.Hue == restockItem.Color))
                    .Sum(i => i.Amount);

                int needed = restockItem.Amount - currentAmount;
                if (needed <= 0) continue;

                // FR-044: color matching — filter by Hue when Color != -1
                var foundItems = _worldService.GetItemsInContainer(config.Source)
                    .Where(i => i.Graphic == restockItem.Graphic
                             && (restockItem.Color == -1 || i.Hue == restockItem.Color))
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

        // FR-043: one-shot restock pass with explicit source/dest/delay
        public void RunOnce(string listName, uint sourceSerial, uint destSerial, int delayMs)
        {
            var profile = _configService.CurrentProfile;
            var config = profile?.RestockLists.FirstOrDefault(l => l.Name == listName);
            if (config == null)
            {
                _logger.LogWarning("Restock.RunOnce: list '{Name}' not found", listName);
                return;
            }

            uint src = sourceSerial != 0 ? sourceSerial : config.Source;
            uint dst = destSerial != 0 ? destSerial : config.Destination;
            if (dst == 0 && _worldService.Player?.Backpack != null)
                dst = _worldService.Player.Backpack.Serial;
            int delay = delayMs > 0 ? delayMs : Math.Max(100, config.Delay);

            _ = Task.Run(async () =>
            {
                foreach (var restockItem in config.ItemList)
                {
                    int currentAmount = _worldService.GetItemsInContainer(dst)
                        .Where(i => i.Graphic == restockItem.Graphic
                                 && (restockItem.Color == -1 || i.Hue == restockItem.Color))
                        .Sum(i => i.Amount);

                    int needed = restockItem.Amount - currentAmount;
                    if (needed <= 0) continue;

                    foreach (var item in _worldService.GetItemsInContainer(src)
                        .Where(i => i.Graphic == restockItem.Graphic
                                 && (restockItem.Color == -1 || i.Hue == restockItem.Color)))
                    {
                        if (needed <= 0) break;
                        int toMove = Math.Min(item.Amount, needed);
                        await MoveItemAsync(item.Serial, (ushort)toMove, dst);
                        needed -= toMove;
                        await Task.Delay(delay);
                    }
                }
                OnComplete?.Invoke();
            });
        }

        private async Task<bool> MoveItemAsync(uint serial, ushort amount, uint targetContainer)
        {
            return await _dragDropCoordinator.RequestDragDrop(serial, targetContainer, amount);
        }
    }
}
