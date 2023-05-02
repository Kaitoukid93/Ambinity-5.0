using OpenRGB;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Windows.Media.Color;
using adrilight.Util;
using adrilight.ViewModel;
using System.Threading;
using NLog;
using System.Threading.Tasks;
using System.Diagnostics;
using adrilight.Spots;
using System.Collections.ObjectModel;

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
        private double _musicStartIndex;
        private double _breathingBrightnessValue;
        private int _frameIndex;
        public double RainbowStartIndex {
            get { return _rainbowStartIndex; }
            set { _rainbowStartIndex = value; }
        }
        public int FrameIndex {
            get { return _frameIndex; }
            set { _frameIndex = value; }
        }

        public double MusicStartIndex {
            get { return _musicStartIndex; }
            set { _musicStartIndex = value; }
        }

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

        public void Run(CancellationToken token)

        {
            var rainbowMaxTick = GeneralSettings.SystemRainbowMaxTick;
            var musicMaxTick = GeneralSettings.SystemMusicMaxTick;


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
                            if(tick.IsRunning)
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
                        FrameIndex += 1;
                        if (FrameIndex >= rainbowMaxTick)
                        {
                            FrameIndex = 0;
                        }
                        double musicSpeed = GeneralSettings.SystemMusicSpeed / 5d;
                        MusicStartIndex += musicSpeed;
                        if (MusicStartIndex > musicMaxTick)
                        {
                            MusicStartIndex = 0;
                        }

                        //static breathing ticker
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
