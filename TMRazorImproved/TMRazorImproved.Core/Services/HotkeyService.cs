using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TMRazorImproved.Core.Utilities;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;
using TMRazorImproved.Shared.Models.Config;
using TMRazorImproved.Shared.Enums;

namespace TMRazorImproved.Core.Services
{
    public class HotkeyService : IHotkeyService, IDisposable
    {
        private readonly IConfigService _configService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<HotkeyService> _logger;
        private readonly ConcurrentDictionary<string, Action> _registeredActions = new();
        
        private CancellationTokenSource? _cts;
        private Task? _hookTask;
        private IntPtr _hookId = IntPtr.Zero;
        private NativeMethods.LowLevelKeyboardProc? _proc;
        private volatile string? _lastActionName;

        public bool IsEnabled { get; set; } = true;
        public string? LastActionName => _lastActionName;

        public HotkeyService(IConfigService configService, IServiceProvider serviceProvider, ILogger<HotkeyService> logger)
        {
            _configService = configService;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _proc = HookCallback;
        }

        public void Start()
        {
            if (_hookTask != null && !_hookTask.IsCompleted) return;

            RegisterAllSystemActions();

            _cts = new CancellationTokenSource();
            _hookTask = Task.Run(() => HookLoop(_cts.Token), _cts.Token);
            _logger.LogInformation("Global Hotkey service starting");
        }

        // -----------------------------------------------------------------------
        // 040: Registrazione azioni di sistema
        // -----------------------------------------------------------------------

        private T Resolve<T>() => (T)_serviceProvider.GetService(typeof(T))!;

        private void RegisterAllSystemActions()
        {
            var ps  = Resolve<IPacketService>();
            var ws  = Resolve<IWorldService>();
            var ts  = Resolve<ITargetingService>();
            var ds  = Resolve<IDressService>();
            var autoLoot  = Resolve<IAutoLootService>();
            var scavenger = Resolve<IScavengerService>();
            var organizer = Resolve<IOrganizerService>();
            var bandage   = Resolve<IBandageHealService>();
            var restock   = Resolve<IRestockService>();

            // 040-A: Spell per nome (tutte le scuole via loop)
            foreach (var spell in SpellDefinitions.All)
            {
                int id   = spell.ID;
                string sn = spell.Name;
                RegisterAction($"Spell:{sn}", () => ps.SendToServer(PacketBuilder.CastSpell(id)));
            }

            // 040-B: Weapon Abilities (primary / secondary / clear)
            RegisterAction("Ability:Primary",   () => SendToggleAbility(ps, ws, 0x01));
            RegisterAction("Ability:Secondary", () => SendToggleAbility(ps, ws, 0x02));
            RegisterAction("Ability:Clear",     () => SendToggleAbility(ps, ws, 0x00));

            // 040-C: Attack
            RegisterAction("Attack:Nearest", () =>
            {
                ts.TargetClosest();
                if (ts.LastTarget != 0)
                    ps.SendToServer(PacketBuilder.Attack(ts.LastTarget));
            });
            RegisterAction("Attack:Last", () =>
            {
                if (ts.LastTarget != 0)
                    ps.SendToServer(PacketBuilder.Attack(ts.LastTarget));
            });

            // 040-D: Bandage (doppio click + target accodato)
            RegisterAction("Bandage:Self", () => UseBandage(ps, ws, ts, isSelf: true));
            RegisterAction("Bandage:Last", () => UseBandage(ps, ws, ts, isSelf: false));

            // 040-E: Pozioni — cerca nel backpack per graphic e fa double-click
            (string name, ushort graphic)[] potions =
            {
                ("Heal",      0x0F0C), ("Cure",      0x0F07), ("Refresh",    0x0F0B),
                ("Agility",   0x0F08), ("Strength",  0x0F09), ("Explosion",  0x0F0D),
                ("Poison",    0x0F0A), ("NightSight", 0x0F06),
            };
            foreach (var (name, graphic) in potions)
            {
                ushort g = graphic;
                RegisterAction($"Potion:{name}", () => UseItemFromBackpack(ps, ws, g));
            }

            // 040-F: Toggle Agenti
            RegisterAction("Agent:AutoLoot",  () => ToggleAgent(autoLoot));
            RegisterAction("Agent:Scavenger", () => ToggleAgent(scavenger));
            RegisterAction("Agent:Organizer", () => ToggleAgent(organizer));
            RegisterAction("Agent:Bandage",   () => ToggleAgent(bandage));
            RegisterAction("Agent:Restock",   () => ToggleAgent(restock));

            // 040-G: Dress / Undress / Mani
            RegisterAction("Dress:Active",   () => ds.DressUp());
            RegisterAction("Undress:Active", () => ds.Undress());
            RegisterAction("Hands:Clear",    () => ClearHands(ps, ws));

            // 040-I: Master Toggle (abilita/disabilita tutti gli hotkey)
            RegisterAction("Hotkey:Toggle", () => { IsEnabled = !IsEnabled; });

            _logger.LogDebug("System hotkey actions registered ({Count} actions)", _registeredActions.Count);
        }

