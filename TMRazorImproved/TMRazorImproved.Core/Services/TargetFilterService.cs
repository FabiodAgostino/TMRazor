using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models.Config;

namespace TMRazorImproved.Core.Services
{
    public class TargetFilterService : ITargetFilterService
    {
        private readonly IConfigService _config;
        private readonly IWorldService _worldService;
        private readonly IFriendsService _friendsService;
        private readonly ILogger<TargetFilterService> _logger;
        private readonly ConcurrentDictionary<uint, string> _filters = new();

        public IReadOnlyList<TargetFilterEntry> Filters => _config.CurrentProfile.ExcludedTargets;

        public TargetFilterService(
            IConfigService config, 
            IWorldService worldService, 
            IFriendsService friendsService,
            ILogger<TargetFilterService> logger)
        {
            _config = config;
            _worldService = worldService;
            _friendsService = friendsService;
            _logger = logger;

            InitializeFilters();
        }

        private void InitializeFilters()
        {
            _filters.Clear();
            foreach (var entry in _config.CurrentProfile.ExcludedTargets)
            {
                if (entry.Enabled)
                {
                    _filters.TryAdd(entry.Serial, entry.Name);
                }
            }
        }

        public void AddFilter(uint serial, string name)
        {
            if (_friendsService.IsFriend(serial))
            {
                _logger.LogWarning("Cannot add friend {Name} (0x{Serial:X}) to target filters", name, serial);
                return;
            }

            if (!_config.CurrentProfile.ExcludedTargets.Any(f => f.Serial == serial))
            {
                _config.CurrentProfile.ExcludedTargets.Add(new TargetFilterEntry { Serial = serial, Name = name, Enabled = true });
                _filters.TryAdd(serial, name);
                _config.Save();
                _logger.LogInformation("Added {Name} (0x{Serial:X}) to target filter list", name, serial);
            }
        }

        public void AddAllMobiles()
        {
            var mobiles = _worldService.Mobiles.Where(m => m.Serial != _worldService.Player?.Serial);
            foreach (var m in mobiles)
            {
                if (!_friendsService.IsFriend(m.Serial))
                {
                    AddFilter(m.Serial, m.Name);
                }
            }
        }

        public void AddAllHumanoids()
        {
            var humanoids = _worldService.Mobiles.Where(m => m.Serial != _worldService.Player?.Serial && m.IsHuman);
            foreach (var m in humanoids)
            {
                if (!_friendsService.IsFriend(m.Serial))
                {
                    AddFilter(m.Serial, m.Name);
                }
            }
        }

        public bool IsFiltered(uint serial)
        {
            return _filters.ContainsKey(serial);
        }

        public void RemoveFilter(uint serial)
        {
            var entry = _config.CurrentProfile.ExcludedTargets.FirstOrDefault(f => f.Serial == serial);
            if (entry != null)
            {
                _config.CurrentProfile.ExcludedTargets.Remove(entry);
                _filters.TryRemove(serial, out _);
                _config.Save();
                _logger.LogInformation("Removed 0x{Serial:X} from target filter list", serial);
            }
        }

        public void ClearAll()
        {
            _config.CurrentProfile.ExcludedTargets.Clear();
            _filters.Clear();
            _config.Save();
            _logger.LogInformation("Cleared all target filters");
        }
    }
}
