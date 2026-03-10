using System;
using System.Collections.Generic;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.Shared.Interfaces
{
    public interface IJournalService
    {
        IEnumerable<JournalEntry> Entries { get; }
        void AddEntry(JournalEntry entry);
        void Clear();
        bool Contains(string text);
        JournalEntry? GetLast();
        
        void AddFilter(string text);
        void RemoveFilter(string text);
        IEnumerable<string> GetFilters();

        event Action<JournalEntry> OnNewEntry;
    }
}
