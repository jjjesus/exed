#region

using System.Collections.Generic;
using System.Waf.Applications;
using System.Windows.Input;
using XmlEditor.Applications.Documents;
using XmlEditor.Applications.Services;
using XmlEditor.Applications.Views;

#endregion

namespace XmlEditor.Applications.ViewModels
{
    public class SaveChangesViewModel : ViewModel<ISaveChangesView>
    {
        private readonly DelegateCommand cancelCommand;
        private readonly IEnumerable<IDocument> documents;
        private readonly DelegateCommand noCommand;
        private readonly DelegateCommand yesCommand;
        private readonly IZoomService zoomService;
        private ViewResult viewResult;

        public SaveChangesViewModel(ISaveChangesView view, IZoomService zoomService, IEnumerable<IDocument> documents)
            : base(view) {
            this.zoomService = zoomService;
            this.documents = documents;
            yesCommand = new DelegateCommand(() => Close(ViewResult.Yes));
            noCommand = new DelegateCommand(() => Close(ViewResult.No));
            cancelCommand = new DelegateCommand(() => Close(ViewResult.Cancel));
            viewResult = ViewResult.Cancel;
        }

        public static string Title {
            get { return "Save changes?"; }
            //get { return ApplicationInfo.ProductName; }
        }

        public IZoomService ZoomService {
            get { return zoomService; }
        }

        public IEnumerable<IDocument> Documents {
            get { return documents; }
        }

        public ViewResult ViewResult {
            get { return viewResult; }
        }

        public ICommand YesCommand {
            get { return yesCommand; }
        }

        public ICommand NoCommand {
            get { return noCommand; }
        }

        public ICommand CancelCommand {
            get { return cancelCommand; }
        }

        public void ShowDialog(object owner) {
            viewResult = ViewResult.Cancel;
            ViewCore.ShowDialog(owner);
        }

        private void Close(ViewResult myViewResult) {
            viewResult = myViewResult;
            ViewCore.Close();
        }
    }
}