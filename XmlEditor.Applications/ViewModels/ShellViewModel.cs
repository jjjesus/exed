#region

using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Waf.Applications;
using XmlEditor.Applications.Services;
using XmlEditor.Applications.Views;

#endregion

namespace XmlEditor.Applications.ViewModels
{
    [Export] public class ShellViewModel : ViewModel<IShellView>
    {
        private readonly IZoomService zoomService;
        private object contentView;

        [ImportingConstructor] public ShellViewModel(IShellView view, IZoomService zoomService) : base(view) {
            this.zoomService = zoomService;
            view.Closing += ViewClosing;
        }

        //public static string Title { get { return ApplicationInfo.ProductName; } }
        public static string Title { get { return "eXed - eXtended XML editor"; } }

        public IZoomService ZoomService { get { return zoomService; } }

        public object ContentView {
            get { return contentView; }
            set {
                if (contentView == value) return;
                contentView = value;
                RaisePropertyChanged("ContentView");
            }
        }

        public event CancelEventHandler Closing;
        public void Show() { ViewCore.Show(); }

        public void Close() { ViewCore.Close(); }

        protected virtual void OnClosing(CancelEventArgs e) { if (Closing != null) Closing(this, e); }

        private void ViewClosing(object sender, CancelEventArgs e) { OnClosing(e); }
    }
}