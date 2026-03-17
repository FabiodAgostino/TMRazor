using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Mvvm.Messaging;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models.Config;
using TMRazorImproved.Shared.Messages;

namespace TMRazorImproved.Core.Services
{
    public class ConfigService : IConfigService
    {
        private const string ConfigFolder = "Config";
        private const string GlobalFile = "global.json";
        private const string ProfilesFolder = "Profiles";

        private readonly ILogger<ConfigService> _logger;
        private readonly IMessenger _messenger;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
        };

        public GlobalSettings Global { get; private set; } = new();
        public UserProfile CurrentProfile { get; private set; } = new();
        public string CurrentShardId { get; private set; } = "Unknown";

        public ConfigService(ILogger<ConfigService> logger, IMessenger messenger)
        {
            _logger = logger;
            _messenger = messenger;
            EnsureDirectories();
            Load();
        }

        public void SetCurrentShard(string shardId)
        {
            if (string.IsNullOrWhiteSpace(shardId)) shardId = "Unknown";
            if (CurrentShardId == shardId) return;

            _logger.LogInformation("Current Shard detected: {Shard}", shardId);
            CurrentShardId = shardId;

            // Se il profilo corrente era "Unknown", lo associamo al nuovo shard
            if (CurrentProfile.ShardId == "Unknown")
            {
                CurrentProfile.ShardId = CurrentShardId;
                SaveProfile(CurrentProfile);
            }

            _messenger.Send(new ShardChangedMessage(CurrentShardId));
        }

        private void EnsureDirectories()
        {
            string configDir = Path.Combine(AppContext.BaseDirectory, ConfigFolder);
            if (!Directory.Exists(configDir)) Directory.CreateDirectory(configDir);
            
            string profilesPath = Path.Combine(configDir, ProfilesFolder);
            if (!Directory.Exists(profilesPath)) Directory.CreateDirectory(profilesPath);
        }

        public void Load()
        {
            string globalPath = Path.Combine(AppContext.BaseDirectory, ConfigFolder, GlobalFile);
            if (File.Exists(globalPath))
            {
                Global = SafeDeserialize<GlobalSettings>(globalPath) ?? new GlobalSettings();
            }
            else
            {
                Global = new GlobalSettings();
                SaveGlobal();
            }

            SwitchProfile(Global.LastProfile);
        }

        public void Save()
        {
            SaveGlobal();
            SaveProfile(CurrentProfile);
        }

        private void SaveGlobal()
        {
            string globalPath = Path.Combine(AppContext.BaseDirectory, ConfigFolder, GlobalFile);
            string json = JsonSerializer.Serialize(Global, _jsonOptions);
            File.WriteAllText(globalPath, json);
            _logger.LogInformation("Global settings saved to {Path}", globalPath);
        }

        private void SaveProfile(UserProfile profile)
        {
            string profilePath = Path.Combine(AppContext.BaseDirectory, ConfigFolder, ProfilesFolder, $"{profile.Name}.json");
            string json = JsonSerializer.Serialize(profile, _jsonOptions);
            File.WriteAllText(profilePath, json);
            _logger.LogInformation("Profile {Profile} saved to {Path}", profile.Name, profilePath);
        }

