using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using TMRazorImproved.Shared.Interfaces;
using Wpf.Ui;

namespace TMRazorImproved.UI.ViewModels
{
    public partial class DashboardViewModel : ViewModelBase
    {
        private readonly IScreenCaptureService _screenCapture;
        private readonly IVideoCaptureService _videoCapture;
        private readonly ISnackbarService _snackbar;
        private readonly ILanguageService _languageService;

        [ObservableProperty]
        private bool _isRecording;

        [ObservableProperty]
        private string _selectedLanguage;

        public ObservableCollection<string> RecentScreenshots { get; } = new();
        public ObservableCollection<string> AvailableLanguages { get; } = new();

        public DashboardViewModel(IScreenCaptureService screenCapture, IVideoCaptureService videoCapture, ISnackbarService snackbar, ILanguageService languageService, IConfigService configService)
        {
            _screenCapture = screenCapture;
            _videoCapture = videoCapture;
            _snackbar = snackbar;
            _languageService = languageService;
            
            foreach (var lang in _languageService.GetAvailableLanguages())
            {
                AvailableLanguages.Add(lang);
            }

            // Bind to current config
            _selectedLanguage = configService.Global.Language;

            LoadRecentScreenshots();
        }

        partial void OnSelectedLanguageChanged(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _languageService.Load(value);
                TMRazorImproved.UI.Utilities.TranslationSource.Instance.Refresh();
                
                // Save to configuration so it persists
                var configService = App.GetService<IConfigService>();
                if (configService != null && configService.Global.Language != value)
                {
                    configService.Global.Language = value;
                    configService.Save();
                }
            }
        }

        private void LoadRecentScreenshots()
        {
            var path = _screenCapture.GetCapturePath();
            if (Directory.Exists(path))
            {
                var files = Directory.GetFiles(path, "*.jpg")
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.CreationTime)
                    .Take(10)
                    .Select(f => f.FullName);

                RecentScreenshots.Clear();
                foreach (var file in files)
                    RecentScreenshots.Add(file);
            }
        }

        [RelayCommand]
        private async Task TakeScreenshot()
        {
            var result = await _screenCapture.CaptureAsync();
            if (!string.IsNullOrEmpty(result))
            {
                _snackbar.Show("Screenshot Captured", "Image saved to your screenshots folder.", Wpf.Ui.Controls.ControlAppearance.Success, null, TimeSpan.FromSeconds(3));
                LoadRecentScreenshots();
            }
        }

        [RelayCommand]
        private async Task ToggleRecording()
        {
            if (IsRecording)
            {
                await _videoCapture.StopAsync();
                IsRecording = false;
                _snackbar.Show("Recording Stopped", "Video has been saved.", Wpf.Ui.Controls.ControlAppearance.Info, null, TimeSpan.FromSeconds(3));
            }
            else
            {
                var ok = await _videoCapture.StartAsync();
                if (ok)
                {
                    IsRecording = true;
                    _snackbar.Show("Recording Started", "UO Client is being recorded.", Wpf.Ui.Controls.ControlAppearance.Caution, null, TimeSpan.FromSeconds(3));
                }
                else
                {
                    _snackbar.Show("Recording Failed", "Could not start video capture.", Wpf.Ui.Controls.ControlAppearance.Danger, null, TimeSpan.FromSeconds(3));
                }
            }
        }

        [RelayCommand]
        private void OpenGallery()
        {
            var path = _screenCapture.GetCapturePath();
            if (Directory.Exists(path))
            {
                System.Diagnostics.Process.Start("explorer.exe", path);
            }
        }
    }
}
