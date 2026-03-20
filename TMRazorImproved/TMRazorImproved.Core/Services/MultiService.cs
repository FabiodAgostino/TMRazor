using System.Collections.Concurrent;
using TMRazorImproved.Shared.Interfaces;
using Ultima;

namespace TMRazorImproved.Core.Services
{
    /// <summary>
    /// Tracks multi structures (houses and boats) in the game world.
    ///
    /// In UO, "multi" items are placed structures (houses, boats) identified by a graphic ID with bit 0x4000 set.
    /// Each multi occupies a rectangular area derived from the MultiComponentList in the .mul files.
    /// The bounding box (Corner1..Corner2) is used to check if a tile is inside any placed house.
    ///
    /// Legacy reference: <c>Assistant.World.Multis</c> + <c>Statics.CheckDeedHouse(x, y)</c>.
    /// </summary>
    public class MultiService : IMultiService
    {
        private record MultiEntry(int X1, int Y1, int X2, int Y2);

        private readonly ConcurrentDictionary<uint, MultiEntry> _multis = new();

        public void AddMulti(uint serial, int x, int y, ushort graphic)
        {
            try
            {
                // graphic already has bit 0x4000 set for multis; strip it to get the component list index
                int typeId = graphic & 0x3FFF;
                MultiComponentList info = Multis.GetComponents(typeId);

                if (info.Count == 0)
                    info = new MultiComponentList(typeId, x, y);

                const int stairSpace = 1;
                int x1 = x + info.Min.X;
                int y1 = y + info.Min.Y;
                int x2 = x + info.Max.X;
                int y2 = y + info.Max.Y + stairSpace;

                _multis[serial] = new MultiEntry(x1, y1, x2, y2);
            }
            catch
            {
                // UltimaSDK not initialized or graphic not in table — skip silently
            }
        }

        public void RemoveMulti(uint serial)
        {
            _multis.TryRemove(serial, out _);
        }

        public bool CheckDeedHouse(int x, int y)
        {
            foreach (var entry in _multis.Values)
            {
                if (x >= entry.X1 && x <= entry.X2 + 1 &&
                    y >= entry.Y1 && y <= entry.Y2 + 1)
                    return true;
            }
            return false;
        }

        public void Clear()
        {
            _multis.Clear();
        }
    }
}
