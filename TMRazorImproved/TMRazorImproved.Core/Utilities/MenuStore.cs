using System;
using System.Collections.Generic;
using System.Threading;

namespace TMRazorImproved.Core.Utilities
{
    /// <summary>
    /// Singolo item di un menu classico UO (0x7C).
    /// </summary>
    public class UOMenuItem
    {
        public int    Index   { get; }  // 1-based, usato nella risposta 0x7D
        public ushort Graphic { get; }
        public ushort Hue     { get; }
        public string Name    { get; }

        public UOMenuItem(int index, ushort graphic, ushort hue, string name)
        {
            Index   = index;
            Graphic = graphic;
            Hue     = hue;
            Name    = name;
        }
    }

    /// <summary>
    /// Menu classico UO (pre-context-menu, pacchetto 0x7C).
    /// </summary>
    public class UOMenu
    {
        public uint              Serial  { get; }
        public ushort            MenuId  { get; }
        public string            Title   { get; }
        public List<UOMenuItem>  Items   { get; }

        public UOMenu(uint serial, ushort menuId, string title, List<UOMenuItem> items)
        {
            Serial = serial;
            MenuId = menuId;
            Title  = title;
            Items  = items;
        }
    }

    /// <summary>
    /// Thread-safe store per l'ultimo menu classico ricevuto dal server (0x7C).
    /// <see cref="TMRazorImproved.Core.Handlers.WorldPacketHandler"/> scrive qui;
    /// <see cref="TMRazorImproved.Core.Services.Scripting.Api.MiscApi"/> legge qui.
    /// </summary>
    internal static class MenuStore
    {
        private static readonly object _lock = new();
        private static UOMenu? _currentMenu;
        private static long    _version;     // incrementato ad ogni Set(), per distinguere menu nuovi

        internal static void Set(UOMenu menu)
        {
            lock (_lock)
            {
                _currentMenu = menu;
                Interlocked.Increment(ref _version);
            }
        }

        internal static void Clear()
        {
            lock (_lock)
                _currentMenu = null;
        }

        internal static bool HasMenu()
        {
            lock (_lock)
                return _currentMenu != null;
        }

        internal static UOMenu? Get()
        {
            lock (_lock)
                return _currentMenu;
        }

        /// <summary>
        /// Attende fino a <paramref name="timeoutMs"/> ms che arrivi un menu dal server.
        /// Ritorna <c>true</c> se il menu è arrivato, <c>false</c> in caso di timeout.
        /// </summary>
        internal static bool WaitForMenu(int timeoutMs)
        {
            // Legge la versione prima di iniziare ad aspettare: vogliamo un menu *nuovo*
            long versionBefore = Interlocked.Read(ref _version);
            var deadline = Environment.TickCount64 + timeoutMs;

            while (Environment.TickCount64 < deadline)
            {
                if (Interlocked.Read(ref _version) != versionBefore)
                {
                    lock (_lock)
                        return _currentMenu != null;
                }
                Thread.Sleep(10);
            }
            return false;
        }
    }
}
