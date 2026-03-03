using System;
using System.Collections.Concurrent;
using System.Windows.Media.Imaging;
using Ultima;
using TMRazorImproved.UI.Utilities;

namespace TMRazorImproved.UI.Services
{
    public interface IUltimaImageCache
    {
        BitmapSource? GetGump(int gumpId);
        BitmapSource? GetStatic(int itemId);
        BitmapSource? GetLand(int landId);
        void Clear();
    }

    public class UltimaImageCache : IUltimaImageCache
    {
        private readonly ConcurrentDictionary<int, BitmapSource> _gumpCache = new();
        private readonly ConcurrentDictionary<int, BitmapSource> _staticCache = new();
        private readonly ConcurrentDictionary<int, BitmapSource> _landCache = new();

        public BitmapSource? GetGump(int gumpId)
        {
            if (_gumpCache.TryGetValue(gumpId, out var cached)) return cached;

            var gump = Gumps.GetGump(gumpId);
            var source = UltimaImageHelper.ToBitmapSource(gump);
            
            if (source != null)
            {
                source.Freeze(); // Optimization for cross-thread usage
                _gumpCache.TryAdd(gumpId, source);
            }
            
            return source;
        }

        public BitmapSource? GetStatic(int itemId)
        {
            if (_staticCache.TryGetValue(itemId, out var cached)) return cached;

            var art = Art.GetStatic(itemId);
            var source = UltimaImageHelper.ToBitmapSource(art);
            
            if (source != null)
            {
                source.Freeze();
                _staticCache.TryAdd(itemId, source);
            }
            
            return source;
        }

        public BitmapSource? GetLand(int landId)
        {
            if (_landCache.TryGetValue(landId, out var cached)) return cached;

            var art = Art.GetLand(landId);
            var source = UltimaImageHelper.ToBitmapSource(art);
            
            if (source != null)
            {
                source.Freeze();
                _landCache.TryAdd(landId, source);
            }
            
            return source;
        }

        public void Clear()
        {
            _gumpCache.Clear();
            _staticCache.Clear();
            _landCache.Clear();
        }
    }
}
