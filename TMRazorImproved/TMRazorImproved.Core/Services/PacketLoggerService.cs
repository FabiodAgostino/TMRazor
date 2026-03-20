using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TMRazorImproved.Shared.Enums;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;
using TMRazorImproved.Shared.Utilities;

namespace TMRazorImproved.Core.Services
{
    public class PacketLoggerService : IPacketLoggerService, IDisposable
    {
        private readonly IPacketService _packetService;
        private readonly ILogger<PacketLoggerService> _logger;
        private readonly ConcurrentDictionary<int, PacketTemplate> _templates = new();
        private readonly HashSet<int> _blacklist = new();
        private readonly HashSet<int> _whitelist = new();
        private readonly HashSet<PacketPath> _activePaths = new() { PacketPath.ClientToServer, PacketPath.ServerToClient };
        
        private bool _isRecording;
        private string _outputPath;
        private StreamWriter? _writer;
        private readonly object _lock = new();

        public event EventHandler<bool>? RecordingStatusChanged;

        public bool IsRecording => _isRecording;

        public string OutputPath
        {
            get => _outputPath;
            set
            {
                lock (_lock)
                {
                    if (_isRecording) StopRecording();
                    _outputPath = value;
                }
            }
        }

        public PacketLoggerService(IPacketService packetService, ILogger<PacketLoggerService> logger)
        {
            _packetService = packetService;
            _logger = logger;
            
            // Default path: Desktop/Razor_Packets.log as in legacy
            _outputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Razor_Packets.log");
            
            _packetService.PacketReceived += OnPacketIntercepted;
        }

