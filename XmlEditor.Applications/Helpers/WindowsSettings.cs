#region

using System;
using System.ComponentModel;
using System.Configuration;
using System.Windows;

#endregion

namespace XmlEditor.Applications.Helpers
{
    /// <summary>
    ///   Persists a Window's Size, Location and WindowState to UserScopeSettings
    /// </summary>
    public class WindowSettings
    {
        #region Fields

        /// <summary>
        ///   Register the "Save" attached property and the "OnSaveInvalidated" callback
        /// </summary>
        public static readonly DependencyProperty SaveProperty = DependencyProperty.RegisterAttached("Save",
                                                                                                     typeof (bool),
                                                                                                     typeof (
                                                                                                         WindowSettings),
                                                                                                     new FrameworkPropertyMetadata
                                                                                                         (new PropertyChangedCallback
                                                                                                              (OnSaveInvalidated)));

        private readonly Window window;

        private WindowApplicationSettings windowApplicationSettings;

        #endregion Fields

        #region Constructors

        public WindowSettings(Window window) { this.window = window; }

        #endregion Constructors

        #region Properties

        [Browsable(false)] public WindowApplicationSettings Settings {
            get {
                if (windowApplicationSettings == null) windowApplicationSettings = CreateWindowApplicationSettingsInstance();
                return windowApplicationSettings;
            }
        }

        #endregion Properties

        #region Methods

        public static void SetSave(DependencyObject dependencyObject, bool enabled) { dependencyObject.SetValue(SaveProperty, enabled); }

        protected virtual WindowApplicationSettings CreateWindowApplicationSettingsInstance() { return new WindowApplicationSettings(this); }

        /// <summary>
        ///   Load the Window Size Location and State from the settings object
        /// </summary>
        protected virtual void LoadWindowState() {
            Settings.Reload();
            if (Settings.Location != Rect.Empty) {
                window.Left = Settings.Location.Left;
                window.Top = Settings.Location.Top;
                window.Width = Settings.Location.Width;
                window.Height = Settings.Location.Height;
            }
            if (Settings.WindowState != WindowState.Maximized) window.WindowState = Settings.WindowState;
        }

        /// <summary>
        ///   Save the Window Size, Location and State to the settings object
        /// </summary>
        protected virtual void SaveWindowState() {
            Settings.WindowState = window.WindowState;
            Settings.Location = window.RestoreBounds;
            Settings.Save();
        }

        /// <summary>
        ///   Called when Save is changed on an object.
        /// </summary>
        private static void OnSaveInvalidated(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e) {
            var window = dependencyObject as Window;
            if (window != null)
                if ((bool) e.NewValue) {
                    var settings = new WindowSettings(window);
                    settings.Attach();
                }
        }

        private void Attach() {
            if (window != null) {
                window.Closing += WindowClosing;
                window.Initialized += WindowInitialized;
                window.Loaded += WindowLoaded;
            }
        }

        private void WindowClosing(object sender, CancelEventArgs e) { SaveWindowState(); }

        private void WindowInitialized(object sender, EventArgs e) { LoadWindowState(); }

        private void WindowLoaded(object sender, RoutedEventArgs e) { if (Settings.WindowState == WindowState.Maximized) window.WindowState = Settings.WindowState; }

        #endregion Methods

        #region Nested Types

        public class WindowApplicationSettings : ApplicationSettingsBase
        {
            #region Constructors

            //EV: Doesn't seem to be needed
            //private WindowSettings windowSettings;
            //public WindowApplicationSettings(WindowSettings windowSettings)
            //    : base(windowSettings.window.PersistId.ToString()) {
            //    this.windowSettings = windowSettings;
            //}
            public WindowApplicationSettings(WindowSettings windowSettings) { }

            #endregion Constructors

            #region Properties

            [UserScopedSetting] public Rect Location {
                get {
                    if (this["Location"] != null) return ((Rect) this["Location"]);
                    return Rect.Empty;
                }
                set { this["Location"] = value; }
            }

            [UserScopedSetting] public WindowState WindowState {
                get {
                    if (this["WindowState"] != null) return (WindowState) this["WindowState"];
                    return WindowState.Normal;
                }
                set { this["WindowState"] = value; }
            }

            #endregion Properties
        }

        #endregion Nested Types
    }
}