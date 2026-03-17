using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models.Config;

namespace TMRazorImproved.UI.ViewModels
{
    public partial class DisplayViewModel : ViewModelBase
    {
        private readonly IConfigService _config;
        private readonly ITitleBarService _titleBar;
        private readonly ITargetingService _targeting;
        private readonly ILanguageService _lang;

        [ObservableProperty] private bool _titleBarEnabled;
        [ObservableProperty] private string _titleBarTemplate = string.Empty;

        [ObservableProperty] private bool _showNames;
        [ObservableProperty] private bool _showHealth;
        [ObservableProperty] private bool _highlightTarget;
        [ObservableProperty] private bool _incomingNames;
        [ObservableProperty] private bool _showIncomingDamage;

        public ObservableCollection<CounterDefinition> Counters { get; } = new();

        public DisplayViewModel(
            IConfigService config,
            ITitleBarService titleBar,
            ITargetingService targeting,
            ILanguageService languageService)
        {
            _config = config;
            _titleBar = titleBar;
            _targeting = targeting;
            _lang = languageService;

            LoadFromConfig();
        }

        private void LoadFromConfig()
        {
            var profile = _config.CurrentProfile;
            TitleBarEnabled = profile.TitleBarEnabled;
            TitleBarTemplate = profile.TitleBarTemplate;

            ShowNames = profile.ShowNames;
            ShowHealth = profile.ShowHealth;
            HighlightTarget = profile.HighlightTarget;
            IncomingNames = profile.IncomingNames;
            ShowIncomingDamage = profile.ShowIncomingDamage;

            Counters.Clear();
            foreach (var counter in profile.Counters)
            {
                Counters.Add(counter);
            }
        }

        partial void OnTitleBarEnabledChanged(bool value)
        {
            _config.CurrentProfile.TitleBarEnabled = value;
            _titleBar.IsEnabled = value;
            _config.Save();
        }

        partial void OnTitleBarTemplateChanged(string value)
        {
            _config.CurrentProfile.TitleBarTemplate = value;
            _titleBar.Template = value;
            _config.Save();
        }

        partial void OnShowNamesChanged(bool value) { _config.CurrentProfile.ShowNames = value; _config.Save(); }
        partial void OnShowHealthChanged(bool value) { _config.CurrentProfile.ShowHealth = value; _config.Save(); }
        partial void OnHighlightTargetChanged(bool value) { _config.CurrentProfile.HighlightTarget = value; _config.Save(); }
        partial void OnIncomingNamesChanged(bool value) { _config.CurrentProfile.IncomingNames = value; _config.Save(); }
        partial void OnShowIncomingDamageChanged(bool value) { _config.CurrentProfile.ShowIncomingDamage = value; _config.Save(); }

        [RelayCommand]
        private void AddCounter()
        {
            var newCounter = new CounterDefinition { Name = "New Counter", Graphic = 0x0EED };
            _config.CurrentProfile.Counters.Add(newCounter);
            Counters.Add(newCounter);
            _config.Save();
        }

        [RelayCommand]
        private void RemoveCounter(CounterDefinition? counter)
        {
            if (counter == null) return;
            _config.CurrentProfile.Counters.Remove(counter);
            Counters.Remove(counter);
            _config.Save();
        }

        [RelayCommand]
        private async Task SetCounterGraphicAsync(CounterDefinition? counter)
        {
            if (counter == null) return;
            var target = await _targeting.AcquireTargetAsync();
            if (target.Serial != 0)
            {
                // In a real scenario we'd get the Graphic from WorldService using the serial
                // But since we are in UI, we might need a way to get it.
                // For now just a placeholder serial-as-graphic or similar if we don't have item info here.
            }
        }
    }
}
