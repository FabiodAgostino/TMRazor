using System;
using TMRazorImproved.Shared.Enums;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models.Config;
using Microsoft.Extensions.Logging;

namespace TMRazorImproved.Core.Handlers
{
    /// <summary>
    /// Gestisce l'attivazione/disattivazione dei filtri classici di Razor
    /// intercettando e bloccando i relativi pacchetti.
    /// </summary>
    public class FilterHandler
    {
        private readonly IPacketService _packetService;
        private readonly IConfigService _configService;
        private readonly ILogger<FilterHandler> _logger;

        public FilterHandler(IPacketService packetService, IConfigService configService, ILogger<FilterHandler> logger)
        {
            _packetService = packetService;
            _configService = configService;
            _logger = logger;

            RegisterFilters();
        }

        private void RegisterFilters()
        {
            // Light Filter (0x4E, 0x4F)
            _packetService.RegisterFilter(PacketPath.ServerToClient, 0x4E, _ => !_configService.CurrentProfile.FilterLight);
            _packetService.RegisterFilter(PacketPath.ServerToClient, 0x4F, _ => !_configService.CurrentProfile.FilterLight);

            // Weather Filter (0x65)
            _packetService.RegisterFilter(PacketPath.ServerToClient, 0x65, _ => !_configService.CurrentProfile.FilterWeather);

            // Sound Filter (0x54)
            _packetService.RegisterFilter(PacketPath.ServerToClient, 0x54, _ => !_configService.CurrentProfile.FilterSound);

            // Death Filter (0x2C)
            _packetService.RegisterFilter(PacketPath.ServerToClient, 0x2C, _ => !_configService.CurrentProfile.FilterDeath);

            // Staff Filter (0xAE, 0x1C) - Check if from a staff member? 
            // In classic Razor this blocks some staff-specific packets or messages.

            // Season Filter (0xBC)
            _packetService.RegisterFilter(PacketPath.ServerToClient, 0xBC, _ => !_configService.CurrentProfile.FilterSeason);

            // Footsteps (0x54 with specific sounds?)
            // Razor usually filters specific sound IDs for footsteps.

            _logger.LogInformation("Classic Razor filters registered");
        }
    }
}
