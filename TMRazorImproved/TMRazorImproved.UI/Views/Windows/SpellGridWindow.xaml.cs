using System.Windows;
using System.Windows.Input;
using TMRazorImproved.UI.ViewModels;
using Wpf.Ui.Controls;

namespace TMRazorImproved.UI.Views.Windows
{
    public partial class SpellGridWindow : FluentWindow
    {
        private readonly SpellGridViewModel _viewModel;

        public SpellGridWindow(SpellGridViewModel viewModel)
        {
            _viewModel = viewModel;
            DataContext = viewModel;
            InitializeComponent();

            Loaded += SpellGridWindow_Loaded;
            LocationChanged += SpellGridWindow_LocationChanged;

            // Abilita trascinamento della finestra
            RootGrid.MouseDown += (s, e) => {
                if (e.ChangedButton == MouseButton.Left)
                    DragMove();
            };

            // Dispone il ViewModel (ferma il timer cooldown) quando la finestra viene chiusa
            Closed += (_, _) => {
                _viewModel.Save();
                _viewModel.Dispose();
            };
        }

        private void SpellGridWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (!double.IsNaN(_viewModel.WindowX))
                Left = _viewModel.WindowX;
            
            if (!double.IsNaN(_viewModel.WindowY))
                Top = _viewModel.WindowY;
        }

        private void SpellGridWindow_LocationChanged(object? sender, System.EventArgs e)
        {
            _viewModel.WindowX = Left;
            _viewModel.WindowY = Top;
        }
    }
}

