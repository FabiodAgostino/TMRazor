using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Enums;

namespace TMRazorImproved.Core.Services
{
    public interface IBoneCutterService : IAgentService { }

    public class BoneCutterService : AgentServiceBase, IBoneCutterService
    {
        private readonly IWorldService _world;
        private readonly ITargetingService _targeting;
        private readonly IPacketService _packet;
        private readonly ILogger<BoneCutterService> _logger;
        private readonly System.Collections.Generic.HashSet<uint> _cutBones = new();
        
        // Da 0x0ECA a 0x0ED2
        private static readonly int[] _boneGraphics = { 0x0ECA, 0x0ECB, 0x0ECC, 0x0ECD, 0x0ECE, 0x0ECF, 0x0ED0, 0x0ED1, 0x0ED2 };

        public BoneCutterService(
            IWorldService world,
            ITargetingService targeting,
            IPacketService packet,
            IConfigService config,
            ILogger<BoneCutterService> logger) : base(config)
        {
            _world = world;
            _targeting = targeting;
            _packet = packet;
            _logger = logger;
        }

        protected override async Task AgentLoopAsync(CancellationToken cancel)
        {
            while (!cancel.IsCancellationRequested)
            {
                await Task.Delay(500, cancel);

                if (_world.Player == null) continue;
                if (!_configService.CurrentProfile.BoneCutter || _configService.CurrentProfile.BoneCutterBlade == 0) continue;

                var player = _world.Player;
                var blade = _world.FindItem(_configService.CurrentProfile.BoneCutterBlade);
                
                if (blade == null || blade.Container != player.Serial && (blade.Container != 0 && _world.FindItem(blade.Container)?.Container != player.Serial))
                {
                    continue;
                }

                // Trova ossa vicine non ancora tagliate
                var bones = _world.Items.Where(i => 
                    _boneGraphics.Contains(i.Graphic) && 
                    i.Container == 0 && 
                    !_cutBones.Contains(i.Serial) &&
                    i.DistanceTo(player) <= 1
                ).ToList();

                foreach (var bone in bones)
                {
                    _logger.LogInformation($"BoneCutter: cutting bone {bone.Serial}");
                    
                    _targeting.ClearTargetCursor();
                    
                    // Double click blade
                    _packet.SendToServer(Utilities.PacketBuilder.DoubleClick(blade.Serial));
                    
                    // Wait for server to send target cursor
                    await Task.Delay(150, cancel);
                    
                    uint cursorId = _targeting.PendingCursorId;
                    _targeting.ClearTargetCursor();
                    
                    // Send target to bone
                    _packet.SendToServer(Utilities.PacketBuilder.TargetObject(bone.Serial, cursorId));
                    
                    _cutBones.Add(bone.Serial);
                    await Task.Delay(500, cancel);
                }
            }
        }
    }
}
