using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Enums;
using TMRazorImproved.UI.Utilities;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Mvvm.Messaging;
using TMRazorImproved.Shared.Messages;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Nodes;
using Wpf.Ui;

namespace TMRazorImproved.UI.ViewModels
{
    public partial class GeneralViewModel : ViewModelBase, IRecipient<ShardChangedMessage>
    {
        [ObservableProperty]
        private string _selectedProfile;

        public ObservableCollection<string> AvailableProfiles { get; } = new();

        private readonly IConfigService _configService;
        private readonly ILanguageService _languageService;
        private readonly IClientInteropService _clientInterop;
        private readonly IPacketService _packetService;
        private readonly IContentDialogService _dialogService;
        private readonly ISnackbarService _snackbarService;
        private readonly IUOModService _uoModService;
        private readonly ILogger<GeneralViewModel> _logger;
        private readonly IMessenger _messenger;

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

        [ObservableProperty]
        private bool _isGameRunning;

        [ObservableProperty]
        private string _launchButtonText = "Launch";

        private readonly System.Windows.Threading.DispatcherTimer _processCheckTimer;

        partial void OnRemoveStaminaCheckChanged(bool value)
        {
            _configService.Global.RemoveStaminaCheck = value;
            _configService.Save();
        }

        private Views.Windows.SpellGridWindow? _spellGrid;
        private Views.Windows.FloatingToolbarWindow? _toolbar;
        private Views.Windows.DPSMeterWindow? _dpsMeter;

        private readonly ISkillsService _skillsService;

        public GeneralViewModel(
            IConfigService configService,
            ILanguageService languageService,
            IClientInteropService clientInterop,
            IPacketService packetService,
            ISkillsService skillsService,
            IContentDialogService dialogService,
            ISnackbarService snackbarService,
            IUOModService uoModService,
            IMessenger messenger,
            ILogger<GeneralViewModel> logger)
        {
            _configService = configService;
            _languageService = languageService;
            _clientInterop = clientInterop;
            _packetService = packetService;
            _skillsService = skillsService;
            _dialogService = dialogService;
            _snackbarService = snackbarService;
            _uoModService = uoModService;
            _messenger = messenger;
            _logger = logger;

            // Carica i dati iniziali dal file di configurazione globale
            _clientPath = _configService.Global.ClientPath;
            _dataPath = _configService.Global.DataPath;

            // Carica i nomi delle skill dai file dati del client (se disponibili)
            _skillsService.LoadNamesFromDataPath(_dataPath);
            _patchEncryption = _configService.Global.PatchEncryption;
            _allowMultiClient = _configService.Global.AllowMultiClient;
            _negotiateFeatures = _configService.Global.NegotiateFeatures;
            _removeStaminaCheck = _configService.Global.RemoveStaminaCheck;
            
            _selectedProfile = _configService.Global.LastProfile;
            LoadAvailableProfiles();

            _statusMessage = _languageService.GetString("Status.Ready");
            _launchButtonText = _languageService.GetString("General.Action.Launch");

            _processCheckTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            _processCheckTimer.Tick += (s, e) => CheckProcessStatus();
            _processCheckTimer.Start();
            CheckProcessStatus();

            _messenger.Register<ShardChangedMessage>(this);
        }

