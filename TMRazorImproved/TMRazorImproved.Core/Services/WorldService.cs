using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.Messaging;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.Core.Services
{
    public class WorldService : IWorldService, 
        CommunityToolkit.Mvvm.Messaging.IRecipient<TMRazorImproved.Shared.Messages.BuffDebuffMessage>
    {
        private readonly ConcurrentDictionary<uint, Mobile> _mobiles = new();
        private readonly ConcurrentDictionary<uint, Item> _items = new();
        private readonly CommunityToolkit.Mvvm.Messaging.IMessenger _messenger;

        public WorldService(CommunityToolkit.Mvvm.Messaging.IMessenger messenger)
        {
            _messenger = messenger;
            _messenger.RegisterAll(this);
        }

        private static readonly Dictionary<ushort, string> _buffNames = new()
        {
            [1001] = "Dismount", [1002] = "Disarm", [1005] = "Night Sight", [1006] = "Death Strike",
            [1007] = "Evil Omen", [1008] = "Gump Mana", [1009] = "Regeneration", [1010] = "Divine Fury",
            [1011] = "Enemy Of One", [1012] = "Hiding", [1013] = "Meditation", [1014] = "Blood Oath Caster",
            [1015] = "Blood Oath", [1016] = "Corpse Skin", [1017] = "Mind rot", [1018] = "Pain Spike",
            [1019] = "Strangle", [1020] = "Gift of Renewal", [1021] = "Attune Weapon", [1022] = "Thunderstorm",
            [1023] = "Essence of Wind", [1024] = "Ethereal Voyage", [1025] = "Gift Of Life", [1026] = "Arcane Empowerment",
            [1027] = "Mortal Strike", [1028] = "Reactive Armor", [1029] = "Protection", [1030] = "Arch Protection",
            [1031] = "Magic Reflection", [1032] = "Incognito", [1033] = "Disguised", [1034] = "Animal Form",
            [1035] = "Polymorph", [1036] = "Invisibility", [1037] = "Paralyze", [1038] = "Poison",
            [1039] = "Bleed", [1040] = "Clumsy", [1041] = "Feeblemind", [1042] = "Weaken",
            [1043] = "Curse", [1044] = "Mass Curse", [1045] = "Agility", [1046] = "Cunning",
            [1047] = "Strength", [1048] = "Bless"
        };

        public void Receive(TMRazorImproved.Shared.Messages.BuffDebuffMessage message)
        {
            var (serial, type, added, duration) = message.Value;
            var mobile = FindMobile(serial);
            if (mobile == null) return;

            string name = _buffNames.TryGetValue(type, out var n) ? n : $"Buff_{type}";

            lock (mobile.ActiveBuffs)
            {
                if (added) mobile.ActiveBuffs[name] = duration;
                else mobile.ActiveBuffs.Remove(name);
            }
        }

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
        private readonly System.Collections.Concurrent.ConcurrentDictionary<uint, byte> _partyMembers = new();
        public IReadOnlyCollection<uint> PartyMembers => _partyMembers.Keys.ToList().AsReadOnly();

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

        public void AddPartyMember(uint serial) => _partyMembers.TryAdd(serial, 0);

        public void RemovePartyMember(uint serial) => _partyMembers.TryRemove(serial, out _);

        public void ClearParty() => _partyMembers.Clear();

        public bool IsPartyMember(uint serial) => _partyMembers.ContainsKey(serial);

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
