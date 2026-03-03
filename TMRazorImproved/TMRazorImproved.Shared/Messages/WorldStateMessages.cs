using System.Collections.Generic;
using CommunityToolkit.Mvvm.Messaging.Messages;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.Shared.Messages
{
    /// <summary>Inviato quando un contenitore viene aperto (server ha risposto con 0x3C).</summary>
    public class ContainerContentMessage : ValueChangedMessage<(uint ContainerSerial, IReadOnlyList<Item> Items)>
    {
        public ContainerContentMessage(uint containerSerial, IReadOnlyList<Item> items)
            : base((containerSerial, items)) { }
    }

    /// <summary>Inviato quando un singolo item viene aggiunto a un contenitore (0x25).</summary>
    public class ContainerItemAddedMessage : ValueChangedMessage<(uint ContainerSerial, uint ItemSerial)>
    {
        public ContainerItemAddedMessage(uint containerSerial, uint itemSerial)
            : base((containerSerial, itemSerial)) { }
    }

    /// <summary>Inviato quando appare un oggetto nel mondo (0x1A).</summary>
    public class WorldItemMessage : ValueChangedMessage<Item>
    {
        public WorldItemMessage(Item item) : base(item) { }
    }

    /// <summary>Inviato quando si apre il Buy Menu di un vendor (0x74).</summary>
    public class VendorBuyMessage : ValueChangedMessage<(uint VendorSerial, IReadOnlyList<(uint Price, string Name)> Items)>
    {
        public VendorBuyMessage(uint vendorSerial, IReadOnlyList<(uint Price, string Name)> items) 
            : base((vendorSerial, items)) { }
    }

    /// <summary>Inviato quando si apre il Sell Menu di un vendor (0x9E).</summary>
    public class VendorSellMessage : ValueChangedMessage<(uint VendorSerial, IReadOnlyList<(uint Serial, ushort Graphic, ushort Hue, ushort Amount, ushort Price, string Name)> Items)>
    {
        public VendorSellMessage(uint vendorSerial, IReadOnlyList<(uint Serial, ushort Graphic, ushort Hue, ushort Amount, ushort Price, string Name)> items) 
            : base((vendorSerial, items)) { }
    }

    /// <summary>Inviato quando un item viene equipaggiato o rimosso da un mobile (0x2E).</summary>
    public class EquipmentChangedMessage : ValueChangedMessage<(uint MobileSerial, uint ItemSerial, byte Layer)>
    {
        public EquipmentChangedMessage(uint mobileSerial, uint itemSerial, byte layer)
            : base((mobileSerial, itemSerial, layer)) { }
    }

    /// <summary>Inviato quando il login è completato (0x55). Il mondo è pronto.</summary>
    public class LoginCompleteMessage : ValueChangedMessage<bool>
    {
        public LoginCompleteMessage() : base(true) { }
    }

    /// <summary>Inviato quando il nome di un mobile viene ricevuto (0x98).</summary>
    public class MobileNameMessage : ValueChangedMessage<(uint Serial, string Name)>
    {
        public MobileNameMessage(uint serial, string name)
            : base((serial, name)) { }
    }

    /// <summary>Inviato quando un mobile muore (0xAF DeathAnimation).</summary>
    public class MobileDeathMessage : ValueChangedMessage<uint>
    {
        public MobileDeathMessage(uint killedSerial) : base(killedSerial) { }
    }

    /// <summary>Inviato quando lo stato poison/yellow-hits di un mobile cambia (0x16/0x17).</summary>
    public class MobilePoisonedMessage : ValueChangedMessage<(uint Serial, bool IsPoisoned, bool IsYellowHits)>
    {
        public MobilePoisonedMessage(uint serial, bool isPoisoned, bool isYellowHits)
            : base((serial, isPoisoned, isYellowHits)) { }
    }

    /// <summary>Inviato quando il server rifiuta un lift item (0x27).</summary>
    public class LiftRejectMessage : ValueChangedMessage<byte>
    {
        public LiftRejectMessage(byte reason) : base(reason) { }
    }

    /// <summary>Inviato quando il giocatore muore (0x2C).</summary>
    public class PlayerDeathMessage : ValueChangedMessage<byte>
    {
        public PlayerDeathMessage(byte deathType) : base(deathType) { }
    }

    /// <summary>Inviato quando cambia la stagione del gioco (0xBC).</summary>
    public class SeasonChangeMessage : ValueChangedMessage<byte>
    {
        public SeasonChangeMessage(byte season) : base(season) { }
    }

    /// <summary>Inviato quando il server aggiorna la freccia di tracking (0xBA).</summary>
    public class TrackingArrowMessage : ValueChangedMessage<(bool Active, ushort X, ushort Y, uint TargetSerial)>
    {
        public TrackingArrowMessage(bool active, ushort x, ushort y, uint targetSerial)
            : base((active, x, y, targetSerial)) { }
    }

    /// <summary>Inviato quando il server invia un'animazione di test (0xE2).</summary>
    public class AnimationMessage : ValueChangedMessage<(uint Serial, short Action, short FrameCount, byte Delay)>
    {
        public AnimationMessage(uint serial, short action, short frameCount, byte delay)
            : base((serial, action, frameCount, delay)) { }
    }

    /// <summary>Inviato quando arriva un aggiornamento skill (0x3A). I dati grezzi vengono inoltrati a SkillsService.</summary>
    public class SkillsUpdatedMessage : ValueChangedMessage<byte[]>
    {
        public SkillsUpdatedMessage(byte[] rawData) : base(rawData) { }
    }

    /// <summary>Inviato quando il server invia il context menu di un'entità (0xBF sub 0x14).</summary>
    public class ContextMenuMessage : ValueChangedMessage<uint>
    {
        public ContextMenuMessage(uint entitySerial) : base(entitySerial) { }
    }

    /// <summary>Inviato quando il server conferma un passo di movimento (0x22).</summary>
    public class MovementAckMessage : ValueChangedMessage<(byte Seq, byte Notoriety)>
    {
        public MovementAckMessage(byte seq, byte notoriety) : base((seq, notoriety)) { }
    }

    /// <summary>Inviato quando il server richiede input testuale (0xAB).</summary>
    public class StringQueryMessage : ValueChangedMessage<(uint Serial, int QueryId, byte QueryType)>
    {
        public StringQueryMessage(uint serial, int queryId, byte queryType)
            : base((serial, queryId, queryType)) { }
    }

    /// <summary>Inviato quando il server invia/aggiorna una richiesta di scambio (0x6F).</summary>
    public class TradeMessage : ValueChangedMessage<(uint Serial, byte Action)>
    {
        public TradeMessage(uint serial, byte action) : base((serial, action)) { }
    }

    /// <summary>Inviato quando il server cambia la posizione del giocatore dopo un map change (0x76).</summary>
    public class ServerChangeMessage : ValueChangedMessage<(ushort X, ushort Y, short Z)>
    {
        public ServerChangeMessage(ushort x, ushort y, short z) : base((x, y, z)) { }
    }

    /// <summary>Inviato quando il server aggiorna il range di visione (0xC8).</summary>
    public class UpdateRangeMessage : ValueChangedMessage<byte>
    {
        public UpdateRangeMessage(byte range) : base(range) { }
    }

    /// <summary>Inviato quando il server invia un prompt testuale ASCII (0x9A S2C).</summary>
    public class AsciiPromptMessage : ValueChangedMessage<bool>
    {
        public AsciiPromptMessage(bool hasPrompt) : base(hasPrompt) { }
    }

    /// <summary>Inviato quando il server invia le feature flags (0xB9).</summary>
    public class FeaturesMessage : ValueChangedMessage<ushort>
    {
        public FeaturesMessage(ushort features) : base(features) { }
    }
}
