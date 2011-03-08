#region

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Waf.Applications;
using System.Waf.Applications.Services;
using System.Windows;
using System.Windows.Input;
using XmlEditor.Applications.Documents;
using XmlEditor.Applications.Interfaces;
using XmlEditor.Applications.Views;

#endregion

namespace XmlEditor.Applications.ViewModels
{
    [Export]
    public class MainViewModel : ViewModel<IMainView>
    {
        private readonly DelegateCommand aboutCommand;
        private readonly DelegateCommand closeCommand;
        private readonly IDocumentManager documentManager;
        private readonly ObservableCollection<object> documentViews;
        private readonly DelegateCommand englishCommand;
        private readonly DelegateCommand germanCommand;
        private readonly IMessageService messageService;
        private readonly DelegateCommand newCommand;
        private readonly DelegateCommand nextDocumentCommand;
        private readonly DelegateCommand openCommand;
        private readonly DelegateCommand saveAsCommand;
        private readonly DelegateCommand saveCommand;
        private readonly DelegateCommand printCommand;
        private readonly DelegateCommand printPreviewCommand;
        private IDocument activeDocument;
        private object activeDocumentView;
        private ICommand exitCommand;

        [ImportingConstructor]
        public MainViewModel(IMainView view, IDocumentManager documentManager, IMessageService messageService)
            : base(view) {
            this.documentManager = documentManager;
            this.messageService = messageService;
            documentViews = new ObservableCollection<object>();
            newCommand = new DelegateCommand(NewDocument);
            openCommand = new DelegateCommand(OpenDocument);
            closeCommand = new DelegateCommand(CloseDocument, CanCloseDocument);
            saveCommand = new DelegateCommand(SaveDocument, CanSaveDocument);
            saveAsCommand = new DelegateCommand(SaveAsDocument, CanSaveAsDocument);
            englishCommand = new DelegateCommand(() => SelectLanguage(new CultureInfo("en-US")));
            germanCommand = new DelegateCommand(() => SelectLanguage(new CultureInfo("de-DE")));
            aboutCommand = new DelegateCommand(ShowAboutMessage);
            nextDocumentCommand = new DelegateCommand(SetNextDocumentActive);
            printCommand = new DelegateCommand(PrintDocument, CanPrintDocument);
            printPreviewCommand = new DelegateCommand(PrintPreviewDocument, CanPrintDocument);

            AddWeakEventListener(documentManager, DocumentManagerPropertyChanged);
        }

        public ObservableCollection<object> DocumentViews {
            get { return documentViews; }
        }

        public object ActiveDocumentView {
            get { return activeDocumentView; }
            set {
                if (activeDocumentView == value) return;
                activeDocumentView = value;
                RaisePropertyChanged("ActiveDocumentView");
            }
        }

        private string searchTerm;
        public string SearchTerm {
            get { return searchTerm; } 
            set {
                searchTerm = value;
                Search(true);
                RaisePropertyChanged("SearchTerm");
            }
        }

        private void Search(bool nextTerm) {
            if (ActiveDocumentView == null) return;
            if (!(((FrameworkElement)ActiveDocumentView).DataContext is ISearch)) return;
            (((FrameworkElement)ActiveDocumentView).DataContext as ISearch).Search(searchTerm, nextTerm);
        }

        public CultureInfo NewLanguage { get; private set; }

        public ObservableCollection<string> MRU { get { return documentManager.MRU; } }

        //public ICommand UndoCommand { get { return undoCommand; } }

        public ICommand NewCommand {
            get { return newCommand; }
        }

        public ICommand OpenCommand {
            get { return openCommand; }
        }

        public ICommand CloseCommand {
            get { return closeCommand; }
        }

        public ICommand SaveCommand {
            get { return saveCommand; }
        }

        public ICommand SaveAsCommand {
            get { return saveAsCommand; }
        }

        public ICommand PrintPreviewCommand {
            get { return printPreviewCommand; }
        }

