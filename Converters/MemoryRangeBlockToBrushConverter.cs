using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using OneColumnEncoder.Models;

namespace OneColumnEncoder.Converters
{
    public class MemoryRangeBlockToBrushConverter : IValueConverter, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            MemoryCategory category = values.Length > 0 && values[0] is MemoryCategory c ? c : MemoryCategory.Empty;
            int fillLevel = values.Length > 1 ? ParseFillLevel(values[1]) : 0;
            return GetBrush(category, fillLevel);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MemoryRangeBlockM block)
            {
                if (block.FillLevel <= 0) return GetBrush(MemoryCategory.Empty, 0);
                return GetBrush(block.Category, block.FillLevel);
            }

            int fillLevel = ParseFillLevel(value);
            return GetBrush(MemoryCategory.Upstream, fillLevel);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            Binding.DoNothing;

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
            CreateDoNothingResults(targetTypes);

        private static int ParseFillLevel(object value)
        {
            int fillLevel = 0;
            if (value is int i)
                fillLevel = i;
            else if (value != null)
                int.TryParse(value.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out fillLevel);

            return Math.Clamp(fillLevel, 0, 8);
        }

        private static Brush GetBrush(MemoryCategory category, int fillLevel)
        {
            string key = fillLevel <= 0
                ? "MemoryRangeEmpty"
                : $"MemoryRange{category}{fillLevel}";
            return Application.Current.TryFindResource(key) as Brush ?? Brushes.Transparent;
        }

        private static object[] CreateDoNothingResults(Type[] targetTypes)
        {
            object[] results = new object[targetTypes.Length];
            for (int index = 0; index < results.Length; index++)
                results[index] = Binding.DoNothing;

            return results;
        }
    }
}
