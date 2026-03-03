using TMRazorImproved.Shared.Interfaces;

namespace TMRazorImproved.Core.Services.Scripting.Api
{
    public class FiltersApi
    {
        private readonly ScriptCancellationController _cancel;

        public FiltersApi(ScriptCancellationController cancel)
        {
            _cancel = cancel;
        }

        public virtual void Enable(string name) { }
        public virtual void Disable(string name) { }
        public virtual bool IsEnabled(string name) => false;
    }
}
