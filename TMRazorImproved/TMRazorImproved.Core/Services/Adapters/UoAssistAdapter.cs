using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Interop;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Messages;

namespace TMRazorImproved.Core.Services.Adapters
{
    /// <summary>
    /// FR-065: Implementazione del layer di compatibilità UOAssist.
    /// Gestisce il protocollo basato su WM_USER+200 che tool esterni (EasyUO, UOAssist-compatibili)
    /// usano per interagire con Razor/TMRazor.
    ///
    /// Protocollo:
    ///   – Inbound (da tool esterno): WM_USER+200..216 inviati come PostMessage alla nostra finestra.
    ///   – Outbound (da noi): PostMessage alla finestra del tool registrato, WM_USER+301..314.
    ///
    /// Attivazione:
    ///   Chiamare Initialize(hwnd) dopo che la MainWindow è caricata:
    ///   <code>
    ///     Loaded += (_, _) => _uoAssist.Initialize(new WindowInteropHelper(this).Handle);
    ///   </code>
    /// </summary>
    public class UoAssistAdapter : IUoAssistService,
        IRecipient<LoginCompleteMessage>,
        IRecipient<PlayerStatusMessage>,
        IRecipient<SkillsUpdatedMessage>
    {
        // ── WM constants ─────────────────────────────────────────────────────────
        private const int WM_USER = 0x0400;

        // Inbound (ricevuti dalla nostra finestra)
        private const int UOA_REGISTER         = WM_USER + 200;
        private const int UOA_COUNT_RESOURCES  = WM_USER + 201;
        private const int UOA_GET_COORDS       = WM_USER + 202;
        private const int UOA_GET_SKILL        = WM_USER + 203;
        private const int UOA_GET_STAT         = WM_USER + 204;
        private const int UOA_SET_MACRO        = WM_USER + 205;
        private const int UOA_PLAY_MACRO       = WM_USER + 206;
        private const int UOA_DISPLAY_TEXT     = WM_USER + 207;
        private const int UOA_REQUEST_MULTIS   = WM_USER + 208;
        private const int UOA_ADD_CMD          = WM_USER + 209;
        private const int UOA_GET_UID          = WM_USER + 210;
        private const int UOA_GET_SHARDNAME    = WM_USER + 211;
        private const int UOA_ADD_USER_2_PARTY = WM_USER + 212;
        private const int UOA_GET_UO_HWND      = WM_USER + 213;
        private const int UOA_GET_POISON       = WM_USER + 214;
        private const int UOA_SET_SKILL_LOCK   = WM_USER + 215;
        private const int UOA_GET_ACCT_ID      = WM_USER + 216;

        // Outbound (inviati alle finestre registrate)
        private const uint UOA_RES_COUNT_DONE  = WM_USER + 301;
        private const uint UOA_CAST_SPELL      = WM_USER + 302;
        private const uint UOA_LOGIN           = WM_USER + 303;
        private const uint UOA_MAGERY_LEVEL    = WM_USER + 304;
        private const uint UOA_INT_STATUS      = WM_USER + 305;
        private const uint UOA_SKILL_LEVEL     = WM_USER + 306;
        private const uint UOA_MACRO_DONE      = WM_USER + 307;
        private const uint UOA_LOGOUT          = WM_USER + 308;
        private const uint UOA_STR_STATUS      = WM_USER + 309;
        private const uint UOA_DEX_STATUS      = WM_USER + 310;
        private const uint UOA_ADD_MULTI       = WM_USER + 311;
        private const uint UOA_REM_MULTI       = WM_USER + 312;
        private const uint UOA_MAP_INFO        = WM_USER + 313;
        private const uint UOA_POWERHOUR       = WM_USER + 314;

        // ── State ─────────────────────────────────────────────────────────────────
        private readonly IWorldService _world;
        private readonly ISkillsService _skills;
        private readonly IConfigService _config;
        private readonly IMessenger _messenger;
        private readonly ILogger<UoAssistAdapter> _logger;

        private HwndSource? _hwndSource;
        private readonly List<RegisteredWindow> _registeredWindows = new();
        private bool _disposed;

        public bool IsActive => _hwndSource != null;
        public int NotificationCount => _registeredWindows.Count;

        // ── Constructor ───────────────────────────────────────────────────────────
        public UoAssistAdapter(
            IWorldService world,
            ISkillsService skills,
            IConfigService config,
            IMessenger messenger,
            ILogger<UoAssistAdapter> logger)
        {
            _world = world;
            _skills = skills;
            _config = config;
            _messenger = messenger;
            _logger = logger;

            _messenger.RegisterAll(this);
        }

        // ── IUoAssistService.Initialize ───────────────────────────────────────────
        public void Initialize(IntPtr windowHandle)
        {
            if (windowHandle == IntPtr.Zero) return;
            _hwndSource = HwndSource.FromHwnd(windowHandle);
            _hwndSource?.AddHook(WndProc);
            _logger.LogInformation("[UoAssist] Layer inizializzato su HWND {hwnd:X}", windowHandle);
        }

        // ── WndProc hook ──────────────────────────────────────────────────────────
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg < UOA_REGISTER || msg > UOA_GET_ACCT_ID)
                return IntPtr.Zero;

