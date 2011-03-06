#region

using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Input;
using XmlEditor.Applications.Views;

#endregion

namespace XmlEditor.Presentation.Views
{
    [Export(typeof (IMainView))] public partial class MainView : IMainView
    {
        public MainView() {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object pSender, RoutedEventArgs pRoutedEventArgs)
        {
            Keyboard.Focus(Documents);
        }
    }
}