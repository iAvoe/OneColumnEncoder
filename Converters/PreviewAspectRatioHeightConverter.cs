namespace OneColumnEncoder.Converters;

public sealed class PreviewAspectRatioHeightConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length == 0 || values[0] is not double width)
            return 0;

        int maximum = values.Length > 1 && values[1] is int maxHeight
            ? maxHeight
            : 480;
        return CalculateHeight(width, maximum);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();

    public static int CalculateHeight(double width, int maximum = 480)
    {
        if (double.IsNaN(width) || double.IsInfinity(width) || width <= 0d)
            return Math.Max(1, maximum);

        return Math.Max(1, Math.Min(Math.Max(1, maximum), (int)Math.Round(width / 2d)));
    }
}
