using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using Microsoft.Win32.SafeHandles;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Mvvm.Messaging;
using TMRazorImproved.Shared.Enums;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;
using TMRazorImproved.Shared.Messages;

namespace TMRazorImproved.Core.Services
{
    public unsafe class PacketService : IPacketService
    {
        private readonly IMessenger _messenger;
        private readonly IClientInteropService _interop;
        private readonly ILogger<PacketService> _logger;

        private void* _inRecv;
        private void* _outRecv;
        private void* _inSend;
        private void* _outSend;
        private Mutex? _commMutex;

        private readonly ConcurrentDictionary<(PacketPath, int), List<Action<byte[]>>> _viewers = new();
        private readonly ConcurrentDictionary<(PacketPath, int), List<Func<byte[], bool>>> _filters = new();

        public event Action<PacketPath, byte[]>? PacketReceived;

        public PacketService(IMessenger messenger, IClientInteropService interop, ILogger<PacketService> logger)
        {
            _messenger = messenger;
            _interop = interop;
            _logger = logger;

            System.Diagnostics.Trace.WriteLine("[PacketService] Constructor hit!");
            
            // Timer di fallback per la scansione della memoria
            var fallbackTimer = new System.Timers.Timer(100);
            fallbackTimer.Elapsed += (s, e) => 
            {
                try 
                {
                    if (_inRecv == null) EnsureInitialized();
                    
                    if (_inSend != null) HandleComm((ClientInteropService.SharedBuffer*)_inSend, (ClientInteropService.SharedBuffer*)_outSend, PacketPath.ClientToServer);
                    if (_inRecv != null) HandleComm((ClientInteropService.SharedBuffer*)_inRecv, (ClientInteropService.SharedBuffer*)_outRecv, PacketPath.ServerToClient);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine($"[PacketService] Timer Error: {ex.Message}");
                }
            };
            fallbackTimer.AutoReset = true;
            fallbackTimer.Start();
        }

        private void EnsureInitialized()
        {
            // Re-run if either buffers or mutex are missing (they can arrive at different times
            // in the plugin approach: shared mem opens before InstallLibrary sets the mutex).
            if (_inRecv != null && _commMutex != null) return;

            IntPtr baseAddr = _interop.GetSharedAddress();

            if (baseAddr != IntPtr.Zero)
            {
                if (_inRecv == null)
                {
                    System.Diagnostics.Trace.WriteLine($"[PacketService] Shared Memory Initialized: 0x{baseAddr.ToInt64():X}");
                    _logger.LogInformation("Initializing shared buffers from base address: 0x{Address:X}", baseAddr.ToInt64());

                    byte* basePtr = (byte*)baseAddr.ToPointer();
                    int stride = sizeof(ClientInteropService.SharedBuffer);

                    _inRecv  = basePtr;
                    _outRecv = basePtr + stride;
                    _inSend  = basePtr + stride * 2;
                    _outSend = basePtr + stride * 3;
                }

                if (_commMutex == null)
                {
                    IntPtr hMutex = _interop.GetCommMutex();
                    System.Diagnostics.Trace.WriteLine($"[PacketService] CommMutex handle: 0x{hMutex.ToInt64():X}");
                    if (hMutex != IntPtr.Zero)
                    {
                        _commMutex = new Mutex { SafeWaitHandle = new SafeWaitHandle(hMutex, false) };
                        System.Diagnostics.Trace.WriteLine("[PacketService] CommMutex acquired — packet processing active.");
                    }
                }
            }
        }

        public void RegisterViewer(PacketPath path, int packetId, Action<byte[]> callback)
        {
            var key = (path, packetId);
            _viewers.AddOrUpdate(key, 
                _ => new List<Action<byte[]>> { callback }, 
                (_, list) => { lock(list) { list.Add(callback); } return list; });
        }

        public void RegisterFilter(PacketPath path, int packetId, Func<byte[], bool> callback)
        {
            var key = (path, packetId);
            _filters.AddOrUpdate(key, 
                _ => new List<Func<byte[], bool>> { callback }, 
                (_, list) => { lock(list) { list.Add(callback); } return list; });
        }

        public void UnregisterViewer(PacketPath path, int packetId, Action<byte[]> callback)
        {
            if (_viewers.TryGetValue((path, packetId), out var list))
            {
                lock(list) { list.Remove(callback); }
            }
        }

        public void UnregisterFilter(PacketPath path, int packetId, Func<byte[], bool> callback)
        {
            if (_filters.TryGetValue((path, packetId), out var list))
            {
                lock(list) { list.Remove(callback); }
            }
        }

        public bool OnPacketReceived(PacketPath path, byte[] data)
        {
            if (data == null || data.Length == 0) return true;

            System.Diagnostics.Trace.WriteLine($"[Packet] ID: 0x{data[0]:X2} ({path})");

            PacketReceived?.Invoke(path, data);

            int packetId = data[0];
            var key = (path, packetId);

            if (_filters.TryGetValue(key, out var filters))
            {
                lock (filters)
                {
                    foreach (var filter in filters)
                    {
                        if (!filter(data)) return false;
                    }
                }
            }

            if (_viewers.TryGetValue(key, out var viewers))
            {
                lock (viewers)
                {
                    foreach (var viewer in viewers)
                    {
                        try { viewer(data); } 
                        catch (Exception ex) { _logger.LogWarning(ex, "Viewer exception"); }
                    }
                }
            }

            _messenger.Send(new UOPacketMessage(path, new UOPacket(data)));
            return true;
        }

