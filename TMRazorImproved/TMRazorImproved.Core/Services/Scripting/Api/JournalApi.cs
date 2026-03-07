using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.Core.Services.Scripting.Api
{
    public class JournalApi
    {
        private readonly IJournalService _journal;
        private readonly ScriptCancellationController _cancel;

        public JournalApi(IJournalService journal, ScriptCancellationController cancel)
        {
            _journal = journal;
            _cancel = cancel;
        }

        public virtual bool InJournal(string text)
        {
            _cancel.ThrowIfCancelled();
            return _journal.Contains(text);
        }

        /// <summary>Ritorna il numero totale di messaggi nel journal (max 500).</summary>
        public virtual int GetJournalCount()
        {
            _cancel.ThrowIfCancelled();
            return _journal.Entries.Count();
        }

        /// <summary>Ritorna il testo del messaggio all'indice specificato (0 = più vecchio).</summary>
        public virtual string GetJournalEntry(int index)
        {
            _cancel.ThrowIfCancelled();
            var entries = _journal.Entries.ToList();
            if (index < 0 || index >= entries.Count) return string.Empty;
            return entries[index].Text;
        }

        /// <summary>Ritorna tutti i messaggi che contengono il testo specificato.</summary>
        public virtual System.Collections.Generic.IEnumerable<string> GetJournalEntries(string text)
        {
            _cancel.ThrowIfCancelled();
            return _journal.Entries
                .Where(e => e.Text.Contains(text, StringComparison.OrdinalIgnoreCase))
                .Select(e => e.Text)
                .ToList();
        }

        /// <summary>Cerca il testo nel journal e ritorna vero se trovato.</summary>
        public virtual bool SearchJournal(string text)
        {
            _cancel.ThrowIfCancelled();
            return _journal.Entries.Any(e => e.Text.Contains(text, StringComparison.OrdinalIgnoreCase));
        }

        public virtual void Clear()
        {
            _cancel.ThrowIfCancelled();
            _journal.Clear();
        }

        public virtual string? GetLast()
        {
            _cancel.ThrowIfCancelled();
            return _journal.GetLast()?.Text;
        }

        public virtual bool WaitJournal(string text, int timeoutMs = 5000)
        {
            var deadline = Environment.TickCount64 + timeoutMs;
            while (Environment.TickCount64 < deadline)
            {
                _cancel.ThrowIfCancelled();
                if (_journal.Contains(text)) return true;
                System.Threading.Thread.Sleep(50);
            }
            return false;
        }

        /// <summary>Attende la comparsa di un nuovo messaggio nel journal.</summary>
        public virtual bool WaitForJournal(string text, int timeoutMs = 5000)
        {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            Action<JournalEntry> handler = entry =>
            {
                if (entry.Text.Contains(text, StringComparison.OrdinalIgnoreCase))
                    tcs.TrySetResult(true);
            };

            _journal.OnNewEntry += handler;
            try
            {
                using var cts = new CancellationTokenSource();
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, _cancel.Token);
                cts.CancelAfter(timeoutMs);
                
                var task = tcs.Task;
                // WaitSync in .NET 6+ o Task.WhenAny per compatibilità
                if (Task.WhenAny(task, Task.Delay(timeoutMs, linkedCts.Token)).GetAwaiter().GetResult() == task)
                    return task.Result;
                
                return false;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            finally
            {
                _journal.OnNewEntry -= handler;
            }
        }

        // ------------------------------------------------------------------
        // Ricerca avanzata
        // ------------------------------------------------------------------

        /// <summary>
        /// True se almeno una entry nel journal contiene il testo (case-insensitive)
        /// con la stessa semantica di <see cref="InJournal"/> ma con nome RazorEnhanced.
        /// </summary>
        public virtual bool InJournalLine(string text)
        {
            _cancel.ThrowIfCancelled();
            return InJournal(text);
        }

        /// <summary>
        /// True se almeno una entry nel journal corrisponde all'espressione regolare.
        /// Utile per pattern come "(\d+) gold" o "You (gained|lost)".
        /// </summary>
        public virtual bool InJournalRegex(string pattern)
        {
            _cancel.ThrowIfCancelled();
            if (string.IsNullOrEmpty(pattern)) return false;
            try
            {
                var rx = new Regex(pattern, RegexOptions.IgnoreCase);
                return _journal.Entries.Any(e => rx.IsMatch(e.Text));
            }
            catch (RegexParseException) { return false; }
        }

        /// <summary>
        /// True se almeno una entry contiene il testo E il Name corrisponde al tipo.
        /// JournalEntry.Name contiene il nome del mittente (es. "System" per messaggi di sistema).
        /// Usare "System" per messaggi di sistema, nome del mobile per chat normale.
        /// </summary>
        public virtual bool InJournalByType(string text, string type)
        {
            _cancel.ThrowIfCancelled();
            if (string.IsNullOrEmpty(text)) return false;
            return _journal.Entries.Any(e =>
                e.Text.Contains(text, StringComparison.OrdinalIgnoreCase) &&
                e.Name.Equals(type, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Ritorna il testo dell'ultima entry dove Name corrisponde al tipo, null se assente.
        /// </summary>
        public virtual string? GetLastByType(string type)
        {
            _cancel.ThrowIfCancelled();
            return _journal.Entries
                .LastOrDefault(e => e.Name.Equals(type, StringComparison.OrdinalIgnoreCase))
                ?.Text;
        }

        /// <summary>
        /// True se almeno una entry contiene il testo E proviene dal serial specificato.
        /// </summary>
        public virtual bool InJournalBySerial(string text, uint serial)
        {
            _cancel.ThrowIfCancelled();
            if (string.IsNullOrEmpty(text)) return false;
            return _journal.Entries.Any(e =>
                e.Text.Contains(text, StringComparison.OrdinalIgnoreCase) &&
                e.Serial == serial);
        }

        /// <summary>
        /// Ritorna tutte le entries che corrispondono al pattern regex.
        /// </summary>
        public virtual List<string> GetJournalEntriesRegex(string pattern)
        {
            _cancel.ThrowIfCancelled();
            if (string.IsNullOrEmpty(pattern)) return new List<string>();
            try
            {
                var rx = new Regex(pattern, RegexOptions.IgnoreCase);
                return _journal.Entries
                    .Where(e => rx.IsMatch(e.Text))
                    .Select(e => e.Text)
                    .ToList();
            }
            catch (RegexParseException) { return new List<string>(); }
        }
    }
}
