using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TMRazorImproved.Shared.Enums;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.Core.Services
{
    public class MacrosService : IMacrosService
    {
        private readonly string _macrosPath;
        private readonly IConfigService _config;
        private readonly IPacketService _packetService;
        private readonly IWorldService _worldService;
        private readonly ITargetingService _targetingService;
        private readonly ILogger<MacrosService> _logger;

        public ObservableCollection<string> MacroList { get; } = new();
        public bool IsRecording { get; private set; }
        public bool IsPlaying { get; private set; }
        public string? ActiveMacro { get; private set; }

        private CancellationTokenSource? _playCts;

        // FIX BUG-P1-05: buffer di registrazione macro
        private readonly List<string> _recordingBuffer = new();
        private readonly List<Action> _recordingUnsubscribers = new();

        public MacrosService(
            IConfigService config,
            IPacketService packetService,
            IWorldService worldService,
            ITargetingService targetingService,
            ILogger<MacrosService> logger)
        {
            _config = config;
            _packetService = packetService;
            _worldService = worldService;
            _targetingService = targetingService;
            _logger = logger;
            _macrosPath = Path.Combine(AppContext.BaseDirectory, "Macros");

            if (!Directory.Exists(_macrosPath))
                Directory.CreateDirectory(_macrosPath);
        }

        public void LoadMacros()
        {
            MacroList.Clear();
            if (Directory.Exists(_macrosPath))
            {
                var files = Directory.GetFiles(_macrosPath, "*.macro");
                foreach (var file in files)
                {
                    MacroList.Add(Path.GetFileNameWithoutExtension(file));
                }
            }
        }

        public void Play(string name)
        {
            if (IsPlaying || IsRecording) return;
            IsPlaying = true;
            ActiveMacro = name;
            
            var steps = GetSteps(name);
            _playCts = new CancellationTokenSource();

            _ = Task.Run(async () =>
            {
                try
                {
                    await ExecuteMacroAsync(steps, _playCts.Token);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Macro {Name} cancelled.", name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error playing macro {Name}", name);
                }
                finally
                {
                    IsPlaying = false;
                    ActiveMacro = null;
                }
            });
        }

        private async Task ExecuteMacroAsync(List<MacroStep> steps, CancellationToken token)
        {
            foreach (var step in steps)
            {
                if (token.IsCancellationRequested) break;
                if (!step.IsEnabled) continue;

                var cmd = step.Command.Trim();
                if (string.IsNullOrEmpty(cmd) || cmd.StartsWith("//") || cmd.StartsWith("#")) continue;

                var parts = cmd.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                string action = parts[0].ToUpperInvariant();
                string args = parts.Length > 1 ? parts[1] : "";

                await ExecuteActionAsync(action, args, token);
            }
        }

        private async Task ExecuteActionAsync(string action, string args, CancellationToken token)
        {
            switch (action)
            {
                case "PAUSE":
                    if (int.TryParse(args, out int ms))
                        await Task.Delay(ms, token);
                    break;
                case "SAY":
                    SendSpeech(args);
                    await Task.Delay(100, token);
                    break;
                case "DOUBLECLICK":
                    if (uint.TryParse(args, out uint dclickSerial))
                        SendDoubleClick(dclickSerial);
                    break;
                case "SINGLECLICK":
                    if (uint.TryParse(args, out uint sclickSerial))
                        SendSingleClick(sclickSerial);
                    break;
                case "TARGET":
                    if (uint.TryParse(args, out uint targetSerial))
                        SendTarget(targetSerial);
                    break;
                case "CAST":
                    if (int.TryParse(args, out int spellId))
                        SendCastSpell(spellId);
                    break;
                case "USESKILL":
                    if (int.TryParse(args, out int skillId))
                        SendUseSkill(skillId);
                    break;
                case "MSG":
                    SendSpeech(args);
                    break;
                case "ATTACK":
                    if (uint.TryParse(args, out uint atkSerial))
                        SendAttack(atkSerial);
                    break;
                case "DCLICK":
                    if (uint.TryParse(args, out uint dcSerial))
                        SendDoubleClick(dcSerial);
                    break;
                case "WAIT":
                    if (int.TryParse(args, out int waitMs))
                        await Task.Delay(waitMs, token);
                    else
                        await Task.Delay(500, token);
                    break;
                // FIX BUG-P2-03: WAITFORTARGET attende il vero S2C 0x6C dal server (non un semplice delay)
                case "WAITFORTARGET":
                {
                    int wftTimeout = int.TryParse(args, out int wftMs) ? wftMs : 5000;
                    var wftTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                    Action<byte[]> wftHandler = _ => wftTcs.TrySetResult(true);
                    _packetService.RegisterViewer(PacketPath.ServerToClient, 0x6C, wftHandler);
                    try
                    {
                        using var wftCts = CancellationTokenSource.CreateLinkedTokenSource(token);
                        wftCts.CancelAfter(wftTimeout);
                        await wftTcs.Task.WaitAsync(wftCts.Token);
                    }
                    catch (OperationCanceledException) { }
                    finally
                    {
                        _packetService.UnregisterViewer(PacketPath.ServerToClient, 0x6C, wftHandler);
                    }
                    break;
                }
                default:
                    _logger.LogWarning("Unknown macro action: {Action}", action);
                    break;
            }
        }

        private void SendSpeech(string text)
        {
            // FIX BUG-C05: 0x12 è ClientTextCommand (spell/skill), NON speech visibile.
            // 0xAD = UnicodeSpeech: cmd(1) len(2) type(1) hue(2) font(2) lang(4) text(Unicode BE) null(2)
            byte[] msgBytes = Encoding.BigEndianUnicode.GetBytes(text);
            int pktLen = 1 + 2 + 1 + 2 + 2 + 4 + msgBytes.Length + 2;
            byte[] pkt = new byte[pktLen];
            pkt[0] = 0xAD;
            BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(1), (ushort)pktLen);
            pkt[3] = 0x00; // type: Normal
            pkt[4] = 0x00; pkt[5] = 0x34; // hue
            pkt[6] = 0x00; pkt[7] = 0x03; // font
            pkt[8] = (byte)'E'; pkt[9] = (byte)'N'; pkt[10] = (byte)'U'; pkt[11] = 0x00; // lang "ENU\0"
            Array.Copy(msgBytes, 0, pkt, 12, msgBytes.Length);
            // null terminator (2 bytes): già 0 dalla new byte[pktLen]
            _packetService.SendToServer(pkt);
        }

        private void SendDoubleClick(uint serial)
        {
            byte[] pkt = new byte[5];
            pkt[0] = 0x06; // Double click
            pkt[1] = (byte)(serial >> 24);
            pkt[2] = (byte)(serial >> 16);
            pkt[3] = (byte)(serial >> 8);
            pkt[4] = (byte)serial;
            _packetService.SendToServer(pkt);
        }

        private void SendSingleClick(uint serial)
        {
            byte[] pkt = new byte[5];
            pkt[0] = 0x09; // Single click
            pkt[1] = (byte)(serial >> 24);
            pkt[2] = (byte)(serial >> 16);
            pkt[3] = (byte)(serial >> 8);
            pkt[4] = (byte)serial;
            _packetService.SendToServer(pkt);
        }

        private void SendTarget(uint serial)
        {
            byte[] pkt = new byte[19];
            pkt[0] = 0x6C; // Target
            pkt[1] = 0x00; // Client response
            // Cursor ID (4 bytes) - typically matched from server, using 0 for macro forced
            pkt[6] = 0x00; // Cursor type (0=object, 1=ground)
            pkt[7] = (byte)(serial >> 24);
            pkt[8] = (byte)(serial >> 16);
            pkt[9] = (byte)(serial >> 8);
            pkt[10] = (byte)serial;
            _packetService.SendToServer(pkt);
        }

        private void SendCastSpell(int spellId)
        {
            string spellStr = $"{spellId}";
            byte[] spellBytes = Encoding.ASCII.GetBytes(spellStr);
            byte[] pkt = new byte[4 + spellBytes.Length + 1];
            pkt[0] = 0x12; // Cast uses speech normally in old clients, or 0xBF for specific
            pkt[1] = (byte)(pkt.Length >> 8);
            pkt[2] = (byte)pkt.Length;
            // FIX BUG-P1-06: tipo 0x27 era il formato legacy pre-ML, 0x56 è il formato corrente
            pkt[3] = 0x56; // Type: CastSpell (formato standard UO post-ML)
            Array.Copy(spellBytes, 0, pkt, 4, spellBytes.Length);
            _packetService.SendToServer(pkt);
        }

        private void SendUseSkill(int skillId)
        {
            string skillStr = $"{skillId} 0";
            byte[] skillBytes = Encoding.ASCII.GetBytes(skillStr);
            byte[] pkt = new byte[4 + skillBytes.Length + 1];
            pkt[0] = 0x12; // Use skill (usually via Action type speech)
            pkt[1] = (byte)(pkt.Length >> 8);
            pkt[2] = (byte)pkt.Length;
            pkt[3] = 0x24; // Action type
            Array.Copy(skillBytes, 0, pkt, 4, skillBytes.Length);
            _packetService.SendToServer(pkt);
        }

        private void SendAttack(uint serial)
        {
            byte[] pkt = new byte[5];
            pkt[0] = 0x05; // Attack Req
            pkt[1] = (byte)(serial >> 24);
            pkt[2] = (byte)(serial >> 16);
            pkt[3] = (byte)(serial >> 8);
            pkt[4] = (byte)serial;
            _packetService.SendToServer(pkt);
        }

        public void Stop()
        {
            if (IsPlaying && _playCts != null)
                _playCts.Cancel();

            if (IsRecording)
            {
                // Deregistra tutti i viewer di registrazione
                foreach (var unsub in _recordingUnsubscribers) unsub();
                _recordingUnsubscribers.Clear();

                // Salva automaticamente il macro registrato se ha steps
                List<string> captured;
                lock (_recordingBuffer)
                {
                    captured = new List<string>(_recordingBuffer);
                    _recordingBuffer.Clear();
                }
                if (!string.IsNullOrEmpty(ActiveMacro) && captured.Count > 0)
                {
                    var steps = captured.Select(cmd => new MacroStep(cmd, cmd)).ToList();
                    Save(ActiveMacro, steps);
                    _logger.LogInformation("Macro '{Name}' saved with {Count} steps", ActiveMacro, steps.Count);
                }
            }

            IsPlaying = false;
            IsRecording = false;
            ActiveMacro = null;
        }

        public void Record(string name)
        {
            if (IsPlaying || IsRecording) return;
            IsRecording = true;
            ActiveMacro = name;
            _recordingBuffer.Clear();
            _recordingUnsubscribers.Clear();

            // FIX BUG-P1-05: registra viewer C2S per catturare le azioni del giocatore
            Action<byte[]> onDoubleClick = data =>
            {
                if (data.Length >= 5)
                {
                    uint serial = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(1));
                    lock (_recordingBuffer) _recordingBuffer.Add($"DOUBLECLICK {serial}");
                }
            };
            Action<byte[]> onSingleClick = data =>
            {
                if (data.Length >= 5)
                {
                    uint serial = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(1));
                    lock (_recordingBuffer) _recordingBuffer.Add($"SINGLECLICK {serial}");
                }
            };
            Action<byte[]> onSpeech = data =>
            {
                // 0xAD: cmd(1) len(2) type(1) hue(2) font(2) lang(4) text(var Unicode BE)
                if (data.Length > 13)
                {
                    try
                    {
                        string text = Encoding.BigEndianUnicode.GetString(data, 12, data.Length - 14).TrimEnd('\0');
                        if (!string.IsNullOrWhiteSpace(text))
                            lock (_recordingBuffer) _recordingBuffer.Add($"SAY {text}");
                    }
                    catch { }
                }
            };
            Action<byte[]> onTarget = data =>
            {
                if (data.Length >= 11 && data[6] == 0x00) // action 0 = target (not cancel)
                {
                    uint serial = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(7));
                    lock (_recordingBuffer) _recordingBuffer.Add($"TARGET {serial}");
                }
            };
            Action<byte[]> onTextCmd = data =>
            {
                // 0x12: cmd(1) len(2) type(1) text(var ASCII) null(1)
                if (data.Length > 5)
                {
                    byte cmdType = data[3];
                    try
                    {
                        string text = Encoding.ASCII.GetString(data, 4, data.Length - 5).TrimEnd('\0');
                        if (cmdType == 0x56 || cmdType == 0x27) // cast spell
                        {
                            if (int.TryParse(text, out int spellId))
                                lock (_recordingBuffer) _recordingBuffer.Add($"CAST {spellId}");
                        }
                        else if (cmdType == 0x24) // use skill
                        {
                            var parts = text.Split(' ');
                            if (parts.Length > 0 && int.TryParse(parts[0], out int skillId))
                                lock (_recordingBuffer) _recordingBuffer.Add($"USESKILL {skillId}");
                        }
                    }
                    catch { }
                }
            };
            Action<byte[]> onAttack = data =>
            {
                if (data.Length >= 5)
                {
                    uint serial = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(1));
                    lock (_recordingBuffer) _recordingBuffer.Add($"ATTACK {serial}");
                }
            };

            _packetService.RegisterViewer(PacketPath.ClientToServer, 0x06, onDoubleClick);
            _packetService.RegisterViewer(PacketPath.ClientToServer, 0x09, onSingleClick);
            _packetService.RegisterViewer(PacketPath.ClientToServer, 0xAD, onSpeech);
            _packetService.RegisterViewer(PacketPath.ClientToServer, 0x6C, onTarget);
            _packetService.RegisterViewer(PacketPath.ClientToServer, 0x12, onTextCmd);
            _packetService.RegisterViewer(PacketPath.ClientToServer, 0x05, onAttack);

            _recordingUnsubscribers.Add(() => _packetService.UnregisterViewer(PacketPath.ClientToServer, 0x06, onDoubleClick));
            _recordingUnsubscribers.Add(() => _packetService.UnregisterViewer(PacketPath.ClientToServer, 0x09, onSingleClick));
            _recordingUnsubscribers.Add(() => _packetService.UnregisterViewer(PacketPath.ClientToServer, 0xAD, onSpeech));
            _recordingUnsubscribers.Add(() => _packetService.UnregisterViewer(PacketPath.ClientToServer, 0x6C, onTarget));
            _recordingUnsubscribers.Add(() => _packetService.UnregisterViewer(PacketPath.ClientToServer, 0x12, onTextCmd));
            _recordingUnsubscribers.Add(() => _packetService.UnregisterViewer(PacketPath.ClientToServer, 0x05, onAttack));

            _logger.LogInformation("Recording macro '{Name}' started", name);
        }

        public void Save(string name, List<MacroStep> steps)
        {
            var path = Path.Combine(_macrosPath, $"{name}.macro");
            File.WriteAllLines(path, steps.Select(s => s.Command));
            if (!MacroList.Contains(name))
                MacroList.Add(name);
        }

        public List<MacroStep> GetSteps(string name)
        {
            var steps = new List<MacroStep>();
            var path = Path.Combine(_macrosPath, $"{name}.macro");
            
            if (File.Exists(path))
            {
                var lines = File.ReadAllLines(path);
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    
                    // Basic parsing: Command [args]
                    // Description is set same as command for now
                    steps.Add(new MacroStep(line, line));
                }
            }
            
            return steps;
        }

        public void Delete(string name)
        {
            var path = Path.Combine(_macrosPath, $"{name}.macro");
            if (File.Exists(path))
            {
                File.Delete(path);
                MacroList.Remove(name);
            }
        }

        public void Rename(string oldName, string newName)
        {
            var oldPath = Path.Combine(_macrosPath, $"{oldName}.macro");
            var newPath = Path.Combine(_macrosPath, $"{newName}.macro");
            if (File.Exists(oldPath))
            {
                File.Move(oldPath, newPath);
                var index = MacroList.IndexOf(oldName);
                if (index != -1)
                    MacroList[index] = newName;
            }
        }
    }
}
