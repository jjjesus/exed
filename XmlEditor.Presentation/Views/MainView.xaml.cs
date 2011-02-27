#region

using System.ComponentModel.Composition;
using System.Windows;
using XmlEditor.Applications.ViewModels;
using XmlEditor.Applications.Views;

#endregion

namespace XmlEditor.Presentation.Views
{
    [Export(typeof (IMainView))] public partial class MainView : IMainView
    {
        public MainView() {
            InitializeComponent();
            DataContextChanged += ThisDataContextChanged;
        }

        private MainViewModel ViewModel { get { return DataContext as MainViewModel; } }

        private void ThisDataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            newKeyBinding.Command = ViewModel.NewCommand;
            openKeyBinding.Command = ViewModel.OpenCommand;
            closeKeyBinding.Command = ViewModel.CloseCommand;
            saveKeyBinding.Command = ViewModel.SaveCommand;
            //printKeyBinding.Command = ViewModel.PrintCommand;
            aboutKeyBinding.Command = ViewModel.AboutCommand;
            nextDocumentKeyBinding.Command = ViewModel.NextDocumentCommand;
        }
    }
}