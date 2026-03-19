using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using TMRazorImproved.Shared.Interfaces;
using CommunityToolkit.Mvvm.Messaging;
using TMRazorImproved.Shared.Messages;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.Core.Services
{
    public class SecureTradeService : ISecureTradeService, IRecipient<TradeMessage>
    {
        private readonly IPacketService _packetService;
        private readonly ILogger<SecureTradeService> _logger;
        // BUG-NEW-02 FIX: Dictionary non thread-safe — il packet thread (Receive) e il thread UI
        // (CancelTrade, GetTrade, ActiveTrades) accedono concorrentemente. Usiamo _tradesLock.
        private readonly Dictionary<uint, TradeData> _trades = new();
        private readonly object _tradesLock = new();

        public event Action<uint>? TradeStarted;
        public event Action<uint>? TradeUpdated;
        public event Action<uint>? TradeClosed;

        public IReadOnlyList<uint> ActiveTrades
        {
            get { lock (_tradesLock) return _trades.Keys.ToList().AsReadOnly(); }
        }

        public TradeData? GetTrade(uint tradeSerial)
        {
            lock (_tradesLock) return _trades.GetValueOrDefault(tradeSerial);
        }

        public SecureTradeService(IPacketService packetService, IMessenger messenger, ILogger<SecureTradeService> logger)
        {
            _packetService = packetService;
            _logger = logger;
            messenger.RegisterAll(this);
        }

        public void StartTrade(uint targetSerial)
        {
            _logger.LogInformation("Starting secure trade with serial {Serial}", targetSerial);
            // Non si manda un 0x6F per avviare il trade, si apre droppando un item su di un player.
        }

        public void AcceptTrade(uint tradeSerial)
        {
            _logger.LogInformation("Accepting secure trade {Serial}", tradeSerial);
            byte[] pkt = new byte[17];
            pkt[0] = 0x6F;
            pkt[1] = (byte)(pkt.Length >> 8);
            pkt[2] = (byte)pkt.Length;
            pkt[3] = 0x02; // Action: Update/Accept
            pkt[4] = (byte)(tradeSerial >> 24);
            pkt[5] = (byte)(tradeSerial >> 16);
            pkt[6] = (byte)(tradeSerial >> 8);
            pkt[7] = (byte)tradeSerial;
            pkt[8] = 0x00;
            pkt[9] = 0x00;
            pkt[10] = 0x00;
            pkt[11] = 0x01; // 1 = accepted
            _packetService.SendToServer(pkt);
        }

        public void CancelTrade(uint tradeSerial)
        {
            _logger.LogInformation("Cancelling secure trade {Serial}", tradeSerial);
            byte[] pkt = new byte[17];
            pkt[0] = 0x6F;
            pkt[1] = (byte)(pkt.Length >> 8);
            pkt[2] = (byte)pkt.Length;
            pkt[3] = 0x01; // Action: Cancel
            pkt[4] = (byte)(tradeSerial >> 24);
            pkt[5] = (byte)(tradeSerial >> 16);
            pkt[6] = (byte)(tradeSerial >> 8);
            pkt[7] = (byte)tradeSerial;
            _packetService.SendToServer(pkt);

            bool removed;
            lock (_tradesLock) { removed = _trades.Remove(tradeSerial); }
            if (removed) TradeClosed?.Invoke(tradeSerial);
        }

        public void Offer(uint tradeSerial, uint gold, uint platinum)
        {
            _logger.LogInformation("Offering {Gold} gold and {Platinum} platinum in trade {Serial}", gold, platinum, tradeSerial);
            byte[] pkt = new byte[17];
            pkt[0] = 0x6F;
            pkt[1] = (byte)(pkt.Length >> 8);
            pkt[2] = (byte)pkt.Length;
            pkt[3] = 0x03; // Action: MoneyUpdate
            pkt[4] = (byte)(tradeSerial >> 24);
            pkt[5] = (byte)(tradeSerial >> 16);
            pkt[6] = (byte)(tradeSerial >> 8);
            pkt[7] = (byte)tradeSerial;
            pkt[8] = (byte)(gold >> 24);
            pkt[9] = (byte)(gold >> 16);
            pkt[10] = (byte)(gold >> 8);
            pkt[11] = (byte)gold;
            pkt[12] = (byte)(platinum >> 24);
            pkt[13] = (byte)(platinum >> 16);
            pkt[14] = (byte)(platinum >> 8);
            pkt[15] = (byte)platinum;
            pkt[16] = 0x00;
            _packetService.SendToServer(pkt);

            lock (_tradesLock)
            {
                if (_trades.TryGetValue(tradeSerial, out var trade))
                {
                    trade.GoldMe = gold;
                    trade.PlatinumMe = platinum;
                }
            }
        }

        public void Receive(TradeMessage message)
        {
            var (action, serial, data) = message.Value;
            // Tutte le modifiche a _trades avvengono sotto _tradesLock per evitare
            // corse con CancelTrade/GetTrade/ActiveTrades chiamati dal thread UI.
            Action? postLockEvent = null;

            lock (_tradesLock)
            {
                if (action == 0 && data != null) // Start
                {
                    _trades[serial] = data;
                    _logger.LogInformation("Trade opened with {Serial}, Name: {Name}", serial, data.NameTrader);
                    postLockEvent = () => TradeStarted?.Invoke(serial);
                }
                else if (action == 1) // Cancel
                {
                    if (_trades.Remove(serial))
                    {
                        _logger.LogInformation("Trade cancelled/closed with {Serial}", serial);
                        postLockEvent = () => TradeClosed?.Invoke(serial);
                    }
                }
                else if (action == 2 && data != null) // Update
                {
                    if (_trades.TryGetValue(serial, out var existing))
                    {
                        existing.AcceptMe = data.AcceptMe;
                        existing.AcceptTrader = data.AcceptTrader;
                        _logger.LogInformation("Trade updated for {Serial}. AcceptMe: {Me}, AcceptTrader: {Trader}", serial, existing.AcceptMe, existing.AcceptTrader);
                        postLockEvent = () => TradeUpdated?.Invoke(serial);
                    }
                }
                else if (action == 3 && data != null) // MoneyUpdate
                {
                    if (_trades.TryGetValue(serial, out var existing))
                    {
                        if (data.GoldTrader > 0 || data.PlatinumTrader > 0)
                        {
                            existing.GoldTrader = data.GoldTrader;
                            existing.PlatinumTrader = data.PlatinumTrader;
                        }
                        if (data.GoldMe > 0 || data.PlatinumMe > 0)
                        {
                            existing.GoldMe = data.GoldMe;
                            existing.PlatinumMe = data.PlatinumMe;
                        }
                        postLockEvent = () => TradeUpdated?.Invoke(serial);
                    }
                }
                else if (action == 4 && data != null) // MoneyLimit
                {
                    if (_trades.TryGetValue(serial, out var existing))
                    {
                        existing.GoldMax = data.GoldMax;
                        existing.PlatinumMax = data.PlatinumMax;
                        postLockEvent = () => TradeUpdated?.Invoke(serial);
                    }
                }
            }

            // Invoca gli eventi fuori dal lock per evitare deadlock con i subscriber
            postLockEvent?.Invoke();
        }
    }
}
