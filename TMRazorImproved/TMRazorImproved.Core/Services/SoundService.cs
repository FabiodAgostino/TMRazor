using System;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using TMRazorImproved.Shared.Interfaces;

namespace TMRazorImproved.Core.Services
{
    public class SoundService : ISoundService
    {
        private readonly IPacketService _packetService;
        private readonly IClientInteropService _clientInterop;
        private readonly ILogger<SoundService> _logger;

        public SoundService(IPacketService packetService, IClientInteropService clientInterop, ILogger<SoundService> logger)
        {
            _packetService = packetService;
            _clientInterop = clientInterop;
            _logger = logger;
        }

        public void PlaySound(ushort soundId, int x = 0, int y = 0, int z = 0)
        {
            // 0x54: cmd(1) flags(1) soundID(2) volume(2) x(2) y(2) z(2) = 12 bytes
            byte[] data = new byte[12];
            data[0] = 0x54;
            data[1] = 0x01; // flags
            data[2] = (byte)(soundId >> 8);
            data[3] = (byte)soundId;
            // data[4], data[5] volume (0)
            data[6] = (byte)(x >> 8);
            data[7] = (byte)x;
            data[8] = (byte)(y >> 8);
            data[9] = (byte)y;
            data[10] = (byte)(z >> 8);
            data[11] = (byte)z;
            _packetService.SendToClient(data);
        }

        public void PlayMusic(ushort musicId)
        {
            // 0x6D: cmd(1) musicId(2)
            byte[] data = new byte[3];
            data[0] = 0x6D;
            data[1] = (byte)(musicId >> 8);
            data[2] = (byte)musicId;
            _packetService.SendToClient(data);
        }

        public void StopMusic() => PlayMusic(0xFFFF);

        // ─────────────────────────────────────────────────────────────────────────────
        #region Volume Control — Windows Core Audio API (ISimpleAudioVolume per processo)
        // ─────────────────────────────────────────────────────────────────────────────

        public void SetVolume(float volume)
        {
            volume = Math.Clamp(volume, 0f, 1f);
            try
            {
                int pid = _clientInterop.GetUOProcessId();
                if (pid <= 0) return;

                var simpleVol = GetUOAudioSession(pid);
                if (simpleVol != null)
                {
                    simpleVol.SetMasterVolume(volume, Guid.Empty);
                    Marshal.ReleaseComObject(simpleVol);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "SetVolume failed");
            }
        }

        public float GetVolume()
        {
            try
            {
                int pid = _clientInterop.GetUOProcessId();
                if (pid <= 0) return 1f;

                var simpleVol = GetUOAudioSession(pid);
                if (simpleVol != null)
                {
                    simpleVol.GetMasterVolume(out float vol);
                    Marshal.ReleaseComObject(simpleVol);
                    return vol;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "GetVolume failed");
            }
            return 1f;
        }

        /// <summary>
        /// Trova la sessione audio del processo UO tramite Windows Core Audio API
        /// e ne restituisce ISimpleAudioVolume, oppure null se non trovata.
        /// </summary>
        private ISimpleAudioVolume? GetUOAudioSession(int targetPid)
        {
            IMMDeviceEnumerator? enumerator = null;
            IMMDevice? device = null;
            IAudioSessionManager2? sessionMgr = null;
            IAudioSessionEnumerator? sessionEnum = null;

            try
            {
                enumerator = (IMMDeviceEnumerator)new MMDeviceEnumeratorClass();
                enumerator.GetDefaultAudioEndpoint(0 /* eRender */, 1 /* eMultimedia */, out device);
                if (device == null) return null;

                var sessionMgrGuid = typeof(IAudioSessionManager2).GUID;
                var iUnknownGuid   = new Guid("00000000-0000-0000-C000-000000000046");
                device.Activate(ref sessionMgrGuid, 0, IntPtr.Zero, out object sessionMgrObj);
                sessionMgr = (IAudioSessionManager2)sessionMgrObj;

                sessionMgr.GetSessionEnumerator(out sessionEnum);
                sessionEnum.GetCount(out int count);

                for (int i = 0; i < count; i++)
                {
                    sessionEnum.GetSession(i, out IAudioSessionControl? ctrl);
                    if (ctrl == null) continue;

                    var ctrl2 = ctrl as IAudioSessionControl2;
                    if (ctrl2 != null)
                    {
                        ctrl2.GetProcessId(out uint pid);
                        if ((int)pid == targetPid)
                        {
                            // QI for ISimpleAudioVolume
                            var simpleVol = ctrl as ISimpleAudioVolume;
                            // Don't release ctrl — the caller owns the returned simpleVol reference
                            return simpleVol;
                        }
                    }
                    Marshal.ReleaseComObject(ctrl);
                }
            }
            finally
            {
                if (sessionEnum != null) Marshal.ReleaseComObject(sessionEnum);
                if (sessionMgr  != null) Marshal.ReleaseComObject(sessionMgr);
                if (device      != null) Marshal.ReleaseComObject(device);
                if (enumerator  != null) Marshal.ReleaseComObject(enumerator);
            }
            return null;
        }

