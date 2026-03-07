using System.Windows.Controls;
using TMRazorImproved.UI.ViewModels;

namespace TMRazorImproved.UI.Views.Pages
{
    public partial class GumpListPage : Page
    {
        public GumpListViewModel ViewModel { get; }

        public GumpListPage(GumpListViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this.ViewModel;
            InitializeComponent();
        }
    }
}
