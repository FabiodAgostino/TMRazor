using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TMRazorImproved.Shared.Interfaces;

namespace TMRazorImproved.Core.Services
{
    public interface IAutoCarverService : IAgentService { }

    public class AutoCarverService : AgentServiceBase, IAutoCarverService
    {
        private readonly IWorldService _world;
        private readonly ITargetingService _targeting;
        private readonly IPacketService _packet;
        private readonly IConfigService _config;
        private readonly ILogger<AutoCarverService> _logger;
        private readonly System.Collections.Generic.HashSet<uint> _carvedCorpses = new();

        public AutoCarverService(
            IWorldService world,
            ITargetingService targeting,
            IPacketService packet,
            IConfigService config,
            ILogger<AutoCarverService> logger)
        {
            _world = world;
            _targeting = targeting;
            _packet = packet;
            _config = config;
            _logger = logger;
        }

        protected override async Task AgentLoopAsync(CancellationToken cancel)
        {
            while (!cancel.IsCancellationRequested)
            {
                await Task.Delay(500, cancel);

                if (_world.Player == null) continue;
                if (!_config.CurrentProfile.AutoCarver || _config.CurrentProfile.AutoCarverBlade == 0) continue;

                var player = _world.Player;
                var blade = _world.FindItem(_config.CurrentProfile.AutoCarverBlade);
                
                if (blade == null || blade.Container != player.Serial && (blade.Container != 0 && _world.FindItem(blade.Container)?.Container != player.Serial))
                {
                    // Blade not found in backpack or hands
                    continue;
                }

                // Trova corpi vicini non ancora tagliati
                var corpses = _world.Items.Where(i => 
                    i.Graphic == 0x2006 && 
                    i.Container == 0 && 
                    !_carvedCorpses.Contains(i.Serial) &&
                    i.DistanceTo(player) <= 3
                ).ToList();

                foreach (var corpse in corpses)
                {
                    _logger.LogInformation($"AutoCarver: cutting corpse {corpse.Serial}");
                    
                    _targeting.ClearTargetCursor();
                    
                    // Double click blade
                    _packet.SendToServer(Utilities.PacketBuilder.DoubleClick(blade.Serial));
                    
                    // Wait for server to send target cursor
                    await Task.Delay(150, cancel);
                    
                    uint cursorId = _targeting.PendingCursorId;
                    _targeting.ClearTargetCursor();
                    
                    // Send target to corpse
                    _packet.SendToServer(Utilities.PacketBuilder.TargetObject(corpse.Serial, cursorId));
                    
                    _carvedCorpses.Add(corpse.Serial);
                    await Task.Delay(500, cancel);
                }
            }
        }
    }
}
