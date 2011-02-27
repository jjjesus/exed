using System.IO;
using System.Waf.Applications;

namespace XmlEditor.Applications.Views
{
    public interface IXsltView : IView
    {
        void NavigateToStream(Stream html);
        event System.Windows.RoutedEventHandler Loaded;
    }
}