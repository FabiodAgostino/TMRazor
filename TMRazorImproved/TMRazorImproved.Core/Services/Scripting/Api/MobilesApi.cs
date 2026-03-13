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
    /// <summary>
    /// API esposta agli script Python come variabile <c>Mobiles</c>.
    /// Tutte le proprietà sono <c>public virtual</c> per compatibilità con il binder DLR di IronPython.
    /// </summary>
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

        /// <summary>Cerca un mobile per serial. Ritorna None se non trovato.</summary>
        public virtual ScriptMobile? FindBySerial(uint serial)
        {
            _cancel.ThrowIfCancelled();
            var result = _world.FindMobile(serial);
            if (result == null) _logger?.LogDebug("FindBySerial: mobile 0x{Serial:X} not found", serial);
            return Wrap(result);
        }

        public virtual ScriptMobile? FindByID(int graphic)
        {
            _cancel.ThrowIfCancelled();
            return Wrap(_world.Mobiles.FirstOrDefault(m => m.Graphic == graphic));
        }

        /// <summary>Ritorna tutti i mobile con il graphic specificato entro il range dal giocatore.</summary>
        public virtual List<ScriptMobile> FindAllByID(int graphic, int range = -1)
        {
            _cancel.ThrowIfCancelled();
            var player = _world.Player;
            return Wrap(_world.Mobiles
                .Where(m => m.Graphic == (ushort)graphic)
                .Where(m => range == -1 || (player != null && m.DistanceTo(player) <= range)));
        }

        public virtual int GetDistance(uint serial)
        {
            _cancel.ThrowIfCancelled();
            var m = _world.FindMobile(serial);
            var p = _world.Player;
            if (m == null || p == null) return 999;
            return m.DistanceTo(p);
        }

        /// <summary>
        /// Ritorna la lista di tutti i mobile visibili nel range specificato
        /// rispetto alla posizione del giocatore.
        /// </summary>
        public virtual List<ScriptMobile> FindInRange(int range)
        {
            _cancel.ThrowIfCancelled();
            var player = _world.Player;
            if (player == null) return new List<ScriptMobile>();

            return Wrap(_world.Mobiles
                .Where(m => m.Serial != player.Serial && m.DistanceTo(player) <= range));
        }

        /// <summary>Ritorna il mobile più vicino al giocatore (escluso il giocatore stesso).</summary>
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

        /// <summary>Ritorna il nemico più vicino (Notoriety: 3, 4, 5, 6).</summary>
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

        /// <summary>Ritorna il mobile amico più vicino (in lista amici o Notoriety: 1, 2).</summary>
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

        /// <summary>Filtra i mobile per graphic ID (body).</summary>
        public virtual List<ScriptMobile> FilterByBody(int body)
        {
            _cancel.ThrowIfCancelled();
            return Wrap(_world.Mobiles.Where(m => m.Graphic == (ushort)body));
        }


        /// <summary>Controlla se un mobile esiste e non è morto.</summary>
        public virtual bool IsAlive(uint serial)
        {
            _cancel.ThrowIfCancelled();
            var m = _world.FindMobile(serial);
            return m != null && m.Hits > 0;
        }

        /// <summary>Controlla se un mobile è morto (Hits <= 0 o non trovato).</summary>
        public virtual bool IsDead(uint serial)
        {
            _cancel.ThrowIfCancelled();
            var m = _world.FindMobile(serial);
            if (m == null) return true;
            return m.Hits <= 0;
        }

        /// <summary>Controlla se il mobile è nella lista amici.</summary>
        public virtual bool IsFriend(uint serial)
        {
            _cancel.ThrowIfCancelled();
            return _friends.IsFriend(serial);
        }

        /// <summary>Ritorna la percentuale di HP (0-100).</summary>
        public virtual int GetHealthPercent(uint serial)
        {
            _cancel.ThrowIfCancelled();
            var m = _world.FindMobile(serial);
            if (m == null || m.HitsMax <= 0) return 0;
            return (int)((m.Hits * 100.0) / m.HitsMax);
        }

        public virtual List<ScriptMobile> FilterByNotoriety(int notoriety)
        {
            _cancel.ThrowIfCancelled();
            return Wrap(_world.Mobiles.Where(m => m.Notoriety == notoriety));
        }

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

        /// <summary>Ritorna il nome del mobile, stringa vuota se non trovato.</summary>
        public virtual string GetName(uint serial)
        {
            _cancel.ThrowIfCancelled();
            return _world.FindMobile(serial)?.Name ?? string.Empty;
        }

        /// <summary>Ritorna il Graphic (body) del mobile, 0 se non trovato.</summary>
        public virtual int GetGraphic(uint serial)
        {
            _cancel.ThrowIfCancelled();
            return _world.FindMobile(serial)?.Graphic ?? 0;
        }

        /// <summary>Ritorna il Hue del mobile, 0 se non trovato.</summary>
        public virtual int GetHue(uint serial)
        {
            _cancel.ThrowIfCancelled();
            return _world.FindMobile(serial)?.Hue ?? 0;
        }

        /// <summary>Ritorna il Notoriety del mobile (1=blue, 3=gray, 4=criminal, 5=enemy, 6=red, 7=invited).</summary>
        public virtual int GetNotoriety(uint serial)
        {
            _cancel.ThrowIfCancelled();
            return _world.FindMobile(serial)?.Notoriety ?? 0;
        }

        /// <summary>True se il mobile è in war mode.</summary>
        public virtual bool IsWarMode(uint serial)
        {
            _cancel.ThrowIfCancelled();
            return _world.FindMobile(serial)?.WarMode ?? false;
        }

        /// <summary>True se il mobile è avvelenato.</summary>
        public virtual bool IsPoisoned(uint serial)
        {
            _cancel.ThrowIfCancelled();
            return _world.FindMobile(serial)?.IsPoisoned ?? false;
        }

        /// <summary>True se il mobile è nascosto.</summary>
        public virtual bool IsHidden(uint serial)
        {
            _cancel.ThrowIfCancelled();
            return _world.FindMobile(serial)?.IsHidden ?? false;
        }

        /// <summary>True se il mobile è nel party del player.</summary>
        public virtual bool IsParty(uint serial)
        {
            _cancel.ThrowIfCancelled();
            return _world.PartyMembers.Contains(serial);
        }

        /// <summary>Ritorna la mana del mobile, 0 se non noto.</summary>
        public virtual int GetMana(uint serial)
        {
            _cancel.ThrowIfCancelled();
            return _world.FindMobile(serial)?.Mana ?? 0;
        }

        /// <summary>Ritorna la mana del mobile come percentuale (0-100), 0 se non noto.</summary>
        public virtual int GetManaPercent(uint serial)
        {
            _cancel.ThrowIfCancelled();
            var m = _world.FindMobile(serial);
            if (m == null || m.ManaMax == 0) return 0;
            return (int)((double)m.Mana / m.ManaMax * 100);
        }

        /// <summary>Ritorna la stamina del mobile, 0 se non nota.</summary>
        public virtual int GetStam(uint serial)
        {
            _cancel.ThrowIfCancelled();
            return _world.FindMobile(serial)?.Stam ?? 0;
        }

        /// <summary>Ritorna la stamina del mobile come percentuale (0-100), 0 se non nota.</summary>
        public virtual int GetStamPercent(uint serial)
        {
            _cancel.ThrowIfCancelled();
            var m = _world.FindMobile(serial);
            if (m == null || m.StamMax == 0) return 0;
            return (int)((double)m.Stam / m.StamMax * 100);
        }

        /// <summary>Ritorna la X corrente del mobile.</summary>
        public virtual int GetX(uint serial)
        {
            _cancel.ThrowIfCancelled();
            return _world.FindMobile(serial)?.X ?? 0;
        }

        /// <summary>Ritorna la Y corrente del mobile.</summary>
        public virtual int GetY(uint serial)
        {
            _cancel.ThrowIfCancelled();
            return _world.FindMobile(serial)?.Y ?? 0;
        }

        /// <summary>Ritorna la Z corrente del mobile.</summary>
        public virtual int GetZ(uint serial)
        {
            _cancel.ThrowIfCancelled();
            return _world.FindMobile(serial)?.Z ?? 0;
        }

        // ------------------------------------------------------------------
        // Classificazione per body
        // ------------------------------------------------------------------

        /// <summary>
        /// True se il mobile ha un body umano (0x190=Male, 0x191=Female).
        /// Copre anche race variant bodies (Gargoyle 0x029A/0x029B, Elf 0x025D/0x025E).
        /// </summary>
        public virtual bool IsHuman(uint serial)
        {
            _cancel.ThrowIfCancelled();
            var m = _world.FindMobile(serial);
            if (m == null) return false;
            return m.Graphic is 0x190 or 0x191     // Human
                               or 0x025D or 0x025E  // Elf
                               or 0x029A or 0x029B; // Gargoyle
        }

        /// <summary>True se il mobile non è umano e non è un NPC (graphic > 0x3E9, conv).</summary>
        public virtual bool IsMonster(uint serial)
        {
            _cancel.ThrowIfCancelled();
            var m = _world.FindMobile(serial);
            if (m == null) return false;
            // Heuristic: bodies >= 400 tipicamente mostri; human range sotto 400
            return !IsHuman(serial) && m.Graphic >= 0x0190 + 0x200;
        }

        /// <summary>True se il body rientra nel range NPC umanoide (1-0x18F).</summary>
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

        /// <summary>
        /// Filtra i mobile nel mondo corrente utilizzando un oggetto Filter.
        /// </summary>
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

        /// <summary>
        /// Filtra i mobile nel mondo corrente per criteri multipli combinati.
        /// I parametri a -1/null/0 vengono ignorati (nessun filtro applicato).
        /// </summary>
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

        public virtual void SingleClick(uint serial)
        {
            _cancel.ThrowIfCancelled();
            _packet.SendToServer(PacketBuilder.SingleClick(serial));
        }
        
        public virtual void SingleClick(Mobile mob)
        {
            if (mob != null) SingleClick(mob.Serial);
        }

        public virtual void UseMobile(uint serial)
        {
            _cancel.ThrowIfCancelled();
            _packet.SendToServer(PacketBuilder.DoubleClick(serial));
        }

        public virtual void UseMobile(Mobile mob)
        {
            if (mob != null) UseMobile(mob.Serial);
        }

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

        public virtual void Message(Mobile mob, int hue, string message, bool wait = true)
        {
            if (mob != null) Message(mob.Serial, hue, message, wait);
        }
        
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

        public virtual int ContextExist(Mobile mob, string name, bool showContext = false)
        {
            if (mob == null) return -1;
            return ContextExist(mob.Serial, name, showContext);
        }

        public virtual int DistanceTo(uint serial)
        {
            _cancel.ThrowIfCancelled();
            return GetDistance(serial);
        }

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

        public virtual ScriptItem? GetItemOnLayer(uint serial, string layerName)
        {
            _cancel.ThrowIfCancelled();
            if (!Enum.TryParse<Layer>(layerName, true, out var layer)) return null;
            return WrapItem(_world.Items.FirstOrDefault(i => i.Container == serial && i.Layer == (byte)layer));
        }

        public virtual List<string> GetPropStringList(uint serial)
        {
            _cancel.ThrowIfCancelled();
            var m = _world.FindMobile(serial);
            if (m?.OPL == null) return new List<string>();
            return m.OPL.Properties.Select(p => p.Arguments).ToList();
        }
        
        public virtual List<string> GetPropStringList(Mobile mob)
        {
            if (mob == null) return new List<string>();
            return GetPropStringList(mob.Serial);
        }

        public virtual string GetPropStringByIndex(uint serial, int index)
        {
            _cancel.ThrowIfCancelled();
            var m = _world.FindMobile(serial);
            if (m?.OPL == null) return string.Empty;
            var props = m.OPL.Properties;
            return index >= 0 && index < props.Count ? props[index].Arguments : string.Empty;
        }

        public virtual string GetPropStringByIndex(Mobile mob, int index)
        {
            if (mob == null) return string.Empty;
            return GetPropStringByIndex(mob.Serial, index);
        }

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

        public virtual float GetPropValue(Mobile mob, string name)
        {
            if (mob == null) return 0;
            return GetPropValue(mob.Serial, name);
        }

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

        public virtual bool WaitForProps(Mobile m, int delay)
        {
            if (m == null) return false;
            return WaitForProps(m.Serial, delay);
        }

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

        public virtual bool WaitForStats(Mobile m, int delay)
        {
            if (m == null) return false;
            return WaitForStats(m.Serial, delay);
        }

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

        public virtual bool UpdateKarma(Mobile mob)
        {
            if (mob == null) return false;
            return UpdateKarma(mob.Serial);
        }

        public class TrackingInfo
        {
            public ushort x { get; set; }
            public ushort y { get; set; }
            public uint serial { get; set; }
            public DateTime lastUpdate { get; set; }
        }

        public virtual TrackingInfo GetTrackingInfo()
        {
            _cancel.ThrowIfCancelled();
            return new TrackingInfo();
        }

        public virtual MobilesFilter Filter() => new MobilesFilter();

        public class MobilesFilter
        {
            public bool Enabled { get; set; } = true;
            public List<int> Serials { get; set; } = new();
            public string Name { get; set; } = string.Empty;
            public List<int> Bodies { get; set; } = new();
            public List<int> Graphics { get => Bodies; set => Bodies = value; }
            public List<int> Hues { get; set; } = new();
            public int RangeMin { get; set; } = -1;
            public int RangeMax { get; set; } = -1;
            public List<byte> Notorieties { get; set; } = new();
            public int IsHuman { get; set; } = 0;
            public int IsGhost { get; set; } = 0;
            public int IsAlly { get; set; } = 0;
            public int IsEnemy { get; set; } = 0;
            public int IsNeutral { get; set; } = 0;
            public bool OnlyAlive { get; set; } = false;
            public bool OnlyEnemy { get; set; } = false;
        }

        public virtual MobilesFilter GetTargetingFilter(string target_name)
        {
            _cancel.ThrowIfCancelled();
            return new MobilesFilter(); 
        }
    }
}