            var player = _world.Player;
            int result = HandleUoaMessage(msg, wParam.ToInt32(), lParam.ToInt32(), player);
            handled = true;
            return new IntPtr(result);
        }

        private int HandleUoaMessage(int msg, int wParam, int lParam, Shared.Models.Mobile? player)
        {
            switch (msg)
            {
                case UOA_REGISTER:
                {
                    for (int i = 0; i < _registeredWindows.Count; i++)
                    {
                        if (_registeredWindows[i].Handle == wParam)
                        {
                            _registeredWindows.RemoveAt(i);
                            return 2; // already registered → toggle off
                        }
                    }
                    _registeredWindows.Add(new RegisteredWindow(wParam, lParam == 1));
                    return 1; // registered ok
                }

                case UOA_GET_COORDS:
                    if (player == null) return 0;
                    return (player.X & 0xFFFF) | ((player.Y & 0xFFFF) << 16);

                case UOA_GET_UID:
                case UOA_GET_ACCT_ID:
                    return player == null ? 0 : (int)player.Serial;

                case UOA_GET_POISON:
                    return player?.IsPoisoned == true ? 1 : 0;

                case UOA_GET_STAT:
                {
                    if (player == null || wParam < 0 || wParam > 5) return 0;
                    return wParam switch
                    {
                        0 => player.Str,
                        1 => player.Int,
                        2 => player.Dex,
                        3 => player.Weight,
                        4 => player.HitsMax,
                        5 => player.Tithe,
                        _ => 0
                    };
                }

                case UOA_GET_SKILL:
                {
                    var skillList = _skills.Skills;
                    if (wParam < 0 || wParam >= skillList.Count || lParam < 0 || lParam > 2)
                        return 0;
                    var sk = skillList[wParam];
                    return lParam switch
                    {
                        0 => (int)(sk.Value * 10),      // FixedValue
                        1 => (int)(sk.BaseValue * 10),  // FixedBase
                        2 => (int)sk.Lock,              // LockType
                        _ => 0
                    };
                }

                case UOA_GET_SHARDNAME:
                {
                    var name = _config.CurrentProfile.ShardName;
                    if (string.IsNullOrEmpty(name)) return 0;
                    // GlobalAddAtom non è disponibile facilmente; restituiamo hash come fallback
                    return GlobalAddAtom(name);
                }

                case UOA_REQUEST_MULTIS:
                    return player != null ? 1 : 0;

                case UOA_GET_UO_HWND:
                    // Restituiamo 0 (non abbiamo un HWND client UO separato in modalità plugin)
                    return 0;

                case UOA_SET_SKILL_LOCK:
                {
                    if (player == null || wParam < 0 || lParam < 0 || lParam >= 3) return 0;
                    _skills.SetLock(wParam, (Shared.Models.SkillLock)lParam);
                    return 1;
                }

                case UOA_ADD_USER_2_PARTY:
                    return 1; // not supported

                case UOA_COUNT_RESOURCES:
                case UOA_SET_MACRO:
                case UOA_PLAY_MACRO:
                case UOA_DISPLAY_TEXT:
                case UOA_ADD_CMD:
                    return 0; // not implemented

                default:
                    return 0;
            }
        }

        // ── IRecipient implementations ────────────────────────────────────────────
        public void Receive(LoginCompleteMessage message)
        {
            var player = _world.Player;
            if (player != null)
                PostLogin(player.Serial);
        }

        public void Receive(PlayerStatusMessage message)
        {
            var player = _world.Player;
            if (player == null) return;

            switch (message.Value.Stat)
            {
                case StatType.Hits:
                    PostHitsUpdate(message.Value.Max, message.Value.Current);
                    break;
                case StatType.Mana:
                    PostManaUpdate(message.Value.Max, message.Value.Current);
                    break;
                case StatType.Stamina:
                    PostStamUpdate(message.Value.Max, message.Value.Current);
                    break;
            }
        }

        public void Receive(SkillsUpdatedMessage message)
        {
            // L'aggiornamento completo skills viene gestito da SkillsService.
            // Qui inviamo skill level per ogni skill aggiornata.
            var skillList = _skills.Skills;
            for (int i = 0; i < skillList.Count; i++)
            {
                var sk = skillList[i];
                PostSkillUpdate(i, (int)(sk.Value * 10));
            }
        }

        // ── IUoAssistService outbound ─────────────────────────────────────────────
        public void PostLogin(uint serial)
        {
            BroadcastToRegistered(UOA_LOGIN, new IntPtr((int)serial), IntPtr.Zero);
        }

        public void PostLogout()
        {
            foreach (var wnd in _registeredWindows)
                PostMessage(new IntPtr(wnd.Handle), UOA_LOGOUT, IntPtr.Zero, IntPtr.Zero);
        }

        public void PostSkillUpdate(int skill, int val)
        {
            BroadcastToRegistered(UOA_SKILL_LEVEL, new IntPtr(skill), new IntPtr(val));
            // Se è Magery (ID 25 nel legacy), invia anche MAGERY_LEVEL
            if (skill == 25)
                BroadcastToRegistered(UOA_MAGERY_LEVEL, new IntPtr(val / 10), new IntPtr(val % 10));
        }

        public void PostHitsUpdate(ushort max, ushort current)
            => BroadcastToRegistered(UOA_STR_STATUS, new IntPtr(max), new IntPtr(current));

        public void PostManaUpdate(ushort max, ushort current)
            => BroadcastToRegistered(UOA_INT_STATUS, new IntPtr(max), new IntPtr(current));

        public void PostStamUpdate(ushort max, ushort current)
            => BroadcastToRegistered(UOA_DEX_STATUS, new IntPtr(max), new IntPtr(current));

        public void PostMapChange(int map)
            => BroadcastToRegistered(UOA_MAP_INFO, new IntPtr(map), IntPtr.Zero);

        public void PostSpellCast(int spell)
            => BroadcastToRegistered(UOA_CAST_SPELL, new IntPtr(spell), IntPtr.Zero);

        public void PostMacroDone()
            => BroadcastToRegistered(UOA_MACRO_DONE, IntPtr.Zero, IntPtr.Zero);

        // ── Helpers ───────────────────────────────────────────────────────────────
        private void BroadcastToRegistered(uint msg, IntPtr wParam, IntPtr lParam)
        {
            var dead = new List<RegisteredWindow>();
            foreach (var wnd in _registeredWindows)
            {
                if (PostMessage(new IntPtr(wnd.Handle), msg, wParam, lParam) == 0)
                    dead.Add(wnd);
            }
            foreach (var d in dead)
                _registeredWindows.Remove(d);
        }

        // ── P/Invoke ──────────────────────────────────────────────────────────────
        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern ushort GlobalAddAtom(string str);

        // ── IDisposable ───────────────────────────────────────────────────────────
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            PostLogout();
            _hwndSource?.RemoveHook(WndProc);
            _hwndSource = null;
            _messenger.UnregisterAll(this);
        }

        // ── Inner types ───────────────────────────────────────────────────────────
        private sealed record RegisteredWindow(int Handle, bool WantsMultiNotifications);
    }
}
