using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Enums;

namespace TMRazorImproved.Core.Services
{
    public interface IAutoRemountService : IAgentService { }

    public class AutoRemountService : AgentServiceBase, IAutoRemountService
    {
        private readonly IWorldService _world;
        private readonly IPacketService _packet;
        private readonly ILogger<AutoRemountService> _logger;

        public AutoRemountService(
            IWorldService world,
            IPacketService packet,
            IConfigService config,
            ILogger<AutoRemountService> logger) : base(config)
        {
            _world = world;
            _packet = packet;
            _logger = logger;
        }

        protected override async Task AgentLoopAsync(CancellationToken cancel)
        {
            while (!cancel.IsCancellationRequested)
            {
                await Task.Delay(500, cancel);

                if (_world.Player == null) continue;
                if (!_configService.CurrentProfile.AutoRemount || _configService.CurrentProfile.RemountSerial == 0) continue;

                var player = _world.Player;
                
                // Don't mount if dead or not human body
                if (player.Hits == 0 || (player.Graphic != 0x0190 && player.Graphic != 0x0191 && player.Graphic != 0x025D && player.Graphic != 0x025E && player.Graphic != 0x029A && player.Graphic != 0x029B))
                    continue;

                // Already mounted
                var mountedItem = _world.GetItemsInContainer(player.Serial).FirstOrDefault(i => i.Layer == (byte)Layer.Mount);
                if (mountedItem != null)
                    continue;

                // Try item mount (Ethereal)
                var etheralMount = _world.FindItem(_configService.CurrentProfile.RemountSerial);
                if (etheralMount != null)
                {
                    _logger.LogInformation($"AutoRemount: using ethereal mount {etheralMount.Serial}");
                    _packet.SendToServer(Utilities.PacketBuilder.DoubleClick(etheralMount.Serial));
                    await Task.Delay(2000, cancel); // Wait before retrying
                    continue;
                }

                // Try mobile mount (Pet)
                var mount = _world.FindMobile(_configService.CurrentProfile.RemountSerial);
                if (mount != null && mount.DistanceTo(player) <= 2)
                {
                    _logger.LogInformation($"AutoRemount: using pet mount {mount.Serial}");
                    _packet.SendToServer(Utilities.PacketBuilder.DoubleClick(mount.Serial));
                    await Task.Delay(2000, cancel);
                }
            }
        }
    }
}
