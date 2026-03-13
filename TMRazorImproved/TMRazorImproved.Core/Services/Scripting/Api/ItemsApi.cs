using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Mvvm.Messaging;
using TMRazorImproved.Core.Utilities;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;
using TMRazorImproved.Shared.Messages;

namespace TMRazorImproved.Core.Services.Scripting.Api
{
    public class ItemsApi
    {
        private readonly IWorldService _world;
        private readonly IPacketService _packet;
        private readonly ITargetingService _targeting;
        private readonly ScriptCancellationController _cancel;
        private readonly ILogger<ItemsApi>? _logger;
        private readonly IMessenger _messenger;

        private static readonly List<int> _ignoreList = new();

        public ItemsApi(IWorldService world, IPacketService packet, ITargetingService targeting, ScriptCancellationController cancel,
            ILogger<ItemsApi>? logger = null, IMessenger? messenger = null)
        {
            _world = world;
            _packet = packet;
            _targeting = targeting;
            _cancel = cancel;
            _logger = logger;
            _messenger = messenger ?? WeakReferenceMessenger.Default;
        }

        private ScriptItem? Wrap(Item? item) => item == null ? null : new ScriptItem(item, _world, _packet, _targeting);
        private List<ScriptItem> Wrap(IEnumerable<Item> items) => items.Select(i => new ScriptItem(i, _world, _packet, _targeting)).ToList();

        public virtual ScriptItem? FindBySerial(uint serial)
        {
            _cancel.ThrowIfCancelled();
            return Wrap(_world.FindItem(serial));
        }

        public virtual ScriptItem? FindByID(int graphic)
        {
            _cancel.ThrowIfCancelled();
            if (_ignoreList.Contains(graphic)) return null;
            return Wrap(_world.Items.FirstOrDefault(i => i.Graphic == graphic));
        }

        public virtual List<ScriptItem> FindAllByID(int graphic)
        {
            _cancel.ThrowIfCancelled();
            if (_ignoreList.Contains(graphic)) return new List<ScriptItem>();
            return Wrap(_world.Items.Where(i => i.Graphic == graphic));
        }

        public virtual List<ScriptItem> GetBackpackItems()
        {
            _cancel.ThrowIfCancelled();
            var bp = _world.Player?.Backpack;
            if (bp == null) return new List<ScriptItem>();
            return Wrap(_world.Items.Where(i => i.ContainerSerial == bp.Serial));
        }

        public virtual ScriptItem? FindByID(int graphic, int hue = -1, uint container = 0, int range = -1)
        {
            _cancel.ThrowIfCancelled();
            if (_ignoreList.Contains(graphic)) return null;
            var player = _world.Player;
            IEnumerable<Item> searchSpace = container == 0 ? _world.Items : GetItemsInContainer(container, true);
            return Wrap(searchSpace.FirstOrDefault(i => 
                i.Graphic == graphic && 
                (hue == -1 || i.Hue == hue) && 
                (range == -1 || (player != null && i.DistanceTo(player) <= range))
            ));
        }

        public virtual ScriptItem? FindByID(int graphic, int hue, uint container, bool recurse)
        {
            _cancel.ThrowIfCancelled();
            if (_ignoreList.Contains(graphic)) return null;
            IEnumerable<Item> searchSpace = container == 0 ? _world.Items : GetItemsInContainer(container, recurse);
            return Wrap(searchSpace.FirstOrDefault(i => i.Graphic == graphic && (hue == -1 || i.Hue == hue)));
        }

        public virtual List<ScriptItem> FindAllByID(int graphic, int hue = -1, uint container = 0, bool recurse = true)
        {
            _cancel.ThrowIfCancelled();
            if (_ignoreList.Contains(graphic)) return new List<ScriptItem>();
            IEnumerable<Item> searchSpace = container == 0 ? _world.Items : GetItemsInContainer(container, recurse);
            return Wrap(searchSpace.Where(i => i.Graphic == graphic && (hue == -1 || i.Hue == hue)));
        }

        public virtual ScriptItem? FindByID(System.Collections.IEnumerable graphics, int hue = -1, uint container = 0, bool recurse = true)
        {
            _cancel.ThrowIfCancelled();
            var ids = ConvertToList(graphics);
            IEnumerable<Item> searchSpace = container == 0 ? _world.Items : GetItemsInContainer(container, recurse);
            return Wrap(searchSpace.FirstOrDefault(i => ids.Contains(i.Graphic) && !_ignoreList.Contains(i.Graphic) && (hue == -1 || i.Hue == hue)));
        }

