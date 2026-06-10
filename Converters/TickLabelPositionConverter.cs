using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace OneColumnEncoder.Converters
{
    public class TickLabelPositionConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 5 ||
                values[0] is not double actualWidth ||
                values[1] is not double labelWidth ||
                actualWidth <= 0)
            {
                return 0d;
            }

            if (!TryToDouble(values[2], out double value) ||
                !TryToDouble(values[3], out double minimum) ||
                !TryToDouble(values[4], out double maximum) ||
                maximum <= minimum)
            {
                return 0d;
            }

            double travelWidth = Math.Max(0d, actualWidth - OneColumnEncoder.Components.IntegerSlider.ThumbWidth);
            double normalized = Math.Max(0d, Math.Min(1d, (value - minimum) / (maximum - minimum)));
            double center = normalized * travelWidth + OneColumnEncoder.Components.IntegerSlider.ThumbWidth / 2d;
            return center - labelWidth / 2d;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            object[] results = new object[targetTypes.Length];
            for (int index = 0; index < results.Length; index++)
                results[index] = Binding.DoNothing;

            return results;
        }

        private static bool TryToDouble(object value, out double result)
        {
            if (value == null || value == DependencyProperty.UnsetValue)
            {
                result = 0d;
                return false;
            }

            string text = value.ToString() ?? string.Empty;
            int length = 0;
            while (length < text.Length && (char.IsDigit(text[length]) || text[length] == '.' || text[length] == '-' || text[length] == '+'))
            {
                length++;
            }

            if (length > 0)
            {
                text = text[..length];
            }

            return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out result) ||
                   double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out result);
        }
    }
}