        public void SwitchProfile(string profileName)
        {
            // Sanitizza il nome profilo per prevenire directory traversal
            if (string.IsNullOrWhiteSpace(profileName) || profileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                profileName = "Default";

            _logger.LogInformation("Switching to profile: {Profile}", profileName);

            string profilePath = Path.Combine(AppContext.BaseDirectory, ConfigFolder, ProfilesFolder, $"{profileName}.json");

            if (File.Exists(profilePath))
            {
                CurrentProfile = SafeDeserialize<UserProfile>(profilePath)
                                 ?? new UserProfile { Name = profileName, ShardId = CurrentShardId };
                
                // Se il profilo caricato non ha shard o è Unknown, lo associamo a quello attuale
                if (CurrentProfile.ShardId == "Unknown")
                {
                    CurrentProfile.ShardId = CurrentShardId;
                    SaveProfile(CurrentProfile);
                }
            }
            else
            {
                _logger.LogInformation("Profile file not found, creating new profile: {Profile}", profileName);
                CurrentProfile = new UserProfile { Name = profileName, ShardId = CurrentShardId };
                SaveProfile(CurrentProfile);
            }

            Global.LastProfile = profileName;
            SaveGlobal();
        }

        public void CreateProfile(string profileName)
        {
            if (string.IsNullOrWhiteSpace(profileName)) return;
            _logger.LogInformation("Creating new profile: {Profile}", profileName);
            var newProfile = new UserProfile { Name = profileName };
            SaveProfile(newProfile);
        }

        public void CloneProfile(string sourceProfileName, string newProfileName)
        {
            if (string.IsNullOrWhiteSpace(sourceProfileName) || string.IsNullOrWhiteSpace(newProfileName)) return;
            
            _logger.LogInformation("Cloning profile {Source} to {Target}", sourceProfileName, newProfileName);
            
            string sourcePath = Path.Combine(AppContext.BaseDirectory, ConfigFolder, ProfilesFolder, $"{sourceProfileName}.json");
            if (!File.Exists(sourcePath)) return;

            var profile = SafeDeserialize<UserProfile>(sourcePath);
            if (profile != null)
            {
                profile.Name = newProfileName;
                SaveProfile(profile);
            }
        }

        public void RenameProfile(string oldProfileName, string newProfileName)
        {
            if (string.IsNullOrWhiteSpace(oldProfileName) || string.IsNullOrWhiteSpace(newProfileName) || oldProfileName == "Default") return;

            _logger.LogInformation("Renaming profile {Old} to {New}", oldProfileName, newProfileName);

            string oldPath = Path.Combine(AppContext.BaseDirectory, ConfigFolder, ProfilesFolder, $"{oldProfileName}.json");
            if (!File.Exists(oldPath)) return;

            if (CurrentProfile.Name == oldProfileName)
            {
                CurrentProfile.Name = newProfileName;
                SaveProfile(CurrentProfile);
                Global.LastProfile = newProfileName;
                SaveGlobal();
            }
            else
            {
                var profile = SafeDeserialize<UserProfile>(oldPath);
                if (profile != null)
                {
                    profile.Name = newProfileName;
                    SaveProfile(profile);
                }
            }

            File.Delete(oldPath);
        }

        public void DeleteProfile(string profileName)
        {
            if (profileName == "Default") return; // Non cancellare il default
            _logger.LogInformation("Deleting profile: {Profile}", profileName);
            string profilePath = Path.Combine(AppContext.BaseDirectory, ConfigFolder, ProfilesFolder, $"{profileName}.json");
            if (File.Exists(profilePath)) File.Delete(profilePath);
            
            if (Global.LastProfile == profileName) SwitchProfile("Default");
        }

        public IEnumerable<string> GetAvailableProfiles(string? shardId = null)
        {
            string profilesPath = Path.Combine(AppContext.BaseDirectory, ConfigFolder, ProfilesFolder);
            if (!Directory.Exists(profilesPath)) yield break;

            foreach (var file in Directory.GetFiles(profilesPath, "*.json"))
            {
                string name = Path.GetFileNameWithoutExtension(file);
                if (shardId == null)
                {
                    yield return name;
                }
                else
                {
                    var profile = SafeDeserialize<UserProfile>(file);
                    if (profile != null && (profile.ShardId == shardId || profile.ShardId == "Unknown" || name == "Default"))
                    {
                        yield return name;
                    }
                }
            }
        }

        private T? SafeDeserialize<T>(string filePath) where T : class
        {
            try
            {
                string json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<T>(json, _jsonOptions);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON corruption detected in {Path}", filePath);
                // JSON corrotto — salva backup per debug, poi ritorna null per recovery
                try
                {
                    string backupPath = filePath + ".corrupt." + DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    File.Copy(filePath, backupPath, overwrite: true);
                    File.Delete(filePath);
                    _logger.LogInformation("Corrupted file moved to {BackupPath}", backupPath);
                }
                catch (Exception backupEx) { _logger.LogWarning(backupEx, "Failed to create backup of corrupted file"); }
                return null;
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "IO error while reading {Path}", filePath);
                // File in uso o inaccessibile — ritorna null per recovery con default
                return null;
            }
        }
    }
}