        // -----------------------------------------------------------------------
        // Helper methods
        // -----------------------------------------------------------------------

        /// <summary>040-B: Toggle weapon ability (0x01=primary, 0x02=secondary, 0x00=clear).</summary>
        private static void SendToggleAbility(IPacketService ps, IWorldService ws, byte abilityIndex)
        {
            byte[] pkt = new byte[9];
            pkt[0] = 0xD7;
            pkt[1] = 0x00; pkt[2] = 0x09;
            BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(3), ws.Player?.Serial ?? 0);
            pkt[7] = 0x00; pkt[8] = abilityIndex;
            ps.SendToServer(pkt);
        }

        /// <summary>040-D: Usa una bandage dal backpack e accoda il target.</summary>
        private static void UseBandage(IPacketService ps, IWorldService ws, ITargetingService ts, bool isSelf)
        {
            const ushort BandageGraphic = 0x0E21;
            var player = ws.Player;
            if (player?.Backpack == null) return;
            var bandage = ws.Items.FirstOrDefault(i => i.Container == player.Backpack.Serial && i.Graphic == BandageGraphic);
            if (bandage == null) return;
            ps.SendToServer(PacketBuilder.DoubleClick(bandage.Serial));
            // Accoda il target: verrà eseguito quando arriva il cursor 0x6C dal server
            if (isSelf)
                ts.TargetSelf();
            else
                ts.DoLastTarget();
        }

        /// <summary>040-E: Cerca un item per graphic nel backpack e fa double-click.</summary>
        private static void UseItemFromBackpack(IPacketService ps, IWorldService ws, ushort graphic)
        {
            var player = ws.Player;
            if (player?.Backpack == null) return;
            var item = ws.Items.FirstOrDefault(i => i.Container == player.Backpack.Serial && i.Graphic == graphic);
            if (item == null) return;
            ps.SendToServer(PacketBuilder.DoubleClick(item.Serial));
        }

        /// <summary>040-F: Toggle start/stop di un agente.</summary>
        private static void ToggleAgent(IAgentService agent)
        {
            if (agent.IsRunning)
                _ = agent.StopAsync();
            else
                agent.Start();
        }

        /// <summary>040-G: Rimuove gli item equipaggiati nelle mani (layer 0x01/0x02) e li mette nel backpack.</summary>
        private static void ClearHands(IPacketService ps, IWorldService ws)
        {
            var player = ws.Player;
            if (player?.Backpack == null) return;
            var handItems = ws.Items
                .Where(i => i.Container == player.Serial && (i.Layer == 0x01 || i.Layer == 0x02))
                .ToList();
            if (handItems.Count == 0) return;

            _ = Task.Run(async () =>
            {
                foreach (var item in handItems)
                {
                    ps.SendToServer(PacketBuilder.LiftItem(item.Serial));
                    await Task.Delay(150);
                    ps.SendToServer(PacketBuilder.DropToContainer(item.Serial, player.Backpack.Serial));
                    await Task.Delay(150);
                }
            });
        }

        public async Task StopAsync()
        {
            if (_cts == null) return;
            _cts.Cancel();
            try
            {
                if (_hookTask != null) await _hookTask;
            }
            catch (OperationCanceledException) { }
            finally
            {
                _cts.Dispose();
                _cts = null;
                _hookTask = null;
                _logger.LogInformation("Global Hotkey service stopped");
            }
        }

        public void RegisterAction(string actionName, Action execute)
        {
            _registeredActions[actionName] = execute;
            _logger.LogTrace("Registered local action handler for: {ActionName}", actionName);
        }

