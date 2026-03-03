using System.Collections.Generic;
using TMRazorImproved.Shared.Models.Config;

namespace TMRazorImproved.Shared.Interfaces
{
    public interface IConfigService
    {
        GlobalSettings Global { get; }
        UserProfile CurrentProfile { get; }
        
        IEnumerable<string> GetAvailableProfiles();
        
        void Load();
        void Save();
        
        void SwitchProfile(string profileName);
        void CreateProfile(string profileName);
        void CloneProfile(string sourceProfileName, string newProfileName);
        void DeleteProfile(string profileName);
        void RenameProfile(string oldProfileName, string newProfileName);
    }
}
