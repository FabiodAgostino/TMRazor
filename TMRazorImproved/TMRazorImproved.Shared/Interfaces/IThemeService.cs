using System;

namespace TMRazorImproved.Shared.Interfaces
{
    public interface IThemeService
    {
        void ApplyTheme(string themeName);
        void ApplyAccentColor(string color);
        void SetSystemThemeWatcher(bool enabled);
        string GetCurrentTheme();
    }
}
