namespace TMRazorImproved.Shared.Interfaces
{
    /// <summary>
    /// Servizio WebSocket server per il controllo remoto di TMRazorImproved.
    /// Espone le funzionalità di esecuzione e registrazione script a tool esterni
    /// tramite un protocollo binario basato su protobuf over WebSocket.
    /// Porta default: 15454-15463 (primo disponibile).
    /// Endpoint: ws://127.0.0.1:{port}/proto
    /// </summary>
    public interface IProtoControlService
    {
        /// <summary>Porta assegnata al server, null se non avviato.</summary>
        int? Port { get; }

        /// <summary>True se il server è attualmente in ascolto.</summary>
        bool IsRunning { get; }

        /// <summary>Avvia il server WebSocket. Trova automaticamente la prima porta libera nell'intervallo 15454-15463.</summary>
        bool Start();

        /// <summary>Ferma il server WebSocket e tutte le sessioni attive.</summary>
        void Stop();
    }
}
