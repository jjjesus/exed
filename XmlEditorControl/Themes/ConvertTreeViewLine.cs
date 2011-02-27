#region

using System;
using System.Globalization;
using System.Windows.Controls;

#endregion

namespace TreeListControl.Themes
{
    internal class ConvertTreeViewLine
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var item = (TreeViewItem) value;
            var ic = ItemsControl.ItemsControlFromItemContainer(item);
            return ic.ItemContainerGenerator.IndexFromContainer(item) == ic.Items.Count - 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { throw new Exception("The method or operation is not implemented."); }
    }
}