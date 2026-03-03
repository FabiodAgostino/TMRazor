using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models.Config;

namespace TMRazorImproved.UI.ViewModels
{
    public partial class DisplayViewModel : ViewModelBase
    {
        private readonly IConfigService _configService;

        public UserProfile CurrentProfile => _configService.CurrentProfile;

        [ObservableProperty] private int _forceWidth;
        [ObservableProperty] private int _forceHeight;
        [ObservableProperty] private bool _scaleItems;
        [ObservableProperty] private bool _alwaysOnTop;
        [ObservableProperty] private double _uiOpacity;
        
        [ObservableProperty] private bool _showNames;
        [ObservableProperty] private bool _showHealth;
        [ObservableProperty] private bool _highlightTarget;
        [ObservableProperty] private bool _incomingNames;
        [ObservableProperty] private bool _showIncomingDamage;

        private Views.Windows.TargetHPWindow? _targetHP;

        public DisplayViewModel(IConfigService configService)
        {
            _configService = configService;
            LoadFromConfig();
        }

        [RelayCommand]
        private void ToggleTargetHP()
        {
            if (_targetHP == null)
            {
                _targetHP = App.GetService<Views.Windows.TargetHPWindow>();
                _targetHP?.Show();
            }
            else
            {
                if (_targetHP.IsVisible) _targetHP.Hide();
                else _targetHP.Show();
            }
        }

        private void LoadFromConfig()
        {
            var profile = _configService.CurrentProfile;
            ForceWidth = profile.ForceWidth;
            ForceHeight = profile.ForceHeight;
            ScaleItems = profile.ScaleItems;
            AlwaysOnTop = profile.AlwaysOnTop;
            UiOpacity = profile.UiOpacity;
            
            ShowNames = profile.ShowNames;
            ShowHealth = profile.ShowHealth;
            HighlightTarget = profile.HighlightTarget;
            IncomingNames = profile.IncomingNames;
            ShowIncomingDamage = profile.ShowIncomingDamage;
        }

        partial void OnForceWidthChanged(int value) => SaveToConfig();
        partial void OnForceHeightChanged(int value) => SaveToConfig();
        partial void OnScaleItemsChanged(bool value) => SaveToConfig();
        partial void OnAlwaysOnTopChanged(bool value) => SaveToConfig();
        partial void OnUiOpacityChanged(double value) => SaveToConfig();
        
        partial void OnShowNamesChanged(bool value) => SaveToConfig();
        partial void OnShowHealthChanged(bool value) => SaveToConfig();
        partial void OnHighlightTargetChanged(bool value) => SaveToConfig();
        partial void OnIncomingNamesChanged(bool value) => SaveToConfig();
        partial void OnShowIncomingDamageChanged(bool value) => SaveToConfig();

        private void SaveToConfig()
        {
            var profile = _configService.CurrentProfile;
            profile.ForceWidth = ForceWidth;
            profile.ForceHeight = ForceHeight;
            profile.ScaleItems = ScaleItems;
            profile.AlwaysOnTop = AlwaysOnTop;
            profile.UiOpacity = UiOpacity;
            
            profile.ShowNames = ShowNames;
            profile.ShowHealth = ShowHealth;
            profile.HighlightTarget = HighlightTarget;
            profile.IncomingNames = IncomingNames;
            profile.ShowIncomingDamage = ShowIncomingDamage;
            
            _configService.Save();
        }
    }
}
