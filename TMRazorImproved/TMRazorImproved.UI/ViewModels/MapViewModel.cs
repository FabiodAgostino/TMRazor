using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows;

namespace TMRazorImproved.UI.ViewModels
{
    public partial class MapViewModel : ViewModelBase, IDisposable
    {
        private readonly IWorldService _worldService;
        private readonly IMapService _mapService;
        private readonly DispatcherTimer _updateTimer;

        [ObservableProperty]
        private int _playerX;

        [ObservableProperty]
        private int _playerY;

        [ObservableProperty]
        private int _mapId;

        [ObservableProperty]
        private double _zoom = 1.0;

        [ObservableProperty]
        private bool _isTracking = true;

        [ObservableProperty]
        private WriteableBitmap? _radarBitmap;

        public ObservableCollection<MapMarker> Markers { get; } = new();

        public MapViewModel(IWorldService worldService, IMapService mapService)
        {
            _worldService = worldService;
            _mapService = mapService;

            // Radar 256x256 tiles (standard size for UO radar)
            RadarBitmap = new WriteableBitmap(256, 256, 96, 96, System.Windows.Media.PixelFormats.Bgr555, null);

            _updateTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
            _updateTimer.Tick += (s, e) => UpdatePosition();
            _updateTimer.Start();

            UpdatePosition();
        }

        [RelayCommand]
        private void UpdatePosition()
        {
            if (_worldService.Player != null)
            {
                PlayerX = _worldService.Player.X;
                PlayerY = _worldService.Player.Y;
                MapId = _worldService.Player.MapId;

                UpdateRadar();

                if (IsTracking)
                {
                    UpdateMarkers();
                }
            }
        }

        private void UpdateRadar()
        {
            if (RadarBitmap == null) return;

            // Centro del radar sul player (128, 128)
            // Area 256x256
            int startX = PlayerX - 128;
            int startY = PlayerY - 128;

            byte[]? data = _mapService.GetMapImage(MapId, startX, startY, 256, 256, true);
            if (data != null && data.Length > 0)
            {
                RadarBitmap.WritePixels(new Int32Rect(0, 0, 256, 256), data, 256 * 2, 0);
            }
        }

        private void UpdateMarkers()
        {
            Markers.Clear();
            
            // Aggiungi Player (al centro del radar)
            Markers.Add(new MapMarker { X = 128, Y = 128, Color = "Gold", Name = "You", Type = "Player" });

            // Aggiungi Mobiles vicini (relativi al player)
            foreach (var mobile in _worldService.Mobiles.Take(50))
            {
                if (mobile.Serial == _worldService.Player?.Serial) continue;
                
                int relX = 128 + (mobile.X - PlayerX);
                int relY = 128 + (mobile.Y - PlayerY);

                if (relX < 0 || relX >= 256 || relY < 0 || relY >= 256) continue;

                string color = "Crimson"; // Nemico
                if (mobile.Notoriety == 1) color = "DodgerBlue"; // Alleato
                if (mobile.Notoriety == 3) color = "Gray"; // Neutrale
                
                Markers.Add(new MapMarker { 
                    X = relX, 
                    Y = relY, 
                    Color = color, 
                    Name = mobile.Name,
                    Type = "Mobile"
                });
            }
        }

        [RelayCommand]
        private void ZoomIn() => Zoom = Math.Min(4.0, Zoom + 0.2);

        [RelayCommand]
        private void ZoomOut() => Zoom = Math.Max(0.5, Zoom - 0.2);

        public void Dispose()
        {
            _updateTimer.Stop();
        }
    }

    public class MapMarker
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Color { get; set; } = "Red";
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = "Default";
    }
}