        private List<DocumentSubType> documentTypes;
        public List<DocumentSubType> DocumentTypes {
            get {
                if (documentTypes == null) {
                    documentTypes = new List<DocumentSubType>();
                    foreach (var docType in documentManager.DocumentTypes) {
                        if (docType.SubTypes != null && docType.SubTypes.Count > 0) documentTypes.AddRange(docType.SubTypes);
                        else documentTypes.Add(new DocumentSubType {Description = docType.Description, Tag = docType.Description});
                    }
                }
                return documentTypes;
            }
        }

        public ICommand PrintCommand {
            get { return printCommand; }
        }

        public ICommand ExitCommand {
            get { return exitCommand; }
            set {
                if (exitCommand == value) return;
                exitCommand = value;
                RaisePropertyChanged("ExitCommand");
            }
        }

        public ICommand EnglishCommand {
            get { return englishCommand; }
        }

        public ICommand GermanCommand {
            get { return germanCommand; }
        }

        public ICommand AboutCommand {
            get { return aboutCommand; }
        }

        public ICommand NextDocumentCommand {
            get { return nextDocumentCommand; }
        }

        private void NewDocument(object docSubType) {
            var dst = (DocumentSubType) docSubType;
            if (dst == null || !documentManager.DocumentTypes.Contains(dst.Type)) return;
            documentManager.New(dst.Type, dst.Tag);
            UpdateCommands();
            //documentManager.New(documentManager.DocumentTypes.First());
        }

        private void OpenDocument(object fileName) {
            if (fileName == null) documentManager.Open(null);
            else documentManager.Open(fileName as string);
            UpdateCommands();
        }

        private bool CanCloseDocument() {
            return documentManager.ActiveDocument != null;
        }

        private void CloseDocument() {
            documentManager.Close(documentManager.ActiveDocument);
            UpdateCommands();
        }

        private bool CanSaveDocument()
        {
            return documentManager.ActiveDocument != null && documentManager.ActiveDocument.Modified;
        }

        private void SaveDocument() {
            documentManager.Save(documentManager.ActiveDocument);
        }

        private void PrintDocument() {
            documentManager.Print(documentManager.ActiveDocument);
        }

        private bool CanPrintDocument() { return documentManager.ActiveDocument != null; }

        private void PrintPreviewDocument() {
            documentManager.PrintPreview(documentManager.ActiveDocument);
        }

        private bool CanSaveAsDocument() {
            return documentManager.ActiveDocument != null;
        }

        private void SaveAsDocument() {
            documentManager.SaveAs(documentManager.ActiveDocument);
        }

        private void SelectLanguage(CultureInfo uiCulture) {
            if (!uiCulture.Equals(CultureInfo.CurrentUICulture)) //messageService.ShowMessage(Resources.RestartApplication + "\n\n" +
                //                           Resources.ResourceManager.GetString("RestartApplication", uiCulture));
                messageService.ShowMessage("Restart Application \n\n");
            NewLanguage = uiCulture;
        }

        private void ShowAboutMessage() {
            messageService.ShowMessage(string.Format(CultureInfo.CurrentCulture, "About {0}, {1}",
                                                     ApplicationInfo.ProductName, ApplicationInfo.Version));
        }

        private void SetNextDocumentActive() {
            if (documentManager.Documents.Count <= 1) return;
            var index = documentManager.Documents.IndexOf(documentManager.ActiveDocument) + 1;
            if (index >= documentManager.Documents.Count) index = 0;
            documentManager.ActiveDocument = documentManager.Documents[index];
        }

        private void DocumentManagerPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName != "ActiveDocument") return;
            if (activeDocument != null) RemoveWeakEventListener(activeDocument, ActiveDocumentPropertyChanged);
            activeDocument = documentManager.ActiveDocument;
            if (activeDocument != null) AddWeakEventListener(activeDocument, ActiveDocumentPropertyChanged);
            UpdateCommands();
        }

        private void ActiveDocumentPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == "Modified") UpdateCommands();
        }

        private void UpdateCommands() {
            closeCommand.RaiseCanExecuteChanged();
            saveCommand.RaiseCanExecuteChanged();
            saveAsCommand.RaiseCanExecuteChanged();
            printCommand.RaiseCanExecuteChanged();
            printPreviewCommand.RaiseCanExecuteChanged();
        }
    }

}