using System.Windows.Controls;
using TMRazorImproved.UI.ViewModels;

namespace TMRazorImproved.UI.Views.Pages.Agents
{
    public partial class FriendsPage : Page
    {
        public FriendsViewModel ViewModel { get; }

        public FriendsPage(FriendsViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
