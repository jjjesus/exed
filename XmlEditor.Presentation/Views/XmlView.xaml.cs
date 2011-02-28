#region

using System.ComponentModel.Composition;
using XmlEditor.Applications.Views;

#endregion

namespace XmlEditor.Presentation.Views
{
    [Export(typeof (IXmlView)), PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class XmlView : IXmlView
    {
        public XmlView() {
            InitializeComponent();
        }

        //private ScenarioViewModel ViewModel { get { return DataContext as ScenarioViewModel;}}

        //private void TabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
        //    if (e.)
        //}
    }
}