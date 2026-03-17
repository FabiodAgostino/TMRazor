using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Messages;
using TMRazorImproved.Shared.Models;
using TMRazorImproved.UI.Utilities;

namespace TMRazorImproved.UI.ViewModels
{
    public partial class TargetHPViewModel : ViewModelBase, IRecipient<PlayerStatusMessage>, IDisposable
    {
        private readonly IWorldService _worldService;
        private readonly IConfigService _configService;
        private readonly UiThrottler _throttler;

        [ObservableProperty] private uint _targetSerial;
        [ObservableProperty] private string _targetName = "No Target";
        [ObservableProperty] private ushort _hits, _hitsMax;
        [ObservableProperty] private bool _isTargetActive;

        [ObservableProperty] private double _windowX = double.NaN;
        [ObservableProperty] private double _windowY = double.NaN;
        [ObservableProperty] private bool _topMost = true;
        [ObservableProperty] private double _opacity = 0.8;

        public double HitsPercent => HitsMax > 0 ? (double)Hits / HitsMax : 0;

        private volatile ushort _pendingHits, _pendingHitsMax;
        private int _dirty;

        public TargetHPViewModel(IMessenger messenger, IWorldService worldService, IConfigService configService)
        {
            _worldService = worldService;
            _configService = configService;
            _throttler = new UiThrottler(TimeSpan.FromMilliseconds(100), FlushUpdates);
            messenger.RegisterAll(this);

            LoadConfig();

            // Periodically check if target changed
            var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            timer.Tick += (s, e) => CheckTarget();
            timer.Start();
        }

        private void LoadConfig()
        {
            var config = _configService.CurrentProfile?.TargetHP;
            if (config != null)
            {
                WindowX = config.X;
                WindowY = config.Y;
                TopMost = config.TopMost;
                Opacity = config.Opacity;
            }
        }

        public void Save()
        {
            var profile = _configService.CurrentProfile;
            if (profile != null)
            {
                profile.TargetHP ??= new Shared.Models.Config.TargetHPConfig();
                profile.TargetHP.X = WindowX;
                profile.TargetHP.Y = WindowY;
                profile.TargetHP.TopMost = TopMost;
                profile.TargetHP.Opacity = Opacity;
                _configService.Save();
            }
        }

        private void CheckTarget()
        {
            var targetSerial = _worldService.Player?.AttackTarget ?? 0;
            if (targetSerial != TargetSerial)
            {
                TargetSerial = targetSerial;
                if (targetSerial == 0)
                {
                    IsTargetActive = false;
                    TargetName = "No Target";
                    Hits = 0; HitsMax = 0;
                }
                else
                {
                    var m = _worldService.FindMobile(targetSerial);
                    if (m != null)
                    {
                        IsTargetActive = true;
                        TargetName = m.Name;
                        Hits = m.Hits;
                        HitsMax = m.HitsMax;
                        OnPropertyChanged(nameof(HitsPercent));
                    }
                    else
                    {
                        IsTargetActive = false;
                    }
                }
            }
        }

        public void Receive(PlayerStatusMessage message)
        {
            var (stat, serial, current, max) = message.Value;
            if (serial != TargetSerial || stat != StatType.Hits) return;

            _pendingHits = current;
            _pendingHitsMax = max;
            System.Threading.Interlocked.Exchange(ref _dirty, 1);
        }

        private void FlushUpdates()
        {
            if (System.Threading.Interlocked.Exchange(ref _dirty, 0) != 0)
            {
                Hits = _pendingHits;
                HitsMax = _pendingHitsMax;
                OnPropertyChanged(nameof(HitsPercent));
            }
        }

        public void Dispose()
        {
            _throttler.Dispose();
            WeakReferenceMessenger.Default.UnregisterAll(this);
        }
    }
}
