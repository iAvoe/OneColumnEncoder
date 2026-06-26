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
                double itemWidth = 300;
                if (parameter is not string text || !TryParseParameter(text, culture, out int maxColumns, out itemWidth)) return width > 500 ? 2 : 1;

                int columns = Math.Max(1, (int)(width / itemWidth));
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

        private static bool TryParseParameter(string text, CultureInfo culture, out int maxColumns, out double itemWidth)
        {
            itemWidth = 300;

            string[] parts = text.Split(':', 2);
            if (!int.TryParse(parts[0], out maxColumns))
                return false;

            if (parts.Length == 2 && (!TryGetDouble(parts[1], culture, out itemWidth) || itemWidth <= 0))
                return false;

            return true;
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
