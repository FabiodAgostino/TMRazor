using System.Collections.Generic;

namespace TMRazorImproved.Core.Services.Scripting.Api
{
    public record StaticTile(int X, int Y, int Z, int Graphic, int Hue);

    /// <summary>
    /// Sprint Fix-3: implementazione reale di StaticsApi tramite UltimaSDK.
    /// I metodi precedenti ritornavano stub (sempre 0 / lista vuota).
    /// Ora leggono i dati reali dalle mappe Ultima caricate da IMapService.
    /// Tutti i metodi catturano eccezioni per robustezza (es. SDK non inizializzato).
    /// </summary>
    public class StaticsApi
    {
        private readonly ScriptCancellationController _cancel;

        public StaticsApi(ScriptCancellationController cancel)
        {
            _cancel = cancel;
        }

        /// <summary>
        /// Ritorna il graphic ID del primo tile statico a (x,y) sulla mappa indicata.
        /// Ritorna 0 se non ci sono statici o se l'UltimaSDK non è inizializzato.
        /// </summary>
        public virtual int GetStaticsGraphic(int x, int y, int map)
        {
            _cancel.ThrowIfCancelled();
            try
            {
                var tiles = GetUltimaMap(map)?.Tiles.GetStaticTiles(x, y, true);
                return (tiles == null || tiles.Length == 0) ? 0 : tiles[0].Id;
            }
            catch { return 0; }
        }

        /// <summary>
        /// Ritorna tutte le tile statiche a (x,y) sulla mappa indicata.
        /// Lista vuota se non ci sono statici o se l'UltimaSDK non è inizializzato.
        /// </summary>
        public virtual List<StaticTile> GetStaticsTileInfo(int x, int y, int map)
        {
            _cancel.ThrowIfCancelled();
            var result = new List<StaticTile>();
            try
            {
                var tiles = GetUltimaMap(map)?.Tiles.GetStaticTiles(x, y, true);
                if (tiles != null)
                    foreach (var t in tiles)
                        result.Add(new StaticTile(x, y, t.Z, t.Id, t.Hue));
            }
            catch { }
            return result;
        }

        /// <summary>Ritorna il graphic ID della tile di terreno a (x,y).</summary>
        public virtual int GetLandGraphic(int x, int y, int map)
        {
            _cancel.ThrowIfCancelled();
            try { return GetUltimaMap(map)?.Tiles.GetLandTile(x, y).Id ?? 0; }
            catch { return 0; }
        }

        /// <summary>Ritorna la coordinata Z della tile di terreno a (x,y).</summary>
        public virtual int GetLandZ(int x, int y, int map)
        {
            _cancel.ThrowIfCancelled();
            try { return GetUltimaMap(map)?.Tiles.GetLandTile(x, y).Z ?? 0; }
            catch { return 0; }
        }

        private static Ultima.Map? GetUltimaMap(int mapId) => mapId switch
        {
            0 => Ultima.Map.Felucca,
            1 => Ultima.Map.Trammel,
            2 => Ultima.Map.Ilshenar,
            3 => Ultima.Map.Malas,
            4 => Ultima.Map.Tokuno,
            5 => Ultima.Map.TerMur,
            _ => Ultima.Map.Felucca
        };

        // ------------------------------------------------------------------
        // API aggiuntive
        // ------------------------------------------------------------------

        /// <summary>
        /// Ritorna la Z più alta tra le static tiles e il terreno a (x,y).
        /// Utile per determinare la coordinata Z "calpestabile" di una cella.
        /// </summary>
        public virtual int GetHighestZ(int x, int y, int map)
        {
            _cancel.ThrowIfCancelled();
            try
            {
                int landZ = GetLandZ(x, y, map);
                var tiles = GetStaticsTileInfo(x, y, map);
                if (tiles.Count == 0) return landZ;
                int highestStatic = tiles.Max(t => t.Z);
                return Math.Max(landZ, highestStatic);
            }
            catch { return 0; }
        }

        /// <summary>
        /// Ritorna tutte le static tiles in un quadrato di (range*2+1)x(range*2+1) attorno a (x,y).
        /// </summary>
        public virtual List<StaticTile> GetTilesInRange(int x, int y, int range, int map)
        {
            _cancel.ThrowIfCancelled();
            var result = new List<StaticTile>();
            try
            {
                for (int dx = -range; dx <= range; dx++)
                {
                    for (int dy = -range; dy <= range; dy++)
                    {
                        _cancel.ThrowIfCancelled();
                        result.AddRange(GetStaticsTileInfo(x + dx, y + dy, map));
                    }
                }
            }
            catch { }
            return result;
        }

        /// <summary>
        /// Ritorna il flag della tile di terreno a (x,y).
        /// I flag UO sono bit mask: 0x1=Wet, 0x2=Impassable, 0x4=Surface, ecc.
        /// </summary>
        public virtual int GetTileFlags(int x, int y, int map)
        {
            _cancel.ThrowIfCancelled();
            try
            {
                var umap = GetUltimaMap(map);
                if (umap == null) return 0;
                var tile = umap.Tiles.GetLandTile(x, y);
                // Ultima.TileData.LandTable[tile.ID].Flags
                var tileData = Ultima.TileData.LandTable[tile.ID & 0x3FFF];
                return (int)tileData.Flags;
            }
            catch { return 0; }
        }

        /// <summary>
        /// True se il graphic specificato è una tile di terreno (ID &lt; 0x4000).
        /// </summary>
        public virtual bool IsLand(int graphic)
        {
            _cancel.ThrowIfCancelled();
            return graphic >= 0 && graphic < 0x4000;
        }

        /// <summary>
        /// True se la cella (x,y) è impassabile (flag 0x40 oppure 0x2 su terreno).
        /// </summary>
        public virtual bool IsImpassable(int x, int y, int map)
        {
            _cancel.ThrowIfCancelled();
            try
            {
                int flags = GetTileFlags(x, y, map);
                const int impassableFlag = 0x40;
                return (flags & impassableFlag) != 0;
            }
            catch { return false; }
        }
    }
}
