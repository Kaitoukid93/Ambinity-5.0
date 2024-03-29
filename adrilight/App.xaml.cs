﻿
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

namespace adrilight
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// 
  
   

    public sealed partial class App : System.Windows.Application
    {
        private static readonly ILogger _log = LogManager.GetCurrentClassLogger();
        private static System.Threading.Mutex _mutex = null;
        private static Mutex _adrilightMutex;
        protected override void OnStartup(StartupEventArgs startupEvent)
        {




            _adrilightMutex = new Mutex(true, "adrilight2");
            if (!_adrilightMutex.WaitOne(TimeSpan.Zero, true))
            {
                //another instance is already running!
                HandyControl.Controls.MessageBox.Show("Adrilight đã được khởi chạy trước đó rồi, vui lòng kiểm tra Task Manager hoặc System Tray Icon"
                    , "App đã được khởi chạy!");
                Shutdown();
                return;
            }

            SetupDebugLogging();
            SetupLoggingForProcessWideEvents();

            base.OnStartup(startupEvent);

            _log.Debug($"adrilight {VersionNumber}: Main() started.");


            kernel = SetupDependencyInjection(false);



            this.Resources["Locator"] = new ViewModelLocator(kernel);


            GeneralSettings = kernel.Get<IGeneralSettings>();
            _telemetryClient = kernel.Get<TelemetryClient>();






            if (GeneralSettings.ThemeIndex == 1)
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
            }
            else
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
            }


            ThemeManager.Current.AccentColor = GeneralSettings.AccentColor;
            // Current.MainWindow = kernel.Get<MainView>();
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



        internal static IKernel SetupDependencyInjection(bool isInDesignMode)
        {

            var kernel = new StandardKernel(new DeviceSettingsInjectModule());
            kernel.Bind<MainViewViewModel>().ToSelf().InSingletonScope();
            kernel.Bind<MainView>().ToSelf().InSingletonScope();
            //Load setting từ file Json//
            var settingsManager = new UserSettingsManager();
            var existedDevice = settingsManager.LoadDeviceIfExists();

            kernel.Bind(x => x.FromThisAssembly()
              .SelectAllClasses()
              .BindAllInterfaces());
            //var desktopDuplicationReader = kernel.Get<IDesktopDuplicatorReader>();
            //var desktopDuplicationReaderSecondary = kernel.Get<IDesktopDuplicatorReaderSecondary>();
            //var desktopDuplicationReaderThird = kernel.Get<IDesktopDuplicatorReaderThird>();
            // var openRGBClient = kernel.Get<IOpenRGBClientDevice>();
            var serialDeviceDetection = kernel.Get<ISerialDeviceDetection>();
            //var shaderEffect = kernel.Get<IShaderEffect>();
            var context = kernel.Get<IContext>();
            var desktopFrame = kernel.GetAll<IDesktopFrame>();


            //var hotKeyManager = kernel.Get<IHotKeyManager>();
            kernel.Bind<IOpenRGBStream>().To<OpenRGBStream>().InSingletonScope();
            var openRGBStream = kernel.Get<IOpenRGBStream>();
            var rainbowTicker = kernel.Get<IRainbowTicker>();
            var hwMonitor = kernel.Get<IHWMonitor>();

            var audioFrame = kernel.Get<IAudioFrame>();
            //// tách riêng từng setting của từng device///
            if (existedDevice != null)
            {
                foreach (var device in existedDevice)
                {




                    
                    var iD = device.DeviceUID.ToString();
                    var outputs = device.AvailableOutputs.ToList();
                    if (device.UnionOutput != null)
                        outputs.Add(device.UnionOutput);
                    var connectionType = device.DeviceConnectionType;
                    if (connectionType != "OpenRGB")
                    {
                        switch (connectionType)
                        {
                            case "wired":
                                kernel.Bind<ISerialStream>().To<SerialStream>().InSingletonScope().Named(iD).WithConstructorArgument("deviceSettings", kernel.Get<IDeviceSettings>(iD));

                                break;
                            case "wireless":
                                kernel.Bind<ISerialStream>().To<NetworkStream>().InSingletonScope().Named(iD).WithConstructorArgument("deviceSettings", kernel.Get<IDeviceSettings>(iD));

                                break;

                        }
                        var serialStream = kernel.Get<ISerialStream>(iD);
                    }


                    foreach (var output in outputs)
                    {

                        var outputID = iD + output.OutputID.ToString();
                        //kernel.Bind<IStaticColor>().To<StaticColor>().InSingletonScope().Named(DeviceName).WithConstructorArgument("deviceSettings", kernel.Get<IDeviceSettings>(DeviceName)).WithConstructorArgument("deviceSpotSet", kernel.Get<IDeviceSpotSet>(DeviceName));
                        kernel.Bind<IRainbow>().To<Rainbow>().InSingletonScope().Named(outputID).WithConstructorArgument("outputSettings", kernel.Get<IOutputSettings>(outputID));
                        kernel.Bind<IDeviceSpotSet>().To<DeviceSpotSet>().InSingletonScope().Named(outputID).WithConstructorArgument("outputSettings", kernel.Get<IOutputSettings>(outputID));
                        kernel.Bind<IMusic>().To<Music>().InSingletonScope().Named(outputID).WithConstructorArgument("outputSettings", kernel.Get<IOutputSettings>(outputID));
                        kernel.Bind<IDesktopDuplicatorReader>().To<DesktopDuplicatorReader>().InSingletonScope().Named(outputID).WithConstructorArgument("outputSettings", kernel.Get<IOutputSettings>(outputID));
                        kernel.Bind<IGifxelation>().To<Gifxelation>().InSingletonScope().Named(outputID).WithConstructorArgument("outputSettings", kernel.Get<IOutputSettings>(outputID));
                        kernel.Bind<IStaticColor>().To<StaticColor>().InSingletonScope().Named(outputID).WithConstructorArgument("outputSettings", kernel.Get<IOutputSettings>(outputID));
                        var spotset = kernel.Get<IDeviceSpotSet>(outputID);
                        var rainbow = kernel.Get<IRainbow>(outputID);
                        var screencapture = kernel.Get<IDesktopDuplicatorReader>(outputID);
                        var music = kernel.Get<IMusic>(outputID);
                        var staticColor = kernel.Get<IStaticColor>(outputID);
                        var gifxelation = kernel.Get<IGifxelation>(outputID);

                    }



                }




            }

            return kernel;


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
                foreach (var device in devices)
                {
                    device.CurrentState = State.sleep;
                    /*Thread.Sleep(1000);*/ //wait for serialstream to finising sending
                    //serialStream.Stop();
                }
                //dispose hwmonitor to prevent file lock
                hwMonitor.Dispose();
                _log.Debug("Application exit!");
            };



            SystemEvents.PowerModeChanged += (s, e) =>
            {
                _log.Debug("Changing Powermode to {0}", e.Mode);
                if (e.Mode == PowerModes.Resume)
                {
                    GC.Collect();
                    var devices = kernel.GetAll<IDeviceSettings>();
                    foreach (var device in devices)
                    {
                        device.CurrentState = State.normal;
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
                    foreach (var device in devices)
                    {
                        device.CurrentState = State.sleep;
                        //Thread.Sleep(1000);
                        //serialStream.Stop();
                    }


                    //var desktopFrame = kernel.Get<IDesktopFrame>();
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
                foreach (var device in devices)
                {
                    device.CurrentState = State.sleep;
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

        private IGeneralSettings GeneralSettings { get; set; }


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