        public void SendToServer(byte[] data)
        {
            if (data == null || data.Length == 0) return;
            EnsureInitialized();
            if (_outSend == null || _commMutex == null) return;

            try
            {
                if (!_commMutex.WaitOne(100)) return;
                ClientInteropService.SharedBuffer* buff = (ClientInteropService.SharedBuffer*)_outSend;
                int writeOffset = buff->Start + buff->Length;
                if (writeOffset + data.Length > ClientInteropService.SHARED_BUFF_SIZE) return;

                fixed (byte* pSrc = data)
                {
                    byte* pDest = (&buff->Buff0) + writeOffset;
                    _interop.CopyMemory(pDest, pSrc, data.Length);
                    buff->Length += data.Length;
                }

                IntPtr hwnd = _interop.FindUOWindow();
                if (hwnd != IntPtr.Zero) _interop.PostMessage(hwnd, 0x400 + 1, new IntPtr(1), IntPtr.Zero);
            }
            finally { _commMutex?.ReleaseMutex(); }
        }

        public void SendToClient(byte[] data)
        {
            if (data == null || data.Length == 0) return;
            EnsureInitialized();
            if (_outRecv == null || _commMutex == null) return;

            try
            {
                if (!_commMutex.WaitOne(100)) return;
                ClientInteropService.SharedBuffer* buff = (ClientInteropService.SharedBuffer*)_outRecv;
                int writeOffset = buff->Start + buff->Length;
                if (writeOffset + data.Length > ClientInteropService.SHARED_BUFF_SIZE) return;

                fixed (byte* pSrc = data)
                {
                    byte* pDest = (&buff->Buff0) + writeOffset;
                    _interop.CopyMemory(pDest, pSrc, data.Length);
                    buff->Length += data.Length;
                }

                IntPtr hwnd = _interop.FindUOWindow();
                if (hwnd != IntPtr.Zero) _interop.PostMessage(hwnd, 0x400 + 1, new IntPtr(2), IntPtr.Zero);
            }
            finally { _commMutex?.ReleaseMutex(); }
        }

        public bool OnMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam)
        {
            if (msg == 0x401)
            {
                uint type = (uint)wParam.ToInt64() & 0xFFFF;
                switch (type)
                {
                    case 1: HandleComm((ClientInteropService.SharedBuffer*)_inSend, (ClientInteropService.SharedBuffer*)_outSend, PacketPath.ClientToServer); break;
                    case 2: HandleComm((ClientInteropService.SharedBuffer*)_inRecv, (ClientInteropService.SharedBuffer*)_outRecv, PacketPath.ServerToClient); break;
                    case 3:
                        System.Diagnostics.Trace.WriteLine("[PacketService] READY from TmClient — hook installed.");
                        EnsureInitialized();
                        break;
                    case 4:
                        // IError: 1=NO_UOWND, 2=NO_TID, 3=NO_HOOK, 4=NO_SHAREMEM, 6=NO_PATCH, 7=NO_COPY
                        System.Diagnostics.Trace.WriteLine($"[PacketService] NOT_READY from TmClient. IError={lParam.ToInt64()}");
                        break;
                }
                return true;
            }
            return false;
        }

        private void HandleComm(ClientInteropService.SharedBuffer* inBuff, ClientInteropService.SharedBuffer* outBuff, PacketPath path)
        {
            if (inBuff == null || outBuff == null || _commMutex == null) return;
            try
            {
                if (!_commMutex.WaitOne(10)) return;
                while (inBuff->Length > 0)
                {
                    if (inBuff->Start < 0 || inBuff->Start >= ClientInteropService.SHARED_BUFF_SIZE) { inBuff->Start = 0; inBuff->Length = 0; break; }
                    byte* buffPtr = (&inBuff->Buff0) + inBuff->Start;
                    int bufLen = Math.Min(inBuff->Length, ClientInteropService.SHARED_BUFF_SIZE - inBuff->Start);
                    int len = _interop.GetPacketLength(buffPtr, bufLen);

                    if (len > bufLen || len <= 0) break;

                    byte[] packetData = new byte[len];
                    fixed (byte* pDest = packetData) { _interop.CopyMemory(pDest, buffPtr, len); }
                    inBuff->Start += len; inBuff->Length -= len;

                    if (!OnPacketReceived(path, packetData)) continue;

                    int writeOffset = outBuff->Start + outBuff->Length;
                    if (writeOffset + len > ClientInteropService.SHARED_BUFF_SIZE)
                    {
                        if (outBuff->Length + len <= ClientInteropService.SHARED_BUFF_SIZE)
                        {
                            _interop.CopyMemory(&outBuff->Buff0, (&outBuff->Buff0) + outBuff->Start, outBuff->Length);
                            outBuff->Start = 0; writeOffset = outBuff->Length;
                        } else break;
                    }
                    fixed (byte* pSrc = packetData) { _interop.CopyMemory((&outBuff->Buff0) + writeOffset, pSrc, len); outBuff->Length += len; }
                }
                if (inBuff->Length == 0) inBuff->Start = 0;
            }
            catch (Exception ex) { System.Diagnostics.Trace.WriteLine($"[PacketService] Comm Error: {ex.Message}"); }
            finally { _commMutex?.ReleaseMutex(); }
        }
    }
}
