using System.Windows.Controls;
using TMRazorImproved.UI.ViewModels;

namespace TMRazorImproved.UI.Views.Pages
{
    public partial class OptionsPage : Page
    {
        public OptionsViewModel ViewModel { get; }

        public OptionsPage(OptionsViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = viewModel;

            InitializeComponent();
        }
    }
}
