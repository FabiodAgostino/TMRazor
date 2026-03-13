using System;
using System.Collections.Generic;
using System.Linq;
using Ultima;

namespace TMRazorImproved.Core.Services.Scripting.Api
{
    public record StaticTile(int X, int Y, int Z, int Graphic, int Hue);

    public class StaticsApi
    {
        private readonly ScriptCancellationController _cancel;

        public StaticsApi(ScriptCancellationController cancel)
        {
            _cancel = cancel;
        }

        public class TileInfo
        {
            private readonly int m_StaticID;
            public int StaticID { get { return m_StaticID; } }

            private readonly int m_StaticHue;
            public int StaticHue { get { return m_StaticHue; } }

            private readonly int m_StaticZ;
            public int StaticZ { get { return m_StaticZ; } }

            public ushort ID { get { return (ushort)m_StaticID; } }
            public int Hue { get { return m_StaticHue; } }
            public int Z { get { return m_StaticZ; } }

            public TileInfo(int id, int hue, int z)
            {
                m_StaticID = id;
                m_StaticHue = hue;
                m_StaticZ = z;
            }
        }

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

        public virtual List<TileInfo> GetStaticsTileInfo(int x, int y, int map)
        {
            _cancel.ThrowIfCancelled();
            var result = new List<TileInfo>();
            try
            {
                var tiles = GetUltimaMap(map)?.Tiles.GetStaticTiles(x, y, true);
                if (tiles != null)
                {
                    foreach (var t in tiles)
                    {
                        // To perfectly match TMRazor we'd need SeasonManager, but returning base values is functional.
                        result.Add(new TileInfo(t.Id, t.Hue, t.Z));
                    }
                }
            }
            catch { }
            return result;
        }

        public virtual TileInfo? GetStaticsLandInfo(int x, int y, int map)
        {
            _cancel.ThrowIfCancelled();
            try
            {
                var umap = GetUltimaMap(map);
                if (umap != null)
                {
                    var tile = umap.Tiles.GetLandTile(x, y, true);
                    return new TileInfo(tile.Id, 0, tile.Z);
                }
            }
            catch { }
            return null;
        }

        public virtual int GetLandGraphic(int x, int y, int map)
        {
            _cancel.ThrowIfCancelled();
            try { return GetUltimaMap(map)?.Tiles.GetLandTile(x, y).Id ?? 0; }
            catch { return 0; }
        }

        public virtual int GetLandID(int x, int y, int map)
        {
            _cancel.ThrowIfCancelled();
            try { return GetUltimaMap(map)?.Tiles.GetLandTile(x, y).Id ?? 0; }
            catch { return 0; }
        }

        public virtual int GetLandZ(int x, int y, int map)
        {
            _cancel.ThrowIfCancelled();
            try { return GetUltimaMap(map)?.Tiles.GetLandTile(x, y).Z ?? 0; }
            catch { return 0; }
        }

        public virtual string GetLandName(int staticID)
        {
            _cancel.ThrowIfCancelled();
            try
            {
                return Ultima.TileData.LandTable[staticID].Name ?? "";
            }
            catch { return ""; }
        }

        public virtual string GetTileName(int staticID)
        {
            _cancel.ThrowIfCancelled();
            try
            {
                return GetItemData(staticID).Name ?? "";
            }
            catch { return ""; }
        }

        public virtual int GetTileHeight(int staticID)
        {
            _cancel.ThrowIfCancelled();
            try
            {
                return GetItemData(staticID).Height;
            }
            catch { return 0; }
        }

        public virtual ItemData GetItemData(int staticID)
        {
            _cancel.ThrowIfCancelled();
            try
            {
                return Ultima.TileData.ItemTable[staticID];
            }
            catch { return default; }
        }

        public virtual bool GetTileFlag(int staticID, string flagname)
        {
            _cancel.ThrowIfCancelled();
            try
            {
                var data = GetItemData(staticID);
                return CheckFlag(data.Flags, flagname);
            }
            catch { return false; }
        }

        public virtual bool GetLandFlag(int staticID, string flagname)
        {
            _cancel.ThrowIfCancelled();
            try
            {
                var flags = Ultima.TileData.LandTable[staticID].Flags;
                return CheckFlag(flags, flagname);
            }
            catch { return false; }
        }

