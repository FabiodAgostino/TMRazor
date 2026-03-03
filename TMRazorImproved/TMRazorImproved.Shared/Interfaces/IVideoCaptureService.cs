using System.Threading.Tasks;

namespace TMRazorImproved.Shared.Interfaces
{
    public interface IVideoCaptureService
    {
        bool IsRecording { get; }
        
        /// <summary>Inizia la registrazione video del client UO.</summary>
        Task<bool> StartAsync(int fps = 15);
        
        /// <summary>Ferma la registrazione video.</summary>
        Task StopAsync();
    }
}
