using System.Windows.Controls;
using TMRazorImproved.UI.ViewModels;

namespace TMRazorImproved.UI.Views.Pages
{
    public partial class FiltersPage : Page
    {
        public FiltersViewModel ViewModel { get; }

        public FiltersPage(FiltersViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = ViewModel;

            InitializeComponent();
        }
    }
}
