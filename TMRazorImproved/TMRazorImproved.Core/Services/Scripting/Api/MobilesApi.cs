using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using TMRazorImproved.Core.Utilities;
using TMRazorImproved.Shared.Enums;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.Core.Services.Scripting.Api
{
    /// <summary>Provides script access to all mobiles (players and NPCs) in the world: find, filter, query stats, classify, and interact.</summary>
    public class MobilesApi
    {
        private readonly IWorldService _world;
        private readonly IFriendsService _friends;
        private readonly IPacketService _packet;
        private readonly ITargetingService _targeting;
        private readonly ScriptCancellationController _cancel;
        private readonly ILogger<MobilesApi>? _logger;

        public MobilesApi(
            IWorldService world, 
            IFriendsService friends, 
            IPacketService packet,
            ITargetingService targeting,
            ScriptCancellationController cancel, 
            ILogger<MobilesApi>? logger = null)
        {
            _world = world;
            _friends = friends;
            _packet = packet;
            _targeting = targeting;
            _cancel = cancel;
            _logger = logger;
        }

        private ScriptMobile? Wrap(Mobile? m) => m == null ? null : new ScriptMobile(m, _world, _packet, _targeting);
        private List<ScriptMobile> Wrap(IEnumerable<Mobile> mobiles) => mobiles.Select(m => new ScriptMobile(m, _world, _packet, _targeting)).ToList();
        private ScriptItem? WrapItem(Item? i) => i == null ? null : new ScriptItem(i, _world, _packet, _targeting);

        /// <summary>Finds a mobile by its unique serial number. Returns <c>null</c> if not found.</summary>
        public virtual ScriptMobile? FindBySerial(uint serial)
        {
            _cancel.ThrowIfCancelled();
            var result = _world.FindMobile(serial);
            if (result == null) _logger?.LogDebug("FindBySerial: mobile 0x{Serial:X} not found", serial);
            return Wrap(result);
        }

        /// <summary>Finds the first mobile with the given body graphic ID. Returns <c>null</c> if not found.</summary>
        public virtual ScriptMobile? FindByID(int graphic)
        {
            _cancel.ThrowIfCancelled();
            return Wrap(_world.Mobiles.FirstOrDefault(m => m.Graphic == graphic));
        }

        /// <summary>Returns all mobiles with the given body graphic, optionally limited to a tile range from the player.</summary>
        public virtual List<ScriptMobile> FindAllByID(int graphic, int range = -1)
        {
            _cancel.ThrowIfCancelled();
            var player = _world.Player;
            return Wrap(_world.Mobiles
                .Where(m => m.Graphic == (ushort)graphic)
                .Where(m => range == -1 || (player != null && m.DistanceTo(player) <= range)));
        }

        /// <summary>Returns the tile distance from the player to the mobile with the given serial, or 999 if not found.</summary>
        public virtual int GetDistance(uint serial)
        {
            _cancel.ThrowIfCancelled();
            var m = _world.FindMobile(serial);
            var p = _world.Player;
            if (m == null || p == null) return 999;
            return m.DistanceTo(p);
        }

        /// <summary>Returns all mobiles within the specified tile range of the player, excluding the player itself.</summary>
        public virtual List<ScriptMobile> FindInRange(int range)
        {
            _cancel.ThrowIfCancelled();
            var player = _world.Player;
            if (player == null) return new List<ScriptMobile>();

            return Wrap(_world.Mobiles
                .Where(m => m.Serial != player.Serial && m.DistanceTo(player) <= range));
        }

        /// <summary>Returns the closest mobile to the player, excluding the player themselves.</summary>
        public virtual ScriptMobile? FindNearest()
        {
            _cancel.ThrowIfCancelled();
            var player = _world.Player;
            if (player == null) return null;

            return Wrap(_world.Mobiles
                .Where(m => m.Serial != player.Serial)
                .OrderBy(m => m.DistanceTo(player))
                .FirstOrDefault());
        }

        /// <summary>Returns the nearest hostile mobile (notoriety 3–6: gray, criminal, enemy, red).</summary>
        public virtual ScriptMobile? FindNearestEnemy()
        {
            _cancel.ThrowIfCancelled();
            var player = _world.Player;
            if (player == null) return null;

            return Wrap(_world.Mobiles
                .Where(m => m.Serial != player.Serial && (m.Notoriety >= 3 && m.Notoriety <= 6))
                .OrderBy(m => m.DistanceTo(player))
                .FirstOrDefault());
        }

        /// <summary>Returns the nearest friendly mobile (in the friend list, or notoriety 1–2: blue or green).</summary>
        public virtual ScriptMobile? FindNearestFriend()
        {
            _cancel.ThrowIfCancelled();
            var player = _world.Player;
            if (player == null) return null;

            return Wrap(_world.Mobiles
                .Where(m => m.Serial != player.Serial && (_friends.IsFriend(m.Serial) || m.Notoriety == 1 || m.Notoriety == 2))
                .OrderBy(m => m.DistanceTo(player))
                .FirstOrDefault());
        }

        /// <summary>Returns all mobiles whose body graphic matches the given value.</summary>
        public virtual List<ScriptMobile> FilterByBody(int body)
        {
            _cancel.ThrowIfCancelled();
            return Wrap(_world.Mobiles.Where(m => m.Graphic == (ushort)body));
        }


        /// <summary>Returns true if the mobile with the given serial exists and has HP greater than 0.</summary>
        public virtual bool IsAlive(uint serial)
        {
            _cancel.ThrowIfCancelled();
            var m = _world.FindMobile(serial);
            return m != null && m.Hits > 0;
        }

        /// <summary>Returns true if the mobile with the given serial has HP of 0 or is not found in the world.</summary>
        public virtual bool IsDead(uint serial)
        {
            _cancel.ThrowIfCancelled();
            var m = _world.FindMobile(serial);
            if (m == null) return true;
            return m.Hits <= 0;
        }

        /// <summary>Returns true if the mobile with the given serial is in the active friend list.</summary>
        public virtual bool IsFriend(uint serial)
        {
            _cancel.ThrowIfCancelled();
            return _friends.IsFriend(serial);
        }

        /// <summary>Returns the current HP of the mobile as a percentage (0–100), or 0 if not found.</summary>
        public virtual int GetHealthPercent(uint serial)
        {
            _cancel.ThrowIfCancelled();
            var m = _world.FindMobile(serial);
            if (m == null || m.HitsMax <= 0) return 0;
            return (int)((m.Hits * 100.0) / m.HitsMax);
        }

        /// <summary>Returns all mobiles with the given notoriety value (1=blue, 2=green, 3=gray, 4=criminal, 5=enemy, 6=red, 7=invited).</summary>
        public virtual List<ScriptMobile> FilterByNotoriety(int notoriety)
        {
            _cancel.ThrowIfCancelled();
            return Wrap(_world.Mobiles.Where(m => m.Notoriety == notoriety));
        }

        /// <summary>Returns all mobiles whose tile distance from the player falls within [minRange, maxRange].</summary>
        public virtual List<ScriptMobile> FilterByDistance(int minRange, int maxRange)
        {
            _cancel.ThrowIfCancelled();
            var player = _world.Player;
            if (player == null) return new List<ScriptMobile>();

            return Wrap(_world.Mobiles.Where(m => {
                int dist = m.DistanceTo(player);
                return dist >= minRange && dist <= maxRange;
            }));
        }

        // ------------------------------------------------------------------
        // Proprietà individuali da serial
        // ------------------------------------------------------------------

        /// <summary>Returns the name of the mobile with the given serial, or empty string if not found.</summary>
        public virtual string GetName(uint serial)
        {
            _cancel.ThrowIfCancelled();
            return _world.FindMobile(serial)?.Name ?? string.Empty;
        }

        /// <summary>Returns the body graphic ID of the mobile with the given serial, or 0 if not found.</summary>
        public virtual int GetGraphic(uint serial)
        {
            _cancel.ThrowIfCancelled();
            return _world.FindMobile(serial)?.Graphic ?? 0;
        }

        /// <summary>Returns the hue color of the mobile with the given serial, or 0 if not found.</summary>
        public virtual int GetHue(uint serial)
        {
            _cancel.ThrowIfCancelled();
            return _world.FindMobile(serial)?.Hue ?? 0;
        }

        /// <summary>Returns the notoriety value of the mobile (1=blue, 2=green, 3=gray, 4=criminal, 5=enemy, 6=red, 7=invited), or 0 if not found.</summary>
        public virtual int GetNotoriety(uint serial)
        {
            _cancel.ThrowIfCancelled();
            return _world.FindMobile(serial)?.Notoriety ?? 0;
        }

        /// <summary>Returns true if the mobile is currently in war mode.</summary>
        public virtual bool IsWarMode(uint serial)
        {
            _cancel.ThrowIfCancelled();
            return _world.FindMobile(serial)?.WarMode ?? false;
        }

        /// <summary>Returns true if the mobile is currently poisoned.</summary>
        public virtual bool IsPoisoned(uint serial)
        {
            _cancel.ThrowIfCancelled();
            return _world.FindMobile(serial)?.IsPoisoned ?? false;
        }

        /// <summary>Returns true if the mobile is currently hidden (invisible).</summary>
        public virtual bool IsHidden(uint serial)
        {
            _cancel.ThrowIfCancelled();
            return _world.FindMobile(serial)?.IsHidden ?? false;
        }

        /// <summary>Returns true if the mobile with the given serial is a member of the player's party.</summary>
        public virtual bool IsParty(uint serial)
        {
            _cancel.ThrowIfCancelled();
            return _world.IsPartyMember(serial);
        }

        /// <summary>Returns the current mana of the mobile with the given serial, or 0 if not known.</summary>
        public virtual int GetMana(uint serial)
        {
            _cancel.ThrowIfCancelled();
            return _world.FindMobile(serial)?.Mana ?? 0;
        }

        /// <summary>Returns the current mana of the mobile as a percentage (0–100), or 0 if not known.</summary>
        public virtual int GetManaPercent(uint serial)
        {
            _cancel.ThrowIfCancelled();
            var m = _world.FindMobile(serial);
            if (m == null || m.ManaMax == 0) return 0;
            return (int)((double)m.Mana / m.ManaMax * 100);
        }

        /// <summary>Returns the current stamina of the mobile with the given serial, or 0 if not known.</summary>
        public virtual int GetStam(uint serial)
        {
            _cancel.ThrowIfCancelled();
            return _world.FindMobile(serial)?.Stam ?? 0;
        }

        /// <summary>Returns the current stamina of the mobile as a percentage (0–100), or 0 if not known.</summary>
        public virtual int GetStamPercent(uint serial)
        {
            _cancel.ThrowIfCancelled();
            var m = _world.FindMobile(serial);
            if (m == null || m.StamMax == 0) return 0;
            return (int)((double)m.Stam / m.StamMax * 100);
        }

        /// <summary>Returns the current X map coordinate of the mobile with the given serial.</summary>
        public virtual int GetX(uint serial)
        {
            _cancel.ThrowIfCancelled();
            return _world.FindMobile(serial)?.X ?? 0;
        }

        /// <summary>Returns the current Y map coordinate of the mobile with the given serial.</summary>
        public virtual int GetY(uint serial)
        {
            _cancel.ThrowIfCancelled();
            return _world.FindMobile(serial)?.Y ?? 0;
        }

        /// <summary>Returns the current Z altitude of the mobile with the given serial.</summary>
        public virtual int GetZ(uint serial)
        {
            _cancel.ThrowIfCancelled();
            return _world.FindMobile(serial)?.Z ?? 0;
        }

        // ------------------------------------------------------------------
        // Classificazione per body
        // ------------------------------------------------------------------

        /// <summary>Returns true if the mobile has a human or humanoid body (Human, Elf, or Gargoyle race graphics).</summary>
        public virtual bool IsHuman(uint serial)
        {
            _cancel.ThrowIfCancelled();
            var m = _world.FindMobile(serial);
            if (m == null) return false;
            return m.Graphic is 0x190 or 0x191     // Human
                               or 0x025D or 0x025E  // Elf
                               or 0x029A or 0x029B; // Gargoyle
        }

        /// <summary>Returns true if the mobile is a monster (non-humanoid body graphic outside the human and NPC range).</summary>
        public virtual bool IsMonster(uint serial)
        {
            _cancel.ThrowIfCancelled();
            var m = _world.FindMobile(serial);
            if (m == null) return false;
            // Heuristic: bodies >= 400 tipicamente mostri; human range sotto 400
            return !IsHuman(serial) && m.Graphic >= 0x0190 + 0x200;
        }

        /// <summary>Returns true if the mobile has a humanoid NPC body graphic (body IDs 1–0x18F, non-player).</summary>
        public virtual bool IsNPC(uint serial)
        {
            _cancel.ThrowIfCancelled();
            var m = _world.FindMobile(serial);
            if (m == null) return false;
            return !IsHuman(serial) && m.Graphic < 0x190;
        }

        // ------------------------------------------------------------------
        // Filter multipli
        // ------------------------------------------------------------------

        /// <summary>Returns all mobiles matching all criteria defined in the given <see cref="MobilesFilter"/> object.</summary>
        public virtual List<ScriptMobile> ApplyFilter(MobilesFilter filter)
        {
            _cancel.ThrowIfCancelled();
            if (filter == null || !filter.Enabled) return new List<ScriptMobile>();

            var player = _world.Player;
            return Wrap(_world.Mobiles.Where(m =>
            {
                if (filter.Bodies.Count > 0 && !filter.Bodies.Contains(m.Graphic)) return false;
                if (filter.Hues.Count > 0 && !filter.Hues.Contains(m.Hue)) return false;
                if (filter.Serials.Count > 0 && !filter.Serials.Contains((int)m.Serial)) return false;
                if (filter.RangeMin != -1 && player != null && m.DistanceTo(player) < filter.RangeMin) return false;
                if (filter.RangeMax != -1 && player != null && m.DistanceTo(player) > filter.RangeMax) return false;
                
                if (filter.Notorieties.Count > 0 && !filter.Notorieties.Contains(m.Notoriety)) return false;
                
                if (filter.OnlyAlive && m.Hits == 0) return false;
                if (filter.OnlyEnemy && m.Notoriety is 1 or 2) return false;

                bool isHuman = m.Graphic == 0x0190 || m.Graphic == 0x0191 || m.Graphic == 0x025D || m.Graphic == 0x025E;
                if (filter.IsHuman == 1 && !isHuman) return false;
                if (filter.IsHuman == -1 && isHuman) return false;

                if (filter.IsGhost == 1 && !m.IsGhost) return false;
                if (filter.IsGhost == -1 && m.IsGhost) return false;

                bool isAlly = m.Notoriety == 1 || m.Notoriety == 2;
                if (filter.IsAlly == 1 && !isAlly) return false;
                if (filter.IsAlly == -1 && isAlly) return false;

                bool isEnemy = m.Notoriety is 3 or 4 or 5 or 6;
                if (filter.IsEnemy == 1 && !isEnemy) return false;
                if (filter.IsEnemy == -1 && isEnemy) return false;

                if (filter.IsNeutral == 1 && m.Notoriety != 3) return false;
                if (filter.IsNeutral == -1 && m.Notoriety == 3) return false;

                if (!string.IsNullOrEmpty(filter.Name) && (m.Name == null || !m.Name.Contains(filter.Name, StringComparison.OrdinalIgnoreCase))) return false;
                
                return true;
            }));
        }

        /// <summary>Returns all mobiles matching the given graphic, notoriety, range, and alive/enemy flags. Parameters set to -1 or false are ignored.</summary>
        public virtual List<ScriptMobile> ApplyFilter(
            int graphic = -1,
            int notoriety = -1,
            int rangeMax = -1,
            bool onlyAlive = false,
            bool onlyEnemy = false)
        {
            _cancel.ThrowIfCancelled();
            var player = _world.Player;

            return Wrap(_world.Mobiles.Where(m =>
            {
                if (graphic != -1 && m.Graphic != (ushort)graphic) return false;
                if (notoriety != -1 && m.Notoriety != (byte)notoriety) return false;
                if (rangeMax != -1 && player != null && m.DistanceTo(player) > rangeMax) return false;
                if (onlyAlive && m.Hits == 0) return false;
                if (onlyEnemy && m.Notoriety is 1 or 2) return false; // 1=blue 2=green skip
                return true;
            }));
        }

        // ------------------------------------------------------------------
        // Migration Compatibility Methods
        // ------------------------------------------------------------------

        /// <summary>Sends a single-click packet for the mobile with the given serial, requesting its name.</summary>
        public virtual void SingleClick(uint serial)
        {
            _cancel.ThrowIfCancelled();
            _packet.SendToServer(PacketBuilder.SingleClick(serial));
        }

        /// <summary>Sends a single-click packet for the given mobile object.</summary>
        public virtual void SingleClick(Mobile mob)
        {
            if (mob != null) SingleClick(mob.Serial);
        }

        /// <summary>Double-clicks (uses) the mobile with the given serial.</summary>
        public virtual void UseMobile(uint serial)
        {
            _cancel.ThrowIfCancelled();
            _packet.SendToServer(PacketBuilder.DoubleClick(serial));
        }

        /// <summary>Double-clicks (uses) the given mobile object.</summary>
        public virtual void UseMobile(Mobile mob)
        {
            if (mob != null) UseMobile(mob.Serial);
        }

        /// <summary>Sends an overhead speech message from the mobile with the given serial to the client (packet 0xAE).</summary>
        public virtual void Message(uint serial, int hue, string message, bool wait = true)
        {
            _cancel.ThrowIfCancelled();
            var m = _world.FindMobile(serial);
            if (m == null) return;
            
            byte[] msgBytes = Encoding.BigEndianUnicode.GetBytes(message + "\0");
            int size = 48 + msgBytes.Length;
            byte[] packet = new byte[size];
            packet[0] = 0xAE;
            packet[1] = (byte)(size >> 8); packet[2] = (byte)size;
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(packet.AsSpan(3), serial);
            packet[7] = (byte)(m.Graphic >> 8); packet[8] = (byte)(m.Graphic & 0xff);
            packet[9] = 0x00;
            packet[10] = (byte)(hue >> 8); packet[11] = (byte)hue;
            packet[12] = 0x00; packet[13] = 0x03;
            packet[14] = (byte)'e'; packet[15] = (byte)'n'; packet[16] = (byte)'u'; packet[17] = 0;
            string sysName = (m.Name ?? "").PadRight(30, '\0');
            for (int i = 0; i < 30; i++) packet[18 + i] = (byte)sysName[i];
            Array.Copy(msgBytes, 0, packet, 48, msgBytes.Length);
            
            _packet.SendToClient(packet);
        }

        /// <summary>Sends an overhead speech message from the given mobile object to the client.</summary>
        public virtual void Message(Mobile mob, int hue, string message, bool wait = true)
        {
            if (mob != null) Message(mob.Serial, hue, message, wait);
        }

        /// <summary>Requests the context menu for the mobile and returns the response code of the entry matching <paramref name="name"/>, or -1 if not found.</summary>
        public virtual int ContextExist(uint serial, string name, bool showContext = false)
        {
            _cancel.ThrowIfCancelled();
            var m = _world.FindMobile(serial);
            if (m == null) return -1;

            ContextMenuStore.Clear();
            byte[] pkt = new byte[9];
            pkt[0] = 0xBF;
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(1), 9);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(3), 0x0E);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(5), serial);
            _packet.SendToServer(pkt);

            var entries = ContextMenuStore.WaitForSerial(serial, 10000);
            var match = entries.FirstOrDefault(e => e.Entry.Equals(name, StringComparison.OrdinalIgnoreCase));
            return match != null ? match.Response : -1;
        }

        /// <summary>Requests the context menu for the given mobile object and returns the response code of the named entry, or -1 if not found.</summary>
        public virtual int ContextExist(Mobile mob, string name, bool showContext = false)
        {
            if (mob == null) return -1;
            return ContextExist(mob.Serial, name, showContext);
        }

        /// <summary>Returns the tile distance from the player to the mobile with the given serial. Alias for <see cref="GetDistance"/>.</summary>
        public virtual int DistanceTo(uint serial)
        {
            _cancel.ThrowIfCancelled();
            return GetDistance(serial);
        }

        /// <summary>Finds a mobile matching the given body, notoriety list, max range, and selection strategy (nearest, farthest, weakest, strongest, random).</summary>
        public virtual ScriptMobile? FindMobile(int graphic, List<byte> notoriety, int rangemax, string selector, bool highlight)
        {
            _cancel.ThrowIfCancelled();
            var player = _world.Player;
            if (player == null) return null;
            
            var list = _world.Mobiles.Where(m =>
                m.Serial != player.Serial &&
                (graphic <= 0 || m.Graphic == graphic) &&
                (notoriety == null || notoriety.Count == 0 || notoriety.Contains(m.Notoriety)) &&
                (rangemax <= 0 || m.DistanceTo(player) <= rangemax)
            ).ToList();

            var result = SelectInternal(list, selector);
            if (result != null && highlight)
            {
                _targeting.SetLastTarget(result.Serial);
            }
            return Wrap(result);
        }

        /// <summary>Finds a mobile matching any of the given body graphics, notoriety list, max range, and selection strategy.</summary>
        public virtual ScriptMobile? FindMobile(List<int> graphics, List<byte> notoriety, int rangemax, string selector, bool highlight)
        {
            _cancel.ThrowIfCancelled();
            var player = _world.Player;
            if (player == null) return null;
            
            var list = _world.Mobiles.Where(m =>
                m.Serial != player.Serial &&
                (graphics == null || graphics.Count == 0 || graphics.Contains(m.Graphic)) &&
                (notoriety == null || notoriety.Count == 0 || notoriety.Contains(m.Notoriety)) &&
                (rangemax <= 0 || m.DistanceTo(player) <= rangemax)
            ).ToList();

            var result = SelectInternal(list, selector);
            if (result != null && highlight)
            {
                _targeting.SetLastTarget(result.Serial);
            }
            return Wrap(result);
        }

        /// <summary>Selects a single mobile from the enumerable using the specified strategy (nearest, farthest, weakest, strongest, random).</summary>
        public virtual ScriptMobile? Select(IEnumerable<Mobile> mobiles, string selector)
        {
            return Wrap(SelectInternal(mobiles, selector));
        }

        private Mobile? SelectInternal(IEnumerable<Mobile> mobiles, string selector)
        {
            _cancel.ThrowIfCancelled();
            var list = mobiles.ToList();
            if (!list.Any()) return null;

            var player = _world.Player;
            if (player == null) return list.First();

            return selector?.ToLowerInvariant() switch
            {
                "nearest" => list.OrderBy(m => m.DistanceTo(player)).FirstOrDefault(),
                "farthest" => list.OrderByDescending(m => m.DistanceTo(player)).FirstOrDefault(),
                "weakest" => list.OrderBy(m => m.Hits).FirstOrDefault(),
                "strongest" => list.OrderByDescending(m => m.Hits).FirstOrDefault(),
                "random" => list.OrderBy(m => Guid.NewGuid()).FirstOrDefault(),
                _ => list.First()
            };
        }

        /// <summary>Returns the item equipped by the specified mobile on the named layer (e.g. "LeftHand", "Helm"), or null if not found.</summary>
        public virtual ScriptItem? GetItemOnLayer(uint serial, string layerName)
        {
            _cancel.ThrowIfCancelled();
            if (!Enum.TryParse<Layer>(layerName, true, out var layer)) return null;
            return WrapItem(_world.Items.FirstOrDefault(i => i.Container == serial && i.Layer == (byte)layer));
        }

        /// <summary>Returns all OPL property argument strings for the mobile with the given serial.</summary>
        public virtual List<string> GetPropStringList(uint serial)
        {
            _cancel.ThrowIfCancelled();
            var m = _world.FindMobile(serial);
            if (m?.OPL == null) return new List<string>();
            return m.OPL.Properties.Select(p => p.Arguments).ToList();
        }

        /// <summary>Returns all OPL property argument strings for the given mobile object.</summary>
        public virtual List<string> GetPropStringList(Mobile mob)
        {
            if (mob == null) return new List<string>();
            return GetPropStringList(mob.Serial);
        }

        /// <summary>Returns the OPL property argument string at the given zero-based index for the mobile with the given serial.</summary>
        public virtual string GetPropStringByIndex(uint serial, int index)
        {
            _cancel.ThrowIfCancelled();
            var m = _world.FindMobile(serial);
            if (m?.OPL == null) return string.Empty;
            var props = m.OPL.Properties;
            return index >= 0 && index < props.Count ? props[index].Arguments : string.Empty;
        }

        /// <summary>Returns the OPL property argument string at the given zero-based index for the given mobile object.</summary>
        public virtual string GetPropStringByIndex(Mobile mob, int index)
        {
            if (mob == null) return string.Empty;
            return GetPropStringByIndex(mob.Serial, index);
        }

        /// <summary>Returns the numeric value of the named property from the mobile's OPL, parsing the first integer or float found.</summary>
        public virtual float GetPropValue(uint serial, string name)
        {
            _cancel.ThrowIfCancelled();
            var m = _world.FindMobile(serial);
            if (m?.OPL == null || string.IsNullOrEmpty(name)) return 0;
            foreach (var prop in m.OPL.Properties)
            {
                string text = prop.Arguments;
                if (!text.Contains(name, StringComparison.OrdinalIgnoreCase)) continue;
                var match = System.Text.RegularExpressions.Regex.Match(text, @"[-+]?\d+");
                if (match.Success && float.TryParse(match.Value, out float val)) return val;
                return 1;
            }
            return 0;
        }

        /// <summary>Returns the numeric value of the named property from the given mobile object's OPL.</summary>
        public virtual float GetPropValue(Mobile mob, string name)
        {
            if (mob == null) return 0;
            return GetPropValue(mob.Serial, name);
        }

        /// <summary>Requests the OPL properties for the mobile with the given serial if not yet received. Returns true if the mobile exists.</summary>
        public virtual bool WaitForProps(uint serial, int delay)
        {
            _cancel.ThrowIfCancelled();
            var m = _world.FindMobile(serial);
            if (m == null) return false;
            if (m.OPL != null) return true;
            // Query property 0xD6
            byte[] pkt = new byte[7];
            pkt[0] = 0xD6;
            pkt[1] = 0x00; pkt[2] = 0x07;
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(3), serial);
            _packet.SendToServer(pkt);
            return true;
        }

        /// <summary>Requests OPL properties for the given mobile object if not yet received. Returns true if the mobile is valid.</summary>
        public virtual bool WaitForProps(Mobile m, int delay)
        {
            if (m == null) return false;
            return WaitForProps(m.Serial, delay);
        }

        /// <summary>Requests the status bar data for the mobile with the given serial if not yet received. Returns true if the mobile exists.</summary>
        public virtual bool WaitForStats(uint serial, int delay)
        {
            _cancel.ThrowIfCancelled();
            var m = _world.FindMobile(serial);
            if (m == null) return false;
            if (m.HitsMax > 0) return true;
            // Status Query 0x34
            byte[] pkt = { 0x34, 0xED, 0xED, 0xED, 0xED, 0x04 };
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(1), serial);
            _packet.SendToServer(pkt);
            return true;
        }

        /// <summary>Requests the status bar data for the given mobile object if not yet received. Returns true if the mobile is valid.</summary>
        public virtual bool WaitForStats(Mobile m, int delay)
        {
            if (m == null) return false;
            return WaitForStats(m.Serial, delay);
        }

        /// <summary>Sends a status request packet (0xB8) to retrieve karma/fame info for the mobile with the given serial.</summary>
        public virtual bool UpdateKarma(uint serial)
        {
            _cancel.ThrowIfCancelled();
            byte[] pkt = new byte[7];
            pkt[0] = 0xB8;
            pkt[1] = 0x00; pkt[2] = 0x07;
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(3), serial);
            _packet.SendToServer(pkt);
            return true;
        }

        /// <summary>Sends a status request packet for the given mobile object to retrieve karma/fame info.</summary>
        public virtual bool UpdateKarma(Mobile mob)
        {
            if (mob == null) return false;
            return UpdateKarma(mob.Serial);
        }

        /// <summary>Holds the last known tracking information for a mobile (serial, position, timestamp).</summary>
        public class TrackingInfo
        {
            /// <summary>X coordinate of the tracked mobile.</summary>
            public ushort x { get; set; }
            /// <summary>Y coordinate of the tracked mobile.</summary>
            public ushort y { get; set; }
            /// <summary>Serial of the tracked mobile.</summary>
            public uint serial { get; set; }
            /// <summary>Timestamp of the last tracking update.</summary>
            public DateTime lastUpdate { get; set; }
        }

        /// <summary>Returns the last known tracking information received from the server. Returns a default empty record if none available.</summary>
        /// <remarks>⚠️ Stub: returns placeholder value.</remarks>
        public virtual TrackingInfo GetTrackingInfo()
        {
            _cancel.ThrowIfCancelled();
            return new TrackingInfo();
        }

        /// <summary>Creates and returns a new empty <see cref="MobilesFilter"/> instance for building mobile search criteria.</summary>
        public virtual MobilesFilter Filter() => new MobilesFilter();

        /// <summary>Defines filter criteria used with <see cref="ApplyFilter(MobilesFilter)"/> to query mobiles by multiple properties.</summary>
        public class MobilesFilter
        {
            /// <summary>When false, the filter is skipped and ApplyFilter returns an empty list.</summary>
            public bool Enabled { get; set; } = true;
            /// <summary>List of specific serials to match. Empty means no restriction.</summary>
            public List<int> Serials { get; set; } = new();
            /// <summary>Partial name to match (case-insensitive). Empty string disables name filtering.</summary>
            public string Name { get; set; } = string.Empty;
            /// <summary>List of body graphic IDs to match. Empty means no restriction.</summary>
            public List<int> Bodies { get; set; } = new();
            /// <summary>Alias for <see cref="Bodies"/>.</summary>
            public List<int> Graphics { get => Bodies; set => Bodies = value; }
            /// <summary>List of hue values to match. Empty means no restriction.</summary>
            public List<int> Hues { get; set; } = new();
            /// <summary>Minimum tile distance from the player. -1 disables.</summary>
            public int RangeMin { get; set; } = -1;
            /// <summary>Maximum tile distance from the player. -1 disables.</summary>
            public int RangeMax { get; set; } = -1;
            /// <summary>List of notoriety values to match. Empty means no restriction.</summary>
            public List<byte> Notorieties { get; set; } = new();
            /// <summary>1 = only human-bodied mobiles, -1 = only non-human, 0 = no restriction.</summary>
            public int IsHuman { get; set; } = 0;
            /// <summary>1 = only ghost bodies, -1 = only non-ghost, 0 = no restriction.</summary>
            public int IsGhost { get; set; } = 0;
            /// <summary>1 = only allies (notoriety 1–2), -1 = exclude allies, 0 = no restriction.</summary>
            public int IsAlly { get; set; } = 0;
            /// <summary>1 = only enemies (notoriety 3–6), -1 = exclude enemies, 0 = no restriction.</summary>
            public int IsEnemy { get; set; } = 0;
            /// <summary>1 = only neutral (notoriety 3), -1 = exclude neutral, 0 = no restriction.</summary>
            public int IsNeutral { get; set; } = 0;
            /// <summary>When true, excludes dead mobiles (HP = 0).</summary>
            public bool OnlyAlive { get; set; } = false;
            /// <summary>When true, excludes non-hostile mobiles (notoriety 1–2).</summary>
            public bool OnlyEnemy { get; set; } = false;
        }

        /// <summary>Returns a pre-populated <see cref="MobilesFilter"/> for the named targeting preset. Returns an empty filter if unrecognized.</summary>
        /// <remarks>⚠️ Stub: returns placeholder value.</remarks>
        public virtual MobilesFilter GetTargetingFilter(string target_name)
        {
            _cancel.ThrowIfCancelled();
            return new MobilesFilter();
        }

        #region int-serial overloads — RazorEnhanced compatibility (TASK-FR-012)
        public virtual ScriptMobile? FindBySerial(int serial) => FindBySerial((uint)serial);
        public virtual int GetDistance(int serial) => GetDistance((uint)serial);
        public virtual bool IsAlive(int serial) => IsAlive((uint)serial);
        public virtual bool IsDead(int serial) => IsDead((uint)serial);
        public virtual bool IsFriend(int serial) => IsFriend((uint)serial);
        public virtual int GetHealthPercent(int serial) => GetHealthPercent((uint)serial);
        public virtual string GetName(int serial) => GetName((uint)serial);
        public virtual int GetGraphic(int serial) => GetGraphic((uint)serial);
        public virtual int GetHue(int serial) => GetHue((uint)serial);
        public virtual int GetNotoriety(int serial) => GetNotoriety((uint)serial);
        public virtual bool IsWarMode(int serial) => IsWarMode((uint)serial);
        public virtual bool IsPoisoned(int serial) => IsPoisoned((uint)serial);
        public virtual bool IsHidden(int serial) => IsHidden((uint)serial);
        public virtual bool IsParty(int serial) => IsParty((uint)serial);
        public virtual int GetMana(int serial) => GetMana((uint)serial);
        public virtual int GetManaPercent(int serial) => GetManaPercent((uint)serial);
        public virtual int GetStam(int serial) => GetStam((uint)serial);
        public virtual int GetStamPercent(int serial) => GetStamPercent((uint)serial);
        public virtual int GetX(int serial) => GetX((uint)serial);
        public virtual int GetY(int serial) => GetY((uint)serial);
        public virtual int GetZ(int serial) => GetZ((uint)serial);
        public virtual bool IsHuman(int serial) => IsHuman((uint)serial);
        public virtual bool IsMonster(int serial) => IsMonster((uint)serial);
        public virtual bool IsNPC(int serial) => IsNPC((uint)serial);
        public virtual void SingleClick(int serial) => SingleClick((uint)serial);
        public virtual void UseMobile(int serial) => UseMobile((uint)serial);
        public virtual void Message(int serial, int hue, string message, bool wait = true) => Message((uint)serial, hue, message, wait);
        public virtual int ContextExist(int serial, string name, bool showContext = false) => ContextExist((uint)serial, name, showContext);
        public virtual int DistanceTo(int serial) => DistanceTo((uint)serial);
        public virtual ScriptItem? GetItemOnLayer(int serial, string layerName) => GetItemOnLayer((uint)serial, layerName);
        public virtual List<string> GetPropStringList(int serial) => GetPropStringList((uint)serial);
        public virtual string GetPropStringByIndex(int serial, int index) => GetPropStringByIndex((uint)serial, index);
        public virtual float GetPropValue(int serial, string name) => GetPropValue((uint)serial, name);
        public virtual bool WaitForProps(int serial, int delay) => WaitForProps((uint)serial, delay);
        public virtual bool WaitForStats(int serial, int delay) => WaitForStats((uint)serial, delay);
        public virtual bool UpdateKarma(int serial) => UpdateKarma((uint)serial);
        #endregion
    }
}