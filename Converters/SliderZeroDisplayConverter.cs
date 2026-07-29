using System.Globalization;
using System.Windows.Data;

namespace OneColumnEncoder.Converters
{
    public class SliderZeroDisplayConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] is int intValue && values[1] is string zeroText)
                return intValue == 0 && !string.IsNullOrEmpty(zeroText) ? zeroText : intValue.ToString();
            return "0";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
