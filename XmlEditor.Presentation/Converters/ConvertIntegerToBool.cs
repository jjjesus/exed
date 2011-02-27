using System;
using System.Globalization;
using System.Windows.Data;

namespace XmlEditor.Presentation.Converters
{
    public class ConvertIntegerToBool : IValueConverter
    {
        private static readonly ConvertIntegerToBool DefaultInstance = new ConvertIntegerToBool();

        public static ConvertIntegerToBool Default
        {
            get { return DefaultInstance; }
        }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(parameter is string)) return null;
            try
            {
                var limit = double.Parse(parameter as string);
                var val = (int) value;
                return val > limit;
            }
            catch
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}