        public virtual List<ScriptItem> FindAllByID(System.Collections.IEnumerable graphics, int hue = -1, uint container = 0, bool recurse = true)
        {
            _cancel.ThrowIfCancelled();
            var ids = ConvertToList(graphics);
            IEnumerable<Item> searchSpace = container == 0 ? _world.Items : GetItemsInContainer(container, recurse);
            return Wrap(searchSpace.Where(i => ids.Contains(i.Graphic) && !_ignoreList.Contains(i.Graphic) && (hue == -1 || i.Hue == hue)));
        }

        private List<int> ConvertToList(System.Collections.IEnumerable input)
        {
            var list = new List<int>();
            foreach (var item in input)
            {
                if (item is int i) list.Add(i);
                else if (item is IConvertible c) list.Add(Convert.ToInt32(c));
            }
            return list;
        }

        public virtual bool WaitForContents(uint serial, int timeout = 5000)
        {
            _cancel.ThrowIfCancelled();
            var deadline = Environment.TickCount64 + timeout;
            bool received = false;
            _messenger.Register<ItemsApi, ContainerContentMessage>(this, (r, m) => { if (m.Value.ContainerSerial == serial) received = true; });
            try {
                while (Environment.TickCount64 < deadline) {
                    _cancel.ThrowIfCancelled();
                    if (received) return true;
                    System.Threading.Thread.Sleep(50);
                }
                return false;
            } finally { _messenger.Unregister<ContainerContentMessage>(this); }
        }

        private IEnumerable<Item> GetItemsInContainer(uint containerSerial, bool recurse)
        {
            var children = _world.Items.Where(i => i.ContainerSerial == containerSerial).ToList();
            foreach (var child in children) {
                yield return child;
                if (recurse) {
                    foreach (var grandchild in GetItemsInContainer(child.Serial, true))
                        yield return grandchild;
                }
            }
        }

        public virtual void Select(uint serial) { _cancel.ThrowIfCancelled(); }

        public virtual string GetPropString(uint serial, string name)
        {
            _cancel.ThrowIfCancelled();
            var entity = _world.FindItem(serial) as TMRazorImproved.Shared.Models.UOEntity ?? _world.FindMobile(serial) as TMRazorImproved.Shared.Models.UOEntity;
            if (entity?.OPL == null) return string.Empty;
            var entry = entity.OPL.Properties.FirstOrDefault(p => p.Arguments.Contains(name, StringComparison.OrdinalIgnoreCase));
            return entry?.Arguments ?? string.Empty;
        }

        public virtual void WaitForProps(object itemOrSerial, int timeout = 1000)
        {
            _cancel.ThrowIfCancelled();
            uint serial = itemOrSerial switch { uint u => u, int i => (uint)i, Item itm => itm.Serial, _ => 0 };
            if (serial == 0) return;
            var entity = _world.FindItem(serial) as TMRazorImproved.Shared.Models.UOEntity ?? _world.FindMobile(serial) as TMRazorImproved.Shared.Models.UOEntity;
            if (entity == null || (entity.OPL != null && entity.OPL.Hash != 0)) return;
            _packet.SendToServer(Utilities.PacketBuilder.QueryProperties(serial));
            var deadline = Environment.TickCount64 + timeout;
            while (Environment.TickCount64 < deadline) {
                _cancel.ThrowIfCancelled();
                if (entity.OPL != null && entity.OPL.Hash != 0) return;
                System.Threading.Thread.Sleep(10);
            }
        }

        public virtual int GetPropValue(uint serial, string name)
        {
            _cancel.ThrowIfCancelled();
            string text = GetPropString(serial, name);
            if (string.IsNullOrEmpty(text)) return 0;
            var match = Regex.Match(text, @"([-+]?\d+)(?:\s*/\s*\d+)?");
            return match.Success && int.TryParse(match.Groups[1].Value, out int val) ? val : 0;
        }

        public virtual bool Exists(uint serial) { _cancel.ThrowIfCancelled(); return _world.FindItem(serial) != null; }
        public virtual string GetName(uint serial) { _cancel.ThrowIfCancelled(); return _world.FindItem(serial)?.OPL?.GetNameOrEmpty() ?? string.Empty; }
        public virtual int GetAmount(uint serial) { _cancel.ThrowIfCancelled(); return _world.FindItem(serial)?.Amount ?? 0; }

