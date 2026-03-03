using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.Messaging;
using TMRazorImproved.Shared.Enums;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Messages;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.Core.Handlers
{
    public class WorldPacketHandler
    {
        private readonly IPacketService _packetService;
        private readonly IWorldService _worldService;
        private readonly IJournalService _journalService;
        private readonly ILanguageService _languageService;
        private readonly IMessenger _messenger;

        public WorldPacketHandler(
            IPacketService packetService, 
            IWorldService worldService, 
            IJournalService journalService,
            ILanguageService languageService,
            IMessenger messenger)
        {
            _packetService = packetService;
            _worldService = worldService;
            _journalService = journalService;
            _languageService = languageService;
            _messenger = messenger;

            RegisterHandlers();
        }

        private void RegisterHandlers()
        {
            // ... (altri handler) ...
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x1B, HandleLoginConfirm);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0xBF, HandleExtendedPacket);

            // Messages & Journal
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x1C, HandleAsciiMessage);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0xAE, HandleUnicodeMessage);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0xAF, HandleLocalizedMessage);

            // Objects & Mobiles
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x78, HandleMobileIncoming);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x1A, HandleWorldItem);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x1D, HandleRemoveObject);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x20, HandleMobileUpdate);

            // Stats
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x11, HandleMobileStatus);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0xA1, HandleHitsUpdate);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0xA2, HandleManaUpdate);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0xA3, HandleStaminaUpdate);

            // Gumps
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0xB0, HandleGump);
            _packetService.RegisterViewer(PacketPath.ClientToServer, 0xB1, HandleGumpResponse);

            // OPL (Object Property List)
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0xD6, HandleOPL);
        }

        #region Login Handlers

        private void HandleMobileStatus(byte[] data)
        {
            var reader = new UOBufferReader(data);
            reader.ReadByte(); // 0x11
            reader.ReadUInt16(); // length
            uint serial = reader.ReadUInt32();
            string name = reader.ReadString(30);
            ushort hits = reader.ReadUInt16();
            ushort maxHits = reader.ReadUInt16();
            reader.ReadByte(); // can rename
            byte type = reader.ReadByte();

            var m = _worldService.FindMobile(serial);
            if (m == null)
            {
                m = new Mobile(serial);
                _worldService.AddMobile(m);
            }

            m.Name = name;
            m.Hits = hits;
            m.HitsMax = maxHits;

            // Se è il giocatore, il tipo definisce quanti dati extra ci sono (statistiche complete)
            if (m == _worldService.Player && type > 0)
            {
                // In UO, a seconda del tipo arrivano Str, Dex, Int, Stamina, Mana, ecc.
                // Implementazione parziale per brevità:
                if (type >= 1) 
                {
                    m.Str = reader.ReadUInt16();
                    m.Dex = reader.ReadUInt16();
                    m.Int = reader.ReadUInt16();
                    m.Stam = reader.ReadUInt16();
                    m.StamMax = reader.ReadUInt16();
                    m.Mana = reader.ReadUInt16();
                    m.ManaMax = reader.ReadUInt16();
                }
                
                // Notifica la UI del cambio globale di status
                _messenger.Send(new PlayerStatusMessage(StatType.Hits, serial, m.Hits, m.HitsMax));
                _messenger.Send(new PlayerStatusMessage(StatType.Mana, serial, m.Mana, m.ManaMax));
                _messenger.Send(new PlayerStatusMessage(StatType.Stamina, serial, m.Stam, m.StamMax));
            }
        }

        private void HandleLoginConfirm(byte[] data)
        {
            var reader = new UOBufferReader(data);
            reader.ReadByte(); // 0x1B
            uint serial = reader.ReadUInt32();
            
            var player = new Mobile(serial);
            _worldService.SetPlayer(player);
        }

        private void HandleExtendedPacket(byte[] data)
        {
            var reader = new UOBufferReader(data);
            reader.ReadByte(); // 0xBF
            reader.ReadUInt16(); // length
            ushort sub = reader.ReadUInt16();

            switch (sub)
            {
                case 0x08: // Set Player Serial
                    uint serial = reader.ReadUInt32();
                    if (_worldService.Player == null || _worldService.Player.Serial != serial)
                    {
                        var player = new Mobile(serial);
                        _worldService.SetPlayer(player);
                    }
                    break;
            }
        }

        #endregion

        #region Mobile & Item Handlers

        private void HandleMobileIncoming(byte[] data)
        {
            var reader = new UOBufferReader(data);
            reader.ReadByte(); // 0x78
            reader.ReadUInt16(); // length
            uint serial = reader.ReadUInt32();
            ushort body = reader.ReadUInt16();
            ushort x = reader.ReadUInt16();
            ushort y = reader.ReadUInt16();
            sbyte z = reader.ReadSByte();
            byte dir = reader.ReadByte();
            ushort hue = reader.ReadUInt16();
            byte flag = reader.ReadByte();
            byte notoriety = reader.ReadByte();

            var m = _worldService.FindMobile(serial);
            if (m == null)
            {
                m = new Mobile(serial);
                _worldService.AddMobile(m);
            }

            m.Graphic = body;
            m.X = x;
            m.Y = y;
            m.Z = z;
            m.Direction = dir;
            m.Hue = hue;
            m.Notoriety = notoriety;
        }

        private void HandleMobileUpdate(byte[] data)
        {
            var reader = new UOBufferReader(data);
            reader.ReadByte(); // 0x20
            uint serial = reader.ReadUInt32();
            ushort body = reader.ReadUInt16();
            reader.ReadByte(); // status
            ushort hue = reader.ReadUInt16();
            byte flag = reader.ReadByte();
            ushort x = reader.ReadUInt16();
            ushort y = reader.ReadUInt16();
            reader.ReadUInt16(); // 0?
            byte dir = reader.ReadByte();
            sbyte z = reader.ReadSByte();

            var m = _worldService.FindMobile(serial);
            if (m != null)
            {
                m.Graphic = body;
                m.Hue = hue;
                m.X = x;
                m.Y = y;
                m.Z = z;
                m.Direction = dir;
            }
        }

        private void HandleWorldItem(byte[] data)
        {
            var reader = new UOBufferReader(data);
            reader.ReadByte(); // 0x1A
            reader.ReadUInt16(); // length
            uint serial = reader.ReadUInt32();
            ushort itemID = (ushort)(reader.ReadUInt16() & 0x7FFF);

            ushort amount = 1;
            if ((serial & 0x80000000) != 0)
                amount = reader.ReadUInt16();

            ushort x = reader.ReadUInt16();
            ushort y = reader.ReadUInt16();
            sbyte z = reader.ReadSByte();
            ushort hue = 0;
            if ((x & 0x8000) != 0)
                hue = reader.ReadUInt16();

            var item = _worldService.FindItem(serial & 0x7FFFFFFF);
            if (item == null)
            {
                item = new Item(serial & 0x7FFFFFFF);
                _worldService.AddItem(item);
            }

            item.Graphic = itemID;
            item.Amount = amount;
            item.X = x & 0x7FFF;
            item.Y = y;
            item.Z = z;
            item.Hue = hue;
        }

        private void HandleRemoveObject(byte[] data)
        {
            var reader = new UOBufferReader(data);
            reader.ReadByte(); // 0x1D
            uint serial = reader.ReadUInt32();

            _worldService.RemoveMobile(serial);
            _worldService.RemoveItem(serial);
        }

        #endregion

        #region Stats Handlers

        private void HandleHitsUpdate(byte[] data)
        {
            var reader = new UOBufferReader(data);
            reader.ReadByte();
            uint serial = reader.ReadUInt32();
            ushort max = reader.ReadUInt16();
            ushort cur = reader.ReadUInt16();

            var mobile = _worldService.FindMobile(serial);
            if (mobile != null)
            {
                mobile.HitsMax = max;
                mobile.Hits = cur;
                _messenger.Send(new PlayerStatusMessage(StatType.Hits, serial, cur, max));
            }
        }

        private void HandleManaUpdate(byte[] data)
        {
            var reader = new UOBufferReader(data);
            reader.ReadByte();
            uint serial = reader.ReadUInt32();
            ushort max = reader.ReadUInt16();
            ushort cur = reader.ReadUInt16();

            var mobile = _worldService.FindMobile(serial);
            if (mobile != null)
            {
                mobile.ManaMax = max;
                mobile.Mana = cur;
                _messenger.Send(new PlayerStatusMessage(StatType.Mana, serial, cur, max));
            }
        }

        private void HandleStaminaUpdate(byte[] data)
        {
            var reader = new UOBufferReader(data);
            reader.ReadByte();
            uint serial = reader.ReadUInt32();
            ushort max = reader.ReadUInt16();
            ushort cur = reader.ReadUInt16();

            var mobile = _worldService.FindMobile(serial);
            if (mobile != null)
            {
                mobile.StamMax = max;
                mobile.Stam = cur;
                _messenger.Send(new PlayerStatusMessage(StatType.Stamina, serial, cur, max));
            }
        }

        #endregion

        #region Gump Handlers

        private void HandleGump(byte[] data)
        {
            var reader = new UOBufferReader(data);
            reader.ReadByte(); // 0xB0
            reader.ReadUInt16(); // length
            uint serial = reader.ReadUInt32();
            uint gumpId = reader.ReadUInt32();
            int x = (int)reader.ReadUInt32();
            int y = (int)reader.ReadUInt32();

            var gump = new UOGump(serial, gumpId) { X = x, Y = y };

            ushort layoutLen = reader.ReadUInt16();
            if (layoutLen > 0)
                gump.Layout = reader.ReadString(layoutLen);

            ushort linesCount = reader.ReadUInt16();
            for (int i = 0; i < linesCount; i++)
            {
                ushort lineLen = reader.ReadUInt16();
                if (lineLen > 0 && reader.Remaining >= lineLen)
                    gump.AddString(reader.ReadUnicodeString(lineLen >> 1));
            }

            gump.Freeze(); // Congela prima di rendere visibile
            _worldService.SetCurrentGump(gump);
        }

        private void HandleGumpResponse(byte[] data)
        {
            // Quando il client risponde, il gump si chiude (nella maggior parte dei casi)
            _worldService.RemoveGump();
        }

        #endregion

        #region OPL Handlers

        private void HandleOPL(byte[] data)
        {
            var reader = new UOBufferReader(data);
            reader.ReadByte(); // 0xD6
            reader.ReadUInt16(); // length
            reader.ReadUInt16(); // 0x0001
            uint serial = reader.ReadUInt32();
            reader.ReadByte(); // 0
            reader.ReadByte(); // 0
            int hash = reader.ReadInt32();

            var opl = new UOPropertyList(serial) { Hash = hash };

            // Guard: una property OPL richiede almeno 6 byte (4 cliloc + 2 argLen).
            // Max 256 properties per oggetto (nessun item UO ne ha più di ~30).
            const int MinPropertyBytes = 6;
            const int MaxProperties = 256;
            int count = 0;

            while (!reader.AtEnd && reader.Remaining >= MinPropertyBytes && count < MaxProperties)
            {
                int cliloc = reader.ReadInt32();
                if (cliloc == 0) break;

                short argLen = reader.ReadInt16();
                string args = string.Empty;
                if (argLen > 0 && reader.Remaining >= argLen)
                    args = reader.ReadUnicodeString(argLen >> 1);

                opl.AddProperty(new UOPropertyEntry(cliloc, args));
                count++;
            }

            opl.Freeze(); // Congela prima di assegnare all'entità

            var entity = _worldService.FindEntity(serial);
            if (entity != null)
                entity.Properties = opl;
        }

        #endregion

        #region Message & Journal Handlers

        private void HandleAsciiMessage(byte[] data)
        {
            var reader = new UOBufferReader(data);
            reader.ReadByte(); // 0x1C
            reader.ReadUInt16(); // length
            uint serial = reader.ReadUInt32();
            ushort body = reader.ReadUInt16();
            byte type = reader.ReadByte();
            ushort hue = reader.ReadUInt16();
            ushort font = reader.ReadUInt16();
            string name = reader.ReadString(30);
            string text = reader.ReadString();

            _journalService.AddEntry(new JournalEntry(text, name, serial, hue));
        }

        private void HandleUnicodeMessage(byte[] data)
        {
            var reader = new UOBufferReader(data);
            reader.ReadByte(); // 0xAE
            reader.ReadUInt16(); // length
            uint serial = reader.ReadUInt32();
            ushort body = reader.ReadUInt16();
            byte type = reader.ReadByte();
            ushort hue = reader.ReadUInt16();
            ushort font = reader.ReadUInt16();
            string lang = reader.ReadString(4);
            string name = reader.ReadString(30);
            string text = reader.ReadUnicodeString();

            _journalService.AddEntry(new JournalEntry(text, name, serial, hue));
        }

        private void HandleLocalizedMessage(byte[] data)
        {
            var reader = new UOBufferReader(data);
            reader.ReadByte(); // 0xAF
            reader.ReadUInt16(); // length
            uint serial = reader.ReadUInt32();
            ushort body = reader.ReadUInt16();
            byte type = reader.ReadByte();
            ushort hue = reader.ReadUInt16();
            ushort font = reader.ReadUInt16();
            int cliloc = reader.ReadInt32();
            string name = reader.ReadString(30);
            string args = reader.ReadUnicodeString();

            string text = _languageService.ClilocFormat(cliloc, args);
            _journalService.AddEntry(new JournalEntry(text, name, serial, hue));
        }

        #endregion
    }
}
