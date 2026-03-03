using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
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

        // Indirizzi dei buffer condivisi con Crypt.dll
        private void* _inRecv;
        private void* _outRecv;
        private void* _inSend;
        private void* _outSend;
        private Mutex? _commMutex;

        // Dizionari per gestire i callback di visualizzazione (non bloccanti)
        // NOTA THREAD-SAFETY: Le entry nel ConcurrentDictionary non vengono mai rimosse
        // (TryRemove non è mai chiamato). Solo gli elementi interni alle liste vengono
        // aggiunti/rimossi sotto lock. Questo rende il pattern TryGetValue + lock sicuro.
        private readonly ConcurrentDictionary<(PacketPath, int), List<Action<byte[]>>> _viewers = new();
        
        // Dizionari per gestire i filtri (bloccanti)
        private readonly ConcurrentDictionary<(PacketPath, int), List<Func<byte[], bool>>> _filters = new();

        public event Action<PacketPath, byte[]>? PacketReceived;

        public PacketService(IMessenger messenger, IClientInteropService interop, ILogger<PacketService> logger)
        {
            _messenger = messenger;
            _interop = interop;
            _logger = logger;
        }

        private void EnsureInitialized()
        {
            if (_inRecv != null) return;

            IntPtr baseAddr = _interop.GetSharedAddress();
            if (baseAddr == IntPtr.Zero)
            {
                _logger.LogWarning("Shared address not yet available from Crypt.dll");
                return;
            }

            _logger.LogInformation("Initializing shared buffers from base address: 0x{Address:X}", baseAddr.ToInt64());

            byte* basePtr = (byte*)baseAddr.ToPointer();
            int stride = sizeof(ClientInteropService.SharedBuffer);

            _inRecv  = basePtr;
            _outRecv = basePtr + stride;
            _inSend  = basePtr + stride * 2;
            _outSend = basePtr + stride * 3;

            IntPtr hMutex = _interop.GetCommMutex();
            if (hMutex != IntPtr.Zero)
            {
                _commMutex = new Mutex { SafeWaitHandle = new SafeWaitHandle(hMutex, false) };
                _logger.LogDebug("PacketService communication mutex acquired");
            }
        }

        public void RegisterViewer(PacketPath path, int packetId, Action<byte[]> callback)
        {
            var key = (path, packetId);
            _viewers.AddOrUpdate(key, 
                _ => new List<Action<byte[]>> { callback }, 
                (_, list) => { lock(list) { list.Add(callback); } return list; });
            
            _logger.LogTrace("Registered viewer for packet {PacketId:X2} on path {Path}", packetId, path);
        }

        public void RegisterFilter(PacketPath path, int packetId, Func<byte[], bool> callback)
        {
            var key = (path, packetId);
            _filters.AddOrUpdate(key, 
                _ => new List<Func<byte[], bool>> { callback }, 
                (_, list) => { lock(list) { list.Add(callback); } return list; });
            
            _logger.LogTrace("Registered filter for packet {PacketId:X2} on path {Path}", packetId, path);
        }

        public void UnregisterViewer(PacketPath path, int packetId, Action<byte[]> callback)
        {
            if (_viewers.TryGetValue((path, packetId), out var list))
            {
                lock(list) { list.Remove(callback); }
                _logger.LogTrace("Unregistered viewer for packet {PacketId:X2} on path {Path}", packetId, path);
            }
        }

        public void UnregisterFilter(PacketPath path, int packetId, Func<byte[], bool> callback)
        {
            if (_filters.TryGetValue((path, packetId), out var list))
            {
                lock(list) { list.Remove(callback); }
                _logger.LogTrace("Unregistered filter for packet {PacketId:X2} on path {Path}", packetId, path);
            }
        }

        /// <summary>
        /// Metodo interno chiamato quando arriva un pacchetto dal client o dal server.
        /// Ritorna false se il pacchetto deve essere bloccato.
        /// </summary>
        public bool OnPacketReceived(PacketPath path, byte[] data)
        {
            if (data == null || data.Length == 0) return true;

            PacketReceived?.Invoke(path, data);

            int packetId = data[0];
            var key = (path, packetId);

            // 1. Applica i filtri (se uno solo ritorna false, il pacchetto è bloccato)
            if (_filters.TryGetValue(key, out var filters))
            {
                lock (filters)
                {
                    foreach (var filter in filters)
                    {
                        if (!filter(data)) 
                        {
                            _logger.LogTrace("Packet {PacketId:X2} on path {Path} BLOCKED by filter", packetId, path);
                            return false;
                        }
                    }
                }
            }

            // 2. Esegue i viewers (in parallelo o sequenziale, non bloccanti)
            if (_viewers.TryGetValue(key, out var viewers))
            {
                lock (viewers)
                {
                    foreach (var viewer in viewers)
                    {
                        try { viewer(data); } 
                        catch (Exception ex) 
                        { 
                            _logger.LogWarning(ex, "Viewer exception for packet {PacketId:X2} on path {Path}", packetId, path); 
                        }
                    }
                }
            }

            // 3. Notifica la UI e altri servizi asincroni tramite Messenger
            _messenger.Send(new UOPacketMessage(path, new UOPacket(data)));

            return true;
        }

        public void SendToServer(byte[] data)
        {
            if (data == null || data.Length == 0) return;

            PacketReceived?.Invoke(PacketPath.ClientToServer, data);

            EnsureInitialized();
            
            if (_outSend == null || _commMutex == null) return;

            try
            {
                if (!_commMutex.WaitOne(100)) return;

                ClientInteropService.SharedBuffer* buff = (ClientInteropService.SharedBuffer*)_outSend;
                
                int writeOffset = buff->Start + buff->Length;
                if (writeOffset + data.Length > ClientInteropService.SHARED_BUFF_SIZE)
                {
                    _logger.LogWarning("Cannot send to server: shared buffer full");
                    return;
                }

                fixed (byte* pSrc = data)
                {
                    byte* pDest = (&buff->Buff0) + writeOffset;
                    _interop.CopyMemory(pDest, pSrc, data.Length);
                    buff->Length += data.Length;
                }

                // Segnala al client che c'è un pacchetto da inviare
                IntPtr hwnd = _interop.FindUOWindow();
                if (hwnd != IntPtr.Zero)
                {
                    _interop.PostMessage(hwnd, 0x400 + 1, new IntPtr(1), IntPtr.Zero);
                }
            }
            finally
            {
                _commMutex?.ReleaseMutex();
            }
        }

        public void SendToClient(byte[] data)
        {
            if (data == null || data.Length == 0) return;

            PacketReceived?.Invoke(PacketPath.ServerToClient, data);

            EnsureInitialized();

            if (_outRecv == null || _commMutex == null) return;

            try
            {
                if (!_commMutex.WaitOne(100)) return;

                ClientInteropService.SharedBuffer* buff = (ClientInteropService.SharedBuffer*)_outRecv;

                int writeOffset = buff->Start + buff->Length;
                if (writeOffset + data.Length > ClientInteropService.SHARED_BUFF_SIZE)
                {
                    _logger.LogWarning("Cannot send to client: shared buffer full");
                    return;
                }

                fixed (byte* pSrc = data)
                {
                    byte* pDest = (&buff->Buff0) + writeOffset;
                    _interop.CopyMemory(pDest, pSrc, data.Length);
                    buff->Length += data.Length;
                }

                // Segnala al client che c'è un pacchetto da ricevere
                IntPtr hwnd = _interop.FindUOWindow();
                if (hwnd != IntPtr.Zero)
                {
                    _interop.PostMessage(hwnd, 0x400 + 1, new IntPtr(2), IntPtr.Zero);
                }
            }
            finally
            {
                _commMutex?.ReleaseMutex();
            }
        }

        public bool OnMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam)
        {
            const int WM_USER = 0x400;
            const int WM_UONETEVENT = WM_USER + 1;
            const int WM_COPYDATA = 0x4A;

            if (msg == WM_UONETEVENT)
            {
                uint type = (uint)wParam.ToInt64() & 0xFFFF;
                switch (type)
                {
                    case 1: // Send (Client -> Server)
                        HandleComm((ClientInteropService.SharedBuffer*)_inSend, (ClientInteropService.SharedBuffer*)_outSend, PacketPath.ClientToServer);
                        break;
                    case 2: // Recv (Server -> Client)
                        HandleComm((ClientInteropService.SharedBuffer*)_inRecv, (ClientInteropService.SharedBuffer*)_outRecv, PacketPath.ServerToClient);
                        break;
                    case 3: // Ready
                        EnsureInitialized();
                        break;
                }
                return true;
            }
            else if (msg == WM_COPYDATA)
            {
                // TODO: Gestione CopyData per la posizione del player
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
                    // FIX: Controllo che Start sia all'interno dei limiti del buffer
                    if (inBuff->Start < 0 || inBuff->Start >= ClientInteropService.SHARED_BUFF_SIZE)
                    {
                        _logger.LogError("Critical: inBuff->Start ({Start}) out of bounds. Resetting buffer.", inBuff->Start);
                        inBuff->Start = 0;
                        inBuff->Length = 0;
                        break;
                    }

                    byte* buffPtr = (&inBuff->Buff0) + inBuff->Start;
                    
                    // FIX: Controllo che il puntatore non superi la fine del blocco di memoria allocato
                    int remainingPhysical = ClientInteropService.SHARED_BUFF_SIZE - inBuff->Start;
                    int bufLen = Math.Min(inBuff->Length, remainingPhysical);

                    int len = _interop.GetPacketLength(buffPtr, bufLen);

                    if (len > bufLen || len <= 0)
                    {
                        // Pacchetto incompleto o corrotto alla fine del buffer fisico
                        if (len > bufLen) _logger.LogWarning("Packet truncated at buffer boundary on {Path}", path);
                        break;
                    }

                    // Estrae il pacchetto in un buffer gestito
                    byte[] packetData = new byte[len];
                    fixed (byte* pDest = packetData)
                    {
                        _interop.CopyMemory(pDest, buffPtr, len);
                    }

                    // Avanza il buffer nativo
                    inBuff->Start += len;
                    inBuff->Length -= len;

                    // Elabora il pacchetto
                    if (!OnPacketReceived(path, packetData))
                    {
                        continue;
                    }

                    // Se non filtrato, lo scrive nel buffer di uscita per Crypt.dll
                    if (outBuff->Start < 0 || outBuff->Start >= ClientInteropService.SHARED_BUFF_SIZE)
                        outBuff->Start = 0;

                    int writeOffset = outBuff->Start + outBuff->Length;
                    if (writeOffset + len > ClientInteropService.SHARED_BUFF_SIZE)
                    {
                        // Se il buffer ha spazio all'inizio ma non alla fine, resetta Start a 0 (Shift buffer)
                        if (outBuff->Length + len <= ClientInteropService.SHARED_BUFF_SIZE)
                        {
                            _interop.CopyMemory(&outBuff->Buff0, (&outBuff->Buff0) + outBuff->Start, outBuff->Length);
                            outBuff->Start = 0;
                            writeOffset = outBuff->Length;
                        }
                        else
                        {
                            _logger.LogWarning("Output buffer saturated for path {Path}. Packet discarded.", path);
                            break;
                        }
                    }

                    fixed (byte* pSrc = packetData)
                    {
                        byte* pDestBase = (&outBuff->Buff0) + writeOffset;
                        _interop.CopyMemory(pDestBase, pSrc, len);
                        outBuff->Length += len;
                    }
                }

                // Reset Start quando il buffer è vuoto per ottimizzare lo spazio
                if (inBuff->Length == 0) inBuff->Start = 0;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Fatal error in HandleComm for path {Path}", path);
            }
            finally
            {
                _commMutex?.ReleaseMutex();
            }
        }
    }
}
