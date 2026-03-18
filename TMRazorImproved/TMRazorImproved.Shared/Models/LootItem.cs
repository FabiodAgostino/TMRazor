using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Linq;

namespace TMRazorImproved.Shared.Models
{
    public partial class LootItem : ObservableObject
    {
        [ObservableProperty]
        private int _graphic;

        [ObservableProperty]
        private int _color = -1; // -1 = any color

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private bool _isEnabled = true;

        [ObservableProperty]
        private int _amount = -1; // -1 = any amount (for organizer)

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FormattedProperties))]
        private List<PropertyFilter> _propertyFilters = new();

        public LootItem() { }

        public LootItem(int graphic, int color, string name)
        {
            Graphic = graphic;
            Color = color;
            Name = name;
        }

        public string FormattedGraphic => $"0x{Graphic:X4}";
        public string FormattedColor => Color == -1 ? "Any" : $"0x{Color:X4}";
        public string FormattedProperties => PropertyFilters.Count > 0 ? string.Join(", ", PropertyFilters.Select(pf => pf.ToString())) : "Any";
    }
}
