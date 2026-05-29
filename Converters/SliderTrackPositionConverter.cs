using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace OneColumnEncoder.Converters
{
    public class SliderTrackPositionConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 4)
            {
                return 0d;
            }

            if (values[0] is not double actualWidth || actualWidth <= 0)
            {
                return 0d;
            }

            if (!TryToDouble(values[1], out double minimum) ||
                !TryToDouble(values[2], out double maximum) ||
                !TryToDouble(values[3], out double value) ||
                maximum <= minimum)
            {
                return 0d;
            }

            double normalized = Math.Max(0d, Math.Min(1d, (value - minimum) / (maximum - minimum)));
            return normalized * actualWidth;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
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
