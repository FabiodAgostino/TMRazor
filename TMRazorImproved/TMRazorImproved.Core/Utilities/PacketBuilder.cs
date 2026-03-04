using System;
using System.Buffers.Binary;
using System.Text;

namespace TMRazorImproved.Core.Utilities
{
    /// <summary>
    /// Costruttore centralizzato di pacchetti UO client→server.
    /// Elimina la duplicazione del codice di packet building presente in 6+ servizi
    /// (AutoLootService, ScavengerService, DressService, RestockService, OrganizerService, ItemsApi).
    /// </summary>
    public static class PacketBuilder
    {
        // -------------------------------------------------------------------------
        // Item Movement (Lift + Drop)
        // -------------------------------------------------------------------------

        /// <summary>Lift Request 0x07: solleva un item dal mondo o da un container.</summary>
        public static byte[] LiftItem(uint serial, ushort amount = 1)
        {
            byte[] pkt = new byte[7];
            pkt[0] = 0x07;
            BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(1), serial);
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(5), amount);
            return pkt;
        }

        /// <summary>
        /// Drop Request 0x08 (SA format, 15 byte): deposita un item in un container.
        /// x/y/z = 0xFFFF/0xFFFF/0 = posizione casuale all'interno del container.
        /// FIX P1-04: layout esplicito SA — byte[10]=grid (0=slot casuale/nessuna preferenza).
        /// Layout: cmd(1) serial(4) x(2) y(2) z(1) grid(1) container(4).
        /// </summary>
        public static byte[] DropToContainer(uint serial, uint containerSerial, byte grid = 0)
        {
            byte[] pkt = new byte[15];
            pkt[0] = 0x08;
            BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(1), serial);
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(5), 0xFFFF); // X (random)
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(7), 0xFFFF); // Y (random)
            pkt[9]  = 0;                                                   // Z
            pkt[10] = grid;                                                // Grid slot (SA)
            BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(11), containerSerial);
            return pkt;
        }

        // -------------------------------------------------------------------------
        // Equipment
        // -------------------------------------------------------------------------

        /// <summary>WearItem Request 0x13: equipaggia un item su un layer del mobile.</summary>
        public static byte[] WearItem(uint itemSerial, byte layer, uint mobileSerial)
        {
            byte[] pkt = new byte[10];
            pkt[0] = 0x13;
            BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(1), itemSerial);
            pkt[5] = layer;
            BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(6), mobileSerial);
            return pkt;
        }

        // -------------------------------------------------------------------------
        // Action Commands
        // -------------------------------------------------------------------------

        /// <summary>Double Click 0x06: doppio click su un serial (usa oggetto, apre container, ecc.).</summary>
        public static byte[] DoubleClick(uint serial)
        {
            byte[] pkt = new byte[5];
            pkt[0] = 0x06;
            BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(1), serial);
            return pkt;
        }

        /// <summary>Single Click 0x09: click singolo su un serial (mostra tooltip).</summary>
        public static byte[] SingleClick(uint serial)
        {
            byte[] pkt = new byte[5];
            pkt[0] = 0x09;
            BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(1), serial);
            return pkt;
        }

        /// <summary>Attack Request 0x05: attacca un mobile per serial.</summary>
        public static byte[] Attack(uint serial)
        {
            byte[] pkt = new byte[5];
            pkt[0] = 0x05;
            BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(1), serial);
            return pkt;
        }

        // -------------------------------------------------------------------------
        // Spells / Skills
        // -------------------------------------------------------------------------

        /// <summary>Cast Spell via 0x12 (type 0x56): lancia un incantesimo per ID numerico.</summary>
        public static byte[] CastSpell(int spellId)
        {
            byte[] spellBytes = Encoding.ASCII.GetBytes(spellId.ToString());
            byte[] pkt = new byte[4 + spellBytes.Length + 1];
            pkt[0] = 0x12;
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(1), (ushort)pkt.Length);
            pkt[3] = 0x56; // CastSpell type (post-Mondain's Legacy, standard)
            Array.Copy(spellBytes, 0, pkt, 4, spellBytes.Length);
            return pkt;
        }

        /// <summary>Use Skill via 0x12 (type 0x24): usa una skill per ID numerico.</summary>
        public static byte[] UseSkill(int skillId)
        {
            byte[] skillBytes = Encoding.ASCII.GetBytes($"{skillId} 0");
            byte[] pkt = new byte[4 + skillBytes.Length + 1];
            pkt[0] = 0x12;
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(1), (ushort)pkt.Length);
            pkt[3] = 0x24; // UseSkill type
            Array.Copy(skillBytes, 0, pkt, 4, skillBytes.Length);
            return pkt;
        }

        /// <summary>Set Skill Lock via 0x3A: cambia lo stato del lock (Up/Down/Locked) di una skill.</summary>
        public static byte[] SetSkillLock(int skillId, byte lockType)
        {
            // Packet 0x3A single skill update (client->server lock change)
            // Format: cmd(1) len(2) type(1) skillID(2) lock(1)
            byte[] pkt = new byte[7];
            pkt[0] = 0x3A;
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(1), 7);
            pkt[3] = 0xDF; // Single skill lock change
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(4), (ushort)skillId);
            pkt[6] = lockType;
            return pkt;
        }

        // -------------------------------------------------------------------------
        // Targeting
        // -------------------------------------------------------------------------

        /// <summary>
        /// Target Response C2S 0x6C: risponde a una richiesta di target puntando su un oggetto/mobile.
        /// </summary>
        public static byte[] TargetObject(uint serial, uint cursorId = 0)
        {
            byte[] pkt = new byte[19];
            pkt[0] = 0x6C;
            pkt[1] = 0x01; // Target type: Object
            BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(2), cursorId);
            pkt[6] = 0x00; // Action: 0 = select target
            BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(7), serial);
            return pkt;
        }

        // -------------------------------------------------------------------------
        // Speech
        // -------------------------------------------------------------------------

        /// <summary>
        /// Unicode Speech 0xAD: invia testo parlato visibile a schermo.
        /// type=0 = Normal; hue=0x0034 = default; font=0x0003 = default.
        /// </summary>
        public static byte[] UnicodeSpeech(string text, byte type = 0x00, ushort hue = 0x0034, ushort font = 0x0003, string lang = "ENU")
        {
            byte[] msgBytes = Encoding.BigEndianUnicode.GetBytes(text);
            int pktLen = 1 + 2 + 1 + 2 + 2 + 4 + msgBytes.Length + 2;
            byte[] pkt = new byte[pktLen];
            pkt[0] = 0xAD;
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(1), (ushort)pktLen);
            pkt[3] = type;
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(4), hue);
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(6), font);
            // Language: 4 ASCII bytes (es. "ENU\0")
            byte[] langBytes = Encoding.ASCII.GetBytes(lang.PadRight(4, '\0').Substring(0, 4));
            Array.Copy(langBytes, 0, pkt, 8, 4);
            Array.Copy(msgBytes, 0, pkt, 12, msgBytes.Length);
            // Null terminator (2 bytes): already 0 from new byte[pktLen]
            return pkt;
        }

        // -------------------------------------------------------------------------
        // Gump
        // -------------------------------------------------------------------------

        /// <summary>
        /// Gump Menu Select 0xB1: risponde a un gump chiudendolo o premendo un pulsante.
        /// </summary>
        public static byte[] RespondGump(uint gumpSerial, uint gumpTypeId, int buttonId)
        {
            // Struttura: cmd(1) len(2) serial(4) gumpId(4) buttonId(4) switchCount(4) textCount(4)
            const ushort pktLen = 23;
            byte[] pkt = new byte[pktLen];
            pkt[0] = 0xB1;
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(1), pktLen);
            BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(3), gumpSerial);
            BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(7), gumpTypeId);
            BinaryPrimitives.WriteInt32BigEndian(pkt.AsSpan(11), buttonId);
            // switchCount = 0, textCount = 0 (already 0 from new byte[pktLen])
            return pkt;
        }
    }
}
