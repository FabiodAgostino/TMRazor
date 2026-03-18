using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models.Config;

namespace TMRazorImproved.UI.ViewModels
{
    public partial class FriendsViewModel : ViewModelBase
    {
        private readonly IFriendsService _friendsService;
        private readonly ITargetingService _targetingService;
        private readonly IConfigService _config;
        
        private readonly object _friendsLock = new();
        private readonly object _guildsLock = new();
        private readonly object _listsLock = new();

        [ObservableProperty]
        private string _newListName = string.Empty;

        [ObservableProperty]
        private string _newGuildName = string.Empty;

        [ObservableProperty]
        private string _newPlayerName = string.Empty;

        [ObservableProperty]
        private string _newPlayerSerial = string.Empty;

        public ObservableCollection<string> FriendsLists { get; } = new();
        
        [ObservableProperty]
        private string? _selectedList;

        public FriendsConfig ActiveConfig => _friendsService.ActiveList;

        public ObservableCollection<FriendPlayer> Players { get; } = new();
        public ObservableCollection<FriendGuild> Guilds { get; } = new();

        public FriendsViewModel(IFriendsService friendsService, ITargetingService targetingService, IConfigService config)
        {
            _friendsService = friendsService;
            _targetingService = targetingService;
            _config = config;

            EnableThreadSafeCollection(FriendsLists, _listsLock);
            EnableThreadSafeCollection(Players, _friendsLock);
            EnableThreadSafeCollection(Guilds, _guildsLock);

            RefreshLists();
            RefreshActiveList();
        }

        private void RefreshLists()
        {
            SyncCollection(FriendsLists, _config.CurrentProfile.FriendsLists.Select(l => l.Name), _listsLock);
            SelectedList = _config.CurrentProfile.ActiveFriendsList;
        }

        private void RefreshActiveList()
        {
            SyncCollection(Players, ActiveConfig.Players, _friendsLock);
            SyncCollection(Guilds, ActiveConfig.Guilds, _guildsLock);
        }

        partial void OnSelectedListChanged(string? value)
        {
            if (value != null && value != _config.CurrentProfile.ActiveFriendsList)
            {
                _friendsService.SwitchList(value);
                OnPropertyChanged(nameof(ActiveConfig));
                RefreshActiveList();
            }
        }

        [RelayCommand]
        private void CreateList()
        {
            if (!string.IsNullOrWhiteSpace(NewListName))
            {
                _friendsService.CreateList(NewListName);
                RefreshLists();
                SelectedList = NewListName;
                NewListName = string.Empty;
            }
        }

        [RelayCommand]
        private void DeleteList()
        {
            if (SelectedList != null)
            {
                _friendsService.DeleteList(SelectedList);
                RefreshLists();
            }
        }

        [RelayCommand]
        private void AddFriendManual()
        {
            if (string.IsNullOrWhiteSpace(NewPlayerName)) return;
            uint serial = 0;
            var s = NewPlayerSerial.Trim();
            if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                uint.TryParse(s[2..], System.Globalization.NumberStyles.HexNumber, null, out serial);
            else
                uint.TryParse(s, out serial);
            if (serial == 0) return;
            _friendsService.AddFriend(serial, NewPlayerName.Trim());
            RefreshActiveList();
            NewPlayerName = string.Empty;
            NewPlayerSerial = string.Empty;
        }

        [RelayCommand]
        private async Task AddFriendTarget()
        {
            var targetInfo = await _targetingService.AcquireTargetAsync(); uint serial = targetInfo.Serial;
            if (serial != 0 && serial != 0xFFFFFFFF)
            {
                _friendsService.AddFriend(serial, $"Friend_{serial:X}");
                RefreshActiveList();
            }
        }

        [RelayCommand]
        private void RemoveFriend(FriendPlayer player)
        {
            if (player != null)
            {
                _friendsService.RemoveFriend(player.Serial);
                RefreshActiveList();
            }
        }

        [RelayCommand]
        private void AddGuild()
        {
            if (!string.IsNullOrWhiteSpace(NewGuildName))
            {
                _friendsService.AddGuild(NewGuildName);
                RefreshActiveList();
                NewGuildName = string.Empty;
            }
        }

        [RelayCommand]
        private void RemoveGuild(FriendGuild guild)
        {
            if (guild != null)
            {
                _friendsService.RemoveGuild(guild.Name);
                RefreshActiveList();
            }
        }

        // Options toggles
        public bool AutoAcceptParty { get => ActiveConfig.AutoAcceptParty; set { ActiveConfig.AutoAcceptParty = value; _config.Save(); } }
        public bool PreventAttack { get => ActiveConfig.PreventAttack; set { ActiveConfig.PreventAttack = value; _config.Save(); } }
        public bool IncludeParty { get => ActiveConfig.IncludeParty; set { ActiveConfig.IncludeParty = value; _config.Save(); } }
        public bool SLFriend { get => ActiveConfig.SLFriend; set { ActiveConfig.SLFriend = value; _config.Save(); } }
        public bool TBFriend { get => ActiveConfig.TBFriend; set { ActiveConfig.TBFriend = value; _config.Save(); } }
        public bool COMFriend { get => ActiveConfig.COMFriend; set { ActiveConfig.COMFriend = value; _config.Save(); } }
        public bool MINFriend { get => ActiveConfig.MINFriend; set { ActiveConfig.MINFriend = value; _config.Save(); } }
    }
}
