#region

using System.ComponentModel.Composition;
using XmlEditor.Applications.Views;

#endregion

namespace XmlEditor.Presentation.Views
{
    /// <summary>
    ///   Interaction logic for Window1.xaml
    /// </summary>
    [Export(typeof (IShellView))]
    public partial class ShellWindow : IShellView
    {
        public ShellWindow() {
            InitializeComponent();
        }

        //private ShellViewModel ViewModel { get { return DataContext as ShellViewModel; } }

        //private void ThisDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        //{
        //    openKeyBinding.Command = ViewModel.OpenCommand;
        //    saveKeyBinding.Command = ViewModel.SaveCommand;
        //}

    }
}