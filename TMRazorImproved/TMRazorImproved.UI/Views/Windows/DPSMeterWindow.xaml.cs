using System.Windows;
using System.Windows.Input;
using TMRazorImproved.UI.ViewModels;

namespace TMRazorImproved.UI.Views.Windows
{
    public partial class DPSMeterWindow : Window
    {
        public DPSMeterViewModel ViewModel { get; }

        public DPSMeterWindow(DPSMeterViewModel viewModel)
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
