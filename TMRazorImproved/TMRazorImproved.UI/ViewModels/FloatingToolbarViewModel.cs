using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Messages;
using TMRazorImproved.UI.Utilities;

namespace TMRazorImproved.UI.ViewModels
{
    public partial class FloatingToolbarViewModel : ViewModelBase, IRecipient<PlayerStatusMessage>, IDisposable
    {
        private readonly IWorldService _worldService;
        private readonly IConfigService _config;
        private readonly ICounterService _counterService;
        private readonly UiThrottler _throttler;

        // Pending values for stats
        private volatile ushort _pendingHits, _pendingHitsMax;
        private volatile ushort _pendingMana, _pendingManaMax;
        private volatile ushort _pendingStam, _pendingStamMax;
        private volatile ushort _pendingWeight, _pendingMaxWeight;
        private volatile ushort _pendingFollowers, _pendingFollowersMax;
        private volatile int _pendingTithe;

        // Dirty flags
        private int _hitsDirty, _manaDirty, _stamDirty, _weightDirty, _followersDirty, _titheDirty;

        [ObservableProperty] private ushort _hits, _hitsMax;
        [ObservableProperty] private ushort _mana, _manaMax;
        [ObservableProperty] private ushort _stam, _stamMax;
        [ObservableProperty] private ushort _weight, _maxWeight;
        [ObservableProperty] private ushort _followers, _followersMax;
        [ObservableProperty] private int _tithe;

        public double HitsPercent => HitsMax > 0 ? (double)Hits / HitsMax : 0;
        public double ManaPercent => ManaMax > 0 ? (double)Mana / ManaMax : 0;
        public double StamPercent => StamMax > 0 ? (double)Stam / StamMax : 0;

        public ObservableCollection<CounterStatus> ActiveCounters { get; } = new();

        public FloatingToolbarViewModel(IMessenger messenger, IWorldService worldService, IConfigService config, ICounterService counterService)
        {
            _worldService = worldService;
            _config = config;
            _counterService = counterService;
            _throttler = new UiThrottler(TimeSpan.FromMilliseconds(100), FlushPendingUpdates);
            messenger.RegisterAll(this);
            
            _counterService.CounterChanged += OnCounterChanged;

            // Initial values
            if (_worldService.Player != null)
            {
                Hits = _worldService.Player.Hits;
                HitsMax = _worldService.Player.HitsMax;
                Mana = _worldService.Player.Mana;
                ManaMax = _worldService.Player.ManaMax;
                Stam = _worldService.Player.Stam;
                StamMax = _worldService.Player.StamMax;
                Weight = _worldService.Player.Weight;
                MaxWeight = _worldService.Player.MaxWeight;
                Followers = _worldService.Player.Followers;
                FollowersMax = _worldService.Player.FollowersMax;
                Tithe = _worldService.Player.Tithe;
            }

            InitializeCounters();
        }

        private void InitializeCounters()
        {
            ActiveCounters.Clear();
            foreach (var def in _config.CurrentProfile.Counters.Where(c => c.Enabled))
            {
                ActiveCounters.Add(new CounterStatus 
                { 
                    Abbreviation = def.Abbreviation, 
                    Graphic = def.Graphic, 
                    Hue = def.Hue,
                    Count = _counterService.GetCount(def.Graphic, def.Hue)
                });
            }
        }

        private void OnCounterChanged(ushort graphic, ushort hue, int count)
        {
            var status = ActiveCounters.FirstOrDefault(c => c.Graphic == graphic && c.Hue == hue);
            if (status != null)
            {
                // UI update via Dispatcher if needed, but Messenger usually handles thread safety or UiThrottler
                App.Current.Dispatcher.Invoke(() => status.Count = count);
            }
        }

        public void Receive(PlayerStatusMessage message)
        {
            var (stat, serial, current, max) = message.Value;
            if (_worldService.Player?.Serial != serial) return;

            switch (stat)
            {
                case StatType.Hits:
                    _pendingHits = current; _pendingHitsMax = max;
                    System.Threading.Interlocked.Exchange(ref _hitsDirty, 1);
                    break;
                case StatType.Mana:
                    _pendingMana = current; _pendingManaMax = max;
                    System.Threading.Interlocked.Exchange(ref _manaDirty, 1);
                    break;
                case StatType.Stamina:
                    _pendingStam = current; _pendingStamMax = max;
                    System.Threading.Interlocked.Exchange(ref _stamDirty, 1);
                    break;
                case StatType.Weight:
                    _pendingWeight = current; _pendingMaxWeight = max;
                    System.Threading.Interlocked.Exchange(ref _weightDirty, 1);
                    break;
                case StatType.Followers:
                    _pendingFollowers = current; _pendingFollowersMax = max;
                    System.Threading.Interlocked.Exchange(ref _followersDirty, 1);
                    break;
                case StatType.Tithe:
                    _pendingTithe = current;
                    System.Threading.Interlocked.Exchange(ref _titheDirty, 1);
                    break;
            }
        }

        private void FlushPendingUpdates()
        {
            if (System.Threading.Interlocked.Exchange(ref _hitsDirty, 0) != 0)
            {
                Hits = _pendingHits; HitsMax = _pendingHitsMax;
                OnPropertyChanged(nameof(HitsPercent));
            }
            if (System.Threading.Interlocked.Exchange(ref _manaDirty, 0) != 0)
            {
                Mana = _pendingMana; ManaMax = _pendingManaMax;
                OnPropertyChanged(nameof(ManaPercent));
            }
            if (System.Threading.Interlocked.Exchange(ref _stamDirty, 0) != 0)
            {
                Stam = _pendingStam; StamMax = _pendingStamMax;
                OnPropertyChanged(nameof(StamPercent));
            }
            if (System.Threading.Interlocked.Exchange(ref _weightDirty, 0) != 0)
            {
                Weight = _pendingWeight; MaxWeight = _pendingMaxWeight;
            }
            if (System.Threading.Interlocked.Exchange(ref _followersDirty, 0) != 0)
            {
                Followers = _pendingFollowers; FollowersMax = _pendingFollowersMax;
            }
            if (System.Threading.Interlocked.Exchange(ref _titheDirty, 0) != 0)
            {
                Tithe = _pendingTithe;
            }
        }

        public void Dispose()
        {
            _throttler.Dispose();
            _counterService.CounterChanged -= OnCounterChanged;
            WeakReferenceMessenger.Default.UnregisterAll(this);
        }
    }
}
