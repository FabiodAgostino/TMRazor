using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<MacrosService> _logger;

        public ObservableCollection<string> MacroList { get; } = new();
        public bool IsRecording { get; private set; }
        public bool IsPlaying { get; private set; }
        public string? ActiveMacro { get; private set; }

        private CancellationTokenSource? _playCts;

        public MacrosService(IConfigService config, IPacketService packetService, IWorldService worldService, ILogger<MacrosService> logger)
        {
            _config = config;
            _packetService = packetService;
            _worldService = worldService;
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
                // Basic actions placeholder (implementing 10+ core actions)
                case "WAIT":
                case "WAITFORTARGET":
                    await Task.Delay(500, token); // simple wait implementation
                    break;
                default:
                    _logger.LogWarning("Unknown macro action: {Action}", action);
                    break;
            }
        }

        private void SendSpeech(string text)
        {
            byte[] textBytes = Encoding.UTF8.GetBytes(text);
            byte[] pkt = new byte[8 + textBytes.Length + 1];
            pkt[0] = 0x12; // Cmd
            pkt[1] = (byte)(pkt.Length >> 8);
            pkt[2] = (byte)pkt.Length;
            pkt[3] = 0x00; // Type (Normal)
            pkt[4] = 0x00; // Hue (default)
            pkt[5] = 0x34; // Hue
            pkt[6] = 0x00; // Font
            pkt[7] = 0x03; // Font
            Array.Copy(textBytes, 0, pkt, 8, textBytes.Length);
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
            pkt[3] = 0x27; // Type: Cast Spell (classic client representation for internal speech)
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
            {
                _playCts.Cancel();
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
            // TODO: Implement recording logic (intercepting actions/packets)
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
