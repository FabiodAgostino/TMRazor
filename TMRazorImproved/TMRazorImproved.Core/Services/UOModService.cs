using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TMRazorImproved.Shared.Interfaces;

namespace TMRazorImproved.Core.Services
{
    public class UOModService : IUOModService
    {
        private readonly ILogger<UOModService> _logger;
        private readonly IClientInteropService _interopService;

        private IntPtr _modHandle = IntPtr.Zero;
        private TaskCompletionSource<bool> _handleReady = new TaskCompletionSource<bool>();

        // Privileges for process opening
        private const int PROCESS_CREATE_THREAD = 0x0002;
        private const int PROCESS_QUERY_INFORMATION = 0x0400;
        private const int PROCESS_VM_OPERATION = 0x0008;
        private const int PROCESS_VM_WRITE = 0x0020;
        private const int PROCESS_VM_READ = 0x0010;

        private const uint MEM_COMMIT = 0x00001000;
        private const uint MEM_RESERVE = 0x00002000;
        private const uint PAGE_READWRITE = 4;

        private enum PatchMessages
        {
            PM_INSTALL = 0x400 + 666, // WM_USER + 666
            PM_INFO,
            PM_ENABLE,
            PM_DISABLE,
            PM_VIEW_RANGE_VALUE
        }

        public UOModService(ILogger<UOModService> logger, IClientInteropService interopService)
        {
            _logger = logger;
            _interopService = interopService;
        }

        public void InjectUoMod(int pid)
        {
            _handleReady = new TaskCompletionSource<bool>();
            string dllPath = Path.Combine(AppContext.BaseDirectory, "UOMod.dll");
            if (!File.Exists(dllPath))
            {
                _logger.LogWarning("UOMod.dll non trovata al percorso: {Path}", dllPath);
                _handleReady.TrySetResult(false);
                return;
            }

            IntPtr hProcess = OpenProcess(
                PROCESS_QUERY_INFORMATION | PROCESS_CREATE_THREAD | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ,
                false, 
                pid);

            if (hProcess == IntPtr.Zero)
            {
                _logger.LogError("Impossibile aprire il processo UO per l'iniezione (PID {PID}).", pid);
                _handleReady.TrySetResult(false);
                return;
            }

            IntPtr pszLibFileRemote = IntPtr.Zero;
            IntPtr hThread = IntPtr.Zero;

            try
            {
                int cb = (dllPath.Length + 1) * Marshal.SystemDefaultCharSize;
                pszLibFileRemote = VirtualAllocEx(hProcess, IntPtr.Zero, (uint)cb, MEM_COMMIT, PAGE_READWRITE);

                if (pszLibFileRemote == IntPtr.Zero)
                {
                    _logger.LogError("VirtualAllocEx fallita.");
                    _handleReady.TrySetResult(false);
                    return;
                }

                byte[] bytes = Encoding.Default.GetBytes(dllPath + '\0');
                if (!WriteProcessMemory(hProcess, pszLibFileRemote, bytes, (uint)bytes.Length, out _))
                {
                    _logger.LogError("WriteProcessMemory fallita.");
                    _handleReady.TrySetResult(false);
                    return;
                }

                IntPtr pfnThreadRtn = GetProcAddress(GetModuleHandle("Kernel32.dll"), "LoadLibraryA");
                if (pfnThreadRtn == IntPtr.Zero)
                {
                    _logger.LogError("Impossibile trovare LoadLibraryA.");
                    _handleReady.TrySetResult(false);
                    return;
                }

                hThread = CreateRemoteThread(hProcess, IntPtr.Zero, 0, pfnThreadRtn, pszLibFileRemote, 0, IntPtr.Zero);
                if (hThread == IntPtr.Zero)
                {
                    _logger.LogError("CreateRemoteThread fallita.");
                    _handleReady.TrySetResult(false);
                    return;
                }

                WaitForSingleObject(hThread, unchecked((int)0xFFFFFFFF)); // INFINITE
                _logger.LogInformation("UOMod.dll iniettata con successo.");

                // Attendi e trova la finestra
                Task.Run(async () =>
                {
                    for (int i = 0; i < 20; i++) // Prova per 10 secondi
                    {
                        await Task.Delay(500);
                        IntPtr hwnd = _interopService.GetWindowHandle();
                        if (hwnd != IntPtr.Zero)
                        {
                            string windowName = "UOModWindow_" + hwnd.ToString("x8").ToUpper();
                            _modHandle = FindWindow(null, windowName);

                            if (_modHandle != IntPtr.Zero)
                            {
                                SendMessage(_modHandle, (int)PatchMessages.PM_VIEW_RANGE_VALUE, IntPtr.Zero, new IntPtr(0));
                                SendMessage(_modHandle, (int)PatchMessages.PM_INFO, IntPtr.Zero, new IntPtr(-1)); // 0xFFFFFFFF
                                _logger.LogInformation("Connessione a UOModWindow stabilita.");
                                _handleReady.TrySetResult(true);
                                return;
                            }
                        }
                    }
                    _logger.LogWarning("UOModWindow non trovata dopo 10 secondi.");
                    _handleReady.TrySetResult(false);
                });
            }
            finally
            {
                if (pszLibFileRemote != IntPtr.Zero)
                    VirtualFreeEx(hProcess, pszLibFileRemote, 0, 0x8000); // MEM_RELEASE

                if (hThread != IntPtr.Zero)
                    CloseHandle(hThread);

                if (hProcess != IntPtr.Zero)
                    CloseHandle(hProcess);
            }
        }

