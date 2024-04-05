using adrilight.Manager;
using adrilight.Ninject;
using adrilight.Services.CaptureEngine;
using adrilight.Services.CaptureEngine.ScreenCapture;
using adrilight.Services.DeviceDiscoveryServices;
using adrilight.Services.LightingEngine;
using adrilight.Services.OpenRGBService;
using adrilight.Services.SerialStream;
using adrilight.Ticker;
using adrilight.Util;
using adrilight.View;
using adrilight.ViewModel;
using adrilight_shared.Enums;
using adrilight_shared.Helpers;
using adrilight_shared.Models.ControlMode.Mode;
using adrilight_shared.Models.Device;
using adrilight_shared.Models.Device.Zone;
using adrilight_shared.Models.Language;
using adrilight_shared.Services;
using adrilight_shared.Settings;
using adrilight_shared.View.Dialogs;
using adrilight_shared.ViewModel;
using HandyControl.Themes;
using HandyControl.Tools.Extension;
using Microsoft.ApplicationInsights;
using Microsoft.Win32;
using Newtonsoft.Json;
using Ninject;
using Ninject.Extensions.Conventions;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
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
            //check if this app is already open in the background
            var settingsManager = new UserSettingsManager();
            var generalSettings = settingsManager.LoadIfExists() ?? settingsManager.MigrateOrDefault();
            Thread.CurrentThread.CurrentCulture = generalSettings.AppCulture.Culture;
            Thread.CurrentThread.CurrentUICulture = generalSettings.AppCulture.Culture;
            CultureInfo.DefaultThreadCurrentCulture = generalSettings.AppCulture.Culture;
            CultureInfo.DefaultThreadCurrentUICulture = generalSettings.AppCulture.Culture;
            ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
            var accentColor = generalSettings.AccentColor;
            ThemeManager.Current.AccentColor = new SolidColorBrush(accentColor);
            _adrilightMutex = new Mutex(true, "adrilight2");
            if (!_adrilightMutex.WaitOne(TimeSpan.Zero, true))
            {
                //another instance is already running!
                HandyControl.Controls.MessageBox.Show(adrilight_shared.Properties.Resources.App_Already_Launch
                    , adrilight_shared.Properties.Resources.App_Already_Launch_Header);
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
            _splashScreen.Header.Text = "Adrilight is loading";
            _splashScreen.status.Text = "LOADING KERNEL...";
            kernel = await Task.Run(() => SetupDependencyInjection(false));
            var dialogService = kernel.Get<IDialogService>();
            dialogService.RegisterDialog<DeleteDialog, DeleteDialogViewModel>();
            dialogService.RegisterDialog<RenameDialog, RenameDialogViewModel>();
            dialogService.RegisterDialog<AddNewDialog, AddNewDialogViewModel>();
            dialogService.RegisterDialog<NumberInputDialog, NumberInputDialogViewModel>();
            dialogService.RegisterDialog<ProgressDialog, ProgressDialogViewModel>();
            //close splash screen and open dashboard
            this.Resources["Locator"] = new ViewModelLocator(kernel);
            _telemetryClient = kernel.Get<TelemetryClient>();


            // show main window
            OpenSettingsWindow(GeneralSettings.StartMinimized);

            SetupTrackingForProcessWideEvents(_telemetryClient);
            kernel.Get<AdrilightUpdater>().StartThread();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            _adrilightMutex?.Dispose();

            Log.CloseAndFlush();
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
            //Load setting từ file Json//
            var deviceManager = kernel.Get<DeviceManagerViewModel>();
            var captureEngines = kernel.GetAll<ICaptureEngine>();
            var rainbowTicker = kernel.Get<RainbowTicker>();
            var playlistDecoder = kernel.Get<PlaylistDecoder>();
            var hwMonitor = kernel.Get<HWMonitor>();

            System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {
                _splashScreen.status.Text = "DONE LOADING KERNEL";
            });

            System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {
                _splashScreen.status.Text = "PROCESSES CREATED";
            });
            MainViewViewModel = kernel.Get<MainViewViewModel>();
            if (!GeneralSettings.StartMinimized)
                MainViewViewModel.IsAppActivated = true;
            MainViewViewModel.AvailableDevices.CollectionChanged += (s, e) =>
            {
                switch (e.Action)
                {
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                        var newDevices = e.NewItems;
                        // create new IDeviceSettings with new Name
                        //Get ID:
                        foreach (IDeviceSettings device in newDevices)
                        {
                            var iD = device.DeviceUID.ToString();
                            kernel.Bind<IDeviceSettings>().ToConstant(device).Named(iD);
                            //now inject
                            InjectingDevice(kernel, device);
                            device.DeviceEnableChanged();
                            var playlistDecoder = kernel.Get<PlaylistDecoder>();
                            playlistDecoder.AvailableDevices.Add(device);
                            //since openRGBStream is single instance, we need to manually add device then refresh
                        }
                        break;

                    case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                        var removedDevice = e.OldItems;
                        foreach (IDeviceSettings device in removedDevice) // when an item got removed, simply stop dependencies service from running
                        {
                            UnInjectingDevice(kernel, device);
                        }
                        break;

                    case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                        ResetDevicesBinding(kernel);
                        break;
                }
            };
            if (deviceManager.AvailableDevices != null)
            {
                foreach (var device in deviceManager.AvailableDevices.Items)
                {
                    lock (MainViewViewModel.AvailableDeviceLock)
                    {
                        MainViewViewModel.AvailableDevices.Insert(0, device as DeviceSettings);
                    }
                }
            }
            var deviceDiscovery = kernel.Get<DeviceDiscovery>();
            var dbManager = kernel.Get<DBmanager>();
            return kernel;
        }

        private void App_Activated(object sender, EventArgs e)
        {
            // Application activated
            //tell mainview that this app is being focused
            if (MainViewViewModel != null)
                MainViewViewModel.IsAppActivated = true;
        }

        private void App_Deactivated(object sender, EventArgs e)
        {
            // Application deactivated
            if (MainViewViewModel != null)
                MainViewViewModel.IsAppActivated = false;
        }

        private static void InjectingZone(IKernel kernel, IControlZone zone, IDeviceSettings device)
        {
            kernel.Bind<ILightingEngine>().To<DesktopDuplicatorReader>().InSingletonScope().Named(zone.ZoneUID).WithConstructorArgument("zone", kernel.Get<IControlZone>(zone.ZoneUID));
            kernel.Bind<ILightingEngine>().To<StaticColor>().InSingletonScope().Named(zone.ZoneUID).WithConstructorArgument("zone", kernel.Get<IControlZone>(zone.ZoneUID));
            kernel.Bind<ILightingEngine>().To<Rainbow>().InSingletonScope().Named(zone.ZoneUID).WithConstructorArgument("zone", kernel.Get<IControlZone>(zone.ZoneUID)).WithConstructorArgument("device", device);
            kernel.Bind<ILightingEngine>().To<Animation>().InSingletonScope().Named(zone.ZoneUID).WithConstructorArgument("zone", kernel.Get<IControlZone>(zone.ZoneUID)).WithConstructorArgument("device", device);
            kernel.Bind<ILightingEngine>().To<Music>().InSingletonScope().Named(zone.ZoneUID).WithConstructorArgument("zone", kernel.Get<IControlZone>(zone.ZoneUID));
            kernel.Bind<ILightingEngine>().To<Gifxelation>().InSingletonScope().Named(zone.ZoneUID).WithConstructorArgument("zone", kernel.Get<IControlZone>(zone.ZoneUID));
            var availableLightingModes = kernel.GetAll<ILightingEngine>(zone.ZoneUID);
            if (zone.CurrentActiveControlMode == null)
                zone.CurrentActiveControlMode = zone.AvailableControlMode.FirstOrDefault();
            foreach (var lightingMode in availableLightingModes)
            {
                if (lightingMode.Type == (zone.CurrentActiveControlMode as LightingMode).BasedOn)
                    lightingMode.Refresh();
            }
        }

        private static void ResetDevicesBinding(IKernel kernel)
        {
            var lightingEngineServices = kernel.GetAll<ILightingEngine>();
            foreach (var lightingEngine in lightingEngineServices)
            {
                if (lightingEngine.IsRunning)
                    lightingEngine.Stop();
            }
            var serialStreams = kernel.GetAll<ISerialStream>();
            serialStreams.ForEach(s => s.Stop());
            kernel.Unbind<ILightingEngine>();
            kernel.Unbind<IDeviceSettings>();
            kernel.Unbind<IControlZone>();
            kernel.Unbind<ISerialStream>();
        }

        private static void UnInjectingZone(IKernel kernel, IControlZone zone)
        {
            var availableLightingModes = kernel.GetAll<ILightingEngine>(zone.ZoneUID);
            foreach (var lightingMode in availableLightingModes)
            {
                if (lightingMode.IsRunning)
                    lightingMode.Stop();
            }
        }

        private static void UnInjectingDevice(IKernel kernel, IDeviceSettings device)
        {
            foreach (var output in device.AvailableLightingOutputs)
            {
                foreach (var zone in output.SlaveDevice.ControlableZones)
                {
                    UnInjectingZone(kernel, zone as IControlZone);
                }
            }
        }

        private static void InjectingDevice(IKernel kernel, IDeviceSettings device)
        {
            foreach (var output in device.AvailableLightingOutputs)
            {
                //catch collection changed to inject
                output.SlaveDevice.ControlableZones.CollectionChanged += (s, e) =>
                {
                    switch (e.Action)
                    {
                        case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                            var newZone = e.NewItems;
                            foreach (var zone in newZone)
                            {
                                //new zone added, inject
                                var ledZone = zone as LEDSetup; ;
                                kernel.Bind<IControlZone>().ToConstant(ledZone).Named((ledZone).ZoneUID);
                                InjectingZone(kernel, zone as IControlZone, device);
                            }
                            break;
                    }
                };
                //slave device got replaced, injecting all new zone that available in this slave device
                output.PropertyChanged += (_, __) =>
                {
                    switch (__.PropertyName)
                    {
                        case nameof(output.SlaveDevice):
                            foreach (var zone in output.SlaveDevice.ControlableZones)
                            {
                                var ledZone = zone as LEDSetup;
                                kernel.Bind<IControlZone>().ToConstant(zone as IControlZone).Named((zone as IControlZone).ZoneUID);
                                InjectingZone(kernel, zone as IControlZone, device);
                            }
                            output.SlaveDevice.ControlableZones.CollectionChanged += (s, e) =>
                            {
                                switch (e.Action)
                                {
                                    case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                                        var newZone = e.NewItems;
                                        foreach (var zone in newZone)
                                        {
                                            //new zone added, inject
                                            var ledZone = zone as LEDSetup; ;
                                            kernel.Bind<IControlZone>().ToConstant(ledZone).Named((ledZone).ZoneUID);
                                            InjectingZone(kernel, zone as IControlZone, device);
                                        }
                                        break;
                                }
                            };
                            break;
                    }
                };
                //this is existed zone
                foreach (var zone in output.SlaveDevice.ControlableZones)
                {
                    var ledZone = zone as LEDSetup;
                    kernel.Bind<IControlZone>().ToConstant(ledZone).Named((ledZone).ZoneUID);
                    InjectingZone(kernel, zone as IControlZone, device);
                }
            }
            var connectionType = device.DeviceType.ConnectionTypeEnum;
            switch (connectionType)
            {
                case DeviceConnectionTypeEnum.Wired:
                        kernel.Bind<ISerialStream>().To<SerialStream>().InSingletonScope().Named(device.DeviceUID.ToString()).WithConstructorArgument("deviceSettings", kernel.Get<IDeviceSettings>(device.DeviceUID.ToString()));
                    break;

                case DeviceConnectionTypeEnum.Wireless:

                    break;

                case DeviceConnectionTypeEnum.OpenRGB:
                    kernel.Bind<ISerialStream>().To<OpenRGBStream>().InSingletonScope().Named(device.DeviceUID.ToString()).WithConstructorArgument("deviceSettings", kernel.Get<IDeviceSettings>(device.DeviceUID.ToString()));
                    break;
            }
            var serialStream = kernel.Get<ISerialStream>(device.DeviceUID.ToString());
            //serialStream.RefreshTransferState();
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
                MainViewViewModel.ExecuteShudownAutomationActions();

                var hwMonitor = kernel.GetAll<HWMonitor>().FirstOrDefault();
                var ambinityClient = kernel.GetAll<IAmbinityClient>().FirstOrDefault();
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
                    var ambinityClient = kernel.GetAll<IAmbinityClient>().FirstOrDefault();
                    ambinityClient.Dispose();
                    Log.Information("System suspended!");
                }
            };
            SystemEvents.SessionEnding += (s, e) =>
            {
                MainViewViewModel.ExecuteShudownAutomationActions();
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
        public static MainViewViewModel MainViewViewModel { get; set; }

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