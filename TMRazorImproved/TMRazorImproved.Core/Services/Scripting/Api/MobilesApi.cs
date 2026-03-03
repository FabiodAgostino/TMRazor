using System.Collections.Generic;
using System.Linq;
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
        private readonly ScriptCancellationController _cancel;

        public MobilesApi(IWorldService world, ScriptCancellationController cancel)
        {
            _world = world;
            _cancel = cancel;
        }

        /// <summary>Cerca un mobile per serial. Ritorna None se non trovato.</summary>
        public virtual Mobile? FindBySerial(uint serial)
        {
            _cancel.ThrowIfCancelled();
            return _world.FindMobile(serial);
        }

        public virtual Mobile? FindByID(int graphic)
        {
            _cancel.ThrowIfCancelled();
            return _world.Mobiles.FirstOrDefault(m => m.Graphic == graphic);
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

        /// <summary>Controlla se un mobile esiste e non è morto.</summary>
        public virtual bool IsAlive(uint serial)
        {
            _cancel.ThrowIfCancelled();
            var m = _world.FindMobile(serial);
            return m != null && m.Hits > 0;
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
