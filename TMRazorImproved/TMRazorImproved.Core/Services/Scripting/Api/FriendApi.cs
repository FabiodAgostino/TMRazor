using System.Collections.Generic;
using System.Linq;
using TMRazorImproved.Shared.Interfaces;

namespace TMRazorImproved.Core.Services.Scripting.Api
{
    // FIX BUG-P2-02: FriendApi ora delega a IFriendsService reale
    public class FriendApi
    {
        private readonly IFriendsService _friends;
        private readonly ScriptCancellationController _cancel;

        public FriendApi(IFriendsService friends, ScriptCancellationController cancel)
        {
            _friends = friends;
            _cancel = cancel;
        }

        public virtual bool IsFriend(uint serial)
        {
            _cancel.ThrowIfCancelled();
            return _friends.IsFriend(serial);
        }

        public virtual void Add(uint serial)
        {
            _cancel.ThrowIfCancelled();
            _friends.AddFriend(serial, string.Empty);
        }

        public virtual void Remove(uint serial)
        {
            _cancel.ThrowIfCancelled();
            _friends.RemoveFriend(serial);
        }

        public virtual List<uint> GetFriendList()
        {
            _cancel.ThrowIfCancelled();
            return _friends.ActiveList?.Players?
                .Where(p => p.Enabled)
                .Select(p => p.Serial)
                .ToList() ?? new List<uint>();
        }

        /// <summary>Numero di amici nella lista attiva.</summary>
        public virtual int GetCount()
        {
            _cancel.ThrowIfCancelled();
            return _friends.ActiveList?.Players?.Count(p => p.Enabled) ?? 0;
        }

        /// <summary>Rimuove tutti gli amici dalla lista attiva.</summary>
        public virtual void Clear()
        {
            _cancel.ThrowIfCancelled();
            var players = _friends.ActiveList?.Players;
            if (players == null) return;
            foreach (var p in players.ToList())
                _friends.RemoveFriend(p.Serial);
        }

        /// <summary>Rinomina l'amico con il serial specificato.</summary>
        public virtual void Rename(uint serial, string newName)
        {
            _cancel.ThrowIfCancelled();
            _friends.AddFriend(serial, newName); // AddFriend aggiorna se già esiste
        }
    }
}
