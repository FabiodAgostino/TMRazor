using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using TMRazorImproved.Shared.Interfaces;
using CommunityToolkit.Mvvm.Messaging;
using TMRazorImproved.Shared.Messages;
using System.Linq;

namespace TMRazorImproved.Core.Services
{
    public class SecureTradeService : ISecureTradeService, IRecipient<TradeMessage>
    {
        private readonly IPacketService _packetService;
        private readonly ILogger<SecureTradeService> _logger;
        private readonly List<uint> _activeTrades = new();

        public event Action<uint>? TradeStarted;
        public event Action<uint>? TradeUpdated;
        public event Action<uint>? TradeClosed;

        public IReadOnlyList<uint> ActiveTrades => _activeTrades.AsReadOnly();

        public SecureTradeService(IPacketService packetService, IMessenger messenger, ILogger<SecureTradeService> logger)
        {
            _packetService = packetService;
            _logger = logger;
            messenger.RegisterAll(this);
        }

        public void StartTrade(uint targetSerial)
        {
            _logger.LogInformation("Starting secure trade with serial {Serial}", targetSerial);
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
            pkt[8] = 0x00; // Type?
            pkt[9] = 0x01; // 1 = accepted
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
            
            if (_activeTrades.Contains(tradeSerial))
            {
                _activeTrades.Remove(tradeSerial);
                TradeClosed?.Invoke(tradeSerial);
            }
        }

        public void Receive(TradeMessage message)
        {
            // 0=Start, 1=Cancel, 2=Update
            var (serial, action) = message.Value;

            if (action == 0 && !_activeTrades.Contains(serial))
            {
                _activeTrades.Add(serial);
                _logger.LogInformation("Trade opened with {Serial}", serial);
                TradeStarted?.Invoke(serial);
            }
            else if (action == 1 && _activeTrades.Contains(serial))
            {
                _activeTrades.Remove(serial);
                _logger.LogInformation("Trade cancelled/closed with {Serial}", serial);
                TradeClosed?.Invoke(serial);
            }
            else if (action == 2 && _activeTrades.Contains(serial))
            {
                _logger.LogInformation("Trade updated for {Serial}", serial);
                TradeUpdated?.Invoke(serial);
            }
        }
    }
}
