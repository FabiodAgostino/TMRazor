using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TMRazorImproved.Shared.Enums;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Utilities;

namespace TMRazorImproved.UI.ViewModels
{
    public partial class PacketLoggerViewModel : ViewModelBase, IDisposable
    {
        private readonly IPacketService _packetService;
        private readonly object _lock = new();

        public ObservableCollection<PacketEntry> Packets { get; } = new();

        [ObservableProperty]
        private bool _isRecording = true;

        [ObservableProperty]
        private string _filterId = string.Empty;

        [ObservableProperty]
        private PacketEntry? _selectedPacket;

        public PacketLoggerViewModel(IPacketService packetService)
        {
            _packetService = packetService;
            BindingOperations.EnableCollectionSynchronization(Packets, _lock);
            
            _packetService.PacketReceived += OnPacketReceived;
        }

        private void OnPacketReceived(PacketPath path, byte[] data)
        {
            if (!IsRecording) return;

            // Filtraggio ID (semplice)
            if (!string.IsNullOrWhiteSpace(FilterId))
            {
                if (int.TryParse(FilterId, System.Globalization.NumberStyles.HexNumber, null, out int targetId))
                {
                    if (data[0] != targetId) return;
                }
            }

            lock (_lock)
            {
                Packets.Insert(0, new PacketEntry(path, data));
                
                if (Packets.Count > 500)
                {
                    Packets.RemoveAt(Packets.Count - 1);
                }
            }
        }

        [RelayCommand]
        private void Clear()
        {
            lock (_lock)
            {
                Packets.Clear();
                SelectedPacket = null;
            }
        }

        [RelayCommand]
        private void ToggleRecording() => IsRecording = !IsRecording;

        public void Dispose()
        {
            _packetService.PacketReceived -= OnPacketReceived;
        }
    }

    public class PacketEntry
    {
        public DateTime Timestamp { get; }
        public PacketPath Direction { get; }
        public int Length { get; }
        public byte Id { get; }
        public string Name { get; }
        public string Hex { get; }
        public string RawHex { get; }

        public PacketEntry(PacketPath direction, byte[] data)
        {
            Timestamp = DateTime.Now;
            Direction = direction;
            Length = data.Length;
            Id = data[0];
            Name = PacketNames.GetName(direction, Id);
            RawHex = BitConverter.ToString(data).Replace("-", " ");
            Hex = FormatHexDump(data);
        }

        private static string FormatHexDump(byte[] data)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < data.Length; i += 16)
            {
                int remaining = Math.Min(16, data.Length - i);
                
                // Offset
                sb.Append($"{i:X4}: ");

                // Hex
                for (int j = 0; j < 16; j++)
                {
                    if (j < remaining)
                        sb.Append($"{data[i + j]:X2} ");
                    else
                        sb.Append("   ");
                }

                sb.Append("  ");

                // ASCII
                for (int j = 0; j < remaining; j++)
                {
                    char c = (char)data[i + j];
                    sb.Append(char.IsControl(c) ? '.' : c);
                }

                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}
