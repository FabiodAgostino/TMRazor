using System;

namespace TMRazorImproved.Shared.Models
{
    /// <summary>
    /// Rappresenta un pacchetto di Ultima Online.
    /// </summary>
    public class UOPacket
    {
        public byte PacketId { get; }
        public byte[] Data { get; }

        public UOPacket(byte[] data)
        {
            if (data == null || data.Length == 0)
                throw new ArgumentException("I dati del pacchetto non possono essere vuoti.", nameof(data));

            Data = data;
            PacketId = data[0];
        }

        public int Length => Data.Length;

        public override string ToString()
        {
            return $"Packet 0x{PacketId:X2}, Length: {Length}";
        }
    }
}
