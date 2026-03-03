using CommunityToolkit.Mvvm.Messaging.Messages;
using TMRazorImproved.Shared.Enums;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.Shared.Messages
{
    /// <summary>
    /// Messaggio inviato quando un pacchetto UO viene ricevuto o inviato.
    /// Utilizzato per aggiornare la UI o altri servizi in modo asincrono.
    /// </summary>
    public class UOPacketMessage : ValueChangedMessage<UOPacket>
    {
        public PacketPath Path { get; }

        public UOPacketMessage(PacketPath path, UOPacket packet) : base(packet)
        {
            Path = path;
        }
    }
}
