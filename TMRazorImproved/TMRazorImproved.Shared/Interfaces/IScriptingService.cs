using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMRazorImproved.Shared.Enums;

namespace TMRazorImproved.Shared.Interfaces
{
    /// <summary>
    /// Stato di completamento di uno script.
    /// </summary>
    public record ScriptCompletionInfo(
        string ScriptName,
        bool WasCancelled,
        Exception? Error,
        TimeSpan Elapsed);

    /// <summary>
    /// Servizio per l'esecuzione di script (Python, UOSteam, C#).
    /// Gestisce il ciclo di vita del motore, la cancellazione cooperativa
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
        /// Avvia l'esecuzione asincrona di uno script nel linguaggio specificato.
        /// Un solo script può girare alla volta.
        /// </summary>
        Task RunAsync(string code, ScriptLanguage language = ScriptLanguage.Python,
                      string scriptName = "unnamed",
                      CancellationToken externalToken = default);

        /// <summary>Richiede l'interruzione cooperativa dello script in corso.</summary>
        Task StopAsync();

        /// <summary>Richiede l'interruzione di qualsiasi script con il nome specificato.</summary>
        void StopScript(string name);

        /// <summary>Ritorna la lista dei nomi degli script caricati (es. nomi dei file).</summary>
        IEnumerable<string> GetLoadedScripts();

        /// <summary>Esegue uno script caricato cercandolo per nome.</summary>
        Task RunScript(string name);

        /// <summary>Esegue una scansione di sicurezza sul codice e ritorna eventuali avvisi.</summary>
        IEnumerable<string> ValidateScript(string code, ScriptLanguage language);
    }
}
