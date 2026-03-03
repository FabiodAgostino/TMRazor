using System;
using System.Windows;
using System.Windows.Controls;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.UI.ViewModels;

namespace TMRazorImproved.UI.Views.Pages
{
    public partial class DashboardPage : Page
    {
        private readonly IClientInteropService _clientInterop;
        private readonly DashboardViewModel _viewModel;

        public DashboardPage(DashboardViewModel viewModel, IClientInteropService clientInterop)
        {
            _viewModel = viewModel;
            _clientInterop = clientInterop;
            
            DataContext = _viewModel;
            InitializeComponent();

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
