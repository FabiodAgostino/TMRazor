using System;
using TMRazorImproved.Shared.Enums;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.Core.Handlers
{
    public class FriendsHandler
    {
        private readonly IPacketService _packetService;
        private readonly IFriendsService _friendsService;

        public FriendsHandler(IPacketService packetService, IFriendsService friendsService)
        {
            _packetService = packetService;
            _friendsService = friendsService;

            RegisterHandlers();
        }

        private void RegisterHandlers()
        {
            // Filter outgoing Attack packets (0x05)
            _packetService.RegisterFilter(PacketPath.ClientToServer, 0x05, OnAttackRequest);

            // Viewer for Party Invites (0xBF sub 0x06 type 0x07)
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0xBF, OnExtendedPacket);
        }

        private bool OnAttackRequest(byte[] data)
        {
            if (!_friendsService.ActiveList.PreventAttack) return true;

            if (data.Length >= 5)
            {
                var reader = new UOBufferReader(data);
                reader.ReadByte(); // 0x05
                uint serial = reader.ReadUInt32();

                if (_friendsService.IsFriend(serial))
                {
                    // Block attack on friend
                    return false;
                }
            }
            return true;
        }

        private void OnExtendedPacket(byte[] data)
        {
            if (data.Length < 5) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();       // 0xBF
            reader.ReadUInt16();     // length
            ushort sub = reader.ReadUInt16();

            if (sub == 0x06) // Party Message
            {
                if (reader.Remaining < 1) return;
                byte type = reader.ReadByte();

                if (type == 0x07) // Party Invite
                {
                    if (_friendsService.ActiveList.AutoAcceptParty)
                    {
                        if (reader.Remaining >= 4)
                        {
                            uint leaderSerial = reader.ReadUInt32();
                            AcceptParty(leaderSerial);
                        }
                    }
                }
            }
        }

        private void AcceptParty(uint leaderSerial)
        {
            // 0xBF sub 0x06 type 0x08: Accept Party
            // length = 10 (0xBF 1 0x000A 0x0006 0x08 leader(4))
            byte[] response = new byte[10];
            var writer = new UOBufferWriter(response);
            writer.Write((byte)0xBF);
            writer.Write((ushort)10);
            writer.Write((ushort)0x06);
            writer.Write((byte)0x08);
            writer.Write(leaderSerial);
            
            _packetService.SendToServer(response);
        }
    }

    // Helper writer since I don't see one in Shared
    internal class UOBufferWriter
    {
        private byte[] _buffer;
        private int _pos;

        public UOBufferWriter(byte[] buffer) { _buffer = buffer; }

        public void Write(byte b) => _buffer[_pos++] = b;
        public void Write(ushort s) { _buffer[_pos++] = (byte)(s >> 8); _buffer[_pos++] = (byte)s; }
        public void Write(uint i)
        {
            _buffer[_pos++] = (byte)(i >> 24);
            _buffer[_pos++] = (byte)(i >> 16);
            _buffer[_pos++] = (byte)(i >> 8);
            _buffer[_pos++] = (byte)i;
        }
    }
}
