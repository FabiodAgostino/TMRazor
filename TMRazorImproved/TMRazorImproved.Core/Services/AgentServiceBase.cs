using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;
using TMRazorImproved.Shared.Models.Config;

namespace TMRazorImproved.Core.Services
{
    /// <summary>
    /// Classe base per tutti i servizi in background (AutoLoot, Scavenger, Macro)
    /// Sostituisce il vecchio meccanismo basato su Thread.Abort() in .NET 10
    /// utilizzando Task e CancellationToken cooperativi.
    /// </summary>
    public abstract class AgentServiceBase : IAgentService, IDisposable
    {
        protected readonly IConfigService _configService;
        private CancellationTokenSource? _cts;
        private Task? _agentTask;

        protected AgentServiceBase(IConfigService configService)
        {
            _configService = configService;
        }

        public bool IsRunning => _agentTask != null && !_agentTask.IsCompleted;

        /// <summary>
        /// Ottiene la configurazione attiva per una specifica categoria (AutoLoot, Dress, ecc.)
        /// </summary>
        protected TConfig? GetActiveConfig<TConfig>(
            Func<UserProfile, IList<TConfig>> getList,
            Func<UserProfile, string?> getActiveName)
            where TConfig : class, INamedConfig
        {
            var profile = _configService.CurrentProfile;
            if (profile == null) return null;
            var list = getList(profile);
            var active = getActiveName(profile);
            return list.FirstOrDefault(c => c.Name == active) ?? list.FirstOrDefault();
        }

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

        protected bool MatchProperties(Item item, List<PropertyFilter> filters)
        {
            var itemProps = item.Properties;
            if (itemProps == null || itemProps.Count == 0) return false;

            return filters.All(pf =>
            {
                var matchingProp = itemProps.FirstOrDefault(p => p.Contains(pf.Name, StringComparison.OrdinalIgnoreCase));
                if (matchingProp == null) return false;

                double val = ExtractValue(matchingProp);
                return val >= pf.MinValue && val <= pf.MaxValue;
            });
        }

        protected double ExtractValue(string propertyLine)
        {
            // Regex semplice per estrarre il primo numero (intero o decimale) dalla riga
            var match = Regex.Match(propertyLine, @"(\d+(\.\d+)?)");
            if (match.Success && double.TryParse(match.Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double result))
            {
                return result;
            }
            return 0;
        }

        public void Dispose()
        {
            Stop();
            GC.SuppressFinalize(this);
        }
    }
}
