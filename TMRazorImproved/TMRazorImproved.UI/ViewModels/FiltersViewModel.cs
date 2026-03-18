using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Linq;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models.Config;

namespace TMRazorImproved.UI.ViewModels
{
    public partial class FiltersViewModel : ViewModelBase
    {
        private readonly IConfigService _config;
        private readonly ITargetingService _targeting;
        private readonly IMessenger _messenger;
        private readonly object _graphLock = new();

        public UserProfile Profile => _config.CurrentProfile;

        public ObservableCollection<GraphChangeData> GraphFilters { get; } = new();

        public FiltersViewModel(IConfigService config, ITargetingService targeting, IMessenger messenger)
        {
            _config = config;
            _targeting = targeting;
            _messenger = messenger;
            
            EnableThreadSafeCollection(GraphFilters, _graphLock);
            SyncCollection(GraphFilters, Profile.GraphFilters, _graphLock);
        }

        // Classic Filters Properties
        public bool FilterLight { get => Profile.FilterLight; set { Profile.FilterLight = value; _config.Save(); OnPropertyChanged(); } }
        public bool FilterWeather { get => Profile.FilterWeather; set { Profile.FilterWeather = value; _config.Save(); OnPropertyChanged(); } }
        public bool FilterSound { get => Profile.FilterSound; set { Profile.FilterSound = value; _config.Save(); OnPropertyChanged(); } }
        public bool FilterDeath { get => Profile.FilterDeath; set { Profile.FilterDeath = value; _config.Save(); OnPropertyChanged(); } }
        public bool FilterStaffItems 
        { 
            get => Profile.FilterStaffItems; 
            set 
            { 
                Profile.FilterStaffItems = value; 
                _config.Save(); 
                OnPropertyChanged(); 
                _messenger.Send(new Shared.Messages.ConfigChangedMessage(nameof(UserProfile.FilterStaffItems)));
            } 
        }
        public bool FilterStaffNpcs 
        { 
            get => Profile.FilterStaffNpcs; 
            set 
            { 
                Profile.FilterStaffNpcs = value; 
                _config.Save(); 
                OnPropertyChanged(); 
                _messenger.Send(new Shared.Messages.ConfigChangedMessage(nameof(UserProfile.FilterStaffNpcs)));
            } 
        }
        public bool FilterPoison { get => Profile.FilterPoison; set { Profile.FilterPoison = value; _config.Save(); OnPropertyChanged(); } }
        public bool FilterSnoop { get => Profile.FilterSnoop; set { Profile.FilterSnoop = value; _config.Save(); OnPropertyChanged(); } }
        public bool FilterBardMusic { get => Profile.FilterBardMusic; set { Profile.FilterBardMusic = value; _config.Save(); OnPropertyChanged(); } }
        public bool FilterFootsteps { get => Profile.FilterFootsteps; set { Profile.FilterFootsteps = value; _config.Save(); OnPropertyChanged(); } }
        public bool FilterKarmaFame { get => Profile.FilterKarmaFame; set { Profile.FilterKarmaFame = value; _config.Save(); OnPropertyChanged(); } }
        public bool FilterSeason { get => Profile.FilterSeason; set { Profile.FilterSeason = value; _config.Save(); OnPropertyChanged(); } }
        public bool FilterDragon { get => Profile.FilterDragon; set { Profile.FilterDragon = value; _config.Save(); OnPropertyChanged(); } }
        public bool FilterDrake { get => Profile.FilterDrake; set { Profile.FilterDrake = value; _config.Save(); OnPropertyChanged(); } }
        public bool FilterDaemon { get => Profile.FilterDaemon; set { Profile.FilterDaemon = value; _config.Save(); OnPropertyChanged(); } }
        public bool FilterVetRewardGump { get => Profile.FilterVetRewardGump; set { Profile.FilterVetRewardGump = value; _config.Save(); OnPropertyChanged(); } }

        // Advanced Filters Properties
        public bool HighlightFlags { get => Profile.HighlightFlags; set { Profile.HighlightFlags = value; _config.Save(); OnPropertyChanged(); } }
        public bool ColorizeFlags { get => Profile.ColorizeFlags; set { Profile.ColorizeFlags = value; _config.Save(); OnPropertyChanged(); } }
        public bool ColorizeSelfFlags { get => Profile.ColorizeSelfFlags; set { Profile.ColorizeSelfFlags = value; _config.Save(); OnPropertyChanged(); } }
        public bool StaticFields { get => Profile.StaticFields; set { Profile.StaticFields = value; _config.Save(); OnPropertyChanged(); } }
        public bool BlockTradeRequest { get => Profile.BlockTradeRequest; set { Profile.BlockTradeRequest = value; _config.Save(); OnPropertyChanged(); } }
        public bool BlockPartyInvite { get => Profile.BlockPartyInvite; set { Profile.BlockPartyInvite = value; _config.Save(); OnPropertyChanged(); } }
        public bool MobFilterEnabled { get => Profile.MobFilterEnabled; set { Profile.MobFilterEnabled = value; _config.Save(); OnPropertyChanged(); } }

        // Auto Agents
        public bool AutoCarver { get => Profile.AutoCarver; set { Profile.AutoCarver = value; _config.Save(); OnPropertyChanged(); } }
        public bool BoneCutter { get => Profile.BoneCutter; set { Profile.BoneCutter = value; _config.Save(); OnPropertyChanged(); } }
        public bool AutoRemount { get => Profile.AutoRemount; set { Profile.AutoRemount = value; _config.Save(); OnPropertyChanged(); } }

        [RelayCommand]
        private void AddGraphFilter()
        {
            var filter = new GraphChangeData();
            Profile.GraphFilters.Add(filter);
            SyncCollection(GraphFilters, Profile.GraphFilters, _graphLock);
            _config.Save();
        }

        [RelayCommand]
        private void RemoveGraphFilter(GraphChangeData filter)
        {
            if (filter != null)
            {
                Profile.GraphFilters.Remove(filter);
                SyncCollection(GraphFilters, Profile.GraphFilters, _graphLock);
                _config.Save();
            }
        }

        [RelayCommand]
        private async Task SetCarverBlade()
        {
            var targetInfo = await _targeting.AcquireTargetAsync(); uint serial = targetInfo.Serial;
            if (serial != 0)
            {
                Profile.AutoCarverBlade = serial;
                _config.Save();
                OnPropertyChanged(nameof(AutoCarverBladeText));
            }
        }

        [RelayCommand]
        private async Task SetBoneBlade()
        {
            var targetInfo = await _targeting.AcquireTargetAsync(); uint serial = targetInfo.Serial;
            if (serial != 0)
            {
                Profile.BoneCutterBlade = serial;
                _config.Save();
                OnPropertyChanged(nameof(BoneCutterBladeText));
            }
        }

        [RelayCommand]
        private async Task SetRemountSerial()
        {
            var targetInfo = await _targeting.AcquireTargetAsync(); uint serial = targetInfo.Serial;
            if (serial != 0)
            {
                Profile.RemountSerial = serial;
                _config.Save();
                OnPropertyChanged(nameof(RemountSerialText));
            }
        }

        public string AutoCarverBladeText => Profile.AutoCarverBlade == 0 ? "Not Set" : $"0x{Profile.AutoCarverBlade:X8}";
        public string BoneCutterBladeText => Profile.BoneCutterBlade == 0 ? "Not Set" : $"0x{Profile.BoneCutterBlade:X8}";
        public string RemountSerialText => Profile.RemountSerial == 0 ? "Not Set" : $"0x{Profile.RemountSerial:X8}";
    }
}
