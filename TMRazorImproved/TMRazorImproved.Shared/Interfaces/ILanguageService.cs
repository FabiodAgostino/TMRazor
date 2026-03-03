using System.Collections.Generic;
using TMRazorImproved.Shared.Enums;

namespace TMRazorImproved.Shared.Interfaces
{
    public interface ILanguageService
    {
        string CurrentLanguage { get; }
        
        /// <summary>
        /// Recupera una stringa dell'interfaccia Razor per nome/chiave.
        /// </summary>
        string GetString(LocString key);
        string GetString(int key);
        string GetString(string key);
        
        /// <summary>
        /// Recupera una stringa dal Cliloc di Ultima Online.
        /// </summary>
        string GetCliloc(int clilocId);
        string ClilocFormat(int clilocId, string args);

        void Load(string langCode);
        IEnumerable<string> GetAvailableLanguages();
    }
}
