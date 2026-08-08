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
            if (!TryParseParameters(parameter, culture, out int count, out double spacing) || count <= 0)
                return Binding.DoNothing;

            if (!TryGetDouble(value, culture, out double buttonWidth))
                return Binding.DoNothing;

            return buttonWidth * count + spacing * (count - 1);
        }

        private static bool TryParseParameters(object parameter, CultureInfo culture, out int count, out double spacing)
        {
            count = 0;
            spacing = 0d;

            if (parameter is not string text)
                return false;

            string[] parts = text.Split(',');
            if (!int.TryParse(parts[0], out count))
                return false;

            if (parts.Length > 1 && !double.TryParse(parts[1], NumberStyles.Float, culture, out spacing))
                return false;

            return true;
        }

        private static bool TryGetDouble(object value, CultureInfo culture, out double result)
        {
            if (value is double doubleValue)
            {
                result = doubleValue;
                return true;
            }

            try
            {
                result = System.Convert.ToDouble(value, culture);
                return true;
            }
            catch (Exception)
            {
                result = 0d;
                return false;
            }
        }
    }
}
