using System.Collections.Concurrent;

namespace TMRazorImproved.Core.Utilities
{
    /// <summary>
    /// Dati di un item mappa UO ricevuti tramite pacchetto 0x90 (MapDetails).
    /// </summary>
    public class MapItemData
    {
        public uint   Serial     { get; }
        public ushort ItemId     { get; }
        public int    MapOriginX { get; }
        public int    MapOriginY { get; }
        public int    MapEndX    { get; }
        public int    MapEndY    { get; }
        public int    Width      { get; }
        public int    Height     { get; }
        public ushort Facet      { get; }

        public MapItemData(uint serial, ushort itemId,
            int originX, int originY, int endX, int endY,
            int width, int height, ushort facet)
        {
            Serial     = serial;
            ItemId     = itemId;
            MapOriginX = originX;
            MapOriginY = originY;
            MapEndX    = endX;
            MapEndY    = endY;
            Width      = width;
            Height     = height;
            Facet      = facet;
        }
    }

    /// <summary>
    /// Store statico per i dati degli item mappa (pacchetto 0x90).
    /// Indicizzato per serial dell'item.
    /// </summary>
    public static class MapDataStore
    {
        private static readonly ConcurrentDictionary<uint, MapItemData> _data = new();

        public static void Set(MapItemData entry) => _data[entry.Serial] = entry;

        public static MapItemData? Get(uint serial)
            => _data.TryGetValue(serial, out var e) ? e : null;

        public static void Clear() => _data.Clear();
    }
}
