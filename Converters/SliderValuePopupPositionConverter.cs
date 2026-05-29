using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace OneColumnEncoder.Converters
{
    public class SliderValuePopupPositionConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 5 || values[0] is not double actualWidth || actualWidth <= 0)
            {
                return 0d;
            }

            double popupWidth = values[1] is double width ? width : 0d;
            double travelWidth = Math.Max(0d, actualWidth - OneColumnEncoder.Components.IntegerSlider.ThumbWidth);

            if (!TryToDouble(values[2], out double minimum) ||
                !TryToDouble(values[3], out double maximum) ||
                !TryToDouble(values[4], out double value) ||
                maximum <= minimum)
            {
                return 0d;
            }

            double normalized = Math.Max(0d, Math.Min(1d, (value - minimum) / (maximum - minimum)));
            double center = normalized * travelWidth + OneColumnEncoder.Components.IntegerSlider.ThumbWidth / 2d;
            return center - popupWidth / 2d;
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
