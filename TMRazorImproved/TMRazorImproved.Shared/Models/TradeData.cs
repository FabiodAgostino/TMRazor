using System;

namespace TMRazorImproved.Shared.Models
{
    public class TradeData
    {
        public uint TradeId { get; set; }
        public uint SerialTrader { get; set; }
        public uint ContainerMe { get; set; }
        public uint ContainerTrader { get; set; }
        public string NameTrader { get; set; } = string.Empty;
        public bool AcceptMe { get; set; }
        public bool AcceptTrader { get; set; }
        public uint GoldMe { get; set; }
        public uint PlatinumMe { get; set; }
        public uint GoldTrader { get; set; }
        public uint PlatinumTrader { get; set; }
        public uint GoldMax { get; set; }
        public uint PlatinumMax { get; set; }
    }
}
