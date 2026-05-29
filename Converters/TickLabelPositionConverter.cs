using System;
using System.Globalization;
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
            throw new NotImplementedException();
        }

        private static bool TryToDouble(object value, out double result)
        {
            if (value == null)
            {
                result = 0d;
                return false;
            }

            return double.TryParse(value.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out result) ||
                   double.TryParse(value.ToString(), NumberStyles.Float, CultureInfo.CurrentCulture, out result);
        }
    }
}
