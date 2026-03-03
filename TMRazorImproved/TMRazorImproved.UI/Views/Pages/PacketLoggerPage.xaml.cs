using System;
using System.Windows.Controls;
using TMRazorImproved.UI.ViewModels;

namespace TMRazorImproved.UI.Views.Pages
{
    public partial class PacketLoggerPage : Page
    {
        public PacketLoggerViewModel ViewModel { get; }

        public PacketLoggerPage(PacketLoggerViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