        public void EnablePatch(UOPatchType patch, bool enable)
        {
            if (_modHandle == IntPtr.Zero)
            {
                _logger.LogWarning("Impossibile abilitare {Patch}: UOModWindow non agganciata.", patch);
                return;
            }

            int msg = enable ? (int)PatchMessages.PM_ENABLE : (int)PatchMessages.PM_DISABLE;
            SendMessage(_modHandle, msg, IntPtr.Zero, new IntPtr((int)patch));
        }

        public void SetViewRange(int value)
        {
            if (_modHandle == IntPtr.Zero) return;
            SendMessage(_modHandle, (int)PatchMessages.PM_VIEW_RANGE_VALUE, IntPtr.Zero, new IntPtr(value));
        }

        public async void ApplyProfilePatches(TMRazorImproved.Shared.Models.Config.UserProfile profile)
        {
            // Attendi l'handle se l'iniezione è in corso
            if (_modHandle == IntPtr.Zero)
            {
                await _handleReady.Task;
            }

            if (_modHandle == IntPtr.Zero) return;

            EnablePatch(UOPatchType.FPS, profile.UoModFps);
            EnablePatch(UOPatchType.Stamina, profile.UoModStamina);
            EnablePatch(UOPatchType.AlwaysLight, profile.UoModAlwaysLight);
            EnablePatch(UOPatchType.PaperdollSlots, profile.UoModPaperdollSlots);
            EnablePatch(UOPatchType.SplashScreen, profile.UoModSplashScreen);
            EnablePatch(UOPatchType.Resolution, profile.UoModResolution);
            EnablePatch(UOPatchType.OptionsNotification, profile.UoModOptionsNotification);
            EnablePatch(UOPatchType.MultiUO, profile.UoModMultiUo);
            EnablePatch(UOPatchType.NoCrypt, profile.UoModNoCrypt);
            EnablePatch(UOPatchType.GlobalSound, profile.UoModGlobalSound);
            
            if (profile.UoModViewRange)
            {
                SetViewRange(profile.UoModViewRangeValue);
                EnablePatch(UOPatchType.ViewRange, true);
            }
            else
            {
                EnablePatch(UOPatchType.ViewRange, false);
            }
        }

        #region P/Invoke
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out UIntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int WaitForSingleObject(IntPtr hHandle, int dwMilliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint dwFreeType);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr FindWindow(string? lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
        #endregion
    }
}
