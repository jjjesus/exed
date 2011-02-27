#region

using System.ComponentModel;
using System.Waf.Applications;

#endregion

namespace XmlEditor.Applications.Views
{
    public interface IShellView : IView
    {
        event CancelEventHandler Closing;

        void Show();
        void Close();
    }
}