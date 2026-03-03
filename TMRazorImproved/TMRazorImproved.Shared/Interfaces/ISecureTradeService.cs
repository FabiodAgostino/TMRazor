using System;
using System.Collections.Generic;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.Shared.Interfaces
{
    public interface ISecureTradeService
    {
        IReadOnlyList<uint> ActiveTrades { get; }
        
        event Action<uint>? TradeStarted;
        event Action<uint>? TradeUpdated;
        event Action<uint>? TradeClosed;

        void StartTrade(uint targetSerial);
        void AcceptTrade(uint tradeSerial);
        void CancelTrade(uint tradeSerial);
    }
}
