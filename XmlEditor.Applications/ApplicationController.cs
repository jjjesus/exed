#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Waf.Applications;
using XmlEditor.Applications.Controllers;
using XmlEditor.Applications.Documents;
using XmlEditor.Applications.Properties;
using XmlEditor.Applications.Services;
using XmlEditor.Applications.ViewModels;
using XmlEditor.Applications.Views;

#endregion

namespace XmlEditor.Applications
{
    [Export] public class ApplicationController : Controller
    {
        private readonly CompositionContainer container;
        private readonly IDocumentManager documentManager;
        private readonly DelegateCommand exitCommand;
        private readonly MainViewModel mainViewModel;
        private readonly XmlDocumentController myXmlDocumentController;
        private readonly ShellViewModel shellViewModel;
        private XmlDocumentType xmlDocumentType;

        [ImportingConstructor] 
        public ApplicationController(CompositionContainer container, IDocumentManager documentManager) {
            InitializeCultures();

            this.container = container;
            this.documentManager = documentManager;
            this.documentManager.DocumentsClosing += DocumentManagerDocumentsClosing;

            myXmlDocumentController = container.GetExportedValue<XmlDocumentController>();
            shellViewModel = container.GetExportedValue<ShellViewModel>();
            mainViewModel = container.GetExportedValue<MainViewModel>();
            shellViewModel.Closing += ShellViewModelClosing;
            exitCommand = new DelegateCommand(Close);
        }

        public void Initialize() {
            mainViewModel.ExitCommand = exitCommand;

            xmlDocumentType = new XmlDocumentType();
            documentManager.Register(xmlDocumentType);
        }

        /// <summary>
        /// Runs the specified args.
        /// </summary>
        /// <param name="args">The command line arguments, if any.</param>
        public void Run(string[] args) {
            shellViewModel.ContentView = mainViewModel.View;
            shellViewModel.Show();
            if (args != null) mainViewModel.Open(args);
        }

        public void Shutdown() {
            // Save your settings here
            if (mainViewModel.NewLanguage != null) Settings.Default.UICulture = mainViewModel.NewLanguage.Name;
            Settings.Default.Save();
        }

        private static void InitializeCultures() {
            if (!String.IsNullOrEmpty(Settings.Default.Culture)) Thread.CurrentThread.CurrentCulture = new CultureInfo(Settings.Default.Culture);
            if (!String.IsNullOrEmpty(Settings.Default.UICulture)) Thread.CurrentThread.CurrentUICulture = new CultureInfo(Settings.Default.UICulture);
        }

        private void Close() { shellViewModel.Close(); }

        private void ShellViewModelClosing(object sender, CancelEventArgs e) {
            // Try to close all documents and see if the user has already saved them.
            e.Cancel = !documentManager.CloseAll();
        }

        private void DocumentManagerDocumentsClosing(object sender, DocumentsClosingEventArgs e) {
            IEnumerable<IDocument> modifiedDocuments = e.Documents.Where(d => d.Modified).ToList();
            if (!modifiedDocuments.Any()) return;

            // Show the save changes view to the user
            var saveChangesView = container.GetExportedValue<ISaveChangesView>();
            var zoomService = container.GetExportedValue<IZoomService>();
            var saveChangesViewModel = new SaveChangesViewModel(saveChangesView, zoomService, modifiedDocuments);
            saveChangesViewModel.ShowDialog(shellViewModel.View);

            e.Cancel = saveChangesViewModel.ViewResult == ViewResult.Cancel;

            if (saveChangesViewModel.ViewResult == ViewResult.Yes) foreach (var document in modifiedDocuments) documentManager.Save(document);
        }
    }
}