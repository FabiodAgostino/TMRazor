using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TMRazorImproved.Shared.Messages
{
    /// <summary>
    /// Identifica quale statistica del giocatore è stata aggiornata.
    /// </summary>
    public enum StatType { Hits, Mana, Stamina, Weight, Followers, Tithe }

    /// <summary>
    /// Messaggio inviato ogni volta che HP, Mana o Stamina del giocatore cambiano.
    /// Sostituisce il parsing manuale di UOPacketMessage (0xA1/0xA2/0xA3) nei ViewModel.
    /// </summary>
    public class PlayerStatusMessage : ValueChangedMessage<(StatType Stat, uint Serial, ushort Current, ushort Max)>
    {
        public PlayerStatusMessage(StatType stat, uint serial, ushort current, ushort max)
            : base((stat, serial, current, max)) { }
    }
}
