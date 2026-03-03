using System.Windows;
using System.Windows.Input;
using TMRazorImproved.UI.ViewModels;

namespace TMRazorImproved.UI.Views.Windows
{
    public partial class TargetHPWindow : Window
    {
        public TargetHPWindow(TargetHPViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
    }
}
