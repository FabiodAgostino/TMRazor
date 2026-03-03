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
    }
}
