using System;
using System.ComponentModel.Composition;
using System.Windows.Controls;
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

        private void ListViewSelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (FoundNodes.SelectedItem != null) FoundNodes.ScrollIntoView(FoundNodes.SelectedItem);
        }
    }
}
