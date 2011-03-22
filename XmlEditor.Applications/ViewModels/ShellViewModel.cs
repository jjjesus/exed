#region

using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Timers;
using System.Waf.Applications;
using XmlEditor.Applications.Helpers;
using XmlEditor.Applications.Services;
using XmlEditor.Applications.Views;

#endregion

namespace XmlEditor.Applications.ViewModels
{
    [Export]
    public class ShellViewModel : ViewModel<IShellView>, IHandle<StatusMessage>
    {
        private const int StatusMessageTimeout = 5000;
        private readonly IZoomService zoomService;
        private readonly Timer statusMessageWiperTimer = new Timer(StatusMessageTimeout);
        private object contentView;

        [ImportingConstructor] public ShellViewModel(IShellView view, IZoomService zoomService) : base(view) {
            this.zoomService = zoomService;
            view.Closing += ViewClosing;
            EventAggregationProvider.Instance.Subscribe(this);

            statusMessageWiperTimer.Elapsed += (s, e) => {
                Status = "Ready";
                statusMessageWiperTimer.Stop();
            };
            statusMessageWiperTimer.Start();
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

        private string status;
        public string Status
        {
            get { return status; }
            set
            {
                if (status == value) return;
                status = value;
                statusMessageWiperTimer.Stop();
                statusMessageWiperTimer.Start();
                RaisePropertyChanged("Status");
            }
        }

        public event CancelEventHandler Closing;
        public void Show() { ViewCore.Show(); }

        public void Close() { ViewCore.Close(); }

        protected virtual void OnClosing(CancelEventArgs e) { if (Closing != null) Closing(this, e); }

        private void ViewClosing(object sender, CancelEventArgs e) { OnClosing(e); }
        
        public void Handle(StatusMessage message) { Status = message.Text; }
    }
}