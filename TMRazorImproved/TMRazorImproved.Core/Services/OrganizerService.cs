using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;
using TMRazorImproved.Shared.Models.Config;
using TMRazorImproved.Core.Utilities;

namespace TMRazorImproved.Core.Services
{
    public class OrganizerService : AgentServiceBase, IOrganizerService
    {
        private readonly IPacketService _packetService;
        private readonly IConfigService _configService;
        private readonly IWorldService _worldService;
        private readonly ILogger<OrganizerService> _logger;
        
        public event Action? OnComplete;

        public OrganizerService(
            IPacketService packetService, 
            IConfigService configService,
            IWorldService worldService,
            IHotkeyService hotkeyService,
            ILogger<OrganizerService> logger)
        {
            _packetService = packetService;
            _configService = configService;
            _worldService = worldService;
            _logger = logger;

            hotkeyService.RegisterAction("Organizer Start", () => Start());
            hotkeyService.RegisterAction("Organizer Stop", () => _ = StopAsync());
            hotkeyService.RegisterAction("Organizer Toggle", () => { if (IsRunning) _ = StopAsync(); else Start(); });
        }

        public void ChangeList(string listName)
        {
            var profile = _configService.CurrentProfile;
            if (profile != null)
            {
                profile.ActiveOrganizerList = listName;
                _logger.LogInformation("Organizer list changed to: {ListName}", listName);
            }
        }

        // BUG-P1-04 FIX: il return type era non-nullable ma il metodo poteva restituire null (CS8603)
        private OrganizerConfig? GetActiveConfig()
        {
            var profile = _configService.CurrentProfile;
            if (profile == null) return null;
            return profile.OrganizerLists.FirstOrDefault(l => l.Name == profile.ActiveOrganizerList)
                   ?? profile.OrganizerLists.FirstOrDefault();
        }

        protected override async Task AgentLoopAsync(CancellationToken token)
        {
            var config = GetActiveConfig();
            if (config == null || config.Source == 0 || config.Destination == 0)
            {
                _logger.LogWarning("Organizer failed: Source or Destination container not set or config null.");
                return;
            }

            _logger.LogInformation("Organizer started. Source: 0x{Source:X}, Dest: 0x{Dest:X}", config.Source, config.Destination);

            // Troviamo gli item da spostare nel WorldService
            // Filtriamo per quelli che hanno come container il source e il cui graphic è nella lista (se la lista non è vuota)
            var itemsToMove = _worldService.Items
                .Where(i => i.Container == config.Source)
                .Where(i => config.ItemList.Count == 0 || config.ItemList.Any(li => li.IsEnabled && li.Graphic == (int)i.Graphic))
                .ToList();

            if (itemsToMove.Count == 0)
            {
                _logger.LogInformation("Organizer: No items found to move.");
                OnComplete?.Invoke();
                return;
            }

            _logger.LogDebug("Found {Count} items to organize", itemsToMove.Count);

            foreach (var item in itemsToMove)
            {
                if (token.IsCancellationRequested) break;

                _logger.LogTrace("Moving item 0x{Serial:X} (Graphic: 0x{Graphic:X})", item.Serial, item.Graphic);
                
                MoveItem(item.Serial, item.Amount, config.Destination);

                // Delay tra uno spostamento e l'altro (tipico di Razor)
                await Task.Delay(Math.Max(100, config.Delay), token);
            }

            _logger.LogInformation("Organizer completed.");
            OnComplete?.Invoke();
        }

        private void MoveItem(uint serial, ushort amount, uint targetContainer)
        {
            _packetService.SendToServer(PacketBuilder.LiftItem(serial, amount));
            _packetService.SendToServer(PacketBuilder.DropToContainer(serial, targetContainer));
        }
    }
}
