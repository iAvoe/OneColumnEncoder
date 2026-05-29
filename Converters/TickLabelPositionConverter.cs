using System;
using System.Globalization;
using System.Windows.Data;

namespace OneColumnEncoder.Converters
{
    public class TickLabelPositionConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 4 ||
                values[0] is not double actualWidth ||
                values[1] is not double labelWidth ||
                values[2] is not int index ||
                values[3] is not int tickCount ||
                actualWidth <= 0 ||
                tickCount <= 1)
            {
                return 0d;
            }

            double normalized = Math.Max(0d, Math.Min(1d, index / (double)(tickCount - 1)));
            double position = normalized * actualWidth;

            if (index == 0)
            {
                return position;
            }

            if (index == tickCount - 1)
            {
                return position - labelWidth;
            }

            return position - labelWidth / 2d;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
