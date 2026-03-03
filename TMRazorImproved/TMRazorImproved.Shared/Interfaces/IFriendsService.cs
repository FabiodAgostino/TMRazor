using System.Collections.Generic;
using TMRazorImproved.Shared.Models.Config;

namespace TMRazorImproved.Shared.Interfaces
{
    public interface IFriendsService
    {
        FriendsConfig ActiveList { get; }
        
        bool IsFriend(uint serial);
        void AddFriend(uint serial, string name);
        void RemoveFriend(uint serial);
        
        void AddGuild(string name);
        void RemoveGuild(string name);
        
        void CreateList(string name);
        void DeleteList(string name);
        void SwitchList(string name);
    }
}
