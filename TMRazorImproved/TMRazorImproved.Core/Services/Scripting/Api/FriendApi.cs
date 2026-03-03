using System.Collections.Generic;
using TMRazorImproved.Shared.Interfaces;

namespace TMRazorImproved.Core.Services.Scripting.Api
{
    public class FriendApi
    {
        private readonly ScriptCancellationController _cancel;

        public FriendApi(ScriptCancellationController cancel)
        {
            _cancel = cancel;
        }

        public virtual bool IsFriend(uint serial) => false;
        public virtual void Add(uint serial) { }
        public virtual void Remove(uint serial) { }
        public virtual List<uint> GetFriendList() => new List<uint>();
    }
}
