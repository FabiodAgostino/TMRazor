using System;
using TMRazorImproved.Shared.Enums;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Interfaces.Adapters;

namespace TMRazorImproved.Core.Services.Adapters
{
    public class OsiClientAdapter : IClientAdapter
    {
        private readonly IClientInteropService _interopService;

        public OsiClientAdapter(IClientInteropService interopService)
        {
            _interopService = interopService;
        }

        public bool Connect(int processId)
        {
            // Placeholder: Connection is currently managed in PacketService using _interopService.GetSharedAddress()
            return true;
        }

        public void Disconnect()
        {
            // Placeholder: Cleanup shared memory handles if required
        }

        public byte[] ReceivePacket(PacketPath direction)
        {
            // Placeholder: Handled via shared memory polling/hooks in PacketService currently.
            return Array.Empty<byte>();
        }

        public void SendPacket(byte[] data, PacketPath direction)
        {
            // Placeholder: Uses _interopService for shared memory write
        }
    }
}
