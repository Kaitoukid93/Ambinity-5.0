using adrilight.DesktopDuplication;
using adrilight.Spots;
using adrilight.ViewModel;
using GalaSoft.MvvmLight;
using Polly;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;

namespace adrilight
{
    internal class DesktopFrameDXGI : ViewModelBase, ICaptureEngine
    {
        public DesktopFrameDXGI(IGeneralSettings userSettings, MainViewViewModel mainViewViewModel)
        {
            UserSettings = userSettings ?? throw new ArgumentNullException(nameof(userSettings));
            MainViewModel = mainViewViewModel ?? throw new ArgumentNullException(nameof(mainViewViewModel));
            MainViewModel.AvailableBitmaps.CollectionChanged += (s, e) =>
            {
                switch (e.Action)
                {
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                        ScreenSetupChanged();
                        break;
                }
            };
            RefreshCapturingState();
        }

        #region private field
        private List<Thread> _workerThreads;
        private enum RunningState { Capturing, Waiting, Canceling };
        private RunningState _state = RunningState.Waiting;
        private DesktopDuplicator[] _desktopDuplicators;
        private CancellationTokenSource _cancellationTokenSource;
        #endregion

        #region public properties
        public bool IsRunning { get; private set; } = false;
        public ByteFrame Frame { get; set; }
        public ByteFrame[] Frames { get; set; }
        private DrawableHelpers DrHlprs { get; set; }
        public object Lock { get; } = new object();
        #endregion
        private void ScreenSetupChanged()
        {
            RefreshCapturingState();
        }
        public void RefreshCapturingState()
        {
            //start it
            _state = RunningState.Waiting;
            if (_desktopDuplicators != null)
            {
                for (int i = 0; i < _desktopDuplicators.Length; i++)
                {
                    _desktopDuplicators[i]?.Dispose();
                    _desktopDuplicators[i] = null;
                }
            }
            Stop();
            _workerThreads = new List<Thread>();
            _cancellationTokenSource = new CancellationTokenSource();
            Log.Information("starting DXGI");
            IEnumerable<MonitorInfo> monitors = MonitorEnumerationHelper.GetMonitors();
            Frames = new ByteFrame[monitors.Count()];
            _desktopDuplicators = new DesktopDuplicator[monitors.Count()];
            int index = 0;
            foreach (var monitor in monitors)
            {
                Thread workerThread = new Thread(() => Run(index++)) {
                    IsBackground = true,
                    Priority = ThreadPriority.BelowNormal,
                    Name = "DXGI" + monitor.DeviceName
                };
                _state = RunningState.Capturing;
                workerThread.Start();
                _workerThreads.Add(workerThread);
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

        public void Run(int screenIndex)
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
                while (_state == RunningState.Capturing)
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
                    int minFrameTimeInMs = 1000 / 30;
                    var elapsedMs = (int)frameTime.ElapsedMilliseconds;
                    if (elapsedMs < minFrameTimeInMs)
                    {
                        Thread.Sleep(minFrameTimeInMs - elapsedMs);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, this.ToString());
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
            for (int i = 0; i < _workerThreads.Count(); i++)
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
                Log.Error(ex.ToString());
                Log.Information("Problem when getting next frame from screen: " + screenIndex);
                _desktopDuplicators[screenIndex]?.Dispose();
                _desktopDuplicators[screenIndex] = null;
                GC.Collect();
                throw;
            }
        }

    }
}