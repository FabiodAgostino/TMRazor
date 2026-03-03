using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models.Config;
using System.Collections.Generic;

namespace TMRazorImproved.UI.ViewModels
{
    public partial class OptionsViewModel : ViewModelBase
    {
        private readonly IConfigService _configService;

        public UserProfile CurrentProfile => _configService.CurrentProfile;

        [ObservableProperty]
        private bool _filterLight;

        [ObservableProperty]
        private bool _filterWeather;

        [ObservableProperty]
        private bool _filterSound;

        [ObservableProperty]
        private bool _filterDeath;

        [ObservableProperty]
        private bool _filterStaff;

        [ObservableProperty]
        private bool _filterPoison;

        [ObservableProperty]
        private bool _filterSnoop;

        public OptionsViewModel(IConfigService configService)
        {
            _configService = configService;
            LoadFromConfig();
        }

        private void LoadFromConfig()
        {
            var profile = _configService.CurrentProfile;
            FilterLight = profile.FilterLight;
            FilterWeather = profile.FilterWeather;
            FilterSound = profile.FilterSound;
            FilterDeath = profile.FilterDeath;
            FilterStaff = profile.FilterStaff;
            FilterPoison = profile.FilterPoison;
            FilterSnoop = profile.FilterSnoop;
        }

        partial void OnFilterLightChanged(bool value) => SaveToConfig();
        partial void OnFilterWeatherChanged(bool value) => SaveToConfig();
        partial void OnFilterSoundChanged(bool value) => SaveToConfig();
        partial void OnFilterDeathChanged(bool value) => SaveToConfig();
        partial void OnFilterStaffChanged(bool value) => SaveToConfig();
        partial void OnFilterPoisonChanged(bool value) => SaveToConfig();
        partial void OnFilterSnoopChanged(bool value) => SaveToConfig();

        private void SaveToConfig()
        {
            var profile = _configService.CurrentProfile;
            profile.FilterLight = FilterLight;
            profile.FilterWeather = FilterWeather;
            profile.FilterSound = FilterSound;
            profile.FilterDeath = FilterDeath;
            profile.FilterStaff = FilterStaff;
            profile.FilterPoison = FilterPoison;
            profile.FilterSnoop = FilterSnoop;
            
            _configService.Save();
        }
    }
}
