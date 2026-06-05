using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using OneColumnEncoder.Models;

namespace OneColumnEncoder.Converters
{
    public class HeatCategoryLevelToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is HeatMapCellM cell)
            {
                if (cell.Level <= 0) return HeatmapBrush(MemoryCategory.Empty, 0);
                return HeatmapBrush(cell.Category, cell.Level);
            }

            int legacyLevel = 0;
            if (value is int i)
            {
                legacyLevel = i;
            }
            else if (value != null)
            {
                int.TryParse(value.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out legacyLevel);
            }
            legacyLevel = Math.Clamp(legacyLevel, 0, 8);
            return HeatmapBrush(MemoryCategory.Upstream, legacyLevel);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();

        private static Brush HeatmapBrush(MemoryCategory category, int level)
        {
            string key = level <= 0
                ? "HeatmapEmpty"
                : $"Heatmap{category}{level}";
            return Application.Current.TryFindResource(key) as Brush ?? Brushes.Transparent;
        }
    }
}
