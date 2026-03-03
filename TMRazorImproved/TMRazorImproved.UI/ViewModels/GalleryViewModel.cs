using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TMRazorImproved.Shared.Interfaces;

namespace TMRazorImproved.UI.ViewModels
{
    public partial class GalleryViewModel : ViewModelBase
    {
        private readonly IScreenCaptureService _captureService;
        private readonly string _basePath;

        public ObservableCollection<GalleryItem> Items { get; } = new();

        [ObservableProperty]
        private GalleryItem? _selectedItem;

        public GalleryViewModel(IScreenCaptureService captureService)
        {
            _captureService = captureService;
            _basePath = _captureService.GetCapturePath();
            
            _ = RefreshAsync();
        }

        [RelayCommand]
        public async Task RefreshAsync()
        {
            if (!Directory.Exists(_basePath)) return;

            await Task.Run(() =>
            {
                var files = Directory.GetFiles(_basePath, "*.jpg")
                    .OrderByDescending(f => File.GetCreationTime(f))
                    .Take(100) // Limite per performance
                    .ToList();

                RunOnUIThread(() =>
                {
                    Items.Clear();
                    foreach (var file in files)
                    {
                        Items.Add(new GalleryItem(file));
                    }
                });
            });
        }

        [RelayCommand]
        private void OpenFolder()
        {
            if (Directory.Exists(_basePath))
                Process.Start("explorer.exe", _basePath);
        }

        [RelayCommand]
        private void Delete(GalleryItem item)
        {
            try
            {
                if (File.Exists(item.FullPath))
                {
                    File.Delete(item.FullPath);
                    Items.Remove(item);
                    if (SelectedItem == item) SelectedItem = null;
                }
            }
            catch (Exception ex)
            {
                StatusText = $"Error deleting file: {ex.Message}";
            }
        }
    }

    public class GalleryItem
    {
        public string FullPath { get; }
        public string FileName { get; }
        public DateTime Date { get; }
        public BitmapImage Thumbnail { get; }

        public GalleryItem(string path)
        {
            FullPath = path;
            FileName = Path.GetFileName(path);
            Date = File.GetCreationTime(path);
            
            // Carica thumbnail in modo efficiente per WPF (senza lock del file)
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(path);
            bitmap.DecodePixelWidth = 200; // Ottimizzazione memoria
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze(); // Necessario per cross-thread
            Thumbnail = bitmap;
        }
    }
}
