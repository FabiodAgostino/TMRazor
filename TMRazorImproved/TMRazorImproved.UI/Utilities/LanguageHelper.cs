using System;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Enums;

namespace TMRazorImproved.UI.Utilities
{
    /// <summary>
    /// Helper statico per l'accesso alle stringhe localizzate.
    /// Simula il pattern del vecchio LanguageHelper di Razor per facilità d'uso.
    /// </summary>
    public static class LanguageHelper
    {
        private static ILanguageService? _service => App.GetService<ILanguageService>();

        public static string GetString(string key) => _service?.GetString(key) ?? $"[{key}]";
        public static string GetString(LocString key) => _service?.GetString(key) ?? $"[{key}]";
        public static string GetString(int key) => _service?.GetString(key) ?? $"[{key}]";

        public static class Status
        {
            public static string Ready => GetString("Status.Ready");
            public static string ErrorEmptyPath => GetString("Status.ErrorEmptyPath");
            public static string Launching => GetString("Status.Launching");
            public static string Browsing => GetString("Status.Browsing");
            public static string ClientReady => GetString("Status.ClientReady");
        }
    }
}
