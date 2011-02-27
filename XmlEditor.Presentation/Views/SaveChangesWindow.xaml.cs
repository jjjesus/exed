#region

using System.ComponentModel.Composition;
using System.Windows;
using XmlEditor.Applications.Views;

#endregion

namespace XmlEditor.Presentation.Views
{
    [Export(typeof (ISaveChangesView)), PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class SaveChangesWindow : ISaveChangesView
    {
        public SaveChangesWindow() {
            InitializeComponent();
        }

        #region ISaveChangesView Members

        public void ShowDialog(object owner) {
            Owner = owner as Window;
            ShowDialog();
        }

        #endregion
    }
}