using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models.Config;
using System.Collections.Generic;
using Microsoft.Win32;
using System.Threading.Tasks;

namespace TMRazorImproved.UI.ViewModels
{
    public partial class MediaViewModel : ViewModelBase
    {
        private readonly IConfigService _config;
        private readonly IScreenCaptureService _screenCapture;
        private readonly IVideoCaptureService _videoCapture;
        private readonly ILanguageService _language;

        [ObservableProperty] private string _screenshotPath;
        [ObservableProperty] private string _videoPath;
        [ObservableProperty] private string _screenshotFormat;
        [ObservableProperty] private int _screenshotQuality;
        [ObservableProperty] private int _videoFps;
        [ObservableProperty] private string _videoCodec;
        [ObservableProperty] private bool _autoScreenshotOnDeath;

        public List<string> Formats { get; } = new() { "JPG", "PNG" };
        public List<string> Codecs { get; } = new() { "Uncompressed" };

        public MediaViewModel(IConfigService config, IScreenCaptureService screenCapture, IVideoCaptureService videoCapture, ILanguageService language)
        {
            _config = config;
            _screenCapture = screenCapture;
            _videoCapture = videoCapture;
            _language = language;

            var media = _config.CurrentProfile.Media;
            _screenshotPath = media.ScreenshotPath;
            _videoPath = media.VideoPath;
            _screenshotFormat = media.ScreenshotFormat;
            _screenshotQuality = media.ScreenshotQuality;
            _videoFps = media.VideoFps;
            _videoCodec = media.VideoCodec;
            _autoScreenshotOnDeath = _config.CurrentProfile.AutoScreenshotOnDeath;
        }

        partial void OnScreenshotPathChanged(string value) => _config.CurrentProfile.Media.ScreenshotPath = value;
        partial void OnVideoPathChanged(string value) => _config.CurrentProfile.Media.VideoPath = value;
        partial void OnScreenshotFormatChanged(string value) => _config.CurrentProfile.Media.ScreenshotFormat = value;
        partial void OnScreenshotQualityChanged(int value) => _config.CurrentProfile.Media.ScreenshotQuality = value;
        partial void OnVideoFpsChanged(int value) => _config.CurrentProfile.Media.VideoFps = value;
        partial void OnVideoCodecChanged(string value) => _config.CurrentProfile.Media.VideoCodec = value;
        partial void OnAutoScreenshotOnDeathChanged(bool value) => _config.CurrentProfile.AutoScreenshotOnDeath = value;

        [RelayCommand]
        private void BrowseScreenshotPath()
        {
            var dialog = new OpenFolderDialog();
            if (dialog.ShowDialog() == true)
            {
                ScreenshotPath = dialog.FolderName;
            }
        }

        [RelayCommand]
        private void BrowseVideoPath()
        {
            var dialog = new OpenFolderDialog();
            if (dialog.ShowDialog() == true)
            {
                VideoPath = dialog.FolderName;
            }
        }

        [RelayCommand]
        private async Task TakeScreenshotAsync()
        {
            string path = await _screenCapture.CaptureAsync();
            if (!string.IsNullOrEmpty(path))
            {
                StatusText = _language.GetString("Media.ScreenshotTaken");
            }
        }
    }
}
