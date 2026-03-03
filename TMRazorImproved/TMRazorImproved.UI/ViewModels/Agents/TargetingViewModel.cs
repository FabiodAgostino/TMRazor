using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;
using TMRazorImproved.Shared.Models.Config;
using System.Linq;

namespace TMRazorImproved.UI.ViewModels.Agents
{
    public sealed partial class TargetingViewModel : ViewModelBase
    {
        private readonly IConfigService _config;
        private readonly ITargetingService _targeting;
        private readonly IFriendsService _friends;
        private readonly IWorldService _world;
        private readonly object _lock = new();

        [ObservableProperty]
        private string _statusText = string.Empty;

        [ObservableProperty]
        private int _range = 12;

        [ObservableProperty]
        private bool _targetInnocents;

        [ObservableProperty]
        private bool _targetFriends;

        [ObservableProperty]
        private TargetPriority _priority;

        [ObservableProperty]
        private TargetFilter? _selectedFilter;

        public ObservableCollection<TargetFilter> TargetLists { get; } = new();
        public ObservableCollection<Mobile> FriendList { get; } = new();

        public IAsyncRelayCommand AddFriendCommand { get; }
        public IRelayCommand<Mobile> RemoveFriendCommand { get; }
        public IRelayCommand ClearTargetCommand { get; }
        
        public IRelayCommand AddFilterCommand { get; }
        public IRelayCommand RemoveFilterCommand { get; }
        public IRelayCommand AddBodyIDCommand { get; }
        public IRelayCommand AddHueCommand { get; }

        public TargetingViewModel(IConfigService config, ITargetingService targeting, IWorldService world, IFriendsService friends)
        {
            _config = config;
            _targeting = targeting;
            _world = world;
            _friends = friends;

            EnableThreadSafeCollection(TargetLists, _lock);
            EnableThreadSafeCollection(FriendList, _lock);

            AddFriendCommand = new AsyncRelayCommand(AddFriendAsync);
            RemoveFriendCommand = new RelayCommand<Mobile>(RemoveFriend);
            ClearTargetCommand = new RelayCommand(() => _targeting.Clear());
            
            AddFilterCommand = new RelayCommand(AddFilter);
            RemoveFilterCommand = new RelayCommand(RemoveFilter);
            AddBodyIDCommand = new RelayCommand(AddBodyID);
            AddHueCommand = new RelayCommand(AddHue);

            LoadConfig();
        }

        private void LoadConfig()
        {
            var cfg = _config.CurrentProfile?.Targeting;
            if (cfg != null)
            {
                Range = cfg.Range;
                TargetInnocents = cfg.TargetInnocents;
                TargetFriends = cfg.TargetFriends;
                Priority = cfg.Priority;
            }

            TargetLists.Clear();
            if (_config.CurrentProfile != null)
            {
                foreach (var filter in _config.CurrentProfile.TargetLists)
                    TargetLists.Add(filter);
                
                SelectedFilter = TargetLists.FirstOrDefault(f => f.Name == _config.CurrentProfile.ActiveTargetList) ?? TargetLists.FirstOrDefault();
            }

            RefreshFriendList();
        }

        private void RefreshFriendList()
        {
            FriendList.Clear();
            foreach (var friend in _friends.ActiveList.Players)
            {
                var m = _world.FindMobile(friend.Serial) ?? new Mobile(friend.Serial) { Name = friend.Name };
                FriendList.Add(m);
            }
        }

        private void AddFilter()
        {
            var newFilter = new TargetFilter { Name = $"Filter {TargetLists.Count + 1}" };
            _config.CurrentProfile?.TargetLists.Add(newFilter);
            TargetLists.Add(newFilter);
            SelectedFilter = newFilter;
            _config.Save();
        }

        private void RemoveFilter()
        {
            if (SelectedFilter != null && TargetLists.Count > 1)
            {
                _config.CurrentProfile?.TargetLists.Remove(SelectedFilter);
                TargetLists.Remove(SelectedFilter);
                SelectedFilter = TargetLists.FirstOrDefault();
                _config.Save();
            }
        }

        private void AddBodyID()
        {
            if (SelectedFilter != null)
            {
                SelectedFilter.BodyIDs.Add(0x0190); // Default human male
                OnPropertyChanged(nameof(SelectedFilter));
                _config.Save();
            }
        }

        private void AddHue()
        {
            if (SelectedFilter != null)
            {
                SelectedFilter.Hues.Add(0);
                OnPropertyChanged(nameof(SelectedFilter));
                _config.Save();
            }
        }

        private async Task AddFriendAsync()
        {
            StatusText = "Seleziona un giocatore da aggiungere agli amici...";
            var serial = await _targeting.AcquireTargetAsync();
            if (serial != 0)
            {
                if (_friends.IsFriend(serial))
                {
                    StatusText = "Giocatore già presente nella lista amici.";
                    return;
                }

                var m = _world.FindMobile(serial);
                string name = m?.Name ?? $"Friend_{serial:X8}";
                _friends.AddFriend(serial, name);
                RefreshFriendList();
                StatusText = $"Amico aggiunto: {name}";
            }
        }

        private void RemoveFriend(Mobile? m)
        {
            if (m != null)
            {
                _friends.RemoveFriend(m.Serial);
                RefreshFriendList();
                StatusText = $"Amico rimosso: {m.Name}";
            }
        }

        partial void OnSelectedFilterChanged(TargetFilter? value)
        {
            if (value != null && _config.CurrentProfile != null)
            {
                _config.CurrentProfile.ActiveTargetList = value.Name;
                _config.Save();
            }
        }

        partial void OnRangeChanged(int value) { if (_config.CurrentProfile?.Targeting != null) { _config.CurrentProfile.Targeting.Range = value; _config.Save(); } }
        partial void OnTargetInnocentsChanged(bool value) { if (_config.CurrentProfile?.Targeting != null) { _config.CurrentProfile.Targeting.TargetInnocents = value; _config.Save(); } }
        partial void OnTargetFriendsChanged(bool value) { if (_config.CurrentProfile?.Targeting != null) { _config.CurrentProfile.Targeting.TargetFriends = value; _config.Save(); } }
        partial void OnPriorityChanged(TargetPriority value) { if (_config.CurrentProfile?.Targeting != null) { _config.CurrentProfile.Targeting.Priority = value; _config.Save(); } }
    }
}
