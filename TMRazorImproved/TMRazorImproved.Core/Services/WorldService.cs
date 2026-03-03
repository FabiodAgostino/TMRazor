using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.Core.Services
{
    public class WorldService : IWorldService
    {
        private readonly ConcurrentDictionary<uint, Mobile> _mobiles = new();
        private readonly ConcurrentDictionary<uint, Item> _items = new();
        
        public Mobile? Player { get; private set; }
        public UOGump? CurrentGump { get; private set; }

        public IEnumerable<Mobile> Mobiles => _mobiles.Values;
        public IEnumerable<Item> Items => _items.Values;

        public Mobile? FindMobile(uint serial) => _mobiles.GetValueOrDefault(serial);
        
        public Item? FindItem(uint serial) => _items.GetValueOrDefault(serial);

        public UOEntity? FindEntity(uint serial)
        {
            if (_mobiles.TryGetValue(serial, out var m)) return m;
            if (_items.TryGetValue(serial, out var i)) return i;
            return null;
        }

        public void AddMobile(Mobile mobile)
        {
            _mobiles.AddOrUpdate(mobile.Serial, mobile, (_, existing) => mobile);
        }

        public void AddItem(Item item)
        {
            _items.AddOrUpdate(item.Serial, item, (_, existing) => item);
        }

        public void RemoveMobile(uint serial) => _mobiles.TryRemove(serial, out _);

        public void RemoveItem(uint serial) => _items.TryRemove(serial, out _);

        public void SetPlayer(Mobile player)
        {
            Player = player;
            AddMobile(player);
        }

        public void SetCurrentGump(UOGump? gump)
        {
            CurrentGump = gump;
        }

        public void RemoveGump()
        {
            CurrentGump = null;
        }

        public void Clear()
        {
            _mobiles.Clear();
            _items.Clear();
            Player = null;
            CurrentGump = null;
        }
    }
}
