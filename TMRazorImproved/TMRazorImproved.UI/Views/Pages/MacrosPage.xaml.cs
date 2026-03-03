using System.Windows.Controls;
using TMRazorImproved.UI.ViewModels;
using Wpf.Ui.Controls;

namespace TMRazorImproved.UI.Views.Pages
{
    public partial class MacrosPage : Page
    {
        public MacrosViewModel ViewModel { get; }

        public MacrosPage(MacrosViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
