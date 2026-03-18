using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.UI.ViewModels.Windows
{
    public partial class EditLootItemViewModel : ViewModelBase
    {
        [ObservableProperty]
        private LootItem _item;

        public ObservableCollection<PropertyFilter> PropertyFilters { get; } = new();

        public IRelayCommand AddPropertyCommand { get; }
        public IRelayCommand RemovePropertyCommand { get; }

        public EditLootItemViewModel(LootItem item)
        {
            _item = item;
            foreach (var pf in item.PropertyFilters)
            {
                PropertyFilters.Add(new PropertyFilter(pf.Name, pf.MinValue, pf.MaxValue));
            }

            AddPropertyCommand = new RelayCommand(AddProperty);
            RemovePropertyCommand = new RelayCommand<PropertyFilter>(RemoveProperty);
        }

        private void AddProperty()
        {
            PropertyFilters.Add(new PropertyFilter("New Property", 0, 100));
        }

        private void RemoveProperty(PropertyFilter? pf)
        {
            if (pf != null)
            {
                PropertyFilters.Remove(pf);
            }
        }

        public void Save()
        {
            Item.PropertyFilters.Clear();
            foreach (var pf in PropertyFilters)
            {
                Item.PropertyFilters.Add(pf);
            }
        }
    }
}
