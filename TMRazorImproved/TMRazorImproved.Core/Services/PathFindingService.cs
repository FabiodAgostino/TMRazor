using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using TMRazorImproved.Shared.Interfaces;
using Ultima;

namespace TMRazorImproved.Core.Services
{
    public class PathFindingService : IPathFindingService
    {
        private const int BigCost = int.MaxValue / 2;
        private const int PersonHeight = 16;
        private const int StepHeight = 2;
        private static readonly TileFlag ImpassableSurface = TileFlag.Impassable | TileFlag.Surface;

        private static readonly (int X, int Y)[] Dirs = {
            (1, 0), (-1, 0), (0, 1), (0, -1),
            (-1, -1), (1, 1), (-1, 1), (1, -1)
        };

        private readonly IWorldService _worldService;
        private readonly IMapDataProvider _mapProvider;
        private readonly IConfigService _configService;
        private readonly ILogger<PathFindingService> _logger;

        public PathFindingService(
            IWorldService worldService,
            IMapDataProvider mapProvider,
            IConfigService configService,
            ILogger<PathFindingService> logger)
        {
            _worldService = worldService;
            _mapProvider = mapProvider;
            _configService = configService;
            _logger = logger;
        }

        public List<(int X, int Y)>? GetPath(int startX, int startY, int startZ, int destX, int destY, int mapId, bool ignoreMobiles = false, bool ignoreDoors = false)
        {
            if (!_mapProvider.IsMapAvailable(mapId))
            {
                _logger.LogWarning("Map {MapId} is not available", mapId);
                return null;
            }

            int distanceX = Math.Abs(startX - destX);
            int distanceY = Math.Abs(startY - destY);
            int scanMaxRange = Math.Max(distanceX, distanceY) + 2;
            int maxLimit = _configService.CurrentProfile?.PathFindingMaxRange ?? 200;
            if (scanMaxRange > maxLimit) scanMaxRange = maxLimit;

            var start = (X: startX, Y: startY);
            var goal  = (X: destX,  Y: destY);

            var frontier  = new PriorityQueue<(int X, int Y, int Z), int>();
            frontier.Enqueue((startX, startY, startZ), 0);

            var cameFrom  = new Dictionary<(int X, int Y), (int X, int Y)>();
            var costSoFar = new Dictionary<(int X, int Y), int>();

            cameFrom[start]  = start;
            costSoFar[start] = 0;

            var bounds = new { MinX = startX - scanMaxRange, MaxX = startX + scanMaxRange,
                                MinY = startY - scanMaxRange, MaxY = startY + scanMaxRange };

            var itemsOnGround = _worldService.Items.Where(i => i.Container == 0).ToList();

            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();

                if (current.X == goal.X && current.Y == goal.Y)
                    break;

                foreach (var dir in Dirs)
                {
                    var next = (X: current.X + dir.X, Y: current.Y + dir.Y);

                    if (next.X < bounds.MinX || next.X > bounds.MaxX ||
                        next.Y < bounds.MinY || next.Y > bounds.MaxY)
                        continue;

                    int cost = GetMoveCost(itemsOnGround, current, mapId, next, ignoreMobiles, ignoreDoors, out int nextZ);

                    if (cost < BigCost)
                    {
                        var current2D = (current.X, current.Y);
                        int newCost   = costSoFar[current2D] + cost;

                        if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                        {
                            costSoFar[next] = newCost;
                            cameFrom[next]  = current2D;
                            frontier.Enqueue((next.X, next.Y, nextZ), newCost + Heuristic(next, goal));
                        }
                    }
                }
            }

            if (!cameFrom.ContainsKey(goal))
                return null;

            // FIX BUG-P1-01: ricostruzione corretta senza duplicare il goal.
            var path = new List<(int X, int Y)>();
            var currNode = goal;
            while (currNode != start)
            {
                path.Add(currNode);
                currNode = cameFrom[currNode];
            }
            if (path.Count > 0) path.Add(start);
            path.Reverse();
            return path;
        }

