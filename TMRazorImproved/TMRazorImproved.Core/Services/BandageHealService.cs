using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Mvvm.Messaging;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;
using TMRazorImproved.Shared.Models.Config;
using TMRazorImproved.Shared.Messages;
using TMRazorImproved.Core.Utilities;

namespace TMRazorImproved.Core.Services
{
    public class BandageHealService : AgentServiceBase, IBandageHealService, IRecipient<LoginCompleteMessage>
    {
        private readonly IPacketService _packetService;
        private readonly IWorldService _worldService;
        private readonly ITargetingService _targetingService;
        private readonly IFriendsService _friendsService;
        private readonly IMessenger _messenger;
        private readonly ILogger<BandageHealService> _logger;

        public BandageHealService(
            IPacketService packetService,
            IConfigService configService,
            IWorldService worldService,
            ITargetingService targetingService,
            IFriendsService friendsService,
            IMessenger messenger,
            IHotkeyService hotkeyService,
            ILogger<BandageHealService> logger) : base(configService)
        {
            _packetService = packetService;
            _worldService = worldService;
            _targetingService = targetingService;
            _friendsService = friendsService;
            _messenger = messenger;
            _logger = logger;

            _messenger.Register<LoginCompleteMessage>(this);

            hotkeyService.RegisterAction("BandageHeal Start", () => Start());
            hotkeyService.RegisterAction("BandageHeal Stop", () => _ = StopAsync());
            hotkeyService.RegisterAction("BandageHeal Toggle", () => { if (IsRunning) _ = StopAsync(); else Start(); });
        }

        // FR-052: AutoStart on login — enable BandageHeal if AutoStart == true
        public void Receive(LoginCompleteMessage message)
        {
            var config = _configService.CurrentProfile?.BandageHeal;
            if (config?.AutoStart == true)
            {
                config.Enabled = true;
                _logger.LogInformation("BandageHeal: AutoStart attivato al login");
            }
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
                        // FR-038: check MortalStrike via buff (buff-based) in addition to YellowHits (hue-based)
                        bool isMortalByBuff = targetSerial == player.Serial &&
                            player.ActiveBuffs.ContainsKey("Mortal Strike");
                        bool isMortal = target.IsYellowHits || isMortalByBuff;
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

                            // FR-035: text-based healing (shard commands like [bandself)
                            if (config.SendTextMsg)
                            {
                                string cmd = targetSerial == player.Serial ? config.TextMsgSelf : config.TextMsgTarget;
                                _packetService.SendToServer(PacketBuilder.UnicodeSpeech(cmd));
                                if (targetSerial != player.Serial)
                                {
                                    await Task.Delay(150, token);
                                    uint cursorId = _targetingService.PendingCursorId;
                                    _targetingService.ClearTargetCursor();
                                    _packetService.SendToServer(PacketBuilder.TargetObject(targetSerial, cursorId));
                                }
                            }
                            else
                            {
                                _packetService.SendToServer(PacketBuilder.DoubleClick(bandage.Serial));
                                await Task.Delay(150, token);
                                // FIX P0-01: usa il cursorId pendente dal server (0x6C S2C) come richiesto dal protocollo UO.
                                uint cursorId = _targetingService.PendingCursorId;
                                _targetingService.ClearTargetCursor();
                                _packetService.SendToServer(PacketBuilder.TargetObject(targetSerial, cursorId));
                            }

                            int delayMs = CalculateBandageDelay(player.Dex, config.CustomDelay);

                            // FR-036: countdown overhead display
                            if (config.ShowCountdown)
                                _ = ShowCountdownAsync(target, delayMs, token);

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
                "Friend" => GetWeakestFriend(player, config.MaxRange)?.Serial ?? 0,
                // FR-034: FriendOrSelf — heals the friend or player with the lower HP%
                "FriendOrSelf" => GetFriendOrSelfTarget(player, config.MaxRange),
                _ => player.Serial
            };
        }

        private Mobile? GetWeakestFriend(Mobile player, int range)
        {
            return _worldService.Mobiles
                .Where(m => m.Serial != player.Serial &&
                            _friendsService.IsFriend(m.Serial) &&
                            m.DistanceTo(player) <= range)
                .OrderBy(m => m.HitsMax > 0 ? (m.Hits * 100 / (int)m.HitsMax) : 100)
                .FirstOrDefault();
        }

        // FR-034: returns the serial of whoever (player or weakest in-range friend) has the lower HP%
        private uint GetFriendOrSelfTarget(Mobile player, int range)
        {
            var weakestFriend = GetWeakestFriend(player, range);
            if (weakestFriend == null) return player.Serial;

            int friendPct = weakestFriend.HitsMax > 0
                ? weakestFriend.Hits * 100 / (int)weakestFriend.HitsMax
                : 100;
            int playerPct = player.HitsMax > 0
                ? player.Hits * 100 / (int)player.HitsMax
                : 100;

            return friendPct < playerPct ? weakestFriend.Serial : player.Serial;
        }

        private int CalculateBandageDelay(ushort dex, int customDelay)
        {
            if (customDelay > 0) return customDelay;

            // FR-037: legacy formula — (11 - (dex - dex%10) / 20) * 1000, min 100ms
            int delay = (11 - (dex - dex % 10) / 20) * 1000;
            return Math.Max(100, delay);
        }

        // FR-036: shows an incrementing overhead countdown above the heal target
        private async Task ShowCountdownAsync(Mobile target, int totalMs, CancellationToken token)
        {
            int elapsed = 0;
            int tickMs = 1000;
            while (elapsed < totalMs && !token.IsCancellationRequested)
            {
                await Task.Delay(tickMs, token).ConfigureAwait(false);
                elapsed += tickMs;
                int seconds = elapsed / 1000;
                // Send overhead message to client (S→C 0xAE Unicode message above the mobile)
                _packetService.SendToClient(PacketBuilder.OverheadUnicodeSpeech(
                    seconds.ToString(), target.Serial, target.Graphic,
                    type: 0x00, hue: 0x0035));
            }
        }

        protected override void OnStopped()
        {
            _logger.LogInformation("BandageHeal agent stopped");
        }
    }
}
