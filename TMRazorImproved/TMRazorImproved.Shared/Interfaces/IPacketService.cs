using System;
using TMRazorImproved.Shared.Enums;

namespace TMRazorImproved.Shared.Interfaces
{
    public interface IPacketService
    {
        /// <summary>
        /// Invia un pacchetto al server
        /// </summary>
        void SendToServer(byte[] data);

        /// <summary>
        /// Invia un pacchetto al client
        /// </summary>
        void SendToClient(byte[] data);

        /// <summary>
        /// Registra un osservatore per un pacchetto specifico. Non può bloccare il pacchetto.
        /// </summary>
        void RegisterViewer(PacketPath path, int packetId, Action<byte[]> callback);

        /// <summary>
        /// Registra un filtro per un pacchetto specifico. Se ritorna false, il pacchetto viene bloccato.
        /// </summary>
        void RegisterFilter(PacketPath path, int packetId, Func<byte[], bool> callback);

        /// <summary>
        /// Rimuove un osservatore
        /// </summary>
        void UnregisterViewer(PacketPath path, int packetId, Action<byte[]> callback);

        /// <summary>
        /// Rimuove un filtro
        /// </summary>
        void UnregisterFilter(PacketPath path, int packetId, Func<byte[], bool> callback);

        /// <summary>
        /// Gestisce i messaggi in arrivo dal loop dei messaggi di Windows (WndProc)
        /// </summary>
        bool OnMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam);
    }
}
