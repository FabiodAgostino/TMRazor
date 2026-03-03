using CommunityToolkit.Mvvm.ComponentModel;

namespace TMRazorImproved.UI.ViewModels
{
    public partial class CounterStatus : ObservableObject
    {
        [ObservableProperty] private string _abbreviation = string.Empty;
        [ObservableProperty] private int _count;
        [ObservableProperty] private ushort _graphic;
        [ObservableProperty] private ushort _hue;
    }
}
