using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TMRazorImproved.UI.ViewModels;
using Wpf.Ui.Controls;

namespace TMRazorImproved.UI.Views.Pages
{
    public partial class HotkeysPage : INavigableView<HotkeysViewModel>
    {
        public HotkeysViewModel ViewModel { get; }

        public HotkeysPage(HotkeysViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is HotkeyActionNode node && node.Action != null)
            {
                ViewModel.SelectedActionName = node.Action;
            }
            else
            {
                ViewModel.SelectedActionName = "None";
            }
        }

        private void Page_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (ViewModel.IsCapturing)
            {
                // Gestione dei tasti speciali (Alt, ecc.)
                Key key = (e.Key == Key.System) ? e.SystemKey : e.Key;
                
                ViewModel.HandleCapturedKey(key, Keyboard.Modifiers);
                
                e.Handled = true;
            }
        }
    }
}
