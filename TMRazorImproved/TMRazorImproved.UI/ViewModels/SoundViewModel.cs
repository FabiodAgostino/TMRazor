using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TMRazorImproved.Shared.Interfaces;

namespace TMRazorImproved.UI.ViewModels
{
    public partial class SoundViewModel : ViewModelBase
    {
        private readonly ISoundService _soundService;

        [ObservableProperty]
        private ushort _soundId;

        [ObservableProperty]
        private ushort _musicId;

        public SoundViewModel(ISoundService soundService)
        {
            _soundService = soundService;
        }

        [RelayCommand]
        private void PlaySound()
        {
            _soundService.PlaySound(SoundId);
        }

        [RelayCommand]
        private void PlayMusic()
        {
            _soundService.PlayMusic(MusicId);
        }

        [RelayCommand]
        private void StopMusic()
        {
            _soundService.StopMusic();
        }
    }
}