        private int Heuristic((int X, int Y) a, (int X, int Y) b)
            => Math.Max(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));

        private int GetMoveCost(List<TMRazorImproved.Shared.Models.Item> items,
                                (int X, int Y, int Z) loc, int mapId,
                                (int X, int Y) b, bool ignoreMobiles, bool ignoreDoors,
                                out int bZ)
        {
            GetStartZ(loc, mapId, items.Where(x => x.X == loc.X && x.Y == loc.Y), out _, out int startTop);
            bool moveIsOk = Check(mapId, items.Where(x => x.X == b.X && x.Y == b.Y),
                                  b.X, b.Y, startTop, loc.Z, ignoreMobiles, ignoreDoors, out int newZ);
            if (moveIsOk) { bZ = newZ; return 1; }
            bZ = loc.Z;
            return BigCost;
        }

        private bool Check(int mapId, IEnumerable<TMRazorImproved.Shared.Models.Item> items,
                           int x, int y, int startTop, int startZ,
                           bool ignoreMobiles, bool ignoreDoors, out int newZ)
        {
            newZ = 0;
            var landTile = _mapProvider.GetLandTile(x, y, mapId);
            var landData = _mapProvider.GetLandData(landTile.Id);
            bool landBlocks  = (landData.Flags & TileFlag.Impassable) != 0;
            bool considerLand = !Ignored(landTile);

            int landZ = 0, landCenter = 0, landTop = 0;
            GetAverageZ(mapId, x, y, ref landZ, ref landCenter, ref landTop);

            bool moveIsOk  = false;
            int  stepTop   = startTop + StepHeight;
            int  checkTop  = startZ   + PersonHeight;
            bool ignoreSpellFields = true;

            if (!ignoreMobiles)
            {
                if (_worldService.Mobiles.Any(m => m.X == x && m.Y == y
                                              && m.Serial != _worldService.Player?.Serial))
                    return false;
            }

            var staticTiles = _mapProvider.GetStaticTiles(x, y, mapId);
            foreach (var tile in staticTiles)
            {
                var itemData = _mapProvider.GetItemData(tile.Id);
                if ((itemData.Flags & ImpassableSurface) != TileFlag.Surface) continue;

                int itemZ  = tile.Z;
                int ourZ   = itemZ + itemData.CalcHeight;
                int ourTop = ourZ + PersonHeight;
                int testTop = checkTop;

                if (moveIsOk)
                {
                    int cmp = Math.Abs(ourZ - (_worldService.Player?.Z ?? 0))
                            - Math.Abs(newZ  - (_worldService.Player?.Z ?? 0));
                    if (cmp > 0 || (cmp == 0 && ourZ > newZ)) continue;
                }

                if (ourTop > testTop) testTop = ourTop;
                int itemTop = itemZ;
                if (!itemData.Bridge) itemTop += itemData.Height;
                if (stepTop < itemTop) continue;

                int landCheck = itemZ;
                if (itemData.Height >= StepHeight) landCheck += StepHeight;
                else landCheck += itemData.Height;
                if (considerLand && landCheck < landCenter && landCenter > ourZ && testTop > landZ) continue;

                if (!IsOk(ignoreDoors, ignoreSpellFields, ourZ, testTop, staticTiles, items)) continue;

                newZ = ourZ;
                moveIsOk = true;
            }

            foreach (var item in items)
            {
                var itemData = _mapProvider.GetItemData(item.Graphic);
                if ((itemData.Flags & ImpassableSurface) != TileFlag.Surface) continue;

                int itemZ  = item.Z;
                int ourZ   = itemZ + itemData.CalcHeight;
                int ourTop = ourZ + PersonHeight;
                int testTop = checkTop;

                if (moveIsOk)
                {
                    int cmp = Math.Abs(ourZ - (_worldService.Player?.Z ?? 0))
                            - Math.Abs(newZ  - (_worldService.Player?.Z ?? 0));
                    if (cmp > 0 || (cmp == 0 && ourZ > newZ)) continue;
                }

                if (ourTop > testTop) testTop = ourTop;
                int itemTop = itemZ;
                if (!itemData.Bridge) itemTop += itemData.Height;
                if (stepTop < itemTop) continue;

                int landCheck = itemZ;
                if (itemData.Height >= StepHeight) landCheck += StepHeight;
                else landCheck += itemData.Height;
                if (considerLand && landCheck < landCenter && landCenter > ourZ && testTop > landZ) continue;

                if (!IsOk(ignoreDoors, ignoreSpellFields, ourZ, testTop, staticTiles, items)) continue;

                newZ = ourZ;
                moveIsOk = true;
            }

            if (!considerLand || landBlocks) return moveIsOk;

            int finalOurZ   = landCenter;
            int finalOurTop = finalOurZ + PersonHeight;
            int finalTestTop = checkTop;
            if (finalOurTop > finalTestTop) finalTestTop = finalOurTop;

            if (moveIsOk)
            {
                int cmp = Math.Abs(finalOurZ - (_worldService.Player?.Z ?? 0))
                        - Math.Abs(newZ       - (_worldService.Player?.Z ?? 0));
                if (cmp > 0 || (cmp == 0 && finalOurZ > newZ))
                    return moveIsOk;
            }

            if (!IsOk(ignoreDoors, ignoreSpellFields, finalOurZ, finalTestTop, staticTiles, items))
                return moveIsOk;

            newZ = finalOurZ;
            return true;
        }

        private bool IsOk(bool ignoreDoors, bool ignoreSpellFields,
                          int ourZ, int ourTop,
                          HuedTile[] tiles, IEnumerable<TMRazorImproved.Shared.Models.Item> items)
        {
            foreach (var t in tiles)
            {
                var itemData = _mapProvider.GetItemData(t.Id);
                if (t.Z + itemData.CalcHeight > ourZ && ourTop > t.Z
                    && (itemData.Flags & ImpassableSurface) != 0)
                    return false;
            }
            foreach (var i in items)
            {
                var itemData = _mapProvider.GetItemData(i.Graphic);
                if ((itemData.Flags & ImpassableSurface) == 0) continue;
                if (ignoreDoors && ((itemData.Flags & TileFlag.Door) != 0
                    || i.Graphic == 0x692 || i.Graphic == 0x846 || i.Graphic == 0x873
                    || (i.Graphic >= 0x6F5 && i.Graphic <= 0x6F6))) continue;
                if (ignoreSpellFields && (i.Graphic == 0x82 || i.Graphic == 0x3946 || i.Graphic == 0x3956)) continue;
                if (i.Z + itemData.CalcHeight > ourZ && ourTop > i.Z)
                    return false;
            }
            return true;
        }

        private void GetStartZ((int X, int Y, int Z) loc, int mapId,
                               IEnumerable<TMRazorImproved.Shared.Models.Item> itemList,
                               out int zLow, out int zTop)
        {
            var landTile = _mapProvider.GetLandTile(loc.X, loc.Y, mapId);
            var landData = _mapProvider.GetLandData(landTile.Id);
            bool landBlocks = (landData.Flags & TileFlag.Impassable) != 0;

            int landZ = 0, landCenter = 0, landTop = 0;
            GetAverageZ(mapId, loc.X, loc.Y, ref landZ, ref landCenter, ref landTop);

            bool considerLand = !Ignored(landTile);

            int zCenter = 0;
            zLow = zTop = 0;
            bool isSet = false;

            if (considerLand && !landBlocks && loc.Z >= landCenter)
            {
                zLow    = landZ;
                zCenter = landCenter;
                zTop    = landTop;
                isSet   = true;
            }

            var staticTiles = _mapProvider.GetStaticTiles(loc.X, loc.Y, mapId);
            foreach (var tile in staticTiles)
            {
                var tileData = _mapProvider.GetItemData(tile.Id);
                int calcTop  = tile.Z + tileData.CalcHeight;

                if (isSet && calcTop < zCenter) continue;
                if ((tileData.Flags & TileFlag.Surface) == 0) continue;
                if (loc.Z < calcTop) continue;

                zLow    = tile.Z;
                zCenter = calcTop;
                int top = tile.Z + tileData.Height;
                if (!isSet || top > zTop) zTop = top;
                isSet = true;
            }

            foreach (var item in itemList)
            {
                var itemData = _mapProvider.GetItemData(item.Graphic);
                int calcTop  = item.Z + itemData.CalcHeight;

                if (isSet && calcTop < zCenter) continue;
                if ((itemData.Flags & TileFlag.Surface) == 0) continue;
                if (loc.Z < calcTop) continue;

                zLow    = item.Z;
                zCenter = calcTop;
                int top = item.Z + itemData.Height;
                if (!isSet || top > zTop) zTop = top;
                isSet = true;
            }

            if (!isSet) zLow = zTop = loc.Z;
            else if (loc.Z > zTop) zTop = loc.Z;
        }

        private void GetAverageZ(int mapId, int x, int y, ref int z, ref int avg, ref int top)
        {
            int zT = _mapProvider.GetLandTile(x,     y,     mapId).Z;
            int zL = _mapProvider.GetLandTile(x,     y + 1, mapId).Z;
            int zR = _mapProvider.GetLandTile(x + 1, y,     mapId).Z;
            int zB = _mapProvider.GetLandTile(x + 1, y + 1, mapId).Z;

            z = Math.Min(Math.Min(zT, zL), Math.Min(zR, zB));
            top = Math.Max(Math.Max(zT, zL), Math.Max(zR, zB));

            avg = Math.Abs(zT - zB) > Math.Abs(zL - zR)
                ? FloorAverage(zL, zR)
                : FloorAverage(zT, zB);
        }

        private static int FloorAverage(int a, int b)
        {
            int v = a + b;
            if (v < 0) --v;
            return v / 2;
        }

        private static bool Ignored(Tile tile)
            => tile.Id == 2 || tile.Id == 0x1DB || (tile.Id >= 0x1AE && tile.Id <= 0x1B5);
    }
}
