using adrilight.ViewModel;
using adrilight_shared.Helpers;
using adrilight_shared.Models.FrameData;
using adrilight_shared.Settings;
using GalaSoft.MvvmLight;
using Polly;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace adrilight.Services.CaptureEngine.ScreenCapture
{
    internal class DesktopFrameDXGI : ViewModelBase, ICaptureEngine
    {
        public DesktopFrameDXGI(IGeneralSettings userSettings, MainViewViewModel mainViewViewModel)
        {
            UserSettings = userSettings ?? throw new ArgumentNullException(nameof(userSettings));
            UserSettings.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(UserSettings.ScreenCapturingEnabled):
                        RefreshCapturingState();
                        break;
                }
            };
            MainViewModel = mainViewViewModel ?? throw new ArgumentNullException(nameof(mainViewViewModel));
            MainViewModel.AvailableBitmaps.CollectionChanged += async (s, e) =>
            {
                switch (e.Action)
                {
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                        await Task.Run(() => ScreenSetupChanged());
                        break;
                }
            };
            RefreshCapturingState();
        }

        #region private field
        private List<Thread> _workerThreads;
        private enum RunningState { Capturing, Waiting, Canceling };
        private RunningState _state = RunningState.Canceling;
        private DesktopDuplicator[] _desktopDuplicators;
        private CancellationTokenSource _cancellationTokenSource;
        #endregion

        #region public properties
        public bool IsRunning { get; private set; } = false;
        private int _serviceRequired;
        public int ServiceRequired {
            get { return _serviceRequired; }
            set
            {
                if ((_serviceRequired == 0 && value == 1) || (_serviceRequired == 1 && value == 0))
                {
                    _serviceRequired = value;
                    RefreshCapturingState();
                }
                _serviceRequired = value;
            }
        }
        public ByteFrame Frame { get; set; }
        public ByteFrame[] Frames { get; set; }
        private DrawableHelpers DrHlprs { get; set; }
        public object Lock { get; } = new object();
        #endregion
        private void ScreenSetupChanged()
        {
            Stop();
            _workerThreads = new List<Thread>();
            _cancellationTokenSource = new CancellationTokenSource();
            Log.Information("starting DXGI");
            IEnumerable<MonitorInfo> monitors = MonitorEnumerationHelper.GetMonitors();
            Frames = new ByteFrame[monitors.Count()];
            if (_desktopDuplicators != null)
            {
                for (var i = 0; i < _desktopDuplicators.Length; i++)
                {
                    _desktopDuplicators[i]?.Dispose();
                    _desktopDuplicators[i] = null;
                }
            }
            _desktopDuplicators = new DesktopDuplicator[monitors.Count()];
            var index = 0;
            foreach (var monitor in monitors)
            {
                var workerThread = new Thread(() => Run(index++, _cancellationTokenSource.Token)) {
                    IsBackground = true,
                    Priority = ThreadPriority.BelowNormal,
                    Name = "DXGI" + monitor.DeviceName
                };
                _state = RunningState.Capturing;
                workerThread.Start();
                _workerThreads.Add(workerThread);
            }
        }
        public void RefreshCapturingState()
        {
            //start it
            var isRunning = _state != RunningState.Canceling;
            var shouldBeRunning = UserSettings.ScreenCapturingEnabled && ServiceRequired > 0;
            IEnumerable<MonitorInfo> monitors = MonitorEnumerationHelper.GetMonitors();
            Frames = new ByteFrame[monitors.Count()];

            if (isRunning && !shouldBeRunning)
            {
                //stop it!
                Log.Information("DesktopFrameDXGI is disabled");
                _state = RunningState.Waiting;

            }
            // this is start sign
            else if (!isRunning && shouldBeRunning)
            {
                _workerThreads = new List<Thread>();
                _cancellationTokenSource = new CancellationTokenSource();
                Log.Information("starting DXGI");
                if (_desktopDuplicators != null)
                {
                    for (var i = 0; i < _desktopDuplicators.Length; i++)
                    {
                        _desktopDuplicators[i]?.Dispose();
                        _desktopDuplicators[i] = null;
                    }
                }
                _desktopDuplicators = new DesktopDuplicator[monitors.Count()];
                var index = 0;
                foreach (var monitor in monitors)
                {
                    var workerThread = new Thread(() => Run(index++, _cancellationTokenSource.Token)) {
                        IsBackground = true,
                        Priority = ThreadPriority.BelowNormal,
                        Name = "DXGI" + monitor.DeviceName
                    };
                    _state = RunningState.Capturing;
                    workerThread.Start();
                    _workerThreads.Add(workerThread);
                }
            }
            else if (isRunning && shouldBeRunning)
            {
                _state = RunningState.Capturing;
            }
        }
        private bool CheckRectangle(Rect parrentRect, Rect childRect)
        {
            if (Rect.Intersect(parrentRect, childRect).Equals(childRect))
                return true;
            return false;
        }
        private IGeneralSettings UserSettings { get; set; }
        private MainViewViewModel MainViewModel { get; set; }



        private TimeSpan ProvideDelayDuration(int index)
        {
            if (index < 10)
            {

                return TimeSpan.FromMilliseconds(100);
            }

            if (index < 10 + 256)
            {
                //steps where there is also led dimming

                return TimeSpan.FromMilliseconds(5000d / 256);
            }
            return TimeSpan.FromMilliseconds(1000);
        }

        public void Run(int screenIndex, CancellationToken token)
        {
            IsRunning = true;
            Log.Information("DXGI is running for screen " + screenIndex);
            Frame = new ByteFrame();
            Frames[screenIndex] = new ByteFrame();
            var _retryPolicy = Policy.Handle<Exception>()
            .WaitAndRetryForever(sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(2000));
            var policyContext = new Context("RetryContext");

            policyContext.Add("CancellationTokenSource", _cancellationTokenSource);
            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (_state == RunningState.Capturing)
                    {
                        var frameTime = Stopwatch.StartNew();
                        var frame = _retryPolicy.Execute((ctx, ct) => GetNextFrame(screenIndex), policyContext, _cancellationTokenSource.Token);
                        if (frame == null)
                        {
                            //there was a timeout before there was the next frame, simply retry!
                            continue;
                        }

                        lock (Lock)
                        {
                            Frames[screenIndex] = frame;
                        }
                        if (MainViewModel.IsRegionSelectionOpen)
                        {
                            MainViewModel.DesktopsPreviewUpdate(Frames[screenIndex], screenIndex);
                        }
                        var minFrameTimeInMs = 1000 / 30;
                        var elapsedMs = (int)frameTime.ElapsedMilliseconds;
                        if (elapsedMs < minFrameTimeInMs)
                        {
                            Thread.Sleep(minFrameTimeInMs - elapsedMs);
                        }
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, ToString());
            }
        }


        public void Stop()
        {
            Log.Information("Stop called for DXGI");
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource = null;
            }
            _state = RunningState.Canceling;
            if (_workerThreads == null)
                return;
            for (var i = 0; i < _workerThreads.Count(); i++)
            {
                if (_workerThreads[i] == null) return;
                //_captures[i]?.Dispose();
                GC.Collect();
                _workerThreads[i]?.Join();
                _workerThreads[i] = null;
            }

        }

        private int? _lastObservedHeight;
        private int? _lastObservedWidth;
        private ByteFrame GetNextFrame(int screenIndex)
        {



            if (_desktopDuplicators[screenIndex] == null)
            {

                _desktopDuplicators[screenIndex] = new DesktopDuplicator(0, screenIndex);

            }

            try
            {
                var latestFrame = _desktopDuplicators[screenIndex].GetLatestFrame();
                return latestFrame;

            }
            catch (Exception ex)
            {
                // Log.Error(ex.ToString());
                // Log.Information("Problem when getting next frame from screen: " + screenIndex);
                _desktopDuplicators[screenIndex]?.Dispose();
                _desktopDuplicators[screenIndex] = null;
                GC.Collect();
                throw;
            }
        }

    }
}