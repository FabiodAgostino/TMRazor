using System;
using System.Collections.Generic;
using System.Linq;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.Core.Services
{
    public class CounterService : ICounterService
    {
        private readonly IWorldService _worldService;
        private readonly Dictionary<(ushort, ushort), int> _counts = new();

        public event Action<ushort, ushort, int>? CounterChanged;

        public CounterService(IWorldService worldService)
        {
            _worldService = worldService;
        }

        public int GetCount(ushort graphic, ushort hue = 0)
        {
            lock (_counts)
            {
                if (_counts.TryGetValue((graphic, hue), out int count))
                    return count;
                return 0;
            }
        }

        public void RecalculateAll()
        {
            if (_worldService.Player?.Backpack == null) return;

            var backpackSerial = _worldService.Player.Backpack.Serial;
            var newCounts = new Dictionary<(ushort, ushort), int>();

            ScanContainerRecursive(backpackSerial, newCounts);

            lock (_counts)
            {
                // Find changes and notify
                foreach (var kvp in newCounts)
                {
                    if (!_counts.TryGetValue(kvp.Key, out int oldVal) || oldVal != kvp.Value)
                    {
                        CounterChanged?.Invoke(kvp.Key.Item1, kvp.Key.Item2, kvp.Value);
                    }
                }

                // Check for items that are now 0
                foreach (var kvp in _counts)
                {
                    if (!newCounts.ContainsKey(kvp.Key) && kvp.Value != 0)
                    {
                        CounterChanged?.Invoke(kvp.Key.Item1, kvp.Key.Item2, 0);
                    }
                }

                _counts.Clear();
                foreach (var kvp in newCounts) _counts[kvp.Key] = kvp.Value;
            }
        }

        private void ScanContainerRecursive(uint containerSerial, Dictionary<(ushort, ushort), int> counts)
        {
            var items = _worldService.GetItemsInContainer(containerSerial);
            foreach (var item in items)
            {
                var key = (item.Graphic, item.Hue);
                if (counts.ContainsKey(key))
                    counts[key] += item.Amount;
                else
                    counts[key] = item.Amount;

                // Recursive scan if it's a container
                // Note: We might need a better way to know if an item is a container (Graphic check)
                // For now, if there are items inside it, it's a container.
                ScanContainerRecursive(item.Serial, counts);
            }
        }
    }
}
