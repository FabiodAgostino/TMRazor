using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.Core.Services
{
    public class WeaponService : IWeaponService
    {
        private readonly ConcurrentDictionary<ushort, WeaponInfo> _weapons = new();
        private readonly ILogger<WeaponService> _logger;

        public WeaponService(IConfigService configService, ILogger<WeaponService> logger)
        {
            _logger = logger;
            LoadWeapons(configService.Global.DataPath);
        }

        private void LoadWeapons(string dataPath)
        {
            try
            {
                // Cerca in diverse posizioni possibili
                string[] paths = new[]
                {
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "weapons.json"),
                    Path.Combine(dataPath, "Config", "weapons.json"),
                    Path.Combine(dataPath, "weapons.json")
                };

                foreach (var path in paths)
                {
                    if (File.Exists(path))
                    {
                        var json = File.ReadAllText(path);
                        var list = JsonSerializer.Deserialize<List<WeaponInfo>>(json, new JsonSerializerOptions 
                        { 
                            PropertyNameCaseInsensitive = true 
                        });

                        if (list != null)
                        {
                            foreach (var w in list)
                            {
                                _weapons[w.Graphic] = w;
                            }
                            _logger.LogInformation("Loaded {Count} weapons from {Path}", list.Count, path);
                            return;
                        }
                    }
                }
                _logger.LogWarning("weapons.json not found in any of the search paths.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading weapons.json");
            }
        }

        public bool IsTwoHanded(ushort graphic)
        {
            return _weapons.TryGetValue(graphic, out var info) && info.TwoHanded;
        }

        public WeaponInfo? GetWeaponInfo(ushort graphic)
        {
            return _weapons.GetValueOrDefault(graphic);
        }
    }
}
