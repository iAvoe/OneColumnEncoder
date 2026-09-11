namespace OneColumnEncoder.Converters;

public sealed class PreviewAspectRatioHeightConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2
            || values[0] is not double width
            || double.IsNaN(width)
            || double.IsInfinity(width)
            || width <= 0d)
            return double.NaN;

        return values[1] is true ? width / 2d : double.NaN;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
