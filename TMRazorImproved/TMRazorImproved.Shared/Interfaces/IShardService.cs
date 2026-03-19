using System.Collections.Generic;
using TMRazorImproved.Shared.Models.Config;

namespace TMRazorImproved.Shared.Interfaces
{
    public interface IShardService
    {
        IReadOnlyList<ShardEntry> GetAll();
        ShardEntry? GetSelected();
        void Add(ShardEntry shard);
        void Update(string originalName, ShardEntry shard);
        void Delete(string name);
        void Select(string name);
        void Save();
        void Load();
    }
}
