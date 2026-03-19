using System;
using System.Threading;

namespace TMRazorImproved.Core.Utilities
{
    /// <summary>
    /// Rappresenta una richiesta di Query String (0xAB) inviata dal server.
    /// </summary>
    public class UOStringQuery
    {
        public uint Serial    { get; }
        public int  QueryId   { get; }
        public byte QueryType { get; }

        public UOStringQuery(uint serial, int queryId, byte queryType)
        {
            Serial    = serial;
            QueryId   = queryId;
            QueryType = queryType;
        }
    }

    /// <summary>
    /// Thread-safe store per l'ultimo pacchetto String Query (0xAB) ricevuto.
    /// <see cref="TMRazorImproved.Core.Handlers.WorldPacketHandler"/> scrive qui;
    /// <see cref="TMRazorImproved.Core.Services.Scripting.Api.MiscApi"/> legge qui.
    /// </summary>
    internal static class StringQueryStore
    {
        private static readonly object _lock = new();
        private static UOStringQuery? _currentQuery;
        private static long    _version;

        internal static void Set(UOStringQuery query)
        {
            lock (_lock)
            {
                _currentQuery = query;
                Interlocked.Increment(ref _version);
            }
        }

        internal static void Clear()
        {
            lock (_lock)
                _currentQuery = null;
        }

        internal static bool HasQuery()
        {
            lock (_lock)
                return _currentQuery != null;
        }

        internal static UOStringQuery? Get()
        {
            lock (_lock)
                return _currentQuery;
        }

        /// <summary>
        /// Attende fino a <paramref name="timeoutMs"/> ms che arrivi una Query String dal server.
        /// </summary>
        internal static bool WaitForQuery(int timeoutMs)
        {
            long versionBefore = Interlocked.Read(ref _version);
            var deadline = Environment.TickCount64 + timeoutMs;

            while (Environment.TickCount64 < deadline)
            {
                if (Interlocked.Read(ref _version) != versionBefore)
                {
                    lock (_lock)
                        return _currentQuery != null;
                }
                Thread.Sleep(10);
            }
            return false;
        }
    }
}
