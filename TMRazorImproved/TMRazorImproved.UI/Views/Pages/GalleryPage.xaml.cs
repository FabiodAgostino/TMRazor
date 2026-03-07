using System;
using System.Windows.Controls;
using TMRazorImproved.UI.ViewModels;

namespace TMRazorImproved.UI.Views.Pages
{
    public partial class GalleryPage : Page
    {
        public GalleryViewModel ViewModel { get; }

        public GalleryPage(GalleryViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = ViewModel;

            InitializeComponent();
        }
    }
}
