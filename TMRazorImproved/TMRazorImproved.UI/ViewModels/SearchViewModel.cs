using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.UI.ViewModels
{
    public partial class SearchViewModel : ViewModelBase
    {
        private readonly ISearchService _searchService;

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        [ObservableProperty]
        private ObservableCollection<SearchItem> _searchResults = new();

        [ObservableProperty]
        private SearchItem? _selectedItem;

        [ObservableProperty]
        private bool _isOverlayVisible;

        public SearchViewModel(ISearchService searchService)
        {
            _searchService = searchService;
        }

        partial void OnSearchQueryChanged(string value)
        {
            UpdateResults();
        }

        private void UpdateResults()
        {
            var results = _searchService.Search(SearchQuery);
            SearchResults.Clear();
            foreach (var item in results)
            {
                SearchResults.Add(item);
            }

            if (SearchResults.Count > 0)
            {
                SelectedItem = SearchResults[0];
            }
            else
            {
                SelectedItem = null;
            }
        }

        [RelayCommand]
        public void ToggleOverlay()
        {
            IsOverlayVisible = !IsOverlayVisible;
            if (IsOverlayVisible)
            {
                SearchQuery = string.Empty;
                UpdateResults();
            }
        }

        [RelayCommand]
        public void ExecuteSelected()
        {
            if (SelectedItem != null)
            {
                var cmd = SelectedItem.Command;
                var param = SelectedItem.Parameter;
                
                // Chiude prima l'overlay per evitare sovrapposizioni visive
                IsOverlayVisible = false;
                
                // Esegue il comando (es. navigazione a una pagina)
                cmd.Execute(param);
            }
        }

        [RelayCommand]
        public void CloseOverlay()
        {
            IsOverlayVisible = false;
        }
    }
}
