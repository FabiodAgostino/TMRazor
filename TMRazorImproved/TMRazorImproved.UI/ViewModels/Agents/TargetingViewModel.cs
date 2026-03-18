using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;
using TMRazorImproved.Shared.Models.Config;
using System.Linq;
using System;

namespace TMRazorImproved.UI.ViewModels.Agents
{
    public sealed partial class TargetingViewModel : ViewModelBase
    {
        private readonly IConfigService _config;
        private readonly ITargetingService _targeting;
        private readonly IFriendsService _friends;
        private readonly ITargetFilterService _targetFilterService;
        private readonly IWorldService _world;
        private readonly ILanguageService _lang;
        private readonly object _lock = new();

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
        public ObservableCollection<TargetFilterEntry> ExcludedTargets { get; } = new();

        public IAsyncRelayCommand AddFriendCommand { get; }
        public IRelayCommand<Mobile> RemoveFriendCommand { get; }
        public IRelayCommand ClearTargetCommand { get; }
        
        public IRelayCommand AddFilterCommand { get; }
        public IRelayCommand RemoveFilterCommand { get; }
        public IRelayCommand AddBodyIDCommand { get; }
        public IRelayCommand AddHueCommand { get; }

        public IAsyncRelayCommand AddExcludedTargetCommand { get; }
        public IRelayCommand<TargetFilterEntry> RemoveExcludedTargetCommand { get; }
        public IRelayCommand AddAllMobilesCommand { get; }
        public IRelayCommand AddAllHumanoidsCommand { get; }
        public IRelayCommand ClearExcludedTargetsCommand { get; }

        public TargetingViewModel(IConfigService config, ITargetingService targeting, IWorldService world, IFriendsService friends, ITargetFilterService targetFilterService, ILanguageService languageService)
        {
            _config = config;
            _targeting = targeting;
            _world = world;
            _friends = friends;
            _targetFilterService = targetFilterService;
            _lang = languageService;

            EnableThreadSafeCollection(TargetLists, _lock);
            EnableThreadSafeCollection(FriendList, _lock);
            EnableThreadSafeCollection(ExcludedTargets, _lock);

            AddFriendCommand = new AsyncRelayCommand(AddFriendAsync);
            RemoveFriendCommand = new RelayCommand<Mobile>(RemoveFriend);
            ClearTargetCommand = new RelayCommand(() => _targeting.Clear());
            
            AddFilterCommand = new RelayCommand(AddFilter);
            RemoveFilterCommand = new RelayCommand(RemoveFilter);
            AddBodyIDCommand = new RelayCommand(AddBodyID);
            AddHueCommand = new RelayCommand(AddHue);

            AddExcludedTargetCommand = new AsyncRelayCommand(AddExcludedTargetAsync);
            RemoveExcludedTargetCommand = new RelayCommand<TargetFilterEntry>(RemoveExcludedTarget);
            AddAllMobilesCommand = new RelayCommand(() => { _targetFilterService.AddAllMobiles(); RefreshExcludedTargets(); });
            AddAllHumanoidsCommand = new RelayCommand(() => { _targetFilterService.AddAllHumanoids(); RefreshExcludedTargets(); });
            ClearExcludedTargetsCommand = new RelayCommand(() => { _targetFilterService.ClearAll(); RefreshExcludedTargets(); });

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
            RefreshExcludedTargets();
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

        private void RefreshExcludedTargets()
        {
            ExcludedTargets.Clear();
            foreach (var filter in _targetFilterService.Filters)
            {
                ExcludedTargets.Add(filter);
            }
        }

        private async Task AddExcludedTargetAsync()
        {
            StatusText = _lang.GetString("Agents.General.SelectItem");
            var targetInfo = await _targeting.AcquireTargetAsync();
            if (targetInfo.Serial != 0)
            {
                var m = _world.FindMobile(targetInfo.Serial);
                string name = m?.Name ?? $"Mobile_{targetInfo.Serial:X8}";
                _targetFilterService.AddFilter(targetInfo.Serial, name);
                RefreshExcludedTargets();
                StatusText = $"Filtered target: {name}";
            }
        }

        private void RemoveExcludedTarget(TargetFilterEntry? entry)
        {
            if (entry != null)
            {
                _targetFilterService.RemoveFilter(entry.Serial);
                RefreshExcludedTargets();
            }
        }

        private void AddFilter()
        {
            var newFilter = new TargetFilter { Name = $"{_lang.GetString("Agents.General.NewList")} {TargetLists.Count + 1}" };
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
            StatusText = _lang.GetString("Agents.General.SelectItem");
            var targetInfo = await _targeting.AcquireTargetAsync(); var serial = targetInfo.Serial;
            if (serial != 0)
            {
                if (_friends.IsFriend(serial))
                {
                    StatusText = "Player already in friends list.";
                    return;
                }

                var m = _world.FindMobile(serial);
                string name = m?.Name ?? $"Friend_{serial:X8}";
                _friends.AddFriend(serial, name);
                RefreshFriendList();
                StatusText = $"Added friend: {name}";
            }
        }

        private void RemoveFriend(Mobile? m)
        {
            if (m != null)
            {
                _friends.RemoveFriend(m.Serial);
                RefreshFriendList();
                StatusText = $"Removed friend: {m.Name}";
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
