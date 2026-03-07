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
        private void* _inSend;
        private void* _outRecv;   // buffer 1: unused in plugin mode (kept for SendToClient)
        private void* _outSend;   // buffer 3: unused in plugin mode (kept for SendToServer)
        private Mutex? _commMutex;
        private bool _cryptReady;
        private int  _diagTick;

        private readonly ConcurrentDictionary<(PacketPath, int), List<Action<byte[]>>> _viewers = new();
        private readonly ConcurrentDictionary<(PacketPath, int), List<Func<byte[], bool>>> _filters = new();

        public event Action<PacketPath, byte[]>? PacketReceived;

        public PacketService(IMessenger messenger, IClientInteropService interop, ILogger<PacketService> logger)
        {
            _messenger = messenger;
            _interop = interop;
            _logger = logger;

            System.Diagnostics.Trace.WriteLine("[PacketService] Constructor hit!");
            
            // Timer di fallback per la scansione della memoria.
            // AutoReset=false previene la re-entranza: il timer viene riavviato solo
            // dopo che il tick corrente è completato, evitando accessi concorrenti ai buffer.
            var fallbackTimer = new System.Timers.Timer(100);
            fallbackTimer.AutoReset = false;
            fallbackTimer.Elapsed += (s, e) =>
            {
                try
                {
                    if (_inRecv == null || _commMutex == null) EnsureInitialized();
                    if (!_cryptReady) return;

                    _diagTick++;
                    if (_diagTick % 20 == 1)
                    {
                        var cts = (ClientInteropService.SharedBuffer*)_inSend;
                        var stc = (ClientInteropService.SharedBuffer*)_inRecv;
                        System.Diagnostics.Trace.WriteLine(
                            $"[PacketService] Diag — CTS inLen={(cts != null ? cts->Length : -1)}  STC inLen={(stc != null ? stc->Length : -1)}");
                    }

                    if (_inSend != null) HandleComm((ClientInteropService.SharedBuffer*)_inSend, PacketPath.ClientToServer);
                    if (_inRecv != null) HandleComm((ClientInteropService.SharedBuffer*)_inRecv, PacketPath.ServerToClient);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine($"[PacketService] Timer Error: {ex.Message}");
                }
                finally
                {
                    // Riavvia il timer solo dopo che il tick è completato (non re-entrante)
                    try { (s as System.Timers.Timer)?.Start(); } catch { }
                }
            };
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

                    _inRecv  = basePtr;                 // buffer 0: server→client (plugin writes)
                    _outRecv = basePtr + stride;        // buffer 1: unused in plugin mode
                    _inSend  = basePtr + stride * 2;    // buffer 2: client→server (plugin writes)
                    _outSend = basePtr + stride * 3;    // buffer 3: unused in plugin mode
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

        /// <summary>
        /// Called by GeneralViewModel after InstallLibrary completes.
        /// Enables packet processing — with the length-prefix protocol, no Crypt.dll
        /// pShared initialization is required, but we still gate here to ensure we only
        /// start consuming packets after the client is fully set up.
        /// </summary>
        public void NotifyCryptReady()
        {
            _cryptReady = true;
            System.Diagnostics.Trace.WriteLine("[PacketService] CryptReady — packet processing enabled.");
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

            // Build length-prefixed frame (same format as Engine.cs WritePacket)
            byte[] frame = new byte[LENGTH_PREFIX + data.Length];
            frame[0] = (byte)( data.Length        & 0xFF);
            frame[1] = (byte)((data.Length >>  8) & 0xFF);
            frame[2] = (byte)((data.Length >> 16) & 0xFF);
            frame[3] = (byte)((data.Length >> 24) & 0xFF);
            Array.Copy(data, 0, frame, LENGTH_PREFIX, data.Length);

            try
            {
                if (!_commMutex.WaitOne(100))
                {
                    //System.Diagnostics.Trace.WriteLine("[PacketService] SendToServer: mutex timeout");
                    return;
                }
                ClientInteropService.SharedBuffer* buff = (ClientInteropService.SharedBuffer*)_outSend;
                int writeOffset = buff->Start + buff->Length;
                //System.Diagnostics.Trace.WriteLine($"[PacketService] SendToServer: writing {frame.Length}b at offset {writeOffset}, id=0x{data[0]:X2}");
                if (writeOffset + frame.Length > ClientInteropService.SHARED_BUFF_SIZE)
                {
                    //System.Diagnostics.Trace.WriteLine("[PacketService] SendToServer: buffer full");
                    return;
                }

                fixed (byte* pSrc = frame)
                {
                    byte* pDest = (&buff->Buff0) + writeOffset;
                    _interop.CopyMemory(pDest, pSrc, frame.Length);
                    buff->Length += frame.Length;
                }
                //System.Diagnostics.Trace.WriteLine($"[PacketService] SendToServer: done, buff->Length={buff->Length}");
            }
            finally { _commMutex?.ReleaseMutex(); }
        }

        public void SendToClient(byte[] data)
        {
            if (data == null || data.Length == 0) return;
            EnsureInitialized();
            if (_outRecv == null || _commMutex == null) return;

            byte[] frame = new byte[LENGTH_PREFIX + data.Length];
            frame[0] = (byte)( data.Length        & 0xFF);
            frame[1] = (byte)((data.Length >>  8) & 0xFF);
            frame[2] = (byte)((data.Length >> 16) & 0xFF);
            frame[3] = (byte)((data.Length >> 24) & 0xFF);
            Array.Copy(data, 0, frame, LENGTH_PREFIX, data.Length);

            try
            {
                if (!_commMutex.WaitOne(100)) return;
                ClientInteropService.SharedBuffer* buff = (ClientInteropService.SharedBuffer*)_outRecv;
                int writeOffset = buff->Start + buff->Length;
                if (writeOffset + frame.Length > ClientInteropService.SHARED_BUFF_SIZE) return;

                fixed (byte* pSrc = frame)
                {
                    byte* pDest = (&buff->Buff0) + writeOffset;
                    _interop.CopyMemory(pDest, pSrc, frame.Length);
                    buff->Length += frame.Length;
                }
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
                    case 1: HandleComm((ClientInteropService.SharedBuffer*)_inSend, PacketPath.ClientToServer); break;
                    case 2: HandleComm((ClientInteropService.SharedBuffer*)_inRecv, PacketPath.ServerToClient); break;
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

        // Must match Engine.cs LENGTH_PREFIX = 4
        private const int LENGTH_PREFIX = 4;

        private void HandleComm(ClientInteropService.SharedBuffer* inBuff, PacketPath path)
        {
            if (inBuff == null || _commMutex == null) return;

            // Step 1: hold mutex only long enough to snapshot+drain the shared buffer.
            // This minimises blocking the game's network thread (WritePacket WaitOne).
            byte[] snapshot = null;
            int snapshotLen = 0;
            bool acquired = false;
            try
            {
                acquired = _commMutex.WaitOne(10);
                if (!acquired) return;
                int len   = inBuff->Length;
                int start = inBuff->Start;
                // Bounds check: prevent AccessViolationException if Start/Length are corrupt
                // (can happen if the mutex wasn't correctly shared with the plugin).
                if (len >= LENGTH_PREFIX &&
                    start >= 0 &&
                    (long)start + len <= ClientInteropService.SHARED_BUFF_SIZE)
                {
                    snapshot = new byte[len];
                    fixed (byte* pDest = snapshot)
                        _interop.CopyMemory(pDest, (&inBuff->Buff0) + start, len);
                    snapshotLen    = len;
                    inBuff->Start  = 0;
                    inBuff->Length = 0;
                }
                else if (len != 0)
                {
                    // Corrupt state — reset silently
                    System.Diagnostics.Trace.WriteLine($"[PacketService] Corrupt buffer state (start={start} len={len}), resetting.");
                    inBuff->Start  = 0;
                    inBuff->Length = 0;
                }
            }
            catch (Exception ex) { System.Diagnostics.Trace.WriteLine($"[PacketService] Comm Error (copy): {ex.Message}"); return; }
            finally { if (acquired) _commMutex?.ReleaseMutex(); }

            // Step 2: parse and dispatch entirely outside the mutex — game thread never blocked here.
            if (snapshot == null || snapshotLen < LENGTH_PREFIX) return;
            try
            {
                int pos = 0;
                int dispatchCount = 0;
                while (pos + LENGTH_PREFIX <= snapshotLen)
                {
                    int packetLen = snapshot[pos] | (snapshot[pos + 1] << 8) | (snapshot[pos + 2] << 16) | (snapshot[pos + 3] << 24);
                    pos += LENGTH_PREFIX;
                    if (packetLen <= 0 || pos + packetLen > snapshotLen)
                    {
                        System.Diagnostics.Trace.WriteLine($"[PacketService] Parse BREAK at pos={pos - LENGTH_PREFIX} packetLen={packetLen} snapshotLen={snapshotLen} dispatched={dispatchCount} path={path}");
                        break;
                    }

                    byte[] packetData = new byte[packetLen];
                    Array.Copy(snapshot, pos, packetData, 0, packetLen);
                    pos += packetLen;
                    dispatchCount++;

                    OnPacketReceived(path, packetData);
                }
                if (dispatchCount > 0)
                    System.Diagnostics.Trace.WriteLine($"[PacketService] Dispatched {dispatchCount} packets ({snapshotLen}b) path={path}");
            }
            catch (Exception ex) { System.Diagnostics.Trace.WriteLine($"[PacketService] Comm Error (dispatch): {ex.Message}"); }
        }
    }
}
