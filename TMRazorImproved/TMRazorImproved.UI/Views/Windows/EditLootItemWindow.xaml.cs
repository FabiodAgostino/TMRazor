using System.Windows;
using TMRazorImproved.UI.ViewModels.Windows;
using Wpf.Ui.Controls;

namespace TMRazorImproved.UI.Views.Windows
{
    public partial class EditLootItemWindow : FluentWindow
    {
        public EditLootItemViewModel ViewModel { get; }

        public EditLootItemWindow(EditLootItemViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this.ViewModel;
            InitializeComponent();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Save();
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
