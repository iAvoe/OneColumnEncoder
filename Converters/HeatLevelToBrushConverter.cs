using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace OneColumnEncoder.Converters
{
    public class HeatLevelToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int level = 0;
            if (value is int i)
                level = i;
            else if (value != null)
                int.TryParse(value.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out level);

            level = Math.Clamp(level, 0, 8);
            return Application.Current.TryFindResource($"Heatmap{level}") as Brush ?? Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
