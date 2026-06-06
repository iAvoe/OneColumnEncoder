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
            throw new NotImplementedException();
        }
    }
}
