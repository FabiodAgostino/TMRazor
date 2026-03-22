using System;
using System.Collections.Generic;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.Shared.Models
{
    /// <summary>
    /// FR-089: Metadata per le mappe di Ultima Online (equivalente di Razor/Core/Facet.cs).
    /// Contiene nomi, ID, dimensioni e helper di conversione per le mappe UO standard.
    /// </summary>
    public class FacetInfo
    {
        public int    Id     { get; }
        public string Name   { get; }
        public int    Width  { get; }
        public int    Height { get; }

        public FacetInfo(int id, string name, int width, int height)
        {
            Id = id; Name = name; Width = width; Height = height;
        }

        public bool IsInBounds(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;
        public override string ToString() => $"{Name} (ID={Id}, {Width}x{Height})";
    }

    /// <summary>
    /// Tabella statica delle mappe UO.
    /// </summary>
    public static class Facets
    {
        public static readonly FacetInfo Felucca  = new(0, "Felucca",  7168, 4096);
        public static readonly FacetInfo Trammel  = new(1, "Trammel",  7168, 4096);
        public static readonly FacetInfo Ilshenar = new(2, "Ilshenar", 2304, 1600);
        public static readonly FacetInfo Malas    = new(3, "Malas",    2560, 2048);
        public static readonly FacetInfo Tokuno   = new(4, "Tokuno",   1448, 1448);
        public static readonly FacetInfo TerMur   = new(5, "TerMur",   1280, 4096);

        private static readonly IReadOnlyList<FacetInfo> _all =
            new[] { Felucca, Trammel, Ilshenar, Malas, Tokuno, TerMur };

        public static IReadOnlyList<FacetInfo> All => _all;

        /// <summary>Restituisce la FacetInfo per l'ID mappa dato. Default: Felucca se fuori range.</summary>
        public static FacetInfo GetById(int mapId) =>
            (uint)mapId < (uint)_all.Count ? _all[mapId] : Felucca;

        /// <summary>Converte un nome mappa (case-insensitive) in ID. Ritorna 0 (Felucca) se non trovato.</summary>
        public static int ParseName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return 0;
            return name.ToLowerInvariant() switch
            {
                "felucca"                => 0,
                "trammel"                => 1,
                "ilshenar"               => 2,
                "malas"                  => 3,
                "tokuno" or "samurai"    => 4,
                "termur"                 => 5,
                _                        => 0,
            };
        }

        /// <summary>True se le coordinate (x, y) sono nei limiti della mappa mapId.</summary>
        public static bool IsInBounds(int mapId, int x, int y) => GetById(mapId).IsInBounds(x, y);
    }
}
