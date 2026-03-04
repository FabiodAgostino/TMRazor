using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.UI.ViewModels
{
    public partial class SecureTradeViewModel : ViewModelBase, IDisposable
    {
        private readonly ISecureTradeService _tradeService;
        private readonly IWorldService _worldService;
        private readonly object _lock = new();

        public ObservableCollection<TradeSession> ActiveTrades { get; } = new();

        public SecureTradeViewModel(ISecureTradeService tradeService, IWorldService worldService)
        {
            _tradeService = tradeService;
            _worldService = worldService;
            BindingOperations.EnableCollectionSynchronization(ActiveTrades, _lock);

            _tradeService.TradeStarted += OnTradeStarted;
            _tradeService.TradeClosed += OnTradeClosed;
            _tradeService.TradeUpdated += OnTradeUpdated;

            // Carica scambi esistenti
            foreach (var serial in _tradeService.ActiveTrades)
            {
                OnTradeStarted(serial);
            }
        }

        private void OnTradeStarted(uint serial)
        {
            var data = _tradeService.GetTrade(serial);
            if (data == null) return;

            lock (_lock)
            {
                if (ActiveTrades.Any(t => t.Serial == serial)) return;
                var session = new TradeSession(serial)
                {
                    TargetName = data.NameTrader
                };
                ActiveTrades.Add(session);
                UpdateTradeSessionItems(session, data);
            }
        }

        private void OnTradeUpdated(uint serial)
        {
            var data = _tradeService.GetTrade(serial);
            if (data == null) return;

            TradeSession? session;
            lock (_lock)
            {
                session = ActiveTrades.FirstOrDefault(t => t.Serial == serial);
            }

            if (session != null)
            {
                RunOnUIThread(() =>
                {
                    session.MyAccepted = data.AcceptMe;
                    session.TheirAccepted = data.AcceptTrader;
                    session.MyGold = data.GoldMe;
                    session.MyPlatinum = data.PlatinumMe;
                    session.TheirGold = data.GoldTrader;
                    session.TheirPlatinum = data.PlatinumTrader;
                    session.GoldMax = data.GoldMax;
                    session.PlatinumMax = data.PlatinumMax;
                    
                    UpdateTradeSessionItems(session, data);
                });
            }
        }

        private void UpdateTradeSessionItems(TradeSession session, TradeData data)
        {
            session.MyItems.Clear();
            session.TheirItems.Clear();

            var myItems = _worldService.GetItemsInContainer(data.ContainerMe);
            foreach (var item in myItems)
            {
                session.MyItems.Add(new TradeItem(item.Serial, item.Amount, item.Graphic, item.Hue, "Item"));
            }

            var theirItems = _worldService.GetItemsInContainer(data.ContainerTrader);
            foreach (var item in theirItems)
            {
                session.TheirItems.Add(new TradeItem(item.Serial, item.Amount, item.Graphic, item.Hue, "Item"));
            }
        }

        private void OnTradeClosed(uint serial)
        {
            lock (_lock)
            {
                var trade = ActiveTrades.FirstOrDefault(t => t.Serial == serial);
                if (trade != null) ActiveTrades.Remove(trade);
            }
        }

        [RelayCommand]
        private void Accept(uint serial) => _tradeService.AcceptTrade(serial);

        [RelayCommand]
        private void Cancel(uint serial) => _tradeService.CancelTrade(serial);

        public void Dispose()
        {
            _tradeService.TradeStarted -= OnTradeStarted;
            _tradeService.TradeClosed -= OnTradeClosed;
            _tradeService.TradeUpdated -= OnTradeUpdated;
            GC.SuppressFinalize(this);
        }
    }
}
