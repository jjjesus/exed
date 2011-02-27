using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TreeListControl.Resources
{
    public class ConvertTextLengthToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var text = (string) value;
            return (string.IsNullOrEmpty(text) || text.Length == 0) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { throw new NotImplementedException(); }
    }
}
