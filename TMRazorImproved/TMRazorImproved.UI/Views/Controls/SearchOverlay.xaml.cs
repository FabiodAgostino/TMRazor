using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TMRazorImproved.UI.ViewModels;

namespace TMRazorImproved.UI.Views.Controls
{
    public partial class SearchOverlay : UserControl
    {
        public SearchOverlay()
        {
            InitializeComponent();
            IsVisibleChanged += SearchOverlay_IsVisibleChanged;
        }

        private void SearchOverlay_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                // Focus the search box when it becomes visible
                SearchBox.Focus();
            }
        }

        private void OnBackgroundMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is SearchViewModel vm)
            {
                vm.CloseOverlayCommand.Execute(null);
            }
        }

        private void OnSearchBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is SearchViewModel vm)
            {
                if (e.Key == Key.Escape)
                {
                    vm.CloseOverlayCommand.Execute(null);
                }
                else if (e.Key == Key.Enter)
                {
                    vm.ExecuteSelectedCommand.Execute(null);
                }
            }
        }

        private void OnSearchBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is SearchViewModel vm)
            {
                // Navigate results with up/down arrows from the search box
                if (e.Key == Key.Down)
                {
                    int index = vm.SearchResults.IndexOf(vm.SelectedItem);
                    if (index < vm.SearchResults.Count - 1)
                        vm.SelectedItem = vm.SearchResults[index + 1];
                    e.Handled = true;
                }
                else if (e.Key == Key.Up)
                {
                    int index = vm.SearchResults.IndexOf(vm.SelectedItem);
                    if (index > 0)
                        vm.SelectedItem = vm.SearchResults[index - 1];
                    e.Handled = true;
                }
            }
        }
    }
}
