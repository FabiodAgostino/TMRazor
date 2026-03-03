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
        private const TileFlag ImpassableSurface = TileFlag.Impassable | TileFlag.Surface;

        private static readonly (int X, int Y)[] Dirs = {
            (1, 0), (-1, 0), (0, 1), (0, -1),
            (-1, -1), (1, 1), (-1, 1), (1, -1)
        };

        private readonly IWorldService _worldService;
        private readonly ILogger<PathFindingService> _logger;

        public PathFindingService(IWorldService worldService, ILogger<PathFindingService> logger)
        {
            _worldService = worldService;
            _logger = logger;
        }

        public List<(int X, int Y)>? GetPath(int startX, int startY, int startZ, int destX, int destY, int mapId, bool ignoreMobiles = false)
        {
            Map map = GetMap(mapId);
            if (map == null) return null;

            int distanceX = Math.Abs(startX - destX);
            int distanceY = Math.Abs(startY - destY);
            int scanMaxRange = Math.Max(distanceX, distanceY) + 2;

            if (scanMaxRange > 100) scanMaxRange = 100; // Limit scan range to avoid massive memory usage

            var start = (X: startX, Y: startY);
            var goal = (X: destX, Y: destY);

            var frontier = new PriorityQueue<(int X, int Y, int Z), int>();
            frontier.Enqueue((startX, startY, startZ), 0);

            var cameFrom = new Dictionary<(int X, int Y), (int X, int Y)>();
            var costSoFar = new Dictionary<(int X, int Y), int>();

            cameFrom[start] = start;
            costSoFar[start] = 0;

            var bounds = new { MinX = startX - scanMaxRange, MaxX = startX + scanMaxRange, MinY = startY - scanMaxRange, MaxY = startY + scanMaxRange };

            var itemsOnGround = _worldService.Items.Where(i => i.Container == 0).ToList();

            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();

                if (current.X == goal.X && current.Y == goal.Y)
                    break;

                foreach (var dir in Dirs)
                {
                    var next = (X: current.X + dir.X, Y: current.Y + dir.Y);

                    if (next.X < bounds.MinX || next.X > bounds.MaxX || next.Y < bounds.MinY || next.Y > bounds.MaxY)
                        continue;

                    int cost = GetMoveCost(itemsOnGround, current, map, next, ignoreMobiles, out int nextZ);
                    
                    if (cost < BigCost)
                    {
                        var current2D = (current.X, current.Y);
                        int newCost = costSoFar[current2D] + cost;

                        if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                        {
                            costSoFar[next] = newCost;
                            cameFrom[next] = current2D;
                            int priority = newCost + Heuristic(next, goal);
                            frontier.Enqueue((next.X, next.Y, nextZ), priority);
                        }
                    }
                }
            }

            if (!cameFrom.ContainsKey(goal))
                return null;

            var path = new List<(int X, int Y)>();
            var currNode = goal;
            path.Add(currNode);

            while (currNode != start)
            {
                currNode = cameFrom[currNode];
                if (currNode != start)
                    path.Add(currNode);
            }
            
            path.Reverse();
            path.Add(goal); // Ensure goal is always explicitly in path if we moved
            return path;
        }

        private int Heuristic((int X, int Y) a, (int X, int Y) b)
        {
            return Math.Max(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y)); // Chebyshev distance
        }

        private Map GetMap(int mapId)
        {
            return mapId switch
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

        private int GetMoveCost(List<TMRazorImproved.Shared.Models.Item> items, (int X, int Y, int Z) loc, Map map, (int X, int Y) b, bool ignoreMobiles, out int bZ)
        {
            int xForward = b.X, yForward = b.Y;
            int newZ = 0;
            int cost = 1;
            
            GetStartZ(loc, map, items.Where(x => x.X == loc.X && x.Y == loc.Y), out int startZ, out int startTop);
            
            bool moveIsOk = Check(map, items.Where(x => x.X == xForward && x.Y == yForward), xForward, yForward, startTop, startZ, ignoreMobiles, out newZ);
            
            if (moveIsOk)
            {
                bZ = newZ;
                return cost;
            }

            bZ = startZ;
            return BigCost;
        }

        private bool Check(Map map, IEnumerable<TMRazorImproved.Shared.Models.Item> items, int x, int y, int startTop, int startZ, bool ignoreMobiles, out int newZ)
        {
            newZ = 0;
            var landTile = map.Tiles.GetLandTile(x, y);
            var landData = TileData.LandTable[landTile.Id & (TileData.LandTable.Length - 1)];
            var landBlocks = (landData.Flags & TileFlag.Impassable) != 0;
            var considerLand = !Ignored(landTile);

            int landZ = 0, landCenter = 0, landTop = 0;
            GetAverageZ(map, x, y, ref landZ, ref landCenter, ref landTop);

            bool moveIsOk = false;
            int stepTop = startTop + StepHeight;
            int checkTop = startZ + PersonHeight;
            bool ignoreDoors = false;
            bool ignoreSpellFields = true;

            if (!ignoreMobiles)
            {
                if (_worldService.Mobiles.Any(m => m.X == x && m.Y == y && m.Serial != _worldService.Player?.Serial))
                {
                    return false;
                }
            }

            var staticTiles = map.Tiles.GetStaticTiles(x, y, true);
            foreach (var tile in staticTiles)
            {
                var itemData = TileData.ItemTable[tile.Id & (TileData.ItemTable.Length - 1)];
                if ((itemData.Flags & ImpassableSurface) != TileFlag.Surface) continue;

                int itemZ = tile.Z;
                int itemTop = itemZ;
                int ourZ = itemZ + itemData.CalcHeight;
                int ourTop = ourZ + PersonHeight;
                int testTop = checkTop;

                if (moveIsOk)
                {
                    int cmp = Math.Abs(ourZ - (_worldService.Player?.Z ?? 0)) - Math.Abs(newZ - (_worldService.Player?.Z ?? 0));
                    if (cmp > 0 || (cmp == 0 && ourZ > newZ)) continue;
                }

                if (ourTop > testTop) testTop = ourTop;
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
                var itemData = TileData.ItemTable[item.Graphic & (TileData.ItemTable.Length - 1)];
                if ((itemData.Flags & ImpassableSurface) != TileFlag.Surface) continue;

                int itemZ = item.Z;
                int itemTop = itemZ;
                int ourZ = itemZ + itemData.CalcHeight;
                int ourTop = ourZ + PersonHeight;
                int testTop = checkTop;

                if (moveIsOk)
                {
                    int cmp = Math.Abs(ourZ - (_worldService.Player?.Z ?? 0)) - Math.Abs(newZ - (_worldService.Player?.Z ?? 0));
                    if (cmp > 0 || (cmp == 0 && ourZ > newZ)) continue;
                }

                if (ourTop > testTop) testTop = ourTop;
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

            int finalOurZ = landCenter;
            int finalOurTop = finalOurZ + PersonHeight;
            int finalTestTop = checkTop;
            if (finalOurTop > finalTestTop) finalTestTop = finalOurTop;

            bool shouldCheck = true;
            if (moveIsOk)
            {
                int cmp = Math.Abs(finalOurZ - (_worldService.Player?.Z ?? 0)) - Math.Abs(newZ - (_worldService.Player?.Z ?? 0));
                if (cmp > 0 || (cmp == 0 && finalOurZ > newZ)) shouldCheck = false;
            }

            if (!shouldCheck || !IsOk(ignoreDoors, ignoreSpellFields, finalOurZ, finalTestTop, staticTiles, items))
                return moveIsOk;

            newZ = finalOurZ;
            return true;
        }

        private bool IsOk(bool ignoreDoors, bool ignoreSpellFields, int ourZ, int ourTop, Ultima.HuedTile[] tiles, IEnumerable<TMRazorImproved.Shared.Models.Item> items)
        {
            foreach (var t in tiles)
            {
                var itemData = TileData.ItemTable[t.Id & (TileData.ItemTable.Length - 1)];
                if (t.Z + itemData.CalcHeight > ourZ && ourTop > t.Z && (itemData.Flags & ImpassableSurface) != 0)
                    return false;
            }
            foreach (var i in items)
            {
                var itemData = TileData.ItemTable[i.Graphic & (TileData.ItemTable.Length - 1)];
                if ((itemData.Flags & ImpassableSurface) == 0) continue;

                if (ignoreDoors && ((itemData.Flags & TileFlag.Door) != 0 || i.Graphic == 0x692 || i.Graphic == 0x846 || i.Graphic == 0x873 || (i.Graphic >= 0x6F5 && i.Graphic <= 0x6F6))) continue;
                if (ignoreSpellFields && (i.Graphic == 0x82 || i.Graphic == 0x3946 || i.Graphic == 0x3956)) continue;

                if (i.Z + itemData.CalcHeight > ourZ && ourTop > i.Z)
                    return false;
            }
            return true;
        }

        private void GetStartZ((int X, int Y, int Z) loc, Map map, IEnumerable<TMRazorImproved.Shared.Models.Item> itemList, out int zLow, out int zTop)
        {
            int xCheck = loc.X, yCheck = loc.Y;
            var landTile = map.Tiles.GetLandTile(xCheck, yCheck);
            var landData = TileData.LandTable[landTile.Id & (TileData.LandTable.Length - 1)];
            var landBlocks = (landData.Flags & TileFlag.Impassable) != 0;

            int landZ = 0, landCenter = 0, landTop = 0;
            GetAverageZ(map, xCheck, yCheck, ref landZ, ref landCenter, ref landTop);

            var considerLand = !Ignored(landTile);

            int zCenter = 0;
            zLow = zTop = 0;
            bool isSet = false;

            if (considerLand && !landBlocks && loc.Z >= landCenter)
            {
                zLow = landZ;
                zCenter = landCenter;
                zTop = landTop;
                isSet = true;
            }

            var staticTiles = map.Tiles.GetStaticTiles(xCheck, yCheck, true);
            foreach (var tile in staticTiles)
            {
                var tileData = TileData.ItemTable[tile.Id & (TileData.ItemTable.Length - 1)];
                var calcTop = tile.Z + tileData.CalcHeight;

                if (isSet && calcTop < zCenter) continue;
                if ((tileData.Flags & TileFlag.Surface) == 0) continue;
                if (loc.Z < calcTop) continue;

                zLow = tile.Z;
                zCenter = calcTop;
                var top = tile.Z + tileData.Height;
                if (!isSet || top > zTop) zTop = top;
                isSet = true;
            }

            foreach (var item in itemList)
            {
                var itemData = TileData.ItemTable[item.Graphic & (TileData.ItemTable.Length - 1)];
                var calcTop = item.Z + itemData.CalcHeight;

                if (isSet && calcTop < zCenter) continue;
                if ((itemData.Flags & TileFlag.Surface) == 0) continue;
                if (loc.Z < calcTop) continue;

                zLow = item.Z;
                zCenter = calcTop;
                var top = item.Z + itemData.Height;
                if (!isSet || top > zTop) zTop = top;
                isSet = true;
            }

            if (!isSet) zLow = zTop = loc.Z;
            else if (loc.Z > zTop) zTop = loc.Z;
        }

        private void GetAverageZ(Map map, int x, int y, ref int z, ref int avg, ref int top)
        {
            int zTop = map.Tiles.GetLandTile(x, y).Z;
            int zLeft = map.Tiles.GetLandTile(x, y + 1).Z;
            int zRight = map.Tiles.GetLandTile(x + 1, y).Z;
            int zBottom = map.Tiles.GetLandTile(x + 1, y + 1).Z;

            z = zTop;
            if (zLeft < z) z = zLeft;
            if (zRight < z) z = zRight;
            if (zBottom < z) z = zBottom;

            top = zTop;
            if (zLeft > top) top = zLeft;
            if (zRight > top) top = zRight;
            if (zBottom > top) top = zBottom;

            if (Math.Abs(zTop - zBottom) > Math.Abs(zLeft - zRight))
                avg = FloorAverage(zLeft, zRight);
            else
                avg = FloorAverage(zTop, zBottom);
        }

        private int FloorAverage(int a, int b)
        {
            int v = a + b;
            if (v < 0) --v;
            return v / 2;
        }

        private bool Ignored(Ultima.Tile tile)
        {
            return tile.Id == 2 || tile.Id == 0x1DB || (tile.Id >= 0x1AE && tile.Id <= 0x1B5);
        }
    }
}
