using System;
using System.Windows.Controls;
using TMRazorImproved.UI.ViewModels;

namespace TMRazorImproved.UI.Views.Pages
{
    public partial class SecureTradePage : Page
    {
        public SecureTradeViewModel ViewModel { get; }

        public SecureTradePage(SecureTradeViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = ViewModel;

            InitializeComponent();
        }
    }
}
