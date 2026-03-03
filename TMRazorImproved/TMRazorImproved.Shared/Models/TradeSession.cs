using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TMRazorImproved.Shared.Models
{
    public class TradeSession
    {
        public uint Serial { get; }
        public uint TargetSerial { get; }
        public string TargetName { get; set; } = "Unknown";
        public bool MyAccepted { get; set; }
        public bool TheirAccepted { get; set; }
        public DateTime StartTime { get; }

        public List<TradeItem> MyItems { get; } = new();
        public List<TradeItem> TheirItems { get; } = new();

        public TradeSession(uint serial, uint targetSerial)
        {
            Serial = serial;
            TargetSerial = targetSerial;
            StartTime = DateTime.Now;
        }
    }

    public record TradeItem(uint Serial, int Amount, ushort Graphic, ushort Hue, string Name);
}
