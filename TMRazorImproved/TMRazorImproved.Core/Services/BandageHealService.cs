using System;
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
    public class BandageHealService : AgentServiceBase, IBandageHealService
    {
        private readonly IPacketService _packetService;
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
            ILogger<BandageHealService> logger) : base(configService)
        {
            _packetService = packetService;
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

                if (player != null)
                {
                    // 006-A: cerca la bandage nel backpack per ItemID invece di usare serial fisso
                    var bandage = FindBandage(player, config);
                    if (bandage == null)
                    {
                        _logger.LogTrace("BandageHeal: no bandage found in backpack");
                        await Task.Delay(500, token);
                        continue;
                    }

                    // HiddenStop
                    if (config.HiddenStop && player.IsHidden)
                    {
                        await Task.Delay(200, token);
                        continue;
                    }

                    // PoisonBlock
                    if (config.PoisonBlock && player.IsPoisoned)
                    {
                        await Task.Delay(200, token);
                        continue;
                    }

                    // MortalBlock
                    if (config.MortalBlock && player.IsYellowHits)
                    {
                        await Task.Delay(200, token);
                        continue;
                    }

                    // 006-B: se TimeWithBuff è attivo, aspetta che il buff "Healing" non sia più presente
                    if (config.TimeWithBuff && IsBandagingActive(player))
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

                            _packetService.SendToServer(PacketBuilder.DoubleClick(bandage.Serial));
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

        private const ushort BandageGraphic = 0x0E21;

        /// <summary>Cerca la bandage nel backpack del giocatore per ItemID.</summary>
        private Item? FindBandage(Mobile player, BandageHealConfig config)
        {
            uint backpackSerial = player.Backpack?.Serial ?? 0;
            if (backpackSerial == 0) return null;

            ushort targetGraphic = config.UseCustomBandage && config.CustomBandageID > 0
                ? (ushort)config.CustomBandageID
                : BandageGraphic;

            return _worldService.Items.FirstOrDefault(i =>
                i.Container == backpackSerial &&
                i.Graphic == targetGraphic &&
                (config.UseCustomBandage == false || config.CustomBandageColor == -1 || config.CustomBandageColor == 0 || i.Hue == config.CustomBandageColor));
        }

        /// <summary>Verifica se il bandaging è ancora in corso controllando il buff "Healing".</summary>
        private static bool IsBandagingActive(Mobile player)
        {
            lock (player.ActiveBuffs)
            {
                return player.ActiveBuffs.ContainsKey("Healing");
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

            // Formula UO suggerita in Final Review: base 8 secondi, -1 sec ogni 20 DEX (min 3 sec)
            return Math.Max(3000, 8000 - (dex / 20) * 1000);
        }

        protected override void OnStopped()
        {
            _logger.LogInformation("BandageHeal agent stopped");
        }
    }
}
