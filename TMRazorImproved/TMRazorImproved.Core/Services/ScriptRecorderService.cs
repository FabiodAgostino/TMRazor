using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using TMRazorImproved.Shared.Enums;
using TMRazorImproved.Shared.Interfaces;

namespace TMRazorImproved.Core.Services
{
    /// <summary>
    /// Registra le azioni del giocatore intercettando i pacchetti client-to-server
    /// e genera codice script in formato Python o UOSteam.
    ///
    /// Pacchetti intercettati:
    ///   0x05 — Attack             → Mobiles.Attack() / ATTACK
    ///   0x06 — DoubleClick        → Items.UseItem() / DCLICK
    ///   0x08 — DropRequest        → Items.Move() / moveitem
    ///   0x09 — SingleClick        → Items.SingleClick() / SINGLECLICK
    ///   0x12 — TextCommand        → Spells.Cast() / CAST  e  Player.UseSkill() / USESKILL
    ///   0x13 — EquipItem          → Items.Equip() / EQUIPITEM
    ///   0x6C — TargetResponse     → Target.TargetExecute() / TARGET
    ///   0x72 — WarMode            → Player.SetWarMode() / WARMODE
    ///   0x75 — RenameMobile       → Misc.PetRename() / rename
    ///   0x7D — MenuResponse       → Misc.MenuResponse()
    ///   0x9A — AsciiPromptResponse→ Misc.ResponsePrompt() / promptmsg
    ///   0xAC — ResponseStringQuery→ Misc.QueryStringResponse()
    ///   0xAD — UnicodeSpeech      → Player.Say() / SAY
    ///   0xB1 — GumpsResponse      → Gumps.SendAction() / replygump
    ///   0xBF — ExtendedCmd        → FLY (sub 0x32), Misc.ContextReply() (sub 0x15)
    ///   0xD7 — EncodedCommand     → Player.WeaponDisarmSA/StunSA / disarm|stun
    ///   0x02 — MoveRequest        → (omesso, troppo verboso)
    /// </summary>
    public class ScriptRecorderService : IScriptRecorderService
    {
        private readonly IPacketService _packetService;
        private readonly ILogger<ScriptRecorderService> _logger;

        private volatile bool _isRecording;
        private ScriptLanguage _language;
        private readonly List<string> _lines = new();
        private readonly List<Action> _unsubscribers = new();

        public bool IsRecording  => _isRecording;
        public ScriptLanguage Language => _language;

        public ScriptRecorderService(IPacketService packetService, ILogger<ScriptRecorderService> logger)
        {
            _packetService = packetService;
            _logger        = logger;
        }

