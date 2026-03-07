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
            [StructLayout(LayoutKind.Sequential)]
            internal struct POINT
            {
                public int X;
                public int Y;
            }

            [DllImport("user32.dll")]
            internal static extern bool GetCursorPos(out POINT lpPoint);

            [DllImport("user32.dll")]
            internal static extern bool SetCursorPos(int X, int Y);

            [DllImport("user32.dll")]
            internal static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

            [DllImport("user32.dll")]
            internal static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

            [DllImport("Loader.dll", EntryPoint = "Load", SetLastError = true, CharSet = CharSet.Ansi)]
            internal static unsafe extern uint Load([MarshalAs(UnmanagedType.LPStr)] string exe, [MarshalAs(UnmanagedType.LPStr)] string dll, [MarshalAs(UnmanagedType.LPStr)] string func, void* dllData, int dataLen, out uint pid);

            [DllImport("Crypt.dll", EntryPoint = "InstallLibrary", SetLastError = true)]
            internal static extern int InstallLibrary(IntPtr thisWnd, int procid, int features);

            [DllImport("Crypt.dll", EntryPoint = "SetUOWindowHandle", SetLastError = false)]
            internal static extern void SetUOWindowHandle(IntPtr hwnd);

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

            [DllImport("Crypt.dll", EntryPoint = "WaitForWindow", SetLastError = true)]
            internal static extern void WaitForWindow(uint pid);

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

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool QueryFullProcessImageName(IntPtr hProcess, uint dwFlags,
            System.Text.StringBuilder lpExeName, ref uint lpdwSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = false)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint dwFreeType);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out uint lpThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetExitCodeThread(IntPtr hThread, out uint lpExitCode);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        // Cerca un processo già in esecuzione nella stessa directory del client.
        // Ritorna il PID se trovato con almeno una finestra, 0 altrimenti.
        public uint FindRunningGameProcess(string clientDirectory)
        {
            const uint PROCESS_QUERY_LIMITED = 0x1000;
            clientDirectory = clientDirectory.TrimEnd('\\', '/');

            foreach (var proc in System.Diagnostics.Process.GetProcesses())
            {
                try
                {
                    if (proc.Id == System.Diagnostics.Process.GetCurrentProcess().Id) continue;

                    IntPtr hProc = OpenProcess(PROCESS_QUERY_LIMITED, false, proc.Id);
                    if (hProc == IntPtr.Zero) continue;

                    try
                    {
                        var sb = new System.Text.StringBuilder(1024);
                        uint size = (uint)sb.Capacity;
                        if (!QueryFullProcessImageName(hProc, 0, sb, ref size)) continue;

                        string procDir = System.IO.Path.GetDirectoryName(sb.ToString()) ?? "";
                        if (!string.Equals(procDir, clientDirectory, StringComparison.OrdinalIgnoreCase))
                            continue;

                        // Processo nella stessa directory: ha finestre?
                        IntPtr hwnd = FindWindowByPid((uint)proc.Id);
                        if (hwnd != IntPtr.Zero)
                        {
                            uint tid = GetWindowThreadProcessId(hwnd, out uint foundPid);
                            System.Diagnostics.Trace.WriteLine(
                                $"[Interop] Found already-running game: PID {proc.Id} ({proc.ProcessName}) window 0x{hwnd.ToInt64():X} TID={tid} WndPID={foundPid}");
                            return (uint)proc.Id;
                        }
                    }
                    finally { CloseHandle(hProc); }
                }
                catch { }
            }
            return 0;
        }

        // Snapshot dei PID esistenti prima del launch, usato per rilevare il child process
        private System.Collections.Generic.HashSet<int> _beforeLaunchPids = new();
        // PID del processo UO effettivo trovato (potrebbe differire dal launcher PID)
        private uint _discoveredGamePid = 0;

        public void PrepareForLaunch()
        {
            _beforeLaunchPids = new System.Collections.Generic.HashSet<int>(
                System.Diagnostics.Process.GetProcesses().Select(p => { try { return p.Id; } catch { return -1; } }));
            _discoveredGamePid = 0;
        }

        public uint LaunchClient(string exePath, string dllPath)
        {
            // Snapshot prima del launch: serve per trovare il child process
            // nel caso in cui exePath sia un launcher che spawna il vero client
            _beforeLaunchPids = new System.Collections.Generic.HashSet<int>(
                System.Diagnostics.Process.GetProcesses().Select(p => { try { return p.Id; } catch { return -1; } }));
            _discoveredGamePid = 0;

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
            // Pre-setta hUOWindow nella DLL direttamente dall'handle trovato lato C#
            // (bypassa la logica di ricerca per finestre con class-name custom come TmClient)
            IntPtr gameWnd = FindWindowByPid((uint)processId);
            uint csharpTid = gameWnd != IntPtr.Zero ? GetWindowThreadProcessId(gameWnd, out _) : 0;
            System.Diagnostics.Trace.WriteLine($"[Interop] InstallLibrary called: PID={processId}, gameWnd=0x{gameWnd.ToInt64():X}, CS_TID={csharpTid}");

            // Always store the PID so GetUOProcessId() works even if Crypt.dll's UOProcId
            // stays 0 (e.g. TmClient has a custom window class that FindUOWindow() doesn't match).
            if (processId > 0)
                _discoveredGamePid = (uint)processId;

            if (gameWnd != IntPtr.Zero)
            {
                try
                {
                    NativeMethods.SetUOWindowHandle(gameWnd);
                    System.Diagnostics.Trace.WriteLine($"[Interop] SetUOWindowHandle(0x{gameWnd.ToInt64():X}) called.");
                }
                catch (EntryPointNotFoundException)
                {
                    System.Diagnostics.Trace.WriteLine("[Interop] SetUOWindowHandle not found - rebuild Crypt.dll required.");
                }
            }

            int result = NativeMethods.InstallLibrary(windowHandle, processId, features);
            if (result != 0) // In Crypt.h, SUCCESS è 0
            {
                System.Diagnostics.Trace.WriteLine($"[Interop] InstallLibrary FAILED/PARTIAL with code: {result} (pShared and PacketTable are still initialized)");
                // NO_HOOK (non-zero) is expected for x64 clients — shared memory IS ready.
                // Do NOT return false here; callers may still use shared memory correctly.
            }
            else
            {
                System.Diagnostics.Trace.WriteLine($"[Interop] InstallLibrary SUCCESS");
            }
            return true;
        }

        /// <summary>
        /// Injects Crypt.dll into the target process via CreateRemoteThread + LoadLibraryA,
        /// then calls AttachAndPatch to hook IAT and notify Razor when ready.
        /// Works even when WH_GETMESSAGE hooks fail (e.g. custom game clients).
        /// </summary>
        public bool InjectAndAttach(int processId, string cryptDllPath, IntPtr uoWnd, IntPtr razorWnd, int flags)
        {
            const uint PROCESS_ALL_ACCESS = 0x1F0FFF;
            const uint MEM_COMMIT_RESERVE = 0x3000;
            const uint PAGE_READWRITE = 0x04;
            const uint MEM_RELEASE = 0x8000;

            IntPtr hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, processId);
            if (hProcess == IntPtr.Zero)
            {
                System.Diagnostics.Trace.WriteLine($"[InjectAndAttach] OpenProcess({processId}) failed: {Marshal.GetLastWin32Error()}");
                return false;
            }

            try
            {
                // Step 1: Inject Crypt.dll via LoadLibraryA remote thread
                IntPtr hKernel32 = GetModuleHandle("kernel32.dll");
                IntPtr pfnLoadLibraryA = GetProcAddress(hKernel32, "LoadLibraryA");

                byte[] pathBytes = System.Text.Encoding.ASCII.GetBytes(cryptDllPath + "\0");
                IntPtr remotePath = VirtualAllocEx(hProcess, IntPtr.Zero, (uint)pathBytes.Length, MEM_COMMIT_RESERVE, PAGE_READWRITE);
                if (remotePath == IntPtr.Zero)
                {
                    System.Diagnostics.Trace.WriteLine($"[InjectAndAttach] VirtualAllocEx(path) failed: {Marshal.GetLastWin32Error()}");
                    return false;
                }

                WriteProcessMemory(hProcess, remotePath, pathBytes, pathBytes.Length, out _);

                System.Diagnostics.Trace.WriteLine($"[InjectAndAttach] Loading Crypt.dll into PID {processId} via LoadLibraryA...");
                IntPtr hThread1 = CreateRemoteThread(hProcess, IntPtr.Zero, 0, pfnLoadLibraryA, remotePath, 0, out _);
                if (hThread1 == IntPtr.Zero)
                {
                    System.Diagnostics.Trace.WriteLine($"[InjectAndAttach] CreateRemoteThread(LoadLibrary) failed: {Marshal.GetLastWin32Error()}");
                    VirtualFreeEx(hProcess, remotePath, 0, MEM_RELEASE);
                    return false;
                }

                WaitForSingleObject(hThread1, 10000);
                GetExitCodeThread(hThread1, out uint remoteHModule);
                CloseHandle(hThread1);
                VirtualFreeEx(hProcess, remotePath, 0, MEM_RELEASE);

                if (remoteHModule == 0)
                {
                    System.Diagnostics.Trace.WriteLine("[InjectAndAttach] LoadLibraryA returned 0 in remote process (DLL load failed)");
                    return false;
                }
                System.Diagnostics.Trace.WriteLine($"[InjectAndAttach] Crypt.dll loaded at 0x{remoteHModule:X} in PID {processId}");

                // Step 2: Calculate AttachAndPatch address in remote process
                // Since both are x86, the offset from module base is identical
                IntPtr localBase = GetModuleHandle("Crypt.dll");
                IntPtr localProc = GetProcAddress(localBase, "AttachAndPatch");
                if (localProc == IntPtr.Zero)
                {
                    System.Diagnostics.Trace.WriteLine("[InjectAndAttach] GetProcAddress(AttachAndPatch) failed — rebuild Crypt.dll");
                    return false;
                }
                long offset = localProc.ToInt64() - localBase.ToInt64();
                IntPtr remoteProc = new IntPtr((long)remoteHModule + offset);
                System.Diagnostics.Trace.WriteLine($"[InjectAndAttach] AttachAndPatch: local=0x{localProc.ToInt64():X} offset=0x{offset:X} remote=0x{remoteProc.ToInt64():X}");

                // Step 3: Write AttachParams into remote process
                // struct AttachParams { HWND uoWnd; HWND razorWnd; int flags; } = 12 bytes (3 x int32 on x86)
                byte[] paramsBytes = new byte[12];
                BitConverter.GetBytes((int)uoWnd).CopyTo(paramsBytes, 0);
                BitConverter.GetBytes((int)razorWnd).CopyTo(paramsBytes, 4);
                BitConverter.GetBytes(flags).CopyTo(paramsBytes, 8);

                IntPtr remoteParams = VirtualAllocEx(hProcess, IntPtr.Zero, 12, MEM_COMMIT_RESERVE, PAGE_READWRITE);
                if (remoteParams == IntPtr.Zero)
                {
                    System.Diagnostics.Trace.WriteLine($"[InjectAndAttach] VirtualAllocEx(params) failed: {Marshal.GetLastWin32Error()}");
                    return false;
                }
                WriteProcessMemory(hProcess, remoteParams, paramsBytes, 12, out _);

                // Step 4: Call AttachAndPatch via remote thread
                System.Diagnostics.Trace.WriteLine("[InjectAndAttach] Calling AttachAndPatch in remote process...");
                IntPtr hThread2 = CreateRemoteThread(hProcess, IntPtr.Zero, 0, remoteProc, remoteParams, 0, out _);
                if (hThread2 == IntPtr.Zero)
                {
                    System.Diagnostics.Trace.WriteLine($"[InjectAndAttach] CreateRemoteThread(AttachAndPatch) failed: {Marshal.GetLastWin32Error()}");
                    VirtualFreeEx(hProcess, remoteParams, 0, MEM_RELEASE);
                    return false;
                }

                WaitForSingleObject(hThread2, 15000);
                GetExitCodeThread(hThread2, out uint attachResult);
                CloseHandle(hThread2);
                VirtualFreeEx(hProcess, remoteParams, 0, MEM_RELEASE);

                System.Diagnostics.Trace.WriteLine($"[InjectAndAttach] AttachAndPatch returned {attachResult} (0=SUCCESS)");
                return attachResult == 0;
            }
            finally
            {
                CloseHandle(hProcess);
            }
        }

        public void Shutdown(bool closeClient)
        {
            NativeMethods.Shutdown(closeClient);
        }

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        [DllImport("user32.dll")] private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        // Trova la prima finestra top-level (visibile o no) appartenente al PID dato
        private IntPtr FindWindowByPid(uint pid)
        {
            IntPtr found = IntPtr.Zero;
            EnumWindows((hWnd, _) =>
            {
                GetWindowThreadProcessId(hWnd, out uint wPid);
                if (wPid == pid) { found = hWnd; return false; }
                return true;
            }, IntPtr.Zero);
            return found;
        }

        public void WaitForWindow(uint launcherPid)
        {
            System.Diagnostics.Trace.WriteLine($"[Interop] Waiting for window of PID {launcherPid}...");

            // Il WaitForWindow nativo cerca solo "Ultima Online" class name — non funziona per
            // client con classe custom (es. TmClient). Lo avviamo in background: se trova una
            // finestra "Ultima Online" imposta UOProcId nella DLL; altrimenti non blocca.
            System.Threading.ThreadPool.QueueUserWorkItem(_ => {
                try { NativeMethods.WaitForWindow(launcherPid); } catch { }
            });

            // Scan C#: aspetta fino a 30s che il PID (o un suo figlio) abbia una finestra.
            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < 30000)
            {
                // Caso 1: WaitForWindow nativo ha già trovato un client "Ultima Online"
                int dllPid = NativeMethods.GetUOProcId();
                if (dllPid != 0)
                {
                    _discoveredGamePid = (uint)dllPid;
                    System.Diagnostics.Trace.WriteLine($"[Interop] Direct UO found via DLL: PID {dllPid}");
                    System.Threading.Thread.Sleep(500);
                    return;
                }

                // Caso 2: il launcherPid stesso ha una finestra (client diretto, es. TmClient)
                IntPtr hwnd = FindWindowByPid(launcherPid);
                if (hwnd != IntPtr.Zero)
                {
                    _discoveredGamePid = launcherPid;
                    System.Diagnostics.Trace.WriteLine($"[Interop] Found window for PID {launcherPid}: 0x{hwnd.ToInt64():X}");
                    System.Threading.Thread.Sleep(500);
                    return;
                }

                // Caso 3: launcher ha spawnato un processo figlio con finestra
                foreach (var proc in System.Diagnostics.Process.GetProcesses())
                {
                    try
                    {
                        if (_beforeLaunchPids.Contains(proc.Id) || proc.Id == (int)launcherPid)
                            continue;

                        IntPtr childHwnd = FindWindowByPid((uint)proc.Id);
                        if (childHwnd != IntPtr.Zero)
                        {
                            _discoveredGamePid = (uint)proc.Id;
                            System.Diagnostics.Trace.WriteLine(
                                $"[Interop] Found spawned process PID {proc.Id} ({proc.ProcessName}) window 0x{childHwnd.ToInt64():X}");
                            System.Threading.Thread.Sleep(500);
                            return;
                        }
                    }
                    catch { }
                }

                System.Threading.Thread.Sleep(200);
            }

            System.Diagnostics.Trace.WriteLine("[Interop] WARNING: No game process found after 30s.");
        }

        [DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

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
            // Priorità: PID trovato dalla DLL (caso diretto "Ultima Online" class)
            int dllPid = NativeMethods.GetUOProcId();
            if (dllPid != 0) return dllPid;
            // Fallback: PID scoperto dal scan del child process (caso launcher)
            return (int)_discoveredGamePid;
        }

        #region Win32 API for Shared Memory

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern IntPtr OpenFileMapping(uint dwDesiredAccess, bool bInheritHandle, string lpName);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern IntPtr OpenMutex(uint dwDesiredAccess, bool bInheritHandle, string lpName);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr MapViewOfFile(IntPtr hFileMappingObject, uint dwDesiredAccess, uint dwFileOffsetHigh, uint dwFileOffsetLow, UIntPtr dwNumberOfBytesToMap);

        const uint FILE_MAP_ALL_ACCESS = 0xF001F;
        const uint MUTEX_ALL_ACCESS    = 0x1F0001;

        private IntPtr _hSharedMap  = IntPtr.Zero;
        private IntPtr _pSharedBase = IntPtr.Zero;
        private IntPtr _hMutex      = IntPtr.Zero;

        #endregion

        public IntPtr GetSharedAddress()
        {
            if (_pSharedBase != IntPtr.Zero) return _pSharedBase;

            int pid = GetUOProcessId();
            if (pid == 0) return IntPtr.Zero;

            string mapName = $"UONetSharedFM_{pid:x}";
            System.Diagnostics.Trace.WriteLine($"[Interop] Attempting to open mapping: {mapName} (PID: {pid})");
            
            _hSharedMap = OpenFileMapping(FILE_MAP_ALL_ACCESS, false, mapName);
            
            if (_hSharedMap == IntPtr.Zero)
            {
                System.Diagnostics.Trace.WriteLine($"[Interop] Failed to open {mapName}. LastError: {Marshal.GetLastWin32Error()}");
                // Riprova con PID 0
                _hSharedMap = OpenFileMapping(FILE_MAP_ALL_ACCESS, false, "UONetSharedFM_0");
            }

            if (_hSharedMap != IntPtr.Zero)
            {
                _pSharedBase = MapViewOfFile(_hSharedMap, FILE_MAP_ALL_ACCESS, 0, 0, UIntPtr.Zero);
                System.Diagnostics.Trace.WriteLine($"[Interop] Mapping opened successfully. Base Address: 0x{_pSharedBase.ToInt64():X}");
                return _pSharedBase;
            }

            return IntPtr.Zero;
        }

        public IntPtr GetCommMutex()
        {
            // Always open by name to guarantee we use the same mutex as TMRazorPlugin.
            // Crypt.dll's GetCommMutex may return a different (internal) mutex handle that
            // is NOT synchronized with the plugin's UONetSharedCOMM_{pid:x} mutex, which
            // causes a race condition on Start/Length → out-of-bounds CopyMemory → crash.
            if (_hMutex != IntPtr.Zero) return _hMutex;

            int pid = GetUOProcessId();
            if (pid == 0) return IntPtr.Zero;

            string mutexName = $"UONetSharedCOMM_{pid:x}";
            _hMutex = OpenMutex(MUTEX_ALL_ACCESS, false, mutexName);
            if (_hMutex != IntPtr.Zero)
                System.Diagnostics.Trace.WriteLine($"[Interop] CommMutex opened by name '{mutexName}': 0x{_hMutex.ToInt64():X}");
            else
                System.Diagnostics.Trace.WriteLine($"[Interop] CommMutex '{mutexName}' not found yet (LastError={Marshal.GetLastWin32Error()})");

            return _hMutex;
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

        public (int X, int Y) GetMousePosition()
        {
            NativeMethods.POINT p;
            NativeMethods.GetCursorPos(out p);
            IntPtr hwnd = GetWindowHandle();
            if (hwnd != IntPtr.Zero)
            {
                NativeMethods.ScreenToClient(hwnd, ref p);
            }
            return (p.X, p.Y);
        }

        public void SetMousePosition(int x, int y)
        {
            IntPtr hwnd = GetWindowHandle();
            if (hwnd != IntPtr.Zero)
            {
                NativeMethods.POINT p = new NativeMethods.POINT { X = x, Y = y };
                NativeMethods.ClientToScreen(hwnd, ref p);
                NativeMethods.SetCursorPos(p.X, p.Y);
            }
            else
            {
                NativeMethods.SetCursorPos(x, y);
            }
        }
    }
}
