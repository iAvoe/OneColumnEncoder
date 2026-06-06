using System;
using System.Globalization;
using System.Windows.Data;

namespace OneColumnEncoder.Converters
{
    public class WidthToColumnCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double width)
            {
                if (parameter is not string text || !int.TryParse(text, out int maxColumns)) return width > 500 ? 2 : 1;

                int columns = Math.Max(1, (int)(width / 300));
                return Math.Min(columns, maxColumns);
            }
            return 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!TryGetDouble(value, culture, out double columns))
                return Binding.DoNothing;

            return Math.Max(0d, columns) * 300d;
        }

        private static bool TryGetDouble(object value, CultureInfo culture, out double result)
        {
            if (value is double doubleValue)
            {
                result = doubleValue;
                return true;
            }

            if (value is IConvertible)
            {
                try
                {
                    result = System.Convert.ToDouble(value, culture);
                    return true;
                }
                catch (Exception)
                {
                }
            }

            result = 0d;
            return false;
        }
    }
}
