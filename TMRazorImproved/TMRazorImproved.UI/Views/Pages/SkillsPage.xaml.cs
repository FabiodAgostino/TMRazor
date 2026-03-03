using System.Windows.Controls;
using TMRazorImproved.UI.ViewModels;

namespace TMRazorImproved.UI.Views.Pages
{
    // FIX BUG-P1-04: file code-behind mancante per SkillsPage.
    // Senza questo file la pagina non poteva iniettare il ViewModel né impostare il DataContext,
    // rendendo tutti i binding XAML silenziosamente vuoti.
    public partial class SkillsPage : Page
    {
        public SkillsViewModel ViewModel { get; }

        public SkillsPage(SkillsViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
