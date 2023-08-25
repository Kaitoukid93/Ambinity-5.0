using adrilight.ViewModel;
using adrilight_shared.Helpers;
using adrilight_shared.Models.FrameData;
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
using Windows.Foundation.Metadata;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX.Direct3D11;

namespace adrilight.Services.CaptureEngine.ScreenCapture
{
    internal class DesktopFrame : ViewModelBase, ICaptureEngine
    {
        /// <summary>
        /// this class record all present screens to list of byte arrays
        /// </summary>
        /// <param name="userSettings"></param>
        /// <param name="mainViewViewModel"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public DesktopFrame(IGeneralSettings userSettings, MainViewViewModel mainViewViewModel)
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
            _retryPolicy = Policy.Handle<Exception>()
                .WaitAndRetryForever(ProvideDelayDuration);
            RefreshCapturingState();
            // Microsoft.Win32.SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
        }

        #region private field
        private List<Thread> _workerThreads;
        private enum RunningState { Capturing, Waiting, Canceling };
        private RunningState _state = RunningState.Waiting;
        #endregion

        #region public properties
        public bool IsRunning { get; private set; } = false;
        public bool NeededRefreshing { get; private set; } = false;
        public ByteFrame[] Frames { get; set; }
        public ByteFrame Frame { get; set; }
        private DrawableHelpers DrHlprs { get; set; }
        private CancellationTokenSource _cancellationTokenSource;
        public object Lock { get; } = new object();
        #endregion
        //void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        //{

        //    Stop();
        //    _workerThreads = new List<Thread>();
        //    Log.Information("starting WCG");
        //    IEnumerable<MonitorInfo> monitors = MonitorEnumerationHelper.GetMonitors();
        //    Frames = new ByteFrame[monitors.Count()];
        //    _captures = new BasicCapture[monitors.Count()];
        //    int index = 0;
        //    foreach (var monitor in monitors)
        //    {
        //        Thread workerThread = new Thread(() => Run(monitor, index++)) {
        //            IsBackground = true,
        //            Priority = ThreadPriority.BelowNormal,
        //            Name = "WCG" + monitor.DeviceName
        //        };
        //        _state = RunningState.Capturing;
        //        workerThread.Start();
        //        _workerThreads.Add(workerThread);
        //    }
        //}
        private void ScreenSetupChanged()
        {
            Stop();
            _workerThreads = new List<Thread>();
            Log.Information("starting WCG");
            IEnumerable<MonitorInfo> monitors = MonitorEnumerationHelper.GetMonitors();
            Frames = new ByteFrame[monitors.Count()];
            _captures = new BasicCapture[monitors.Count()];
            var index = 0;
            foreach (var monitor in monitors)
            {
                var workerThread = new Thread(() => Run(monitor, index++)) {
                    IsBackground = true,
                    Priority = ThreadPriority.BelowNormal,
                    Name = "WCG" + monitor.DeviceName
                };
                _state = RunningState.Capturing;
                workerThread.Start();
                _workerThreads.Add(workerThread);
            }
        }
        public void RefreshCapturingState()
        {

            //start it
            //_device?.Dispose();
            _workerThreads = new List<Thread>();
            _cancellationTokenSource = new CancellationTokenSource();
            Log.Information("starting WCG");
            IEnumerable<MonitorInfo> monitors = MonitorEnumerationHelper.GetMonitors();
            Frames = new ByteFrame[monitors.Count()];
            _device = Direct3D11Helper.CreateDevice();
            _captures = new BasicCapture[monitors.Count()];
            var index = 0;
            foreach (var monitor in monitors)
            {
                var workerThread = new Thread(() => Run(monitor, index++)) {
                    IsBackground = true,
                    Priority = ThreadPriority.BelowNormal,
                    Name = "WCG" + monitor.DeviceName
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
        private IDirect3DDevice _device;
        private BasicCapture[] _captures;

        private readonly Policy _retryPolicy;

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
        public async void Run(MonitorInfo monitor, int screenIndex)
        {
            IsRunning = true;
            NeededRefreshing = false;
            Log.Information("WCG is running for screen :" + monitor.DeviceName);
            Frames[screenIndex] = new ByteFrame();
            try
            {
                await StartHmonCapture(monitor, screenIndex);
                while (_state == RunningState.Capturing)
                {
                    var frameTime = Stopwatch.StartNew();
                    lock (_captures[screenIndex].Lock)
                    {
                        Frames[screenIndex] = _captures[screenIndex].CurrentFrame;
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
            }
            catch (Exception ex)
            {
                Log.Error(ex, ToString());
            }
        }

        public void Stop()
        {
            Log.Information("Stop called for WCG");
            _state = RunningState.Canceling;
            for (var i = 0; i < _workerThreads.Count(); i++)
            {
                if (_workerThreads[i] == null) return;
                //_captures[i]?.Dispose();
                GC.Collect();
                _workerThreads[i]?.Join();
                _workerThreads[i] = null;
            }
        }

        public async Task StartHmonCapture(MonitorInfo monitor, int screenIndex)
        {
            GraphicsCaptureItem item = CaptureHelper.CreateItemForMonitor(monitor.Hmon);
            if (item != null)
            {
                await StartCaptureFromItem(item, screenIndex);
            }
        }
        public async Task StartCaptureFromItem(GraphicsCaptureItem item, int screenIndex)
        {
            try
            {
                StopCapture(screenIndex);
                _captures[screenIndex] = new BasicCapture(_device, item);
                if (ApiInformation.IsPropertyPresent(typeof(GraphicsCaptureSession).FullName, nameof(GraphicsCaptureSession.IsBorderRequired)))
                {
                    await _captures[screenIndex].StartCaptureBorderless();
                    Log.Information("This Version of Windows is able to disable the anoying yellow border. Thanks God");
                }
                else
                {
                    _captures[screenIndex].StartCaptureWithBorder();
                    Log.Information("This Version of Windows does not let you turn off the stupid yellow border, just f*king live with it");
                }


            }
            catch (Exception ex)
            {
                Log.Error(ex, "HmonCapture");
            }

        }
        private int? _lastObservedHeight;
        private int? _lastObservedWidth;

        public void StopCapture(int screenIndex)
        {

            _captures[screenIndex]?.Dispose();
            Log.Information("HmonCapture Dispose for Screen index " + screenIndex);
        }


    }
}