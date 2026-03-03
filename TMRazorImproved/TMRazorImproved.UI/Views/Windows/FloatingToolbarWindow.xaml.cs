using System.Windows;
using System.Windows.Input;
using TMRazorImproved.UI.ViewModels;

namespace TMRazorImproved.UI.Views.Windows
{
    public partial class FloatingToolbarWindow : Window
    {
        public FloatingToolbarViewModel ViewModel { get; }

        public FloatingToolbarWindow(FloatingToolbarViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = ViewModel;
            InitializeComponent();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }
    }
}
