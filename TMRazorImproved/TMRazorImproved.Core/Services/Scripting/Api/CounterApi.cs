using TMRazorImproved.Shared.Interfaces;

namespace TMRazorImproved.Core.Services.Scripting.Api
{
    public class CounterApi
    {
        private readonly ICounterService _counter;
        private readonly ScriptCancellationController _cancel;

        public CounterApi(ICounterService counter, ScriptCancellationController cancel)
        {
            _counter = counter;
            _cancel = cancel;
        }

        /// <summary>
        /// Restituisce il conteggio degli item con il graphic specificato nel backpack e suoi container.
        /// </summary>
        public virtual int GetCount(int graphic, int hue = 0)
        {
            _cancel.ThrowIfCancelled();
            return _counter.GetCount((ushort)graphic, (ushort)hue);
        }

        /// <summary>
        /// Forza il ricalcolo immediato di tutti i counter.
        /// </summary>
        public virtual void Recalculate()
        {
            _cancel.ThrowIfCancelled();
            _counter.RecalculateAll();
        }
    }
}
