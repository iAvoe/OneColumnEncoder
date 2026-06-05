using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using OneColumnEncoder.Models;

namespace OneColumnEncoder.Converters
{
    public class HeatCategoryLevelToBrushConverter : IValueConverter, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            MemoryCategory category = values.Length > 0 && values[0] is MemoryCategory c ? c : MemoryCategory.Empty;
            int level = values.Length > 1 ? ParseLevel(values[1]) : 0;
            return HeatmapBrush(category, level);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is HeatMapCellM cell)
            {
                if (cell.Level <= 0) return HeatmapBrush(MemoryCategory.Empty, 0);
                return HeatmapBrush(cell.Category, cell.Level);
            }

            int legacyLevel = ParseLevel(value);
            return HeatmapBrush(MemoryCategory.Upstream, legacyLevel);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();

        private static int ParseLevel(object value)
        {
            int level = 0;
            if (value is int i)
                level = i;
            else if (value != null)
                int.TryParse(value.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out level);

            return Math.Clamp(level, 0, 8);
        }

        private static Brush HeatmapBrush(MemoryCategory category, int level)
        {
            string key = level <= 0
                ? "HeatmapEmpty"
                : $"Heatmap{category}{level}";
            return Application.Current.TryFindResource(key) as Brush ?? Brushes.Transparent;
        }
    }
}
