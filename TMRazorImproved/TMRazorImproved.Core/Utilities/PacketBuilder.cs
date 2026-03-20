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
        // Extended Commands (0xBF)
        // -------------------------------------------------------------------------

        /// <summary>TargetByResource 0xBF sub 0x30: usato per minare/tagliare alberi su resource map.</summary>
        public static byte[] TargetByResource(uint serial, int resourceType)
        {
            byte[] pkt = new byte[11];
            pkt[0] = 0xBF;
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(1), 11); // Length
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(3), 0x30); // SubCommand TargetByResource
            BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(5), serial);
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(9), (ushort)resourceType);
            return pkt;
        }

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
        /// </summary>
        public static byte[] DropToContainer(uint serial, uint containerSerial, byte grid = 0)
        {
            return DropToContainer(serial, containerSerial, 0xFFFF, 0xFFFF, 0, grid);
        }

        /// <summary>
        /// Drop Request 0x08: deposita un item in un container in una posizione specifica.
        /// </summary>
        public static byte[] DropToContainer(uint serial, uint containerSerial, ushort x, ushort y, byte z = 0, byte grid = 0)
        {
            byte[] pkt = new byte[15];
            pkt[0] = 0x08;
            BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(1), serial);
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(5), x);
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(7), y);
            pkt[9] = z;
            pkt[10] = grid;
            BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(11), containerSerial);
            return pkt;
        }

        /// <summary>
        /// Drop Request 0x08: deposita un item nel mondo (a terra).
        /// </summary>
        public static byte[] DropToWorld(uint serial, ushort x, ushort y, short z)
        {
            return DropToContainer(serial, 0, x, y, (byte)z, 0);
        }

        /// <summary>
        /// Remove Object 0x1D S->C: istruisce il client a rimuovere un oggetto dalla vista.
        /// </summary>
        public static byte[] RemoveObject(uint serial)
        {
            byte[] pkt = new byte[5];
            pkt[0] = 0x1D;
            BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(1), serial);
            return pkt;
        }

        /// <summary>
        /// Close Gump / Container 0xBF sub 0x01: chiude un contenitore o gump generico.
        /// </summary>
        public static byte[] CloseContainer(uint serial)
        {
            byte[] pkt = new byte[9];
            pkt[0] = 0xBF;
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(1), 9);
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(3), 0x01);
            BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(5), serial);
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

        /// <summary>
        /// EquipItemMacro 0xEC — UO3D batch equip: equipaggia più item in un singolo pacchetto.
        /// Format: cmd(1) len(2) count(1) serial0(4) ... serialN(4)
        /// </summary>
        public static byte[] EquipItemMacro(IList<uint> serials)
        {
            int pktLen = 1 + 2 + 1 + serials.Count * 4;
            byte[] pkt = new byte[pktLen];
            pkt[0] = 0xEC;
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(1), (ushort)pktLen);
            pkt[3] = (byte)serials.Count;
            for (int i = 0; i < serials.Count; i++)
                BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(4 + i * 4), serials[i]);
            return pkt;
        }

        /// <summary>
        /// UnEquipItemMacro 0xED — UO3D batch unequip: rimuove item da più layer in un singolo pacchetto.
        /// Format: cmd(1) len(2) count(1) layer0(2) ... layerN(2)
        /// </summary>
        public static byte[] UnEquipItemMacro(IList<byte> layers)
        {
            int pktLen = 1 + 2 + 1 + layers.Count * 2;
            byte[] pkt = new byte[pktLen];
            pkt[0] = 0xED;
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(1), (ushort)pktLen);
            pkt[3] = (byte)layers.Count;
            for (int i = 0; i < layers.Count; i++)
                BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(4 + i * 2), layers[i]);
            return pkt;
        }

        /// <summary>
        /// Request Profile 0xB8: richiede il profilo di un mobile (usato anche per forzare aggiornamento Fame/Karma).
        /// </summary>
        public static byte[] RequestProfile(uint serial)
        {
            byte[] pkt = new byte[6];
            pkt[0] = 0xB8;
            pkt[1] = 0x00; // Mode: 0 = request
            BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(2), serial);
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

        /// <summary>
        /// SkillUpdate S→C 0x3A type=0xDF: fake server packet per aggiornare la UI di ClassicUO
        /// dopo un cambio di lock lato client. Stessa struttura del pacchetto server reale.
        /// Layout: cmd(1) len(2) type(1=0xDF) skillId(2) value(2) base(2) lock(1) cap(2) = 13 bytes.
        /// value/base/cap sono in fixed-point ×10 (Big-Endian short).
        /// </summary>
        public static byte[] SkillUpdate(int skillId, double value, double baseValue, double cap, byte lockType)
        {
            byte[] pkt = new byte[13];
            pkt[0] = 0x3A;
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(1), 13);
            pkt[3] = 0xDF; // type: single skill update
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(4),  (ushort)skillId);
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(6),  (ushort)Math.Round(value    * 10));
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(8),  (ushort)Math.Round(baseValue * 10));
            pkt[10] = lockType;
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(11), (ushort)Math.Round(cap      * 10));
            return pkt;
        }

        /// <summary>Set Skill Lock via 0x3A: cambia lo stato del lock (Up/Down/Locked) di una skill.</summary>
        public static byte[] SetSkillLock(int skillId, byte lockType)
        {
            // Packet 0x3A client->server: cmd(1) len(2) skillID(2) lock(1) = 6 bytes
            // NON ha un type byte — il type byte esiste solo nel pacchetto server->client
            byte[] pkt = new byte[6];
            pkt[0] = 0x3A;
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(1), 6);
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(3), (ushort)skillId);
            pkt[5] = lockType;
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

        public static byte[] OverheadUnicodeSpeech(string text, uint serial, ushort body, byte type = 0x00, ushort hue = 0x0034, ushort font = 0x0003, string lang = "ENU", string name = "System")
        {
            byte[] msgBytes = Encoding.BigEndianUnicode.GetBytes(text + "\0");
            int pktLen = 1 + 2 + 4 + 2 + 1 + 2 + 2 + 4 + 30 + msgBytes.Length;
            byte[] pkt = new byte[pktLen];
            pkt[0] = 0xAE;
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(1), (ushort)pktLen);
            BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(3), serial);
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(7), body);
            pkt[9] = type;
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(10), hue);
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(12), font);
            byte[] langBytes = Encoding.ASCII.GetBytes(lang.PadRight(4, '\0').Substring(0, 4));
            Array.Copy(langBytes, 0, pkt, 14, 4);
            byte[] nameBytes = Encoding.ASCII.GetBytes(name.PadRight(30, '\0').Substring(0, 30));
            Array.Copy(nameBytes, 0, pkt, 18, 30);
            Array.Copy(msgBytes, 0, pkt, 48, msgBytes.Length);
            return pkt;
        }

        // -------------------------------------------------------------------------
        // Old Menu (0x7D) — pre-context-menu UO menus
        // -------------------------------------------------------------------------

        /// <summary>
        /// Menu Response C→S 0x7D: risponde a un menu classico UO (0x7C S→C).
        /// <paramref name="index"/> è 1-based (0 = chiude il menu).
        /// Layout: cmd(1) serial(4) menuId(2) index(2) graphic(2) hue(2) = 13 bytes.
        /// </summary>
        public static byte[] MenuResponse(uint serial, ushort menuId, ushort index, ushort graphic, ushort hue)
        {
            byte[] pkt = new byte[13];
            pkt[0] = 0x7D;
            BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(1), serial);
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(5), menuId);
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(7), index);
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(9), graphic);
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(11), hue);
            return pkt;
        }

        // -------------------------------------------------------------------------
        // Query String (0xAC)
        // -------------------------------------------------------------------------

        /// <summary>
        /// String Query Response C→S 0xAC: risponde a una richiesta di input testuale (0xAB S→C).
        /// </summary>
        public static byte[] StringQueryResponse(uint serial, byte type, byte index, bool ok, string response)
        {
            response ??= string.Empty;
            byte[] respBytes = Encoding.ASCII.GetBytes(response);
            
            // Layout: cmd(1) len(2) serial(4) type(1) index(1) ok(1) respLen+1(2) respNull(N+1)
            int pktLen = 1 + 2 + 4 + 1 + 1 + 1 + 2 + respBytes.Length + 1;
            byte[] pkt = new byte[pktLen];
            
            pkt[0] = 0xAC;
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(1), (ushort)pktLen);
            BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(3), serial);
            pkt[7] = type;
            pkt[8] = index;
            pkt[9] = (byte)(ok ? 1 : 0);
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(10), (ushort)(respBytes.Length + 1));
            Array.Copy(respBytes, 0, pkt, 12, respBytes.Length);
            // Null terminator at the end is already 0
            
            return pkt;
        }

        /// <summary>
        /// Unicode Prompt Response C→S 0xC2: risponde a un prompt Unicode (0xC2 S→C).
        /// </summary>
        public static byte[] UnicodePromptResponse(uint serial, uint promptId, uint type, string text, string lang = "ENU")
        {
            text ??= string.Empty;
            byte[] textBytes = Encoding.Unicode.GetBytes(text);
            
            // Layout: cmd(1) len(2) serial(4) promptId(4) type(4) lang(4) text(N unicode null-term)
            int pktLen = 1 + 2 + 4 + 4 + 4 + 4 + textBytes.Length + 2;
            byte[] pkt = new byte[pktLen];
            
            pkt[0] = 0xC2;
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(1), (ushort)pktLen);
            BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(3), serial);
            BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(7), promptId);
            BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(11), type);
            
            byte[] langBytes = Encoding.ASCII.GetBytes(lang.PadRight(4, '\0').Substring(0, 4));
            Array.Copy(langBytes, 0, pkt, 15, 4);
            
            Array.Copy(textBytes, 0, pkt, 19, textBytes.Length);
            // Null terminator (2 bytes) already 0
            
            return pkt;
        }

        /// <summary>
        /// ASCII Prompt Response C→S 0x9A: risponde a un prompt ASCII (0x9A S→C).
        /// </summary>
        public static byte[] PromptResponse(uint serial, uint promptId, uint type, string text)
        {
            text ??= string.Empty;
            byte[] textBytes = Encoding.ASCII.GetBytes(text);
            
            // Layout: cmd(1) len(2) serial(4) promptId(4) type(4) text(N ascii null-term)
            int pktLen = 1 + 2 + 4 + 4 + 4 + textBytes.Length + 1;
            byte[] pkt = new byte[pktLen];
            
            pkt[0] = 0x9A;
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(1), (ushort)pktLen);
            BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(3), serial);
            BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(7), promptId);
            BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(11), type);
            
            Array.Copy(textBytes, 0, pkt, 15, textBytes.Length);
            // Null terminator already 0
            
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

        /// <summary>
        /// Gump Menu Select 0xB1 con switches e text entries.
        /// switches: array di buttonId delle checkbox spuntate.
        /// textEntries: array di (textIndex, textValue) per i campi di input.
        /// </summary>
        public static byte[] RespondGump(uint gumpSerial, uint gumpTypeId, int buttonId,
            int[]? switches, (int index, string text)[]? textEntries)
        {
            switches     ??= Array.Empty<int>();
            textEntries  ??= Array.Empty<(int, string)>();

            // Calcola dimensione: header(19) + switchCount(4) + switches + textCount(4) + text entries
            int switchLen = 4 + switches.Length * 4;
            int textEntryBytes = 0;
            foreach (var (_, t) in textEntries)
                textEntryBytes += 4 + t.Length * 2; // index(2) + len(2) + unicode chars
            int pktLen = 19 + switchLen + textEntryBytes;

            var pkt = new byte[pktLen];
            pkt[0] = 0xB1;
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(1), (ushort)pktLen);
            BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(3), gumpSerial);
            BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(7), gumpTypeId);
            BinaryPrimitives.WriteInt32BigEndian(pkt.AsSpan(11), buttonId);

            int pos = 15;
            BinaryPrimitives.WriteInt32BigEndian(pkt.AsSpan(pos), switches.Length); pos += 4;
            foreach (int sw in switches)
            { BinaryPrimitives.WriteInt32BigEndian(pkt.AsSpan(pos), sw); pos += 4; }

            BinaryPrimitives.WriteInt32BigEndian(pkt.AsSpan(pos), textEntries.Length); pos += 4;
            foreach (var (idx, text) in textEntries)
            {
                BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(pos), (ushort)idx); pos += 2;
                BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(pos), (ushort)text.Length); pos += 2;
                foreach (char c in text)
                {
                    pkt[pos++] = (byte)(c >> 8);
                    pkt[pos++] = (byte)c;
                }
            }
            return pkt;
        }

        // -------------------------------------------------------------------------
        // Properties
        // -------------------------------------------------------------------------

        /// <summary>Richiede le proprietà (Tooltip) di un oggetto (0xBF sub 0x10).</summary>
        public static byte[] QueryProperties(uint serial)
        {
            byte[] pkt = new byte[9];
            pkt[0] = 0xBF;
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(1), 9);
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(3), 0x10);
            BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(5), serial);
            return pkt;
        }

        // -------------------------------------------------------------------------
        // World Items (Server -> Client simulation)
        // -------------------------------------------------------------------------

        /// <summary>
        /// World Item 0x1A: crea un pacchetto per mostrare un oggetto a terra.
        /// Versione non bit-packed per semplicità (tutti i campi opzionali inclusi).
        /// Layout: cmd(1) len(2) serial|0x80000000(4) itemId|0x8000(2) amount(2) x|0x8000(2) y|0x8000|0x4000(2) dir(1) z(1) hue(2) flags(1)
        /// Total len: 1+2+4+2+2+2+2+1+1+2+1 = 20 bytes.
        /// </summary>
        public static byte[] WorldItem(uint serial, ushort itemId, ushort amount, ushort x, ushort y, sbyte z, ushort hue, byte flags)
        {
            byte[] pkt = new byte[20];
            pkt[0] = 0x1A;
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(1), 20);
            BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(3), serial | 0x80000000);
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(7), (ushort)(itemId | 0x8000));
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(9), amount);
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(11), (ushort)(x | 0x8000));
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(13), (ushort)(y | 0xC000));
            pkt[15] = 0; // dir
            pkt[16] = (byte)z;
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(17), hue);
            pkt[19] = flags;
            return pkt;
        }

        /// <summary>
        /// Mobile Update 0x20: aggiorna le informazioni di un mobile (corpo, hue, flags).
        /// </summary>
        public static byte[] MobileUpdate(uint serial, ushort body, ushort hue, byte flags, ushort x, ushort y, sbyte z, byte dir)
        {
            byte[] pkt = new byte[15];
            pkt[0] = 0x20;
            BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(1), serial);
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(5), body);
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(7), hue);
            pkt[9] = flags;
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(10), x);
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(12), y);
            pkt[14] = (byte)z; // Note: layout variant, usually includes dir at end but 0x20 is simple
            return pkt;
        }

        // -------------------------------------------------------------------------
        // PathFind (0x38) — pathfinding client-side
        // -------------------------------------------------------------------------

        /// <summary>
        /// PathFind 0x38 (S→C): informa il client di navigare verso le coordinate indicate.
        /// Layout: cmd(1) x(2) y(2) z(2) = 7 bytes.
        /// </summary>
        public static byte[] PathFind(int x, int y, int z)
        {
            byte[] pkt = new byte[7];
            pkt[0] = 0x38;
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(1), (ushort)x);
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(3), (ushort)y);
            BinaryPrimitives.WriteInt16BigEndian(pkt.AsSpan(5), (short)z);
            return pkt;
        }
    }
}
