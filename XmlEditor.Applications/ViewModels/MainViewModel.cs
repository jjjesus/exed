﻿#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Waf.Applications;
using System.Waf.Applications.Services;
using System.Windows;
using System.Windows.Input;
using XmlEditor.Applications.Documents;
using XmlEditor.Applications.Helpers;
using XmlEditor.Applications.Interfaces;
using XmlEditor.Applications.Views;

#endregion

namespace XmlEditor.Applications.ViewModels
{
    [Export]
    public class MainViewModel : ViewModel<IMainView>
    {
        private readonly IMessageService messageService;
        private readonly IFileDialogService fileDialogService;
        private readonly IDocumentManager documentManager;
        private readonly ObservableCollection<object> documentViews;

        private readonly DelegateCommand aboutCommand;
        private readonly DelegateCommand closeCommand;
        private readonly DelegateCommand englishCommand;
        private readonly DelegateCommand germanCommand;
        private readonly DelegateCommand newDocumentCommand;
        private readonly DelegateCommand newXsdCacheBasedDocumentCommand;
        private readonly DelegateCommand nextDocumentCommand;
        private readonly DelegateCommand openCommand;
        private readonly DelegateCommand saveAsCommand;
        private readonly DelegateCommand saveCommand;
        private readonly DelegateCommand printCommand;
        private readonly DelegateCommand printPreviewCommand;

        private readonly RelayCommand<IDataObject> dropCommand;
        private IDocument activeDocument;
        private object activeDocumentView;
        private ICommand exitCommand;

        [ImportingConstructor]
        public MainViewModel(IMainView view, IDocumentManager documentManager, IMessageService messageService, IFileDialogService fileDialogService)
            : base(view) {
            this.documentManager = documentManager;
            this.messageService = messageService;
            this.fileDialogService = fileDialogService;
            documentViews = new ObservableCollection<object>();
            newDocumentCommand = new DelegateCommand(NewDocument);
            newXsdCacheBasedDocumentCommand = new DelegateCommand(NewXsdCacheBasedDocument);
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

            dropCommand = new RelayCommand<IDataObject>(DropExecuted, DropCanExecute);

            AddWeakEventListener(documentManager, DocumentManagerPropertyChanged);
        }

        public void DropExecuted(IDataObject obj) {
            var files = (string[])obj.GetData(DataFormats.FileDrop);
            foreach (var file in files.Where(file => documentManager.CanOpen(file))) documentManager.Open(file);
        }

        public bool DropCanExecute(IDataObject obj) {
            if (!obj.GetDataPresent(DataFormats.FileDrop, false)) return false;
            var files = (string[])obj.GetData(DataFormats.FileDrop);
            return files.Any(file => documentManager.CanOpen(file));
        }

        public ICommand DropCommand { get { return dropCommand; } }

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

        public ICommand NewDocumentCommand {
            get { return newDocumentCommand; }
        }

        public ICommand NewXsdCacheBasedDocumentCommand {
            get { return newXsdCacheBasedDocumentCommand; }
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

        /// <summary>
        /// Create a new XML document by opening an XSD.
        /// </summary>
        public void NewDocument() {
            var result = fileDialogService.ShowOpenFileDialog(new FileType("XSD files", ".xsd"));
            if (!result.IsValid) return;
            var docType = (from d in documentManager.DocumentTypes where d is XmlDocumentType select d).FirstOrDefault();
            if (docType == null) return;
            documentManager.New(docType, result.FileName);
        }

        /// <summary>
        /// Create a new XML document by opening and XSD in the XsdCache folder.
        /// </summary>
        /// <param name="docSubType">Type of the doc sub.</param>
        private void NewXsdCacheBasedDocument(object docSubType) {
            var dst = (DocumentSubType) docSubType;
            if (dst == null || !documentManager.DocumentTypes.Contains(dst.Type)) return;
            documentManager.New(dst.Type, dst.Tag);
            UpdateCommands();
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

        /// <summary>
        /// Opens the specified arguments, which are supposed to be files.
        /// </summary>
        /// <param name="args">The args.</param>
        public void Open(string[] args) {
            if (args == null) throw new ArgumentNullException("args");
            foreach (var file in args) documentManager.Open(file);
        }
    }

}