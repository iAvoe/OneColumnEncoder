using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace OneColumnEncoder.Converters
{
    public class StringToCharArray : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? text = value as string;
            if (string.IsNullOrEmpty(text)) return null;
            return text.ToCharArray();
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        { 
            char[]? charArray = value as char[];
            if (charArray == null) return null;
            return new string(charArray);
        }
    }
}
