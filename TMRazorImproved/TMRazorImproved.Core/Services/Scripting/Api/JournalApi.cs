using System;
using System.Linq;
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
    }
}
