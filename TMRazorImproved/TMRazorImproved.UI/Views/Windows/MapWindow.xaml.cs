using TMRazorImproved.UI.ViewModels;
using Wpf.Ui.Controls;

namespace TMRazorImproved.UI.Views.Windows
{
    public partial class MapWindow : FluentWindow
    {
        public MapWindow(MapViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