        public virtual List<ScriptItem> FindAllInRange(int range)
        {
            _cancel.ThrowIfCancelled();
            var player = _world.Player;
            if (player == null) return new List<ScriptItem>();
            return Wrap(_world.Items.Where(i => i.ContainerSerial == 0 && i.DistanceTo(player) <= range));
        }

        public virtual List<ScriptItem> FilterByGraphic(int graphic) { _cancel.ThrowIfCancelled(); return Wrap(_world.Items.Where(i => i.Graphic == (ushort)graphic)); }
        public virtual List<ScriptItem> FilterByHue(int hue) { _cancel.ThrowIfCancelled(); return Wrap(_world.Items.Where(i => i.Hue == (ushort)hue)); }

        public virtual void UseType(int graphic, int hue = -1)
        {
            _cancel.ThrowIfCancelled();
            var bp = _world.Player?.Backpack;
            if (bp == null) return;
            var item = _world.Items.FirstOrDefault(i => i.ContainerSerial == bp.Serial && i.Graphic == (ushort)graphic && (hue == -1 || i.Hue == (ushort)hue));
            if (item != null) UseItem(item.Serial);
        }

        public virtual void UseItem(uint serial) { _cancel.ThrowIfCancelled(); _packet.SendToServer(PacketBuilder.DoubleClick(serial)); }
        public virtual void Click(uint serial) { _cancel.ThrowIfCancelled(); _packet.SendToServer(PacketBuilder.SingleClick(serial)); }
        public virtual void Move(uint serial, uint targetContainer, int amount = 1) { _cancel.ThrowIfCancelled(); _packet.SendToServer(PacketBuilder.LiftItem(serial, (ushort)amount)); _packet.SendToServer(PacketBuilder.DropToContainer(serial, targetContainer)); }

        public virtual bool ContainerExists(uint serial) { _cancel.ThrowIfCancelled(); return _world.FindItem(serial) != null || _world.Items.Any(i => i.ContainerSerial == serial); }
        public virtual List<ScriptItem> GetItems(uint containerSerial) { _cancel.ThrowIfCancelled(); return Wrap(_world.Items.Where(i => i.ContainerSerial == containerSerial)); }
        public virtual bool IsInContainer(uint serial, uint containerSerial) { _cancel.ThrowIfCancelled(); return _world.FindItem(serial)?.ContainerSerial == containerSerial; }
        public virtual uint GetContainer(uint serial) { _cancel.ThrowIfCancelled(); return _world.FindItem(serial)?.ContainerSerial ?? 0; }

        public virtual ScriptItem? FindByLayer(byte layer)
        {
            _cancel.ThrowIfCancelled();
            var player = _world.Player;
            if (player == null) return null;
            return Wrap(_world.Items.FirstOrDefault(i => i.Container == player.Serial && i.Layer == layer));
        }

        public virtual ScriptItem? FindByLayer(string layerName) => Enum.TryParse<TMRazorImproved.Shared.Enums.Layer>(layerName, true, out var layer) ? FindByLayer((byte)layer) : null;
        public virtual bool IsOnGround(uint serial) { _cancel.ThrowIfCancelled(); return _world.FindItem(serial)?.ContainerSerial == 0; }
        public virtual int GetGraphic(uint serial) => _world.FindItem(serial)?.Graphic ?? 0;
        public virtual int GetHue(uint serial) => _world.FindItem(serial)?.Hue ?? 0;
        public virtual int GetLayer(uint serial) => _world.FindItem(serial)?.Layer ?? 0;

        public virtual bool WaitForID(int graphic, int hue = -1, uint container = 0, int timeout = 5000)
        {
            _cancel.ThrowIfCancelled();
            var deadline = Environment.TickCount64 + timeout;
            while (Environment.TickCount64 < deadline) {
                _cancel.ThrowIfCancelled();
                if (FindByID(graphic, hue, container) != null) return true;
                System.Threading.Thread.Sleep(50);
            }
            return false;
        }

        public virtual List<ScriptItem> FilterByContainer(uint containerSerial) { _cancel.ThrowIfCancelled(); return Wrap(_world.Items.Where(i => i.ContainerSerial == containerSerial)); }
        public virtual void Lift(uint serial, int amount = 1) { _cancel.ThrowIfCancelled(); _packet.SendToServer(PacketBuilder.LiftItem(serial, (ushort)amount)); }
        public virtual void Drop(uint serial, uint targetContainer, int amount = 1) => Move(serial, targetContainer, amount);
        public virtual int ContainerCount(uint containerSerial) { _cancel.ThrowIfCancelled(); return _world.Items.Count(i => i.ContainerSerial == containerSerial); }

