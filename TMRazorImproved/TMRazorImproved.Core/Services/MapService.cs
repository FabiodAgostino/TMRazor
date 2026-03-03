using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using TMRazorImproved.Shared.Interfaces;
using Ultima;

namespace TMRazorImproved.Core.Services
{
    public class MapService : IMapService
    {
        private readonly ILogger<MapService> _logger;
        private readonly Dictionary<int, Map> _maps = new();
        private string? _currentPath;

        public MapService(ILogger<MapService> logger)
        {
            _logger = logger;
        }

        public void Initialize(string dataPath)
        {
            if (_currentPath == dataPath) return;

            _logger.LogInformation("Initializing MapService with path: {Path}", dataPath);
            _currentPath = dataPath;

            try
            {
                // Configura l'SDK per puntare alla cartella di UO
                Files.SetMulPath(dataPath);
                
                // Forza il ricaricamento dei file dell'SDK
                Map.Reload();
                
                _maps.Clear();
                _maps[0] = Map.Felucca;
                _maps[1] = Map.Trammel;
                _maps[2] = Map.Ilshenar;
                _maps[3] = Map.Malas;
                _maps[4] = Map.Tokuno;
                _maps[5] = Map.TerMur;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize UltimaSDK maps at {Path}", dataPath);
            }
        }

        public (int width, int height) GetMapDimensions(int mapId)
        {
            if (_maps.TryGetValue(mapId, out var map))
            {
                return (map.Width, map.Height);
            }
            return (0, 0);
        }

        public ushort[] GetMapBlock(int mapId, int blockX, int blockY, bool statics)
        {
            if (!_maps.TryGetValue(mapId, out var map))
            {
                return new ushort[64];
            }

            try
            {
                // L'SDK ritorna un "Bitmap" personalizzato (StubSystemDrawing) che contiene i pixel raw
                // GetImage(x, y, width, height, bool statics)
                // Usiamo 1x1 come dimensione del blocco (che nell'SDK significa 8x8 pixel)
                var bmp = map.GetImage(blockX, blockY, 1, 1, statics);
                
                if (bmp?.PixelData == null || bmp.PixelData.Length < 128) // 8x8x2 bytes = 128
                {
                    return new ushort[64];
                }

                // Convertiamo byte[] (Rgb555) in ushort[] per facilitare il rendering in WPF
                ushort[] pixels = new ushort[64];
                Buffer.BlockCopy(bmp.PixelData, 0, pixels, 0, 128);
                return pixels;
            }
            catch (Exception ex)
            {
                _logger.LogDebug("Error rendering map block {X},{Y} on map {Map}: {Msg}", blockX, blockY, mapId, ex.Message);
                return new ushort[64];
            }
        }
    }
}
