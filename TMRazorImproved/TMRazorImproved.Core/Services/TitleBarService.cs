using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.Core.Services
{
    public class TitleBarService : ITitleBarService, IDisposable
    {
        private readonly IClientInteropService _interop;
        private readonly IWorldService _worldService;
        private readonly ILogger<TitleBarService> _logger;
        
        private CancellationTokenSource? _cts;
        private Task? _updateTask;

        public bool IsEnabled { get; set; } = true;
        public string Template { get; set; } = "UO - {char} [HP: {hp}/{hpmax}] [MP: {mp}/{mpmax}] [SP: {sp}/{spmax}]";

        public TitleBarService(
            IClientInteropService interop,
            IWorldService worldService,
            ILogger<TitleBarService> logger)
        {
            _interop = interop;
            _worldService = worldService;
            _logger = logger;
        }

        public void Start()
        {
            if (_updateTask != null && !_updateTask.IsCompleted) return;

            _cts = new CancellationTokenSource();
            _updateTask = Task.Run(() => UpdateLoopAsync(_cts.Token), _cts.Token);
            _logger.LogInformation("TitleBar update service started");
        }

        public async Task StopAsync()
        {
            if (_cts == null) return;
            _cts.Cancel();
            try
            {
                if (_updateTask != null) await _updateTask;
            }
            catch (OperationCanceledException) { }
            finally
            {
                _cts.Dispose();
                _cts = null;
                _updateTask = null;
                _logger.LogInformation("TitleBar update service stopped");
            }
        }

        private async Task UpdateLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (IsEnabled)
                {
                    UpdateTitle();
                }
                await Task.Delay(500, token);
            }
        }

        private void UpdateTitle()
        {
            IntPtr hwnd = _interop.FindUOWindow();
            if (hwnd == IntPtr.Zero) return;

            var player = _worldService.Player;
            if (player == null) return;

            string title = Template
                .Replace("{char}", player.Name ?? "Unknown")
                .Replace("{hp}", player.Hits.ToString())
                .Replace("{hpmax}", player.HitsMax.ToString())
                .Replace("{mp}", player.Mana.ToString())
                .Replace("{mpmax}", player.ManaMax.ToString())
                .Replace("{sp}", player.Stam.ToString())
                .Replace("{spmax}", player.StamMax.ToString());

            _interop.SetWindowText(hwnd, title);
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}