        public void Receive(ShardChangedMessage message)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                StatusMessage = $"Shard detected: {message.Value}";
                LoadAvailableProfiles();
            });
        }

        private void CheckProcessStatus()
        {
            if (string.IsNullOrEmpty(ClientPath)) return;
            string clientDir = System.IO.Path.GetDirectoryName(ClientPath) ?? "";
            uint pid = _clientInterop.FindRunningGameProcess(clientDir);
            
            IsGameRunning = pid != 0;
            LaunchButtonText = IsGameRunning ? _languageService.GetString("General.Action.Connect") : _languageService.GetString("General.Action.Launch");
        }

        private void LoadAvailableProfiles()
        {
            var current = SelectedProfile;
            AvailableProfiles.Clear();
            foreach (var profile in _configService.GetAvailableProfiles(_configService.CurrentShardId))
            {
                AvailableProfiles.Add(profile);
            }
            
            if (AvailableProfiles.Contains(current))
                SelectedProfile = current;
            else
                SelectedProfile = AvailableProfiles.FirstOrDefault() ?? "Default";
        }

        partial void OnSelectedProfileChanged(string value)
        {
            if (!string.IsNullOrEmpty(value) && value != _configService.Global.LastProfile)
            {
                _configService.SwitchProfile(value);
                _configService.Global.LastProfile = value;
                _configService.Save();
                StatusMessage = $"{_languageService.GetString("General.Status.ProfileSwitched")}: {value}";
                _snackbarService.Show(_languageService.GetString("General.Snackbar.ProfileChanged"), $"{_languageService.GetString("General.Snackbar.ProfileActive")} '{value}'", Wpf.Ui.Controls.ControlAppearance.Info, null, TimeSpan.FromSeconds(3));
            }
        }

        [RelayCommand]
        private async Task CreateProfile()
        {
            var textBox = new Wpf.Ui.Controls.TextBox
            {
                PlaceholderText = _languageService.GetString("General.Profile.Create.Placeholder"),
                Margin = new System.Windows.Thickness(0, 10, 0, 0)
            };

            var dialog = new Wpf.Ui.Controls.ContentDialog(_dialogService.GetDialogHost())
            {
                Title = _languageService.GetString("General.Profile.Create.Title"),
                Content = textBox,
                PrimaryButtonText = _languageService.GetString("General.Dialog.Create"),
                CloseButtonText = _languageService.GetString("General.Dialog.Cancel"),
                DefaultButton = Wpf.Ui.Controls.ContentDialogButton.Primary
            };

            var result = await dialog.ShowAsync();

            if (result == Wpf.Ui.Controls.ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(textBox.Text))
            {
                var newName = textBox.Text.Trim();
                _configService.CreateProfile(newName);
                LoadAvailableProfiles();
                SelectedProfile = newName;
                _snackbarService.Show(_languageService.GetString("General.Snackbar.Success"), $"{_languageService.GetString("General.Snackbar.ProfileCreated")} '{newName}'.", Wpf.Ui.Controls.ControlAppearance.Success, null, TimeSpan.FromSeconds(3));
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
            _snackbarService.Show(_languageService.GetString("General.Snackbar.ProfileDeleted"), $"{_languageService.GetString("General.Snackbar.ProfileRemoved")} '{toDelete}'.", Wpf.Ui.Controls.ControlAppearance.Caution, null, TimeSpan.FromSeconds(3));
        }

        [RelayCommand]
        private async Task CloneProfile()
        {
            if (string.IsNullOrEmpty(SelectedProfile)) return;

            var textBox = new Wpf.Ui.Controls.TextBox
            {
                Text = $"{SelectedProfile}_Copy",
                PlaceholderText = _languageService.GetString("General.Profile.Create.Placeholder"),
                Margin = new System.Windows.Thickness(0, 10, 0, 0)
            };

            var dialog = new Wpf.Ui.Controls.ContentDialog(_dialogService.GetDialogHost())
            {
                Title = $"{_languageService.GetString("General.Profile.Clone.Title")}: {SelectedProfile}",
                Content = textBox,
                PrimaryButtonText = _languageService.GetString("General.Dialog.Clone"),
                CloseButtonText = _languageService.GetString("General.Dialog.Cancel"),
                DefaultButton = Wpf.Ui.Controls.ContentDialogButton.Primary
            };

            var result = await dialog.ShowAsync();

            if (result == Wpf.Ui.Controls.ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(textBox.Text))
            {
                var newName = textBox.Text.Trim();
                _configService.CloneProfile(SelectedProfile, newName);
                LoadAvailableProfiles();
                SelectedProfile = newName;
                _snackbarService.Show(_languageService.GetString("General.Snackbar.Success"), $"{_languageService.GetString("General.Snackbar.ProfileCloned")} '{newName}'.", Wpf.Ui.Controls.ControlAppearance.Success, null, TimeSpan.FromSeconds(3));
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
                PlaceholderText = _languageService.GetString("General.Profile.Create.Placeholder"),
                Margin = new System.Windows.Thickness(0, 10, 0, 0)
            };

            var dialog = new Wpf.Ui.Controls.ContentDialog(_dialogService.GetDialogHost())
            {
                Title = $"{_languageService.GetString("General.Profile.Rename.Title")}: {oldName}",
                Content = textBox,
                PrimaryButtonText = _languageService.GetString("General.Dialog.Rename"),
                CloseButtonText = _languageService.GetString("General.Dialog.Cancel"),
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
                _snackbarService.Show(_languageService.GetString("General.Snackbar.Success"), $"{_languageService.GetString("General.Snackbar.ProfileRenamed")} '{newName}'.", Wpf.Ui.Controls.ControlAppearance.Success, null, TimeSpan.FromSeconds(3));
            }
        }

        [RelayCommand]
        private void BrowseClient()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Executables (*.exe)|*.exe|All files (*.*)|*.*",
                Title = "Select Ultima Online Client"
            };

            if (dialog.ShowDialog() == true)
            {
                ClientPath = dialog.FileName;
                _configService.Global.ClientPath = ClientPath;
                
                // Auto-detect DataPath from the executable's directory
                var directory = System.IO.Path.GetDirectoryName(ClientPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    DataPath = directory;
                    _configService.Global.DataPath = DataPath;
                    _skillsService.LoadNamesFromDataPath(DataPath);
                }

                _configService.Save();
                StatusMessage = "Client and Data paths updated.";
                _snackbarService.Show("Settings Updated", "Client path has been configured successfully.", Wpf.Ui.Controls.ControlAppearance.Success, null, TimeSpan.FromSeconds(3));
            }
        }

        [RelayCommand]
        private async Task LaunchClient()
        {
            if (string.IsNullOrEmpty(ClientPath))
            {
                StatusMessage = LanguageHelper.Status.ErrorEmptyPath;
                return;
            }

            StatusMessage = LanguageHelper.Status.Launching;
            
            // Salviamo le impostazioni prima di lanciare
            _configService.Global.ClientPath = ClientPath;
            _configService.Global.DataPath = DataPath;
            _configService.Global.PatchEncryption = PatchEncryption;
            _configService.Global.AllowMultiClient = AllowMultiClient;
            _configService.Global.NegotiateFeatures = NegotiateFeatures;
            _configService.Global.RemoveStaminaCheck = RemoveStaminaCheck;
            _configService.Save();

            try
            {
                // Get MainWindow handle (necessario per InstallLibrary)
                var mainWindow = System.Windows.Application.Current.MainWindow;
                if (mainWindow == null) { _logger.LogError("MainWindow is NULL."); return; }
                IntPtr windowHandle = new System.Windows.Interop.WindowInteropHelper(mainWindow).Handle;
                System.Diagnostics.Trace.WriteLine($"[Launch] MainWindow Handle: 0x{windowHandle.ToInt64():X}");

                // Calcola le flag (features)
                int flags = 0;
                if (NegotiateFeatures) flags |= 0x04;
                if (PatchEncryption) flags |= 0x08;

                string clientDir = System.IO.Path.GetDirectoryName(ClientPath) ?? "";

                // CASO 1: il gioco è già in esecuzione — non deployare il plugin (DLL locked dal processo)
                uint runningPid = _clientInterop.FindRunningGameProcess(clientDir);

                // Deploy TMRazorPlugin.dll only when launching a new client instance.
                // If the game is already running, the plugin is already loaded and the DLL is locked.
                if (runningPid == 0)
                    DeployPlugin(clientDir);

                await Task.Run(() =>
                {
                    int gamePid = 0;

                    if (runningPid != 0)
                    {
                        System.Diagnostics.Trace.WriteLine($"[Launch] Game already running, attaching to PID {runningPid}");
                        gamePid = (int)runningPid;
                    }
                    else
                    {
                        // CASO 2: il gioco non è in esecuzione.
                        // Usa Process.Start direttamente: Loader.dll inietta nel launcher (sbagliato)
                        // se TmClient.exe è un launcher che spawna il vero client.
                        // L'iniezione di Crypt.dll avviene comunque dopo via SetWindowsHookEx.
                        _clientInterop.PrepareForLaunch(); // snapshot PID prima del lancio
                        System.Diagnostics.Trace.WriteLine($"[Launch] Starting client via Process.Start: {ClientPath}");
                        var proc = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(ClientPath)
                        {
                            WorkingDirectory = clientDir,
                            UseShellExecute = true
                        });
                        uint launcherPid = (uint)(proc?.Id ?? 0);
                        System.Diagnostics.Trace.WriteLine($"[Launch] Process started. PID: {launcherPid}");

                        _clientInterop.WaitForWindow(launcherPid);
                        gamePid = _clientInterop.GetUOProcessId();
                        if (gamePid == 0) gamePid = (int)launcherPid;
                        System.Diagnostics.Trace.WriteLine($"[Launch] Game PID after wait: {gamePid} (launcher was {launcherPid})");
                    }

                    System.Diagnostics.Trace.WriteLine($"[Launch] Installing shared memory for game PID: {gamePid}");
                    _clientInterop.InstallLibrary(windowHandle, gamePid, flags);
                    // Signal PacketService that Crypt.dll is ready: enables packet processing
                    // and resets any buffer state from premature timer ticks.
                    _packetService.NotifyCryptReady();
                    System.Diagnostics.Trace.WriteLine($"[Launch] Shared memory ready (PID: {gamePid})");
                    _logger.LogInformation("Shared memory ready (PID: {Pid})", gamePid);

                    // InstallLibrary apre la shared memory creata da TMRazorPlugin.dll (già caricata
                    // da TmClient) e popola PacketTable in modo che GetPacketLength funzioni.
                    // I WH hook non funzionano su TmClient (x64) ma non sono più necessari.

                    if (AllowMultiClient && gamePid != 0)
                    {
                        _uoModService.InjectUoMod(gamePid);
                        _uoModService.EnablePatch(UOPatchType.MultiUO, true);
                    }
                });

                StatusMessage = LanguageHelper.Status.ClientReady;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Errore avvio: {ex.Message}";
            }
        }

        /// <summary>
        /// Copies TMRazorPlugin.dll to the ClassicUO plugin directory and updates settings.json
        /// (merging with existing plugins) so that the client loads it on next startup.
        /// Works with any ClassicUO-based client that uses the standard plugin API.
        /// </summary>
        private void DeployPlugin(string clientDir)
        {
            try
            {
                string appDir    = System.AppContext.BaseDirectory;
                string pluginSrc = System.IO.Path.Combine(appDir, "TMRazorPlugin.dll");
                if (!System.IO.File.Exists(pluginSrc))
                {
                    System.Diagnostics.Trace.WriteLine($"[Deploy] TMRazorPlugin.dll not found in {appDir}, skipping deploy.");
                    return;
                }

                string pluginDir = System.IO.Path.Combine(clientDir, "Data", "Plugins", "TMRazorImproved");
                System.IO.Directory.CreateDirectory(pluginDir);

                string pluginDst = System.IO.Path.Combine(pluginDir, "TMRazorPlugin.dll");
                System.IO.File.Copy(pluginSrc, pluginDst, overwrite: true);
                System.Diagnostics.Trace.WriteLine($"[Deploy] TMRazorPlugin.dll → {pluginDst}");

                // Update settings.json plugins array to point to TMRazorPlugin.dll
                string settingsPath = System.IO.Path.Combine(clientDir, "settings.json");
                if (System.IO.File.Exists(settingsPath))
                {
                    try
                    {
                        string raw  = System.IO.File.ReadAllText(settingsPath);
                        var    node = JsonNode.Parse(raw);
                        if (node != null)
                        {
                            // Merge: keep existing plugins, add ours if not already present
                            var existing = node["plugins"] as JsonArray ?? new JsonArray();
                            bool alreadyPresent = false;
                            foreach (var item in existing)
                                if (string.Equals(item?.GetValue<string>(), pluginDst, StringComparison.OrdinalIgnoreCase))
                                { alreadyPresent = true; break; }

                            if (!alreadyPresent)
                            {
                                var merged = new JsonArray();
                                foreach (var item in existing)
                                    merged.Add(item?.GetValue<string>());
                                merged.Add(pluginDst);
                                node["plugins"] = merged;
                                var opts = new JsonSerializerOptions { WriteIndented = true };
                                System.IO.File.WriteAllText(settingsPath, node.ToJsonString(opts));
                                System.Diagnostics.Trace.WriteLine($"[Deploy] settings.json updated → plugin={pluginDst}");
                            }
                            else
                            {
                                System.Diagnostics.Trace.WriteLine($"[Deploy] settings.json already contains plugin entry, skipping.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine($"[Deploy] settings.json update warning: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"[Deploy] DeployPlugin warning: {ex.Message}");
            }
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
