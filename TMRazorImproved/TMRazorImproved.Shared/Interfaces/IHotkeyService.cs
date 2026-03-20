using System;
using System.Threading.Tasks;

namespace TMRazorImproved.Shared.Interfaces
{
    public interface IHotkeyService
    {
        bool IsEnabled { get; set; }
        void Start();
        Task StopAsync();

        // Permette di registrare callback programmatici per azioni specifiche
        void RegisterAction(string actionName, Action execute);

        /// <summary>Nome dell'ultima azione hotkey eseguita, o null se nessuna ancora.</summary>
        string? LastActionName { get; }
    }
}
