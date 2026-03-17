using System.Collections.Generic;
using TMRazorImproved.Shared.Models.Config;

namespace TMRazorImproved.Shared.Interfaces
{
    public interface IConfigService
    {
        GlobalSettings Global { get; }
        UserProfile CurrentProfile { get; }
        string CurrentShardId { get; }
        
        IEnumerable<string> GetAvailableProfiles(string? shardId = null);
        
        void Load();
        void Save();
        
        void SetCurrentShard(string shardId);
        void SwitchProfile(string profileName);
        void CreateProfile(string profileName);
        void CloneProfile(string sourceProfileName, string newProfileName);
        void DeleteProfile(string profileName);
        void RenameProfile(string oldProfileName, string newProfileName);
    }
}
