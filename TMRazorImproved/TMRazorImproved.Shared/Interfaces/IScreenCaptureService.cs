using System.Threading.Tasks;

namespace TMRazorImproved.Shared.Interfaces
{
    public interface IScreenCaptureService
    {
        /// <summary>Cattura uno screenshot del client UO corrente.</summary>
        /// <returns>Il percorso del file salvato.</returns>
        Task<string> CaptureAsync();

        /// <summary>Configura il percorso di salvataggio degli screenshot.</summary>
        void SetCapturePath(string path);

        /// <summary>Ritorna il percorso attuale degli screenshot.</summary>
        string GetCapturePath();
    }
}
