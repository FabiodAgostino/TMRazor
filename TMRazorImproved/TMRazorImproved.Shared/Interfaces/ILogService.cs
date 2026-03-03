using System;
using System.Collections.Generic;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.Shared.Interfaces
{
    public interface ILogService
    {
        event Action<LogEntry> OnNewLog;
        IEnumerable<LogEntry> GetLogs(string? source = null);
        void Log(string message, LogLevel level = LogLevel.Info, string source = "System");
        void Clear(string? source = null);
    }
}
