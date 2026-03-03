using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Messages;
using TMRazorImproved.UI.Utilities;

namespace TMRazorImproved.UI.ViewModels
{
    /// <summary>
    /// Espone le statistiche vitali del giocatore (HP, Mana, Stamina) alla UI tramite Data Binding.
    /// Implementa throttling a 10fps per disaccoppiare gli aggiornamenti ad alta frequenza
    /// del thread di rete dall'aggiornamento del Dispatcher UI.
    ///
    /// Flusso dati:
    ///   [Network Thread] packet 0xA1/0xA2/0xA3
    ///       → WorldPacketHandler aggiorna Mobile + invia PlayerStatusMessage
    ///       → Receive() scrive i valori nei campi volatile e imposta i flag dirty (non-blocking)
    ///       → DispatcherTimer (UI thread, 100ms) chiama FlushPendingUpdates()
    ///       → Le proprietà Observable cambiano → WPF aggiorna la View
    /// </summary>
    public sealed partial class PlayerStatusViewModel : ViewModelBase, IRecipient<PlayerStatusMessage>, IDisposable
    {
        private readonly IWorldService _worldService;
        private readonly UiThrottler _throttler;

        // Valori in attesa scritti dal thread di rete, letti dal DispatcherTimer.
        // volatile garantisce visibilità cross-thread su x86 senza lock costosi.
        private volatile ushort _pendingHits;
        private volatile ushort _pendingHitsMax;
        private volatile ushort _pendingMana;
        private volatile ushort _pendingManaMax;
        private volatile ushort _pendingStam;
        private volatile ushort _pendingStamMax;

        // Flag dirty: segnalano che c'è almeno un aggiornamento da applicare.
        // Usiamo int invece di bool per poter usare Interlocked.Exchange atomico.
        private int _hitsDirty;  // 0 = clean, 1 = dirty
        private int _manaDirty;
        private int _stamDirty;

        // --- Proprietà Observable (aggiornate solo sul thread UI) ---

        [ObservableProperty]
        private ushort _hits;

        [ObservableProperty]
        private ushort _hitsMax;

        [ObservableProperty]
        private ushort _mana;

        [ObservableProperty]
        private ushort _manaMax;

        [ObservableProperty]
        private ushort _stam;

        [ObservableProperty]
        private ushort _stamMax;

        // Percentuali calcolate per il binding delle ProgressBar
        public double HitsPercent => HitsMax > 0 ? (double)Hits / HitsMax : 0;
        public double ManaPercent => ManaMax > 0 ? (double)Mana / ManaMax : 0;
        public double StamPercent => StamMax > 0 ? (double)Stam / StamMax : 0;

        public PlayerStatusViewModel(IMessenger messenger, IWorldService worldService)
        {
            _worldService = worldService;

            // Il timer gira sul UI thread: nessun Dispatcher.Invoke necessario nel callback.
            _throttler = new UiThrottler(TimeSpan.FromMilliseconds(100), FlushPendingUpdates);

            messenger.RegisterAll(this);
        }

        /// <summary>
        /// Ricevuto dal thread di rete tramite IMessenger (WeakReferenceMessenger).
        /// NON aggiorna le proprietà direttamente: scrive solo nei campi pending e alza i flag.
        /// </summary>
        public void Receive(PlayerStatusMessage message)
        {
            var (stat, serial, current, max) = message.Value;

            // Ignora aggiornamenti di entità che non sono il giocatore.
            if (_worldService.Player?.Serial != serial)
                return;

            switch (stat)
            {
                case StatType.Hits:
                    _pendingHits = current;
                    _pendingHitsMax = max;
                    System.Threading.Interlocked.Exchange(ref _hitsDirty, 1);
                    break;

                case StatType.Mana:
                    _pendingMana = current;
                    _pendingManaMax = max;
                    System.Threading.Interlocked.Exchange(ref _manaDirty, 1);
                    break;

                case StatType.Stamina:
                    _pendingStam = current;
                    _pendingStamMax = max;
                    System.Threading.Interlocked.Exchange(ref _stamDirty, 1);
                    break;
            }
        }

        /// <summary>
        /// Eseguita dal DispatcherTimer sul thread UI ogni 100ms.
        /// Applica i valori pending alle proprietà Observable solo se ci sono cambiamenti.
        /// </summary>
        private void FlushPendingUpdates()
        {
            // Interlocked.Exchange legge il valore attuale e scrive contemporaneamente 0.
            // Se il valore precedente era 1 (dirty), procediamo con l'aggiornamento.
            // Questo previene la perdita di update tra il check e il reset.

            if (System.Threading.Interlocked.Exchange(ref _hitsDirty, 0) != 0)
            {
                Hits = _pendingHits;
                HitsMax = _pendingHitsMax;
                OnPropertyChanged(nameof(HitsPercent));
            }

            if (System.Threading.Interlocked.Exchange(ref _manaDirty, 0) != 0)
            {
                Mana = _pendingMana;
                ManaMax = _pendingManaMax;
                OnPropertyChanged(nameof(ManaPercent));
            }

            if (System.Threading.Interlocked.Exchange(ref _stamDirty, 0) != 0)
            {
                Stam = _pendingStam;
                StamMax = _pendingStamMax;
                OnPropertyChanged(nameof(StamPercent));
            }
        }

        public void Dispose()
        {
            _throttler.Dispose();
            WeakReferenceMessenger.Default.UnregisterAll(this);
        }
    }
}
