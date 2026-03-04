using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TMRazorImproved.Shared.Models
{
    public partial class TradeSession : ObservableObject
    {
        public uint Serial { get; }
        
        [ObservableProperty]
        private string _targetName = "Unknown";
        
        [ObservableProperty]
        private bool _myAccepted;
        
        [ObservableProperty]
        private bool _theirAccepted;
        
        [ObservableProperty]
        private uint _myGold;
        
        [ObservableProperty]
        private uint _myPlatinum;
        
        [ObservableProperty]
        private uint _theirGold;
        
        [ObservableProperty]
        private uint _theirPlatinum;
        
        [ObservableProperty]
        private uint _goldMax;
        
        [ObservableProperty]
        private uint _platinumMax;

        public DateTime StartTime { get; }

        public ObservableCollection<TradeItem> MyItems { get; } = new();
        public ObservableCollection<TradeItem> TheirItems { get; } = new();

        public TradeSession(uint serial)
        {
            Serial = serial;
            StartTime = DateTime.Now;
        }
    }

    public record TradeItem(uint Serial, int Amount, ushort Graphic, ushort Hue, string Name);
}
