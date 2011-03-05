#region

using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using XmlEditor.Applications.ViewModels;
using XmlEditor.Applications.Views;

#endregion

namespace XmlEditor.Presentation.Views
{
    [Export(typeof (IMainView))] public partial class MainView : IMainView
    {
        public MainView() {
            InitializeComponent();
            //DataContextChanged += ThisDataContextChanged;
            Loaded += OnLoaded;
        }

        private MainViewModel ViewModel { get { return DataContext as MainViewModel; } }

        private void OnLoaded(object pSender, RoutedEventArgs pRoutedEventArgs)
        {
            Keyboard.Focus(Documents);
        }

        private void ThisDataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            //var viewModel = ViewModel;
            //newKeyBinding.Command = viewModel.NewCommand;
            //openKeyBinding.Command = viewModel.OpenCommand;
            //closeKeyBinding.Command = viewModel.CloseCommand;
            //saveKeyBinding.Command = viewModel.SaveCommand;
            //printKeyBinding.Command = viewModel.PrintCommand;
            //printPreviewKeyBinding.Command = viewModel.PrintPreviewCommand;
            //aboutKeyBinding.Command = viewModel.AboutCommand;
            //nextDocumentKeyBinding.Command = viewModel.NextDocumentCommand;
        }
    }
}