using System.Runtime.InteropServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models.Config;
using System.Threading.Tasks;

namespace TMRazorImproved.UI.ViewModels
{
    public partial class OptionsViewModel : ViewModelBase
    {
        private readonly IConfigService _configService;
        private readonly ITargetingService _targetingService;
        private readonly ILanguageService _languageService;
        private readonly IClientInteropService _clientInterop;

        public UserProfile CurrentProfile => _configService.CurrentProfile;

        // Display & Tweaks
        [ObservableProperty] private int _forceWidth;
        [ObservableProperty] private int _forceHeight;
        [ObservableProperty] private bool _scaleItems;
        [ObservableProperty] private bool _alwaysOnTop;
        [ObservableProperty] private double _uiOpacity;

        // Automation
        [ObservableProperty] private bool _autoCarver;
        [ObservableProperty] private uint _autoCarverBlade;
        [ObservableProperty] private bool _boneCutter;
        [ObservableProperty] private uint _boneCutterBlade;
        [ObservableProperty] private bool _autoRemount;
        [ObservableProperty] private uint _remountSerial;
        [ObservableProperty] private bool _blockTradeRequest;
        [ObservableProperty] private bool _blockPartyInvite;
        [ObservableProperty] private bool _autoScreenshotOnDeath;

        // Pathfinding
        [ObservableProperty] private int _pathFindingMaxRange;

        // Game Visuals (Integrated from DisplayViewModel)
        [ObservableProperty] private bool _showNames;
        [ObservableProperty] private bool _showHealth;
        [ObservableProperty] private bool _highlightTarget;
        [ObservableProperty] private bool _incomingNames;
        [ObservableProperty] private bool _showIncomingDamage;

        private Views.Windows.TargetHPWindow? _targetHP;
        private Views.Windows.FloatingToolbarWindow? _toolbar;
        private Views.Windows.OverheadMessageOverlay? _overheadOverlay;

        public OptionsViewModel(IConfigService configService, ITargetingService targetingService, ILanguageService languageService, IClientInteropService clientInterop)
        {
            _configService = configService;
            _targetingService = targetingService;
            _languageService = languageService;
            _clientInterop = clientInterop;
            LoadFromConfig();
        }

        private void LoadFromConfig()
        {
            var profile = _configService.CurrentProfile;
            
            // Display & Tweaks
            ForceWidth = profile.ForceWidth;
            ForceHeight = profile.ForceHeight;
            ScaleItems = profile.ScaleItems;
            AlwaysOnTop = profile.AlwaysOnTop;
            UiOpacity = profile.UiOpacity;
            
            // Automation
            AutoCarver = profile.AutoCarver;
            AutoCarverBlade = profile.AutoCarverBlade;
            BoneCutter = profile.BoneCutter;
            BoneCutterBlade = profile.BoneCutterBlade;
            AutoRemount = profile.AutoRemount;
            RemountSerial = profile.RemountSerial;
            BlockTradeRequest = profile.BlockTradeRequest;
            BlockPartyInvite = profile.BlockPartyInvite;
            AutoScreenshotOnDeath = profile.AutoScreenshotOnDeath;
            PathFindingMaxRange = profile.PathFindingMaxRange;

            // Game Visuals
            ShowNames = profile.ShowNames;
            ShowHealth = profile.ShowHealth;
            HighlightTarget = profile.HighlightTarget;
            IncomingNames = profile.IncomingNames;
            ShowIncomingDamage = profile.ShowIncomingDamage;
        }

        // Property Changed Handlers
        partial void OnForceWidthChanged(int value) { SaveToConfig(); ApplyWindowSize(); }
        partial void OnForceHeightChanged(int value) { SaveToConfig(); ApplyWindowSize(); }
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
        partial void OnPathFindingMaxRangeChanged(int value) => SaveToConfig();

        partial void OnShowNamesChanged(bool value) => SaveToConfig();
        partial void OnShowHealthChanged(bool value) => SaveToConfig();
        partial void OnHighlightTargetChanged(bool value) => SaveToConfig();
        partial void OnIncomingNamesChanged(bool value) => SaveToConfig();
        partial void OnShowIncomingDamageChanged(bool value) => SaveToConfig();

        // Automation Commands
        [RelayCommand]
        private async Task SetCarverBladeAsync()
        {
            var target = await _targetingService.AcquireTargetAsync();
            if (target.Serial > 0) AutoCarverBlade = target.Serial;
        }

        [RelayCommand]
        private async Task SetBoneCutterBladeAsync()
        {
            var target = await _targetingService.AcquireTargetAsync();
            if (target.Serial > 0) BoneCutterBlade = target.Serial;
        }

        [RelayCommand]
        private async Task SetMountAsync()
        {
            var target = await _targetingService.AcquireTargetAsync();
            if (target.Serial > 0) RemountSerial = target.Serial;
        }

        // Visual Overlay Commands (Integrated from DisplayViewModel)
        [RelayCommand]
        private void ToggleTargetHP()
        {
            _targetHP ??= App.GetService<Views.Windows.TargetHPWindow>();
            if (_targetHP == null) return;
            if (_targetHP.IsVisible) _targetHP.Hide(); else _targetHP.Show();
        }

        [RelayCommand]
        private void ToggleFloatingToolbar()
        {
            _toolbar ??= App.GetService<Views.Windows.FloatingToolbarWindow>();
            if (_toolbar == null) return;
            if (_toolbar.IsVisible) _toolbar.Hide(); else _toolbar.Show();
        }

        [RelayCommand]
        private void ToggleOverheadMessages()
        {
            _overheadOverlay ??= App.GetService<Views.Windows.OverheadMessageOverlay>();
            if (_overheadOverlay == null) return;
            if (_overheadOverlay.IsVisible) _overheadOverlay.Hide(); else _overheadOverlay.Show();
        }

        private void ApplyWindowSize()
        {
            if (ForceWidth <= 0 || ForceHeight <= 0) return;
            var hwnd = _clientInterop.FindUOWindow();
            if (hwnd == System.IntPtr.Zero) return;

            const uint SWP_NOMOVE = 0x0002;
            const uint SWP_NOZORDER = 0x0004;
            const uint SWP_SHOWWINDOW = 0x0040;
            SetWindowPos(hwnd, System.IntPtr.Zero, 0, 0, ForceWidth, ForceHeight, SWP_NOMOVE | SWP_NOZORDER | SWP_SHOWWINDOW);
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
            profile.PathFindingMaxRange = PathFindingMaxRange;

            profile.ShowNames = ShowNames;
            profile.ShowHealth = ShowHealth;
            profile.HighlightTarget = HighlightTarget;
            profile.IncomingNames = IncomingNames;
            profile.ShowIncomingDamage = ShowIncomingDamage;
            
            _configService.Save();
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(System.IntPtr hWnd, System.IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);
    }
}
