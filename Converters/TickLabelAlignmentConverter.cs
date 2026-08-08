namespace OneColumnEncoder.Converters
{
    public class TickLabelAlignmentConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2 || values[0] is not int index || values[1] is not int tickCount)
            {
                return HorizontalAlignment.Center;
            }

            if (index == 0)
                return HorizontalAlignment.Left;
            if (index == tickCount - 1)
                return HorizontalAlignment.Right;
            return HorizontalAlignment.Center;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            object[] results = new object[targetTypes.Length];
            for (int index = 0; index < results.Length; index++)
                results[index] = Binding.DoNothing;

            return results;
        }
    }
}
