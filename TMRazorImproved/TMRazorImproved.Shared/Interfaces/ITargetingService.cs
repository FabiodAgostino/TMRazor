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
        void SetLastTarget(uint serial);

        // Richiede un target al client e notifica tramite evento
        void RequestTarget();
    }
}
