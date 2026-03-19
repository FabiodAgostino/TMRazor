using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Core.Utilities;
using TMRazorImproved.Shared.Models;
using TMRazorImproved.Shared.Enums;

namespace TMRazorImproved.Core.Services
{
    public class CommandService : ICommandService
    {
        private readonly IPacketService _packetService;
        private readonly IWorldService _worldService;
        private readonly ITargetingService _targetingService;
        private readonly IMacrosService _macrosService;
        private readonly IScriptingService _scriptingService;
        private readonly ILogger<CommandService> _logger;

        public CommandService(
            IPacketService packetService,
            IWorldService worldService,
            ITargetingService targetingService,
            IMacrosService macrosService,
            IScriptingService scriptingService,
            ILogger<CommandService> logger)
        {
            _packetService = packetService;
            _worldService = worldService;
            _targetingService = targetingService;
            _macrosService = macrosService;
            _scriptingService = scriptingService;
            _logger = logger;
        }

        public void Start()
        {
            _packetService.RegisterFilter(PacketPath.ClientToServer, 0x03, OnAsciiSpeech);
            _packetService.RegisterFilter(PacketPath.ClientToServer, 0xAD, OnUnicodeSpeech);
        }

        public void Stop()
        {
            _packetService.UnregisterFilter(PacketPath.ClientToServer, 0x03, OnAsciiSpeech);
            _packetService.UnregisterFilter(PacketPath.ClientToServer, 0xAD, OnUnicodeSpeech);
        }

        public void Dispose()
        {
            Stop();
        }

        private bool OnAsciiSpeech(byte[] data)
        {
            if (data.Length <= 8) return true;
            try
            {
                string text = Encoding.ASCII.GetString(data, 8, data.Length - 8).TrimEnd('\0');
                if (text.StartsWith("-"))
                {
                    return !ProcessCommand(text); // If processed (true), return false to block packet
                }
            }
            catch { }
            return true;
        }

        private bool OnUnicodeSpeech(byte[] data)
        {
            if (data.Length <= 12) return true;
            try
            {
                int textLen = data.Length - 14;
                if (textLen < 0) textLen = data.Length - 12; // fallback
                if (textLen > 0)
                {
                    string text = Encoding.BigEndianUnicode.GetString(data, 12, textLen).TrimEnd('\0');
                    if (text.StartsWith("-"))
                    {
                        return !ProcessCommand(text); // If processed (true), return false to block
                    }
                }
            }
            catch { }
            return true;
        }

        private bool ProcessCommand(string input)
        {
            string[] parts = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return false;

            string cmd = parts[0].ToLowerInvariant();
            string[] args = new string[parts.Length - 1];
            Array.Copy(parts, 1, args, 0, args.Length);

            switch (cmd)
            {
                case "-where":
                    HandleWhere();
                    return true;
                case "-ping":
                    HandlePing();
                    return true;
                case "-getserial":
                    HandleGetSerial();
                    return true;
                case "-inspect":
                    HandleInspect();
                    return true;
                case "-sync":
                case "-resync":
                    HandleResync();
                    return true;
                case "-echo":
                    HandleEcho(args);
                    return true;
                case "-playscript":
                    HandlePlayScript(args);
                    return true;
                case "-help":
                case "-listcommand":
                    HandleHelp();
                    return true;
                case "-setalias":
                    HandleSetAlias(args);
                    return true;
                case "-unsetalias":
                    HandleUnsetAlias(args);
                    return true;
                // Add more as needed
                default:
                    return false;
            }
        }

        private void SendClientMessage(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            byte[] textBytes = Encoding.BigEndianUnicode.GetBytes(message + "\0");
            byte[] packet = new byte[18 + textBytes.Length];
            packet[0] = 0xAE;
            packet[1] = (byte)(packet.Length >> 8);
            packet[2] = (byte)packet.Length;
            packet[3] = 0xFF; packet[4] = 0xFF; packet[5] = 0xFF; packet[6] = 0xFF; // Serial 0xFFFFFFFF
            packet[7] = 0xFF; packet[8] = 0xFF; // Body 0xFFFF
            packet[9] = 0x00; // Type
            packet[10] = (byte)(0x03B2 >> 8); packet[11] = (byte)(0x03B2 & 0xFF); // Hue
            packet[12] = 0x00; packet[13] = 0x03; // Font
            packet[14] = (byte)'E'; packet[15] = (byte)'N'; packet[16] = (byte)'U'; packet[17] = 0x00; // Lang
            Buffer.BlockCopy(textBytes, 0, packet, 18, textBytes.Length);
            _packetService.SendToClient(packet);
        }

        private void HandleWhere()
        {
            if (_worldService.Player != null)
            {
                var p = _worldService.Player;
                string msg = $"You are at: {p.X}, {p.Y}, {p.Z}";
                SendClientMessage(msg);
            }
        }

        private void HandlePing()
        {
            SendClientMessage("Ping command executed");
            _packetService.SendToServer(new byte[] { 0x73, 0x00 }); // Ping 0
        }

        private async void HandleGetSerial()
        {
            SendClientMessage("Target a player or item to get their serial number.");
            var target = await _targetingService.AcquireTargetAsync();
            if (target.Serial != 0)
            {
                SendClientMessage($"Serial: 0x{target.Serial:X8}");
            }
        }

        private async void HandleInspect()
        {
            SendClientMessage("Target a player or item to inspect.");
            var target = await _targetingService.AcquireTargetAsync();
            if (target.Serial != 0)
            {
                SendClientMessage($"Inspecting 0x{target.Serial:X8}");
                _packetService.SendToServer(PacketBuilder.SingleClick(target.Serial));
            }
        }

        private void HandleResync()
        {
            _packetService.SendToServer(new byte[] { 0x22, 0x00, 0x00 }); // MovementAck with seq 0 to resync
            SendClientMessage("Resync request sent.");
        }

        private void HandleEcho(string[] args)
        {
            string msg = string.Join(" ", args);
            SendClientMessage($"Echo: {msg}");
        }

        private void HandlePlayScript(string[] args)
        {
            if (args.Length > 0)
            {
                string scriptName = string.Join(" ", args);
                SendClientMessage($"Attempting to play script: {scriptName}");
                _ = _scriptingService.RunScript(scriptName);
            }
        }

        private async void HandleSetAlias(string[] args)
        {
            if (args.Length > 0)
            {
                string alias = args[0];
                SendClientMessage($"Target for alias '{alias}'");
                var target = await _targetingService.AcquireTargetAsync();
                if (target.Serial != 0)
                {
                    _macrosService.SetAlias(alias, target.Serial);
                    SendClientMessage($"Alias '{alias}' set to 0x{target.Serial:X8}");
                }
            }
        }

        private void HandleUnsetAlias(string[] args)
        {
            if (args.Length > 0)
            {
                string alias = args[0];
                _macrosService.RemoveAlias(alias);
                SendClientMessage($"Alias '{alias}' unset.");
            }
        }

        private void HandleHelp()
        {
            string msg = "Available commands: -where, -ping, -getserial, -inspect, -sync, -echo, -playscript, -setalias, -unsetalias";
            SendClientMessage(msg);
        }
    }
}
