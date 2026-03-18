using System;
using System.Globalization;
using System.Windows.Data;

namespace TMRazorImproved.UI.Converters
{
    public class GainToHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double gain)
            {
                // Un gain di 0.1 diventa 10 pixel, 1.0 diventa 100 pixel.
                return Math.Max(2, gain * 100);
            }
            return 2.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
