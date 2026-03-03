using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.UI.ViewModels
{
    public partial class JournalViewModel : ViewModelBase, IDisposable
    {
        private readonly IJournalService _journalService;
        private readonly object _lock = new();

        public ObservableCollection<JournalEntry> Entries { get; } = new();

        [ObservableProperty]
        private string _searchText = string.Empty;

        public JournalViewModel(IJournalService journalService)
        {
            _journalService = journalService;
            
            // Abilita la sincronizzazione sicura della collezione tra thread (Cruciale per .NET 10/WPF)
            BindingOperations.EnableCollectionSynchronization(Entries, _lock);

            // Carica le voci esistenti
            foreach (var entry in _journalService.Entries)
            {
                Entries.Add(entry);
            }

            // Sottoscrizione ai nuovi messaggi
            _journalService.OnNewEntry += OnNewJournalEntry;
        }

        private void OnNewJournalEntry(JournalEntry entry)
        {
            // Aggiunge la voce alla collezione (thread-safe grazie a EnableCollectionSynchronization)
            lock (_lock)
            {
                Entries.Insert(0, entry); // Le più recenti in alto
                
                // Mantiene il limite UI (es. 200 messaggi per performance)
                if (Entries.Count > 200)
                {
                    Entries.RemoveAt(Entries.Count - 1);
                }
            }
        }

        [RelayCommand]
        private void Clear()
        {
            lock (_lock)
            {
                Entries.Clear();
                _journalService.Clear();
            }
        }

        [RelayCommand]
        private void Export()
        {
            // Logica di esportazione in file (implementazione futura)
        }

        public void Dispose()
        {
            _journalService.OnNewEntry -= OnNewJournalEntry;
        }
    }
}
