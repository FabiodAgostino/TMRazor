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
        private readonly Dictionary<uint, TradeData> _trades = new();

        public event Action<uint>? TradeStarted;
        public event Action<uint>? TradeUpdated;
        public event Action<uint>? TradeClosed;

        public IReadOnlyList<uint> ActiveTrades => _trades.Keys.ToList().AsReadOnly();

        public TradeData? GetTrade(uint tradeSerial) => _trades.GetValueOrDefault(tradeSerial);

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
            
            if (_trades.ContainsKey(tradeSerial))
            {
                _trades.Remove(tradeSerial);
                TradeClosed?.Invoke(tradeSerial);
            }
        }

        public void Receive(TradeMessage message)
        {
            var (action, serial, data) = message.Value;

            if (action == 0 && data != null) // Start
            {
                _trades[serial] = data;
                _logger.LogInformation("Trade opened with {Serial}, Name: {Name}", serial, data.NameTrader);
                TradeStarted?.Invoke(serial);
            }
            else if (action == 1) // Cancel
            {
                if (_trades.Remove(serial))
                {
                    _logger.LogInformation("Trade cancelled/closed with {Serial}", serial);
                    TradeClosed?.Invoke(serial);
                }
            }
            else if (action == 2 && data != null) // Update
            {
                if (_trades.TryGetValue(serial, out var existing))
                {
                    existing.AcceptMe = data.AcceptMe;
                    existing.AcceptTrader = data.AcceptTrader;
                    _logger.LogInformation("Trade updated for {Serial}. AcceptMe: {Me}, AcceptTrader: {Trader}", serial, existing.AcceptMe, existing.AcceptTrader);
                    TradeUpdated?.Invoke(serial);
                }
            }
            else if (action == 3 && data != null) // MoneyUpdate
            {
                if (_trades.TryGetValue(serial, out var existing))
                {
                    // Server o Client? Dipende da dove è stato parsato.
                    // Il WorldPacketHandler ora lo fa sia C2S che S2C, popolando i rispettivi campi
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
                    TradeUpdated?.Invoke(serial);
                }
            }
            else if (action == 4 && data != null) // MoneyLimit
            {
                if (_trades.TryGetValue(serial, out var existing))
                {
                    existing.GoldMax = data.GoldMax;
                    existing.PlatinumMax = data.PlatinumMax;
                    TradeUpdated?.Invoke(serial);
                }
            }
        }
    }
}
