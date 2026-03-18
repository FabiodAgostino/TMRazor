using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TMRazorImproved.Shared.Messages
{
    /// <summary>Inviato quando viene rilevato un cambio di shard (IP/Porta).</summary>
    public class ShardChangedMessage : ValueChangedMessage<string>
    {
        public ShardChangedMessage(string shardId) : base(shardId) { }
    }

    /// <summary>Inviato quando il ping verso il server è stato calcolato.</summary>
    public class PingUpdatedMessage : ValueChangedMessage<double>
    {
        public PingUpdatedMessage(double pingMs) : base(pingMs) { }
    }

    /// <summary>Richiesta di avvio di una sequenza di ping.</summary>
    public class StartPingRequestMessage : ValueChangedMessage<int>
    {
        public StartPingRequestMessage(int count) : base(count) { }
    }
}
