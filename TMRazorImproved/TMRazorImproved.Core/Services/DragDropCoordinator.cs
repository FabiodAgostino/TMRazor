using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Core.Utilities;

namespace TMRazorImproved.Core.Services
{
    public class DragDropCoordinator : IDragDropCoordinator
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly IPacketService _packetService;
        private readonly ILogger<DragDropCoordinator> _logger;

        public DragDropCoordinator(IPacketService packetService, ILogger<DragDropCoordinator> logger)
        {
            _packetService = packetService;
            _logger = logger;
        }

        public async Task<bool> RequestDragDrop(uint serial, uint destination, ushort amount = 1, int timeoutMs = 2000)
        {
            if (serial == 0) return false;

            if (!await _semaphore.WaitAsync(timeoutMs))
            {
                _logger.LogWarning("Timeout waiting for drag-drop lock for item {Serial}", serial);
                return false;
            }

            try
            {
                _packetService.SendToServer(PacketBuilder.LiftItem(serial, amount));
                // Delay between lift and drop to ensure server processing
                await Task.Delay(50);
                _packetService.SendToServer(PacketBuilder.DropToContainer(serial, destination));
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during drag-drop of item {Serial}", serial);
                return false;
            }
            finally
            {
                // Un po' di debounce per evitare che le code si pestino i piedi
                await Task.Delay(150);
                _semaphore.Release();
            }
        }
    }
}
