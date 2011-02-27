#region

using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;

#endregion

namespace XmlEditor.Presentation.Converters
{
    public class FileNameConverter : IMultiValueConverter
    {
        private static readonly FileNameConverter DefaultInstance = new FileNameConverter();

        public static FileNameConverter Default {
            get { return DefaultInstance; }
        }

        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if (!(values[0] is string) || !(values[1] is bool)) return DependencyProperty.UnsetValue;

            var fileName = (string) values[0];
            var modified = (bool) values[1];
            return Path.GetFileName(fileName) + (modified ? "*" : "");
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        #endregion
    }
}