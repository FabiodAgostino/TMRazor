using System;
using System.Buffers.Binary;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.Core.Services.Scripting.Api
{
    public class GumpsApi
    {
        private readonly IWorldService _world;
        private readonly IPacketService _packet;
        private readonly ScriptCancellationController _cancel;

        public GumpsApi(IWorldService world, IPacketService packet, ScriptCancellationController cancel)
        {
            _world = world;
            _packet = packet;
            _cancel = cancel;
        }

        public virtual bool HasGump() => _world.CurrentGump != null;

        public virtual uint CurrentID() => _world.CurrentGump?.GumpId ?? 0;

        public virtual void SendAction(int buttonId)
        {
            _cancel.ThrowIfCancelled();
            var gump = _world.CurrentGump;
            if (gump == null) return;

            // Pacchetto 0xB1: Gump Response
            // [0] 0xB1
            // [1-2] Length
            // [3-6] Serial
            // [7-10] GumpID
            // [11-14] ButtonID
            // [15-18] Switches Count (0 per ora)
            // [19-22] Entries Count (0 per ora)

            byte[] packet = new byte[23];
            packet[0] = 0xB1;
            BinaryPrimitives.WriteUInt16BigEndian(packet.AsSpan(1), (ushort)packet.Length);
            BinaryPrimitives.WriteUInt32BigEndian(packet.AsSpan(3), gump.Serial);
            BinaryPrimitives.WriteUInt32BigEndian(packet.AsSpan(7), gump.GumpId);
            BinaryPrimitives.WriteUInt32BigEndian(packet.AsSpan(11), (uint)buttonId);
            BinaryPrimitives.WriteUInt32BigEndian(packet.AsSpan(15), 0); // Switches count
            BinaryPrimitives.WriteUInt32BigEndian(packet.AsSpan(19), 0); // Text entries count

            _packet.SendToServer(packet);
            
            // Rimuoviamo il gump localmente dato che abbiamo risposto
            _world.RemoveGump();
        }

        public virtual void Close() => SendAction(0);

        public virtual bool WaitForGump(uint gumpId, int timeoutMs = 5000)
        {
            var deadline = Environment.TickCount64 + timeoutMs;
            while (Environment.TickCount64 < deadline)
            {
                _cancel.ThrowIfCancelled();
                if (_world.CurrentGump?.GumpId == gumpId) return true;
                System.Threading.Thread.Sleep(50);
            }
            return false;
        }
    }
}
