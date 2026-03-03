using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.Core.Services
{
    public class JournalService : IJournalService
    {
        private const int MaxEntries = 500;
        private readonly ConcurrentQueue<JournalEntry> _entries = new();
        
        public event Action<JournalEntry>? OnNewEntry;

        public IEnumerable<JournalEntry> Entries => _entries.ToList();

        public void AddEntry(JournalEntry entry)
        {
            _entries.Enqueue(entry);
            
            // Manteniamo la coda entro il limite
            while (_entries.Count > MaxEntries)
            {
                _entries.TryDequeue(out _);
            }

            OnNewEntry?.Invoke(entry);
        }

        public void Clear()
        {
            while (_entries.TryDequeue(out _)) { }
        }

        public bool Contains(string text)
        {
            return _entries.Any(e => e.Text.Contains(text, StringComparison.OrdinalIgnoreCase));
        }

        public JournalEntry? GetLast()
        {
            return _entries.LastOrDefault();
        }
    }
}
