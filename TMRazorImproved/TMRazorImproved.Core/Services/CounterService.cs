using System;
using System.Collections.Generic;
using System.Threading;
using CommunityToolkit.Mvvm.Messaging;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Messages;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.Core.Services
{
    public class CounterService : ICounterService,
                                  IRecipient<ContainerItemAddedMessage>,
                                  IRecipient<ContainerContentMessage>,
                                  IRecipient<LoginCompleteMessage>
    {
        private readonly IWorldService _worldService;
        private readonly Dictionary<(ushort, ushort), int> _counts = new();

        // Throttle: non ricarichiamo più di una volta ogni 500ms
        private System.Threading.Timer? _debounceTimer;
        private const int DebounceMs = 500;

        public event Action<ushort, ushort, int>? CounterChanged;

        public CounterService(IWorldService worldService, IMessenger messenger)
        {
            _worldService = worldService;
            messenger.RegisterAll(this);
        }

        // ── IRecipient handlers ───────────────────────────────────────────────────────

        public void Receive(ContainerItemAddedMessage message) => ScheduleRecalculate();

        public void Receive(ContainerContentMessage message) => ScheduleRecalculate();

        public void Receive(LoginCompleteMessage message) => ScheduleRecalculate();

        // ─────────────────────────────────────────────────────────────────────────────

        public int GetCount(ushort graphic, ushort hue = 0)
        {
            lock (_counts)
            {
                return _counts.TryGetValue((graphic, hue), out int count) ? count : 0;
            }
        }

        /// <summary>
        /// Schedula un ricalcolo dei counter con debounce di 500ms per evitare
        /// aggiornamenti troppo frequenti durante il login o batch di item.
        /// </summary>
        private void ScheduleRecalculate()
        {
            _debounceTimer?.Dispose();
            _debounceTimer = new System.Threading.Timer(_ => RecalculateAll(), null, DebounceMs, System.Threading.Timeout.Infinite);
        }

        public void RecalculateAll()
        {
            if (_worldService.Player?.Backpack == null) return;

            var backpackSerial = _worldService.Player.Backpack.Serial;
            var newCounts = new Dictionary<(ushort, ushort), int>();

            ScanContainerRecursive(backpackSerial, newCounts);

            lock (_counts)
            {
                // Notifica cambiamenti verso l'alto
                foreach (var kvp in newCounts)
                {
                    if (!_counts.TryGetValue(kvp.Key, out int oldVal) || oldVal != kvp.Value)
                        CounterChanged?.Invoke(kvp.Key.Item1, kvp.Key.Item2, kvp.Value);
                }

                // Notifica item ora a 0
                foreach (var kvp in _counts)
                {
                    if (!newCounts.ContainsKey(kvp.Key) && kvp.Value != 0)
                        CounterChanged?.Invoke(kvp.Key.Item1, kvp.Key.Item2, 0);
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

                ScanContainerRecursive(item.Serial, counts);
            }
        }
    }
}
