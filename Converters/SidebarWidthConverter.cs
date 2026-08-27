namespace OneColumnEncoder.Converters;

public class SidebarWidthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool hasSidebar && parameter is string param)
        {
            string[] parts = param.Split(',');
            if (parts.Length >= 2
                && double.TryParse(parts[0], out double withSidebar)
                && double.TryParse(parts[1], out double withoutSidebar))
            {
                return hasSidebar ? withSidebar : withoutSidebar;
            }
        }
        return 760d;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}