        public virtual List<ScriptItem> ApplyFilter(int graphic = -1, int hue = -1, uint container = 0, int range = -1)
        {
            _cancel.ThrowIfCancelled();
            var player = _world.Player;
            return Wrap(_world.Items.Where(i => (graphic == -1 || i.Graphic == (ushort)graphic) && (hue == -1 || i.Hue == (ushort)hue) && (container == 0 || i.ContainerSerial == container) && (range == -1 || (player != null && i.DistanceTo(player) <= range))));
        }

        public virtual List<ScriptItem> ApplyFilter(ItemsFilter filter)
        {
            _cancel.ThrowIfCancelled();
            if (filter == null || !filter.Enabled) return new List<ScriptItem>();

            var player = _world.Player;
            return Wrap(_world.Items.Where(i =>
            {
                if (filter.Graphics.Count > 0 && !filter.Graphics.Contains(i.Graphic)) return false;
                if (filter.Hues.Count > 0 && !filter.Hues.Contains(i.Hue)) return false;
                if (filter.Container != 0 && i.ContainerSerial != filter.Container) return false;
                if (filter.Parent != 0 && i.RootContainer != filter.Parent) return false;

                if (filter.Range != -1 && player != null && i.DistanceTo(player) > filter.Range) return false;
                if (filter.RangeMin != -1 && player != null && i.DistanceTo(player) < filter.RangeMin) return false;
                if (filter.RangeMax != -1 && player != null && i.DistanceTo(player) > filter.RangeMax) return false;

                if (filter.OnGround == 1 && i.ContainerSerial != 0) return false;
                if (filter.OnGround == -1 && i.ContainerSerial == 0) return false;

                if (filter.Movable == 1 && !i.Movable) return false;
                if (filter.Movable == -1 && i.Movable) return false;

                if (filter.IsContainer == 1 && !i.IsContainer) return false;
                if (filter.IsContainer == -1 && i.IsContainer) return false;

                if (filter.IsCorpse == 1 && !i.IsCorpse) return false;
                if (filter.IsCorpse == -1 && i.IsCorpse) return false;

                if (filter.ExcludeSerial != -1 && i.Serial == (uint)filter.ExcludeSerial) return false;

                if (!string.IsNullOrEmpty(filter.Name) && (i.Name == null || !i.Name.Contains(filter.Name, StringComparison.OrdinalIgnoreCase))) return false;

                return true;
            }));
        }

        public virtual int BackpackCount(int graphic, int hue = -1)
        {
            _cancel.ThrowIfCancelled();
            var bp = _world.Player?.Backpack;
            if (bp == null) return 0;
            return _world.Items.Where(i => i.ContainerSerial == bp.Serial && i.Graphic == (ushort)graphic && (hue == -1 || i.Hue == (ushort)hue)).Sum(i => (int)i.Amount);
        }

