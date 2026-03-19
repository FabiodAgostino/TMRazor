using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Core.Utilities;

namespace TMRazorImproved.Core.Services
{
    public class DragDropCoordinator : IDragDropCoordinator, IDisposable
    {
        private readonly IPacketService _packetService;
        private readonly IWorldService _worldService;
        private readonly IConfigService _configService;
        private readonly ILogger<DragDropCoordinator> _logger;

        private readonly Channel<DragDropRequest> _queue =
            Channel.CreateUnbounded<DragDropRequest>(new UnboundedChannelOptions { SingleReader = true });

        private readonly CancellationTokenSource _cts = new();
        private readonly Task _processorTask;

        public DragDropCoordinator(
            IPacketService packetService,
            IWorldService worldService,
            IConfigService configService,
            ILogger<DragDropCoordinator> logger)
        {
            _packetService = packetService;
            _worldService = worldService;
            _configService = configService;
            _logger = logger;
            _processorTask = Task.Run(ProcessQueueAsync);
        }

        public async Task<bool> RequestDragDrop(uint serial, uint destination, ushort amount = 1, int timeoutMs = 2000)
        {
            if (serial == 0) return false;

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var request = new DragDropRequest(serial, destination, amount, tcs);

            await _queue.Writer.WriteAsync(request, _cts.Token);

            using var timeoutCts = new CancellationTokenSource(timeoutMs);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, _cts.Token);

            try
            {
                return await tcs.Task.WaitAsync(linkedCts.Token);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Timeout or cancellation waiting for drag-drop result for item {Serial:X8}", serial);
                tcs.TrySetResult(false);
                return false;
            }
        }

        private async Task ProcessQueueAsync()
        {
            await foreach (var req in _queue.Reader.ReadAllAsync(_cts.Token))
            {
                if (_cts.IsCancellationRequested) break;

                try
                {
                    var result = await ExecuteDragDrop(req);
                    req.Result.TrySetResult(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing drag-drop for item {Serial:X8}", req.Serial);
                    req.Result.TrySetResult(false);
                }
            }
        }

        private async Task<bool> ExecuteDragDrop(DragDropRequest req)
        {
            var profile = _configService.CurrentProfile;
            var player = _worldService.Player;
            var item = _worldService.FindItem(req.Serial);

            // 041-B) Weight check
            if (player != null && item != null && player.MaxWeight > 0)
            {
                if (player.Weight + item.Weight > player.MaxWeight - 5)
                {
                    _logger.LogWarning("Drag-drop skipped for {Serial:X8}: overweight (cur={W}, max={MW})",
                        req.Serial, player.Weight + item.Weight, player.MaxWeight);
                    return false;
                }
            }

            // 041-C) Z-Level validation
            if (player != null && item != null)
            {
                if (Math.Abs(player.Z - item.Z) > 8)
                {
                    _logger.LogWarning("Drag-drop skipped for {Serial:X8}: Z-level diff too large (player={PZ}, item={IZ})",
                        req.Serial, player.Z, item.Z);
                    return false;
                }
            }

            // 041-D) Configurable delays
            int liftDelay = profile.DragDropLiftDelayMs;
            int dropDelay = profile.DragDropDropDelayMs;

            _packetService.SendToServer(PacketBuilder.LiftItem(req.Serial, req.Amount));
            await Task.Delay(liftDelay);
            _packetService.SendToServer(PacketBuilder.DropToContainer(req.Serial, req.Destination));
            await Task.Delay(dropDelay);

            return true;
        }

        public void Dispose()
        {
            _cts.Cancel();
            _queue.Writer.TryComplete();
            try { _processorTask.Wait(1000); } catch { /* ignore */ }
            _cts.Dispose();
        }

        private readonly record struct DragDropRequest(
            uint Serial,
            uint Destination,
            ushort Amount,
            TaskCompletionSource<bool> Result);
    }
}
