using CommunityToolkit.Mvvm.ComponentModel;

namespace TMRazorImproved.Shared.Models
{
    public partial class SpellIcon : ObservableObject
    {
        [ObservableProperty] private int _spellId;
        [ObservableProperty] private string _name = string.Empty;
        [ObservableProperty] private string _iconPath = string.Empty;
        [ObservableProperty] private string _category = "Magery";
        
        [ObservableProperty] private double _cooldownSeconds;
        [ObservableProperty] private bool _isOnCooldown;
        [ObservableProperty] private double _remainingCooldown;
    }
}
