using adrilight.Manager;
using adrilight.Ninject;
using adrilight.Services.CaptureEngine;
using adrilight.Services.CaptureEngine.ScreenCapture;
using adrilight.Services.DeviceDiscoveryServices;
using adrilight.Services.OpenRGBService;
using adrilight.Ticker;
using adrilight.Util;
using adrilight.View;
using adrilight.View.Screens.OOBExperience;
using adrilight.ViewModel;
using adrilight.ViewModel.Automation;
using adrilight_shared.Helpers;
using adrilight_shared.Models.Device;
using adrilight_shared.Models.Language;
using adrilight_shared.Services;
using adrilight_shared.Settings;
using adrilight_shared.View.Dialogs;
using adrilight_shared.ViewModel;
using HandyControl.Themes;
using Microsoft.ApplicationInsights;
using Microsoft.Win32;
using Newtonsoft.Json;
using Ninject;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Un4seen.Bass;
using SplashScreen = adrilight.View.SplashScreen;

namespace adrilight
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    ///
    //structures to display bitmap image after getting data from shader

    public sealed partial class App : System.Windows.Application
    {
        private static System.Threading.Mutex _mutex = null;
        private static Mutex _adrilightMutex;
        private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "adrilight\\");
        private KnownTypesBinder _knownTypeBinders { get; set; }
        protected override async void OnStartup(StartupEventArgs startupEvent)
        {
            //load setting if exist
            var settingsManager = new UserSettingsManager();
            var generalSettings = settingsManager.LoadIfExists() ?? settingsManager.MigrateOrDefault();
            //setting the color
            ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
            var accentColor = generalSettings.AccentColor;
            ThemeManager.Current.AccentColor = new SolidColorBrush(accentColor);
            //ask for language
            if (generalSettings.AskForSelectingLang)
            {
                var lang = await SelectLanguage();
                generalSettings.AskForSelectingLang = false;
                if (lang != null)
                {
                    Thread.CurrentThread.CurrentCulture = lang.Culture;
                    Thread.CurrentThread.CurrentUICulture = lang.Culture;
                    CultureInfo.DefaultThreadCurrentCulture = lang.Culture;
                    CultureInfo.DefaultThreadCurrentUICulture = lang.Culture;
                    generalSettings.AppCulture = lang;
                }

            }
            else
            {
                Thread.CurrentThread.CurrentCulture = generalSettings.AppCulture.Culture;
                Thread.CurrentThread.CurrentUICulture = generalSettings.AppCulture.Culture;
                CultureInfo.DefaultThreadCurrentCulture = generalSettings.AppCulture.Culture;
                CultureInfo.DefaultThreadCurrentUICulture = generalSettings.AppCulture.Culture;
            }

            //check if this app is already open in the background
            _adrilightMutex = new Mutex(true, "adrilight2");
            if (!_adrilightMutex.WaitOne(TimeSpan.Zero, true))
            {
                //another instance is already running!
                HandyControl.Controls.MessageBox.Show(adrilight_shared.Properties.Resources.App_Already_Launch
                    , adrilight_shared.Properties.Resources.App_Already_Launch_Header,MessageBoxButton.OK,MessageBoxImage.Warning);
                Shutdown();
                return;
            }
            /////////
            BassNet.Registration("saorihara93@gmail.com", "2X2831021152222");
            ////////

            _knownTypeBinders = new KnownTypesBinder();
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Objects, SerializationBinder = _knownTypeBinders };
            SetupDebugLogging();
            SetupLoggingForProcessWideEvents();

            base.OnStartup(startupEvent);

            Log.Information($"adrilight {VersionNumber}: Main() started.");

            //show splash screen here to display wtfever you are loading

            //set style and color of the default theme

            //_splashScreen.WindowState = WindowState.Normal;
            // inject all, this task may takes long time
            _splashScreen = new SplashScreen();
            this.MainWindow = _splashScreen;
            _splashScreen.Show();
            _splashScreen.Header.Text = adrilight_shared.Properties.Resources.App_OnStartup_AdrilightIsLoading;
            _splashScreen.status.Text = adrilight_shared.Properties.Resources.App_OnStartup_LOADINGKERNEL;


            kernel = await Task.Run(() => SetupDependencyInjection(false));
            //start device service
            var deviceManager = kernel.Get<DeviceManager>();
            this.Resources["Locator"] = new ViewModelLocator(kernel);

            var dialogService = kernel.Get<IDialogService>();
            RegisterDialog(dialogService);

            //close splash screen and open dashboard
            this.Resources["Locator"] = new ViewModelLocator(kernel);
            _telemetryClient = kernel.Get<TelemetryClient>();


            // show main window
            OpenSettingsWindow(GeneralSettings.StartMinimized);

            SetupTrackingForProcessWideEvents(_telemetryClient);
            //kernel.Get<AdrilightUpdater>().StartThread();
        }
        private void RegisterDialog(IDialogService dialogService)
        {
            dialogService.RegisterDialog<DeleteDialog, DeleteDialogViewModel>();
            dialogService.RegisterDialog<RenameDialog, RenameDialogViewModel>();
            dialogService.RegisterDialog<AddNewDialog, AddNewDialogViewModel>();
            dialogService.RegisterDialog<NumberInputDialog, NumberInputDialogViewModel>();
            dialogService.RegisterDialog<ProgressDialog, ProgressDialogViewModel>();
            dialogService.RegisterDialog<AutomationDialogView, AutomationDialogViewModel>();
            dialogService.RegisterDialog<HotKeySelectionDialog, HotKeySelectionViewModel>();
            dialogService.RegisterDialog<DeviceSearchingScreen, DeviceSearchingDialogViewModel>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            _adrilightMutex?.Dispose();

            Log.CloseAndFlush();
        }
        private async Task<LangModel> SelectLanguage()
        {
            var langSelectDialog = new SelectLanguageWindow();
            var result = langSelectDialog.ShowDialog();
            if(result == true)
            {
                return await Task.FromResult(langSelectDialog.SelectedLanguage);
            }
            else
            {
                return null;
            }
        }
        protected void CloseMutexHandler(object sender, EventArgs startupEvent)
        {
            _mutex?.Close();
        }
        private DeviceHelpers DeviceHlprs { get; set; }
        private TelemetryClient _telemetryClient;
        private static View.SplashScreen _splashScreen;
        private static View.DeviceSearchingScreen _searchingForDeviceScreen;

        internal static IKernel SetupDependencyInjection(bool isInDesignMode)
        {
            var kernel = new StandardKernel(new DeviceSettingsInjectModule());
            GeneralSettings = kernel.Get<IGeneralSettings>();
            Thread.CurrentThread.CurrentUICulture = GeneralSettings.AppCulture.Culture;

            System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {
                Thread.CurrentThread.CurrentUICulture = GeneralSettings.AppCulture.Culture;
                _splashScreen.status.Text = "DONE LOADING KERNEL";
            });

            System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {
                Thread.CurrentThread.CurrentUICulture = GeneralSettings.AppCulture.Culture;
                _splashScreen.status.Text = adrilight_shared.Properties.Resources.App_SetupDependencyInjection_PROCESSESCREATED;
            });

            var dbManager = kernel.Get<DBmanager>();
            return kernel;
        }

        private void App_Activated(object sender, EventArgs e)
        {
            // Application activated
            //tell mainview that this app is being focused
           
        }

        private void App_Deactivated(object sender, EventArgs e)
        {
            // Application deactivated
            
        }


        private void SetupLoggingForProcessWideEvents()
        {
            if (DeviceHlprs == null)
                DeviceHlprs = new DeviceHelpers();
            AppDomain.CurrentDomain.UnhandledException +=
            (sender, args) => ApplicationWideException(sender, args.ExceptionObject as Exception, "CurrentDomain.UnhandledException");
            DispatcherUnhandledException += (sender, args) => ApplicationWideException(sender, args.Exception, "DispatcherUnhandledException");
            Exit += (s, e) =>
            {
                var devices = kernel.GetAll<IDeviceSettings>();
                foreach (var device in devices)
                {
                    lock (device)
                        DeviceHlprs.WriteSingleDeviceInfoJson(device);
                }
                //execute shutdown automation

                var hwMonitor = kernel.GetAll<HWMonitor>().FirstOrDefault();
                var ambinityClient = kernel.GetAll<AmbinityClient>().FirstOrDefault();
                var deviceDiscovery = kernel.GetAll<DeviceDiscovery>().FirstOrDefault();
                //dbmanager delete file
                Thread.Sleep(2000);
                deviceDiscovery.Stop();
                GeneralSettings.IsOpenRGBEnabled = false;
                hwMonitor.Dispose();
                ambinityClient.Dispose();
                Log.Information("Application exit!");
            };

            SystemEvents.PowerModeChanged += async (s, e) =>
            {
                Log.Information("Changing Powermode to {0}", e.Mode);
                if (e.Mode == PowerModes.Resume)
                {
                    GC.Collect();

                    var devices = kernel.GetAll<IDeviceSettings>();
                    foreach (var device in devices)
                    {
                        device.TurnOnLED();
                        Thread.Sleep(10);
                    }
                    Log.Information("System resume!");
                    var desktopFrame = kernel.GetAll<ICaptureEngine>().Where(c => c is DesktopFrame).FirstOrDefault();
                    if (desktopFrame != null)
                    {
                        int count = 0;
                        foreach (var monitor in MonitorEnumerationHelper.GetMonitors())
                        {
                            await (desktopFrame as DesktopFrame).StartHmonCapture(monitor, count++);
                        }
                    }
                    var deviceDiscovery = kernel.GetAll<DeviceDiscovery>().FirstOrDefault();
                    deviceDiscovery.Start();

                }
                else if (e.Mode == PowerModes.Suspend)
                {

                    //foreach (var openRGBStream in kernel.GetAll<ISerialStream>().Where(s => s is OpenRGBStream))
                    //{
                    //    openRGBStream.Stop();
                    //}
                    var devices = kernel.GetAll<IDeviceSettings>();
                    var deviceDiscovery = kernel.GetAll<DeviceDiscovery>().FirstOrDefault();
                    deviceDiscovery.Stop();
                    foreach (var device in devices)
                    {
                        device.IsTransferActive = false;
                    }
                    var ambinityClient = kernel.GetAll<AmbinityClient>().FirstOrDefault();
                    ambinityClient.Dispose();
                    Log.Information("System suspended!");
                }
            };
            SystemEvents.SessionEnding += (s, e) =>
            {
                //execute shutdown automation
                Log.Information("Stop the serial stream due to power down or log off condition!");
            };
        }
        private void SetupTrackingForProcessWideEvents(TelemetryClient tc)
        {
            if (tc == null)
            {
                throw new ArgumentNullException(nameof(tc));
            }

            AppDomain.CurrentDomain.UnhandledException += (sender, args) => tc.TrackException(args.ExceptionObject as Exception);

            DispatcherUnhandledException += (sender, args) => tc.TrackException(args.Exception);

            Exit += (s, e) => { tc.TrackEvent("AppExit"); tc.Flush(); };

            SystemEvents.PowerModeChanged += (s, e) => tc.TrackEvent("PowerModeChanged", new Dictionary<string, string> { { "Mode", e.Mode.ToString() } });
            tc.TrackEvent("AppStart");
        }
        private void SetupDebugLogging()
        {
            var logPath = Path.Combine(JsonPath, "Logs");
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.RollingFile(Path.Combine(logPath, "adrilight-{Date}.txt"), retainedFileCountLimit: 10, shared: true, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            Log.Information($"DEBUG logging set up!");
        }

        private static IKernel kernel;

        private void OpenSettingsWindow(bool isMinimized)
        {
            var _mainForm = new MainView();
            this.MainWindow = _mainForm;
            //bring to front?
            if (!isMinimized)
            {
                _mainForm.Visibility = Visibility.Visible;
                _mainForm.Show();
                Log.Information("Open Dashboard");
            }
            else
            {
                Log.Information("Windows Start Minimized");
            }
            _splashScreen.Close();
        }

        public static string VersionNumber { get; } = Assembly.GetExecutingAssembly().GetName().Version.ToString(3);

        private static IGeneralSettings GeneralSettings { get; set; }

        private void ApplicationWideException(object sender, Exception ex, string eventSource)
        {
            Log.Fatal(ex, $"ApplicationWideException from sender={sender}, adrilight version={VersionNumber}, eventSource={eventSource}");

            var sb = new StringBuilder();
            sb.AppendLine($"Sender: {sender}");
            sb.AppendLine($"Source: {eventSource}");
            if (sender != null)
            {
                sb.AppendLine($"Sender Type: {sender.GetType().FullName}");
            }
            sb.AppendLine("-------");
            do
            {
                sb.AppendLine($"exception type: {ex.GetType().FullName}");
                sb.AppendLine($"exception message: {ex.Message}");
                sb.AppendLine($"exception stacktrace: {ex.StackTrace}");
                sb.AppendLine("-------");
                ex = ex.InnerException;
            } while (ex != null);

            HandyControl.Controls.MessageBox.Show(sb.ToString(), "unhandled exception :-(");
            try
            {
                Shutdown(-1);
            }
            catch
            {
                Environment.Exit(-1);
            }
        }
    }
}