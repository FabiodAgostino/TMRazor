using System;
using System.Collections.Generic;
using System.Linq;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.Core.Services
{
    public class LogService : ILogService
    {
        private readonly List<LogEntry> _logs = new();
        private readonly object _lock = new();
        private const int MaxLogs = 1000;

        public event Action<LogEntry>? OnNewLog;

        public IEnumerable<LogEntry> GetLogs(string? source = null)
        {
            lock (_lock)
            {
                if (string.IsNullOrEmpty(source))
                    return _logs.ToList();
                
                return _logs.Where(l => l.Source == source).ToList();
            }
        }

        public void Log(string message, LogLevel level = LogLevel.Info, string source = "System")
        {
            var entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Message = message,
                Level = level,
                Source = source
            };

            lock (_lock)
            {
                _logs.Add(entry);
                if (_logs.Count > MaxLogs)
                {
                    _logs.RemoveAt(0);
                }
            }

            OnNewLog?.Invoke(entry);
        }

        public void Clear(string? source = null)
        {
            lock (_lock)
            {
                if (string.IsNullOrEmpty(source))
                    _logs.Clear();
                else
                    _logs.RemoveAll(l => l.Source == source);
            }
        }
    }
}
