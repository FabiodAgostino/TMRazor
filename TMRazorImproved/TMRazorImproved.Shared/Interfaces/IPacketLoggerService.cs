using System;
using System.Collections.Generic;
using TMRazorImproved.Shared.Enums;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.Shared.Interfaces
{
    public interface IPacketLoggerService
    {
        bool IsRecording { get; }
        string OutputPath { get; set; }
        
        void StartRecording(bool append = false);
        void StopRecording();
        
        void AddBlacklist(int packetId);
        void RemoveBlacklist(int packetId);
        void ClearBlacklist();
        
        void AddWhitelist(int packetId);
        void RemoveWhitelist(int packetId);
        void ClearWhitelist();
        
        void AddTemplate(string jsonTemplate);
        void RemoveTemplate(int packetId);
        void ClearTemplates();
        
        void ListenPacketPath(PacketPath path, bool active);
        
        // Evento per notifiche UI se necessario (anche se il VM già ascolta il PacketService direttamente per la lista live)
        event EventHandler<bool> RecordingStatusChanged;
    }
}
