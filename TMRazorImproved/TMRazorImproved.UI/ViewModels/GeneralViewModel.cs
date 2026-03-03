using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Enums;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Wpf.Ui;

namespace TMRazorImproved.UI.ViewModels
{
    public partial class GeneralViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _selectedProfile;

        public ObservableCollection<string> AvailableProfiles { get; } = new();

        private readonly IConfigService _configService;
        private readonly ILanguageService _languageService;
        private readonly IClientInteropService _clientInterop;
        private readonly IContentDialogService _dialogService;
        private readonly ISnackbarService _snackbarService;

        [ObservableProperty]
        private string _clientPath;

        [ObservableProperty]
        private string _dataPath;

        [ObservableProperty]
        private string _serverAddress = "127.0.0.1";

        [ObservableProperty]
        private int _serverPort = 2593;

        [ObservableProperty]
        private string _statusMessage;

        [ObservableProperty]
        private bool _patchEncryption;

        partial void OnPatchEncryptionChanged(bool value)
        {
            _configService.Global.PatchEncryption = value;
            _configService.Save();
        }

        [ObservableProperty]
        private bool _allowMultiClient;

        partial void OnAllowMultiClientChanged(bool value)
        {
            _configService.Global.AllowMultiClient = value;
            _configService.Save();
        }

        [ObservableProperty]
        private bool _negotiateFeatures;

        partial void OnNegotiateFeaturesChanged(bool value)
        {
            _configService.Global.NegotiateFeatures = value;
            _configService.Save();
        }

        [ObservableProperty]
        private bool _removeStaminaCheck;

        partial void OnRemoveStaminaCheckChanged(bool value)
        {
            _configService.Global.RemoveStaminaCheck = value;
            _configService.Save();
        }

        private Views.Windows.SpellGridWindow? _spellGrid;
        private Views.Windows.FloatingToolbarWindow? _toolbar;
        private Views.Windows.DPSMeterWindow? _dpsMeter;

        public GeneralViewModel(
            IConfigService configService, 
            ILanguageService languageService,
            IClientInteropService clientInterop,
            IContentDialogService dialogService,
            ISnackbarService snackbarService)
        {
            _configService = configService;
            _languageService = languageService;
            _clientInterop = clientInterop;
            _dialogService = dialogService;
            _snackbarService = snackbarService;

            // Carica i dati iniziali dal file di configurazione globale
            _clientPath = _configService.Global.ClientPath;
            _dataPath = _configService.Global.DataPath;
            _patchEncryption = _configService.Global.PatchEncryption;
            _allowMultiClient = _configService.Global.AllowMultiClient;
            _negotiateFeatures = _configService.Global.NegotiateFeatures;
            _removeStaminaCheck = _configService.Global.RemoveStaminaCheck;
            
            _selectedProfile = _configService.Global.LastProfile;
            LoadAvailableProfiles();

            _statusMessage = _languageService.GetString("Status.Ready");
        }

        private void LoadAvailableProfiles()
        {
            var current = SelectedProfile;
            AvailableProfiles.Clear();
            foreach (var profile in _configService.GetAvailableProfiles())
            {
                AvailableProfiles.Add(profile);
            }
            SelectedProfile = current;
        }

        partial void OnSelectedProfileChanged(string value)
        {
            if (!string.IsNullOrEmpty(value) && value != _configService.Global.LastProfile)
            {
                _configService.SwitchProfile(value);
                _configService.Global.LastProfile = value;
                _configService.Save();
                StatusMessage = $"Profile switched to: {value}";
                _snackbarService.Show("Profile Changed", $"Active profile is now '{value}'", Wpf.Ui.Controls.ControlAppearance.Info, null, TimeSpan.FromSeconds(3));
            }
        }

        [RelayCommand]
        private async Task CreateProfile()
        {
            var textBox = new Wpf.Ui.Controls.TextBox
            {
                PlaceholderText = "Enter profile name...",
                Margin = new System.Windows.Thickness(0, 10, 0, 0)
            };

            var dialog = new Wpf.Ui.Controls.ContentDialog(_dialogService.GetDialogHost())
            {
                Title = "Create New Profile",
                Content = textBox,
                PrimaryButtonText = "Create",
                CloseButtonText = "Cancel",
                DefaultButton = Wpf.Ui.Controls.ContentDialogButton.Primary
            };

            var result = await dialog.ShowAsync();

            if (result == Wpf.Ui.Controls.ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(textBox.Text))
            {
                var newName = textBox.Text.Trim();
                _configService.CreateProfile(newName);
                LoadAvailableProfiles();
                SelectedProfile = newName;
                _snackbarService.Show("Success", $"Profile '{newName}' created.", Wpf.Ui.Controls.ControlAppearance.Success, null, TimeSpan.FromSeconds(3));
            }
        }

