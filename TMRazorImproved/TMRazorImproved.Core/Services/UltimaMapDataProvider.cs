using Ultima;

namespace TMRazorImproved.Core.Services
{
    /// <summary>
    /// Implementazione di IMapDataProvider che delega all'UltimaSDK reale.
    /// Usata in produzione; nei test viene sostituita con un mock.
    /// </summary>
    public class UltimaMapDataProvider : IMapDataProvider
    {
        public bool IsMapAvailable(int mapId)
        {
            try { return GetUltimaMap(mapId) != null; }
            catch { return false; }
        }

        public Tile GetLandTile(int x, int y, int mapId)
        {
            try { return GetUltimaMap(mapId)?.Tiles.GetLandTile(x, y) ?? default; }
            catch { return default; }
        }

        public HuedTile[] GetStaticTiles(int x, int y, int mapId)
        {
            try { return GetUltimaMap(mapId)?.Tiles.GetStaticTiles(x, y, true) ?? System.Array.Empty<HuedTile>(); }
            catch { return System.Array.Empty<HuedTile>(); }
        }

        public LandData GetLandData(int id)
        {
            try { return TileData.LandTable[id & (TileData.LandTable.Length - 1)]; }
            catch { return default; }
        }

        public ItemData GetItemData(int id)
        {
            try { return TileData.ItemTable[id & (TileData.ItemTable.Length - 1)]; }
            catch { return default; }
        }

        private static Map? GetUltimaMap(int mapId) => mapId switch
        {
            0 => Map.Felucca,
            1 => Map.Trammel,
            2 => Map.Ilshenar,
            3 => Map.Malas,
            4 => Map.Tokuno,
            5 => Map.TerMur,
            _ => null
        };
    }
}
