using System.Runtime.InteropServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models.Config;

namespace TMRazorImproved.UI.ViewModels
{
    public partial class DisplayViewModel : ViewModelBase
    {
        private readonly IConfigService _configService;
        private readonly IClientInteropService _clientInterop;

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
        private Views.Windows.FloatingToolbarWindow? _toolbar;
        private Views.Windows.OverheadMessageOverlay? _overheadOverlay;

        public DisplayViewModel(IConfigService configService, IClientInteropService clientInterop)
        {
            _configService = configService;
            _clientInterop = clientInterop;
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

        [RelayCommand]
        private void ToggleFloatingToolbar()
        {
            if (_toolbar == null)
                _toolbar = App.GetService<Views.Windows.FloatingToolbarWindow>();

            if (_toolbar == null) return;
            if (_toolbar.IsVisible) _toolbar.Hide();
            else _toolbar.Show();
        }

        [RelayCommand]
        private void ToggleOverheadMessages()
        {
            if (_overheadOverlay == null)
                _overheadOverlay = App.GetService<Views.Windows.OverheadMessageOverlay>();

            if (_overheadOverlay == null) return;
            if (_overheadOverlay.IsVisible) _overheadOverlay.Hide();
            else _overheadOverlay.Show();
        }

        private void LoadFromConfig()
        {
            var profile = _configService.CurrentProfile;
            ForceWidth  = profile.ForceWidth;
            ForceHeight = profile.ForceHeight;
            ScaleItems  = profile.ScaleItems;
            AlwaysOnTop = profile.AlwaysOnTop;
            UiOpacity   = profile.UiOpacity;

            ShowNames         = profile.ShowNames;
            ShowHealth        = profile.ShowHealth;
            HighlightTarget   = profile.HighlightTarget;
            IncomingNames     = profile.IncomingNames;
            ShowIncomingDamage = profile.ShowIncomingDamage;
        }

        partial void OnForceWidthChanged(int value)  { SaveToConfig(); ApplyWindowSize(); }
        partial void OnForceHeightChanged(int value) { SaveToConfig(); ApplyWindowSize(); }
        partial void OnScaleItemsChanged(bool value) => SaveToConfig();
        partial void OnAlwaysOnTopChanged(bool value) => SaveToConfig();
        partial void OnUiOpacityChanged(double value) => SaveToConfig();

        partial void OnShowNamesChanged(bool value)         => SaveToConfig();
        partial void OnShowHealthChanged(bool value)        => SaveToConfig();
        partial void OnHighlightTargetChanged(bool value)   => SaveToConfig();
        partial void OnIncomingNamesChanged(bool value)     => SaveToConfig();
        partial void OnShowIncomingDamageChanged(bool value) => SaveToConfig();

        /// <summary>
        /// Applica ForceWidth/ForceHeight alla finestra del client UO tramite Win32 SetWindowPos.
        /// </summary>
        private void ApplyWindowSize()
        {
            if (ForceWidth <= 0 || ForceHeight <= 0) return;

            var hwnd = _clientInterop.FindUOWindow();
            if (hwnd == System.IntPtr.Zero) return;

            const uint SWP_NOMOVE    = 0x0002;
            const uint SWP_NOZORDER  = 0x0004;
            const uint SWP_SHOWWINDOW = 0x0040;
            SetWindowPos(hwnd, System.IntPtr.Zero, 0, 0, ForceWidth, ForceHeight,
                         SWP_NOMOVE | SWP_NOZORDER | SWP_SHOWWINDOW);
        }

        private void SaveToConfig()
        {
            var profile = _configService.CurrentProfile;
            profile.ForceWidth  = ForceWidth;
            profile.ForceHeight = ForceHeight;
            profile.ScaleItems  = ScaleItems;
            profile.AlwaysOnTop = AlwaysOnTop;
            profile.UiOpacity   = UiOpacity;

            profile.ShowNames          = ShowNames;
            profile.ShowHealth         = ShowHealth;
            profile.HighlightTarget    = HighlightTarget;
            profile.IncomingNames      = IncomingNames;
            profile.ShowIncomingDamage = ShowIncomingDamage;

            _configService.Save();
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(System.IntPtr hWnd, System.IntPtr hWndInsertAfter,
                                                int x, int y, int cx, int cy, uint uFlags);
    }
}