        public void StartRecording(ScriptLanguage language)
        {
            if (_isRecording) return;
            _isRecording = true;
            _language    = language;

            lock (_lines)
            {
                _lines.Clear();
                // Header script
                _lines.Add(language == ScriptLanguage.Python
                    ? "# Script registrato da TMRazor"
                    : "// Script registrato da TMRazor");
            }

            _unsubscribers.Clear();

            // ── 0x06 DoubleClick ────────────────────────────────────────────────
            Action<byte[]> onDoubleClick = data =>
            {
                if (data.Length < 5) return;
                uint serial = BEReadUInt32(data, 1);
                AddLine(language == ScriptLanguage.Python
                    ? $"Items.UseItem(0x{serial:X})"
                    : $"DCLICK 0x{serial:X}");
            };
            Register(PacketPath.ClientToServer, 0x06, onDoubleClick);

            // ── 0x09 SingleClick ────────────────────────────────────────────────
            Action<byte[]> onSingleClick = data =>
            {
                if (data.Length < 5) return;
                uint serial = BEReadUInt32(data, 1);
                AddLine(language == ScriptLanguage.Python
                    ? $"Items.SingleClick(0x{serial:X})"
                    : $"SINGLECLICK 0x{serial:X}");
            };
            Register(PacketPath.ClientToServer, 0x09, onSingleClick);

            // ── 0x05 Attack ─────────────────────────────────────────────────────
            Action<byte[]> onAttack = data =>
            {
                if (data.Length < 5) return;
                uint serial = BEReadUInt32(data, 1);
                AddLine(language == ScriptLanguage.Python
                    ? $"Mobiles.Attack(0x{serial:X})"
                    : $"ATTACK 0x{serial:X}");
            };
            Register(PacketPath.ClientToServer, 0x05, onAttack);

            // ── 0xAD UnicodeSpeech ──────────────────────────────────────────────
            Action<byte[]> onSpeech = data =>
            {
                if (data.Length <= 13) return;
                try
                {
                    string text = Encoding.BigEndianUnicode.GetString(data, 12, data.Length - 14).TrimEnd('\0');
                    if (string.IsNullOrWhiteSpace(text)) return;
                    string escaped = text.Replace("\"", "\\\"");
                    AddLine(language == ScriptLanguage.Python
                        ? $"Player.Say(\"{escaped}\")"
                        : $"SAY \"{escaped}\"");
                }
                catch { }
            };
            Register(PacketPath.ClientToServer, 0xAD, onSpeech);

            // ── 0x6C TargetResponse ─────────────────────────────────────────────
            Action<byte[]> onTarget = data =>
            {
                // Byte 6: 0x00=object target, 0x01=location target
                if (data.Length < 11) return;
                byte kind = data.Length > 6 ? data[6] : (byte)0;
                if (kind == 0x00)
                {
                    uint serial = BEReadUInt32(data, 7);
                    if (serial == 0) return;
                    AddLine(language == ScriptLanguage.Python
                        ? $"Target.TargetExecute(0x{serial:X})"
                        : $"TARGET 0x{serial:X}");
                }
                else
                {
                    // Location target
                    ushort x = BEReadUInt16(data, 11);
                    ushort y = BEReadUInt16(data, 13);
                    byte   z = data.Length > 15 ? data[15] : (byte)0;
                    AddLine(language == ScriptLanguage.Python
                        ? $"Target.TargetExecuteRelative(0, 0x{x:X}, 0x{y:X}, {z})"
                        : $"TARGETLOC {x} {y} {z}");
                }
            };
            Register(PacketPath.ClientToServer, 0x6C, onTarget);

            // ── 0x12 TextCommand ────────────────────────────────────────────────
            Action<byte[]> onTextCmd = data =>
            {
                if (data.Length <= 5) return;
                byte cmdType = data[3];
                try
                {
                    string payload = Encoding.ASCII.GetString(data, 4, data.Length - 5).TrimEnd('\0');
                    // 0x56 = CastSpell stringa, 0x27 = CastSpell idx
                    if (cmdType == 0x56 || cmdType == 0x27)
                    {
                        if (int.TryParse(payload, out int spellId))
                            AddLine(language == ScriptLanguage.Python
                                ? $"Spells.Cast({spellId})"
                                : $"CAST {spellId}");
                    }
                    // 0x24 = UseSkill
                    else if (cmdType == 0x24)
                    {
                        var p = payload.Split(' ');
                        if (p.Length > 0 && int.TryParse(p[0], out int skillId))
                            AddLine(language == ScriptLanguage.Python
                                ? $"Player.UseSkill({skillId})"
                                : $"USESKILL {skillId}");
                    }
                }
                catch { }
            };
            Register(PacketPath.ClientToServer, 0x12, onTextCmd);

            // ── 0x72 WarMode ────────────────────────────────────────────────────
            Action<byte[]> onWarMode = data =>
            {
                if (data.Length < 2) return;
                bool enable = data[1] != 0;
                AddLine(language == ScriptLanguage.Python
                    ? $"Player.SetWarMode({(enable ? "True" : "False")})"
                    : $"WARMODE {(enable ? "on" : "off")}");
            };
            Register(PacketPath.ClientToServer, 0x72, onWarMode);

            // ── 0xBF Extended ────────────────────────────────────────────────────
            // sub 0x0032 = toggle fly
            // sub 0x0015 = ContextMenuResponse: cmd(1) len(2) sub(2) serial(4) idx(2)
            Action<byte[]> onExtended = data =>
            {
                if (data.Length < 5) return;
                ushort sub = BEReadUInt16(data, 3);
                if (sub == 0x0032)
                {
                    AddLine(language == ScriptLanguage.Python
                        ? "Player.ToggleFly()"
                        : "FLY");
                }
                else if (sub == 0x0015 && data.Length >= 11)
                {
                    uint serial = BEReadUInt32(data, 5);
                    ushort idx  = BEReadUInt16(data, 9);
                    if (language == ScriptLanguage.Python)
                    {
                        AddLine($"Misc.WaitForContext(0x{serial:X}, 10000)");
                        AddLine($"Misc.ContextReply(0x{serial:X}, {idx})");
                    }
                    else
                    {
                        AddLine($"waitforcontext 0x{serial:X} {idx} 10000");
                    }
                }
            };
            Register(PacketPath.ClientToServer, 0xBF, onExtended);

            // ── 0x13 EquipItem ──────────────────────────────────────────────────
            Action<byte[]> onEquip = data =>
            {
                // 0x13: cmd(1) serial(4) layer(1) playerSerial(4)
                if (data.Length < 10) return;
                uint serial = BEReadUInt32(data, 1);
                byte layer  = data[5];
                AddLine(language == ScriptLanguage.Python
                    ? $"Items.Equip(0x{serial:X}, {layer})"
                    : $"EQUIPITEM 0x{serial:X} {layer}");
            };
            Register(PacketPath.ClientToServer, 0x13, onEquip);

            // ── 0x08 DropRequest ────────────────────────────────────────────────
            // cmd(1) serial(4) x(2) y(2) z(1) containerSerial(4)  =  14 bytes
            Action<byte[]> onDrop = data =>
            {
                if (data.Length < 14) return;
                uint itemSerial = BEReadUInt32(data, 1);
                uint destSerial = BEReadUInt32(data, 10);
                if (destSerial == 0xFFFFFFFF)
                    AddLine(language == ScriptLanguage.Python
                        ? $"Items.DropItemGroundSelf(0x{itemSerial:X}, 1)"
                        : $"moveitem 0x{itemSerial:X} ground 0 0 0 1");
                else
                    AddLine(language == ScriptLanguage.Python
                        ? $"Items.Move(0x{itemSerial:X}, 0x{destSerial:X}, 1)"
                        : $"moveitem 0x{itemSerial:X} 0x{destSerial:X} 0 0 1");
            };
            Register(PacketPath.ClientToServer, 0x08, onDrop);

            // ── 0x75 RenameMobile ───────────────────────────────────────────────
            // cmd(1) serial(4) name(30 bytes ASCII null-term)
            Action<byte[]> onRename = data =>
            {
                if (data.Length < 6) return;
                uint serial = BEReadUInt32(data, 1);
                int nameLen = data.Length - 5;
                string name = Encoding.ASCII.GetString(data, 5, nameLen).TrimEnd('\0').Replace("\"", "\\\"");
                AddLine(language == ScriptLanguage.Python
                    ? $"Misc.PetRename(0x{serial:X}, \"{name}\")"
                    : $"rename 0x{serial:X} \"{name}\"");
            };
            Register(PacketPath.ClientToServer, 0x75, onRename);

            // ── 0x9A AsciiPromptResponse ────────────────────────────────────────
            // cmd(1) len(2) promptId(4) type(4) text(ASCII null-term)
            // type == 0 → cancel; type != 0 → OK with text
            Action<byte[]> onPrompt = data =>
            {
                if (data.Length < 11) return;
                uint type = BEReadUInt32(data, 7);
                if (type == 0)
                {
                    AddLine(language == ScriptLanguage.Python
                        ? "Misc.WaitForPrompt(10000)"
                        : "waitforprompt 10000");
                }
                else
                {
                    string text = data.Length > 11
                        ? Encoding.ASCII.GetString(data, 11, data.Length - 11).TrimEnd('\0').Replace("\"", "\\\"")
                        : string.Empty;
                    AddLine(language == ScriptLanguage.Python
                        ? "Misc.WaitForPrompt(10000)"
                        : "waitforprompt 10000");
                    AddLine(language == ScriptLanguage.Python
                        ? $"Misc.ResponsePrompt(\"{text}\")"
                        : $"promptmsg \"{text}\"");
                }
            };
            Register(PacketPath.ClientToServer, 0x9A, onPrompt);

            // ── 0xB1 GumpsResponse ──────────────────────────────────────────────
            // cmd(1) len(2) gumpId(4) typeHash(4) buttonId(4) switchCount(4) [switches] textCount(4) [textEntries]
            Action<byte[]> onGump = data =>
            {
                if (data.Length < 15) return;
                try
                {
                    uint gumpId  = BEReadUInt32(data, 3);
                    uint buttonId = BEReadUInt32(data, 11);
                    // Simplified: emit WaitForGump + SendAction (no switch/text parsing)
                    AddLine(language == ScriptLanguage.Python
                        ? $"Gumps.WaitForGump(0x{gumpId:x}, 10000)"
                        : $"waitforgump 0x{gumpId:x} 15000");
                    AddLine(language == ScriptLanguage.Python
                        ? $"Gumps.SendAction(0x{gumpId:x}, {buttonId})"
                        : $"replygump 0x{gumpId:x} {buttonId}");
                }
                catch { }
            };
            Register(PacketPath.ClientToServer, 0xB1, onGump);

            // ── 0xD7 EncodedCommand (SADisarm / SAStun) ─────────────────────────
            // cmd(1) len(2) playerSerial(4) sub(2)
            // sub 0x000B = disarm, 0x000C = stun
            Action<byte[]> onEncoded = data =>
            {
                if (data.Length < 9) return;
                ushort sub = BEReadUInt16(data, 7);
                if (sub == 0x000B)
                    AddLine(language == ScriptLanguage.Python
                        ? "Player.WeaponDisarmSA()"
                        : "disarm");
                else if (sub == 0x000C)
                    AddLine(language == ScriptLanguage.Python
                        ? "Player.WeaponStunSA()"
                        : "stun");
            };
            Register(PacketPath.ClientToServer, 0xD7, onEncoded);

            // ── 0xAC ResponseStringQuery ─────────────────────────────────────────
            // cmd(1) len(2) serial(4) promptId(4) type(1) text(ASCII)
            // type 1 = OK, 0 = cancel
            Action<byte[]> onStringQuery = data =>
            {
                if (data.Length < 12) return;
                byte accepted = data[11];
                string text = data.Length > 12
                    ? Encoding.ASCII.GetString(data, 12, data.Length - 12).TrimEnd('\0').Replace("\"", "\\\"")
                    : string.Empty;
                AddLine(language == ScriptLanguage.Python
                    ? "Misc.WaitForQueryString(10000)"
                    : "Misc.WaitForQueryString(10000)");
                AddLine(language == ScriptLanguage.Python
                    ? $"Misc.QueryStringResponse({(accepted != 0 ? "True" : "False")}, \"{text}\")"
                    : $"Misc.QueryStringResponse({(accepted != 0 ? "True" : "False")}, \"{text}\")");
            };
            Register(PacketPath.ClientToServer, 0xAC, onStringQuery);

            // ── 0x7D MenuResponse ───────────────────────────────────────────────
            // cmd(1) serial(4) menuId(2) index(2, 0=close) model(2) hue(2)  = 13 bytes
            Action<byte[]> onMenu = data =>
            {
                if (data.Length < 13) return;
                ushort index = BEReadUInt16(data, 7);
                if (index == 0) return; // menu closed without selection
                AddLine(language == ScriptLanguage.Python
                    ? "Misc.WaitForMenu(10000)"
                    : "Misc.WaitForMenu(10000)");
                AddLine(language == ScriptLanguage.Python
                    ? $"Misc.MenuResponse({index})"
                    : $"Misc.MenuResponse({index})");
            };
            Register(PacketPath.ClientToServer, 0x7D, onMenu);

            _logger.LogInformation("ScriptRecorder avviato (linguaggio: {Lang})", language);
        }

        public void StopRecording()
        {
            if (!_isRecording) return;
            foreach (var unsub in _unsubscribers) unsub();
            _unsubscribers.Clear();
            _isRecording = false;
            _logger.LogInformation("ScriptRecorder fermato. {Count} righe registrate.", _lines.Count);
        }

        public string GetRecordedScript()
        {
            lock (_lines)
                return string.Join(Environment.NewLine, _lines);
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private void AddLine(string line)
        {
            lock (_lines) _lines.Add(line);
        }

        private void Register(PacketPath path, int packetId, Action<byte[]> handler)
        {
            _packetService.RegisterViewer(path, packetId, handler);
            _unsubscribers.Add(() => _packetService.UnregisterViewer(path, packetId, handler));
        }

        private static uint BEReadUInt32(byte[] data, int offset)
            => System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(offset));

        private static ushort BEReadUInt16(byte[] data, int offset)
            => System.Buffers.Binary.BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(offset));
    }
}
