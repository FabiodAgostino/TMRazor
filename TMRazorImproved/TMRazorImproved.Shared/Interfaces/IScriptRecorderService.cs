using TMRazorImproved.Shared.Enums;

namespace TMRazorImproved.Shared.Interfaces
{
    /// <summary>
    /// Servizio di registrazione script: cattura le azioni del giocatore intercettando
    /// i pacchetti client-to-server e le converte in codice Python o UOSteam.
    /// </summary>
    public interface IScriptRecorderService
    {
        /// <summary>True se la registrazione è attiva.</summary>
        bool IsRecording { get; }

        /// <summary>Linguaggio di output selezionato per la registrazione corrente.</summary>
        ScriptLanguage Language { get; }

        /// <summary>
        /// Avvia la registrazione catturando le azioni del giocatore.
        /// </summary>
        /// <param name="language">Linguaggio in cui generare il codice.</param>
        void StartRecording(ScriptLanguage language);

        /// <summary>
        /// Ferma la registrazione.
        /// </summary>
        void StopRecording();

        /// <summary>
        /// Restituisce il codice script registrato dall'ultima sessione.
        /// </summary>
        string GetRecordedScript();
    }
}