        public virtual void ChangeDyeingTubColor(uint serial, int color) { _cancel.ThrowIfCancelled(); byte[] pkt = new byte[7]; pkt[0] = 0x3B; System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(1), serial); System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(5), (ushort)color); _packet.SendToServer(pkt); }
        public virtual void Close(uint serial) { _cancel.ThrowIfCancelled(); _packet.SendToServer(PacketBuilder.CloseContainer(serial)); }

        public virtual int ContextExist(uint serial, string name)
        {
            _cancel.ThrowIfCancelled();
            ContextMenuStore.Clear();
            byte[] pkt = new byte[9]; pkt[0] = 0xBF; System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(1), 9); System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(3), 0x0E); System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(5), serial); _packet.SendToServer(pkt);
            var entries = ContextMenuStore.WaitForSerial(serial, 5000);
            var match = entries.FirstOrDefault(e => e.Entry.Equals(name, StringComparison.OrdinalIgnoreCase));
            return match != null ? match.Response : -1;
        }

        public virtual int DistanceTo(uint serial1, uint serial2) { _cancel.ThrowIfCancelled(); var e1 = _world.FindEntity(serial1); var e2 = _world.FindEntity(serial2); return (e1 == null || e2 == null) ? 999 : e1.DistanceTo(e2); }
        public virtual void DropFromHand(uint serial) { _cancel.ThrowIfCancelled(); var bp = _world.Player?.Backpack; if (bp != null) Drop(serial, bp.Serial); }
        public virtual void DropItemGroundSelf(uint serial, int amount = 1) { _cancel.ThrowIfCancelled(); var p = _world.Player; if (p != null) { _packet.SendToServer(PacketBuilder.LiftItem(serial, (ushort)amount)); _packet.SendToServer(PacketBuilder.DropToWorld(serial, (ushort)p.X, (ushort)p.Y, (short)p.Z)); } }
        public virtual ScriptItem? FindByName(string name, bool recurse = true) { _cancel.ThrowIfCancelled(); return Wrap(_world.Items.FirstOrDefault(i => (i.Name != null && i.Name.Contains(name, StringComparison.OrdinalIgnoreCase)) || (i.OPL != null && i.OPL.GetNameOrEmpty().Contains(name, StringComparison.OrdinalIgnoreCase)))); }
        public virtual List<string> GetProperties(uint serial) => GetPropStringList(serial);
        public virtual List<string> GetPropStringList(uint serial) { _cancel.ThrowIfCancelled(); var e = _world.FindEntity(serial); return e?.OPL?.Properties.Select(p => p.Arguments).ToList() ?? new List<string>(); }
        public virtual string GetPropStringByIndex(uint serial, int index) { _cancel.ThrowIfCancelled(); var e = _world.FindEntity(serial); var props = e?.OPL?.Properties; return (props != null && index >= 0 && index < props.Count) ? props[index].Arguments : string.Empty; }
        public virtual string GetPropValueString(uint serial, string name) => GetPropString(serial, name);
        public virtual Point3D GetWorldPosition(uint serial) { _cancel.ThrowIfCancelled(); var item = _world.FindItem(serial); return item == null ? new Point3D(0, 0, 0) : new Point3D(item.X, item.Y, item.Z); }
        public virtual void Hide(uint serial) { _cancel.ThrowIfCancelled(); _packet.SendToClient(PacketBuilder.RemoveObject(serial)); }
        public virtual void IgnoreTypes(List<int> graphics) { _cancel.ThrowIfCancelled(); if (graphics == null) return; foreach (var g in graphics) if (!_ignoreList.Contains(g)) _ignoreList.Add(g); }
        public virtual bool IsChildOf(uint childSerial, uint parentSerial) { _cancel.ThrowIfCancelled(); var child = _world.FindItem(childSerial); if (child == null) return false; if (child.ContainerSerial == parentSerial) return true; if (child.ContainerSerial == 0) return false; return IsChildOf(child.ContainerSerial, parentSerial); }
        public virtual void Message(uint serial, int hue, string message) { /* Implementazione 0xAE */ }
        public virtual void MoveOnGround(uint serial, int x, int y, int z) { _cancel.ThrowIfCancelled(); _packet.SendToServer(PacketBuilder.LiftItem(serial, 1)); _packet.SendToServer(PacketBuilder.DropToWorld(serial, (ushort)x, (ushort)y, (short)z)); }
        public virtual void OpenAt(uint serial, int x, int y) { _cancel.ThrowIfCancelled(); _packet.SendToServer(PacketBuilder.DoubleClick(serial)); }
        public virtual void OpenContainerAt(uint serial, int x, int y) => OpenAt(serial, x, y);
        public virtual void SetColor(uint serial, int color) { _cancel.ThrowIfCancelled(); var item = _world.FindItem(serial); if (item != null) item.Hue = (ushort)color; }
        public virtual void SingleClick(uint serial) { _cancel.ThrowIfCancelled(); _packet.SendToServer(PacketBuilder.SingleClick(serial)); }
        public override string ToString() => "ItemsApi";
        public virtual void UseItemByID(int graphic, int hue = -1) => UseType(graphic, hue);
        public virtual void Color(uint serial, int color) => SetColor(serial, color);

        public virtual ItemsFilter Filter() => new ItemsFilter();

        public class ItemsFilter
        {
            public bool Enabled { get; set; } = true;
            public List<int> Graphics { get; set; } = new();
            public List<int> Bodies { get => Graphics; set => Graphics = value; }
            public List<int> Hues { get; set; } = new();
            public uint Container { get; set; } = 0;
            public int Range { get; set; } = -1;
            public int RangeMin { get; set; } = -1;
            public int RangeMax { get; set; } = -1;
            public string Name { get; set; } = string.Empty;
            public int OnGround { get; set; } = 0;
            public int Movable { get; set; } = 0;
            public int IsContainer { get; set; } = 0;
            public int IsCorpse { get; set; } = 0;
            public int IsDrop { get; set; } = 0;
            public uint Parent { get; set; } = 0;
            public int ExcludeSerial { get; set; } = -1;
        }
    }
}
