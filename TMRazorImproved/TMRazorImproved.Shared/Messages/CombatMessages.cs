using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TMRazorImproved.Shared.Messages
{
    /// <summary>Inviato quando la modalità guerra del giocatore cambia (0x72).</summary>
    public class WarModeMessage : ValueChangedMessage<bool>
    {
        public WarModeMessage(bool warMode) : base(warMode) { }
    }

    /// <summary>Inviato quando un mobile subisce danno (0x0B).</summary>
    public class DamageMessage : ValueChangedMessage<(uint Serial, ushort Amount)>
    {
        public DamageMessage(uint serial, ushort amount)
            : base((serial, amount)) { }
    }

    /// <summary>Inviato quando il target di attacco cambia (0xAA). Serial=0 se nessun target.</summary>
    public class AttackTargetMessage : ValueChangedMessage<uint>
    {
        public AttackTargetMessage(uint targetSerial) : base(targetSerial) { }
    }
}
