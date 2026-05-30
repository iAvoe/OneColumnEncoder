using System;
using System.Globalization;
using System.Windows.Data;

namespace OneColumnEncoder.Converters
{
    public class ButtonWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double parentWidth && parameter is string param)
            {
                string[] parts = param.Split(',');
                if (int.TryParse(parts[0], out int count) && count > 0)
                {
                    double perGapSpacing = parts.Length > 1 && double.TryParse(parts[1], out double s) ? s : 0;
                    double totalSpacing = perGapSpacing * (count - 1);
                    return Math.Max(0, (parentWidth - totalSpacing) / count);
                }
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
