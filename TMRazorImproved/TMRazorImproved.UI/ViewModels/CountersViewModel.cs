using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models.Config;

namespace TMRazorImproved.UI.ViewModels
{
    public partial class CountersViewModel : ViewModelBase
    {
        private readonly IConfigService _config;
        private readonly ICounterService _counterService;
        private readonly ITargetingService _targeting;

        public ObservableCollection<CounterDefinition> Counters { get; }

        public CountersViewModel(IConfigService config, ICounterService counterService, ITargetingService targeting)
        {
            _config = config;
            _counterService = counterService;
            _targeting = targeting;
            Counters = new ObservableCollection<CounterDefinition>(_config.CurrentProfile.Counters);
        }

        [RelayCommand]
        private void AddCounter()
        {
            var counter = new CounterDefinition { Name = "New Counter", Abbreviation = "NEW" };
            _config.CurrentProfile.Counters.Add(counter);
            Counters.Add(counter);
            _config.Save();
        }

        [RelayCommand]
        private void RemoveCounter(CounterDefinition counter)
        {
            if (counter != null)
            {
                _config.CurrentProfile.Counters.Remove(counter);
                Counters.Remove(counter);
                _config.Save();
            }
        }

        [RelayCommand]
        private async Task SetCounterGraphic(CounterDefinition counter)
        {
            var targetInfo = await _targeting.AcquireTargetAsync(); uint serial = targetInfo.Serial;
            if (serial != 0)
            {
                var item = App.GetService<IWorldService>()?.FindItem(serial);
                if (item != null)
                {
                    counter.Graphic = item.Graphic;
                    counter.Hue = item.Hue;
                    _config.Save();
                    // Force refresh of the item in the list
                    int index = Counters.IndexOf(counter);
                    if (index != -1) { Counters.RemoveAt(index); Counters.Insert(index, counter); }
                }
            }
        }

        [RelayCommand]
        private void Recalculate()
        {
            _counterService.RecalculateAll();
        }
    }
}
