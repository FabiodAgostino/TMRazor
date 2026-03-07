using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
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
        private readonly ScriptCancellationController _cancel;
        private readonly ILogger<MobilesApi>? _logger;

        public MobilesApi(IWorldService world, IFriendsService friends, ScriptCancellationController cancel, ILogger<MobilesApi>? logger = null)
        {
            _world = world;
            _friends = friends;
            _cancel = cancel;
            _logger = logger;
        }

        /// <summary>Cerca un mobile per serial. Ritorna None se non trovato.</summary>
        public virtual Mobile? FindBySerial(uint serial)
        {
            _cancel.ThrowIfCancelled();
            var result = _world.FindMobile(serial);
            if (result == null) _logger?.LogDebug("FindBySerial: mobile 0x{Serial:X} not found", serial);
            return result;
        }

        public virtual Mobile? FindByID(int graphic)
        {
            _cancel.ThrowIfCancelled();
            return _world.Mobiles.FirstOrDefault(m => m.Graphic == graphic);
        }

        /// <summary>Ritorna tutti i mobile con il graphic specificato entro il range dal giocatore.</summary>
        public virtual IEnumerable<Mobile> FindAllByID(int graphic, int range = -1)
        {
            _cancel.ThrowIfCancelled();
            var player = _world.Player;
            return _world.Mobiles
                .Where(m => m.Graphic == (ushort)graphic)
                .Where(m => range == -1 || (player != null && m.DistanceTo(player) <= range))
                .ToList();
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
        public virtual IEnumerable<Mobile> FindInRange(int range)
        {
            _cancel.ThrowIfCancelled();
            var player = _world.Player;
            if (player == null) return Enumerable.Empty<Mobile>();

            return _world.Mobiles
                .Where(m => m.Serial != player.Serial && m.DistanceTo(player) <= range)
                .ToList();
        }

        /// <summary>Ritorna il mobile più vicino al giocatore (escluso il giocatore stesso).</summary>
        public virtual Mobile? FindNearest()
        {
            _cancel.ThrowIfCancelled();
            var player = _world.Player;
            if (player == null) return null;

            return _world.Mobiles
                .Where(m => m.Serial != player.Serial)
                .OrderBy(m => m.DistanceTo(player))
                .FirstOrDefault();
        }

        /// <summary>Ritorna il nemico più vicino (Notoriety: 3, 4, 5, 6).</summary>
        public virtual Mobile? FindNearestEnemy()
        {
            _cancel.ThrowIfCancelled();
            var player = _world.Player;
            if (player == null) return null;

            return _world.Mobiles
                .Where(m => m.Serial != player.Serial && (m.Notoriety >= 3 && m.Notoriety <= 6))
                .OrderBy(m => m.DistanceTo(player))
                .FirstOrDefault();
        }

        /// <summary>Ritorna il mobile amico più vicino (in lista amici o Notoriety: 1, 2).</summary>
        public virtual Mobile? FindNearestFriend()
        {
            _cancel.ThrowIfCancelled();
            var player = _world.Player;
            if (player == null) return null;

            return _world.Mobiles
                .Where(m => m.Serial != player.Serial && (_friends.IsFriend(m.Serial) || m.Notoriety == 1 || m.Notoriety == 2))
                .OrderBy(m => m.DistanceTo(player))
                .FirstOrDefault();
        }

        /// <summary>Filtra i mobile per graphic ID (body).</summary>
        public virtual IEnumerable<Mobile> FilterByBody(int body)
        {
            _cancel.ThrowIfCancelled();
            return _world.Mobiles.Where(m => m.Graphic == (ushort)body).ToList();
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

        public virtual IEnumerable<Mobile> FilterByNotoriety(int notoriety)
        {
            _cancel.ThrowIfCancelled();
            return _world.Mobiles.Where(m => m.Notoriety == notoriety).ToList();
        }

        public virtual IEnumerable<Mobile> FilterByDistance(int minRange, int maxRange)
        {
            _cancel.ThrowIfCancelled();
            var player = _world.Player;
            if (player == null) return Enumerable.Empty<Mobile>();

            return _world.Mobiles.Where(m => {
                int dist = m.DistanceTo(player);
                return dist >= minRange && dist <= maxRange;
            }).ToList();
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
        /// Filtra i mobile nel mondo corrente per criteri multipli combinati.
        /// I parametri a -1/null/0 vengono ignorati (nessun filtro applicato).
        /// </summary>
        public virtual List<Mobile> ApplyFilter(
            int graphic = -1,
            int notoriety = -1,
            int rangeMax = -1,
            bool onlyAlive = false,
            bool onlyEnemy = false)
        {
            _cancel.ThrowIfCancelled();
            var player = _world.Player;

            return _world.Mobiles.Where(m =>
            {
                if (graphic != -1 && m.Graphic != (ushort)graphic) return false;
                if (notoriety != -1 && m.Notoriety != (byte)notoriety) return false;
                if (rangeMax != -1 && player != null && m.DistanceTo(player) > rangeMax) return false;
                if (onlyAlive && m.Hits == 0) return false;
                if (onlyEnemy && m.Notoriety is 1 or 2) return false; // 1=blue 2=green skip
                return true;
            }).ToList();
        }
    }
}
