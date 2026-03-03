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
        private readonly ITargetingService _targetingService;

        public UserProfile CurrentProfile => _configService.CurrentProfile;

        [ObservableProperty] private int _forceWidth;
        [ObservableProperty] private int _forceHeight;
        [ObservableProperty] private bool _scaleItems;
        [ObservableProperty] private bool _alwaysOnTop;
        [ObservableProperty] private double _uiOpacity;

        [ObservableProperty] private bool _autoCarver;
        [ObservableProperty] private uint _autoCarverBlade;
        [ObservableProperty] private bool _boneCutter;
        [ObservableProperty] private uint _boneCutterBlade;
        [ObservableProperty] private bool _autoRemount;
        [ObservableProperty] private uint _remountSerial;
        [ObservableProperty] private bool _blockTradeRequest;
        [ObservableProperty] private bool _blockPartyInvite;
        [ObservableProperty] private bool _autoScreenshotOnDeath;

        public OptionsViewModel(IConfigService configService, ITargetingService targetingService)
        {
            _configService = configService;
            _targetingService = targetingService;
            LoadFromConfig();
        }

        private void LoadFromConfig()
        {
            var profile = _configService.CurrentProfile;
            ForceWidth = profile.ForceWidth;
            ForceHeight = profile.ForceHeight;
            ScaleItems = profile.ScaleItems;
            AlwaysOnTop = profile.AlwaysOnTop;
            UiOpacity = profile.UiOpacity;
            
            AutoCarver = profile.AutoCarver;
            AutoCarverBlade = profile.AutoCarverBlade;
            BoneCutter = profile.BoneCutter;
            BoneCutterBlade = profile.BoneCutterBlade;
            AutoRemount = profile.AutoRemount;
            RemountSerial = profile.RemountSerial;
            BlockTradeRequest = profile.BlockTradeRequest;
            BlockPartyInvite = profile.BlockPartyInvite;
            AutoScreenshotOnDeath = profile.AutoScreenshotOnDeath;
        }

        partial void OnForceWidthChanged(int value) => SaveToConfig();
        partial void OnForceHeightChanged(int value) => SaveToConfig();
        partial void OnScaleItemsChanged(bool value) => SaveToConfig();
        partial void OnAlwaysOnTopChanged(bool value) => SaveToConfig();
        partial void OnUiOpacityChanged(double value) => SaveToConfig();
        
        partial void OnAutoCarverChanged(bool value) => SaveToConfig();
        partial void OnAutoCarverBladeChanged(uint value) => SaveToConfig();
        partial void OnBoneCutterChanged(bool value) => SaveToConfig();
        partial void OnBoneCutterBladeChanged(uint value) => SaveToConfig();
        partial void OnAutoRemountChanged(bool value) => SaveToConfig();
        partial void OnRemountSerialChanged(uint value) => SaveToConfig();
        partial void OnBlockTradeRequestChanged(bool value) => SaveToConfig();
        partial void OnBlockPartyInviteChanged(bool value) => SaveToConfig();
        partial void OnAutoScreenshotOnDeathChanged(bool value) => SaveToConfig();

        [RelayCommand]
        private async System.Threading.Tasks.Task SetCarverBladeAsync()
        {
            var target = await _targetingService.AcquireTargetAsync();
            if (target > 0)
            {
                AutoCarverBlade = target;
            }
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task SetBoneCutterBladeAsync()
        {
            var target = await _targetingService.AcquireTargetAsync();
            if (target > 0)
            {
                BoneCutterBlade = target;
            }
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task SetMountAsync()
        {
            var target = await _targetingService.AcquireTargetAsync();
            if (target > 0)
            {
                RemountSerial = target;
            }
        }

        private void SaveToConfig()
        {
            var profile = _configService.CurrentProfile;
            profile.ForceWidth = ForceWidth;
            profile.ForceHeight = ForceHeight;
            profile.ScaleItems = ScaleItems;
            profile.AlwaysOnTop = AlwaysOnTop;
            profile.UiOpacity = UiOpacity;
            
            profile.AutoCarver = AutoCarver;
            profile.AutoCarverBlade = AutoCarverBlade;
            profile.BoneCutter = BoneCutter;
            profile.BoneCutterBlade = BoneCutterBlade;
            profile.AutoRemount = AutoRemount;
            profile.RemountSerial = RemountSerial;
            profile.BlockTradeRequest = BlockTradeRequest;
            profile.BlockPartyInvite = BlockPartyInvite;
            profile.AutoScreenshotOnDeath = AutoScreenshotOnDeath;
            
            _configService.Save();
        }
    }
}
