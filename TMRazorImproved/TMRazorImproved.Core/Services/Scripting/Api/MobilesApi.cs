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
    }
}
