using System;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.Shared.Interfaces
{
    public interface ITargetingService
    {
        uint LastTarget { get; set; }

        // 039-A: Target per tipo di cursore (harm/bene/ground)
        uint LastHarmTarget { get; set; }
        uint LastBeneTarget { get; set; }
        ushort LastGroundX { get; }
        ushort LastGroundY { get; }
        sbyte LastGroundZ { get; }

        bool HasPrompt { get; }

        // true quando il server ha inviato 0x6C S2C e si aspetta ancora una risposta
        bool HasTargetCursor { get; }
        // Il cursor ID inviato dal server nell'ultimo 0x6C S2C; 0 se nessuno pendente
        uint PendingCursorId { get; }
        // Il tipo di cursore (0=neutral, 1=harmful, 2=beneficial)
        byte PendingCursorType { get; }

        // Fired ogni volta che il server invia 0x6C S2C (richiesta di target)
        event Action<uint> TargetCursorRequested;
        event Action<TargetInfo> TargetReceived;
        event Action<bool> PromptChanged;

        // Azzera HasTargetCursor e PendingCursorId (chiamato automaticamente da SendTarget/CancelTarget)
        void ClearTargetCursor();

        void TargetNext();
        void TargetClosest();
        void TargetSelf();
        void Clear();

        // 039-B/C: Smart Last Target con queue quando il cursore non è attivo
        void DoLastTarget();

        // Metodi per l'invio fisico dei pacchetti di targeting
        void SendTarget(uint serial);
        void SendTarget(uint serial, ushort x, ushort y, sbyte z, ushort graphic);
        void SetLastTarget(uint serial);
        void CancelTarget();

        // Metodi per i prompt (0x9A, 0xC2)
        // FIX P1-02: serial e promptId tracciati dal pacchetto 0x9A S2C
        uint PendingPromptSerial { get; }
        uint PendingPromptId { get; }
        bool PendingPromptIsUnicode { get; }
        void SendPrompt(string text);
        void SetPrompt(bool hasPrompt);

        // Richiede un target al client e notifica tramite evento
        void RequestTarget();

        /// <summary>Richiede al client di selezionare una locazione (terreno/mappa).</summary>
        void RequestLocationTarget();

        // Richiede un target al client e attende il risultato in modo asincrono
        Task<TargetInfo> AcquireTargetAsync();
    }
}
