using System;
using TMRazorImproved.Shared.Enums;

namespace TMRazorImproved.Shared.Interfaces.Adapters
{
    public interface IClientAdapter
    {
        bool Connect(int processId);
        void SendPacket(byte[] data, PacketPath direction);
        byte[] ReceivePacket(PacketPath direction);
        void Disconnect();
    }
}
