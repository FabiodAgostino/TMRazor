using TMRazorImproved.Core.Utilities;
using TMRazorImproved.Shared.Interfaces;

namespace TMRazorImproved.Core.Services.Scripting.Api
{
    public class PathFindingApi
    {
        private readonly IPathFindingService _pathfinding;
        private readonly IWorldService _world;
        private readonly IPacketService _packet;
        private readonly ScriptCancellationController _cancel;

        public PathFindingApi(IPathFindingService pathfinding, IWorldService world, IPacketService packet, ScriptCancellationController cancel)
        {
            _pathfinding = pathfinding;
            _world = world;
            _packet = packet;
            _cancel = cancel;
        }

        /// <summary>
        /// Calcola il percorso dalla posizione corrente del giocatore alla destinazione.
        /// Ritorna il numero di passi necessari, o -1 se nessun percorso è trovato.
        /// </summary>
        public virtual int GetPath(int destX, int destY)
        {
            _cancel.ThrowIfCancelled();
            var player = _world.Player;
            if (player == null) return -1;
            var path = _pathfinding.GetPath(player.X, player.Y, player.Z, destX, destY, player.MapId);
            return path?.Count ?? -1;
        }

        /// <summary>
        /// Calcola il percorso tra due punti espliciti.
        /// Ritorna il numero di passi, o -1 se nessun percorso è trovato.
        /// </summary>
        public virtual int GetPath(int startX, int startY, int startZ, int destX, int destY, int mapId = 0)
        {
            _cancel.ThrowIfCancelled();
            int map = mapId > 0 ? mapId : (_world.Player?.MapId ?? 0);
            var path = _pathfinding.GetPath(startX, startY, startZ, destX, destY, map);
            return path?.Count ?? -1;
        }

        /// <summary>
        /// Verifica se esiste un percorso percorribile verso la destinazione.
        /// </summary>
        public virtual bool CanReach(int destX, int destY)
        {
            _cancel.ThrowIfCancelled();
            return GetPath(destX, destY) >= 0;
        }

        /// <summary>
        /// Avvia il pathfinding del client verso le coordinate specificate
        /// inviando il pacchetto 0x38 PathFind.
        /// </summary>
        public virtual void MoveTo(int x, int y, int z)
        {
            _cancel.ThrowIfCancelled();
            _packet.SendToClient(PacketBuilder.PathFind(x, y, z));
        }
    }
}
