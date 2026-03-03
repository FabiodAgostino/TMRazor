using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using TMRazorImproved.Shared.Models.Config;
using Wpf.Ui.Controls;

namespace TMRazorImproved.UI.Views.Converters
{
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NotNullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class HotkeyDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not HotkeyDefinition hk || hk.KeyCode == 0) return "None";

            var parts = new System.Collections.Generic.List<string>();
            if (hk.Ctrl) parts.Add("Ctrl");
            if (hk.Alt) parts.Add("Alt");
            if (hk.Shift) parts.Add("Shift");

            Key wpfKey = KeyInterop.KeyFromVirtualKey(hk.KeyCode);
            parts.Add(wpfKey.ToString());

            return string.Join(" + ", parts);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CaptureAppearanceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isCapturing && isCapturing)
                return ControlAppearance.Primary;
            return ControlAppearance.Secondary;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class InvertBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b) return !b;
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b) return !b;
            return value;
        }
    }

    public class SpellIconConverter : IValueConverter
    {
        private static TMRazorImproved.UI.Services.IUltimaImageCache? _cache;

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not int spellId) return null;

            int gumpId = 0;
            if (spellId >= 1 && spellId <= 64) // Magery
                gumpId = 0x8CA + (spellId - 1);
            else if (spellId >= 101 && spellId <= 117) // Necro
                gumpId = 0x5000 + (spellId - 101);
            else if (spellId >= 201 && spellId <= 210) // Chivalry
                gumpId = 0x5100 + (spellId - 201);
            else if (spellId >= 401 && spellId <= 406) // Bushido
                gumpId = 0x5400 + (spellId - 401);
            else if (spellId >= 501 && spellId <= 508) // Ninjitsu
                gumpId = 0x5300 + (spellId - 501);
            else if (spellId >= 601 && spellId <= 616) // Spellweaving
                gumpId = 0x59D0 + (spellId - 601);

            if (gumpId == 0) return null;

            _cache ??= App.GetService<TMRazorImproved.UI.Services.IUltimaImageCache>();
            return _cache?.GetGump(gumpId);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IntToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue && parameter is string paramValue && int.TryParse(paramValue, out int targetValue))
            {
                return intValue == targetValue;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && boolValue && parameter is string paramValue && int.TryParse(paramValue, out int targetValue))
            {
                return targetValue;
            }
            return Binding.DoNothing;
        }
    }
}
