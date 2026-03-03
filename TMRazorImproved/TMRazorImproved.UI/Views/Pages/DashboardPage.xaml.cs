using System;
using System.Windows;
using System.Windows.Controls;
using TMRazorImproved.Shared.Interfaces;

namespace TMRazorImproved.UI.Views.Pages
{
    public partial class DashboardPage : Page
    {
        private readonly IClientInteropService _clientInterop;

        public DashboardPage()
        {
            InitializeComponent();
            
            // Recuperiamo il servizio dal container DI tramite l'App (approccio rapido per test)
            _clientInterop = App.GetService<IClientInteropService>();

            Loaded += OnPageLoaded;
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            try 
            {
                IntPtr handle = _clientInterop.FindUOWindow();
                // Se non crasha qui, il layer C++ (x86) è caricato correttamente!
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore caricamento DLL native: {ex.Message}", "Native Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
