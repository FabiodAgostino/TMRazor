using Ultima;

namespace TMRazorImproved.Core.Services
{
    /// <summary>
    /// Sprint Fix-4: astrazione sull'accesso ai dati mappa UO (UltimaSDK).
    /// Permette di iniettare un'implementazione mock nei test di PathFindingService
    /// senza dipendere da file mappa UO reali.
    /// </summary>
    public interface IMapDataProvider
    {
        /// <summary>
        /// Indica se la mappa con l'ID specificato è disponibile (es. SDK inizializzato).
        /// Se false, GetPath ritorna null senza lanciare eccezioni.
        /// </summary>
        bool IsMapAvailable(int mapId);

        /// <summary>Ritorna il tile di terreno alla coordinata (x, y) sulla mappa.</summary>
        Tile GetLandTile(int x, int y, int mapId);

        /// <summary>Ritorna tutti i tile statici alla coordinata (x, y) sulla mappa.</summary>
        HuedTile[] GetStaticTiles(int x, int y, int mapId);

        /// <summary>Ritorna i metadati del tile di terreno con l'ID specificato.</summary>
        LandData GetLandData(int id);

        /// <summary>Ritorna i metadati del tile di item (statico o world item) con l'ID specificato.</summary>
        ItemData GetItemData(int id);
    }
}
