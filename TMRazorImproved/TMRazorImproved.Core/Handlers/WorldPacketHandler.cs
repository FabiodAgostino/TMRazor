using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using TMRazorImproved.Core.Utilities;
using TMRazorImproved.Shared.Enums;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Messages;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.Core.Handlers
{
    public class WorldPacketHandler : IRecipient<StartPingRequestMessage>
    {
        private readonly IPacketService _packetService;
        private readonly IWorldService _worldService;
        private readonly IJournalService _journalService;
        private readonly ILanguageService _languageService;
        private readonly IFriendsService _friendsService;
        private readonly IConfigService _configService;
        private readonly IScreenCaptureService _screenCapture;
        private readonly ITargetingService _targetingService;
        private readonly IMultiService _multiService;
        private readonly IMessenger _messenger;

        private readonly ConcurrentDictionary<byte, long> _sentPings = new();
        private int _pingsRemaining = 0;

        public WorldPacketHandler(
            IPacketService packetService,
            IWorldService worldService,
            IJournalService journalService,
            ILanguageService languageService,
            IFriendsService friendsService,
            IConfigService configService,
            IScreenCaptureService screenCapture,
            ITargetingService targetingService,
            IMultiService multiService,
            IMessenger messenger)
        {
            _packetService = packetService;
            _worldService = worldService;
            _journalService = journalService;
            _languageService = languageService;
            _friendsService = friendsService;
            _configService = configService;
            _screenCapture = screenCapture;
            _targetingService = targetingService;
            _multiService = multiService;
            _messenger = messenger;
            _messenger.RegisterAll(this);

            RegisterHandlers();
        }

        public void Receive(StartPingRequestMessage message)
        {
            _pingsRemaining = Math.Max(1, Math.Min(20, message.Value));
            _sentPings.Clear();
            SendPing();
        }

        private void SendPing()
        {
            if (_pingsRemaining <= 0) return;

            _pingsRemaining--;
            byte seq;
            do { seq = (byte)Random.Shared.Next(256); } while (_sentPings.ContainsKey(seq));

            _sentPings[seq] = Stopwatch.GetTimestamp();
            _packetService.SendToServer(new byte[] { 0x73, seq });
        }

        private void MorphGraphic(ref ushort graphic, ref ushort hue)
        {
            if (_configService.CurrentProfile == null) return;

            // 1. Static Fields
            if (_configService.CurrentProfile.StaticFields)
            {
                switch (graphic)
                {
                    case 0x0080: case 0x0082: // Wall of Stone
                        graphic = 0x1363; hue = 0x3B1; break;
                    case 0x3996: case 0x398C: // Fire Field
                        graphic = 0x28A8; hue = 0x0845; break;
                    case 0x3915: case 0x3920: case 0x3922: // Poison Field
                        graphic = 0x28A8; hue = 0x016A; break;
                    case 0x3967: case 0x3979: // Paralyze Field
                        graphic = 0x28A8; hue = 0x00DA; break;
                    case 0x3946: case 0x3956: // Energy Field
                        graphic = 0x28A8; hue = 0x0125; break;
                }
            }

            // 2. Custom Graph Filters
            ushort localGraphic = graphic;
            var custom = _configService.CurrentProfile.GraphFilters.FirstOrDefault(f => f.Enabled && f.RealID == localGraphic);
            if (custom != null)
            {
                graphic = custom.NewID;
                if (custom.NewHue != -1) hue = (ushort)custom.NewHue;
            }
        }

        // Sent once after the first SetPlayer to request current stats from the server.
        private bool _playerStatusRequested = false;

        private void RegisterHandlers()
        {
            // ── Login / Session ─────────────────────────────────────────────────────────
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x1B, HandleLoginConfirm);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x55, HandleLoginComplete);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x8C, HandleRelayServer);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x73, HandlePing);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0xBD, HandleClientVersion);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0xBF, HandleExtendedPacket);

            // ── Messages & Journal ───────────────────────────────────────────────────────
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x1C, HandleAsciiMessage);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0xAE, HandleUnicodeMessage);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0xAD, HandleEncodedUnicodeSpeech);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0xAF, HandleDeathAnimation);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0xC1, HandleLocalizedMessage);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0xCC, HandleLocalizedMessageAffix);

            // ── Mobiles ──────────────────────────────────────────────────────────────────
            _packetService.RegisterFilter(PacketPath.ServerToClient, 0x78, HandleMobileIncoming);
            _packetService.RegisterFilter(PacketPath.ServerToClient, 0x20, HandleMobileUpdate);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x77, HandleMobileMoving);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x98, HandleMobileName);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x88, HandleOpenPaperdoll);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x16, HandleSAMobileStatus);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x17, HandleNewMobileStatus);

            // ── Items ─────────────────────────────────────────────────────────────────────
            _packetService.RegisterFilter(PacketPath.ServerToClient, 0x1A, HandleWorldItem);
            _packetService.RegisterFilter(PacketPath.ServerToClient, 0xF3, HandleSAWorldItem);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x1D, HandleRemoveObject);

            // ── Movement ──────────────────────────────────────────────────────────────────
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x21, HandleWalkReject);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x22, HandleMovementAck);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x97, HandleMovementDemand);

            // ── Container & Inventory ─────────────────────────────────────────────────────
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x3C, HandleContainerContent);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x25, HandleAddItemToContainer);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x2E, HandleEquipUpdate);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x89, HandleCorpseEquipment);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x24, HandleBeginContainerContent);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x27, HandleLiftReject);

            // ── Stats & Skills ─────────────────────────────────────────────────────────────
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x11, HandleMobileStatus);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0xA1, HandleHitsUpdate);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0xA2, HandleManaUpdate);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0xA3, HandleStaminaUpdate);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x2D, HandleMobileStatInfo);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x3A, HandleSkillsUpdate);

            // ── Combat ────────────────────────────────────────────────────────────────────
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x0B, HandleDamage);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x72, HandleWarMode);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0xAA, HandleAttackOK);

            // ── Targeting ─────────────────────────────────────────────────────────────────
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x6C, HandleTargetCursorFromServer);

            // ── Gumps ─────────────────────────────────────────────────────────────────────
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0xB0, HandleGump);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0xDD, HandleCompressedGump);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x7C, HandleOpenMenu);
            _packetService.RegisterViewer(PacketPath.ClientToServer, 0xB1, HandleGumpResponse);

            // ── Trade ─────────────────────────────────────────────────────────────────────
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x6F, HandleTradeRequest);
            _packetService.RegisterViewer(PacketPath.ClientToServer, 0x6F, HandleTradeRequestC2S);

            // ── Vendor ────────────────────────────────────────────────────────────────────
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x74, HandleBuyWindow);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x9E, HandleSellWindow);

            // ── Effects & Audio ───────────────────────────────────────────────────────────
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0xC0, HandleGraphicalEffect);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x6D, HandlePlayMusic);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x54, HandlePlaySoundEffect);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x65, HandleWeather);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0xDF, HandleBuffDebuff);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0xE2, HandleTestAnimation);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x6E, HandleCharacterAnimation);

            // ── Filters ───────────────────────────────────────────────────────────────────
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x4E, HandlePersonalLightLevel);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x4F, HandleGlobalLightLevel);

            // ── Map & Posizione ───────────────────────────────────────────────────────────
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x90, HandleMapDisplay);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0xF5, HandleMapDisplay);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x76, HandleServerChange);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0xF6, HandleMoveBoat);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x56, HandlePinLocation);

            // ── OPL (Object Property List) ────────────────────────────────────────────────
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0xD6, HandleOPL);

            // ── Game State ────────────────────────────────────────────────────────────────
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x2C, HandlePlayerDeath);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x95, HandleHueResponse);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x9A, HandleAsciiPrompt);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0xA8, HandleServerList);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0xAB, HandleDisplayStringQuery);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0xB8, HandleProfile);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0xB9, HandleFeatures);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0xBA, HandleTrackingArrow);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0x83, HandleDeleteCharacter);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0xBC, HandleChangeSeason);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0xC8, HandleSetUpdateRange);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0xD8, HandleCustomHouseInfo);
            _packetService.RegisterViewer(PacketPath.ServerToClient, 0xF0, HandleRunUOProtocol);

            // ── C2S Viewers ───────────────────────────────────────────────────────────────
            _packetService.RegisterViewer(PacketPath.ClientToServer, 0x02, HandleMovementRequest);
            _packetService.RegisterViewer(PacketPath.ClientToServer, 0x05, HandleAttackRequest);
            _packetService.RegisterViewer(PacketPath.ClientToServer, 0x06, HandleClientDoubleClick);
            _packetService.RegisterViewer(PacketPath.ClientToServer, 0x07, HandleLiftRequest);
            _packetService.RegisterViewer(PacketPath.ClientToServer, 0x08, HandleDropRequest);
            _packetService.RegisterViewer(PacketPath.ClientToServer, 0x09, HandleClientSingleClick);
            _packetService.RegisterViewer(PacketPath.ClientToServer, 0x12, HandleClientTextCommand);
            _packetService.RegisterViewer(PacketPath.ClientToServer, 0x13, HandleEquipRequest);
            _packetService.RegisterViewer(PacketPath.ClientToServer, 0x75, HandleRenameMobile);
            _packetService.RegisterViewer(PacketPath.ClientToServer, 0xBF, HandleExtendedClientCommand);
            _packetService.RegisterViewer(PacketPath.ClientToServer, 0xD7, HandleClientEncodedPacket);
        }

        // ─────────────────────────────────────────────────────────────────────────────────
        #region Login & Session Handlers
        // ─────────────────────────────────────────────────────────────────────────────────

        private void HandleLoginConfirm(byte[] data)
        {
            // 0x1B: cmd(1) serial(4) body(4?) x(2) y(2) z(2) map(1) ...
            if (data.Length < 5) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte(); // 0x1B
            uint serial = reader.ReadUInt32();

            // Reuse the existing mobile object so HP/stats already accumulated are preserved.
            // Creating a new Mobile would break the reference shared with _worldService._mobiles.
            var player = _worldService.FindMobile(serial);
            if (player == null)
            {
                player = new Mobile(serial);
                _worldService.AddMobile(player);
            }

            bool wasNull = _worldService.Player == null;
            _worldService.SetPlayer(player);

            // On first SetPlayer, request current stats via 0x34 so the UI populates immediately.
            // wasNull guard prevents spamming (synthetic 0x1B fires every ~100ms).
            if (wasNull && !_playerStatusRequested)
            {
                _playerStatusRequested = true;
                System.Diagnostics.Trace.WriteLine($"[WorldHandler] 0x1B — serial=0x{serial:X8}, requesting stats+skills (0x34)");
                // Request player status (type=0x04 → server replies with 0x11)
                _packetService.SendToServer(new byte[] { 0x34, 0xED, 0xED, 0xED, 0xED, 0x04,
                    (byte)(serial >> 24), (byte)(serial >> 16), (byte)(serial >> 8), (byte)serial });
                // Request skills (type=0x05 → server replies with 0x3A on servers that support it)
                _packetService.SendToServer(new byte[] { 0x34, 0xED, 0xED, 0xED, 0xED, 0x05,
                    (byte)(serial >> 24), (byte)(serial >> 16), (byte)(serial >> 8), (byte)serial });
            }
        }

        private void HandleLoginComplete(byte[] data)
        {
            // 0x55: cmd(1) — nessun dato aggiuntivo
            _messenger.Send(new LoginCompleteMessage());
        }

        private void HandleRelayServer(byte[] data)
        {
            // 0x8C: cmd(1) ip(4) port(2) key(4) — totale 11 byte
            // Il login server reindirizza il client al game server.
            // La chiave è il seed di cifratura per la nuova connessione (rilevante per OSI).
            if (data.Length < 11)
            {
                Trace.WriteLine($"[WorldHandler] 0x8C RelayServer — pacchetto troppo corto ({data.Length} byte)");
                return;
            }

            string ip = $"{data[1]}.{data[2]}.{data[3]}.{data[4]}";
            ushort port = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(5));
            uint encryptionKey = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(7));

            _configService.SetCurrentShard($"{ip}:{port}");

            // Notifica tutti i servizi interessati (es. adapter OSI per re-inizializzare cifratura)
            _messenger.Send(new RelayServerMessage(ip, port, encryptionKey));

            // Log visibile nel journal per diagnostica
            _journalService.AddEntry(new JournalEntry(
                $"[Relay] → {ip}:{port} (key: 0x{encryptionKey:X8})", "System", 0, 0x3B2));

            Trace.WriteLine($"[WorldHandler] 0x8C RelayServer → {ip}:{port} key=0x{encryptionKey:X8}");
            _multiService.Clear();
            _worldService.Clear();
        }

        private void HandlePing(byte[] data)
        {
            // 0x73: cmd(1) seq(1) — il server fa ping, rispondiamo
            if (data.Length < 2) return;
            
            byte seq = data[1];

            if (_sentPings.TryRemove(seq, out long sentTimestamp))
            {
                long now = Stopwatch.GetTimestamp();
                double ms = (double)(now - sentTimestamp) * 1000.0 / Stopwatch.Frequency;
                
                _worldService.UpdatePing(ms);

                if (_pingsRemaining > 0)
                {
                    SendPing();
                }
            }
            else
            {
                // Server-initiated ping: respond with the same sequence
                byte[] response = new byte[] { 0x73, seq };
                _packetService.SendToServer(response);
            }
        }

        private void HandleClientVersion(byte[] data)
        {
            // 0xBD: cmd(1) len(2) version_string(var) — il server chiede versione client (gestita dal client)
        }

        private void HandleExtendedPacket(byte[] data)
        {
            // 0xBF: cmd(1) len(2) sub(2) + dati dipendenti dal sub-command
            if (data.Length < 5) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();       // 0xBF
            reader.ReadUInt16();     // length
            ushort sub = reader.ReadUInt16();

            switch (sub)
            {
                case 0x04: // Close Generic Gump — server chiude un gump aperto
                    // sub(2) gumpTypeSerial(2) gumpId(2)  – nota: in alcuni server sono 4 byte ognuno
                    uint closedGumpId = _worldService.CurrentGump?.GumpId ?? 0;
                    _worldService.RemoveGump();
                    _messenger.Send(new GumpClosedMessage(closedGumpId));
                    break;

                case 0x06: // Party Messages
                    HandlePartyMessage(reader);
                    break;

                case 0x08: // Map Change (facet / map switch)
                    if (reader.Remaining >= 1 && _worldService.Player != null)
                    {
                        byte mapId = reader.ReadByte();
                        lock (_worldService.Player.SyncRoot)
                        {
                            _worldService.Player.MapId = mapId;
                        }
                    }
                    break;

                case 0x10: // Equip Info (attributi arma/armatura visibili)
                    // Formato complesso: serial(4) info(4) owner(4) [name?] [attribs...]
                    // Solo tracking — non modifica world state principale
                    break;

                case 0x14: // Context Menu
                    if (reader.Remaining >= 6)
                    {
                        reader.ReadByte();                  // unknown (always 0x00)
                        byte ctxFmt    = reader.ReadByte(); // 0x01=2D, 0x02=KR/SA
                        uint ctxSerial = reader.ReadUInt32();
                        var  ctxList   = new List<ContextMenuEntry>();
                        if (reader.Remaining >= 2)
                        {
                            ushort ctxCount = reader.ReadUInt16();
                            for (int ci = 0; ci < ctxCount; ci++)
                            {
                                if (ctxFmt == 0x01) // 2D
                                {
                                    if (reader.Remaining < 6) break;
                                    ushort clilocId = reader.ReadUInt16();
                                    ushort flags    = reader.ReadUInt16();
                                    ushort respId   = reader.ReadUInt16();
                                    if ((flags & 0x20) != 0 && reader.Remaining >= 2)
                                        reader.ReadUInt16(); // optional hue
                                    string text = _languageService.GetCliloc(clilocId);
                                    ctxList.Add(new ContextMenuEntry(respId, text));
                                }
                                else // 0x02 KR/SA
                                {
                                    if (reader.Remaining < 6) break;
                                    uint   clilocId = reader.ReadUInt32();
                                    ushort flags    = reader.ReadUInt16();
                                    string text     = _languageService.GetCliloc((int)clilocId);
                                    ctxList.Add(new ContextMenuEntry(ci, text));
                                }
                            }
                        }
                        ContextMenuStore.Set(ctxSerial, ctxList);
                        _messenger.Send(new ContextMenuMessage(ctxSerial));
                    }
                    break;

                case 0x18: // Map Patches
                    if (reader.Remaining >= 4 && _worldService.Player != null)
                    {
                        // Solo tracking — count × 2 interi per patch
                        // (non memorizziamo map patches nel Mobile per ora)
                    }
                    break;

                case 0x19: // Extended Stats (AOS+) — resistenze, luck, danni, follower, tithing
                    HandleExtendedStats(reader);
                    break;

                case 0x1D: // Custom House "General Info" — revisione house
                    if (reader.Remaining >= 8)
                    {
                        uint houseSerial = reader.ReadUInt32();
                        int revision = reader.ReadInt32();
                        // revisione house: non tracciamo nel modello standard
                    }
                    break;

                case 0x21: // Special Ability Execute — il giocatore ha usato una SA
                    // Nessun aggiornamento world state: event notification
                    break;

                case 0x25: // Toggle Special Moves (SA icons rosso/bianco)
                    if (reader.Remaining >= 3)
                    {
                        ushort skillId = reader.ReadUInt16();
                        byte action = reader.ReadByte();
                        // 0x01 = abilita (rosso), 0x00 = disabilita (bianco)
                    }
                    break;
            }
        }

        private void HandlePartyMessage(UOBufferReader reader)
        {
            // 0xBF sub 0x06 — messaggi di party
            if (reader.Remaining < 1) return;
            byte type = reader.ReadByte();

            switch (type)
            {
                case 0x01: // Party list
                    if (reader.Remaining >= 1)
                    {
                        byte count = reader.ReadByte();
                        _worldService.ClearParty();
                        for (int i = 0; i < count && reader.Remaining >= 4; i++)
                        {
                            uint memberSerial = reader.ReadUInt32();
                            _worldService.AddPartyMember(memberSerial);
                        }
                    }
                    break;

                case 0x02: // Remove member / re-list
                    if (reader.Remaining >= 5)
                    {
                        byte count = reader.ReadByte();
                        uint removedSerial = reader.ReadUInt32();
                        _worldService.RemovePartyMember(removedSerial);

                        // If party count is 0 or 1, maybe it disbanded? We'll rely on the server 0x01 or removing ourselves
                        if (_worldService.Player != null && removedSerial == _worldService.Player.Serial)
                        {
                            _worldService.ClearParty();
                        }
                        else
                        {
                            // In some protocol versions, count might indicate remaining members.
                            // The easiest is just to remove the specific serial.
                        }
                    }
                    break;

                case 0x03: // Private party chat
                case 0x04: // Public party chat
                    if (reader.Remaining >= 4)
                    {
                        uint from = reader.ReadUInt32();
                        string text = reader.ReadUnicodeString();
                        _journalService.AddEntry(new JournalEntry(text, "[Party]", from, 0));
                    }
                    break;

                case 0x07: // Party invite
                    if (reader.Remaining >= 4)
                    {
                        uint leaderSerial = reader.ReadUInt32();
                        // This packet is handled for auto-accept in FriendsHandler, 
                        // and filtering for block party invite in FilterHandler.
                    }
                    break;
            }
        }

        private void HandleExtendedStats(UOBufferReader reader)
        {
            // Sub 0x19
            if (reader.Remaining == 4) // 9 byte total: Special Ability Info per The Miracle
            {
                var player = _worldService.Player;
                if (player != null)
                {
                    lock (player.SyncRoot)
                    {
                        player.PrimaryAbilityId = reader.ReadByte();
                        player.SecondaryAbilityId = reader.ReadByte();
                        byte activeFlags = reader.ReadByte();
                        player.PrimaryAbilityActive = (activeFlags & 0x01) != 0;
                        player.SecondaryAbilityActive = (activeFlags & 0x02) != 0;
                    }
                }
                return;
            }

            // Sub 0x19 — type(1) serial(4) + dati dipendenti dal tipo
            if (reader.Remaining < 5) return;
            byte type = reader.ReadByte();
            uint serial = reader.ReadUInt32();

            if (type != 0x02 && type < 3) return; // tipo 0x02 = stat locks, 3+ = AOS stats

            var m = _worldService.FindMobile(serial);
            if (m == null) return;

            lock (m.SyncRoot)
            {
                if (type == 0x02) // stat locks (StrLock, DexLock, IntLock)
                {
                    // Formato: type(0x02) serial(4) unk(1) locks(1)
                    // locks = (str << 4) | (dex << 2) | int  — due bit per lock (Up/Down/Lock)
                    if (reader.Remaining >= 2)
                    {
                        reader.ReadByte(); // unknown
                        byte locks = reader.ReadByte();
                        m.StrLock = (byte)((locks >> 4) & 0x03);
                        m.DexLock = (byte)((locks >> 2) & 0x03);
                        m.IntLock = (byte)(locks & 0x03);
                    }
                }
                else if (type >= 3 && reader.Remaining >= 22) // AOS extended stats
                {
                    m.StatCap = reader.ReadUInt16();
                    m.Followers = reader.ReadByte();
                    m.FollowersMax = reader.ReadByte();
                    m.FireResist = reader.ReadInt16();
                    m.ColdResist = reader.ReadInt16();
                    m.PoisonResist = reader.ReadInt16();
                    m.EnergyResist = reader.ReadInt16();
                    m.Luck = reader.ReadInt32();
                    m.MinDamage = reader.ReadUInt16();
                    m.MaxDamage = reader.ReadUInt16();
                    m.Tithe = reader.ReadInt32();
                }
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────────────
        #region Mobile Handlers
        // ─────────────────────────────────────────────────────────────────────────────────

        private bool HandleMobileIncoming(byte[] data)
        {
            // 0x78: cmd(1) len(2) serial(4) body(2) x(2) y(2) z(1s) dir(1) hue(2) flags(1) notoriety(1)
            if (data.Length < 17) return true;

            // Apply Graph Morphing
            ushort body = (ushort)((data[7] << 8) | data[8]);
            ushort hue = (ushort)((data[15] << 8) | data[16]);
            ushort oldBody = body;
            MorphGraphic(ref body, ref hue);
            if (body != oldBody)
            {
                data[7] = (byte)(body >> 8); data[8] = (byte)body;
                data[15] = (byte)(hue >> 8); data[16] = (byte)hue;
            }

            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0x78
            reader.ReadUInt16();         // length
            uint serial = reader.ReadUInt32();
            body = reader.ReadUInt16();
            ushort x = reader.ReadUInt16();
            ushort y = reader.ReadUInt16();
            sbyte z = reader.ReadSByte();
            byte dir = reader.ReadByte();
            hue = reader.ReadUInt16();
            byte flags = reader.ReadByte();
            byte notoriety = reader.ReadByte();

            var mobile = _worldService.FindMobile(serial);
            if (mobile == null)
            {
                mobile = new Mobile(serial);
                _worldService.AddMobile(mobile);
            }

            bool isPlayer = _worldService.Player?.Serial == serial;

            lock (mobile.SyncRoot)
            {
                mobile.Graphic = body;
                mobile.X = x;
                mobile.Y = y;
                mobile.Z = z;
                mobile.Direction = dir;
                mobile.Hue = hue;
                mobile.Notoriety = notoriety;
                mobile.Flags = flags;
                mobile.IsHidden = (flags & 0x80) != 0;
                mobile.IsPoisoned = (flags & 0x04) != 0;
                mobile.IsYellowHits = (flags & 0x08) != 0;
                
                bool wasParalyzed = mobile.Paralyzed;
                mobile.Paralyzed = (flags & 0x01) != 0;

                if (isPlayer && wasParalyzed != mobile.Paralyzed)
                    _messenger.Send(new PlayerStatusMessage(StatType.Paralyzed, serial, (ushort)(mobile.Paralyzed ? 1 : 0), 1));
            }

            // Lista equipaggiamento: termina con serial=0
            while (reader.Remaining >= 4)
            {
                uint itemSerial = reader.ReadUInt32();
                if (itemSerial == 0) break;
                if (reader.Remaining < 5) break;

                ushort itemId = reader.ReadUInt16();
                byte layer = reader.ReadByte();
                ushort itemHue = 0;
                if ((itemId & 0x8000) != 0)
                {
                    itemId &= 0x7FFF;
                    if (reader.Remaining >= 2) itemHue = reader.ReadUInt16();
                }

                var item = _worldService.FindItem(itemSerial);
                if (item == null)
                {
                    item = new Item(itemSerial);
                    _worldService.AddItem(item);
                }
                lock (item.SyncRoot)
                {
                    item.Graphic = itemId;
                    item.Hue = itemHue;
                    item.Layer = layer;
                    item.Container = serial;
                }
            }

            if (isPlayer)
            {
                System.Diagnostics.Trace.WriteLine($"[WorldHandler] 0x78 MobileIncoming — player serial=0x{mobile.Serial:X8}");
                _worldService.SetPlayer(mobile);
            }
            return true;
        }

        private bool HandleMobileUpdate(byte[] data)
        {
            // 0x20: cmd(1) serial(4) body(2) offset(1s) hue(2) flags(1) x(2) y(2) unk(2) dir(1) z(1s)
            if (data.Length < 17) return true;

            // Apply Graph Morphing
            ushort body = (ushort)((data[5] << 8) | data[6]);
            sbyte offset = (sbyte)data[7];
            ushort realBody = (ushort)(body + offset);
            ushort hue = (ushort)((data[8] << 8) | data[9]);
            ushort oldBody = realBody;
            MorphGraphic(ref realBody, ref hue);
            if (realBody != oldBody)
            {
                data[5] = (byte)(realBody >> 8); data[6] = (byte)realBody;
                data[7] = 0; // Reset offset
                data[8] = (byte)(hue >> 8); data[9] = (byte)hue;
            }

            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0x20
            uint serial = reader.ReadUInt32();
            body = reader.ReadUInt16();
            sbyte bodyOffset = reader.ReadSByte();
            hue = reader.ReadUInt16();
            byte flags = reader.ReadByte();
            ushort x = reader.ReadUInt16();
            ushort y = reader.ReadUInt16();
            reader.ReadUInt16();         // unknown
            byte dir = reader.ReadByte();
            sbyte z = reader.ReadSByte();

            var mobile = _worldService.FindMobile(serial);
            if (mobile == null)
            {
                mobile = new Mobile(serial);
                _worldService.AddMobile(mobile);
            }

            bool isPlayer = _worldService.Player?.Serial == serial;

            lock (mobile.SyncRoot)
            {
                mobile.Graphic = (ushort)(body + bodyOffset);
                mobile.Hue = hue;
                mobile.X = x;
                mobile.Y = y;
                mobile.Z = z;
                mobile.Direction = dir;
                mobile.IsHidden = (flags & 0x80) != 0;
                mobile.IsPoisoned = (flags & 0x04) != 0;
                mobile.IsYellowHits = (flags & 0x08) != 0;
                bool wasParalyzed = mobile.Paralyzed;
                mobile.Paralyzed = (flags & 0x01) != 0;

                if (isPlayer && wasParalyzed != mobile.Paralyzed)
                    _messenger.Send(new PlayerStatusMessage(StatType.Paralyzed, serial, (ushort)(mobile.Paralyzed ? 1 : 0), 1));
            }
            return true;
        }

        private void HandleMobileMoving(byte[] data)
        {
            // 0x77: cmd(1) serial(4) body(2) x(2) y(2) z(1s) dir(1) hue(2) flags(1) notoriety(1)
            if (data.Length < 17) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0x77
            uint serial = reader.ReadUInt32();
            ushort body = reader.ReadUInt16();
            ushort x = reader.ReadUInt16();
            ushort y = reader.ReadUInt16();
            sbyte z = reader.ReadSByte();
            byte dir = reader.ReadByte();
            ushort hue = reader.ReadUInt16();
            byte flags = reader.ReadByte();
            byte notoriety = reader.ReadByte();

            var mobile = _worldService.FindMobile(serial);
            if (mobile == null)
            {
                mobile = new Mobile(serial);
                _worldService.AddMobile(mobile);
            }

            bool isPlayer = _worldService.Player?.Serial == serial;

            lock (mobile.SyncRoot)
            {
                mobile.Graphic = body;
                mobile.X = x;
                mobile.Y = y;
                mobile.Z = z;
                mobile.Direction = dir;
                mobile.Hue = hue;
                mobile.Notoriety = notoriety;
                mobile.Flags = flags;
                mobile.IsHidden = (flags & 0x80) != 0;
                mobile.IsPoisoned = (flags & 0x04) != 0;
                
                bool wasParalyzed = mobile.Paralyzed;
                mobile.Paralyzed = (flags & 0x01) != 0;

                if (isPlayer && wasParalyzed != mobile.Paralyzed)
                    _messenger.Send(new PlayerStatusMessage(StatType.Paralyzed, serial, (ushort)(mobile.Paralyzed ? 1 : 0), 1));
            }

            _messenger.Send(new MobileMovingMessage(serial, x, y, z, dir));
        }

        private void HandleMobileName(byte[] data)
        {
            // 0x98: cmd(1) len(2) serial(4) name[30]
            if (data.Length < 37) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0x98
            reader.ReadUInt16();         // length
            uint serial = reader.ReadUInt32();
            string name = reader.ReadString(30);

            var mobile = _worldService.FindMobile(serial);
            if (mobile != null)
                lock (mobile.SyncRoot) { mobile.Name = name; }

            _messenger.Send(new MobileNameMessage(serial, name));
        }

        private void HandleOpenPaperdoll(byte[] data)
        {
            // 0x88: cmd(1) serial(4) text[60] flags(1)
            if (data.Length < 66) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0x88
            uint serial = reader.ReadUInt32();
            string text = reader.ReadString(60);
            // flags: 0x01 = war mode, 0x02 = can alter paperdoll

            var mobile = _worldService.FindMobile(serial);
            if (mobile != null && text.Length > 0)
                lock (mobile.SyncRoot) { mobile.Name = text.Split(',')[0].Trim(); }
        }

        private void HandleSAMobileStatus(byte[] data)
        {
            // 0x16: cmd(1) serial(4) [block: unk(2) id(2) flag(1)] terminato da unk=0
            // id: 0x01=Poisoned, 0x02=YellowHits; flag: 0x00=Off, !0=On (per poison: livello+1)
            if (data.Length < 5) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0x16
            uint serial = reader.ReadUInt32();

            if (reader.Remaining < 2) return;
            if (reader.ReadUInt16() == 0) return; // fine pacchetto

            if (reader.Remaining < 3) return;
            ushort id = reader.ReadUInt16();
            byte flag = reader.ReadByte();

            var m = _worldService.FindMobile(serial);
            if (m == null) return;

            lock (m.SyncRoot)
            {
                switch (id)
                {
                    case 1: m.IsPoisoned = (flag != 0); break;
                    case 2: m.IsYellowHits = (flag != 0); break;
                }
            }

            _messenger.Send(new MobilePoisonedMessage(serial, m.IsPoisoned, m.IsYellowHits));
        }

        private void HandleNewMobileStatus(byte[] data)
        {
            // 0x17: cmd(1) serial(4) unk(2) id(2) flag(1) — stessa logica di 0x16 con un campo in più
            if (data.Length < 9) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0x17
            uint serial = reader.ReadUInt32();
            reader.ReadUInt16();         // unk (sempre 0x01?)
            if (reader.Remaining < 3) return;
            ushort id = reader.ReadUInt16();
            byte flag = reader.ReadByte();

            var m = _worldService.FindMobile(serial);
            if (m == null) return;

            lock (m.SyncRoot)
            {
                switch (id)
                {
                    case 1: m.IsPoisoned = (flag != 0); break;
                    case 2: m.IsYellowHits = (flag != 0); break;
                }
            }

            _messenger.Send(new MobilePoisonedMessage(serial, m.IsPoisoned, m.IsYellowHits));
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────────────
        #region Item Handlers
        // ─────────────────────────────────────────────────────────────────────────────────

        private bool HandleWorldItem(byte[] data)
        {
            // 0x1A: cmd(1) len(2) serial(4) itemID(2) [amount(2)?] x(2) y(2) [dir(1)?] z(1s) [hue(2)?] [flags(1)?]
            // Bit-flag packed: serial & 0x80000000 → has amount; itemID & 0x8000 → has offset; x & 0x8000 → has dir;
            //                  y & 0x8000 → has hue; y & 0x4000 → has flags
            if (data.Length < 8) return true;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0x1A
            reader.ReadUInt16();         // length
            uint rawSerial = reader.ReadUInt32();
            uint serial = rawSerial & 0x7FFFFFFF;
            ushort itemId = reader.ReadUInt16();

            ushort amount = 1;
            if ((rawSerial & 0x80000000) != 0 && reader.Remaining >= 2)
                amount = reader.ReadUInt16();

            if ((itemId & 0x8000) != 0 && reader.Remaining >= 1)
            {
                sbyte idOffset = reader.ReadSByte();
                itemId = (ushort)((itemId & 0x7FFF) + idOffset);
            }
            else itemId &= 0x7FFF;

            if (reader.Remaining < 4) return true;
            ushort x = reader.ReadUInt16();
            ushort y = reader.ReadUInt16();

            byte dir = 0;
            if ((x & 0x8000) != 0 && reader.Remaining >= 1)
                dir = reader.ReadByte();
            x &= 0x7FFF;

            sbyte z = reader.Remaining >= 1 ? reader.ReadSByte() : (sbyte)0;

            ushort hue = 0;
            if ((y & 0x8000) != 0 && reader.Remaining >= 2)
                hue = reader.ReadUInt16();

            byte flags = 0;
            if ((y & 0x4000) != 0 && reader.Remaining >= 1)
                flags = reader.ReadByte();
            y &= 0x3FFF;

            var item = _worldService.FindItem(serial);
            if (item == null)
            {
                item = new Item(serial);
                _worldService.AddItem(item);
            }

            lock (item.SyncRoot)
            {
                item.Graphic = itemId;
                item.Amount = amount;
                item.X = x;
                item.Y = y;
                item.Z = z;
                item.Hue = hue;
                item.Flags = flags;
                item.Container = 0;
                item.Layer = dir;
            }

            // Advanced Filter: Static Fields (convert fields like Wall of Stone to static graphics)
            if (_configService.CurrentProfile.StaticFields && ItemDataHelper.AnimatedWallToStatic.TryGetValue((ushort)(itemId & 0x3FFF), out var staticData))
            {
                item.Graphic = staticData.NewItemID;
                item.Hue = staticData.NewHue;

                var staticPkt = PacketBuilder.WorldItem(serial, staticData.NewItemID, amount, x, y, z, staticData.NewHue, flags);
                _packetService.SendToClient(staticPkt);

                var labelPkt = PacketBuilder.OverheadUnicodeSpeech(staticData.Label, serial, staticData.NewItemID, hue: staticData.NewHue);
                _packetService.SendToClient(labelPkt);

                _messenger.Send(new WorldItemMessage(item));
                return false;
            }

            // Track multi structures (houses/boats) — graphic bit 0x4000 indicates a multi
            if ((itemId & 0x4000) != 0)
                _multiService.AddMulti(serial, x, y, itemId);

            _messenger.Send(new WorldItemMessage(item));
            return true;
        }

        private bool HandleSAWorldItem(byte[] data)
        {
            // 0xF3: cmd(1) unk(2) artDataID(1) serial(4) itemID(2) dir(1) amount(2) amount2(2)
            //        x(2) y(2) z(1s) light(1) hue(2) flags(1) [unk(2) — post-7.0.9]
            if (data.Length < 24) return true;

            // Apply Graph Morphing
            ushort itemId = (ushort)((data[8] << 8) | data[9]);
            ushort hue = (ushort)((data[21] << 8) | data[22]);
            ushort oldId = itemId;
            MorphGraphic(ref itemId, ref hue);
            if (itemId != oldId)
            {
                data[8] = (byte)(itemId >> 8); data[9] = (byte)itemId;
                data[21] = (byte)(hue >> 8); data[22] = (byte)hue;
            }

            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0xF3
            reader.ReadUInt16();         // unk (0x0001)
            byte artDataId = reader.ReadByte();
            uint serial = reader.ReadUInt32();
            itemId = reader.ReadUInt16();
            if (artDataId == 0x02) itemId |= 0x4000;
            byte dir = reader.ReadByte();
            reader.ReadUInt16();         // amount (first copy)
            ushort amount = reader.ReadUInt16();
            ushort x = reader.ReadUInt16();
            ushort y = reader.ReadUInt16();
            sbyte z = reader.ReadSByte();
            reader.ReadByte();           // light
            hue = reader.ReadUInt16();
            byte flags = reader.ReadByte();           // flags

            var item = _worldService.FindItem(serial);
            if (item == null)
            {
                item = new Item(serial);
                _worldService.AddItem(item);
            }

            lock (item.SyncRoot)
            {
                item.Graphic = itemId;
                item.Amount = amount;
                item.X = x;
                item.Y = y;
                item.Z = z;
                item.Hue = hue;
                item.Flags = flags;
                item.Container = 0;
                item.Layer = dir;
            }

            // Advanced Filter: Static Fields (convert fields like Wall of Stone to static graphics)
            if (_configService.CurrentProfile.StaticFields && ItemDataHelper.AnimatedWallToStatic.TryGetValue((ushort)(itemId & 0x3FFF), out var staticData))
            {
                item.Graphic = staticData.NewItemID;
                item.Hue = staticData.NewHue;

                var staticPkt = PacketBuilder.WorldItem(serial, staticData.NewItemID, amount, x, y, z, staticData.NewHue, flags);
                _packetService.SendToClient(staticPkt);

                var labelPkt = PacketBuilder.OverheadUnicodeSpeech(staticData.Label, serial, staticData.NewItemID, hue: staticData.NewHue);
                _packetService.SendToClient(labelPkt);

                _messenger.Send(new WorldItemMessage(item));
                return false;
            }

            // Track multi structures (houses/boats) — artDataId == 0x02 sets bit 0x4000
            if ((itemId & 0x4000) != 0)
                _multiService.AddMulti(serial, x, y, itemId);

            _messenger.Send(new WorldItemMessage(item));
            return true;
        }

        private void HandleRemoveObject(byte[] data)
        {
            // 0x1D: cmd(1) serial(4) — rimozione entità dal mondo
            if (data.Length < 5) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte(); // 0x1D
            uint serial = reader.ReadUInt32();

            _multiService.RemoveMulti(serial);
            _worldService.RemoveMobile(serial);
            _worldService.RemoveItem(serial);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────────────
        #region Movement Handlers
        // ─────────────────────────────────────────────────────────────────────────────────

        private void HandleWalkReject(byte[] data)
        {
            // 0x21: cmd(1) seq(1) x(2) y(2) dir(1) z(1s) — il server rifiuta il movimento
            if (data.Length < 8) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0x21
            reader.ReadByte();           // sequence number
            ushort x = reader.ReadUInt16();
            ushort y = reader.ReadUInt16();
            byte dir = reader.ReadByte();
            sbyte z = reader.ReadSByte();

            var player = _worldService.Player;
            if (player != null)
            {
                lock (player.SyncRoot)
                {
                    player.X = x;
                    player.Y = y;
                    player.Z = z;
                    player.Direction = dir;
                }
            }
        }

        private void HandleMovementAck(byte[] data)
        {
            // 0x22: cmd(1) seq(1) notoriety(1) — il server conferma il movimento
            if (data.Length < 3) return;
            byte seq = data[1];
            byte notoriety = data[2];

            var player = _worldService.Player;
            if (player != null)
                lock (player.SyncRoot) { player.Notoriety = notoriety; }

            _messenger.Send(new MovementAckMessage(seq, notoriety));
        }

        private void HandleMovementDemand(byte[] data)
        {
            // 0x97: cmd(1) dir(1) — il server forza uno spostamento (teleport fisico)
            if (data.Length < 2) return;
            // Solo notifica — l'aggiornamento di posizione arriva con il successivo 0x20/0x77
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────────────
        #region Container & Inventory Handlers
        // ─────────────────────────────────────────────────────────────────────────────────

        private void HandleBeginContainerContent(byte[] data)
        {
            // 0x24: cmd(1) serial(4) [gumpId(2)?] — segnala apertura container (precede 0x3C)
            if (data.Length < 5) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0x24
            uint serial = reader.ReadUInt32();
            _worldService.SetLastOpenedContainer(serial);
        }

        private void HandleContainerContent(byte[] data)
        {
            // 0x3C: cmd(1) len(2) count(2) [count × item_record]
            // item_record: serial(4) itemID(2) pad(1) amount(2) x(2) y(2) [gridIndex(1)?] containerSerial(4) hue(2)
            // Classic: 19 byte; Extended (post-KR): 20 byte (con grid index)
            if (data.Length < 5) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0x3C
            ushort pktLen = reader.ReadUInt16();
            ushort count = reader.ReadUInt16();

            bool extendedFormat = count > 0 && pktLen > 5 && (pktLen - 5) % 20 == 0 && (pktLen - 5) / 20 == count;

            var addedItems = new List<Item>(count);
            uint containerSerial = 0;

            for (int i = 0; i < count && reader.Remaining >= 19; i++)
            {
                uint itemSerial = reader.ReadUInt32();
                ushort itemId = reader.ReadUInt16();
                reader.ReadByte();       // padding (0)
                ushort amount = reader.ReadUInt16();
                ushort x = reader.ReadUInt16();
                ushort y = reader.ReadUInt16();

                if (extendedFormat && reader.Remaining >= 1)
                    reader.ReadByte();   // grid index

                uint cSerial = reader.ReadUInt32();
                ushort hue = reader.ReadUInt16();

                if (containerSerial == 0) containerSerial = cSerial;

                var item = _worldService.FindItem(itemSerial);
                if (item == null)
                {
                    item = new Item(itemSerial);
                    _worldService.AddItem(item);
                }

                lock (item.SyncRoot)
                {
                    item.Graphic = itemId;
                    item.Amount = amount;
                    item.X = x;
                    item.Y = y;
                    item.Z = 0;
                    item.Hue = hue;
                    item.Container = cSerial;
                }

                addedItems.Add(item);
            }

            if (containerSerial != 0)
            {
                _worldService.SetLastOpenedContainer(containerSerial);
                _messenger.Send(new ContainerContentMessage(containerSerial, addedItems.AsReadOnly()));
            }
        }

        private void HandleAddItemToContainer(byte[] data)
        {
            // 0x25: cmd(1) serial(4) itemID(2) pad(1) amount(2) x(2) y(2) [gridIndex(1)?] containerSerial(4) hue(2)
            if (data.Length < 20) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0x25
            uint itemSerial = reader.ReadUInt32();
            ushort itemId = reader.ReadUInt16();
            reader.ReadByte();           // padding
            ushort amount = reader.ReadUInt16();
            ushort x = reader.ReadUInt16();
            ushort y = reader.ReadUInt16();

            if (data.Length == 21)
                reader.ReadByte();       // grid index (formato esteso)

            uint containerSerial = reader.ReadUInt32();
            ushort hue = reader.ReadUInt16();

            var item = _worldService.FindItem(itemSerial);
            if (item == null)
            {
                item = new Item(itemSerial);
                _worldService.AddItem(item);
            }

            lock (item.SyncRoot)
            {
                item.Graphic = itemId;
                item.Amount = amount;
                item.X = x;
                item.Y = y;
                item.Z = 0;
                item.Hue = hue;
                item.Container = containerSerial;
                item.Layer = 0;
            }

            _messenger.Send(new ContainerItemAddedMessage(containerSerial, itemSerial));
        }

        private void HandleEquipUpdate(byte[] data)
        {
            // 0x2E: cmd(1) serial(4) itemID(2) pad(1) layer(1) mobileSerial(4) hue(2)
            if (data.Length < 15) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0x2E
            uint itemSerial = reader.ReadUInt32();
            ushort itemId = reader.ReadUInt16();
            reader.ReadByte();           // padding
            byte layer = reader.ReadByte();
            uint mobileSerial = reader.ReadUInt32();
            ushort hue = reader.ReadUInt16();

            var item = _worldService.FindItem(itemSerial);
            if (item == null)
            {
                item = new Item(itemSerial);
                _worldService.AddItem(item);
            }

            lock (item.SyncRoot)
            {
                item.Graphic = itemId;
                item.Hue = hue;
                item.Layer = layer;
                item.Container = mobileSerial;
                item.X = 0;
                item.Y = 0;
                item.Z = 0;
            }

            if (layer == (byte)Layer.Backpack)
            {
                var mobile = _worldService.FindMobile(mobileSerial);
                if (mobile != null)
                    lock (mobile.SyncRoot) { mobile.Backpack = item; }
            }

            _messenger.Send(new EquipmentChangedMessage(mobileSerial, itemSerial, layer));
        }

        private void HandleCorpseEquipment(byte[] data)
        {
            // 0x89: cmd(1) len(2) corpseSerial(4) [layer(1) itemSerial(4)]... terminato da layer=0x00
            if (data.Length < 7) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0x89
            reader.ReadUInt16();         // length
            uint corpseSerial = reader.ReadUInt32();

            while (reader.Remaining >= 5)
            {
                byte layer = reader.ReadByte();
                if (layer == 0x00) break;
                uint itemSerial = reader.ReadUInt32();

                var item = _worldService.FindItem(itemSerial);
                if (item == null)
                {
                    item = new Item(itemSerial);
                    _worldService.AddItem(item);
                }
                lock (item.SyncRoot)
                {
                    item.Layer = layer;
                    item.Container = corpseSerial;
                }
            }
        }

        private void HandleLiftReject(byte[] data)
        {
            // 0x27: cmd(1) reason(1) — il server rifiuta il lift di un item
            byte reason = data.Length >= 2 ? data[1] : (byte)0;
            _messenger.Send(new LiftRejectMessage(reason));
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────────────
        #region Stats & Skills Handlers
        // ─────────────────────────────────────────────────────────────────────────────────

        private void HandleMobileStatus(byte[] data)
        {
            // 0x11: cmd(1) len(2) serial(4) name[30] hits(2) maxHits(2) rename(1) type(1)
            // type 0 = solo hits; type 1+ = + str/dex/int/stam/mana/gold/armor/weight...
            if (data.Length < 43) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0x11
            reader.ReadUInt16();         // length
            uint serial = reader.ReadUInt32();
            string name = reader.ReadString(30);
            ushort hits = reader.ReadUInt16();
            ushort maxHits = reader.ReadUInt16();
            reader.ReadByte();           // can rename
            byte type = reader.ReadByte();

            var m = _worldService.FindMobile(serial);
            if (m == null)
            {
                m = new Mobile(serial);
                _worldService.AddMobile(m);
            }

            lock (m.SyncRoot)
            {
                m.Name = name.TrimEnd('\0');
                m.Hits = hits;
                m.HitsMax = maxHits;

                if (type >= 1 && reader.Remaining >= 14)
                {
                    reader.ReadByte();   // isFemale (per il player)
                    m.Str = reader.ReadUInt16();
                    m.Dex = reader.ReadUInt16();
                    m.Int = reader.ReadUInt16();
                    m.Stam = reader.ReadUInt16();
                    m.StamMax = reader.ReadUInt16();
                    m.Mana = reader.ReadUInt16();
                    m.ManaMax = reader.ReadUInt16();
                }

                if (type >= 1 && reader.Remaining >= 6)
                {
                    m.Gold = (ushort)reader.ReadUInt16();
                    m.Armor = reader.ReadUInt16();
                    m.Weight = reader.ReadUInt16();
                }

                if (type >= 5 && reader.Remaining >= 3)
                {
                    m.MaxWeight = reader.ReadUInt16();
                    reader.ReadByte(); // race
                }

                if (type >= 3 && reader.Remaining >= 4)
                {
                    m.StatCap = reader.ReadUInt16();
                    m.Followers = reader.ReadByte();
                    m.FollowersMax = reader.ReadByte();
                }

                if (type >= 4 && reader.Remaining >= 16)
                {
                    m.FireResist = reader.ReadInt16();
                    m.ColdResist = reader.ReadInt16();
                    m.PoisonResist = reader.ReadInt16();
                    m.EnergyResist = reader.ReadInt16();
                    m.Luck = reader.ReadInt16();
                    m.MinDamage = reader.ReadUInt16();
                    m.MaxDamage = reader.ReadUInt16();
                    m.Tithe = reader.ReadInt32();
                }
            }

            if (_worldService.Player?.Serial == serial)
            {
                System.Diagnostics.Trace.WriteLine($"[WorldHandler] 0x11 MobileStatus — HP={hits}/{maxHits} Mana={m.Mana}/{m.ManaMax} Stam={m.Stam}/{m.StamMax} type={type}");
                _messenger.Send(new PlayerStatusMessage(StatType.Hits, serial, hits, maxHits));
                if (type >= 1)
                {
                    _messenger.Send(new PlayerStatusMessage(StatType.Mana, serial, m.Mana, m.ManaMax));
                    _messenger.Send(new PlayerStatusMessage(StatType.Stamina, serial, m.Stam, m.StamMax));
                }
            }
        }

        private void HandleHitsUpdate(byte[] data)
        {
            // 0xA1: cmd(1) serial(4) max(2) cur(2)
            if (data.Length < 9) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();
            uint serial = reader.ReadUInt32();
            ushort max = reader.ReadUInt16();
            ushort cur = reader.ReadUInt16();

            var mobile = _worldService.FindMobile(serial);
            if (mobile != null)
            {
                mobile.HitsMax = max;
                mobile.Hits = cur;
            }
            _messenger.Send(new PlayerStatusMessage(StatType.Hits, serial, cur, max));
        }

        private void HandleManaUpdate(byte[] data)
        {
            // 0xA2: cmd(1) serial(4) max(2) cur(2)
            if (data.Length < 9) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();
            uint serial = reader.ReadUInt32();
            ushort max = reader.ReadUInt16();
            ushort cur = reader.ReadUInt16();

            var mobile = _worldService.FindMobile(serial);
            if (mobile != null)
            {
                mobile.ManaMax = max;
                mobile.Mana = cur;
            }
            _messenger.Send(new PlayerStatusMessage(StatType.Mana, serial, cur, max));
        }

        private void HandleStaminaUpdate(byte[] data)
        {
            // 0xA3: cmd(1) serial(4) max(2) cur(2)
            if (data.Length < 9) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();
            uint serial = reader.ReadUInt32();
            ushort max = reader.ReadUInt16();
            ushort cur = reader.ReadUInt16();

            var mobile = _worldService.FindMobile(serial);
            if (mobile != null)
            {
                mobile.StamMax = max;
                mobile.Stam = cur;
            }
            _messenger.Send(new PlayerStatusMessage(StatType.Stamina, serial, cur, max));
        }

        private void HandleMobileStatInfo(byte[] data)
        {
            // 0x2D: cmd(1) serial(4) maxHits(2) maxMana(2) maxStam(2)
            //        hits(2) mana(2) stam(2)
            // Pacchetto compatto per aggiornare stats di un mobile arbitrario
            if (data.Length < 17) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0x2D
            uint serial = reader.ReadUInt32();
            ushort maxHits = reader.ReadUInt16();
            ushort maxMana = reader.ReadUInt16();
            ushort maxStam = reader.ReadUInt16();
            ushort hits = reader.ReadUInt16();
            ushort mana = reader.ReadUInt16();
            ushort stam = reader.ReadUInt16();

            var m = _worldService.FindMobile(serial);
            if (m == null) return;

            lock (m.SyncRoot)
            {
                m.HitsMax = maxHits;
                m.ManaMax = maxMana;
                m.StamMax = maxStam;
                m.Hits = hits;
                m.Mana = mana;
                m.Stam = stam;
            }

            _messenger.Send(new PlayerStatusMessage(StatType.Hits, serial, hits, maxHits));
        }

        private void HandleSkillsUpdate(byte[] data)
        {
            // 0x3A: cmd(1) len(2) type(1) [skill entries]
            // type: 0x00=list(no cap), 0x02=list(with cap), 0xDF=single(cap), 0xFF=single(no cap)
            // Inoltriamo i dati grezzi a SkillsService tramite Messenger
            if (data.Length < 4) return;
            _messenger.Send(new SkillsUpdatedMessage(data));
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────────────
        #region Combat Handlers
        // ─────────────────────────────────────────────────────────────────────────────────

        private void HandleDamage(byte[] data)
        {
            // 0x0B: cmd(1) serial(4) damage(2)
            if (data.Length < 7) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0x0B
            uint serial = reader.ReadUInt32();
            ushort damage = reader.ReadUInt16();

            _messenger.Send(new DamageMessage(serial, damage));
        }

        private void HandleWarMode(byte[] data)
        {
            // 0x72: cmd(1) warMode(1) pad(3)
            if (data.Length < 2) return;
            bool warMode = data[1] != 0;

            var player = _worldService.Player;
            if (player != null)
                lock (player.SyncRoot) { player.WarMode = warMode; }

            _messenger.Send(new WarModeMessage(warMode));
        }

        private void HandleAttackOK(byte[] data)
        {
            // 0xAA: cmd(1) serial(4) — serial=0 se l'attacco viene cancellato
            if (data.Length < 5) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0xAA
            uint targetSerial = reader.ReadUInt32();

            var player = _worldService.Player;
            if (player != null)
                lock (player.SyncRoot) { player.AttackTarget = targetSerial; }

            _messenger.Send(new AttackTargetMessage(targetSerial));
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────────────
        #region Targeting Handlers
        // ─────────────────────────────────────────────────────────────────────────────────

        private void HandleTargetCursorFromServer(byte[] data)
        {
            // 0x6C S2C: cmd(1) targetType(1) cursorId(4) cursorType(1) ...
            if (data.Length < 7) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0x6C
            byte targetType = reader.ReadByte();   // 0=obj/mobile, 1=terrain
            uint cursorId = reader.ReadUInt32();
            byte cursorType = reader.ReadByte();   // 0=neutro, 1=offensivo, 2=benefico

            _worldService.IsCasting = false;
            _messenger.Send(new TargetCursorMessage(cursorId, targetType, cursorType));
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────────────
        #region Gump Handlers
        // ─────────────────────────────────────────────────────────────────────────────────

        private void HandleGump(byte[] data)
        {
            // 0xB0: cmd(1) len(2) serial(4) gumpId(4) x(4) y(4) layoutLen(2) layout[layoutLen]
            //        linesCount(2) [line: len(2) text[len*2]]
            if (data.Length < 21) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0xB0
            reader.ReadUInt16();         // length
            uint serial = reader.ReadUInt32();
            uint gumpId = reader.ReadUInt32();
            int x = (int)reader.ReadUInt32();
            int y = (int)reader.ReadUInt32();

            var gump = new UOGump(serial, gumpId) { X = x, Y = y };

            if (reader.Remaining >= 2)
            {
                ushort layoutLen = reader.ReadUInt16();
                if (layoutLen > 0 && reader.Remaining >= layoutLen)
                    gump.Layout = reader.ReadString(layoutLen);
            }

            if (reader.Remaining >= 2)
            {
                ushort linesCount = reader.ReadUInt16();
                for (int i = 0; i < linesCount; i++)
                {
                    if (reader.Remaining < 2) break;
                    ushort lineLen = reader.ReadUInt16();
                    if (lineLen > 0 && reader.Remaining >= lineLen * 2)
                        gump.AddString(reader.ReadUnicodeString(lineLen));
                }
            }

            gump.Freeze();
            _worldService.SetCurrentGump(gump);
            _messenger.Send(new GumpMessage(gump));
        }

        private void HandleCompressedGump(byte[] data)
        {
            // 0xDD: cmd(1) len(2) serial(4) gumpId(4) x(4) y(4)
            //        cLen(4) dLen(4) [cLen-4 bytes zlib compressed layout]
            //        numStrings(4) ctLen(4) dtLen(4) [ctLen-4 bytes zlib compressed text]
            if (data.Length < 27) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0xDD
            reader.ReadUInt16();         // total length
            uint serial = reader.ReadUInt32();
            uint gumpId = reader.ReadUInt32();
            int x = reader.ReadInt32();
            int y = reader.ReadInt32();

            if (reader.Remaining < 8) return;
            int cLen = reader.ReadInt32();  // inclusi questi 4 byte
            reader.ReadInt32();             // dLen (decompressed size hint)
            int layoutCompressedSize = cLen - 4;

            if (layoutCompressedSize <= 0 || reader.Remaining < layoutCompressedSize) return;

            string layout = "";
            int layoutStart = reader.Position;
            reader.Skip(layoutCompressedSize);

            try
            {
                using var ms = new MemoryStream(data, layoutStart, layoutCompressedSize);
                using var zlib = new ZLibStream(ms, CompressionMode.Decompress);
                using var sr = new StreamReader(zlib, Encoding.ASCII);
                layout = sr.ReadToEnd();
            }
            catch { }

            var gump = new UOGump(serial, gumpId) { X = x, Y = y, Layout = layout };

            // Stringe compresse
            if (reader.Remaining >= 12)
            {
                int numStrings = reader.ReadInt32();
                if (numStrings < 0 || numStrings > 512) numStrings = 0;
                int ctLen = reader.ReadInt32();
                reader.ReadInt32(); // dtLen
                int textCompressedSize = ctLen - 4;

                if (numStrings > 0 && textCompressedSize > 0 && reader.Remaining >= textCompressedSize)
                {
                    int textStart = reader.Position;
                    try
                    {
                        using var ms = new MemoryStream(data, textStart, textCompressedSize);
                        using var zlib = new ZLibStream(ms, CompressionMode.Decompress);
                        byte[] strLenBuf = new byte[2];

                        for (int i = 0; i < numStrings; i++)
                        {
                            int rb = zlib.ReadAtLeast(strLenBuf, 2, false);
                            if (rb < 2) break;
                            int charCount = (strLenBuf[0] << 8) | strLenBuf[1]; // big-endian
                            if (charCount <= 0) { gump.AddString(""); continue; }

                            byte[] strBuf = new byte[charCount * 2];
                            rb = zlib.ReadAtLeast(strBuf, strBuf.Length, false);
                            if (rb < strBuf.Length) break;
                            gump.AddString(Encoding.BigEndianUnicode.GetString(strBuf));
                        }
                    }
                    catch { }
                }
            }

            gump.Freeze();
            _worldService.SetCurrentGump(gump);
            _messenger.Send(new GumpMessage(gump));
        }

        private void HandleGumpResponse(byte[] data)
        {
            // 0xB1 C2S: il client risponde al gump, che si chiude nella maggior parte dei casi
            // Layout: cmd(1) len(2) serial(4) gumpId(4) buttonId(4) switchCount(4) switches... textCount(4) texts...
            if (data.Length < 19) return;

            var reader = new UOBufferReader(data);
            reader.ReadByte(); // 0xB1
            reader.ReadUInt16(); // len
            uint serial = reader.ReadUInt32();
            uint gumpId = reader.ReadUInt32();
            int buttonId = reader.ReadInt32();

            var switches = new List<int>();
            int switchCount = reader.ReadInt32();
            for (int i = 0; i < switchCount && reader.Remaining >= 4; i++)
            {
                switches.Add(reader.ReadInt32());
            }

            var textEntries = new Dictionary<int, string>();
            int textCount = reader.ReadInt32();
            for (int i = 0; i < textCount && reader.Remaining >= 4; i++)
            {
                int id = reader.ReadUInt16();
                int len = reader.ReadUInt16();
                if (reader.Remaining >= len * 2)
                {
                    string text = reader.ReadUnicodeString(len);
                    textEntries[id] = text;
                }
            }

            _worldService.RemoveGump();
            _messenger.Send(new GumpClosedMessage(gumpId));
            _messenger.Send(new GumpResponseLogMessage(serial, gumpId, buttonId, switches, textEntries));
        }

        private void HandleOpenMenu(byte[] data)
        {
            // 0x7C layout: cmd(1) len(2) serial(4) menuId(2) title[30] count(1)
            //   per ogni item: graphic(2) hue(2) nameLen(1) name[nameLen]
            // Minimo: 1+2+4+2+30+1 = 40 bytes
            if (data.Length < 40) return;

            var reader = new UOBufferReader(data);
            reader.ReadByte();                           // cmd 0x7C
            reader.ReadUInt16();                         // len
            uint   serial = reader.ReadUInt32();
            ushort menuId = reader.ReadUInt16();
            string title  = reader.ReadString(30);
            int    count  = reader.ReadByte();

            var items = new List<UOMenuItem>(count);
            for (int i = 0; i < count && reader.Remaining >= 5; i++)
            {
                ushort graphic = reader.ReadUInt16();
                ushort hue     = reader.ReadUInt16();
                int    nameLen = reader.ReadByte();
                string name    = nameLen > 0 && reader.Remaining >= nameLen
                    ? reader.ReadString(nameLen)
                    : string.Empty;
                items.Add(new UOMenuItem(i + 1, graphic, hue, name));
            }

            MenuStore.Set(new UOMenu(serial, menuId, title, items));
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────────────
        #region Trade Handlers
        // ─────────────────────────────────────────────────────────────────────────────────

        private void HandleTradeRequest(byte[] data)
        {
            // 0x6F S2C: cmd(1) action(1) serial(4) ...
            // action: 0=Start, 1=Cancel, 2=Update, 3=MoneyUpdate, 4=MoneyLimit
            if (data.Length < 6) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0x6F
            byte action = reader.ReadByte();
            uint serial = reader.ReadUInt32();
            
            TradeData? tradeData = null;

            if (action == 0 && reader.Remaining >= 8) // Start
            {
                tradeData = new TradeData
                {
                    TradeId = serial,
                    ContainerMe = reader.ReadUInt32(),
                    ContainerTrader = reader.ReadUInt32()
                };
                if (reader.Remaining > 0)
                {
                    bool hasName = reader.ReadByte() != 0;
                    if (hasName && reader.Remaining > 0)
                    {
                        tradeData.NameTrader = reader.ReadString(reader.Remaining);
                    }
                }
            }
            else if (action == 2 && reader.Remaining >= 8) // Update
            {
                tradeData = new TradeData
                {
                    TradeId = serial,
                    AcceptMe = reader.ReadUInt32() != 0,
                    AcceptTrader = reader.ReadUInt32() != 0
                };
            }
            else if (action == 3 && reader.Remaining >= 8) // MoneyUpdate
            {
                tradeData = new TradeData
                {
                    TradeId = serial,
                    GoldTrader = reader.ReadUInt32(),
                    PlatinumTrader = reader.ReadUInt32()
                };
            }
            else if (action == 4 && reader.Remaining >= 8) // MoneyLimit
            {
                tradeData = new TradeData
                {
                    TradeId = serial,
                    GoldMax = reader.ReadUInt32(),
                    PlatinumMax = reader.ReadUInt32()
                };
            }

            _messenger.Send(new TradeMessage(action, serial, tradeData));
        }

        private void HandleTradeRequestC2S(byte[] data)
        {
            // 0x6F C2S: cmd(1) action(1) serial(4) ...
            if (data.Length < 6) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0x6F
            byte action = reader.ReadByte();
            uint serial = reader.ReadUInt32();
            
            TradeData? tradeData = null;

            if (action == 2 && reader.Remaining >= 8) // Update
            {
                tradeData = new TradeData
                {
                    TradeId = serial,
                    AcceptMe = reader.ReadUInt32() != 0,
                    AcceptTrader = reader.ReadUInt32() != 0
                };
            }
            else if (action == 3 && reader.Remaining >= 8) // MoneyUpdate
            {
                tradeData = new TradeData
                {
                    TradeId = serial,
                    GoldMe = reader.ReadUInt32(),
                    PlatinumMe = reader.ReadUInt32()
                };
            }

            _messenger.Send(new TradeMessage(action, serial, tradeData));
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────────────
        #region Vendor Handlers
        // ─────────────────────────────────────────────────────────────────────────────────

        private void HandleBuyWindow(byte[] data)
        {
            // 0x74: cmd(1) len(2) vendorContainerSerial(4) count(1) [items...]
            if (data.Length < 8) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0x74
            reader.ReadUInt16();         // length
            uint vendorContainerSerial = reader.ReadUInt32();
            byte count = reader.ReadByte();

            // L'originale TMRazor usa LastOpenedContainer per il vendor buy.
            // Aggiorniamo comunque il WorldService.
            _worldService.SetLastOpenedContainer(vendorContainerSerial);

            var vendorItems = _worldService.GetItemsInContainer(vendorContainerSerial).ToList();
            if (vendorItems.Count > 0)
            {
                // Logica di ordinamento legacy per il matching dei prezzi
                var container = _worldService.FindItem(vendorContainerSerial);
                if (container != null && container.Graphic == 0x2af8)
                    vendorItems = vendorItems.OrderBy(i => i.X).ToList();
                else
                    vendorItems.Reverse(); // L'ordine in cui arrivano nel 0x3C è l'opposto di 0x74
            }

            var items = new List<(uint Price, string Name)>();
            for (int i = 0; i < count && reader.Remaining >= 5; i++)
            {
                uint price = reader.ReadUInt32();
                byte nameLen = reader.ReadByte();
                string name = reader.Remaining >= nameLen ? reader.ReadString(nameLen) : "";
                
                // Associa prezzo e nome all'item se possibile
                if (i < vendorItems.Count)
                {
                    var item = vendorItems[i];
                    lock (item.SyncRoot)
                    {
                        item.Price = (int)price;
                        item.Name = name;
                    }
                }

                items.Add((price, name));
            }
            _messenger.Send(new VendorBuyMessage(vendorContainerSerial, items.AsReadOnly()));
        }

        private void HandleSellWindow(byte[] data)
        {
            // 0x9E: cmd(1) len(2) vendorSerial(4) count(2) [items...]
            if (data.Length < 9) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0x9E
            reader.ReadUInt16();         // length
            uint vendorSerial = reader.ReadUInt32();
            ushort count = reader.ReadUInt16();

            var items = new List<(uint Serial, ushort Graphic, ushort Hue, ushort Amount, ushort Price, string Name)>();
            for (int i = 0; i < count && reader.Remaining >= 16; i++)
            {
                uint serial = reader.ReadUInt32();
                ushort graphic = reader.ReadUInt16();
                ushort hue = reader.ReadUInt16();
                ushort amount = reader.ReadUInt16();
                ushort price = reader.ReadUInt16();
                ushort nameLen = reader.ReadUInt16();
                string name = reader.Remaining >= nameLen ? reader.ReadString(nameLen) : "";
                items.Add((serial, graphic, hue, amount, price, name));
            }
            _messenger.Send(new VendorSellMessage(vendorSerial, items.AsReadOnly()));
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────────────
        #region Message & Journal Handlers
        // ─────────────────────────────────────────────────────────────────────────────────

        private void HandleAsciiMessage(byte[] data)
        {
            // 0x1C: cmd(1) len(2) serial(4) body(2) type(1) hue(2) font(2) name[30] text(var ASCII)
            if (data.Length < 44) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0x1C
            reader.ReadUInt16();         // length
            uint serial = reader.ReadUInt32();
            reader.ReadUInt16();         // body
            byte msgType = reader.ReadByte();
            ushort hue = reader.ReadUInt16();
            reader.ReadUInt16();         // font
            string name = reader.ReadString(30);
            string text = reader.ReadString();

            _journalService.AddEntry(new JournalEntry(text, name, serial, hue));

            // Pubblica solo messaggi overhead visibili (Regular, Emote, Yell, Whisper...)
            // System (0x01) e Label (0x06) non hanno un'entità sorgente visibile
            if (msgType != 0x01 && !string.IsNullOrEmpty(text))
                _messenger.Send(new OverheadMessageMessage(serial, name, text, hue,
                    (OverheadMessageType)msgType));
        }

        private void HandleUnicodeMessage(byte[] data)
        {
            // 0xAE: cmd(1) len(2) serial(4) body(2) type(1) hue(2) font(2) lang[4] name[30] text(var Unicode)
            if (data.Length < 48) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0xAE
            reader.ReadUInt16();         // length
            uint serial = reader.ReadUInt32();
            reader.ReadUInt16();         // body
            byte msgType = reader.ReadByte();
            ushort hue = reader.ReadUInt16();
            reader.ReadUInt16();         // font
            reader.ReadString(4);        // language
            string name = reader.ReadString(30);
            string text = reader.ReadUnicodeString();

            _journalService.AddEntry(new JournalEntry(text, name, serial, hue));

            if (msgType != 0x01 && !string.IsNullOrEmpty(text))
                _messenger.Send(new OverheadMessageMessage(serial, name, text, hue,
                    (OverheadMessageType)msgType));
        }

        private void HandleEncodedUnicodeSpeech(byte[] data)
        {
            // 0xAD: cmd(1) len(2) serial(4) body(2) type(1) hue(2) font(2) lang[4] keywords(var) text(var Unicode)
            if (data.Length < 48) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0xAD
            reader.ReadUInt16();         // length
            uint serial = reader.ReadUInt32();
            reader.ReadUInt16();         // body
            byte msgType = reader.ReadByte();
            ushort hue = reader.ReadUInt16();
            reader.ReadUInt16();         // font
            reader.ReadString(4);        // language

            // Decode keywords and text using helper
            string text = EncodedSpeechHelper.Decode(data, reader.Position, out var keywords);

            string name = "Unknown";
            var mobile = _worldService.FindMobile(serial);
            if (mobile != null) name = mobile.Name;

            _journalService.AddEntry(new JournalEntry(text, name, serial, hue));

            if (msgType != 0x01 && !string.IsNullOrEmpty(text))
                _messenger.Send(new OverheadMessageMessage(serial, name, text, hue,
                    (OverheadMessageType)msgType));
        }

        private void HandleDeathAnimation(byte[] data)
        {
            // 0xAF: cmd(1) len(2) killedSerial(4) corpseSerial(4) — animazione di morte
            if (data.Length < 5) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0xAF
            reader.ReadUInt16();         // length
            uint killedSerial = reader.ReadUInt32();

            _messenger.Send(new MobileDeathMessage(killedSerial));
        }

        private void HandleLocalizedMessage(byte[] data)
        {
            // 0xC1: cmd(1) len(2) serial(4) body(2) type(1) hue(2) font(2) cliloc(4) name[30] args(var Unicode LE)
            if (data.Length < 48) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0xC1
            reader.ReadUInt16();         // length
            uint serial = reader.ReadUInt32();
            reader.ReadUInt16();         // body
            reader.ReadByte();           // type
            ushort hue = reader.ReadUInt16();
            reader.ReadUInt16();         // font
            int cliloc = reader.ReadInt32();
            string name = reader.ReadString(30);
            string args = reader.ReadUnicodeString();

            // Tracking IsCasting via clilocs
            switch (cliloc)
            {
                case 502644: // You are already casting a spell.
                case 502645: // You must wait for that spell to complete.
                case 500641: // Your spell fizzles.
                case 502632: // The spell fizzles.
                case 500015: // You do not have that spell!
                    _worldService.IsCasting = false;
                    break;
            }

            string text = _languageService.ClilocFormat(cliloc, args);
            _journalService.AddEntry(new JournalEntry(text, name, serial, hue));
        }

        private void HandleLocalizedMessageAffix(byte[] data)
        {
            // 0xCC: cmd(1) len(2) serial(4) body(2) type(1) hue(2) font(2) cliloc(4) flags(1) name[30]
            //        affix(var ASCII null-term) args(var Unicode LE)
            if (data.Length < 49) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0xCC
            reader.ReadUInt16();         // length
            uint serial = reader.ReadUInt32();
            reader.ReadUInt16();         // body
            reader.ReadByte();           // type
            ushort hue = reader.ReadUInt16();
            reader.ReadUInt16();         // font
            int cliloc = reader.ReadInt32();
            byte flags = reader.ReadByte();   // 0x01=prepend, 0x02=system
            string name = reader.ReadString(30);
            string affix = reader.ReadString();     // affix ASCII null-terminated
            string args = reader.ReadUnicodeString();

            string baseText = _languageService.ClilocFormat(cliloc, args);
            string text = (flags & 0x01) != 0 ? affix + baseText : baseText + affix;
            _journalService.AddEntry(new JournalEntry(text, name, serial, hue));
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────────────
        #region Effects & Audio Handlers
        // ─────────────────────────────────────────────────────────────────────────────────

        private void HandleGraphicalEffect(byte[] data)
        {
            // 0xC0: cmd(1) type(1) srcSerial(4) tgtSerial(4) itemID(2) srcX(2) srcY(2) srcZ(1s)
            //        tgtX(2) tgtY(2) tgtZ(1s) speed(1) duration(1) unk(2) fixedDir(1) explode(1)
            if (data.Length < 28) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0xC0
            byte type = reader.ReadByte();
            uint srcSerial = reader.ReadUInt32();
            uint tgtSerial = reader.ReadUInt32();
            ushort itemId = reader.ReadUInt16();
            ushort srcX = reader.ReadUInt16();
            ushort srcY = reader.ReadUInt16();
            sbyte srcZ = reader.ReadSByte();
            ushort tgtX = reader.ReadUInt16();
            ushort tgtY = reader.ReadUInt16();
            sbyte tgtZ = reader.ReadSByte();

            _messenger.Send(new GraphicalEffectMessage(type, srcSerial, tgtSerial, itemId,
                srcX, srcY, srcZ, tgtX, tgtY, tgtZ));
        }

        private void HandlePlayMusic(byte[] data)
        {
            // 0x6D: cmd(1) musicId(2)
            if (data.Length < 3) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0x6D
            ushort musicId = reader.ReadUInt16();
            _messenger.Send(new MusicMessage(musicId));
        }

        private void HandlePlaySoundEffect(byte[] data)
        {
            // 0x54: cmd(1) mode(1) itemID(2) unk(1) x(2) y(2) z(2)
            // mode: 0x00=no repeat, 0x01=repeat
            if (data.Length < 12) return;
            // Sound effects sono gestiti dal client — solo tracking
        }

        private void HandleBuffDebuff(byte[] data)
        {
            // 0xDF: cmd(1) serial(4) buffType(2) action(2) ...
            if (data.Length < 9) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0xDF
            uint serial = reader.ReadUInt32();
            ushort buffType = reader.ReadUInt16();
            ushort action = reader.ReadUInt16();
            
            int duration = -1;
            if (action != 0 && data.Length >= 15)
            {
                reader.Skip(4); // unused
                reader.Skip(2); // icon again
                reader.Skip(2); // action again
                duration = (int)reader.ReadUInt32();
            }

            _messenger.Send(new BuffDebuffMessage(serial, buffType, action != 0, duration));
        }

        private void HandleTestAnimation(byte[] data)
        {
            // 0xE2: cmd(1) serial(4) action(2) frameCount(2) delay(1)
            if (data.Length < 10) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0xE2
            uint serial = reader.ReadUInt32();
            short action = reader.ReadInt16();
            short frameCount = reader.ReadInt16();
            byte delay = reader.ReadByte();

            _messenger.Send(new AnimationMessage(serial, action, frameCount, delay));
        }

        private void HandleCharacterAnimation(byte[] data)
        {
            // 0x6E S2C: cmd(1) serial(4) action(2) frameCount(2) repeatCount(2) forward(1) repeat(1) delay(1)
            if (data.Length < 14) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0x6E
            uint serial = reader.ReadUInt32();
            ushort action = reader.ReadUInt16();
            ushort frameCount = reader.ReadUInt16();
            ushort repeatCount = reader.ReadUInt16();
            bool forward = reader.ReadByte() != 0;
            bool repeat = reader.ReadByte() != 0;
            byte delay = reader.ReadByte();

            _messenger.Send(new CharacterAnimationMessage(serial, action, frameCount, repeatCount, forward, repeat, delay));
        }

        private void HandleWeather(byte[] data)
        {
            // 0x65: cmd(1) type(1) count(1) temp(1)
            if (data.Length < 4) return;
            byte type = data[1];
            byte count = data[2];
            byte temp = data[3];

            _messenger.Send(new WeatherMessage(type, count, temp));
        }

        private void HandlePersonalLightLevel(byte[] data)
        {
            // 0x4E: cmd(1) serial(4) level(1) — luce locale del personaggio
            if (data.Length < 6) return;
            uint serial = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(1));
            byte level = data[5];
            if (_worldService.Player != null && serial == _worldService.Player.Serial)
                _worldService.CurrentLight = level;
        }

        private void HandleGlobalLightLevel(byte[] data)
        {
            // 0x4F: cmd(1) level(1) — luce globale del mondo
            if (data.Length < 2) return;
            _worldService.CurrentLight = data[1];
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────────────
        #region Map & Posizione Handlers
        // ─────────────────────────────────────────────────────────────────────────────────

        private void HandleMapDisplay(byte[] data)
        {
            // 0x90: cmd(1) serial(4) itemID(2) x1(2) y1(2) x2(2) y2(2) width(2) height(2) [facet(2)?]
            if (data.Length < 19) return;
            var r = new UOBufferReader(data);
            r.ReadByte();                           // 0x90
            uint   serial   = r.ReadUInt32();
            ushort itemId   = r.ReadUInt16();
            int    originX  = r.ReadUInt16();
            int    originY  = r.ReadUInt16();
            int    endX     = r.ReadUInt16();
            int    endY     = r.ReadUInt16();
            int    width    = r.ReadUInt16();
            int    height   = r.ReadUInt16();
            ushort facet    = data.Length >= 21 ? r.ReadUInt16() : (ushort)0;

            MapDataStore.Set(new MapItemData(serial, itemId, originX, originY, endX, endY, width, height, facet));
        }

        private void HandleServerChange(byte[] data)
        {
            // 0x76: cmd(1) x(2) y(2) z(2s) — posizione dopo map change
            if (data.Length < 7) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0x76
            ushort x = reader.ReadUInt16();
            ushort y = reader.ReadUInt16();
            short z = reader.ReadInt16();

            var player = _worldService.Player;
            if (player != null)
            {
                lock (player.SyncRoot)
                {
                    player.X = x;
                    player.Y = y;
                    player.Z = z;
                }
            }

            _messenger.Send(new ServerChangeMessage(x, y, z));
        }

        private void HandleMoveBoat(byte[] data)
        {
            // 0xF6: cmd(1) len(2) boatSerial(4) dir(1) speed(1) xboat(2) yboat(2) zboat(2s)
            //        count(2) [entity: serial(4) x(2) y(2) z(2s)]...
            if (data.Length < 14) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0xF6
            reader.ReadUInt16();         // length
            uint boatSerial = reader.ReadUInt32();
            reader.ReadByte();           // direction
            reader.ReadByte();           // speed
            ushort bx = reader.ReadUInt16();
            ushort by = reader.ReadUInt16();
            short bz = reader.ReadInt16();

            // Aggiorna la posizione della barca
            var boat = _worldService.FindItem(boatSerial);
            if (boat != null)
                lock (boat.SyncRoot) { boat.X = bx; boat.Y = by; boat.Z = bz; }

            if (reader.Remaining < 2) return;
            int entityCount = reader.ReadInt16();

            for (int i = 0; i < entityCount && reader.Remaining >= 10; i++)
            {
                uint entSerial = reader.ReadUInt32();
                ushort ex = reader.ReadUInt16();
                ushort ey = reader.ReadUInt16();
                short ez = reader.ReadInt16();

                // Aggiorna posizione di mobile o item
                var mob = _worldService.FindMobile(entSerial);
                if (mob != null)
                    lock (mob.SyncRoot) { mob.X = ex; mob.Y = ey; mob.Z = ez; }
                else
                {
                    var itm = _worldService.FindItem(entSerial);
                    if (itm != null)
                        lock (itm.SyncRoot) { itm.X = ex; itm.Y = ey; itm.Z = ez; }
                }
            }
        }

        private void HandlePinLocation(byte[] data)
        {
            // 0x56: cmd(1) serial(4) action(1) [unk(1) x(2) y(2) per Add]
            // Marker su mappa — solo tracking
            if (data.Length < 6) return;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────────────
        #region OPL Handler
        // ─────────────────────────────────────────────────────────────────────────────────

        private void HandleOPL(byte[] data)
        {
            // 0xD6: cmd(1) len(2) 0x0001(2) serial(4) 0x00(1) 0x00(1) hash(4)
            //        [cliloc(4) argLen(2) args[argLen]]... terminato da cliloc=0
            if (data.Length < 13) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0xD6
            reader.ReadUInt16();         // length
            reader.ReadUInt16();         // 0x0001
            uint serial = reader.ReadUInt32();
            reader.ReadByte();           // 0
            reader.ReadByte();           // 0
            int hash = reader.ReadInt32();

            var opl = new UOPropertyList(serial) { Hash = hash };

            const int MaxProperties = 256;
            int count = 0;

            while (!reader.AtEnd && reader.Remaining >= 6 && count < MaxProperties)
            {
                int cliloc = reader.ReadInt32();
                if (cliloc == 0) break;

                short argLen = reader.ReadInt16();
                string args = string.Empty;
                if (argLen > 0 && reader.Remaining >= argLen)
                    args = reader.ReadUnicodeString(argLen >> 1);

                opl.AddProperty(new UOPropertyEntry(cliloc, args));
                count++;
            }

            opl.Freeze();

            var entity = _worldService.FindEntity(serial);
            if (entity != null)
                entity.OPL = opl;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────────────
        #region Game State Handlers
        // ─────────────────────────────────────────────────────────────────────────────────

        private void HandlePlayerDeath(byte[] data)
        {
            // 0x2C: cmd(1) type(1)
            // type: 0=morte, 1=resurrezione con penalty, 2=play as ghost
            if (data.Length < 2) return;
            byte deathType = data[1];
            
            if (deathType == 0 && _configService.CurrentProfile != null && _configService.CurrentProfile.AutoScreenshotOnDeath)
            {
                _screenCapture.CaptureAsync();
            }

            _messenger.Send(new PlayerDeathMessage(deathType));
        }

        private void HandleHueResponse(byte[] data)
        {
            // 0x95 S2C: cmd(1) serial(4) itemID(2) hue(2)
            if (data.Length < 9) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0x95
            uint serial = reader.ReadUInt32();
            reader.ReadUInt16();         // itemID
            ushort hue = reader.ReadUInt16();

            var item = _worldService.FindItem(serial);
            if (item != null)
                lock (item.SyncRoot) { item.Hue = hue; }
        }

        private void HandleAsciiPrompt(byte[] data)
        {
            // 0x9A S2C: cmd(1) serial(4) promptId(4) type(4)
            // Il server richiede input testuale ASCII — clearing lato client
            if (data.Length < 13) return;
            _targetingService.SetPrompt(true);
            _messenger.Send(new AsciiPromptMessage(true));
        }

        private void HandleServerList(byte[] data)
        {
            // 0xA8: cmd(1) len(2) unk(1) count(2) [server: idx(2) name[32] full%(1) tz(1s) ip(4)]
            if (data.Length < 6) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0xA8
            reader.ReadUInt16();         // length
            reader.ReadByte();           // unknown
            ushort numServers = reader.ReadUInt16();

            // Legge il nome del primo server e lo salva nel profilo come ShardName
            if (numServers > 0 && data.Length >= reader.Position + 2 + 32)
            {
                reader.ReadUInt16(); // server index
                string serverName = System.Text.Encoding.ASCII.GetString(data, reader.Position, 32).TrimEnd('\0').Trim();
                if (!string.IsNullOrEmpty(serverName) && _configService.CurrentProfile != null)
                    _configService.CurrentProfile.ShardName = serverName;
            }
        }

        private void HandleDisplayStringQuery(byte[] data)
        {
            // 0xAB: cmd(1) serial(4) queryType(1) queryIndex(1)
            // Il server chiede al giocatore di inserire una stringa (nome personaggio, ecc.)
            if (data.Length < 7) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0xAB
            uint serial = reader.ReadUInt32();
            byte queryType = reader.ReadByte();
            int queryId = reader.ReadByte();

            StringQueryStore.Set(new UOStringQuery(serial, queryId, queryType));
            _messenger.Send(new StringQueryMessage(serial, queryId, queryType));
        }

        private void HandleProfile(byte[] data)
        {
            // 0xB8: cmd(1) len(2) serial(4) name(fixed ASCII) footer(unicode) body(unicode)
            if (data.Length < 7) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0xB8
            reader.ReadUInt16();         // length
            uint serial = reader.ReadUInt32();

            if (reader.Remaining < 1) return;
            string name = reader.ReadString(); // null-terminated ASCII

            // Aggiorniamo il nome del mobile se trovato
            var mobile = _worldService.FindMobile(serial);
            if (mobile != null && name.Length > 0)
                lock (mobile.SyncRoot) { if (mobile.Name == "Unknown") mobile.Name = name; }
        }

        private void HandleFeatures(byte[] data)
        {
            // 0xB9: cmd(1) features(2)
            if (data.Length < 3) return;
            ushort features = (ushort)((data[1] << 8) | data[2]);

            var player = _worldService.Player;
            if (player != null)
                lock (player.SyncRoot) { player.Features = features; }

            _messenger.Send(new FeaturesMessage(features));
        }

        private void HandleTrackingArrow(byte[] data)
        {
            // 0xBA: cmd(1) active(1) x(2) y(2) serial(4)
            if (data.Length < 10) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0xBA
            bool active = reader.ReadByte() != 0;
            ushort x = reader.ReadUInt16();
            ushort y = reader.ReadUInt16();
            uint targetSerial = reader.ReadUInt32();

            _messenger.Send(new TrackingArrowMessage(active, x, y, targetSerial));
        }

        private void HandleDeleteCharacter(byte[] data)
        {
            // 0x83: il server conferma la cancellazione di un personaggio. Azzeriamo il mondo.
            _multiService.Clear();
            _worldService.Clear();
        }

        private void HandleChangeSeason(byte[] data)
        {
            // 0xBC: cmd(1) season(1) [unk(1)?]
            // season: 0=Spring, 1=Summer, 2=Fall, 3=Winter, 4=Desolation
            if (data.Length < 2) return;
            byte season = data[1];

            var player = _worldService.Player;
            if (player != null)
                lock (player.SyncRoot) { player.Season = season; }

            _messenger.Send(new SeasonChangeMessage(season));
        }

        private void HandleSetUpdateRange(byte[] data)
        {
            // 0xC8: cmd(1) range(1) — range di visione/aggiornamento
            if (data.Length < 2) return;
            byte range = data[1];

            var player = _worldService.Player;
            if (player != null)
                lock (player.SyncRoot) { player.VisRange = range; }

            _messenger.Send(new UpdateRangeMessage(range));
        }

        private void HandleCustomHouseInfo(byte[] data)
        {
            // 0xD8: cmd(1) len(2) compressed(1) unk(1) serial(4) revision(4) ...
            if (data.Length < 10) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0xD8
            reader.ReadUInt16();         // length
            reader.ReadByte();           // compression type
            reader.ReadByte();           // unknown
            uint houseSerial = reader.ReadUInt32();

            // Solo tracking del serial — i dati completi sono per il client
        }

        private void HandleRunUOProtocol(byte[] data)
        {
            // 0xF0: cmd(1) len(2) type(1) ...
            // type 0x01 = party positions; type 0xFE = features negotiation
            if (data.Length < 4) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0xF0
            reader.ReadUInt16();         // length
            byte type = reader.ReadByte();

            switch (type)
            {
                case 0x01: // Custom Party — posizioni dei membri
                    while (reader.Remaining >= 4)
                    {
                        uint memberSerial = reader.ReadUInt32();
                        if (memberSerial == 0) break;
                        if (reader.Remaining < 5) break;

                        short mx = reader.ReadInt16();
                        short my = reader.ReadInt16();
                        byte mapId = reader.ReadByte();

                        var mob = _worldService.FindMobile(memberSerial);
                        if (mob != null)
                        {
                            lock (mob.SyncRoot)
                            {
                                if (mapId == (_worldService.Player?.MapId ?? 0))
                                {
                                    mob.X = mx;
                                    mob.Y = my;
                                }
                            }
                        }
                    }
                    break;

                case 0xFE: // Features Negotiation — negoziazione feature con RunUO
                    // Il client risponde con RazorNegotiateResponse — non implementato qui
                    break;
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────────────
        #region C2S Viewers
        // ─────────────────────────────────────────────────────────────────────────────────

        private void HandleMovementRequest(byte[] data)
        {
            // 0x02 C2S: cmd(1) dir(1) seq(1) key(4) — richiesta di movimento del client
            // Tracking-only: il PacketService può filtrare (stealth, no-run)
        }

        private void HandleAttackRequest(byte[] data)
        {
            // 0x05 C2S: cmd(1) serial(4) — il client attacca un target
            if (data.Length < 5) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0x05
            uint targetSerial = reader.ReadUInt32();
            // Tracking-only — Friends filter può bloccare questo nel PacketService
        }

        private void HandleClientDoubleClick(byte[] data)
        {
            // 0x06 C2S: cmd(1) serial(4) — double click su oggetto o mobile
            if (data.Length < 5) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0x06
            uint serial = reader.ReadUInt32();
            
            var player = _worldService.Player;
            if (player != null)
                lock (player.SyncRoot) { player.LastObject = serial; }
        }

        private void HandleLiftRequest(byte[] data)
        {
            // 0x07 C2S: cmd(1) serial(4) amount(2) — il client prende un item
            if (data.Length < 7) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0x07
            uint serial = reader.ReadUInt32();
            ushort amount = reader.ReadUInt16();
            // Tracking per drag-drop queue
        }

        private void HandleDropRequest(byte[] data)
        {
            // 0x08 C2S: cmd(1) serial(4) x(2) y(2) z(1s) [gridIndex(1)?] destSerial(4)
            if (data.Length < 14) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0x08
            uint serial = reader.ReadUInt32();
            short x = reader.ReadInt16();
            short y = reader.ReadInt16();
            sbyte z = reader.ReadSByte();
            // Tracking per drag-drop
        }

        private void HandleClientSingleClick(byte[] data)
        {
            // 0x09 C2S: cmd(1) serial(4) — single click (paper doll, name)
            if (data.Length < 5) return;
            // Tracking-only
        }

        private void HandleClientTextCommand(byte[] data)
        {
            // 0x12 C2S: cmd(1) len(2) type(1) command(var ASCII null-term)
            // type: 0x24=UseSkill, 0x27=CastFromBook, 0x56=CastFromMacro, 0xF4=InvokeVirtue
            if (data.Length < 4) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0x12
            reader.ReadUInt16();         // length
            byte commandType = reader.ReadByte();

            if (commandType == 0x27 || commandType == 0x56)
                _worldService.IsCasting = true;

            // Tracking per script recorder (type 0x24 = skill ID, type 0x27 = spell ID)
        }

        private void HandleEquipRequest(byte[] data)
        {
            // 0x13 C2S: cmd(1) itemSerial(4) layer(1) mobileSerial(4)
            if (data.Length < 10) return;
            // Tracking per drag-drop/equip
        }

        private void HandleRenameMobile(byte[] data)
        {
            // 0x75 C2S: cmd(1) serial(4) name[30]
            if (data.Length < 35) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0x75
            uint serial = reader.ReadUInt32();
            string name = reader.ReadString(30);
            // Tracking per script recorder
        }

        private void HandleExtendedClientCommand(byte[] data)
        {
            // 0xBF C2S: cmd(1) len(2) sub(2) + dati
            if (data.Length < 5) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0xBF
            reader.ReadUInt16();         // length
            ushort sub = reader.ReadUInt16();

            switch (sub)
            {
                case 0x1C: // Cast Spell from Macro (AOS+)
                    if (reader.Remaining >= 2)
                    {
                        ushort type = reader.ReadUInt16(); // 1 = with serial, 2 = just spell id
                        if (type == 1 && reader.Remaining >= 6)
                        {
                            uint serial = reader.ReadUInt32();
                            ushort spellId = reader.ReadUInt16();
                            _worldService.IsCasting = true;
                        }
                        else if (type == 2 && reader.Remaining >= 2)
                        {
                            ushort spellId = reader.ReadUInt16();
                            _worldService.IsCasting = true;
                        }
                    }
                    break;

                // Altri sub per C2S possono essere tracciati qui
            }
        }

        private void HandleClientEncodedPacket(byte[] data)
        {
            // 0xD7 C2S: cmd(1) len(2) serial(4) packetID(2) ...
            // packetID 0x19 = set special ability
            if (data.Length < 9) return;
            var reader = new UOBufferReader(data);
            reader.ReadByte();           // 0xD7
            reader.ReadUInt16();         // length
            reader.ReadUInt32();         // serial
            ushort packetId = reader.ReadUInt16();

            if (packetId == 0x19 && reader.Remaining >= 5)
            {
                reader.ReadByte();       // type (0 = set, 1 = clear)
                int ability = reader.ReadInt32();
                // Tracking special ability usage
            }
        }

        #endregion
    }
}
