using System;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.Shared.Interfaces
{
    public interface ITargetingService
    {
        uint LastTarget { get; set; }
        
        event Action<uint> TargetReceived;

        void TargetNext();
        void TargetClosest();
        void TargetSelf();
        void Clear();

        // Metodi per l'invio fisico dei pacchetti di targeting
        void SendTarget(uint serial);
        void SendTarget(uint serial, ushort x, ushort y, sbyte z, ushort graphic);
        void SetLastTarget(uint serial);
        void CancelTarget();

        // Richiede un target al client e notifica tramite evento
        void RequestTarget();

        // Richiede un target al client e attende il risultato in modo asincrono
        Task<uint> AcquireTargetAsync();
    }
}
