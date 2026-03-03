using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using TMRazorImproved.Shared.Interfaces;

namespace TMRazorImproved.Core.Services
{
    /// <summary>
    /// Implementazione del servizio per l'interazione con il client UO e le DLL native.
    /// </summary>
    public partial class ClientInteropService : IClientInteropService
    {
        #region Native Structs

        public const int SHARED_BUFF_SIZE = 524288;

        [StructLayout(LayoutKind.Explicit, Size = 8 + SHARED_BUFF_SIZE)]
        internal struct SharedBuffer
        {
            [FieldOffset(0)]
            internal int Length;

            [FieldOffset(4)]
            internal int Start;

            [FieldOffset(8)]
            internal byte Buff0;
        }

        #endregion

        #region P/Invoke Definitions

        private static class NativeMethods
        {
            [DllImport("Loader.dll", EntryPoint = "Load", SetLastError = true, CharSet = CharSet.Ansi)]
            internal static unsafe extern uint Load([MarshalAs(UnmanagedType.LPStr)] string exe, [MarshalAs(UnmanagedType.LPStr)] string dll, [MarshalAs(UnmanagedType.LPStr)] string func, void* dllData, int dataLen, out uint pid);

            [DllImport("Crypt.dll", EntryPoint = "InstallLibrary", SetLastError = true)]
            internal static extern int InstallLibrary(IntPtr thisWnd, int procid, int features);

            [DllImport("Crypt.dll", EntryPoint = "Shutdown", SetLastError = true)]
            internal static extern void Shutdown(bool closeClient);

            [DllImport("Crypt.dll", EntryPoint = "FindUOWindow", SetLastError = true)]
            internal static extern IntPtr FindUOWindow();

            [DllImport("Crypt.dll", EntryPoint = "GetUOProcId", SetLastError = true)]
            internal static extern int GetUOProcId();

            [DllImport("Crypt.dll", EntryPoint = "GetSharedAddress", SetLastError = true)]
            internal static extern IntPtr GetSharedAddress();

            [DllImport("Crypt.dll", EntryPoint = "GetCommMutex", SetLastError = true)]
            internal static extern IntPtr GetCommMutex();

            [DllImport("Crypt.dll", EntryPoint = "GetPacketLength", SetLastError = true)]
            internal static unsafe extern int GetPacketLength(byte* data, int bufLen);

            [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory", SetLastError = false)]
            internal static unsafe extern void memcpy(void* dest, void* src, int len);

            [DllImport("user32.dll", SetLastError = true)]
            internal static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            internal static extern bool SetWindowText(IntPtr hWnd, string lpString);
        }

        #endregion

        public uint LaunchClient(string exePath, string dllPath)
        {
            uint pid;
            unsafe
            {
                // Avvia il client caricando Crypt.dll tramite Loader.dll
                uint result = NativeMethods.Load(exePath, dllPath, "", null, 0, out pid);
                if (result == 0 || pid == 0)
                {
                    int err = Marshal.GetLastWin32Error();
                    throw new Win32Exception(err,
                        $"Loader.dll Load() fallito per '{exePath}'. Win32 Error: {err} ({new Win32Exception(err).Message})");
                }
            }
            return pid;
        }

        public bool InstallLibrary(IntPtr windowHandle, int processId, int features)
        {
            int result = NativeMethods.InstallLibrary(windowHandle, processId, features);
            if (result == 0)
            {
                int err = Marshal.GetLastWin32Error();
                throw new Win32Exception(err,
                    $"Crypt.dll InstallLibrary() fallito per PID {processId}. Win32 Error: {err}");
            }
            return true;
        }

        public void Shutdown(bool closeClient)
        {
            NativeMethods.Shutdown(closeClient);
        }

        public IntPtr FindUOWindow()
        {
            IntPtr hwnd = NativeMethods.FindUOWindow();
            // FindUOWindow restituisce IntPtr.Zero se non trovato — non è un errore,
            // è una condizione normale (il client non è ancora avviato).
            return hwnd;
        }

        public IntPtr GetWindowHandle() => FindUOWindow();

        public int GetUOProcessId()
        {
            return NativeMethods.GetUOProcId();
        }

        public IntPtr GetSharedAddress()
        {
            return NativeMethods.GetSharedAddress();
        }

        public IntPtr GetCommMutex()
        {
            return NativeMethods.GetCommMutex();
        }

        public unsafe int GetPacketLength(byte* data, int bufLen)
        {
            return NativeMethods.GetPacketLength(data, bufLen);
        }

        public unsafe void CopyMemory(void* dest, void* src, int len)
        {
            NativeMethods.memcpy(dest, src, len);
        }

        public bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            return NativeMethods.PostMessage(hWnd, msg, wParam, lParam);
        }

        public bool SetWindowText(IntPtr hWnd, string text)
        {
            return NativeMethods.SetWindowText(hWnd, text);
        }
    }
}
