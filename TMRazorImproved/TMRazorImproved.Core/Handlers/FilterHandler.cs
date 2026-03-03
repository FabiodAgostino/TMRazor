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
            
            // Snoop/Poison filter can be more complex (parsing text), but for now we focus on the basic ones.
            // Poison: A1, A2, A3 are stats update, but the "Poison" message is usually text.
            // We could filter Unicode Message (0xAE) or ASCII Message (0x1C) based on content.
            
            _logger.LogInformation("Classic Razor filters registered");
        }
    }
}
