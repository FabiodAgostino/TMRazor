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
        private readonly IConfigService _config;
        private readonly IClientInteropService _interop;
        private readonly IWorldService _worldService;
        private readonly ILogger<TitleBarService> _logger;
        
        private CancellationTokenSource? _cts;
        private Task? _updateTask;

        public bool IsEnabled 
        { 
            get => _config.CurrentProfile.TitleBarEnabled; 
            set { _config.CurrentProfile.TitleBarEnabled = value; _config.Save(); } 
        }

        public string Template 
        { 
            get => _config.CurrentProfile.TitleBarTemplate; 
            set { _config.CurrentProfile.TitleBarTemplate = value; _config.Save(); } 
        }

        public event Action<string>? TitleChanged;

        public TitleBarService(
            IConfigService config,
            IClientInteropService interop,
            IWorldService worldService,
            ILogger<TitleBarService> logger)
        {
            _config = config;
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
            var player = _worldService.Player;
            string charName = player?.Name ?? "Unknown";
            string hp = player?.Hits.ToString() ?? "0";
            string hpmax = player?.HitsMax.ToString() ?? "0";
            string mp = player?.Mana.ToString() ?? "0";
            string mpmax = player?.ManaMax.ToString() ?? "0";
            string sp = player?.Stam.ToString() ?? "0";
            string spmax = player?.StamMax.ToString() ?? "0";
            string ping = _worldService.CurrentPing.ToString("F0");
            string pingmin = _worldService.MinPing == double.MaxValue ? "0" : _worldService.MinPing.ToString("F0");
            string pingmax = _worldService.MaxPing.ToString("F0");
            string pingavg = _worldService.AvgPing.ToString("F0");

            string title = Template
                .Replace("{char}", charName)
                .Replace("{hp}", hp)
                .Replace("{hpmax}", hpmax)
                .Replace("{mp}", mp)
                .Replace("{mpmax}", mpmax)
                .Replace("{sp}", sp)
                .Replace("{spmax}", spmax)
                .Replace("{ping}", ping)
                .Replace("{pingmin}", pingmin)
                .Replace("{pingmax}", pingmax)
                .Replace("{pingavg}", pingavg);

            // Aggiorna finestra UO
            IntPtr hwnd = _interop.FindUOWindow();
            if (hwnd != IntPtr.Zero)
            {
                _interop.SetWindowText(hwnd, title);
            }

            // Notifica UI
            TitleChanged?.Invoke(title);
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}
