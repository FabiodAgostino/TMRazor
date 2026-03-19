using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models.Config;

namespace TMRazorImproved.Core.Services
{
    public class ShardService : IShardService
    {
        private const string ConfigFolder = "Config";
        private const string ShardsFile = "shards.json";

        private readonly ILogger<ShardService> _logger;
        private readonly List<ShardEntry> _shards = new();

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter() }
        };

        public ShardService(ILogger<ShardService> logger)
        {
            _logger = logger;
            EnsureDirectory();
            Load();
        }

        public IReadOnlyList<ShardEntry> GetAll() => _shards.AsReadOnly();

        public ShardEntry? GetSelected() => _shards.Find(s => s.IsSelected);

        public void Add(ShardEntry shard)
        {
            if (string.IsNullOrWhiteSpace(shard.Name))
                throw new ArgumentException("Shard name cannot be empty.");
            if (_shards.Exists(s => s.Name.Equals(shard.Name, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"A shard named '{shard.Name}' already exists.");

            _shards.Add(shard);
            Save();
        }

        public void Update(string originalName, ShardEntry shard)
        {
            int index = _shards.FindIndex(s => s.Name.Equals(originalName, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
                throw new KeyNotFoundException($"Shard '{originalName}' not found.");

            bool wasSelected = _shards[index].IsSelected;
            shard.IsSelected = wasSelected;
            _shards[index] = shard;
            Save();
        }

        public void Delete(string name)
        {
            int removed = _shards.RemoveAll(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (removed > 0)
                Save();
        }

        public void Select(string name)
        {
            foreach (var s in _shards)
                s.IsSelected = s.Name.Equals(name, StringComparison.OrdinalIgnoreCase);
            Save();
        }

        public void Save()
        {
            try
            {
                string path = GetFilePath();
                var list = new ShardList { Shards = _shards };
                string json = JsonSerializer.Serialize(list, _jsonOptions);
                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save shards.json");
            }
        }

        public void Load()
        {
            try
            {
                string path = GetFilePath();
                if (!File.Exists(path)) return;

                string json = File.ReadAllText(path);
                var list = JsonSerializer.Deserialize<ShardList>(json, _jsonOptions);
                if (list?.Shards != null)
                {
                    _shards.Clear();
                    _shards.AddRange(list.Shards);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load shards.json");
            }
        }

        private static string GetFilePath()
        {
            string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFolder);
            return Path.Combine(folder, ShardsFile);
        }

        private static void EnsureDirectory()
        {
            string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFolder);
            Directory.CreateDirectory(folder);
        }
    }
}
