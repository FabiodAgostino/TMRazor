using System;
using System.Windows;
using System.Windows.Media;
using TMRazorImproved.Shared.Interfaces;
using Wpf.Ui.Appearance;

namespace TMRazorImproved.UI.Services
{
    public class ThemeService : IThemeService
    {
        private readonly IConfigService _config;

        public ThemeService(IConfigService config)
        {
            _config = config;
        }

        public void ApplyTheme(string themeName)
        {
            var theme = ApplicationTheme.Dark;
            
            switch (themeName?.ToLowerInvariant())
            {
                case "light":
                    theme = ApplicationTheme.Light;
                    break;
                case "highcontrast":
                    theme = ApplicationTheme.HighContrast;
                    break;
                case "auto":
                    SetSystemThemeWatcher(true);
                    return;
                default:
                    theme = ApplicationTheme.Dark;
                    break;
            }

            SetSystemThemeWatcher(false);
            ApplicationThemeManager.Apply(theme);
        }

        public void ApplyAccentColor(string color)
        {
            if (string.IsNullOrEmpty(color) || color.Equals("default", StringComparison.OrdinalIgnoreCase))
            {
                ApplicationAccentColorManager.Apply(SystemParameters.WindowGlassColor);
            }
            else
            {
                try
                {
                    var mediaColor = (Color)ColorConverter.ConvertFromString(color);
                    ApplicationAccentColorManager.Apply(mediaColor);
                }
                catch
                {
                    ApplicationAccentColorManager.Apply(SystemParameters.WindowGlassColor);
                }
            }
        }

        public void SetSystemThemeWatcher(bool enabled)
        {
            if (enabled)
                ApplicationThemeManager.ApplySystemTheme();
        }

        public string GetCurrentTheme()
        {
            return ApplicationThemeManager.GetAppTheme().ToString();
        }
    }
}
