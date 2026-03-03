using System.Windows;
using System.Windows.Input;
using TMRazorImproved.UI.ViewModels;

namespace TMRazorImproved.UI.Views.Windows
{
    public partial class SpellGridWindow : Window
    {
        public SpellGridViewModel ViewModel { get; }

        public SpellGridWindow(SpellGridViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}
