using System.Windows;
using TMRazorImproved.UI.ViewModels;
using Wpf.Ui.Controls;

namespace TMRazorImproved.UI.Views.Windows
{
    public partial class HuePickerWindow : FluentWindow
    {
        public HuePickerViewModel ViewModel { get; }

        public HuePickerWindow(HuePickerViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this.ViewModel;
            InitializeComponent();
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedHue != null)
            {
                DialogResult = true;
                Close();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