        private void HookLoop(CancellationToken token)
        {
            using (var curProcess = Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                if (curModule != null)
                {
                    _hookId = NativeMethods.SetWindowsHookEx(NativeMethods.WH_KEYBOARD_LL, _proc!, NativeMethods.GetModuleHandle(curModule.ModuleName!), 0);
                }
                
                if (_hookId == IntPtr.Zero)
                {
                    int err = Marshal.GetLastWin32Error();
                    _logger.LogError("Failed to install keyboard hook. Error: {Error}", err);
                    return;
                }
            }

            _logger.LogDebug("Keyboard hook installed successfully (ID: {HookId})", _hookId);

            // Un loop dei messaggi è necessario per ricevere le notifiche degli hook
            while (!token.IsCancellationRequested)
            {
                NativeMethods.MSG msg;
                while (NativeMethods.PeekMessage(out msg, IntPtr.Zero, 0, 0, NativeMethods.PM_REMOVE))
                {
                    NativeMethods.TranslateMessage(ref msg);
                    NativeMethods.DispatchMessage(ref msg);
                }
                Thread.Sleep(10); // Piccolo sleep per non saturare la CPU
            }

            NativeMethods.UnhookWindowsHookEx(_hookId);
            _hookId = IntPtr.Zero;
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (IsEnabled && nCode >= 0 && (wParam == (IntPtr)NativeMethods.WM_KEYDOWN || wParam == (IntPtr)NativeMethods.WM_SYSKEYDOWN))
            {
                int vkCode = Marshal.ReadInt32(lParam);
                CheckHotkey(vkCode);
            }
            return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        private void CheckHotkey(int vkCode)
        {
            // Verifichiamo i modificatori (Ctrl, Alt, Shift)
            bool ctrl = (NativeMethods.GetKeyState(NativeMethods.VK_CONTROL) & 0x8000) != 0;
            bool alt = (NativeMethods.GetKeyState(NativeMethods.VK_MENU) & 0x8000) != 0;
            bool shift = (NativeMethods.GetKeyState(NativeMethods.VK_SHIFT) & 0x8000) != 0;

            var profile = _configService.CurrentProfile;
            if (profile == null) return;

            foreach (var hk in profile.Hotkeys)
            {
                if (hk.Enabled && hk.KeyCode == vkCode && hk.Ctrl == ctrl && hk.Alt == alt && hk.Shift == shift)
                {
                    ExecuteAction(hk.Action);
                }
            }
        }

        private void ExecuteAction(string actionName)
        {
            _lastActionName = actionName;
            if (_registeredActions.TryGetValue(actionName, out var action))
            {
                _logger.LogDebug("Executing hotkey action: {ActionName}", actionName);
                _ = Task.Run(() => {
                    try { action(); }
                    catch (Exception ex) { _logger.LogError(ex, "Error during hotkey action {ActionName}", actionName); }
                });
            }
            else if (actionName.StartsWith("Script:", StringComparison.OrdinalIgnoreCase))
            {
                string scriptPath = actionName.Substring(7).Trim();
                _logger.LogInformation("Executing hotkey script: {ScriptPath}", scriptPath);
                Task.Run(async () => {
                    try 
                    { 
                        if (System.IO.File.Exists(scriptPath))
                        {
                            var scriptingService = (IScriptingService)_serviceProvider.GetService(typeof(IScriptingService))!;
                            string code = await System.IO.File.ReadAllTextAsync(scriptPath);
                            
                            var ext = System.IO.Path.GetExtension(scriptPath).ToLower();
                            var lang = ext switch
                            {
                                ".uos" => ScriptLanguage.UOSteam,
                                ".cs" => ScriptLanguage.CSharp,
                                _ => ScriptLanguage.Python
                            };

                            await scriptingService.RunAsync(code, lang, System.IO.Path.GetFileName(scriptPath));
                        }
                        else
                        {
                            _logger.LogWarning("Hotkey script file not found: {ScriptPath}", scriptPath);
                        }
                    }
                    catch (Exception ex) { _logger.LogError(ex, "Error during hotkey script execution: {ScriptPath}", scriptPath); }
                });
            }
            else
            {
                _logger.LogWarning("No handler registered for hotkey action: {ActionName}", actionName);
            }
        }

        public void Dispose()
        {
            if (_hookId != IntPtr.Zero)
            {
                NativeMethods.UnhookWindowsHookEx(_hookId);
            }
            _cts?.Cancel();
        }

        private static class NativeMethods
        {
            public const int WH_KEYBOARD_LL = 13;
            public const int WM_KEYDOWN = 0x0100;
            public const int WM_SYSKEYDOWN = 0x0104;
            public const int VK_SHIFT = 0x10;
            public const int VK_CONTROL = 0x11;
            public const int VK_MENU = 0x12; // ALT key
            public const int PM_REMOVE = 0x0001;

            public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool UnhookWindowsHookEx(IntPtr hhk);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr GetModuleHandle(string lpModuleName);

            [DllImport("user32.dll")]
            public static extern short GetKeyState(int nVirtKey);

            [StructLayout(LayoutKind.Sequential)]
            public struct MSG
            {
                public IntPtr hwnd;
                public uint message;
                public IntPtr wParam;
                public IntPtr lParam;
                public uint time;
                public int pt_x;
                public int pt_y;
            }

            [DllImport("user32.dll")]
            public static extern bool PeekMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

            [DllImport("user32.dll")]
            public static extern bool TranslateMessage(ref MSG lpMsg);

            [DllImport("user32.dll")]
            public static extern IntPtr DispatchMessage(ref MSG lpMsg);
        }
    }
}
