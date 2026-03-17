using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TMRazorImproved.Shared.Messages
{
    /// <summary>Inviato quando viene rilevato un cambio di shard (IP/Porta).</summary>
    public class ShardChangedMessage : ValueChangedMessage<string>
    {
        public ShardChangedMessage(string shardId) : base(shardId) { }
    }
}
