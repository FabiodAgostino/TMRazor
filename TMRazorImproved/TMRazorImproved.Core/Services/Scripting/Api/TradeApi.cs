using System.Collections.Generic;
using System.Linq;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.Core.Services.Scripting.Api
{
    /// <summary>
    /// Exposes secure-trade management to scripts as the <c>Trade</c> global variable.
    /// Mirrors <c>RazorEnhanced.Trade</c> from the legacy codebase.
    /// </summary>
    public class TradeApi
    {
        private readonly ISecureTradeService _trade;
        private readonly ScriptCancellationController _cancel;

        public TradeApi(ISecureTradeService trade, ScriptCancellationController cancel)
        {
            _trade = trade;
            _cancel = cancel;
        }

        /// <summary>Returns a list of all active trade sessions as <see cref="TradeData"/> objects.</summary>
        public virtual List<TradeData> TradeList()
        {
            _cancel.ThrowIfCancelled();
            return _trade.ActiveTrades
                .Select(serial => _trade.GetTrade(serial))
                .Where(t => t != null)
                .Select(t => t!)
                .ToList();
        }

        // Accept overloads ------------------------------------------------------

        /// <summary>
        /// Sets the accept checkbox on the specified trade.
        /// </summary>
        /// <param name="tradeId">Serial of the trade session.</param>
        /// <param name="accept"><c>true</c> to accept, <c>false</c> to uncheck.</param>
        /// <returns><c>true</c> if the trade was found and the packet sent.</returns>
        public virtual bool Accept(uint tradeId, bool accept = true)
        {
            _cancel.ThrowIfCancelled();
            if (_trade.GetTrade(tradeId) == null) return false;
            if (accept)
                _trade.AcceptTrade(tradeId);
            else
                _trade.CancelTrade(tradeId);   // uncheck = cancel in UO protocol
            return true;
        }

        /// <summary>Accepts/rejects the first active trade (convenience overload).</summary>
        public virtual bool Accept(bool accept = true)
        {
            _cancel.ThrowIfCancelled();
            var first = _trade.ActiveTrades.FirstOrDefault();
            if (first == 0) return false;
            return Accept(first, accept);
        }

        // Cancel overloads ------------------------------------------------------

        /// <summary>Cancels (closes) the specified trade session.</summary>
        public virtual bool Cancel(uint tradeId)
        {
            _cancel.ThrowIfCancelled();
            if (_trade.GetTrade(tradeId) == null) return false;
            _trade.CancelTrade(tradeId);
            return true;
        }

        /// <summary>Cancels the first active trade (convenience overload).</summary>
        public virtual bool Cancel()
        {
            _cancel.ThrowIfCancelled();
            var first = _trade.ActiveTrades.FirstOrDefault();
            if (first == 0) return false;
            return Cancel(first);
        }

        // Offer overloads -------------------------------------------------------

        /// <summary>
        /// Sets the gold/platinum offer on the specified trade.
        /// Note: the client-side display may not update — this sends the server packet directly.
        /// </summary>
        public virtual bool Offer(uint tradeId, int gold, int platinum)
        {
            _cancel.ThrowIfCancelled();
            if (_trade.GetTrade(tradeId) == null) return false;
            _trade.Offer(tradeId, (uint)gold, (uint)platinum);
            return true;
        }

        /// <summary>Sets the gold/platinum offer on the first active trade (convenience overload).</summary>
        public virtual bool Offer(int gold, int platinum)
        {
            _cancel.ThrowIfCancelled();
            var first = _trade.ActiveTrades.FirstOrDefault();
            if (first == 0) return false;
            return Offer(first, gold, platinum);
        }

        // int-serial overloads — RazorEnhanced compatibility (TASK-FR-012) ------
        public virtual bool Accept(int tradeId, bool accept = true) => Accept((uint)tradeId, accept);
        public virtual bool Cancel(int tradeId) => Cancel((uint)tradeId);
        public virtual bool Offer(int tradeId, int gold, int platinum) => Offer((uint)tradeId, gold, platinum);
    }
}
