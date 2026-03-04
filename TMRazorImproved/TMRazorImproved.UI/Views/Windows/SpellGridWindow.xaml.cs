using System.Windows;
using System.Windows.Input;
using TMRazorImproved.UI.ViewModels;
using Wpf.Ui.Controls;

namespace TMRazorImproved.UI.Views.Windows
{
    public partial class SpellGridWindow : FluentWindow
    {
        public SpellGridWindow(SpellGridViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();

            // Abilita trascinamento della finestra
            RootGrid.MouseDown += (s, e) => {
                if (e.ChangedButton == MouseButton.Left)
                    DragMove();
            };

            // Dispone il ViewModel (ferma il timer cooldown) quando la finestra viene chiusa
            Closed += (_, _) => viewModel.Dispose();
        }
    }
}
