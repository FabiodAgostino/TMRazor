using System;
using System.Threading;
using System.Threading.Tasks;

namespace TMRazorImproved.Shared.Interfaces
{
    /// <summary>
    /// Stato di completamento di uno script Python.
    /// </summary>
    public record ScriptCompletionInfo(
        string ScriptName,
        bool WasCancelled,
        Exception? Error,
        TimeSpan Elapsed);

    /// <summary>
    /// Servizio per l'esecuzione di script Python tramite IronPython.
    /// Gestisce il ciclo di vita del motore, la cancellazione cooperativa via sys.settrace
    /// e il reindirizzamento dell'output verso la UI.
    /// </summary>
    public interface IScriptingService
    {
        /// <summary>True se uno script è attualmente in esecuzione.</summary>
        bool IsRunning { get; }

        /// <summary>Nome dello script corrente (null se nessuno in esecuzione).</summary>
        string? CurrentScriptName { get; }

        /// <summary>Riga emessa da print() o stdout dello script.</summary>
        event Action<string>? OutputReceived;

        /// <summary>Errore o eccezione non gestita dello script.</summary>
        event Action<string>? ErrorReceived;

        /// <summary>Notifica al termine (completato, cancellato o con errore).</summary>
        event Action<ScriptCompletionInfo>? ScriptCompleted;

        /// <summary>
        /// Avvia l'esecuzione asincrona di uno script Python.
        /// Un solo script può girare alla volta; una seconda chiamata con script in
        /// esecuzione viene rifiutata con un messaggio su ErrorReceived.
        /// </summary>
        Task RunAsync(string code, string scriptName = "unnamed",
                      CancellationToken externalToken = default);

        /// <summary>Richiede l'interruzione cooperativa dello script in corso.</summary>
        Task StopAsync();
    }
}
