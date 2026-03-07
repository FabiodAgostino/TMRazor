using System.Windows.Controls;
using TMRazorImproved.UI.ViewModels.Agents;

namespace TMRazorImproved.UI.Views.Pages.Agents
{
    public partial class RestockPage : Page
    {
        public RestockViewModel ViewModel { get; }

        public RestockPage(RestockViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this.ViewModel;
            InitializeComponent();
        }
    }
}
