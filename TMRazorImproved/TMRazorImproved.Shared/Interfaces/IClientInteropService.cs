using System;

namespace TMRazorImproved.Shared.Interfaces
{
    /// <summary>
    /// Interfaccia per la gestione dell'avvio del client UO e l'interazione con le DLL native.
    /// </summary>
    public interface IClientInteropService
    {
        /// <summary>
        /// Avvia il client UO tramite Loader.dll
        /// </summary>
        uint LaunchClient(string exePath, string dllPath);

        /// <summary>
        /// Inizializza la libreria Crypt.dll per un processo specifico
        /// </summary>
        bool InstallLibrary(IntPtr windowHandle, int processId, int features);

        /// <summary>
        /// Chiude la connessione con il client
        /// </summary>
        void Shutdown(bool closeClient);

        /// <summary>
        /// Ottiene l'Handle della finestra di Ultima Online
        /// </summary>
        IntPtr FindUOWindow();

        /// <summary>
        /// Ottiene l'Handle della finestra corrente per operazioni di cattura.
        /// </summary>
        IntPtr GetWindowHandle();

        /// <summary>
        /// Ottiene l'ID del processo di Ultima Online attualmente agganciato
        /// </summary>
        int GetUOProcessId();

        /// <summary>
        /// Ottiene l'indirizzo della memoria condivisa con Crypt.dll
        /// </summary>
        IntPtr GetSharedAddress();

        /// <summary>
        /// Ottiene l'handle del Mutex di comunicazione
        /// </summary>
        IntPtr GetCommMutex();

        /// <summary>
        /// Calcola la lunghezza di un pacchetto nel buffer
        /// </summary>
        unsafe int GetPacketLength(byte* data, int bufLen);

        /// <summary>
        /// Copia in modo sicuro la memoria nativa
        /// </summary>
        unsafe void CopyMemory(void* dest, void* src, int len);

        /// <summary>
        /// Invia un messaggio asincrono a una finestra (Win32 PostMessage)
        /// </summary>
        bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// Cambia il titolo di una finestra (Win32 SetWindowText)
        /// </summary>
        bool SetWindowText(IntPtr hWnd, string text);
    }
}
