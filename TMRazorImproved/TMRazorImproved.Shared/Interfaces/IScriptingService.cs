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

        /// <summary>
        /// Scattato quando un file script nella cartella viene creato, modificato, rinominato o eliminato.
        /// Parametro: percorso completo del file coinvolto (può essere null per rename/delete).
        /// </summary>
        event Action<string?>? ScriptsChanged;

        /// <summary>Riga emessa da print() o stdout dello script.</summary>
        event Action<string>? OutputReceived;

        /// <summary>Errore o eccezione non gestita dello script.</summary>
        event Action<string>? ErrorReceived;

        /// <summary>Notifica al termine (completato, cancellato o con errore).</summary>
        event Action<ScriptCompletionInfo>? ScriptCompleted;

        /// <summary>
        /// Avvia l'esecuzione asincrona di uno script nel linguaggio specificato.
        /// Un solo script può girare alla volta.
        /// Se <paramref name="loop"/> è <c>true</c>, lo script viene rieseguito
        /// in ciclo continuo fino alla cancellazione (equivale a un while-true esterno).
        /// </summary>
        Task RunAsync(string code, ScriptLanguage language = ScriptLanguage.Python,
                      string scriptName = "unnamed",
                      CancellationToken externalToken = default,
                      bool loop = false);

        /// <summary>Richiede l'interruzione cooperativa dello script in corso.</summary>
        Task StopAsync();

        /// <summary>Richiede l'interruzione di qualsiasi script con il nome specificato.</summary>
        void StopScript(string name);

        /// <summary>Ritorna la lista dei nomi degli script caricati (es. nomi dei file).</summary>
        IEnumerable<string> GetLoadedScripts();

        /// <summary>
        /// Esegue uno script caricato cercandolo per nome.
        /// Se <paramref name="loop"/> è <c>true</c>, lo script viene rieseguito ciclicamente fino allo stop.
        /// </summary>
        Task RunScript(string name, bool loop = false);

        /// <summary>Esegue una scansione di sicurezza sul codice e ritorna eventuali avvisi.</summary>
        IEnumerable<string> ValidateScript(string code, ScriptLanguage language);

        /// <summary>
        /// Pre-compila tutti gli script C# marcati con <c>Preload = true</c> nel profilo corrente,
        /// senza eseguirli. Riduce la latenza della prima esecuzione (Roslyn JIT warm-up).
        /// </summary>
        Task PreloadScripts();

        /// <summary>
        /// Invoca una funzione Python per nome nel contesto dello script Python
        /// attualmente in esecuzione (callback C# -> Python).
        /// Ritorna <c>null</c> se nessuno script Python è in esecuzione, se la funzione
        /// non esiste nello scope corrente o se si verifica un errore durante l'invocazione.
        /// </summary>
        object? CallPythonFunction(string functionName, params object[] args);
    }
}
