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
        private readonly object _lock = new();

        public ObservableCollection<TradeSession> ActiveTrades { get; } = new();

        public SecureTradeViewModel(ISecureTradeService tradeService)
        {
            _tradeService = tradeService;
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
            lock (_lock)
            {
                if (ActiveTrades.Any(t => t.Serial == serial)) return;
                ActiveTrades.Add(new TradeSession(serial, 0)); // TargetSerial inizialmente 0
            }
        }

        private void OnTradeUpdated(uint serial)
        {
            // Logica per aggiornare stato (accettato/non accettato)
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
        }
    }
}
