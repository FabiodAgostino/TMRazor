using System.Collections.Generic;
using TMRazorImproved.Shared.Enums;

namespace TMRazorImproved.Shared.Utilities
{
    public static class PacketNames
    {
        private static readonly Dictionary<byte, string> _s2cNames = new()
        {
            { 0x0B, "Damage" },
            { 0x11, "Mobile Status" },
            { 0x16, "SA Mobile Status" },
            { 0x17, "New Mobile Status" },
            { 0x1A, "World Item" },
            { 0x1B, "Login Confirm" },
            { 0x1C, "ASCII Speech" },
            { 0x1D, "Remove Object" },
            { 0x20, "Mobile Update" },
            { 0x21, "Movement Reject" },
            { 0x22, "Movement Ack" },
            { 0x24, "Container Content" },
            { 0x27, "Lift Reject" },
            { 0x2D, "Mobile Stat Info" },
            { 0x3A, "Skills List" },
            { 0x3C, "Container Content (Bulk)" },
            { 0x4E, "Personal Light" },
            { 0x4F, "Global Light" },
            { 0x54, "Sound/Spell Effect" },
            { 0x6C, "Target Cursor" },
            { 0x6E, "Character Animation" },
            { 0x6F, "Trade Request" },
            { 0x70, "Graphical Effect" },
            { 0x72, "Set War Mode" },
            { 0x73, "Ping Response" },
            { 0x77, "Update Mobile" },
            { 0x78, "Mobile Incoming" },
            { 0x7C, "Menu/Dialog" },
            { 0x88, "Open Paperdoll" },
            { 0x90, "Map Details" },
            { 0xA1, "Hits Update" },
            { 0xA2, "Mana Update" },
            { 0xA3, "Stam Update" },
            { 0xAB, "String Query" },
            { 0xAE, "Unicode Speech" },
            { 0xAF, "Death Animation" },
            { 0xB0, "Gump Message" },
            { 0xB9, "Supported Features" },
            { 0xBA, "Quest Arrow" },
            { 0xBC, "Season Change" },
            { 0xBF, "Extended Packet" },
            { 0xC1, "Cliloc Message" },
            { 0xC8, "Update Range" },
            { 0xD6, "Mega Cliloc" },
            { 0xDD, "Compressed Gump" },
            { 0xDF, "Buff/Debuff" },
            { 0xF3, "SA World Item" },
            { 0xF5, "New Map Details" }
        };

        private static readonly Dictionary<byte, string> _c2sNames = new()
        {
            { 0x00, "Create Character" },
            { 0x01, "Disconnect" },
            { 0x02, "Movement Request" },
            { 0x05, "Attack Request" },
            { 0x06, "Double Click" },
            { 0x07, "Lift Request" },
            { 0x08, "Drop Request" },
            { 0x09, "Single Click" },
            { 0x12, "Action/Text Command" },
            { 0x13, "Equip Request" },
            { 0x22, "Resync Request" },
            { 0x34, "Get Player Status" },
            { 0x3A, "Set Skill Lock" },
            { 0x5D, "Play Character" },
            { 0x6C, "Target Response" },
            { 0x6F, "Trade Response" },
            { 0x72, "Set War Mode" },
            { 0x73, "Ping Request" },
            { 0x75, "Rename Mobile" },
            { 0x7D, "Menu Response" },
            { 0x80, "Login Request" },
            { 0x91, "Login Request (New)" },
            { 0x95, "Hue Response" },
            { 0xA0, "Server Select" },
            { 0xAD, "Unicode Speech" },
            { 0xB1, "Gump Response" },
            { 0xC2, "Unicode Prompt" },
            { 0xD7, "Encoded Packet" },
            { 0xF8, "Create Character (New)" }
        };

        public static string GetName(PacketPath path, byte id)
        {
            var dict = path == PacketPath.ServerToClient ? _s2cNames : _c2sNames;
            if (dict.TryGetValue(id, out var name))
                return name;
            
            return "Unknown";
        }
    }
}
