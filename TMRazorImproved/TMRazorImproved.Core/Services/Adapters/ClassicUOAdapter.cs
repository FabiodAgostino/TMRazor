using System;
using TMRazorImproved.Shared.Enums;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Interfaces.Adapters;

namespace TMRazorImproved.Core.Services.Adapters
{
    /// <summary>
    /// Adapter per ClassicUO (approccio plugin, identico a TmClient).
    /// La ricezione pacchetti è gestita da PacketService tramite polling della shared memory.
    /// </summary>
    public class ClassicUOAdapter : IClientAdapter
    {
        private readonly IClientInteropService _interop;
        private int _connectedPid;

        public ClassicUOAdapter(IClientInteropService interop)
        {
            _interop = interop;
        }

        public bool Connect(int processId)
        {
            _connectedPid = processId;
            System.Diagnostics.Trace.WriteLine($"[ClassicUOAdapter] Connected to PID {processId}");
            return processId > 0;
        }

        public void Disconnect()
        {
            System.Diagnostics.Trace.WriteLine($"[ClassicUOAdapter] Disconnecting from PID {_connectedPid}");
            _connectedPid = 0;
        }

        /// <summary>
        /// La ricezione pacchetti è gestita da PacketService direttamente via shared memory.
        /// </summary>
        public byte[] ReceivePacket(PacketPath direction) => Array.Empty<byte>();

        /// <summary>
        /// L'invio pacchetti è gestito da PacketService.SendToServer/SendToClient.
        /// </summary>
        public void SendPacket(byte[] data, PacketPath direction) { }
    }
}
