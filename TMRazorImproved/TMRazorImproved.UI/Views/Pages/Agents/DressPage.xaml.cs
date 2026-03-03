using System.Windows.Controls;
using TMRazorImproved.UI.ViewModels.Agents;

namespace TMRazorImproved.UI.Views.Pages.Agents
{
    public partial class DressPage
    {
        public DressViewModel ViewModel { get; }

        public DressPage(DressViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
