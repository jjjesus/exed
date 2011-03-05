#region

using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Waf.Applications;
using System.Windows;
using System.Windows.Threading;
using XmlEditor.Applications;

#endregion

namespace XmlEditor.Presentation
{
    /// <summary>
    ///   Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private ApplicationController controller;
        private const string PluginDirectory = "Plugins";

        static App() {
#if (DEBUG)
            WafConfiguration.Debug = true;
#endif
        }

        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);

#if (DEBUG != true)
            // Don't handle the exceptions in Debug mode because otherwise the Debugger wouldn't
            // jump into the code when an exception occurs.
            DispatcherUnhandledException += AppDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += AppDomainUnhandledException;
#endif

            var catalog = new AggregateCatalog();
            // Add the WpfApplicationFramework assembly to the catalog
            catalog.Catalogs.Add(new AssemblyCatalog(typeof (Controller).Assembly));
            // Add the Presentation assembly into the catalog
            catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
            // Add the Applications assembly into the catalog
            catalog.Catalogs.Add(new AssemblyCatalog(typeof (ApplicationController).Assembly));
            // Add the plugin directory into the catelog
            catalog.Catalogs.Add(new DirectoryCatalog(Path.Combine(AppDir, PluginDirectory)));
            var container = new CompositionContainer(catalog);
            var batch = new CompositionBatch();
            batch.AddExportedValue(container);
            container.Compose(batch);

            controller = container.GetExportedValue<ApplicationController>();
            controller.Initialize();
            controller.Run();
        }

        protected override void OnExit(ExitEventArgs e) {
            controller.Shutdown();
            //Presentation.Properties.Settings.Default.Save();
            base.OnExit(e);
        }

        private static void AppDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
            HandleException(e.Exception, false);
            e.Handled = true;
        }

        private static void AppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e) {
            HandleException(e.ExceptionObject as Exception, e.IsTerminating);
        }

        private static void HandleException(Exception e, bool isTerminating) {
            if (e == null) return;

            Trace.TraceError(e.ToString());

            if (!isTerminating)
                MessageBox.Show(string.Format(CultureInfo.CurrentCulture, "Unknown Error: {0}", e), 
                    ApplicationInfo.ProductName, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private static string AppDir { get { return AppDomain.CurrentDomain.BaseDirectory; } }

    }
}