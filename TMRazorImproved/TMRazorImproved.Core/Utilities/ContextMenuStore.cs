using System;
using System.Collections.Generic;
using System.Threading;

namespace TMRazorImproved.Core.Utilities
{
    /// <summary>
    /// Context menu entry returned to Python scripts via Misc.WaitForContext.
    /// Mirrors RazorEnhanced Misc.Context: Response = menu index, Entry = display text.
    /// </summary>
    public class ContextMenuEntry
    {
        public int Response { get; }
        public string Entry { get; }

        public ContextMenuEntry(int response, string entry)
        {
            Response = response;
            Entry    = entry;
        }
    }

    /// <summary>
    /// Thread-safe store for the last context menu received from the server (0xBF sub 0x14).
    /// <see cref="WorldPacketHandler"/> writes here; <see cref="TMRazorImproved.Core.Services.Scripting.Api.MiscApi"/> reads here.
    /// </summary>
    internal static class ContextMenuStore
    {
        private static readonly object _lock = new();
        private static uint _pendingSerial;
        private static List<ContextMenuEntry> _entries = new();

        internal static void Set(uint serial, List<ContextMenuEntry> entries)
        {
            lock (_lock)
            {
                _pendingSerial = serial;
                _entries       = entries;
            }
        }

        /// <summary>
        /// Polls until <paramref name="serial"/> matches the pending serial or timeout expires.
        /// Returns a snapshot of the entries (empty list on timeout).
        /// </summary>
        internal static List<ContextMenuEntry> WaitForSerial(uint serial, int timeoutMs)
        {
            var deadline = Environment.TickCount64 + timeoutMs;
            while (Environment.TickCount64 < deadline)
            {
                lock (_lock)
                {
                    if (_pendingSerial == serial)
                        return new List<ContextMenuEntry>(_entries);
                }
                Thread.Sleep(10);
            }
            lock (_lock)
                return _pendingSerial == serial
                    ? new List<ContextMenuEntry>(_entries)
                    : new List<ContextMenuEntry>();
        }

        internal static void Clear()
        {
            lock (_lock)
            {
                _pendingSerial = 0;
                _entries       = new();
            }
        }
    }
}
