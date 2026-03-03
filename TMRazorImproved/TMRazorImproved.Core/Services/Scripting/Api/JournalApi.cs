using System;
using System.Linq;
using TMRazorImproved.Shared.Interfaces;

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
    }
}
