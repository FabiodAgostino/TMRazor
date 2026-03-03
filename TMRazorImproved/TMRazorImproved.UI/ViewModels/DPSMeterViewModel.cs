using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using TMRazorImproved.Shared.Interfaces;

namespace TMRazorImproved.UI.ViewModels
{
    public partial class DPSMeterViewModel : ViewModelBase
    {
        private readonly IDPSMeterService _dpsMeter;

        [ObservableProperty] private double _currentDPS;
        [ObservableProperty] private double _maxDPS;
        [ObservableProperty] private long _totalDamage;
        [ObservableProperty] private string _combatTime = "00:00";
        [ObservableProperty] private bool _isActive;

        public DPSMeterViewModel(IDPSMeterService dpsMeter)
        {
            _dpsMeter = dpsMeter;
            _dpsMeter.Updated += OnUpdated;
        }

        private void OnUpdated()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                CurrentDPS = _dpsMeter.CurrentDPS;
                MaxDPS = _dpsMeter.MaxDPS;
                TotalDamage = _dpsMeter.TotalDamage;
                CombatTime = _dpsMeter.CombatTime.ToString(@"mm\:ss");
                IsActive = _dpsMeter.IsActive;
            });
        }

        [RelayCommand]
        private void Reset()
        {
            _dpsMeter.Reset();
        }

        [RelayCommand]
        private void Toggle()
        {
            if (_dpsMeter.IsActive) _dpsMeter.Stop();
            else _dpsMeter.Start();
        }
    }
}
