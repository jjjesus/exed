#region

using System.Waf.Applications;

#endregion

namespace XmlEditor.Applications.Views
{
    public interface ISaveChangesView : IView
    {
        void ShowDialog(object owner);

        void Close();
    }
}