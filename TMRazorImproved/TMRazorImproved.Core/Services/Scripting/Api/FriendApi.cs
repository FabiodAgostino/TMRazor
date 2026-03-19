using System.Collections.Generic;
using System.Linq;
using TMRazorImproved.Shared.Interfaces;

namespace TMRazorImproved.Core.Services.Scripting.Api
{
    // FIX BUG-P2-02: FriendApi ora delega a IFriendsService reale
    public class FriendApi
    {
        private readonly IFriendsService _friends;
        private readonly ITargetingService _targeting;
        private readonly IWorldService _world;
        private readonly ScriptCancellationController _cancel;

        public FriendApi(IFriendsService friends, ITargetingService targeting, IWorldService world, ScriptCancellationController cancel)
        {
            _friends = friends;
            _targeting = targeting;
            _world = world;
            _cancel = cancel;
        }

        // TMRazor Legacy Classes
        public class FriendPlayer
        {
            public string Name { get; set; } = string.Empty;
            public int Serial { get; set; }
        }

        public class FriendGuild
        {
            public string Name { get; set; } = string.Empty;
        }

        // TMRazor Legacy Methods
        public virtual void AddFriendTarget()
        {
            _cancel.ThrowIfCancelled();

            // Richiede un target al client
            _targeting.RequestTarget();

            // Attende il risultato (stile TargetApi)
            var task = _targeting.AcquireTargetAsync();
            while (!task.IsCompleted)
            {
                _cancel.ThrowIfCancelled();
                System.Threading.Thread.Sleep(50);
            }

            var info = task.GetAwaiter().GetResult();
            if (info.Serial != 0)
            {
                var mobile = _world.FindMobile(info.Serial);
                string name = mobile?.Name ?? $"Unknown_0x{info.Serial:X}";
                _friends.AddFriend(info.Serial, name);
            }
        }

        public virtual void AddPlayer(string friendlist, string name, int serial)
        {
            _cancel.ThrowIfCancelled();
            _friends.AddFriend((uint)serial, name);
        }

        public virtual void ChangeList(string friendlist)
        {
            _cancel.ThrowIfCancelled();
            _friends.SwitchList(friendlist);
        }

        public virtual List<int> GetList(string friendlist)
        {
            _cancel.ThrowIfCancelled();
            return _friends.ActiveList?.Players?
                .Select(p => (int)p.Serial)
                .ToList() ?? new List<int>();
        }

        public virtual bool RemoveFriend(string friendlist, int serial)
        {
            _cancel.ThrowIfCancelled();
            bool existed = _friends.IsFriend((uint)serial);
            _friends.RemoveFriend((uint)serial);
            return existed;
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