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

        public MainWindow(
            INavigationService navigationService,
            IPageService pageService,
            IPacketService packetService)
        {
            _navigationService = navigationService;
            _packetService = packetService;

            InitializeComponent();

            // Collega il NavigationView al servizio di navigazione
            _navigationService.SetNavigationControl(RootNavigation);
            RootNavigation.SetPageService(pageService);

            Loaded += OnLoaded;
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
