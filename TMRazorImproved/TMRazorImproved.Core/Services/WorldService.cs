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

        // FIX BUG-C04: campo volatile garantisce visibilità cross-thread senza lock.
        private volatile Mobile? _player;
        public Mobile? Player => _player;

        // FIX P0-03: backing field volatile per IsCasting — viene scritto dal packet handler thread
        // (0x12 C2S in WorldPacketHandler) e letto dagli script Python su thread separato.
        // Senza volatile il JIT può cachearlo in registro → lo script non vede mai l'aggiornamento.
        private volatile bool _isCasting;
        public bool IsCasting { get => _isCasting; set => _isCasting = value; }
        public UOGump? CurrentGump { get; private set; }
        public ConcurrentDictionary<uint, UOGump> OpenGumps { get; } = new();
        public uint LastOpenedContainer { get; private set; }

        public IEnumerable<Mobile> Mobiles => _mobiles.Values;
        public IEnumerable<Item> Items => _items.Values;
        public HashSet<uint> PartyMembers { get; } = new();

        public Mobile? FindMobile(uint serial) => _mobiles.GetValueOrDefault(serial);
        
        public Item? FindItem(uint serial) => _items.GetValueOrDefault(serial);

        public IEnumerable<Item> GetItemsInContainer(uint containerSerial)
        {
            return _items.Values.Where(i => i.Container == containerSerial);
        }

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

        public void AddPartyMember(uint serial)
        {
            lock (PartyMembers)
            {
                PartyMembers.Add(serial);
            }
        }

        public void RemovePartyMember(uint serial)
        {
            lock (PartyMembers)
            {
                PartyMembers.Remove(serial);
            }
        }

        public void ClearParty()
        {
            lock (PartyMembers)
            {
                PartyMembers.Clear();
            }
        }

        public void RemoveMobile(uint serial) => _mobiles.TryRemove(serial, out _);

        public void RemoveItem(uint serial) => _items.TryRemove(serial, out _);

        public void SetPlayer(Mobile player)
        {
            _player = player;
            AddMobile(player);
        }

        public void SetCurrentGump(UOGump? gump)
        {
            CurrentGump = gump;
            if (gump != null)
            {
                OpenGumps.AddOrUpdate(gump.GumpId, gump, (_, _) => gump);
            }
        }

        public void RemoveGump(uint gumpId)
        {
            OpenGumps.TryRemove(gumpId, out _);
            if (CurrentGump?.GumpId == gumpId)
            {
                CurrentGump = null;
            }
        }

        public void RemoveGump()
        {
            if (CurrentGump != null)
            {
                OpenGumps.TryRemove(CurrentGump.GumpId, out _);
            }
            CurrentGump = null;
        }

        public void SetLastOpenedContainer(uint serial)
        {
            LastOpenedContainer = serial;
        }

        public void Clear()
        {
            _mobiles.Clear();
            _items.Clear();
            _player = null;
            CurrentGump = null;
            OpenGumps.Clear();
        }
    }
}
