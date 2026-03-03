using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Enums;
using System.Threading.Tasks;

namespace TMRazorImproved.UI.ViewModels
{
    public partial class GeneralViewModel : ViewModelBase
    {
        private readonly IConfigService _configService;
        private readonly ILanguageService _languageService;
        private readonly IClientInteropService _clientInterop;

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

        public GeneralViewModel(
            IConfigService configService, 
            ILanguageService languageService,
            IClientInteropService clientInterop)
        {
            _configService = configService;
            _languageService = languageService;
            _clientInterop = clientInterop;

            // Carica i dati iniziali dal file di configurazione globale
            _clientPath = _configService.Global.ClientPath;
            _dataPath = _configService.Global.DataPath;
            _statusMessage = _languageService.GetString("Status.Ready");
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
            
            // Salviamo il path nelle impostazioni prima di lanciare
            _configService.Global.ClientPath = ClientPath;
            _configService.Global.DataPath = DataPath;
            _configService.Save();

            // Logica di lancio tramite il servizio interop
            _clientInterop.LaunchClient(ClientPath, "Crypt.dll");
        }
    }
}
