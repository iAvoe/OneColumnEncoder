using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace OneColumnEncoder.Converters
{
    public class SliderTrackPositionConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 5)
            {
                return 0d;
            }

            if (values[0] is not double actualWidth || actualWidth <= 0)
            {
                return 0d;
            }

            double travelWidth = Math.Max(0d, actualWidth - OneColumnEncoder.Components.IntegerSlider.ThumbWidth);

            if (!TryToDouble(values[1], out double minimum) ||
                !TryToDouble(values[2], out double maximum) ||
                !TryToDouble(values[3], out double value) ||
                maximum <= minimum)
            {
                return 0d;
            }

            bool isLog = values[4] is true;

            double normalized = isLog
                ? NormalizeLog(value, minimum, maximum)
                : Math.Max(0d, Math.Min(1d, (value - minimum) / (maximum - minimum)));
            return normalized * travelWidth;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            object[] results = new object[targetTypes.Length];
            for (int index = 0; index < results.Length; index++)
                results[index] = Binding.DoNothing;

            return results;
        }

        private static double NormalizeLog(double value, double minimum, double maximum)
        {
            double logMin = Math.Max(1, minimum);
            double logMax = Math.Max(logMin + 1, maximum);
            double clamped = Math.Max(logMin, Math.Min(logMax, value));
            return Math.Max(0d, Math.Min(1d, Math.Log(clamped / logMin) / Math.Log(logMax / logMin)));
        }

        private static bool TryToDouble(object value, out double result)
        {
            if (value == null || value == DependencyProperty.UnsetValue)
            {
                result = 0d;
                return false;
            }

            try
            {
                result = System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
                return true;
            }
            catch (Exception)
            {
                result = 0d;
                return false;
            }
        }
    }
}
