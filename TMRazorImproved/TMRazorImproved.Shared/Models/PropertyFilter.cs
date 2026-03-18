using CommunityToolkit.Mvvm.ComponentModel;

namespace TMRazorImproved.Shared.Models
{
    public partial class PropertyFilter : ObservableObject
    {
        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private double _minValue;

        [ObservableProperty]
        private double _maxValue;

        public PropertyFilter() { }

        public PropertyFilter(string name, double min, double max)
        {
            _name = name;
            _minValue = min;
            _maxValue = max;
        }

        public override string ToString() => $"{Name} ({MinValue}-{MaxValue})";
    }
}
