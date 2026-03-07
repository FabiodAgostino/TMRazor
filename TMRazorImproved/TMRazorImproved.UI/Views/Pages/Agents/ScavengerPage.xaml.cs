using System.Windows.Controls;
using TMRazorImproved.UI.ViewModels.Agents;

namespace TMRazorImproved.UI.Views.Pages.Agents
{
    public partial class ScavengerPage
    {
        public ScavengerViewModel ViewModel { get; }

        public ScavengerPage(ScavengerViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this.ViewModel;

            InitializeComponent();
        }
    }
}
