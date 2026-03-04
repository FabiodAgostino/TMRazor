using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Threading;
using TMRazorImproved.Shared.Messages;

namespace TMRazorImproved.UI.ViewModels
{
    /// <summary>Rappresenta un singolo messaggio overhead visibile nell'overlay.</summary>
    public partial class OverheadEntry : ObservableObject
    {
        [ObservableProperty] private string _text = string.Empty;
        [ObservableProperty] private string _senderName = string.Empty;
        [ObservableProperty] private double _opacity = 1.0;

        public Brush TextBrush { get; }
        public DateTime ExpiresAt { get; }

        public OverheadEntry(string sender, string text, ushort hue, OverheadMessageType type, int displaySeconds)
        {
            _senderName = sender;
            _text       = text;
            ExpiresAt   = DateTime.UtcNow.AddSeconds(displaySeconds);
            TextBrush   = HueToSolidBrush(hue, type);
        }

        private static SolidColorBrush HueToSolidBrush(ushort hue, OverheadMessageType type)
        {
            // Mapping semplificato: usiamo i colori UO classici per i tipi principali
            return type switch
            {
                OverheadMessageType.Emote   => new SolidColorBrush(Color.FromRgb(0xFF, 0x80, 0xFF)),
                OverheadMessageType.Yell    => new SolidColorBrush(Color.FromRgb(0xFF, 0x40, 0x40)),
                OverheadMessageType.Whisper => new SolidColorBrush(Color.FromRgb(0x80, 0x80, 0x80)),
                OverheadMessageType.Guild   => new SolidColorBrush(Color.FromRgb(0x00, 0xFF, 0x00)),
                OverheadMessageType.Alliance => new SolidColorBrush(Color.FromRgb(0x00, 0xD4, 0xFF)),
                OverheadMessageType.Spell   => new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0x00)),
                _                           => hue != 0 ? UoHueToBrush(hue)
                                                        : new SolidColorBrush(Colors.White)
            };
        }

        // Conversione approssimata UO hue → colore RGB (palette ridotta)
        private static SolidColorBrush UoHueToBrush(ushort hue)
        {
            // UO hue è un indice nella palette (0-3000). Usiamo una mappatura semplificata.
            byte r = (byte)((hue * 37) % 200 + 55);
            byte g = (byte)((hue * 59) % 200 + 55);
            byte b = (byte)((hue * 89) % 200 + 55);
            return new SolidColorBrush(Color.FromRgb(r, g, b));
        }
    }

    /// <summary>
    /// ViewModel per l'OverheadMessageOverlay.
    /// Riceve messaggi speech/emote via Messenger e li mostra con fade-out automatico.
    /// </summary>
    public partial class OverheadMessageOverlayViewModel : ViewModelBase,
                                                           IRecipient<OverheadMessageMessage>,
                                                           IDisposable
    {
        /// <summary>Durata di visualizzazione di ogni messaggio in secondi.</summary>
        private const int MessageDisplaySeconds = 6;

        /// <summary>Massimo numero di messaggi contemporaneamente visibili.</summary>
        private const int MaxMessages = 10;

        public ObservableCollection<OverheadEntry> Messages { get; } = new();

        private readonly DispatcherTimer _cleanupTimer;

        public OverheadMessageOverlayViewModel(IMessenger messenger)
        {
            messenger.RegisterAll(this);

            _cleanupTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
            _cleanupTimer.Tick += OnCleanupTick;
            _cleanupTimer.Start();
        }

        public void Receive(OverheadMessageMessage message)
        {
            var (serial, name, text, hue, msgType) = message.Value;

            var entry = new OverheadEntry(name, text, hue, msgType, MessageDisplaySeconds);

            RunOnUIThread(() =>
            {
                Messages.Add(entry);

                // Mantieni il massimo
                while (Messages.Count > MaxMessages)
                    Messages.RemoveAt(0);
            });
        }

        private void OnCleanupTick(object? sender, EventArgs e)
        {
            var now = DateTime.UtcNow;

            for (int i = Messages.Count - 1; i >= 0; i--)
            {
                var entry = Messages[i];
                double remaining = (entry.ExpiresAt - now).TotalSeconds;

                if (remaining <= 0)
                {
                    Messages.RemoveAt(i);
                }
                else if (remaining < 1.5)
                {
                    // Fade-out progressivo nell'ultimo 1.5 secondi
                    entry.Opacity = remaining / 1.5;
                }
            }
        }

        public void Dispose()
        {
            _cleanupTimer.Stop();
            _cleanupTimer.Tick -= OnCleanupTick;
            WeakReferenceMessenger.Default.UnregisterAll(this);
        }
    }
}
