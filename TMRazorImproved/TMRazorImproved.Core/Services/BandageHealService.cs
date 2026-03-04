using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;
using TMRazorImproved.Shared.Models.Config;
using TMRazorImproved.Core.Utilities;

namespace TMRazorImproved.Core.Services
{
    public class BandageHealService : AgentServiceBase, IBandageHealService
    {
        private readonly IPacketService _packetService;
        private readonly IConfigService _configService;
        private readonly IWorldService _worldService;
        private readonly ILogger<BandageHealService> _logger;

        public BandageHealService(
            IPacketService packetService,
            IConfigService configService,
            IWorldService worldService,
            IHotkeyService hotkeyService,
            ILogger<BandageHealService> logger)
        {
            _packetService = packetService;
            _configService = configService;
            _worldService = worldService;
            _logger = logger;

            hotkeyService.RegisterAction("BandageHeal Start", () => Start());
            hotkeyService.RegisterAction("BandageHeal Stop", () => _ = StopAsync());
            hotkeyService.RegisterAction("BandageHeal Toggle", () => { if (IsRunning) _ = StopAsync(); else Start(); });
        }

        protected override async Task AgentLoopAsync(CancellationToken token)
        {
            _logger.LogInformation("BandageHeal agent loop started");

            while (!token.IsCancellationRequested)
            {
                // FIX BUG-C06: null check su CurrentProfile prima di accedere a BandageHeal
                var profile = _configService.CurrentProfile;
                if (profile == null)
                {
                    await Task.Delay(500, token);
                    continue;
                }

                var config = profile.BandageHeal;
                var player = _worldService.Player;

                if (player != null && config.BandageSerial != 0)
                {
                    // Sprint Fix-3: HiddenStop — se attivo e il player è nascosto, sospende il ciclo
                    if (config.HiddenStop && player.IsHidden)
                    {
                        _logger.LogDebug("BandageHeal paused: player is hidden and HiddenStop is active.");
                        await Task.Delay(200, token);
                        continue;
                    }

                    bool needsHeal = (player.Hits * 100 / Math.Max(1, (int)player.HitsMax)) <= config.HpStart;
                    bool isPoisoned = player.IsPoisoned;

                    if (needsHeal || (isPoisoned && config.PoisonPriority))
                    {
                        _logger.LogDebug("Heal triggered. Hits: {Hits}/{Max}, Poisoned: {Poisoned}", player.Hits, player.HitsMax, isPoisoned);

                        // 1. Double click sulle bende (0x06)
                        _packetService.SendToServer(PacketBuilder.DoubleClick(config.BandageSerial));

                        // 2. Attendiamo un attimo che il server elabori il double click e chieda il target
                        await Task.Delay(150, token);

                        // 3. Target sul giocatore (self) (0x6C)
                        _packetService.SendToServer(PacketBuilder.TargetObject(player.Serial));

                        // 4. Calcolo del delay della benda
                        int delayMs = CalculateBandageDelay(player.Dex, config.CustomDelay);
                        _logger.LogTrace("Bandage applied. Waiting {Delay}ms", delayMs);

                        await Task.Delay(delayMs, token);
                    }
                }

                await Task.Delay(200, token);
            }
        }

        private int CalculateBandageDelay(ushort dex, int customDelay)
        {
            if (customDelay > 0) return customDelay;

            // Formula classica UO: (11 - (DEX / 20)) secondi; minimo 2s
            double seconds = 11.0 - (dex / 20.0);
            if (seconds < 2.0) seconds = 2.0;
            return (int)(seconds * 1000);
        }

        protected override void OnStopped()
        {
            _logger.LogInformation("BandageHeal agent stopped");
        }
    }
}
