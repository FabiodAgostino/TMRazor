using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;
using TMRazorImproved.Shared.Models.Config;
using System.Buffers.Binary;

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
                    bool needsHeal = (player.Hits * 100 / Math.Max(1, (int)player.HitsMax)) <= config.HpStart;
                    bool isPoisoned = player.IsPoisoned;

                    if (needsHeal || (isPoisoned && config.PoisonPriority))
                    {
                        // TODO: Verificare se siamo hidden e se HiddenStop è attivo
                        
                        _logger.LogDebug("Heal triggered. Hits: {Hits}/{Max}, Poisoned: {Poisoned}", player.Hits, player.HitsMax, isPoisoned);

                        // 1. Double click sulle bende (0x06)
                        SendDoubleClick(config.BandageSerial);

                        // 2. Attendiamo un attimo che il server elabori il double click e chieda il target
                        // In una versione più avanzata, aspetteremmo il pacchetto 0xAA (Target Request)
                        await Task.Delay(150, token);

                        // 3. Target sul giocatore (self) (0x6C)
                        SendTargetSelf(player.Serial);

                        // 4. Calcolo del delay della benda
                        int delayMs = CalculateBandageDelay(player.Dex, config.CustomDelay);
                        _logger.LogTrace("Bandage applied. Waiting {Delay}ms", delayMs);

                        // Aspettiamo che la benda finisca prima di riprovare
                        await Task.Delay(delayMs, token);
                    }
                }

                await Task.Delay(200, token);
            }
        }

        private int CalculateBandageDelay(ushort dex, int customDelay)
        {
            if (customDelay > 0) return customDelay;

            // Formula classica UO: (11 - (DEX / 20)) secondi
            // Minimo 2 secondi (es. bende veloci su shard non-OSI)
            double seconds = 11.0 - (dex / 20.0);
            if (seconds < 2.0) seconds = 2.0;

            return (int)(seconds * 1000);
        }

        private void SendDoubleClick(uint serial)
        {
            byte[] packet = new byte[5];
            packet[0] = 0x06;
            BinaryPrimitives.WriteUInt32BigEndian(packet.AsSpan(1), serial);
            _packetService.SendToServer(packet);
        }

        private void SendTargetSelf(uint playerSerial)
        {
            // Pacchetto 0x6C: Target
            // [0] 0x6C
            // [1] Target Type (0=Cursor, 1=Object, 2=Ground)
            // [2-5] Target ID (0x00000001 per cursor?)
            // [6] Cursor Action (0=Target, 1=Cancel)
            // [7-10] Serial dell'oggetto target
            // [11-12] X
            // [13-14] Y
            // [15] Z
            // [16-17] Graphic (ItemID)

            byte[] packet = new byte[19];
            packet[0] = 0x6C;
            packet[1] = 0x01; // Object
            BinaryPrimitives.WriteUInt32BigEndian(packet.AsSpan(2), 0); // Cursor ID (ignoro per ora)
            packet[6] = 0x00; // Action: Target
            BinaryPrimitives.WriteUInt32BigEndian(packet.AsSpan(7), playerSerial);
            BinaryPrimitives.WriteUInt16BigEndian(packet.AsSpan(11), 0); // X
            BinaryPrimitives.WriteUInt16BigEndian(packet.AsSpan(13), 0); // Y
            packet[15] = 0; // Z
            BinaryPrimitives.WriteUInt16BigEndian(packet.AsSpan(16), 0); // Graphic

            _packetService.SendToServer(packet);
        }

        protected override void OnStopped()
        {
            _logger.LogInformation("BandageHeal agent stopped");
        }
    }
}
