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
        private readonly ITargetingService _targetingService;
        private readonly IFriendsService _friendsService;
        private readonly ILogger<BandageHealService> _logger;

        public BandageHealService(
            IPacketService packetService,
            IConfigService configService,
            IWorldService worldService,
            ITargetingService targetingService,
            IFriendsService friendsService,
            IHotkeyService hotkeyService,
            ILogger<BandageHealService> logger)
        {
            _packetService = packetService;
            _configService = configService;
            _worldService = worldService;
            _targetingService = targetingService;
            _friendsService = friendsService;
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
                    // HiddenStop
                    if (config.HiddenStop && player.IsHidden)
                    {
                        await Task.Delay(200, token);
                        continue;
                    }

                    // Determina il target
                    uint targetSerial = GetTargetSerial(config, player);
                    var target = _worldService.FindMobile(targetSerial);

                    if (target != null)
                    {
                        bool isPoisoned = target.IsPoisoned;
                        bool isMortal = target.IsYellowHits;
                        int hpPercent = (target.Hits * 100 / Math.Max(1, (int)target.HitsMax));

                        // Validazione condizioni
                        if (isMortal && !config.HealMortal) { await Task.Delay(200, token); continue; }
                        if (isPoisoned && !config.HealPoison) { await Task.Delay(200, token); continue; }
                        
                        bool needsHeal = hpPercent <= config.HpStart;
                        if (needsHeal || (isPoisoned && config.PoisonPriority))
                        {
                            if (player.DistanceTo(target) > config.MaxRange)
                            {
                                _logger.LogTrace("BandageHeal: target out of range ({0})", player.DistanceTo(target));
                                await Task.Delay(500, token);
                                continue;
                            }

                            _logger.LogDebug("Heal triggered on {0:X}. Hits: {1}%, Poisoned: {2}, Mortal: {3}", 
                                targetSerial, hpPercent, isPoisoned, isMortal);

                            _packetService.SendToServer(PacketBuilder.DoubleClick(config.BandageSerial));
                            await Task.Delay(150, token);
                            // FIX P0-01: usa il cursorId pendente dal server (0x6C S2C) come richiesto dal protocollo UO.
                            // CursorId=0 causava il rifiuto silenzioso del target da parte del server.
                            uint cursorId = _targetingService.PendingCursorId;
                            _targetingService.ClearTargetCursor();
                            _packetService.SendToServer(PacketBuilder.TargetObject(targetSerial, cursorId));

                            int delayMs = CalculateBandageDelay(player.Dex, config.CustomDelay);
                            await Task.Delay(delayMs, token);
                        }
                    }
                }

                await Task.Delay(200, token);
            }
        }

        private uint GetTargetSerial(BandageHealConfig config, Mobile player)
        {
            return config.TargetType switch
            {
                "Self" => player.Serial,
                "Last" => _targetingService.LastTarget,
                "Friend" => GetNearestFriend(player, config.MaxRange),
                _ => player.Serial
            };
        }

        private uint GetNearestFriend(Mobile player, int range)
        {
            var friend = _worldService.Mobiles
                .Where(m => m.Serial != player.Serial && _friendsService.IsFriend(m.Serial))
                .OrderBy(m => m.DistanceTo(player))
                .FirstOrDefault();
            
            return friend?.Serial ?? 0;
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
