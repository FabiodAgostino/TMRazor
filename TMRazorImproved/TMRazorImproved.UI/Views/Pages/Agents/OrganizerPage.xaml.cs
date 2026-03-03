using System.Windows.Controls;
using TMRazorImproved.UI.ViewModels.Agents;

namespace TMRazorImproved.UI.Views.Pages.Agents
{
    public partial class OrganizerPage
    {
        public OrganizerViewModel ViewModel { get; }

        public OrganizerPage(OrganizerViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