        // ── COM Declarations ──────────────────────────────────────────────────────────

        [ComImport, Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
        private class MMDeviceEnumeratorClass { }

        [ComImport,
         Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"),
         InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMMDeviceEnumerator
        {
            [PreserveSig] int EnumAudioEndpoints(int dataFlow, int stateMask, out object devices);
            [PreserveSig] int GetDefaultAudioEndpoint(int dataFlow, int role, out IMMDevice ppDevice);
            [PreserveSig] int GetDevice([MarshalAs(UnmanagedType.LPWStr)] string pwstrId, out IMMDevice ppDevice);
            [PreserveSig] int RegisterEndpointNotificationCallback(IntPtr pClient);
            [PreserveSig] int UnregisterEndpointNotificationCallback(IntPtr pClient);
        }

        [ComImport,
         Guid("D666063F-1587-4E43-81F1-B948E807363F"),
         InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMMDevice
        {
            [PreserveSig] int Activate(ref Guid iid, int dwClsCtx, IntPtr pActivationParams,
                                       [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);
            [PreserveSig] int OpenPropertyStore(int stgmAccess, out IntPtr ppProperties);
            [PreserveSig] int GetId([MarshalAs(UnmanagedType.LPWStr)] out string ppstrId);
            [PreserveSig] int GetState(out int pdwState);
        }

        [ComImport,
         Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F"),
         InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IAudioSessionManager2
        {
            [PreserveSig] int GetAudioSessionControl(ref Guid AudioSessionGuid, int StreamFlags,
                                                     out IAudioSessionControl SessionControl);
            [PreserveSig] int GetSimpleAudioVolume(ref Guid AudioSessionGuid, int StreamFlags,
                                                   out ISimpleAudioVolume AudioVolume);
            [PreserveSig] int GetSessionEnumerator(out IAudioSessionEnumerator SessionEnum);
            [PreserveSig] int RegisterSessionNotification(IntPtr SessionNotification);
            [PreserveSig] int UnregisterSessionNotification(IntPtr SessionNotification);
            [PreserveSig] int RegisterDuckNotification([MarshalAs(UnmanagedType.LPWStr)] string sessionId, IntPtr duckNotification);
            [PreserveSig] int UnregisterDuckNotification(IntPtr duckNotification);
        }

        [ComImport,
         Guid("E2F5BB11-0570-40CA-ACDD-3AA01277DEE8"),
         InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IAudioSessionEnumerator
        {
            [PreserveSig] int GetCount(out int SessionCount);
            [PreserveSig] int GetSession(int SessionCount, out IAudioSessionControl? Session);
        }

        [ComImport,
         Guid("F4B1A599-7266-4319-A8CA-E70ACB11E8CD"),
         InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IAudioSessionControl
        {
            [PreserveSig] int GetState(out int pRetVal);
            [PreserveSig] int GetDisplayName([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);
            [PreserveSig] int SetDisplayName([MarshalAs(UnmanagedType.LPWStr)] string Value, [MarshalAs(UnmanagedType.LPStruct)] Guid EventContext);
            [PreserveSig] int GetIconPath([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);
            [PreserveSig] int SetIconPath([MarshalAs(UnmanagedType.LPWStr)] string Value, [MarshalAs(UnmanagedType.LPStruct)] Guid EventContext);
            [PreserveSig] int GetGroupingParam(out Guid pRetVal);
            [PreserveSig] int SetGroupingParam([MarshalAs(UnmanagedType.LPStruct)] Guid Override, [MarshalAs(UnmanagedType.LPStruct)] Guid EventContext);
            [PreserveSig] int RegisterAudioSessionNotification(IntPtr NewNotifications);
            [PreserveSig] int UnregisterAudioSessionNotification(IntPtr NewNotifications);
        }

        [ComImport,
         Guid("BFB7FF88-7239-4FC9-8FA2-07C950BE9C6D"),
         InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IAudioSessionControl2 : IAudioSessionControl
        {
            // Inherits IAudioSessionControl methods (9), then adds:
            [PreserveSig] int GetSessionIdentifier([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);
            [PreserveSig] int GetSessionInstanceIdentifier([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);
            [PreserveSig] int GetProcessId(out uint pRetVal);
            [PreserveSig] int IsSystemSoundsSession();
            [PreserveSig] int SetDuckingPreference(bool optOut);
        }

        [ComImport,
         Guid("87CE5498-68D6-44E5-9215-6DA47EF883D8"),
         InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface ISimpleAudioVolume
        {
            [PreserveSig] int SetMasterVolume(float fLevel, [MarshalAs(UnmanagedType.LPStruct)] Guid EventContext);
            [PreserveSig] int GetMasterVolume(out float pfLevel);
            [PreserveSig] int SetMute([MarshalAs(UnmanagedType.Bool)] bool bMute, [MarshalAs(UnmanagedType.LPStruct)] Guid EventContext);
            [PreserveSig] int GetMute([MarshalAs(UnmanagedType.Bool)] out bool pbMute);
        }

        #endregion
    }
}
