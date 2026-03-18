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
    ///   0x06 — DoubleClick  → Items.UseItem() / DCLICK
    ///   0x09 — SingleClick  → Items.SingleClick() / SINGLECLICK
    ///   0x05 — Attack       → Mobiles.Attack() / ATTACK
    ///   0xAD — UnicodeSpeech → Player.Say() / SAY
    ///   0x6C — TargetResponse → Target.TargetExecute() / TARGET
    ///   0x12 — TextCommand  → Spells.Cast() / CAST  e  Player.UseSkill() / USESKILL
    ///   0x72 — WarMode      → Player.SetWarMode() / WARMODE
    ///   0xBF — ExtendedCmd  → Player.ToggleFly() / FLY|LAND  (sub 0x32)
    ///   0x02 — MoveRequest  → (omesso, troppo verboso)
    ///   0x13 — EquipItem    → Items.Equip() / EQUIPITEM
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

            // ── 0xBF Extended (sub 0x0032 = toggle fly) ─────────────────────────
            Action<byte[]> onExtended = data =>
            {
                if (data.Length >= 5 && data[3] == 0x00 && data[4] == 0x32)
                    AddLine(language == ScriptLanguage.Python
                        ? "Player.ToggleFly()"
                        : "FLY");
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
