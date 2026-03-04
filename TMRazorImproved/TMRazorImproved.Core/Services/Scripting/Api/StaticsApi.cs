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
    }
}
