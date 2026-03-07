using System.Windows.Controls;
using TMRazorImproved.UI.ViewModels.Agents;

namespace TMRazorImproved.UI.Views.Pages.Agents
{
    public partial class TargetingPage
    {
        public TargetingViewModel ViewModel { get; }

        public TargetingPage(TargetingViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this.ViewModel;

            InitializeComponent();
        }
    }
}
