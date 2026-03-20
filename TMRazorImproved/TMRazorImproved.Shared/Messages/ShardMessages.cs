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

    /// <summary>
    /// Inviato quando il login server reindirizza il client al game server (pacchetto 0x8C).
    /// Contiene IP, porta e chiave di cifratura per la nuova connessione.
    /// Per i client OSI, la chiave è il seed per la cifratura del game server.
    /// </summary>
    public class RelayServerMessage : ValueChangedMessage<(string Ip, ushort Port, uint EncryptionKey)>
    {
        public RelayServerMessage(string ip, ushort port, uint encryptionKey)
            : base((ip, port, encryptionKey)) { }

        public string Ip => Value.Ip;
        public ushort Port => Value.Port;
        public uint EncryptionKey => Value.EncryptionKey;
    }
}