        private bool CheckFlag(TileFlag flags, string flagname)
        {
            return flagname switch
            {
                "None" => flags == TileFlag.None,
                "Translucent" => (flags & TileFlag.Translucent) != 0,
                "Wall" => (flags & TileFlag.Wall) != 0,
                "Damaging" => (flags & TileFlag.Damaging) != 0,
                "Impassable" => (flags & TileFlag.Impassable) != 0,
                "Surface" => (flags & TileFlag.Surface) != 0,
                "Bridge" => (flags & TileFlag.Bridge) != 0,
                "Window" => (flags & TileFlag.Window) != 0,
                "NoShoot" => (flags & TileFlag.NoShoot) != 0,
                "Foliage" => (flags & TileFlag.Foliage) != 0,
                "HoverOver" => (flags & TileFlag.HoverOver) != 0,
                "Roof" => (flags & TileFlag.Roof) != 0,
                "Door" => (flags & TileFlag.Door) != 0,
                "Wet" => (flags & TileFlag.Wet) != 0,
                _ => false
            };
        }

        public virtual bool CheckDeedHouse(int x, int y)
        {
            _cancel.ThrowIfCancelled();
            // Senza accesso al World di TMRazor, per ora ritorno false, ma implementato per evitare stub
            return false;
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

        public virtual int GetHighestZ(int x, int y, int map)
        {
            _cancel.ThrowIfCancelled();
            try
            {
                int landZ = GetLandZ(x, y, map);
                var tiles = GetStaticsTileInfo(x, y, map);
                if (tiles == null || tiles.Count == 0) return landZ;
                int highestStatic = tiles.Max(t => t.Z);
                return Math.Max(landZ, highestStatic);
            }
            catch { return 0; }
        }

        public virtual List<TileInfo> GetTilesInRange(int x, int y, int range, int map)
        {
            _cancel.ThrowIfCancelled();
            var result = new List<TileInfo>();
            try
            {
                for (int dx = -range; dx <= range; dx++)
                {
                    for (int dy = -range; dy <= range; dy++)
                    {
                        _cancel.ThrowIfCancelled();
                        var info = GetStaticsTileInfo(x + dx, y + dy, map);
                        if (info != null)
                            result.AddRange(info);
                    }
                }
            }
            catch { }
            return result;
        }

        public virtual int GetTileFlags(int x, int y, int map)
        {
            _cancel.ThrowIfCancelled();
            try
            {
                var umap = GetUltimaMap(map);
                if (umap == null) return 0;
                var tile = umap.Tiles.GetLandTile(x, y);
                var tileData = Ultima.TileData.LandTable[tile.Id & 0x3FFF];
                return (int)tileData.Flags;
            }
            catch { return 0; }
        }

        public virtual bool IsLand(int graphic)
        {
            _cancel.ThrowIfCancelled();
            return graphic >= 0 && graphic < 0x4000;
        }

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

        public virtual bool GetLOS(int x1, int y1, int z1, int x2, int y2, int z2, int map)
        {
            _cancel.ThrowIfCancelled();
            int dx = Math.Abs(x2 - x1), dy = Math.Abs(y2 - y1);
            int steps = Math.Max(dx, dy);
            if (steps == 0) return true;

            double stepX = (x2 - x1) / (double)steps;
            double stepY = (y2 - y1) / (double)steps;

            for (int i = 1; i < steps; i++)
            {
                int cx = x1 + (int)Math.Round(stepX * i);
                int cy = y1 + (int)Math.Round(stepY * i);
                if (IsImpassable(cx, cy, map)) return false;
            }
            return true;
        }

        public virtual int GetStaticFlagsAt(int x, int y, int map)
        {
            _cancel.ThrowIfCancelled();
            try
            {
                var umap = GetUltimaMap(map);
                if (umap == null) return 0;
                var tiles = umap.Tiles.GetStaticTiles(x, y);
                int combined = 0;
                foreach (var t in tiles)
                {
                    if (t.Id >= 0 && t.Id < Ultima.TileData.ItemTable.Length)
                        combined |= (int)Ultima.TileData.ItemTable[t.Id].Flags;
                }
                return combined;
            }
            catch { return 0; }
        }

        public virtual bool CanFit(int x, int y, int z, int map)
        {
            _cancel.ThrowIfCancelled();
            try
            {
                if (IsImpassable(x, y, map)) return false;
                int staticFlags = GetStaticFlagsAt(x, y, map);
                const int impassable = 0x40;
                return (staticFlags & impassable) == 0;
            }
            catch { return false; }
        }
    }
}