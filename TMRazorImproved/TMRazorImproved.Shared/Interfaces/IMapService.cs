namespace TMRazorImproved.Shared.Interfaces
{
    public interface IMapService
    {
        /// <summary>Inizializza l'SDK con il percorso dei file .mul/.uop.</summary>
        void Initialize(string dataPath);
        
        /// <summary>Ritorna le dimensioni della mappa specificata.</summary>
        (int width, int height) GetMapDimensions(int mapId);
        
        /// <summary>Ritorna un blocco 8x8 di pixel (formato Rgb555 ushort).</summary>
        ushort[] GetMapBlock(int mapId, int blockX, int blockY, bool statics);

        /// <summary>Ritorna un'immagine dell'area specificata (formato Rgb555 byte[]).</summary>
        byte[]? GetMapImage(int mapId, int tileX, int tileY, int widthTiles, int heightTiles, bool statics);
    }
}
