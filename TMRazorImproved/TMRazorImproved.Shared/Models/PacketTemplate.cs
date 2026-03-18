using System;
using System.Collections.Generic;

namespace TMRazorImproved.Shared.Models
{
    public class PacketTemplate
    {
        public int Version { get; set; } = 1;
        public int PacketID { get; set; } = -1;
        public string Name { get; set; } = string.Empty;
        public bool ShowHexDump { get; set; } = false;
        public bool DynamicLength { get; set; } = false;
        public List<FieldTemplate> Fields { get; set; } = new();
    }

    public class FieldTemplate
    {
        public string Name { get; set; } = string.Empty;
        public int Length { get; set; } = -1;
        public string Type { get; set; } = FieldType.DUMP;
        public List<FieldTemplate> Fields { get; set; } = new();
        public PacketTemplate? Subpacket { get; set; }
    }

    public static class FieldType
    {
        public const string PACKETID = "packetID";
        public const string SERIAL = "serial";
        public const string MODELID = "modelID";
        public const string BOOL = "bool";
        public const string INT = "int";
        public const string UINT = "uint";
        public const string HEX = "hex";
        public const string STRING = "string";
        public const string ASCII = "ascii";
        public const string DUMP = "dump";
        public const string OFFSET = "offset";
        public const string BIT = "bit";
        public const string CLILOC = "cliloc";
        public const string TEXT = "text";
        public const string UNICODE = "unicode";
        public const string GUMP = "gump";
    }
}
