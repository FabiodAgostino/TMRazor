using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TMRazorImproved.Shared.Interfaces;

namespace TMRazorImproved.UI.ViewModels
{
    public partial class SoundViewModel : ViewModelBase
    {
        private readonly ISoundService _soundService;

        [ObservableProperty] private ushort _soundId;
        [ObservableProperty] private ushort _musicId;

        /// <summary>Volume del client UO in percentuale (0-100).</summary>
        [ObservableProperty] private double _volume = 100;

        public SoundViewModel(ISoundService soundService)
        {
            _soundService = soundService;
            // Leggi il volume corrente del processo UO se già avviato
            _volume = _soundService.GetVolume() * 100.0;
        }

        partial void OnVolumeChanged(double value)
        {
            _soundService.SetVolume((float)(value / 100.0));
        }

        [RelayCommand]
        private void PlaySound() => _soundService.PlaySound(SoundId);

        [RelayCommand]
        private void PlayMusic() => _soundService.PlayMusic(MusicId);

        [RelayCommand]
        private void StopMusic() => _soundService.StopMusic();
    }
}
