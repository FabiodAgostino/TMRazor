using System.Windows.Controls;
using TMRazorImproved.UI.ViewModels;

namespace TMRazorImproved.UI.Views.Pages
{
    public partial class SoundPage : Page
    {
        public SoundViewModel ViewModel { get; }

        public SoundPage(SoundViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = ViewModel;

            InitializeComponent();
        }
    }
}
