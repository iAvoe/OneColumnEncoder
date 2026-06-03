using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace OneColumnEncoder.Converters
{
    public class IndexToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
                return Visibility.Visible;

            if (!TryGetInt(values[0], out int index) || !TryGetInt(values[1], out int count))
                return Visibility.Visible;

            return index >= 0 && index < count - 1 ? Visibility.Visible : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();

        private static bool TryGetInt(object value, out int result)
        {
            if (value is int i)
            {
                result = i;
                return true;
            }

            if (value is string s && int.TryParse(s, out i))
            {
                result = i;
                return true;
            }

            result = 0;
            return false;
        }
    }
}
