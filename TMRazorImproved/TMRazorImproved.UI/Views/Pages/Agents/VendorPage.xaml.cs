using System.Windows.Controls;
using TMRazorImproved.UI.ViewModels.Agents;

namespace TMRazorImproved.UI.Views.Pages.Agents
{
    public partial class VendorPage
    {
        public VendorViewModel ViewModel { get; }

        public VendorPage(VendorViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
