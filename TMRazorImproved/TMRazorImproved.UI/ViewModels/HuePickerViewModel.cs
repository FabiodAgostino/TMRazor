using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ultima;

namespace TMRazorImproved.UI.ViewModels
{
    public partial class HuePickerViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ObservableCollection<HueItemViewModel> _hues = new();

        [ObservableProperty]
        private HueItemViewModel? _selectedHue;

        [ObservableProperty]
        private string _searchText = string.Empty;

        private readonly List<HueItemViewModel> _allHues = new();

        public HuePickerViewModel()
        {
            LoadHues();
        }

        private void LoadHues()
        {
            if (Ultima.Hues.List == null) return;

            foreach (var hue in Ultima.Hues.List)
            {
                if (hue == null) continue;
                
                var hueVm = new HueItemViewModel(hue);
                _allHues.Add(hueVm);
                Hues.Add(hueVm);
            }
        }

        [RelayCommand]
        private void Search()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                Hues = new ObservableCollection<HueItemViewModel>(_allHues);
                return;
            }

            var filtered = _allHues.Where(h => 
                h.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) || 
                h.Index.ToString().Contains(SearchText)).ToList();
            
            Hues = new ObservableCollection<HueItemViewModel>(filtered);
        }

        partial void OnSearchTextChanged(string value)
        {
            Search();
        }
    }

    public class HueItemViewModel : ObservableObject
    {
        public int Index { get; }
        public string Name { get; }
        public Brush PreviewBrush { get; }

        public HueItemViewModel(Hue hue)
        {
            Index = hue.Index;
            Name = string.IsNullOrWhiteSpace(hue.Name) || hue.Name == "Null" ? $"Hue {hue.Index}" : hue.Name;
            
            // Usiamo il colore centrale della tabella per la preview
            var color = hue.GetColor(16);
            PreviewBrush = new SolidColorBrush(Color.FromArgb(255, (byte)color.R, (byte)color.G, (byte)color.B));
            PreviewBrush.Freeze();
        }
    }
}
