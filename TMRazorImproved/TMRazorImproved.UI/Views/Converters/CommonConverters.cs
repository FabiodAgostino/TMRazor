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
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool isNull = value == null;
            if (parameter is string p && p.Equals("Invert", StringComparison.OrdinalIgnoreCase))
                return isNull ? Visibility.Visible : Visibility.Collapsed;
            
            return isNull ? Visibility.Collapsed : Visibility.Visible;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NotNullToVisibilityConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value != null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class HotkeyDisplayConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
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

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CaptureAppearanceConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isCapturing && isCapturing)
                return ControlAppearance.Primary;
            return ControlAppearance.Secondary;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class InvertBooleanConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b) return !b;
            return value;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b) return !b;
            return value;
        }
    }

    public class BooleanInvertConverter : InvertBooleanConverter { }

    public class IntToVisibilityConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                if (parameter is string p && int.TryParse(p, out int targetValue))
                {
                    return intValue == targetValue ? Visibility.Visible : Visibility.Collapsed;
                }
                return intValue > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class SpellIconConverter : IValueConverter
    {
        private static TMRazorImproved.UI.Services.IUltimaImageCache? _cache;

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not int spellId) return null;

            int gumpId = 0;
            if (spellId >= 1 && spellId <= 64) // Magery
                gumpId = 0x8C0 + (spellId - 1);
            else if (spellId >= 101 && spellId <= 117) // Necro
                gumpId = 0x5000 + (spellId - 101);
            else if (spellId >= 201 && spellId <= 210) // Chivalry
                gumpId = 0x5100 + (spellId - 201);
            else if (spellId >= 401 && spellId <= 406) // Bushido
                gumpId = 0x5420 + (spellId - 401);
            else if (spellId >= 501 && spellId <= 508) // Ninjitsu
                gumpId = 0x5320 + (spellId - 501);
            else if (spellId >= 601 && spellId <= 616) // Spellweaving
                gumpId = 0x59D8 + (spellId - 601);
            else if (spellId >= 701 && spellId <= 716) // Mysticism
                gumpId = 0x5DC0 + (spellId - 701);

            if (gumpId == 0) return null;

            _cache ??= App.GetService<TMRazorImproved.UI.Services.IUltimaImageCache>();
            return _cache?.GetGump(gumpId);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CountToVisibilityConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int count) return count > 0 ? Visibility.Visible : Visibility.Collapsed;
            return Visibility.Collapsed;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class IntToBoolConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int intValue && parameter is string paramValue && int.TryParse(paramValue, out int targetValue))
            {
                return intValue == targetValue;
            }
            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue && boolValue && parameter is string paramValue && int.TryParse(paramValue, out int targetValue))
            {
                return targetValue;
            }
            return Binding.DoNothing;
        }
    }

    public class BooleanToTextConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b && parameter is string p)
            {
                var parts = p.Split('|');
                if (parts.Length == 2) return b ? parts[0] : parts[1];
            }
            return value?.ToString() ?? "";
        }
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class BooleanToIconConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b && parameter is string p)
            {
                var parts = p.Split('|');
                if (parts.Length == 2) return b ? parts[0] : parts[1];
            }
            return null;
        }
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class BooleanToAppearanceConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b && parameter is string p)
            {
                var parts = p.Split('|');
                if (parts.Length == 2)
                {
                    var val = b ? parts[0] : parts[1];
                    if (Enum.TryParse<ControlAppearance>(val, true, out var result))
                        return result;
                }
            }
            return ControlAppearance.Primary;
        }
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
