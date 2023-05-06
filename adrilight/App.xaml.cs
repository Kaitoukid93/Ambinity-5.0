
using adrilight.View;
using adrilight.ViewModel;
using Microsoft.Win32;
using Ninject;
using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using Ninject.Extensions.Conventions;
using adrilight.Resources;
using adrilight.Util;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.DependencyInjection;
using adrilight.Ninject;
using adrilight.Spots;
using System.Diagnostics;
using System.Windows.Forms;
using System.Windows.Controls;
using GalaSoft.MvvmLight;
using System.Threading;
using Un4seen.BassWasapi;
using Un4seen.Bass;
using System.Windows.Media;
using HandyControl.Themes;
using System.Threading.Tasks;
using adrilight.Settings;
using System.Drawing;

namespace adrilight
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// 
    //structures to display bitmap image after getting data from shader



    public sealed partial class App : System.Windows.Application
    {
        private static readonly ILogger _log = LogManager.GetCurrentClassLogger();
        private static System.Threading.Mutex _mutex = null;
        private static Mutex _adrilightMutex;
        protected override async void OnStartup(StartupEventArgs startupEvent)
        {



            //check if this app is already open in the background
            _adrilightMutex = new Mutex(true, "adrilight2");
            if (!_adrilightMutex.WaitOne(TimeSpan.Zero, true))
            {
                //another instance is already running!
                HandyControl.Controls.MessageBox.Show("Adrilight đã được khởi chạy trước đó rồi, vui lòng kiểm tra Task Manager hoặc System Tray Icon"
                    , "App đã được khởi chạy!");
                Shutdown();
                return;
            }
            /////////
            SetupDebugLogging();
            SetupLoggingForProcessWideEvents();

            base.OnStartup(startupEvent);

            _log.Debug($"adrilight {VersionNumber}: Main() started.");

            //show splash screen here to display wtfever you are loading

            //set style and color of the default theme
            ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
            ThemeManager.Current.AccentColor = System.Windows.Media.Brushes.BlueViolet;
            _splashScreen = new View.SplashScreen();
            //_splashScreen.WindowState = WindowState.Minimized;
            _splashScreen.Header.Text = "Adrilight is loading";
            _splashScreen.status.Text = "LOADING KERNEL...";
            _splashScreen.Show();
            //_splashScreen.WindowState = WindowState.Normal;

            // inject all, this task may takes long time
            kernel = await Task.Run(() => SetupDependencyInjection(false));
            //close splash screen and open dashboard
            _splashScreen.Close();

            this.Resources["Locator"] = new ViewModelLocator(kernel);

            _telemetryClient = kernel.Get<TelemetryClient>();

            ///set theme and color
            if (GeneralSettings.ThemeIndex == 1)
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
            }
            else
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
            }

            var accentColor = GeneralSettings.AccentColor;
            ThemeManager.Current.AccentColor = new SolidColorBrush(accentColor);

            // show main window
            OpenSettingsWindow(GeneralSettings.StartMinimized);

            SetupTrackingForProcessWideEvents(_telemetryClient);
            kernel.Get<AdrilightUpdater>().StartThread();

        }


        protected override void OnExit(ExitEventArgs e)
        {


            base.OnExit(e);
            _adrilightMutex?.Dispose();

            LogManager.Shutdown();
        }
        protected void CloseMutexHandler(object sender, EventArgs startupEvent)
        {
            _mutex?.Close();
        }
        private TelemetryClient _telemetryClient;

        private static TelemetryClient SetupApplicationInsights(IDeviceSettings settings)
        {
            const string ik = "65086b50-8c52-4b13-9b05-92fbe69c7a52";
            TelemetryConfiguration.Active.InstrumentationKey = ik;
            var tc = new TelemetryClient {
                InstrumentationKey = ik
            };

            tc.Context.User.Id = "1234";//settings.InstallationId.ToString();
            tc.Context.Session.Id = Guid.NewGuid().ToString();
            tc.Context.Device.OperatingSystem = Environment.OSVersion.ToString();

            GlobalDiagnosticsContext.Set("user_id", tc.Context.User.Id);
            GlobalDiagnosticsContext.Set("session_id", tc.Context.Session.Id);
            return tc;
        }

        private static View.SplashScreen _splashScreen;


        internal static IKernel SetupDependencyInjection(bool isInDesignMode)
        {

            var kernel = new StandardKernel(new DeviceSettingsInjectModule());
            GeneralSettings = kernel.Get<IGeneralSettings>();
            //Load setting từ file Json//
            var settingsManager = new UserSettingsManager();
            kernel.Bind(x => x.FromThisAssembly()
              .SelectAllClasses()
              .BindAllInterfaces());
            var serialDeviceDetection = kernel.Get<ISerialDeviceDetection>();
            var captureEngines = kernel.GetAll<ICaptureEngine>();
            var rainbowTicker = kernel.Get<IRainbowTicker>();
            var hwMonitor = kernel.Get<IHWMonitor>();
            



            System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {
                _splashScreen.status.Text = "DONE LOADING KERNEL";
            });

            System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {
                _splashScreen.status.Text = "PROCESSES CREATED";
            });
            var deviceDiscovery = kernel.Get<IDeviceDiscovery>();

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
                            //since openRGBStream is single instance, we need to manually add device then refresh                                        
                        }

                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                        var removedDevice = e.OldItems;
                        foreach (IDeviceSettings device in removedDevice)
                        {
                            var iD = device.DeviceUID.ToString();
                            //stop serialstream
                            var serialStream = kernel.Get<ISerialStream>(iD);
                            if (serialStream != null)
                                serialStream.Stop();
                        }
                        break;

                }
            };
            if(settingsManager.LoadDeviceIfExists() is not null)
            {
                foreach (var device in settingsManager.LoadDeviceIfExists())
                {
                    MainViewViewModel.AvailableDevices.Insert(0, device);
                }
            }

            return kernel;

        }
        void App_Activated(object sender, EventArgs e)
        {
            // Application activated
            //tell mainview that this app is being focused
            if(MainViewViewModel!=null)
            MainViewViewModel.IsAppActivated = true;
        }

        void App_Deactivated(object sender, EventArgs e)
        {
            // Application deactivated
            if (MainViewViewModel != null)
                MainViewViewModel.IsAppActivated = false;
        }
        private static void InjectingZone(IKernel kernel, IControlZone zone)
        {
            kernel.Bind<ILightingEngine>().To<DesktopDuplicatorReader>().InSingletonScope().Named(zone.ZoneUID).WithConstructorArgument("zone", kernel.Get<IControlZone>(zone.ZoneUID));
            kernel.Bind<ILightingEngine>().To<StaticColor>().InSingletonScope().Named(zone.ZoneUID).WithConstructorArgument("zone", kernel.Get<IControlZone>(zone.ZoneUID));
            kernel.Bind<ILightingEngine>().To<Rainbow>().InSingletonScope().Named(zone.ZoneUID).WithConstructorArgument("zone", kernel.Get<IControlZone>(zone.ZoneUID));
            kernel.Bind<ILightingEngine>().To<Animation>().InSingletonScope().Named(zone.ZoneUID).WithConstructorArgument("zone", kernel.Get<IControlZone>(zone.ZoneUID));
            kernel.Bind<ILightingEngine>().To<Music>().InSingletonScope().Named(zone.ZoneUID).WithConstructorArgument("zone", kernel.Get<IControlZone>(zone.ZoneUID));
            var availableLightingModes = kernel.GetAll<ILightingEngine>(zone.ZoneUID);
            foreach(var lightingMode in availableLightingModes )
            {
                lightingMode.Refresh();
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
                                InjectingZone(kernel, zone as IControlZone);
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
                                InjectingZone(kernel, zone as IControlZone);
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
                                            InjectingZone(kernel, zone as IControlZone);
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
                    InjectingZone(kernel, zone as IControlZone);
                }
            }
            var connectionType = device.DeviceConnectionType;

            switch (connectionType)
            {
                case "wired":
                    kernel.Bind<ISerialStream>().To<SerialStream>().InSingletonScope().Named(device.DeviceUID.ToString()).WithConstructorArgument("deviceSettings", kernel.Get<IDeviceSettings>(device.DeviceUID.ToString()));

                    break;
                case "wireless":
                 //   kernel.Bind<ISerialStream>().To<NetworkStream>().InSingletonScope().Named(iD).WithConstructorArgument("deviceSettings", kernel.Get<IDeviceSettings>(iD));

                    break;
                case "OpenRGB":
                 //   kernel.Bind<ISerialStream>().To<OpenRGBStream>().InSingletonScope().Named(iD).WithConstructorArgument("deviceSettings", kernel.Get<IDeviceSettings>(iD));
                    break;

            }
            var serialStream = kernel.Get<ISerialStream>(device.DeviceUID.ToString());
            //serialStream.RefreshTransferState();
        }
        private void ScreenSetupChanged()
        {
            foreach (var desktopDuplicatorReader in kernel.GetAll<ILightingEngine>())
            {
                desktopDuplicatorReader.Stop();
            }
            foreach (var desktopFrame in kernel.GetAll<ICaptureEngine>())
            {
                desktopFrame.Stop();
            }

            //find out what happend
            if (Screen.AllScreens.Length < kernel.GetAll<ICaptureEngine>().Count())
            {
                //screen unpluged
                foreach (var screen in Screen.AllScreens)
                {
                    var desktopFrame = kernel.Get<ICaptureEngine>(screen.DeviceName);
                    if (desktopFrame != null) // this screen is injected already, simply restart it
                        desktopFrame.RefreshCapturingState();


                }

            }
            else if (Screen.AllScreens.Length == kernel.GetAll<ICaptureEngine>().Count())
            {
                //res change. handled in desktopframe
                foreach (var desktopFrame in kernel.GetAll<ICaptureEngine>())
                {
                    desktopFrame.RefreshCapturingState();
                }
            }
            else
            {
                //screen attached
                foreach (var screen in Screen.AllScreens)
                {
                    var desktopFrames = kernel.GetAll<ICaptureEngine>();
                    foreach (var desktopFrame in desktopFrames)
                    {
                        if (desktopFrame.DeviceName == screen.DeviceName)
                            desktopFrame.RefreshCapturingState();
                        else
                        {
                            kernel.Bind<ICaptureEngine>().To<DesktopFrame>().InSingletonScope().Named(screen.DeviceName).WithConstructorArgument("screen", screen.DeviceName);
                            var newDesktopFrame = kernel.Get<ICaptureEngine>(screen.DeviceName);
                            // newDesktopFrame.RefreshCapturingState();
                        }
                    }



                }
            }



            //restart process

            foreach (var desktopDuplicatorReader in kernel.GetAll<ILightingEngine>())
            {
                desktopDuplicatorReader.Refresh();
            }
        }
        private void SetupLoggingForProcessWideEvents()
        {
            AppDomain.CurrentDomain.UnhandledException +=
    (sender, args) => ApplicationWideException(sender, args.ExceptionObject as Exception, "CurrentDomain.UnhandledException");

            DispatcherUnhandledException += (sender, args) => ApplicationWideException(sender, args.Exception, "DispatcherUnhandledException");

            // var desktopduplicators = kernel.GetAll<IDesktopDuplicatorReader>();
            Exit += (s, e) =>
            {
                var devices = kernel.GetAll<IDeviceSettings>();
                var hwMonitor = kernel.GetAll<IHWMonitor>().FirstOrDefault();
                var ambinityClient = kernel.GetAll<IAmbinityClient>().FirstOrDefault();
                var deviceDiscovery = kernel.GetAll<IDeviceDiscovery>().FirstOrDefault();
                deviceDiscovery.Stop();
                foreach (var device in devices)
                {
                    device.DeviceState = DeviceStateEnum.Sleep;
                    Thread.Sleep(10);
                    device.IsTransferActive = false;
                    MainViewViewModel.WriteSingleDeviceInfoJson(device);
                    /*Thread.Sleep(1000);*/ //wait for serialstream to finising sending
                    //serialStream.Stop();
                }
                //dispose hwmonitor to prevent file lock
                hwMonitor.Dispose();
                ambinityClient.Dispose();



                _log.Debug("Application exit!");
            };

            SystemEvents.DisplaySettingsChanged += (s, e) =>
            {
                ScreenSetupChanged();
            };
            SystemEvents.PowerModeChanged += (s, e) =>
        {
            _log.Debug("Changing Powermode to {0}", e.Mode);
            if (e.Mode == PowerModes.Resume)
            {
                GC.Collect();
                var devices = kernel.GetAll<IDeviceSettings>();
                // var deviceDiscovery = kernel.GetAll<IDeviceDiscovery>().FirstOrDefault();
                foreach (var device in devices)
                {
                    device.DeviceState = DeviceStateEnum.Normal;
                }
                // deviceDiscovery.enable = true;
                var desktopFrames = kernel.GetAll<ICaptureEngine>();
                foreach (var desktopFrame in desktopFrames)
                {
                    desktopFrame.RefreshCapturingState(); 

                }
                //var desktopFrame = kernel.Get<IDesktopFrame>();
                //var secondDesktopFrame = kernel.Get<ISecondDesktopFrame>();
                //var thirdDesktopFrame = kernel.Get<IThirdDesktopFrame>();
                //desktopFrame.RefreshCapturingState();
                //secondDesktopFrame.RefreshCapturingState();
                //thirdDesktopFrame.RefreshCapturingState();



                _log.Debug("Restart the serial stream after sleep!");
            }
            else if (e.Mode == PowerModes.Suspend)
            {
                var devices = kernel.GetAll<IDeviceSettings>();
                //var deviceDiscovery = kernel.GetAll<IDeviceDiscovery>().FirstOrDefault();
                // deviceDiscovery.enable = false;
                foreach (var device in devices)
                {
                    device.DeviceState = DeviceStateEnum.Sleep;
                    //Thread.Sleep(1000);
                    //serialStream.Stop();
                }


                var desktopFrames = kernel.GetAll<ICaptureEngine>();
                foreach (var desktopFrame in desktopFrames)
                {
                    desktopFrame.Stop();
                }
                //var secondDesktopFrame = kernel.Get<ISecondDesktopFrame>();
                //var thirdDesktopFrame = kernel.Get<IThirdDesktopFrame>();
                //desktopFrame.Stop();
                //secondDesktopFrame.Stop();
                //thirdDesktopFrame.Stop();
                _log.Debug("Stop the serial stream due to sleep condition!");
            }

        };
            SystemEvents.SessionEnding += (s, e) =>
            {
                var devices = kernel.GetAll<IDeviceSettings>();
                // var deviceDiscovery = kernel.GetAll<IDeviceDiscovery>().FirstOrDefault();
                // deviceDiscovery.enable = false;
                foreach (var device in devices)
                {
                    device.DeviceState = DeviceStateEnum.Sleep;
                    Thread.Sleep(1000);
                    //serialStream.Stop();
                }

                _log.Debug("Stop the serial stream due to power down or log off condition!");
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

        [System.Diagnostics.Conditional("DEBUG")]
        private void SetupDebugLogging()
        {
            var config = new LoggingConfiguration();
            var debuggerTarget = new DebuggerTarget() { Layout = "${processtime} ${message:exceptionSeparator=\n\t:withException=true}" };
            config.AddTarget("debugger", debuggerTarget);
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, debuggerTarget));

            LogManager.Configuration = config;

            _log.Info($"DEBUG logging set up!");
        }


        private IKernel kernel;
        public static MainViewViewModel MainViewViewModel { get; set; }

        private void OpenSettingsWindow(bool isMinimized)
        {
            var _mainForm = kernel.Get<MainView>();

            //bring to front?
            if (!isMinimized)
            {
                _mainForm.Visibility = Visibility.Visible;
                _mainForm.Show();
            }
            else
            {
                //_mainForm.Visibility = Visibility.Collapsed;

            }


        }

        //private void MainForm_FormClosed(object sender, EventArgs e)
        //{
        //    if (_mainForm == null) return;

        //    //deregister to avoid memory leak
        //    _mainForm.Closed -= MainForm_FormClosed;
        //    _mainForm = null;
        //}



        public static string VersionNumber { get; } = Assembly.GetExecutingAssembly().GetName().Version.ToString(3);

        private static IGeneralSettings GeneralSettings { get; set; }


        private void ApplicationWideException(object sender, Exception ex, string eventSource)
        {
            _log.Fatal(ex, $"ApplicationWideException from sender={sender}, adrilight version={VersionNumber}, eventSource={eventSource}");

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
