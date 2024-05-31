using adrilight_content_creator.ViewModel;
using adrilight_shared.Models.Device;
using adrilight_shared.Models.Drawable;
using adrilight_shared.ViewModel;
using HandyControl.Themes;
using Newtonsoft.Json;
using Ninject;
using Ninject.Extensions.Conventions;
using Squirrel;
using System;
using System.Windows;
using System.Windows.Media;

namespace adrilight_content_creator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public sealed partial class App : Application
    {
        private const string ADRILIGHT_RELEASES = "https://github.com/Kaitoukid93/Ambinity_Developer_Release";
        private KnownTypesBinder _knownTypeBinders { get; set; }
        protected override void OnStartup(StartupEventArgs startupEvent)
        {

            base.OnStartup(startupEvent);
            try
            {
                using (var mgr = new UpdateManager(ADRILIGHT_RELEASES))
                {
                    // Note, in most of these scenarios, the app exits after this method
                    // completes!
                    SquirrelAwareApp.HandleEvents(
                      onInitialInstall: v => mgr.CreateShortcutForThisExe(),
                      onAppUpdate: v => mgr.CreateShortcutForThisExe(),
                      onAppUninstall: v => mgr.RemoveShortcutForThisExe());
                }

            }
            catch (Exception ex)
            {

            }
            _knownTypeBinders = new KnownTypesBinder();
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Objects, SerializationBinder = _knownTypeBinders };
            ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
            ThemeManager.Current.AccentColor = new SolidColorBrush(Color.FromArgb(255, 255, 69, 0));
            kernel = SetupDependencyInjection();
            ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
            this.Resources["Locator"] = new ViewModelLocator(kernel);
            OpenMainWindow();

        }





        internal static IKernel SetupDependencyInjection()
        {
            var kernel = new StandardKernel();
            kernel.Bind<MainViewModel>().ToSelf().InSingletonScope();
            kernel.Bind(x => x.FromThisAssembly()
            .SelectAllClasses()
            .InheritedFrom<ISelectableViewPart>()
            .BindAllInterfaces());
            kernel.Bind<DeviceHardwareSettings>().ToSelf().InSingletonScope();
            kernel.Bind<DeviceExporterViewModel>().ToSelf().InSingletonScope();
            kernel.Bind<DeviceUtilViewModel>().ToSelf().InSingletonScope();
            kernel.Bind<DeviceCanvas>().ToSelf().InSingletonScope();
            kernel.Bind<OutputMappingViewModel>().ToSelf().InSingletonScope();
            return kernel;
        }

        MainWindow _mainForm;
        private IKernel kernel;


        private void OpenMainWindow()
        {
            if (_mainForm == null)
            {
                _mainForm = new MainWindow();
                _mainForm.Closed += MainForm_FormClosed;
                _mainForm.Show();
            }
            else
            {
                //bring to front?
                _mainForm.Focus();
            }
        }

        private void MainForm_FormClosed(object sender, EventArgs e)
        {
            if (_mainForm == null) return;

            //deregister to avoid memory leak
            _mainForm.Closed -= MainForm_FormClosed;
            _mainForm = null;
        }

    }
}