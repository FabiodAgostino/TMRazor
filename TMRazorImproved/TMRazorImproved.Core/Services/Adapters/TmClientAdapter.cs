using System;
using TMRazorImproved.Shared.Enums;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Interfaces.Adapters;

namespace TMRazorImproved.Core.Services.Adapters
{
    /// <summary>
    /// Adapter per TmClient e ClassicUO (approccio plugin).
    /// La ricezione pacchetti è gestita da PacketService tramite polling della shared memory.
    /// Questo adapter gestisce la connessione e il ciclo di vita del processo.
    /// </summary>
    public class TmClientAdapter : IClientAdapter
    {
        private readonly IClientInteropService _interop;
        private int _connectedPid;

        public TmClientAdapter(IClientInteropService interop)
        {
            _interop = interop;
        }

        public bool Connect(int processId)
        {
            _connectedPid = processId;
            System.Diagnostics.Trace.WriteLine($"[TmClientAdapter] Connected to PID {processId}");
            return processId > 0;
        }

        public void Disconnect()
        {
            System.Diagnostics.Trace.WriteLine($"[TmClientAdapter] Disconnecting from PID {_connectedPid}");
            _connectedPid = 0;
        }

        /// <summary>
        /// La ricezione pacchetti è gestita da PacketService direttamente via shared memory.
        /// Questo metodo non è utilizzato per client plugin-based.
        /// </summary>
        public byte[] ReceivePacket(PacketPath direction) => Array.Empty<byte>();

        /// <summary>
        /// L'invio pacchetti è gestito da PacketService.SendToServer/SendToClient.
        /// </summary>
        public void SendPacket(byte[] data, PacketPath direction) { }
    }
}
