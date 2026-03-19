using System.Collections.Generic;

namespace TMRazorImproved.Shared.Models.Config
{
    public enum ClientStartType { TmClient, ClassicUO, OSI }

    public class ShardEntry
    {
        public string Name { get; set; } = string.Empty;
        public string Host { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 2593;
        public string ClientPath { get; set; } = string.Empty;
        public string DataFolder { get; set; } = string.Empty;
        public bool PatchEncryption { get; set; } = true;
        public bool OSIEncryption { get; set; }
        public ClientStartType StartType { get; set; } = ClientStartType.TmClient;
        public bool IsSelected { get; set; }
    }

    public class ShardList
    {
        public List<ShardEntry> Shards { get; set; } = new();
    }
}
