using System.Windows;
using System.Windows.Input;
using TMRazorImproved.UI.ViewModels;

namespace TMRazorImproved.UI.Views.Windows
{
    public partial class TargetHPWindow : Window
    {
        private readonly TargetHPViewModel _viewModel;

        public TargetHPWindow(TargetHPViewModel viewModel)
        {
            _viewModel = viewModel;
            DataContext = viewModel;
            InitializeComponent();

            Loaded += TargetHPWindow_Loaded;
            LocationChanged += TargetHPWindow_LocationChanged;
            
            Closed += (s, e) => {
                _viewModel.Save();
                _viewModel.Dispose();
            };
        }

        private void TargetHPWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (!double.IsNaN(_viewModel.WindowX))
                Left = _viewModel.WindowX;
            
            if (!double.IsNaN(_viewModel.WindowY))
                Top = _viewModel.WindowY;
        }

        private void TargetHPWindow_LocationChanged(object? sender, System.EventArgs e)
        {
            _viewModel.WindowX = Left;
            _viewModel.WindowY = Top;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
    }
}
