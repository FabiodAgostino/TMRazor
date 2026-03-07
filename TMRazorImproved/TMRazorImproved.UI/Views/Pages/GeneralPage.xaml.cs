using System.Windows.Controls;
using TMRazorImproved.UI.ViewModels;
using Wpf.Ui.Controls;

namespace TMRazorImproved.UI.Views.Pages
{
    public partial class GeneralPage : Page
    {
        public GeneralViewModel ViewModel { get; }

        public GeneralPage(GeneralViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = ViewModel;

            InitializeComponent();
        }
    }
}
