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

            if (values[1] is not double minimum || values[2] is not double maximum || maximum <= minimum)
            {
                return 0d;
            }

            if (values[3] is not double value)
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
    }
}
