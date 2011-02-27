#region

using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Waf.Applications;
using XmlEditor.Applications.Documents;
using XmlEditor.Applications.ViewModels;
using XmlEditor.Applications.Views;

#endregion

namespace XmlEditor.Applications.Controllers
{
    [Export]
    public class XmlDocumentController : DocumentController
    {
        private readonly CompositionContainer container;
        private readonly IDocumentManager documentManager;
        private readonly MainViewModel mainViewModel;
        private readonly Dictionary<MyXmlDocument, XmlViewModel> xmlViewModels;

        [ImportingConstructor]
        public XmlDocumentController(CompositionContainer container, IDocumentManager documentManager,
                                          MainViewModel mainViewModel) : base(documentManager) {
            this.container = container;
            this.documentManager = documentManager;
            this.mainViewModel = mainViewModel;
            xmlViewModels = new Dictionary<MyXmlDocument, XmlViewModel>();
            this.mainViewModel.PropertyChanged += ShellViewModelPropertyChanged;
        }

        protected override void OnDocumentAdded(IDocument document) {
            var myXmlDocument = document as MyXmlDocument;
            if (myXmlDocument == null) return;
            var xmlView = container.GetExportedValue<IXmlView>();
            var xmlViewModel = new XmlViewModel(container, xmlView, myXmlDocument);
            xmlViewModels.Add(myXmlDocument, xmlViewModel);
            mainViewModel.DocumentViews.Add(xmlViewModel.View);
        }

        protected override void OnDocumentRemoved(IDocument document) {
            var myXmlDocument = document as MyXmlDocument;
            if (myXmlDocument == null) return;
            mainViewModel.DocumentViews.Remove(xmlViewModels[myXmlDocument].View);
            xmlViewModels.Remove(myXmlDocument);
        }

        protected override void OnActiveDocumentChanged(IDocument activeDocument) {
            if (activeDocument == null) mainViewModel.ActiveDocumentView = null;
            else {
                var myXmlDocument = activeDocument as MyXmlDocument;
                if (myXmlDocument != null) mainViewModel.ActiveDocumentView = xmlViewModels[myXmlDocument].View;
            }
        }

        private void ShellViewModelPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName != "ActiveDocumentView") return;
            var xmlView = mainViewModel.ActiveDocumentView as IView;
            if (xmlView == null) return;
            var xmlViewModel = xmlView.GetViewModel<XmlViewModel>();
            if (xmlViewModel != null) documentManager.ActiveDocument = xmlViewModel.Document;
        }
    }
}