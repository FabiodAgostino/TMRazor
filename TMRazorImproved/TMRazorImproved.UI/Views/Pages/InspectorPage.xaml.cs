using TMRazorImproved.UI.ViewModels;
using Wpf.Ui.Controls;

namespace TMRazorImproved.UI.Views.Pages
{
    public partial class InspectorPage : INavigableView<InspectorViewModel>
    {
        public InspectorViewModel ViewModel { get; }

        public InspectorPage(InspectorViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
