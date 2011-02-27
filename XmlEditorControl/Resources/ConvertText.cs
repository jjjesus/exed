using System;
using System.Globalization;
using System.Windows.Data;

namespace TreeListControl.Resources
{
    public class ConvertText : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (!(value is string)) return null;
            return (value as string).Replace("\n", " ").Trim();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { throw new NotImplementedException(); }
    }
}