using System;

namespace TMRazorImproved.Shared.Interfaces
{
    /// <summary>
    /// FR-065: Layer di compatibilità UOAssist.
    /// Gestisce il protocollo WM_USER+200 usato da tool esterni (EasyUO, UOAssist-compatibili)
    /// per interagire con Razor. Solo shard specifici richiedono questa funzionalità.
    /// </summary>
    public interface IUoAssistService : IDisposable
    {
        /// <summary>
        /// Attiva il layer UOAssist collegandosi alla finestra WPF specificata.
        /// Deve essere chiamato dopo che la finestra è stata creata (dopo Loaded).
        /// </summary>
        /// <param name="windowHandle">HWND della finestra principale WPF.</param>
        void Initialize(IntPtr windowHandle);

        /// <summary>True se il layer è attivo e la finestra è agganciata.</summary>
        bool IsActive { get; }

        /// <summary>Numero di client esterni registrati.</summary>
        int NotificationCount { get; }

        // ── Metodi di notifica outbound (richiamati internamente dal codice di gioco) ──

        void PostLogin(uint serial);
        void PostLogout();
        void PostSkillUpdate(int skill, int val);
        void PostHitsUpdate(ushort max, ushort current);
        void PostManaUpdate(ushort max, ushort current);
        void PostStamUpdate(ushort max, ushort current);
        void PostMapChange(int map);
        void PostSpellCast(int spell);
        void PostMacroDone();
    }
}
