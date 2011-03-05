using System.ComponentModel.Composition;
using XmlEditor.Applications.Views;

namespace XmlEditor.Presentation.Views
{
    /// <summary>
    /// Interaction logic for SearchView.xaml
    /// </summary>
    [Export(typeof (ISearchView)), PartCreationPolicy(CreationPolicy.NonShared)] 
    public partial class SearchView : ISearchView
    {
        public SearchView()
        {
            InitializeComponent();
        }
    }
}
