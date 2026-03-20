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
    /// <summary>Provides script access to items in the world and containers: find by graphic/serial/hue, move, use, inspect properties.</summary>
    public class ItemsApi
    {
        private readonly IWorldService _world;
        private readonly IPacketService _packet;
        private readonly ITargetingService _targeting;
        private readonly ScriptCancellationController _cancel;
        private readonly ILogger<ItemsApi>? _logger;
        private readonly IMessenger _messenger;
        private readonly IWeaponService? _weaponService;

        private static readonly List<int> _ignoreList = new();

        public ItemsApi(IWorldService world, IPacketService packet, ITargetingService targeting, ScriptCancellationController cancel,
            ILogger<ItemsApi>? logger = null, IMessenger? messenger = null, IWeaponService? weaponService = null)
        {
            _world = world;
            _packet = packet;
            _targeting = targeting;
            _cancel = cancel;
            _logger = logger;
            _messenger = messenger ?? WeakReferenceMessenger.Default;
            _weaponService = weaponService;
        }

        private ScriptItem? Wrap(Item? item) => item == null ? null : new ScriptItem(item, _world, _packet, _targeting);
        private List<ScriptItem> Wrap(IEnumerable<Item> items) => items.Select(i => new ScriptItem(i, _world, _packet, _targeting)).ToList();

        /// <summary>Finds an item by its unique serial number. Returns <c>null</c> if not found.</summary>
        public virtual ScriptItem? FindBySerial(uint serial)
        {
            _cancel.ThrowIfCancelled();
            return Wrap(_world.FindItem(serial));
        }

        /// <summary>Finds the first item with the given graphic ID anywhere in the world. Returns <c>null</c> if not found.</summary>
        public virtual ScriptItem? FindByID(int graphic)
        {
            _cancel.ThrowIfCancelled();
            if (_ignoreList.Contains(graphic)) return null;
            return Wrap(_world.Items.FirstOrDefault(i => i.Graphic == graphic));
        }

        /// <summary>Finds all items with the given graphic ID anywhere in the world.</summary>
        public virtual List<ScriptItem> FindAllByID(int graphic)
        {
            _cancel.ThrowIfCancelled();
            if (_ignoreList.Contains(graphic)) return new List<ScriptItem>();
            return Wrap(_world.Items.Where(i => i.Graphic == graphic));
        }

        /// <summary>Returns all items directly inside the player's backpack.</summary>
        public virtual List<ScriptItem> GetBackpackItems()
        {
            _cancel.ThrowIfCancelled();
            var bp = _world.Player?.Backpack;
            if (bp == null) return new List<ScriptItem>();
            return Wrap(_world.Items.Where(i => i.ContainerSerial == bp.Serial));
        }

        /// <summary>Finds the first item matching the given graphic, hue, container, and range constraints.</summary>
        /// <param name="graphic">Item graphic ID to match.</param>
        /// <param name="hue">Hue to filter by, or -1 to ignore.</param>
        /// <param name="container">Container serial to search within, or 0 for entire world.</param>
        /// <param name="range">Maximum distance from player, or -1 to ignore distance.</param>
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

        /// <summary>Finds the first item matching graphic and hue within a container, with optional recursive search.</summary>
        public virtual ScriptItem? FindByID(int graphic, int hue, uint container, bool recurse)
        {
            _cancel.ThrowIfCancelled();
            if (_ignoreList.Contains(graphic)) return null;
            IEnumerable<Item> searchSpace = container == 0 ? _world.Items : GetItemsInContainer(container, recurse);
            return Wrap(searchSpace.FirstOrDefault(i => i.Graphic == graphic && (hue == -1 || i.Hue == hue)));
        }

        /// <summary>Finds all items matching the given graphic and optional hue within a container or the entire world.</summary>
        public virtual List<ScriptItem> FindAllByID(int graphic, int hue = -1, uint container = 0, bool recurse = true)
        {
            _cancel.ThrowIfCancelled();
            if (_ignoreList.Contains(graphic)) return new List<ScriptItem>();
            IEnumerable<Item> searchSpace = container == 0 ? _world.Items : GetItemsInContainer(container, recurse);
            return Wrap(searchSpace.Where(i => i.Graphic == graphic && (hue == -1 || i.Hue == hue)));
        }

        /// <summary>Finds the first item whose graphic ID is in the given list, optionally filtered by hue and container.</summary>
        public virtual ScriptItem? FindByID(System.Collections.IEnumerable graphics, int hue = -1, uint container = 0, bool recurse = true)
        {
            _cancel.ThrowIfCancelled();
            var ids = ConvertToList(graphics);
            IEnumerable<Item> searchSpace = container == 0 ? _world.Items : GetItemsInContainer(container, recurse);
            return Wrap(searchSpace.FirstOrDefault(i => ids.Contains(i.Graphic) && !_ignoreList.Contains(i.Graphic) && (hue == -1 || i.Hue == hue)));
        }

        /// <summary>Finds all items whose graphic ID is in the given list, optionally filtered by hue and container.</summary>
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

        /// <summary>Waits until the server sends the contents of the specified container serial, or the timeout (ms) expires.</summary>
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

        /// <summary>Selects the item with the given serial as a targeting candidate. No-op in current implementation.</summary>
        /// <remarks>⚠️ Stub: returns placeholder value.</remarks>
        public virtual void Select(uint serial) { _cancel.ThrowIfCancelled(); }

        /// <summary>Returns the raw property string for the named property on an item or mobile by serial.</summary>
        /// <param name="serial">Serial of the item or mobile.</param>
        /// <param name="name">Property name to search for (case-insensitive).</param>
        public virtual string GetPropString(uint serial, string name)
        {
            _cancel.ThrowIfCancelled();
            var entity = _world.FindItem(serial) as TMRazorImproved.Shared.Models.UOEntity ?? _world.FindMobile(serial) as TMRazorImproved.Shared.Models.UOEntity;
            if (entity?.OPL == null) return string.Empty;
            var entry = entity.OPL.Properties.FirstOrDefault(p => p.Arguments.Contains(name, StringComparison.OrdinalIgnoreCase));
            return entry?.Arguments ?? string.Empty;
        }

        /// <summary>
        /// Waits up to <paramref name="timeout"/> ms for the OPL (object property list) of the given item to be received from the server.
        /// </summary>
        /// <param name="itemOrSerial">An Item object, uint serial, or int serial.</param>
        /// <param name="timeout">Maximum wait time in milliseconds.</param>
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

        /// <summary>Returns the numeric value of the named property from an item's OPL, parsing the first integer found.</summary>
        /// <param name="serial">Serial of the item or mobile to query.</param>
        /// <param name="name">Property name to look up (case-insensitive).</param>
        public virtual int GetPropValue(uint serial, string name)
        {
            _cancel.ThrowIfCancelled();
            string text = GetPropString(serial, name);
            if (string.IsNullOrEmpty(text)) return 0;
            var match = Regex.Match(text, @"([-+]?\d+)(?:\s*/\s*\d+)?");
            return match.Success && int.TryParse(match.Groups[1].Value, out int val) ? val : 0;
        }

        /// <summary>Returns true if an item with the given serial exists in the current world state.</summary>
        public virtual bool Exists(uint serial) { _cancel.ThrowIfCancelled(); return _world.FindItem(serial) != null; }
        /// <summary>Returns the OPL name of the item with the given serial, or empty string if not found.</summary>
        public virtual string GetName(uint serial) { _cancel.ThrowIfCancelled(); return _world.FindItem(serial)?.OPL?.GetNameOrEmpty() ?? string.Empty; }
        /// <summary>Returns the stack amount of the item with the given serial, or 0 if not found.</summary>
        public virtual int GetAmount(uint serial) { _cancel.ThrowIfCancelled(); return _world.FindItem(serial)?.Amount ?? 0; }

        /// <summary>Finds all items on the ground within the given tile range of the player.</summary>
        /// <param name="range">Maximum tile distance from the player.</param>
        public virtual List<ScriptItem> FindAllInRange(int range)
        {
            _cancel.ThrowIfCancelled();
            var player = _world.Player;
            if (player == null) return new List<ScriptItem>();
            return Wrap(_world.Items.Where(i => i.ContainerSerial == 0 && i.DistanceTo(player) <= range));
        }

        /// <summary>Returns all items in the world with the specified graphic ID.</summary>
        public virtual List<ScriptItem> FilterByGraphic(int graphic) { _cancel.ThrowIfCancelled(); return Wrap(_world.Items.Where(i => i.Graphic == (ushort)graphic)); }
        /// <summary>Returns all items in the world with the specified hue color.</summary>
        public virtual List<ScriptItem> FilterByHue(int hue) { _cancel.ThrowIfCancelled(); return Wrap(_world.Items.Where(i => i.Hue == (ushort)hue)); }

        /// <summary>Finds an item of the given graphic and optional hue in the player's backpack and double-clicks (uses) it.</summary>
        public virtual void UseType(int graphic, int hue = -1)
        {
            _cancel.ThrowIfCancelled();
            var bp = _world.Player?.Backpack;
            if (bp == null) return;
            var item = _world.Items.FirstOrDefault(i => i.ContainerSerial == bp.Serial && i.Graphic == (ushort)graphic && (hue == -1 || i.Hue == (ushort)hue));
            if (item != null) UseItem(item.Serial);
        }

        /// <summary>Double-clicks (uses) an item by its serial.</summary>
        public virtual void UseItem(uint serial) { _cancel.ThrowIfCancelled(); _packet.SendToServer(PacketBuilder.DoubleClick(serial)); }
        /// <summary>Single-clicks an item by its serial (requests name/OPL).</summary>
        public virtual void Click(uint serial) { _cancel.ThrowIfCancelled(); _packet.SendToServer(PacketBuilder.SingleClick(serial)); }
        /// <summary>Moves an item to a target container.</summary>
        public virtual void Move(uint serial, uint targetContainer, int amount = 1) { _cancel.ThrowIfCancelled(); _packet.SendToServer(PacketBuilder.LiftItem(serial, (ushort)amount)); _packet.SendToServer(PacketBuilder.DropToContainer(serial, targetContainer)); }
        /// <summary>Moves an item to a specific slot position within a container.</summary>
        public virtual void Move(uint serial, uint targetContainer, int amount, int x, int y) { _cancel.ThrowIfCancelled(); _packet.SendToServer(PacketBuilder.LiftItem(serial, (ushort)amount)); _packet.SendToServer(PacketBuilder.DropToContainer(serial, targetContainer, (ushort)x, (ushort)y)); }

        /// <summary>Returns true if a container with the given serial exists in the world or has child items.</summary>
        public virtual bool ContainerExists(uint serial) { _cancel.ThrowIfCancelled(); return _world.FindItem(serial) != null || _world.Items.Any(i => i.ContainerSerial == serial); }
        /// <summary>Returns all items directly inside the container with the given serial.</summary>
        public virtual List<ScriptItem> GetItems(uint containerSerial) { _cancel.ThrowIfCancelled(); return Wrap(_world.Items.Where(i => i.ContainerSerial == containerSerial)); }
        /// <summary>Returns true if the item with the given serial is directly inside the specified container.</summary>
        public virtual bool IsInContainer(uint serial, uint containerSerial) { _cancel.ThrowIfCancelled(); return _world.FindItem(serial)?.ContainerSerial == containerSerial; }
        /// <summary>Returns the serial of the container holding the given item, or 0 if it is on the ground.</summary>
        public virtual uint GetContainer(uint serial) { _cancel.ThrowIfCancelled(); return _world.FindItem(serial)?.ContainerSerial ?? 0; }

        /// <summary>Finds the item equipped by the player in the given equipment layer by numeric layer index.</summary>
        public virtual ScriptItem? FindByLayer(byte layer)
        {
            _cancel.ThrowIfCancelled();
            var player = _world.Player;
            if (player == null) return null;
            return Wrap(_world.Items.FirstOrDefault(i => i.Container == player.Serial && i.Layer == layer));
        }

        /// <summary>Finds the item equipped by the player in the named equipment layer (e.g. "LeftHand", "Helm").</summary>
        public virtual ScriptItem? FindByLayer(string layerName) => Enum.TryParse<TMRazorImproved.Shared.Enums.Layer>(layerName, true, out var layer) ? FindByLayer((byte)layer) : null;
        /// <summary>Returns true if the item with the given serial is on the ground (not inside any container).</summary>
        public virtual bool IsOnGround(uint serial) { _cancel.ThrowIfCancelled(); return _world.FindItem(serial)?.ContainerSerial == 0; }
        /// <summary>Returns the graphic (tile art) ID of the item with the given serial, or 0 if not found.</summary>
        public virtual int GetGraphic(uint serial) => _world.FindItem(serial)?.Graphic ?? 0;
        /// <summary>Returns the hue color of the item with the given serial, or 0 if not found.</summary>
        public virtual int GetHue(uint serial) => _world.FindItem(serial)?.Hue ?? 0;
        /// <summary>Returns the equipment layer index of the item with the given serial, or 0 if not found.</summary>
        public virtual int GetLayer(uint serial) => _world.FindItem(serial)?.Layer ?? 0;

        /// <summary>Waits up to <paramref name="timeout"/> ms for an item matching the given graphic and hue to appear in the specified container.</summary>
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

        /// <summary>Returns all items whose immediate container matches the given serial.</summary>
        public virtual List<ScriptItem> FilterByContainer(uint containerSerial) { _cancel.ThrowIfCancelled(); return Wrap(_world.Items.Where(i => i.ContainerSerial == containerSerial)); }
        /// <summary>Picks up (lifts) the item with the given serial, placing it in the cursor hand.</summary>
        public virtual void Lift(uint serial, int amount = 1) { _cancel.ThrowIfCancelled(); _packet.SendToServer(PacketBuilder.LiftItem(serial, (ushort)amount)); }
        /// <summary>Drops an item into the specified container, defaulting to amount 1.</summary>
        public virtual void Drop(uint serial, uint targetContainer, int amount = 1) => Move(serial, targetContainer, amount);
        /// <summary>Drops an item into the specified container at the given slot coordinates.</summary>
        public virtual void Drop(uint serial, uint targetContainer, int amount, int x, int y) => Move(serial, targetContainer, amount, x, y);
        /// <summary>Returns the number of items directly inside the specified container.</summary>
        public virtual int ContainerCount(uint containerSerial) { _cancel.ThrowIfCancelled(); return _world.Items.Count(i => i.ContainerSerial == containerSerial); }

        // FR-021: ContainerCount con filtro per graphic e hue (legacy semantics)
        /// <summary>
        /// Counts items matching the given graphic and hue inside a container, optionally recursive (FR-021).
        /// </summary>
        /// <param name="containerSerial">Container to search in.</param>
        /// <param name="itemid">Graphic ID to match (-1 = any).</param>
        /// <param name="color">Hue to match (-1 = any).</param>
        /// <param name="recursive">If true, searches nested containers too.</param>
        public virtual int ContainerCount(uint containerSerial, int itemid, int color = -1, bool recursive = true)
        {
            _cancel.ThrowIfCancelled();
            return CountInContainer(containerSerial, itemid, color, recursive);
        }

        private int CountInContainer(uint containerSerial, int itemid, int color, bool recursive)
        {
            int count = 0;
            foreach (var item in _world.Items.Where(i => i.ContainerSerial == containerSerial))
            {
                if ((itemid < 0 || item.Graphic == (ushort)itemid) && (color < 0 || item.Hue == (ushort)color))
                    count++;
                if (recursive && item.IsContainer)
                    count += CountInContainer(item.Serial, itemid, color, true);
            }
            return count;
        }

        // FR-021 int overload
        /// <summary>Counts items matching the given graphic and hue inside a container, optionally recursive.</summary>
        public virtual int ContainerCount(int containerSerial, int itemid, int color = -1, bool recursive = true)
            => ContainerCount((uint)containerSerial, itemid, color, recursive);

        /// <summary>Returns all items matching the optional graphic, hue, container, and range filters.</summary>
        public virtual List<ScriptItem> ApplyFilter(int graphic = -1, int hue = -1, uint container = 0, int range = -1)
        {
            _cancel.ThrowIfCancelled();
            var player = _world.Player;
            return Wrap(_world.Items.Where(i => (graphic == -1 || i.Graphic == (ushort)graphic) && (hue == -1 || i.Hue == (ushort)hue) && (container == 0 || i.ContainerSerial == container) && (range == -1 || (player != null && i.DistanceTo(player) <= range))));
        }

        /// <summary>Returns all items matching all criteria defined in the given <see cref="ItemsFilter"/> object.</summary>
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

        /// <summary>Returns the total amount of items with the given graphic (and optional hue) in the player's backpack.</summary>
        public virtual int BackpackCount(int graphic, int hue = -1)
        {
            _cancel.ThrowIfCancelled();
            var bp = _world.Player?.Backpack;
            if (bp == null) return 0;
            return _world.Items.Where(i => i.ContainerSerial == bp.Serial && i.Graphic == (ushort)graphic && (hue == -1 || i.Hue == (ushort)hue)).Sum(i => (int)i.Amount);
        }

        /// <summary>Sends a packet to change the color of a dyeing tub item to the specified hue.</summary>
        public virtual void ChangeDyeingTubColor(uint serial, int color) { _cancel.ThrowIfCancelled(); byte[] pkt = new byte[7]; pkt[0] = 0x3B; System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(1), serial); System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(5), (ushort)color); _packet.SendToServer(pkt); }
        /// <summary>Sends a close-container packet to the server for the given container serial.</summary>
        public virtual void Close(uint serial) { _cancel.ThrowIfCancelled(); _packet.SendToServer(PacketBuilder.CloseContainer(serial)); }

        /// <summary>Requests the context menu for the given serial and returns the response code for the named entry, or -1 if not found.</summary>
        public virtual int ContextExist(uint serial, string name)
        {
            _cancel.ThrowIfCancelled();
            ContextMenuStore.Clear();
            byte[] pkt = new byte[9]; pkt[0] = 0xBF; System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(1), 9); System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(3), 0x0E); System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(5), serial); _packet.SendToServer(pkt);
            var entries = ContextMenuStore.WaitForSerial(serial, 5000);
            var match = entries.FirstOrDefault(e => e.Entry.Equals(name, StringComparison.OrdinalIgnoreCase));
            return match != null ? match.Response : -1;
        }

        /// <summary>Returns the tile distance between two entities by their serials, or 999 if either is not found.</summary>
        public virtual int DistanceTo(uint serial1, uint serial2) { _cancel.ThrowIfCancelled(); var e1 = _world.FindEntity(serial1); var e2 = _world.FindEntity(serial2); return (e1 == null || e2 == null) ? 999 : e1.DistanceTo(e2); }
        /// <summary>Drops the currently held item (from the cursor hand) into the player's backpack.</summary>
        public virtual void DropFromHand(uint serial) { _cancel.ThrowIfCancelled(); var bp = _world.Player?.Backpack; if (bp != null) Drop(serial, bp.Serial); }
        /// <summary>Drops the given item on the ground at the player's current map position.</summary>
        public virtual void DropItemGroundSelf(uint serial, int amount = 1) { _cancel.ThrowIfCancelled(); var p = _world.Player; if (p != null) { _packet.SendToServer(PacketBuilder.LiftItem(serial, (ushort)amount)); _packet.SendToServer(PacketBuilder.DropToWorld(serial, (ushort)p.X, (ushort)p.Y, (short)p.Z)); } }
        /// <summary>Finds the first item whose display name or OPL name contains the given string (case-insensitive).</summary>
        public virtual ScriptItem? FindByName(string name, bool recurse = true) { _cancel.ThrowIfCancelled(); return Wrap(_world.Items.FirstOrDefault(i => (i.Name != null && i.Name.Contains(name, StringComparison.OrdinalIgnoreCase)) || (i.OPL != null && i.OPL.GetNameOrEmpty().Contains(name, StringComparison.OrdinalIgnoreCase)))); }
        /// <summary>Returns all OPL property strings for the given entity serial. Alias for <see cref="GetPropStringList"/>.</summary>
        public virtual List<string> GetProperties(uint serial) => GetPropStringList(serial);
        /// <summary>Returns the full list of OPL property argument strings for the given entity serial.</summary>
        public virtual List<string> GetPropStringList(uint serial) { _cancel.ThrowIfCancelled(); var e = _world.FindEntity(serial); return e?.OPL?.Properties.Select(p => p.Arguments).ToList() ?? new List<string>(); }
        /// <summary>Returns the OPL property argument string at the given zero-based index for the entity serial.</summary>
        public virtual string GetPropStringByIndex(uint serial, int index) { _cancel.ThrowIfCancelled(); var e = _world.FindEntity(serial); var props = e?.OPL?.Properties; return (props != null && index >= 0 && index < props.Count) ? props[index].Arguments : string.Empty; }
        /// <summary>Returns the raw property argument string for the named property. Alias for <see cref="GetPropString"/>.</summary>
        public virtual string GetPropValueString(uint serial, string name) => GetPropString(serial, name);
        /// <summary>Returns the world map position of the item with the given serial as a <see cref="Point3D"/>.</summary>
        public virtual Point3D GetWorldPosition(uint serial) { _cancel.ThrowIfCancelled(); var item = _world.FindItem(serial); return item == null ? new Point3D(0, 0, 0) : new Point3D(item.X, item.Y, item.Z); }
        /// <summary>Sends a remove-object packet to the client, making the item invisible locally without affecting the server.</summary>
        public virtual void Hide(uint serial) { _cancel.ThrowIfCancelled(); _packet.SendToClient(PacketBuilder.RemoveObject(serial)); }
        /// <summary>Adds the given graphic IDs to the ignore list so they are excluded from FindByID searches.</summary>
        public virtual void IgnoreTypes(List<int> graphics) { _cancel.ThrowIfCancelled(); if (graphics == null) return; foreach (var g in graphics) if (!_ignoreList.Contains(g)) _ignoreList.Add(g); }
        /// <summary>Returns true if the item with <paramref name="childSerial"/> is contained (directly or recursively) within the item with <paramref name="parentSerial"/>.</summary>
        public virtual bool IsChildOf(uint childSerial, uint parentSerial) { _cancel.ThrowIfCancelled(); var child = _world.FindItem(childSerial); if (child == null) return false; if (child.ContainerSerial == parentSerial) return true; if (child.ContainerSerial == 0) return false; return IsChildOf(child.ContainerSerial, parentSerial); }
        /// <summary>Sends an overhead or speech message associated with the given item serial.</summary>
        /// <remarks>⚠️ Stub: returns placeholder value.</remarks>
        public virtual void Message(uint serial, int hue, string message) { /* Implementazione 0xAE */ }
        /// <summary>Moves an item to the given world coordinate with amount 1.</summary>
        public virtual void MoveOnGround(uint serial, int x, int y, int z) => MoveOnGround(serial, x, y, z, 1);
        /// <summary>Moves the given amount of an item to the specified world (X, Y, Z) coordinate.</summary>
        public virtual void MoveOnGround(uint serial, int x, int y, int z, int amount) { _cancel.ThrowIfCancelled(); _packet.SendToServer(PacketBuilder.LiftItem(serial, (ushort)amount)); _packet.SendToServer(PacketBuilder.DropToWorld(serial, (ushort)x, (ushort)y, (short)z)); }
        /// <summary>Double-clicks a container item, opening it at the given screen coordinates (coordinates are informational only).</summary>
        public virtual void OpenAt(uint serial, int x, int y) { _cancel.ThrowIfCancelled(); _packet.SendToServer(PacketBuilder.DoubleClick(serial)); }
        /// <summary>Opens a container at the given screen coordinates. Alias for <see cref="OpenAt"/>.</summary>
        public virtual void OpenContainerAt(uint serial, int x, int y) => OpenAt(serial, x, y);
        /// <summary>Sets the local hue of the item with the given serial (client-side only, not persisted to server).</summary>
        public virtual void SetColor(uint serial, int color) { _cancel.ThrowIfCancelled(); var item = _world.FindItem(serial); if (item != null) item.Hue = (ushort)color; }
        /// <summary>Sends a single-click packet for the item with the given serial, requesting its name or OPL.</summary>
        public virtual void SingleClick(uint serial) { _cancel.ThrowIfCancelled(); _packet.SendToServer(PacketBuilder.SingleClick(serial)); }
        /// <summary>Finds an item by graphic and optional hue in the backpack, then double-clicks it. Alias for <see cref="UseType"/>.</summary>
        public virtual void UseItemByID(int graphic, int hue = -1) => UseType(graphic, hue);
        /// <summary>Sets the local hue of the item with the given serial. Alias for <see cref="SetColor"/>.</summary>
        public virtual void Color(uint serial, int color) => SetColor(serial, color);

        /// <summary>Creates and returns a new empty <see cref="ItemsFilter"/> instance for building item search criteria.</summary>
        public virtual ItemsFilter Filter() => new ItemsFilter();

        /// <summary>Defines filter criteria used with <see cref="ApplyFilter(ItemsFilter)"/> to query items by multiple properties.</summary>
        public class ItemsFilter
        {
            /// <summary>When false, the filter is skipped and <see cref="ItemsApi.ApplyFilter(ItemsFilter)"/> returns an empty list.</summary>
            public bool Enabled { get; set; } = true;
            /// <summary>List of graphic IDs to match. Empty list matches any graphic.</summary>
            public List<int> Graphics { get; set; } = new();
            /// <summary>Alias for <see cref="Graphics"/>; used in mobile-filter compatibility contexts.</summary>
            public List<int> Bodies { get => Graphics; set => Graphics = value; }
            /// <summary>List of hue values to match. Empty list matches any hue.</summary>
            public List<int> Hues { get; set; } = new();
            /// <summary>Container serial to restrict the search to. 0 means no container restriction.</summary>
            public uint Container { get; set; } = 0;
            /// <summary>Maximum tile distance from the player. -1 disables range filtering.</summary>
            public int Range { get; set; } = -1;
            /// <summary>Minimum tile distance from the player. -1 disables minimum range filtering.</summary>
            public int RangeMin { get; set; } = -1;
            /// <summary>Maximum tile distance from the player (used alongside RangeMin). -1 disables this check.</summary>
            public int RangeMax { get; set; } = -1;
            /// <summary>Partial name to match against item name or OPL name. Empty string disables name filtering.</summary>
            public string Name { get; set; } = string.Empty;
            /// <summary>1 = only on ground, -1 = only in container, 0 = no restriction.</summary>
            public int OnGround { get; set; } = 0;
            /// <summary>1 = only movable items, -1 = only non-movable items, 0 = no restriction.</summary>
            public int Movable { get; set; } = 0;
            /// <summary>1 = only containers, -1 = only non-containers, 0 = no restriction.</summary>
            public int IsContainer { get; set; } = 0;
            /// <summary>1 = only corpses, -1 = only non-corpses, 0 = no restriction.</summary>
            public int IsCorpse { get; set; } = 0;
            /// <summary>Reserved for future use; no effect in current implementation.</summary>
            public int IsDrop { get; set; } = 0;
            /// <summary>Root container serial to match against the item's top-level parent. 0 means no restriction.</summary>
            public uint Parent { get; set; } = 0;
            /// <summary>Serial to exclude from results. -1 disables exclusion.</summary>
            public int ExcludeSerial { get; set; } = -1;
        }

        // FR-018: Select(List<ScriptItem>, string selector) — selects best item from a list
        /// <summary>
        /// Selects and returns the best item from the list using the given selector strategy.
        /// Selectors: Nearest, Farthest, Less (min amount), Most (max amount), Lightest (min weight), Heaviest (max weight), Random.
        /// </summary>
        public virtual ScriptItem? Select(IEnumerable<ScriptItem> items, string selector)
        {
            _cancel.ThrowIfCancelled();
            var list = items?.ToList();
            if (list == null || list.Count == 0) return null;
            var player = _world.Player;
            int Dist(ScriptItem i) => player == null ? 0 : Math.Max(Math.Abs(i.X - player.X), Math.Abs(i.Y - player.Y));
            return selector.ToLowerInvariant() switch
            {
                "nearest"   => list.OrderBy(Dist).First(),
                "farthest"  => list.OrderByDescending(Dist).First(),
                "less"      => list.OrderBy(i => i.Amount).First(),
                "most"      => list.OrderByDescending(i => i.Amount).First(),
                "lightest"  => list.OrderBy(i => i.Weight).First(),
                "heaviest"  => list.OrderByDescending(i => i.Weight).First(),
                "random"    => list[new Random().Next(list.Count)],
                _           => list.OrderBy(Dist).First()
            };
        }

        /// <summary>Selects the best item from a Python/dynamic list using the given selector strategy (FR-018).</summary>
        public virtual ScriptItem? Select(System.Collections.IList items, string selector)
        {
            _cancel.ThrowIfCancelled();
            if (items == null) return null;
            var typed = items.Cast<ScriptItem>().ToList();
            // Call implementation directly to avoid ambiguity with IEnumerable<ScriptItem> overload
            return Select((IEnumerable<ScriptItem>)typed, selector);
        }

        // FR-020: GetWeaponAbility — returns (primary, secondary) from WeaponInfo
        /// <summary>
        /// Returns a tuple-like object with Primary and Secondary ability names for the given item graphic.
        /// Requires weapons.json data to be loaded (FR-020).
        /// </summary>
        public virtual (string Primary, string Secondary) GetWeaponAbility(int itemId)
        {
            _cancel.ThrowIfCancelled();
            if (_weaponService != null)
            {
                var info = _weaponService.GetWeaponInfo((ushort)itemId);
                if (info != null) return (info.Primary, info.Secondary);
            }
            return (string.Empty, string.Empty);
        }

        // FR-019: GetImage — returns a Bitmap of the item art with optional hue applied (mirrors legacy Item.GetImage)
        /// <summary>
        /// Returns a <see cref="Ultima.Data.Bitmap"/> of the static art for <paramref name="itemID"/> with the
        /// specified <paramref name="hue"/> applied.  When <paramref name="hue"/> is 0 the unmodified cached bitmap
        /// is returned.  Returns <c>null</c> if the art cannot be loaded.
        /// </summary>
        public virtual Ultima.Data.Bitmap? GetImage(int itemID, int hue = 0)
        {
            try
            {
                Ultima.Data.Bitmap? bitmapOriginal = Ultima.Art.GetStatic(itemID);
                if (bitmapOriginal == null) return null;

                if (hue <= 0) return bitmapOriginal;

                // Clone so the cached original is not modified
                Ultima.Data.Bitmap bitmapCopy = new(bitmapOriginal);
                bool onlyGray = (hue & 0x8000) != 0;
                int hueIndex = (hue & 0x3FFF) - 1;
                Ultima.Hues.GetHue(hueIndex).ApplyTo(bitmapCopy, onlyGray);
                return bitmapCopy;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Items.GetImage() failed for itemID={ItemID} hue={Hue}", itemID, hue);
                return null;
            }
        }

        #region int-serial overloads — RazorEnhanced compatibility (TASK-FR-012)
        public virtual ScriptItem? FindBySerial(int serial) => FindBySerial((uint)serial);
        public virtual ScriptItem? FindByID(int graphic, int hue, int container, int range) => FindByID(graphic, hue, (uint)container, range);
        public virtual ScriptItem? FindByID(int graphic, int hue, int container, bool recurse) => FindByID(graphic, hue, (uint)container, recurse);
        public virtual List<ScriptItem> FindAllByID(int graphic, int hue, int container, bool recurse = true) => FindAllByID(graphic, hue, (uint)container, recurse);
        public virtual ScriptItem? FindByID(System.Collections.IEnumerable graphics, int hue, int container, bool recurse = true) => FindByID(graphics, hue, (uint)container, recurse);
        public virtual List<ScriptItem> FindAllByID(System.Collections.IEnumerable graphics, int hue, int container, bool recurse = true) => FindAllByID(graphics, hue, (uint)container, recurse);
        public virtual bool WaitForContents(int serial, int timeout = 5000) => WaitForContents((uint)serial, timeout);
        public virtual bool WaitForID(int graphic, int hue, int container, int timeout = 5000) => WaitForID(graphic, hue, (uint)container, timeout);
        public virtual void UseItem(int serial) => UseItem((uint)serial);
        public virtual void Click(int serial) => Click((uint)serial);
        public virtual void Move(int serial, int targetContainer, int amount = 1) => Move((uint)serial, (uint)targetContainer, amount);
        public virtual void Move(int serial, int targetContainer, int amount, int x, int y) => Move((uint)serial, (uint)targetContainer, amount, x, y);
        public virtual void Lift(int serial, int amount = 1) => Lift((uint)serial, amount);
        public virtual void Drop(int serial, int targetContainer, int amount = 1) => Drop((uint)serial, (uint)targetContainer, amount);
        public virtual void Drop(int serial, int targetContainer, int amount, int x, int y) => Drop((uint)serial, (uint)targetContainer, amount, x, y);
        public virtual int ContainerCount(int containerSerial) => ContainerCount((uint)containerSerial);
        public virtual List<ScriptItem> GetItems(int containerSerial) => GetItems((uint)containerSerial);
        public virtual bool IsInContainer(int serial, int containerSerial) => IsInContainer((uint)serial, (uint)containerSerial);
        public virtual uint GetContainer(int serial) => GetContainer((uint)serial);
        public virtual List<ScriptItem> FilterByContainer(int containerSerial) => FilterByContainer((uint)containerSerial);
        public virtual void Hide(int serial) => Hide((uint)serial);
        public virtual void Close(int serial) => Close((uint)serial);
        public virtual List<string> GetPropStringList(int serial) => GetPropStringList((uint)serial);
        public virtual string GetPropStringByIndex(int serial, int index) => GetPropStringByIndex((uint)serial, index);
        public virtual string GetPropValueString(int serial, string name) => GetPropValueString((uint)serial, name);
        public virtual string GetPropString(int serial, string name) => GetPropString((uint)serial, name);
        public virtual int GetPropValue(int serial, string name) => GetPropValue((uint)serial, name);
        public virtual bool Exists(int serial) => Exists((uint)serial);
        public virtual string GetName(int serial) => GetName((uint)serial);
        public virtual int GetAmount(int serial) => GetAmount((uint)serial);
        public virtual bool IsOnGround(int serial) => IsOnGround((uint)serial);
        public virtual int GetGraphic(int serial) => GetGraphic((uint)serial);
        public virtual int GetHue(int serial) => GetHue((uint)serial);
        public virtual int GetLayer(int serial) => GetLayer((uint)serial);
        public virtual Point3D GetWorldPosition(int serial) => GetWorldPosition((uint)serial);
        public virtual void SingleClick(int serial) => SingleClick((uint)serial);
        public virtual void SetColor(int serial, int color) => SetColor((uint)serial, color);
        public virtual void Color(int serial, int color) => Color((uint)serial, color);
        public virtual void ChangeDyeingTubColor(int serial, int color) => ChangeDyeingTubColor((uint)serial, color);
        public virtual void OpenAt(int serial, int x, int y) => OpenAt((uint)serial, x, y);
        public virtual void OpenContainerAt(int serial, int x, int y) => OpenContainerAt((uint)serial, x, y);
        public virtual void DropItemGroundSelf(int serial, int amount = 1) => DropItemGroundSelf((uint)serial, amount);
        public virtual void DropFromHand(int serial) => DropFromHand((uint)serial);
        public virtual void MoveOnGround(int serial, int x, int y, int z) => MoveOnGround((uint)serial, x, y, z);
        public virtual void MoveOnGround(int serial, int x, int y, int z, int amount) => MoveOnGround((uint)serial, x, y, z, amount);
        public virtual void Message(int serial, int hue, string message) => Message((uint)serial, hue, message);
        public virtual int ContextExist(int serial, string name) => ContextExist((uint)serial, name);
        public virtual bool ContainerExists(int serial) => ContainerExists((uint)serial);
        public virtual int DistanceTo(int serial1, int serial2) => DistanceTo((uint)serial1, (uint)serial2);
        public virtual List<string> GetProperties(int serial) => GetProperties((uint)serial);
        public virtual bool IsChildOf(int childSerial, int parentSerial) => IsChildOf((uint)childSerial, (uint)parentSerial);
        public virtual void Select(int serial) => Select((uint)serial);
        #endregion
    }
}
