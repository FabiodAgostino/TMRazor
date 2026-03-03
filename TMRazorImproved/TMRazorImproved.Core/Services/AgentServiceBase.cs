using System;
using System.Threading;
using System.Threading.Tasks;
using TMRazorImproved.Shared.Interfaces;

namespace TMRazorImproved.Core.Services
{
    /// <summary>
    /// Classe base per tutti i servizi in background (AutoLoot, Scavenger, Macro)
    /// Sostituisce il vecchio meccanismo basato su Thread.Abort() in .NET 10
    /// utilizzando Task e CancellationToken cooperativi.
    /// </summary>
    public abstract class AgentServiceBase : IAgentService, IDisposable
    {
        private CancellationTokenSource? _cts;
        private Task? _agentTask;

        public bool IsRunning => _agentTask != null && !_agentTask.IsCompleted;

        /// <summary>
        /// Avvia l'agente asincrono in background.
        /// </summary>
        public void Start()
        {
            if (IsRunning) return;

            _cts = new CancellationTokenSource();
            _agentTask = Task.Run(() => AgentLoopAsync(_cts.Token), _cts.Token);
        }

        /// <summary>
        /// Richiede la cancellazione cooperativa dell'agente e attende (opzionalmente) la sua terminazione.
        /// </summary>
        public async Task StopAsync()
        {
            if (!IsRunning || _cts == null) return;

            // Richiede la cancellazione
            _cts.Cancel();

            try
            {
                if (_agentTask != null)
                {
                    await _agentTask.ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                // Task è stato cancellato correttamente
            }
            finally
            {
                _cts.Dispose();
                _cts = null;
                _agentTask = null;
                OnStopped();
            }
        }

        /// <summary>
        /// Metodo di arresto sincrono per retrocompatibilità, sebbene StopAsync sia preferito.
        /// FIX BUG-P1-07: Wait() con timeout per evitare deadlock in contesti async.
        /// </summary>
        public void Stop()
        {
            if (!IsRunning || _cts == null) return;

            _cts.Cancel();
            try
            {
                if (_agentTask != null)
                {
                    // Timeout di 2 secondi: evita blocco indefinito se il loop non risponde
                    _agentTask.Wait(TimeSpan.FromSeconds(2));
                }
            }
            catch (AggregateException ex) when (ex.InnerException is OperationCanceledException)
            {
                // Gestione prevista: task cancellato correttamente
            }
            catch (Exception)
            {
                // Timeout o altra eccezione: procedi comunque con la pulizia
            }
            finally
            {
                _cts.Dispose();
                _cts = null;
                _agentTask = null;
                OnStopped();
            }
        }

        /// <summary>
        /// Il loop principale dell'agente che deve essere implementato.
        /// È OBBLIGATORIO chiamare regolarmente token.ThrowIfCancellationRequested()
        /// oppure passare il token a metodi asincroni come Task.Delay.
        /// </summary>
        protected abstract Task AgentLoopAsync(CancellationToken token);

        /// <summary>
        /// Hook opzionale per pulizia dello stato dopo lo stop.
        /// </summary>
        protected virtual void OnStopped()
        {
        }

        public void Dispose()
        {
            Stop();
            GC.SuppressFinalize(this);
        }
    }
}
