using adrilight.Util;
using adrilight.ViewModel;
using Microsoft.Win32;
using NLog;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace adrilight
{
    internal class RainbowTicker : IRainbowTicker
    {


        private readonly NLog.ILogger _log = LogManager.GetCurrentClassLogger();

        public RainbowTicker(IDeviceSettings[] allDeviceSettings, IGeneralSettings generalSettings, MainViewViewModel mainViewViewModel)
        {
            //DeviceSettings = deviceSettings ?? throw new ArgumentNullException(nameof(deviceSettings));
            AllDeviceSettings = allDeviceSettings ?? throw new ArgumentNullException(nameof(allDeviceSettings));
            GeneralSettings = generalSettings ?? throw new ArgumentException(nameof(generalSettings));
            //DeviceSpotSet = deviceSpotSet ?? throw new ArgumentNullException(nameof(deviceSpotSet));
            //AllDeviceSpotSet = allDeviceSpotSet ?? throw new ArgumentNullException(nameof(allDeviceSpotSet));
            MainViewViewModel = mainViewViewModel ?? throw new ArgumentNullException(nameof(mainViewViewModel));


            GeneralSettings.PropertyChanged += PropertyChanged;

            Ticks = new ObservableCollection<Tick>();
            RefreshColorState();

            _log.Info($"RainbowColor Created");

        }
        //Dependency Injection//
        // private IDeviceSettings DeviceSettings { get; }
        // private IDeviceSettings ParrentDevice { get; }
        private MainViewViewModel MainViewViewModel { get; }
        private IDeviceSettings[] AllDeviceSettings { get; }

        private IGeneralSettings GeneralSettings { get; }

        private double _rainbowStartIndex;
        public double RainbowStartIndex {
            get { return _rainbowStartIndex; }
            set { _rainbowStartIndex = value; }
        }
        private double _breathingBrightnessValue;
        public double BreathingBrightnessValue {
            get { return _breathingBrightnessValue; }
            set { _breathingBrightnessValue = value; }
        }
        public bool IsRunning { get; private set; } = false;
        private CancellationTokenSource _cancellationTokenSource;

        private void PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(GeneralSettings.SystemRainbowSpeed):
                    RefreshColorState();
                    break;
            }
        }
        //private void ParrentPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{
        //    switch (e.PropertyName)
        //    {
        //        case nameof(ParrentDevice.SyncOn):
        //            RefreshColorState();
        //            break;
        //    }
        //}

        private void RefreshColorState()
        {

            var isRunning = _cancellationTokenSource != null && IsRunning; // Check if sync mode is enabled

            var shouldBeRunning = true;
            if (isRunning && !shouldBeRunning)
            {
                //stop it!
                _log.Debug("stopping the Rainbow Ticker");
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;
            }
            else if (!isRunning && shouldBeRunning)
            {
                //start it
                _log.Debug("starting the Rainbow Ticker");
                _cancellationTokenSource = new CancellationTokenSource();
                var thread = new Thread(() => Run(_cancellationTokenSource.Token)) {
                    IsBackground = true,
                    Priority = ThreadPriority.BelowNormal,
                    Name = "RainbowTicker"
                };
                thread.Start();
            }
        }

        public object Lock { get; } = new object();
        public ObservableCollection<Tick> Ticks { get; private set; }
        public Tick MakeNewTick(int maxTick, double tickSpeed, string tickUID, TickEnum tickType)
        {
            var newTick = new Tick() { MaxTick = maxTick, TickSpeed = tickSpeed, TickUID = tickUID, TickType = tickType };
            Ticks.Add(newTick);
            return newTick;
        }
        private static void CheckSystemEventsHandlersForFreeze()
        {
            var handlers = typeof(SystemEvents).GetField("_handlers", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            var handlersValues = handlers.GetType().GetProperty("Values").GetValue(handlers);
            foreach (var invokeInfos in (handlersValues as IEnumerable).OfType<object>().ToArray())
                foreach (var invokeInfo in (invokeInfos as IEnumerable).OfType<object>().ToArray())
                {
                    var syncContext = invokeInfo.GetType().GetField("_syncContext", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(invokeInfo);
                    if (syncContext == null) throw new Exception("syncContext missing");
                    if (!(syncContext is WindowsFormsSynchronizationContext)) continue;
                    var threadRef = (WeakReference)syncContext.GetType().GetField("destinationThreadRef", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(syncContext);
                    if (!threadRef.IsAlive) continue;
                    var thread = (Thread)threadRef.Target;
                    if (thread.ManagedThreadId == 1) continue;  // Change here if you have more valid UI threads to ignore
                    var dlg = (Delegate)invokeInfo.GetType().GetField("_delegate", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(invokeInfo);
                    MessageBox.Show($"SystemEvents handler '{dlg.Method.DeclaringType}.{dlg.Method.Name}' could freeze app due to wrong thread: "
                                    + $"{thread.ManagedThreadId},{thread.IsThreadPoolThread},{thread.IsAlive},{thread.Name}");
                }
        }
        public void Run(CancellationToken token)

        {
            var rainbowMaxTick = GeneralSettings.SystemRainbowMaxTick;
            if (IsRunning) throw new Exception(" Rainbow Ticker is already running!");

            IsRunning = true;
            _log.Debug("Started Rainbow Ticker.");
            try
            {

                float gamma = 0.14f; // affects the width of peak (more or less darkness)
                float beta = 0.5f; // shifts the gaussian to be symmetric
                float ii = 0f;
                while (!token.IsCancellationRequested)
                {
                    lock (Lock)
                    {
                        foreach (var tick in Ticks)
                        {
                            if (tick.IsRunning)
                            {
                                if (tick.CurrentTick < tick.MaxTick - tick.TickSpeed)
                                    tick.CurrentTick += tick.TickSpeed;
                                else
                                    tick.CurrentTick = 0;
                            }
                            else
                            {
                                tick.CurrentTick = 0;
                            }

                        }
                        //rainbow and music ticker//
                        double rainbowSpeed = GeneralSettings.SystemRainbowSpeed / 5d;
                        RainbowStartIndex -= rainbowSpeed;
                        if (RainbowStartIndex < 0)
                        {
                            RainbowStartIndex = rainbowMaxTick;
                        }

                        float smoothness_pts = 2000 - (float)GeneralSettings.BreathingSpeed;
                        double pwm_val = 255.0 * (Math.Exp(-(Math.Pow(((ii++ / smoothness_pts) - beta) / gamma, 2.0)) / 2.0));
                        if (ii > smoothness_pts)
                            ii = 0f;

                        BreathingBrightnessValue = pwm_val / 255d;
                    }
                    Thread.Sleep(10);

                }
            }
            catch (OperationCanceledException)
            {
                _log.Debug("OperationCanceledException catched. returning.");


            }
            catch (Exception ex)
            {
                _log.Debug(ex, "Exception catched.");

                //allow the system some time to recover
                Thread.Sleep(500);
            }
            finally
            {

                _log.Debug("Stopped Rainbow Ticking.");
                IsRunning = false;
            }


        }



    }
}
