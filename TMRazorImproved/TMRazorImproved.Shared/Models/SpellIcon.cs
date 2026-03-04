using CommunityToolkit.Mvvm.ComponentModel;

namespace TMRazorImproved.Shared.Models
{
    public partial class SpellIcon : ObservableObject
    {
        public int SpellId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public double CooldownSeconds { get; set; } = 1.0;

        [ObservableProperty]
        private bool _isOnCooldown;

        [ObservableProperty]
        private double _remainingCooldown;

        [ObservableProperty]
        private bool _isVisible = true;

        [ObservableProperty]
        private int _row;

        [ObservableProperty]
        private int _column;

        public double CooldownProgress => CooldownSeconds > 0 ? (RemainingCooldown / CooldownSeconds) * 100 : 0;

        // Quando RemainingCooldown cambia (via [ObservableProperty] setter), notifica anche CooldownProgress
        // perché è una proprietà calcolata e XAML non sa che dipende da RemainingCooldown.
        partial void OnRemainingCooldownChanged(double value)
        {
            OnPropertyChanged(nameof(CooldownProgress));
        }
    }
}