        [RelayCommand]
        private void DeleteProfile()
        {
            if (AvailableProfiles.Count <= 1 || SelectedProfile == "Default") return;

            var toDelete = SelectedProfile;
            _configService.DeleteProfile(toDelete);
            LoadAvailableProfiles();
            SelectedProfile = AvailableProfiles.FirstOrDefault() ?? "Default";
            _snackbarService.Show("Profile Deleted", $"Profile '{toDelete}' has been removed.", Wpf.Ui.Controls.ControlAppearance.Caution, null, TimeSpan.FromSeconds(3));
        }

        [RelayCommand]
        private async Task CloneProfile()
        {
            if (string.IsNullOrEmpty(SelectedProfile)) return;

            var textBox = new Wpf.Ui.Controls.TextBox
            {
                Text = $"{SelectedProfile}_Copy",
                PlaceholderText = "Enter new profile name...",
                Margin = new System.Windows.Thickness(0, 10, 0, 0)
            };

            var dialog = new Wpf.Ui.Controls.ContentDialog(_dialogService.GetDialogHost())
            {
                Title = $"Clone Profile: {SelectedProfile}",
                Content = textBox,
                PrimaryButtonText = "Clone",
                CloseButtonText = "Cancel",
                DefaultButton = Wpf.Ui.Controls.ContentDialogButton.Primary
            };

            var result = await dialog.ShowAsync();

            if (result == Wpf.Ui.Controls.ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(textBox.Text))
            {
                var newName = textBox.Text.Trim();
                _configService.CloneProfile(SelectedProfile, newName);
                LoadAvailableProfiles();
                SelectedProfile = newName;
                _snackbarService.Show("Success", $"Profile cloned to '{newName}'.", Wpf.Ui.Controls.ControlAppearance.Success, null, TimeSpan.FromSeconds(3));
            }
        }

        [RelayCommand]
        private async Task RenameProfile()
        {
            if (string.IsNullOrEmpty(SelectedProfile) || SelectedProfile == "Default") return;

            var oldName = SelectedProfile;
            var textBox = new Wpf.Ui.Controls.TextBox
            {
                Text = oldName,
                PlaceholderText = "Enter new profile name...",
                Margin = new System.Windows.Thickness(0, 10, 0, 0)
            };

            var dialog = new Wpf.Ui.Controls.ContentDialog(_dialogService.GetDialogHost())
            {
                Title = $"Rename Profile: {oldName}",
                Content = textBox,
                PrimaryButtonText = "Rename",
                CloseButtonText = "Cancel",
                DefaultButton = Wpf.Ui.Controls.ContentDialogButton.Primary
            };

            var result = await dialog.ShowAsync();

            if (result == Wpf.Ui.Controls.ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(textBox.Text))
            {
                var newName = textBox.Text.Trim();
                if (newName == oldName) return;

                _configService.RenameProfile(oldName, newName);
                LoadAvailableProfiles();
                SelectedProfile = newName;
                _snackbarService.Show("Success", $"Profile renamed to '{newName}'.", Wpf.Ui.Controls.ControlAppearance.Success, null, TimeSpan.FromSeconds(3));
            }
        }

        [RelayCommand]
        private void BrowseClient()
        {
            // Implementazione futura con OpenFileDialog
            StatusMessage = _languageService.GetString("Status.Browsing");
        }

        [RelayCommand]
        private void LaunchClient()
        {
            if (string.IsNullOrEmpty(ClientPath))
            {
                StatusMessage = _languageService.GetString("Status.ErrorEmptyPath");
                return;
            }

            StatusMessage = _languageService.GetString("Status.Launching");
            
            // Salviamo le impostazioni prima di lanciare
            _configService.Global.ClientPath = ClientPath;
            _configService.Global.DataPath = DataPath;
            _configService.Global.PatchEncryption = PatchEncryption;
            _configService.Global.AllowMultiClient = AllowMultiClient;
            _configService.Global.NegotiateFeatures = NegotiateFeatures;
            _configService.Global.RemoveStaminaCheck = RemoveStaminaCheck;
            _configService.Save();

            // Logica di lancio tramite il servizio interop
            _clientInterop.LaunchClient(ClientPath, "Crypt.dll");
        }

        [RelayCommand]
        private void ToggleSpellGrid()
        {
            if (_spellGrid == null)
            {
                _spellGrid = App.GetService<Views.Windows.SpellGridWindow>();
                _spellGrid?.Show();
            }
            else
            {
                if (_spellGrid.IsVisible) _spellGrid.Hide();
                else _spellGrid.Show();
            }
        }

        [RelayCommand]
        private void ToggleToolbar()
        {
            if (_toolbar == null)
            {
                _toolbar = App.GetService<Views.Windows.FloatingToolbarWindow>();
                _toolbar?.Show();
            }
            else
            {
                if (_toolbar.IsVisible) _toolbar.Hide();
                else _toolbar.Show();
            }
        }

        [RelayCommand]
        private void ToggleDPSMeter()
        {
            if (_dpsMeter == null)
            {
                _dpsMeter = App.GetService<Views.Windows.DPSMeterWindow>();
                _dpsMeter?.Show();
            }
            else
            {
                if (_dpsMeter.IsVisible) _dpsMeter.Hide();
                else _dpsMeter.Show();
            }
        }
    }
}
