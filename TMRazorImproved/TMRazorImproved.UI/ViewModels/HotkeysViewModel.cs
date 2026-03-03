using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models.Config;

namespace TMRazorImproved.UI.ViewModels
{
    public partial class HotkeysViewModel : ViewModelBase
    {
        private readonly IConfigService _configService;
        private readonly IHotkeyService _hotkeyService;
        private readonly ILanguageService _languageService;

        [ObservableProperty]
        private ObservableCollection<HotkeyActionNode> _actionTree = new();

        [ObservableProperty]
        private ObservableCollection<HotkeyDefinition> _currentHotkeys = new();

        [ObservableProperty]
        private HotkeyDefinition? _selectedHotkey;

        [ObservableProperty]
        private bool _isCapturing;

        [ObservableProperty]
        private string _captureStatus;

        [ObservableProperty]
        private string _selectedActionName = "None";

        [ObservableProperty]
        private bool _selectedHotkeyPassThrough;

        public HotkeysViewModel(IConfigService configService, IHotkeyService hotkeyService, ILanguageService languageService)
        {
            _configService = configService;
            _hotkeyService = hotkeyService;
            _languageService = languageService;

            InitializeActionTree();
            LoadHotkeys();
            
            _captureStatus = _languageService.GetString("Hotkeys.Status.ClickSet");
            EnableThreadSafeCollection(CurrentHotkeys, new object());
        }

        private void InitializeActionTree()
        {
            var root = new List<HotkeyActionNode>();

            // Agenti
            var agents = new HotkeyActionNode { Name = _languageService.GetString("HotKey.Category.Agents") };
            agents.Children.Add(new HotkeyActionNode { Name = "AutoLoot: Toggle", Action = "AutoLoot:Toggle" });
            agents.Children.Add(new HotkeyActionNode { Name = "Scavenger: Toggle", Action = "Scavenger:Toggle" });
            agents.Children.Add(new HotkeyActionNode { Name = "Organizer: Toggle", Action = "Organizer:Toggle" });
            agents.Children.Add(new HotkeyActionNode { Name = "BandageHeal: Toggle", Action = "BandageHeal:Toggle" });
            root.Add(agents);

            // Targeting
            var targeting = new HotkeyActionNode { Name = _languageService.GetString("HotKey.Category.Target") };
            targeting.Children.Add(new HotkeyActionNode { Name = "Target: Self", Action = "Target:Self" });
            targeting.Children.Add(new HotkeyActionNode { Name = "Target: Last", Action = "Target:Last" });
            targeting.Children.Add(new HotkeyActionNode { Name = "Target: Closest Mobile", Action = "Target:ClosestMobile" });
            targeting.Children.Add(new HotkeyActionNode { Name = "Target: Next Mobile", Action = "Target:NextMobile" });
            root.Add(targeting);

            // Dress
            var dress = new HotkeyActionNode { Name = _languageService.GetString("HotKey.Category.Dress") };
            dress.Children.Add(new HotkeyActionNode { Name = "Dress: Undress All", Action = "Dress:UndressAll" });
            root.Add(dress);

            ActionTree = new ObservableCollection<HotkeyActionNode>(root);
        }

        private void LoadHotkeys()
        {
            if (_configService.CurrentProfile != null)
            {
                CurrentHotkeys = new ObservableCollection<HotkeyDefinition>(_configService.CurrentProfile.Hotkeys);
            }
        }

        partial void OnSelectedHotkeyChanged(HotkeyDefinition? value)
        {
            if (value != null)
            {
                SelectedHotkeyPassThrough = value.PassThrough;
            }
        }

        partial void OnSelectedHotkeyPassThroughChanged(bool value)
        {
            if (SelectedHotkey != null && SelectedHotkey.PassThrough != value)
            {
                SelectedHotkey.PassThrough = value;
                _configService.Save();
            }
        }

        [RelayCommand]
        private void AddHotkey()
        {
            if (string.IsNullOrEmpty(SelectedActionName) || SelectedActionName == "None")
                return;

            var newHk = new HotkeyDefinition
            {
                Action = SelectedActionName,
                KeyCode = 0
            };

            CurrentHotkeys.Add(newHk);
            _configService.CurrentProfile?.Hotkeys.Add(newHk);
            SelectedHotkey = newHk;
            _configService.Save();
        }

        [RelayCommand]
        private void RemoveHotkey()
        {
            if (SelectedHotkey == null) return;

            CurrentHotkeys.Remove(SelectedHotkey);
            _configService.CurrentProfile?.Hotkeys.Remove(SelectedHotkey);
            SelectedHotkey = null;
            _configService.Save();
        }

        [RelayCommand]
        private void StartCapture()
        {
            if (SelectedHotkey == null) return;
            IsCapturing = true;
            CaptureStatus = _languageService.GetString("Hotkeys.Status.PressAnyKey");
        }

        public void HandleCapturedKey(Key key, ModifierKeys modifiers)
        {
            if (!IsCapturing || SelectedHotkey == null) return;

            // Convert WPF Key to Virtual Key
            int vk = KeyInterop.VirtualKeyFromKey(key);
            
            // We ignore modifiers as individual keys if they are the only thing pressed
            if (vk == 16 || vk == 17 || vk == 18) return; // Shift, Ctrl, Alt

            SelectedHotkey.KeyCode = vk;
            SelectedHotkey.Ctrl = (modifiers & ModifierKeys.Control) != 0;
            SelectedHotkey.Alt = (modifiers & ModifierKeys.Alt) != 0;
            SelectedHotkey.Shift = (modifiers & ModifierKeys.Shift) != 0;

            // Trigger property change notification for the UI
            var index = CurrentHotkeys.IndexOf(SelectedHotkey);
            CurrentHotkeys[index] = SelectedHotkey; 

            IsCapturing = false;
            CaptureStatus = string.Format(_languageService.GetString("Hotkeys.Status.KeySet"), GetKeyDisplay(SelectedHotkey));
            _configService.Save();
        }

        public string GetKeyDisplay(HotkeyDefinition hk)
        {
            if (hk == null || hk.KeyCode == 0) return "None";

            var parts = new List<string>();
            if (hk.Ctrl) parts.Add("Ctrl");
            if (hk.Alt) parts.Add("Alt");
            if (hk.Shift) parts.Add("Shift");

            Key wpfKey = KeyInterop.KeyFromVirtualKey(hk.KeyCode);
            parts.Add(wpfKey.ToString());

            return string.Join(" + ", parts);
        }
    }

    public class HotkeyActionNode
    {
        public string Name { get; set; } = string.Empty;
        public string? Action { get; set; }
        public List<HotkeyActionNode> Children { get; set; } = new();
    }
}
