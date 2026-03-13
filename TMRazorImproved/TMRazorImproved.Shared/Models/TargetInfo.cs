using System;

namespace TMRazorImproved.Shared.Models
{
    public struct TargetInfo
    {
        public uint Serial { get; set; }
        public ushort X { get; set; }
        public ushort Y { get; set; }
        public sbyte Z { get; set; }
        public ushort Graphic { get; set; }
    }
}
