using System;
using System.Buffers.Binary;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using TMRazorImproved.Shared.Enums;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models.Config;
using TMRazorImproved.Shared.Messages;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Mvvm.Messaging;
using TMRazorImproved.Core.Utilities;

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
        private readonly IWorldService _worldService;
        private readonly ILogger<FilterHandler> _logger;

        // Sound IDs dei passi in UO Classic (footstep sounds).
        // Coprono camminare su: erba/terra (0x12-0x13), pietra (0x14-0x15),
        // legno (0x16-0x17), acqua superficiale (0x18-0x1A).
        private static readonly HashSet<ushort> _footstepSoundIds = new()
        {
            0x0012, 0x0013, 0x0014, 0x0015, 0x0016, 0x0017, 0x0018, 0x0019, 0x001A
        };

        // Mappature sostitutive per filtri grafici (Originale -> Sostitutiva)
        // Usiamo Slime (0x0033) come sostituto leggero.
        private static readonly Dictionary<ushort, ushort> _dragonGraphics = new()
        {
            { 0x000C, 0x0033 }, { 0x003B, 0x0033 }
        };
        private static readonly Dictionary<ushort, ushort> _drakeGraphics = new()
        {
            { 0x003C, 0x0033 }, { 0x003D, 0x0033 }
        };
        private static readonly Dictionary<ushort, ushort> _daemonGraphics = new()
        {
            { 0x0009, 0x0033 }
        };

        public FilterHandler(IPacketService packetService, IConfigService configService, IWorldService worldService, ILogger<FilterHandler> logger, IMessenger messenger)
        {
            _packetService = packetService;
            _configService = configService;
            _worldService = worldService;
            _logger = logger;

            RegisterFilters();

            // Sottoscrizione ai cambi di configurazione per aggiornare la visualizzazione staff items/npcs
            messenger.Register<ConfigChangedMessage>(this, (r, m) =>
            {
                if (m.PropertyName == nameof(UserProfile.FilterStaffItems))
                    RefreshStaffItems();
                else if (m.PropertyName == nameof(UserProfile.FilterStaffNpcs))
                    RefreshStaffNpcs();
                else if (m.PropertyName == nameof(UserProfile.FilterLight))
                    RefreshLight();
                else if (m.PropertyName is nameof(UserProfile.FilterSeason) or nameof(UserProfile.ForcedSeason))
                    RefreshSeason();
            });
        }

        private void SendForcedSeason()
        {
            byte season = _configService.CurrentProfile.ForcedSeason;
            _packetService.SendToClient(new byte[] { 0xBC, season, 0x01 });
        }

        private void RefreshSeason()
        {
            if (_configService.CurrentProfile.FilterSeason)
                SendForcedSeason();
        }

        private void RefreshLight()
        {
            if (_configService.CurrentProfile.FilterLight)
            {
                // Forza massima luminosità inviando 0x4F 0x00 al client
                _packetService.SendToClient(new byte[] { 0x4F, 0x00 });
            }
            else
            {
                // Ripristina luminosità originale dal WorldService
                _packetService.SendToClient(new byte[] { 0x4F, _worldService.CurrentLight });
            }
        }

        private void RegisterFilters()
        {
            // ─── Filtri packet-level (blocco intero pacchetto) ────────────────────────

            // Vet Reward Gump Filter (0xB0, 0xDD)
            // Blocca il gump delle ricompense veterano se appare nel primo minuto di connessione.
            _packetService.RegisterFilter(PacketPath.ServerToClient, 0xB0, data => FilterVetRewardGump(data, isCompressed: false));
            _packetService.RegisterFilter(PacketPath.ServerToClient, 0xDD, data => FilterVetRewardGump(data, isCompressed: true));

            // Light Filter (0x4E personal light, 0x4F overall light)
            _packetService.RegisterFilter(PacketPath.ServerToClient, 0x4E, data =>
            {
                if (_configService.CurrentProfile.FilterLight)
                {
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
                    _packetService.SendToClient(new byte[] { 0x4F, 0x00 });
                    return false;
                }
                return true;
            });

            // Weather Filter (0x65)
            _packetService.RegisterFilter(PacketPath.ServerToClient, 0x65, _ => !_configService.CurrentProfile.FilterWeather);

            // Sound Filter (0x54) — FR-061
            // FilterSound = true blocca tutti i suoni; FilteredSoundIds blocca solo gli ID specificati.
            // Struttura pacchetto: [0]=id, [1]=flags, [2-3]=soundId
            _packetService.RegisterFilter(PacketPath.ServerToClient, 0x54, data =>
            {
                var profile = _configService.CurrentProfile;
                if (profile.FilterSound) return false; // blocca tutto
                if (profile.FilteredSoundIds.Count > 0 && data.Length >= 4)
                {
                    ushort soundId = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(2));
                    if (profile.FilteredSoundIds.Contains(soundId)) return false;
                }
                return true;
            });

            // Death Filter (0x2C)
            _packetService.RegisterFilter(PacketPath.ServerToClient, 0x2C, _ => !_configService.CurrentProfile.FilterDeath);

            // Season Filter (0xBC) — FR-062
            // Blocca il pacchetto stagione dal server e invia la stagione forzata dalla config.
            // Struttura pacchetto 0xBC: [0]=id, [1]=season(0-4), [2]=flag(1=sound)
            _packetService.RegisterFilter(PacketPath.ServerToClient, 0xBC, data =>
            {
                if (!_configService.CurrentProfile.FilterSeason) return true;
                SendForcedSeason();
                return false;
            });

            // Staff Items Filter (0x1A)
            _packetService.RegisterFilter(PacketPath.ServerToClient, 0x1A, FilterStaffItems);

            // Staff NPCs Filter (0x78, 0x20, 0x77)
            _packetService.RegisterFilter(PacketPath.ServerToClient, 0x78, data => FilterStaffNpcs(data, 18));
            _packetService.RegisterFilter(PacketPath.ServerToClient, 0x20, data => FilterStaffNpcs(data, 9));
            _packetService.RegisterFilter(PacketPath.ServerToClient, 0x77, data => FilterStaffNpcs(data, 15));

            // BardMusic Filter (0x6D)
            _packetService.RegisterFilter(PacketPath.ServerToClient, 0x6D, _ => !_configService.CurrentProfile.FilterBardMusic);

            // Block Trade Request (0x6F)
            _packetService.RegisterFilter(PacketPath.ServerToClient, 0x6F, data =>
            {
                if (!_configService.CurrentProfile.BlockTradeRequest) return true;
                if (data.Length < 2) return true;
                return data[1] != 0; 
            });

            // Block Party Invite (0xBF sub 0x06 type 0x07)
            _packetService.RegisterFilter(PacketPath.ServerToClient, 0xBF, data =>
            {
                if (!_configService.CurrentProfile.BlockPartyInvite) return true;
                if (data.Length < 5) return true;
                ushort sub = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(3));
                if (sub == 0x06 && data.Length >= 6)
                {
                    byte type = data[5];
                    if (type == 0x07)
                    {
                        if (data.Length >= 10)
                        {
                            uint leaderSerial = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(6));
                            DeclineParty(leaderSerial);
                        }
                        return false;
                    }
                }
                return true;
            });

            // Footsteps Filter (0x54)
            _packetService.RegisterFilter(PacketPath.ServerToClient, 0x54, data =>
            {
                if (!_configService.CurrentProfile.FilterFootsteps) return true;
                if (data.Length < 4) return true;
                ushort soundId = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(2));
                return !_footstepSoundIds.Contains(soundId);
            });

            // ASCII Message (0x1C)
            _packetService.RegisterFilter(PacketPath.ServerToClient, 0x1C, data =>
            {
                return FilterMessageContent(data, isUnicode: false);
            });

            // Unicode Message (0xAE)
            _packetService.RegisterFilter(PacketPath.ServerToClient, 0xAE, data =>
            {
                return FilterMessageContent(data, isUnicode: true);
            });

            // Localized Message Filter (0xC1) — FR-060
            // Struttura: [0]=id, [1-2]=len, [3-6]=serial, [7-8]=body, [9]=type, [10-11]=hue, [12-13]=font, [14-17]=cliloc_num
            _packetService.RegisterFilter(PacketPath.ServerToClient, 0xC1, data =>
            {
                var profile = _configService.CurrentProfile;
                if (!profile.FilterLocMessages) return true;
                if (data.Length < 18) return true;
                if (profile.FilteredClilocNumbers.Count == 0) return true;

                byte msgType = data[9];
                int clilocNum = BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(14));

                // I cliloc delle spell paladino (1060718-1060727) vengono trattati come Spell (type 0x02)
                if (clilocNum >= 1060718 && clilocNum <= 1060727)
                    msgType = 0x02; // MessageType.Spell

                return !profile.FilteredClilocNumbers.Contains(clilocNum);
            });

            // Localized Message Affix Filter (0xCC) — FR-064
            // Stesso meccanismo di 0xC1: filtra per tipo messaggio e per numero cliloc.
            // Struttura: [0]=id, [1-2]=len, [3-6]=serial, [7-8]=body, [9]=type, [10-11]=hue,
            //            [12-13]=font, [14-17]=cliloc_num, [18]=affix_flags, [19+]=name[30]+affix+args
            _packetService.RegisterFilter(PacketPath.ServerToClient, 0xCC, data =>
            {
                var profile = _configService.CurrentProfile;
                if (data.Length < 19) return true;

                byte msgType = data[9];
                if (profile.FilteredMessageTypes.Contains((OverheadMessageType)msgType)) return false;

                if (profile.FilterLocMessages && profile.FilteredClilocNumbers.Count > 0)
                {
                    int clilocNum = BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(14));
                    if (profile.FilteredClilocNumbers.Contains(clilocNum)) return false;
                }
                return true;
            });

            // Graphics Filters
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x78, ApplyMobileIncomingGraphicsFilter);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x20, ApplyMobileUpdateGraphicsFilter);

            _logger.LogInformation("Classic Razor filters registered");
        }

        private void ApplyMobileIncomingGraphicsFilter(byte[] data)
        {
            if (data.Length < 9) return;
            ushort body = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(7));
            ushort newBody = GetReplacementGraphic(body);
            if (newBody != body)
            {
                BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(7), newBody);
            }
        }

        private void ApplyMobileUpdateGraphicsFilter(byte[] data)
        {
            if (data.Length < 7) return;
            ushort body = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(5));
            ushort newBody = GetReplacementGraphic(body);
            if (newBody != body)
            {
                BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(5), newBody);
            }
        }

        private ushort GetReplacementGraphic(ushort originalBody)
        {
            var profile = _configService.CurrentProfile;

            // User custom filters (from GraphFilters page)
            foreach (var filter in profile.GraphFilters)
            {
                if (filter.Enabled && filter.RealID == originalBody)
                    return filter.NewID;
            }

            if (profile.FilterDragon && _dragonGraphics.ContainsKey(originalBody)) return profile.FilterDragonGraphic;
            if (profile.FilterDrake && _drakeGraphics.ContainsKey(originalBody)) return profile.FilterDrakeGraphic;
            if (profile.FilterDaemon && _daemonGraphics.ContainsKey(originalBody)) return profile.FilterDaemonGraphic;
            return originalBody;
        }

        private bool FilterMessageContent(byte[] data, bool isUnicode)
        {
            var profile = _configService.CurrentProfile;

            // Estraiamo il tipo di messaggio per il filtraggio granulare
            OverheadMessageType msgType;
            if (isUnicode)
            {
                if (data.Length < 10) return true;
                msgType = (OverheadMessageType)data[9];
            }
            else
            {
                if (data.Length < 4) return true;
                msgType = (OverheadMessageType)data[3];
            }

            // Filtro per tipo di messaggio (configurabile)
            if (profile.FilteredMessageTypes.Contains(msgType)) return false;

            if (!profile.FilterPoison && !profile.FilterKarmaFame && !profile.FilterSnoop) return true;

            string text;
            try
            {
                if (isUnicode)
                {
                    if (data.Length < 52) return true;
                    int byteLen = data.Length - 48;
                    if (byteLen >= 2) byteLen -= 2;
                    text = byteLen > 0 ? Encoding.BigEndianUnicode.GetString(data, 48, byteLen).TrimEnd('\0') : string.Empty;
                }
                else
                {
                    if (data.Length < 46) return true;
                    int byteLen = data.Length - 44;
                    text = Encoding.ASCII.GetString(data, 44, byteLen).TrimEnd('\0');
                }
            }
            catch { return true; }

            if (string.IsNullOrEmpty(text)) return true;

            if (profile.FilterPoison && (text.Contains("poison", StringComparison.OrdinalIgnoreCase) || text.Contains("veleno", StringComparison.OrdinalIgnoreCase))) return false;
            if (profile.FilterKarmaFame && (text.Contains("fame", StringComparison.OrdinalIgnoreCase) || text.Contains("karma", StringComparison.OrdinalIgnoreCase) || text.Contains("notoriety", StringComparison.OrdinalIgnoreCase))) return false;
            if (profile.FilterSnoop && (text.Contains("snoop", StringComparison.OrdinalIgnoreCase) || text.Contains("peek", StringComparison.OrdinalIgnoreCase))) return false;

            return true;
        }

        private void DeclineParty(uint leaderSerial)
        {
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

        private bool FilterVetRewardGump(byte[] data, bool isCompressed)
        {
            if (!_configService.CurrentProfile.FilterVetRewardGump) return true;
            if (_worldService.ConnectionStart != DateTime.MinValue && _worldService.ConnectionStart.AddMinutes(1) < DateTime.UtcNow) return true;

            try
            {
                string layout = string.Empty;
                if (isCompressed)
                {
                    if (data.Length < 27) return true;
                    int cLen = BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(19));
                    int layoutCompressedSize = cLen - 4;
                    if (layoutCompressedSize <= 0 || data.Length < 27 + layoutCompressedSize) return true;
                    using var ms = new MemoryStream(data, 27, layoutCompressedSize);
                    using var zlib = new ZLibStream(ms, CompressionMode.Decompress);
                    using var sr = new StreamReader(zlib, Encoding.ASCII);
                    layout = sr.ReadToEnd();
                }
                else
                {
                    if (data.Length < 21) return true;
                    int layoutLen = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(19));
                    if (layoutLen <= 0 || data.Length < 21 + layoutLen) return true;
                    layout = Encoding.ASCII.GetString(data, 21, layoutLen);
                }

                if (string.IsNullOrEmpty(layout)) return true;
                var numbers = Regex.Split(layout, @"\D+");
                foreach (string val in numbers)
                {
                    if (!string.IsNullOrEmpty(val) && int.TryParse(val, out int i) && i == 1006046) return false;
                }
            }
            catch (Exception ex) { _logger.LogWarning(ex, "Error filtering VetRewardGump"); }
            return true;
        }

        private bool FilterStaffItems(byte[] data)
        {
            if (!_configService.CurrentProfile.FilterStaffItems) return true;
            if (data.Length < 9) return true;
            ushort itemId = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(7));
            ushort baseId = (ushort)(itemId & 0x3FFF);
            if (baseId == 0x36FF || baseId == 0x1183) return false;
            return true;
        }

        private bool FilterStaffNpcs(byte[] data, int flagsOffset)
        {
            if (!_configService.CurrentProfile.FilterStaffNpcs) return true;
            if (data.Length <= flagsOffset) return true;
            uint serial = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(1)) & 0x7FFFFFFF;
            if (_worldService.Player != null && serial == _worldService.Player.Serial) return true;
            if ((data[flagsOffset] & 0x80) != 0) return false;
            return true;
        }

        private void RefreshStaffItems()
        {
            bool filterEnabled = _configService.CurrentProfile.FilterStaffItems;
            foreach (var item in _worldService.Items)
            {
                ushort baseId = (ushort)(item.Graphic & 0x3FFF);
                if (baseId == 0x36FF || baseId == 0x1183)
                {
                    if (filterEnabled) _packetService.SendToClient(PacketBuilder.RemoveObject(item.Serial));
                    else _packetService.SendToClient(PacketBuilder.WorldItem(item.Serial, item.Graphic, item.Amount, (ushort)item.X, (ushort)item.Y, (sbyte)item.Z, item.Hue, item.Flags));
                }
            }
        }

        private void RefreshStaffNpcs()
        {
            bool filterEnabled = _configService.CurrentProfile.FilterStaffNpcs;
            foreach (var m in _worldService.Mobiles)
            {
                if (_worldService.Player != null && m.Serial == _worldService.Player.Serial) continue;
                if ((m.Flags & 0x80) != 0)
                {
                    if (filterEnabled) _packetService.SendToClient(PacketBuilder.RemoveObject(m.Serial));
                    else _packetService.SendToClient(PacketBuilder.MobileUpdate(m.Serial, m.Graphic, m.Hue, m.Flags, (ushort)m.X, (ushort)m.Y, (sbyte)m.Z, m.Direction));
                }
            }
        }
    }
}