        public void StartRecording(bool append = false)
        {
            lock (_lock)
            {
                if (_isRecording) return;

                try
                {
                    string? directory = Path.GetDirectoryName(_outputPath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    _writer = new StreamWriter(_outputPath, append, Encoding.UTF8) { AutoFlush = true };
                    _writer.WriteLine();
                    _writer.WriteLine($">>>>>>>>>> Logging START {DateTime.Now} >>>>>>>>>>");
                    _writer.WriteLine();

                    _isRecording = true;
                    RecordingStatusChanged?.Invoke(this, true);
                    _logger.LogInformation("Packet recording started: {Path}", _outputPath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to start packet recording at {Path}", _outputPath);
                    throw;
                }
            }
        }

        public void StopRecording()
        {
            lock (_lock)
            {
                if (!_isRecording) return;

                try
                {
                    _writer?.WriteLine();
                    _writer?.WriteLine($"<<<<<<<<<< Logging END {DateTime.Now} <<<<<<<<<<");
                    _writer?.WriteLine();
                    _writer?.Dispose();
                    _writer = null;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while stopping packet recording");
                }
                finally
                {
                    _isRecording = false;
                    RecordingStatusChanged?.Invoke(this, false);
                    _logger.LogInformation("Packet recording stopped");
                }
            }
        }

        public void AddBlacklist(int packetId) => _blacklist.Add(packetId);
        public void RemoveBlacklist(int packetId) => _blacklist.Remove(packetId);
        public void ClearBlacklist() => _blacklist.Clear();

        public void AddWhitelist(int packetId) => _whitelist.Add(packetId);
        public void RemoveWhitelist(int packetId) => _whitelist.Remove(packetId);
        public void ClearWhitelist() => _whitelist.Clear();

        public void AddTemplate(string jsonTemplate)
        {
            try
            {
                var template = JsonConvert.DeserializeObject<PacketTemplate>(jsonTemplate);
                if (template != null && template.PacketID != -1)
                {
                    _templates[template.PacketID] = template;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse packet template JSON");
            }
        }

        public void RemoveTemplate(int packetId) => _templates.TryRemove(packetId, out _);
        public void ClearTemplates() => _templates.Clear();

        public void ListenPacketPath(PacketPath path, bool active)
        {
            if (active) _activePaths.Add(path);
            else _activePaths.Remove(path);
        }

        private void OnPacketIntercepted(PacketPath path, byte[] data)
        {
            if (!_isRecording) return;
            if (!_activePaths.Contains(path)) return;

            int packetId = data[0];

            // Filtering logic (similar to legacy)
            // If whitelist is not empty, only whitelisted packets are logged
            if (_whitelist.Any() && !_whitelist.Contains(packetId)) return;
            
            // If whitelist is empty, we check the blacklist
            if (!_whitelist.Any() && _blacklist.Contains(packetId)) return;

            LogToFile(path, data);
        }

        private void LogToFile(PacketPath path, byte[] data)
        {
            lock (_lock)
            {
                if (_writer == null) return;

                try
                {
                    string directionStr = GetDirectionString(path);

                    if (_templates.TryGetValue(data[0], out var template))
                    {
                        _writer.WriteLine($"{DateTime.Now:HH:mm:ss.ffff}: {directionStr} 0x{data[0]:X2} ({template.Name}) (Length: {data.Length})");
                        _writer.WriteLine(FormatTemplate(data, template));
                        if (template.ShowHexDump)
                            _writer.WriteLine(FormatHexDump(data));
                    }
                    else
                    {
                        _writer.WriteLine($"{DateTime.Now:HH:mm:ss.ffff}: {directionStr} 0x{data[0]:X2} (Length: {data.Length})");
                        _writer.WriteLine(FormatHexDump(data));
                    }

                    _writer.WriteLine();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error writing packet to log file");
                }
            }
        }

        private string FormatTemplate(byte[] data, PacketTemplate template)
        {
            var sb = new StringBuilder();
            int pos = 0;
            FormatFields(data, ref pos, template.Fields, sb, "  ");
            return sb.ToString();
        }

        private void FormatFields(byte[] data, ref int pos, List<FieldTemplate> fields, StringBuilder sb, string indent)
        {
            foreach (var field in fields)
            {
                if (pos >= data.Length) break;

                int len = field.Length;
                if (len <= 0) len = data.Length - pos; // consume rest

                // Clamp to available data
                len = Math.Min(len, data.Length - pos);

                string value = FormatField(data, ref pos, field, len);
                sb.AppendLine($"{indent}{field.Name}: {value}");

                // Handle nested sub-packet or repeat fields
                if (field.Fields?.Count > 0 && field.Type == FieldType.GUMP)
                    FormatFields(data, ref pos, field.Fields, sb, indent + "  ");
            }
        }

        private string FormatField(byte[] data, ref int pos, FieldTemplate field, int len)
        {
            string result;
            switch (field.Type)
            {
                case FieldType.PACKETID:
                case FieldType.HEX:
                    result = "0x" + BitConverter.ToString(data, pos, len).Replace("-", "");
                    pos += len;
                    break;

                case FieldType.UINT:
                case FieldType.SERIAL:
                case FieldType.MODELID:
                    uint uval = ReadUIntBE(data, pos, len);
                    result = len <= 2 ? uval.ToString() : $"0x{uval:X8} ({uval})";
                    pos += len;
                    break;

                case FieldType.INT:
                    int ival = (int)ReadUIntBE(data, pos, len);
                    result = ival.ToString();
                    pos += len;
                    break;

                case FieldType.BOOL:
                    result = data[pos] != 0 ? "true" : "false";
                    pos += len;
                    break;

                case FieldType.ASCII:
                case FieldType.STRING:
                case FieldType.TEXT:
                    // null-terminated or fixed length ASCII
                    int end = pos;
                    while (end < pos + len && end < data.Length && data[end] != 0) end++;
                    result = Encoding.ASCII.GetString(data, pos, end - pos);
                    pos += len;
                    break;

                case FieldType.UNICODE:
                    int uend = pos;
                    while (uend + 1 < pos + len && uend + 1 < data.Length && (data[uend] != 0 || data[uend + 1] != 0)) uend += 2;
                    result = Encoding.BigEndianUnicode.GetString(data, pos, uend - pos);
                    pos += len;
                    break;

                case FieldType.CLILOC:
                    uint cliloc = ReadUIntBE(data, pos, 4);
                    result = $"#{cliloc}";
                    pos += len;
                    break;

                case FieldType.OFFSET:
                    pos += len;
                    result = $"(skipped {len} bytes)";
                    break;

                case FieldType.DUMP:
                default:
                    result = BitConverter.ToString(data, pos, len).Replace("-", " ");
                    pos += len;
                    break;
            }
            return result;
        }

        private static uint ReadUIntBE(byte[] data, int pos, int len)
        {
            uint val = 0;
            for (int i = 0; i < len && pos + i < data.Length; i++)
                val = (val << 8) | data[pos + i];
            return val;
        }

        private string GetDirectionString(PacketPath path)
        {
            return path switch
            {
                PacketPath.ClientToServer => "Client -> Server",
                PacketPath.ServerToClient => "Server -> Client",
                PacketPath.RazorToServer => "Razor -> Server",
                PacketPath.RazorToClient => "Razor -> Client",
                PacketPath.PacketVideo => "PacketVideo -> Client",
                _ => "Unknown"
            };
        }

        private string FormatHexDump(byte[] data)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < data.Length; i += 16)
            {
                int remaining = Math.Min(16, data.Length - i);
                sb.Append($"{i:X4}: ");
                for (int j = 0; j < 16; j++)
                {
                    if (j < remaining)
                        sb.Append($"{data[i + j]:X2} ");
                    else
                        sb.Append("   ");
                }
                sb.Append("  ");
                for (int j = 0; j < remaining; j++)
                {
                    char c = (char)data[i + j];
                    sb.Append(char.IsControl(c) || c > 127 ? '.' : c);
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public void Dispose()
        {
            StopRecording();
        }
    }
}
