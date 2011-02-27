#region

using System.ComponentModel.Composition;
using System.IO;
using XmlEditor.Applications.Views;

#endregion

namespace XmlEditor.Presentation.Views
{
    /// <summary>
    ///   Interaction logic for XsltView.xaml
    /// </summary>
    [Export(typeof (IXsltView)), PartCreationPolicy(CreationPolicy.NonShared)] public partial class XsltView : IXsltView
    {
        public XsltView() {
            InitializeComponent();
        }

        #region IXsltView Members

        public void NavigateToStream(Stream html) {
            html.Position = 0;
            webBrowser.NavigateToStream(html);
        }

        #endregion
    }
}