using System;
using System.Buffers.Binary;
using System.Text;
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

        // Sound IDs dei passi in UO Classic (footstep sounds).
        // Coprono camminare su: erba/terra (0x12-0x13), pietra (0x14-0x15),
        // legno (0x16-0x17), acqua superficiale (0x18-0x1A).
        private static readonly System.Collections.Generic.HashSet<ushort> _footstepSoundIds = new()
        {
            0x0012, 0x0013, 0x0014, 0x0015, 0x0016, 0x0017, 0x0018, 0x0019, 0x001A
        };

        public FilterHandler(IPacketService packetService, IConfigService configService, ILogger<FilterHandler> logger)
        {
            _packetService = packetService;
            _configService = configService;
            _logger = logger;

            RegisterFilters();
        }

        private void RegisterFilters()
        {
            // ─── Filtri packet-level (blocco intero pacchetto) ────────────────────────

            // Light Filter (0x4E personal light, 0x4F overall light)
            _packetService.RegisterFilter(PacketPath.ServerToClient, 0x4E, data =>
            {
                if (_configService.CurrentProfile.FilterLight)
                {
                    // If we block personal light, we can also force it to max (0) or just let the global handle it.
                    // Personal light packet: 0x4E cmd(1) serial(4) level(1)
                    if (data.Length >= 6)
                    {
                        byte[] maxLight = new byte[6];
                        Array.Copy(data, maxLight, 5);
                        maxLight[5] = 0x00; // max brightness
                        _packetService.SendToClient(maxLight);
                    }
                    return false;
                }
                return true;
            });

            _packetService.RegisterFilter(PacketPath.ServerToClient, 0x4F, data =>
            {
                if (_configService.CurrentProfile.FilterLight)
                {
                    // Global light packet: 0x4F cmd(1) level(1)
                    _packetService.SendToClient(new byte[] { 0x4F, 0x00 });
                    return false;
                }
                return true;
            });

            // Weather Filter (0x65)
            _packetService.RegisterFilter(PacketPath.ServerToClient, 0x65, _ => !_configService.CurrentProfile.FilterWeather);

            // Sound Filter (0x54) — blocca TUTTI i suoni
            _packetService.RegisterFilter(PacketPath.ServerToClient, 0x54, _ => !_configService.CurrentProfile.FilterSound);

            // Death Filter (0x2C death animation)
            _packetService.RegisterFilter(PacketPath.ServerToClient, 0x2C, _ => !_configService.CurrentProfile.FilterDeath);

            // Season Filter (0xBC)
            _packetService.RegisterFilter(PacketPath.ServerToClient, 0xBC, _ => !_configService.CurrentProfile.FilterSeason);

            // BardMusic Filter (0x6D PlayMusic) — blocca tutta la musica del bardo/ambient
            _packetService.RegisterFilter(PacketPath.ServerToClient, 0x6D, _ => !_configService.CurrentProfile.FilterBardMusic);

            // Block Trade Request (0x6F)
            _packetService.RegisterFilter(PacketPath.ServerToClient, 0x6F, data =>
            {
                if (!_configService.CurrentProfile.BlockTradeRequest) return true;
                if (data.Length < 2) return true;
                // action 0 = Start. We only block the start request
                return data[1] != 0; 
            });

            // Block Party Invite (0xBF sub 0x06 type 0x07)
            _packetService.RegisterFilter(PacketPath.ServerToClient, 0xBF, data =>
            {
                if (!_configService.CurrentProfile.BlockPartyInvite) return true;
                if (data.Length < 5) return true;
                
                // 0xBF: cmd(1) len(2) sub(2) + dati dipendenti dal sub-command
                ushort sub = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(3));
                if (sub == 0x06 && data.Length >= 6) // Party Message
                {
                    byte type = data[5];
                    if (type == 0x07) // Party Invite
                    {
                        if (data.Length >= 10)
                        {
                            uint leaderSerial = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(6));
                            DeclineParty(leaderSerial);
                        }
                        return false; // Blocca il pacchetto verso il client
                    }
                }
                return true;
            });

            // Footsteps Filter (0x54) — blocca solo i sound ID riconosciuti come passi.
            // Nota: FilterSound già blocca tutto il 0x54; questo filtro agisce in modo
            // additivo quando FilterSound=false ma FilterFootsteps=true.
            _packetService.RegisterFilter(PacketPath.ServerToClient, 0x54, data =>
            {
                if (!_configService.CurrentProfile.FilterFootsteps) return true;
                if (data.Length < 4) return true;
                // 0x54: [0] cmd [1] flags [2-3] soundId (big-endian)
                ushort soundId = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(2));
                return !_footstepSoundIds.Contains(soundId);
            });

            // ─── Filtri content-level su messaggi ASCII (0x1C) ───────────────────────
            // Blocca Poison / KarmaFame / Snoop in base al testo del messaggio.
            // 0x1C layout: [0] cmd [1-2] len [3-6] serial [7-8] graphic
            //              [9] type [10-11] hue [12-13] font
            //              [14-43] name (30 ASCII, null-padded) [44+] text (ASCII null-term)
            _packetService.RegisterFilter(PacketPath.ServerToClient, 0x1C, data =>
            {
                return FilterMessageContent(data, isUnicode: false);
            });

            // ─── Filtri content-level su messaggi Unicode (0xAE) ────────────────────
            // 0xAE layout: [0] cmd [1-2] len [3-6] serial [7-8] graphic
            //              [9] type [10-11] hue [12-13] font [14-17] lang (4 ASCII)
            //              [18-47] name (30 ASCII, null-padded) [48+] text (UTF-16 BE null-term)
            _packetService.RegisterFilter(PacketPath.ServerToClient, 0xAE, data =>
            {
                return FilterMessageContent(data, isUnicode: true);
            });

            _logger.LogInformation("Classic Razor filters registered");
        }

        /// <summary>
        /// Applica i filtri content-based (Poison, KarmaFame, Snoop) a un messaggio.
        /// Ritorna true = lascia passare il pacchetto; false = blocca.
        /// </summary>
        private bool FilterMessageContent(byte[] data, bool isUnicode)
        {
            var profile = _configService.CurrentProfile;

            // Se nessun filtro content-based è attivo, passa subito
            if (!profile.FilterPoison && !profile.FilterKarmaFame && !profile.FilterSnoop)
                return true;

            string text;
            try
            {
                if (isUnicode)
                {
                    // Testo UTF-16 BE da offset 48
                    if (data.Length < 52) return true; // troppo corto
                    int byteLen = data.Length - 48;
                    // Rimuovi null terminator (ultimi 2 byte)
                    if (byteLen >= 2) byteLen -= 2;
                    text = byteLen > 0
                        ? Encoding.BigEndianUnicode.GetString(data, 48, byteLen).TrimEnd('\0')
                        : string.Empty;
                }
                else
                {
                    // Testo ASCII da offset 44
                    if (data.Length < 46) return true; // troppo corto
                    int byteLen = data.Length - 44;
                    text = Encoding.ASCII.GetString(data, 44, byteLen).TrimEnd('\0');
                }
            }
            catch
            {
                return true; // parsing fallito → lascia passare
            }

            if (string.IsNullOrEmpty(text)) return true;

            // Poison: "poison", "veleno", "veneno", "gift" (UO uses "poisoned")
            if (profile.FilterPoison &&
                (text.Contains("poison", StringComparison.OrdinalIgnoreCase) ||
                 text.Contains("veleno", StringComparison.OrdinalIgnoreCase)))
                return false;

            // KarmaFame: messaggi di variazione fama/karma
            if (profile.FilterKarmaFame &&
                (text.Contains("fame", StringComparison.OrdinalIgnoreCase) ||
                 text.Contains("karma", StringComparison.OrdinalIgnoreCase) ||
                 text.Contains("notoriety", StringComparison.OrdinalIgnoreCase)))
                return false;

            // Snoop: messaggi relativi al peek/snoop su borse altrui
            if (profile.FilterSnoop &&
                (text.Contains("snoop", StringComparison.OrdinalIgnoreCase) ||
                 text.Contains("peek", StringComparison.OrdinalIgnoreCase)))
                return false;

            return true;
        }

        private void DeclineParty(uint leaderSerial)
        {
            // 0xBF sub 0x06 type 0x09: Decline Party
            // length = 10 (0xBF 1 0x000A 0x0006 0x09 leader(4))
            byte[] response = new byte[10];
            response[0] = 0xBF;
            response[1] = 0x00;
            response[2] = 0x0A;
            response[3] = 0x00;
            response[4] = 0x06;
            response[5] = 0x09;
            BinaryPrimitives.WriteUInt32BigEndian(response.AsSpan(6), leaderSerial);
            
            _packetService.SendToServer(response);
        }
    }
}
