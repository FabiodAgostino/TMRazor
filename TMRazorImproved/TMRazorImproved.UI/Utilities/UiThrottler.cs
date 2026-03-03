using System;
using System.Windows.Threading;

namespace TMRazorImproved.UI.Utilities
{
    /// <summary>
    /// Limita la frequenza degli aggiornamenti alla UI usando un DispatcherTimer.
    /// Il callback viene eseguito sul thread UI all'intervallo specificato,
    /// permettendo al thread di rete di scrivere i dati ad alta frequenza
    /// senza causare colli di bottiglia sul Dispatcher.
    /// </summary>
    public sealed class UiThrottler : IDisposable
    {
        private readonly DispatcherTimer _timer;
        private bool _disposed;

        /// <param name="interval">Intervallo minimo tra due aggiornamenti UI (es. 100ms = 10fps).</param>
        /// <param name="callback">Azione eseguita sul thread UI ad ogni tick.</param>
        public UiThrottler(TimeSpan interval, Action callback)
        {
            _timer = new DispatcherTimer(DispatcherPriority.DataBind)
            {
                Interval = interval
            };
            _timer.Tick += (_, _) => callback();
            _timer.Start();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _timer.Stop();
            }
        }
    }
}
