using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.UI.ViewModels
{
    public partial class MapViewModel : ViewModelBase
    {
        private readonly IWorldService _worldService;
        private readonly IMapService _mapService;

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

        public ObservableCollection<MapMarker> Markers { get; } = new();

        public MapViewModel(IWorldService worldService, IMapService mapService)
        {
            _worldService = worldService;
            _mapService = mapService;

            // Sottoscrizione eventi mondo (se esistenti) o aggiornamento periodico
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

                if (IsTracking)
                {
                    UpdateMarkers();
                }
            }
        }

        private void UpdateMarkers()
        {
            Markers.Clear();
            
            // Aggiungi Player
            Markers.Add(new MapMarker { X = PlayerX, Y = PlayerY, Color = "Gold", Name = "You", Type = "Player" });

            // Aggiungi Mobiles vicini
            foreach (var mobile in _worldService.Mobiles.Take(50))
            {
                if (mobile.Serial == _worldService.Player?.Serial) continue;
                
                string color = "Crimson"; // Nemico
                if (mobile.Notoriety == 1) color = "DodgerBlue"; // Alleato
                if (mobile.Notoriety == 3) color = "Gray"; // Neutrale
                
                Markers.Add(new MapMarker { 
                    X = mobile.X, 
                    Y = mobile.Y, 
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
