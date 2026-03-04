using System;
using System.Windows;
using System.Windows.Interop;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.UI.Views.Pages;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace TMRazorImproved.UI
{
    public partial class MainWindow : FluentWindow
    {
        private readonly INavigationService _navigationService;
        private readonly IPacketService _packetService;
        private readonly ITitleBarService _titleBarService;
        private readonly ISnackbarService _snackbarService;
        private readonly IScriptingService _scriptingService;

        public MainWindow(
            INavigationService navigationService,
            IPageService pageService,
            IPacketService packetService,
            ITitleBarService titleBarService,
            ISnackbarService snackbarService,
            IScriptingService scriptingService)
        {
            _navigationService = navigationService;
            _packetService = packetService;
            _titleBarService = titleBarService;
            _snackbarService = snackbarService;
            _scriptingService = scriptingService;

            InitializeComponent();

            // Collega il NavigationView al servizio di navigazione
            _navigationService.SetNavigationControl(RootNavigation);
            RootNavigation.SetPageService(pageService);

            // Collega lo SnackbarService al presentatore definito nel file XAML
            _snackbarService.SetSnackbarPresenter(SnackbarPresenter);

            // Sottoscrizione agli aggiornamenti del titolo dinamico
            _titleBarService.TitleChanged += OnTitleChanged;

            // Sottoscrizione agli eventi di scripting per notifiche UI
            _scriptingService.ErrorReceived += OnScriptError;
            _scriptingService.ScriptCompleted += OnScriptCompleted;

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnScriptError(string message)
        {
            Dispatcher.Invoke(() =>
            {
                _snackbarService.Show(
                    "Script Error",
                    message,
                    ControlAppearance.Danger,
                    new SymbolIcon(SymbolRegular.ErrorCircle24),
                    TimeSpan.FromSeconds(5));
            });
        }

        private void OnScriptCompleted(ScriptCompletionInfo info)
        {
            Dispatcher.Invoke(() =>
            {
                var title = info.WasCancelled ? "Script Stopped" : "Script Completed";
                var message = info.WasCancelled 
                    ? $"The script '{info.ScriptName}' was manually stopped." 
                    : $"The script '{info.ScriptName}' finished in {info.Elapsed.TotalSeconds:F2}s.";
                
                var appearance = info.WasCancelled ? ControlAppearance.Info : ControlAppearance.Success;
                var icon = info.WasCancelled ? SymbolRegular.Stop24 : SymbolRegular.CheckmarkCircle24;

                _snackbarService.Show(
                    title,
                    message,
                    appearance,
                    new SymbolIcon(icon),
                    TimeSpan.FromSeconds(3));
            });
        }

        private void OnTitleChanged(string newTitle)
        {
            // Il servizio gira su un background thread, l'aggiornamento UI deve avvenire sul Dispatcher
            Dispatcher.InvokeAsync(() =>
            {
                AppTitleBar.Title = newTitle;
                this.Title = newTitle; // Aggiorna anche il titolo della finestra per la taskbar
            });
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _titleBarService.TitleChanged -= OnTitleChanged;
            _scriptingService.ErrorReceived -= OnScriptError;
            _scriptingService.ScriptCompleted -= OnScriptCompleted;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            
            // Inizializza l'hook di basso livello per i messaggi di Windows
            var source = PresentationSource.FromVisual(this) as HwndSource;
            source?.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            try
            {
                // Diagnostica: Log dei messaggi di rete (WM_USER + 1 = 0x401)
                if (msg == 0x401)
                {
                    System.Diagnostics.Debug.WriteLine($"[WndProc] Received WM_UONETEVENT: wParam={wParam}, lParam={lParam}");
                }

                // Passa il messaggio al PacketService per la gestione delle comunicazioni native da Crypt.dll
                if (_packetService.OnMessage(hwnd, msg, wParam, lParam))
                {
                    handled = true;
                    return new IntPtr(1); // Ritorna un valore che indica che il messaggio è stato elaborato
                }
            }
            catch (Exception ex)
            {
                // Non rilanciare MAI dal WndProc — uccide il message pump.
                // In futuro sostituire con NLog quando integrato (vedi B9).
                System.Diagnostics.Debug.WriteLine($"[WndProc] {ex.GetType().Name}: {ex.Message}");
#if DEBUG
                System.Diagnostics.Debugger.Break();
#endif
            }

            return IntPtr.Zero;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Naviga alla Dashboard (o GeneralPage) come pagina iniziale solo quando l'interfaccia è pronta visivamente
            _navigationService.Navigate(typeof(GeneralPage));
        }
    }
}
