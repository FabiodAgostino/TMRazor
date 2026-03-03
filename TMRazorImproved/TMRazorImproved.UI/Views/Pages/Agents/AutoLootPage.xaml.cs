using System.Windows.Controls;
using TMRazorImproved.UI.ViewModels.Agents;

namespace TMRazorImproved.UI.Views.Pages.Agents
{
    public partial class AutoLootPage
    {
        public AutoLootViewModel ViewModel { get; }

        public AutoLootPage(AutoLootViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
