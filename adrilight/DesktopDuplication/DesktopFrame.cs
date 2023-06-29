using adrilight.DesktopDuplication;
using adrilight.Spots;
using adrilight.ViewModel;
using GalaSoft.MvvmLight;
using NLog;
using Polly;
using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX.Direct3D11;

namespace adrilight
{
    internal class DesktopFrame : ViewModelBase, ICaptureEngine
    {
        private readonly ILogger _log = LogManager.GetCurrentClassLogger();

        public DesktopFrame(IGeneralSettings userSettings, MainViewViewModel mainViewViewModel, string deviceName)
        {
            UserSettings = userSettings ?? throw new ArgumentNullException(nameof(userSettings));
            DeviceName = deviceName;
            MainViewModel = mainViewViewModel ?? throw new ArgumentNullException(nameof(mainViewViewModel));
            _retryPolicy = Policy.Handle<Exception>()
                .WaitAndRetryForever(ProvideDelayDuration);
            UserSettings.PropertyChanged += PropertyChanged;
            RefreshCapturingState();
            _log.Info($"DesktopDuplicatorReader created.");
        }

        private void PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {


                case nameof(MainViewModel.IsSplitLightingWindowOpen):

                    RefreshCapturingState();
                    break;


            }
        }

        #region private field
        private Thread _workerThread;
        private CancellationTokenSource _cancellationTokenSource;
        private int _currentScreenIdex;
        #endregion

        #region public properties
        public bool IsRunning { get; private set; } = false;
        public bool NeededRefreshing { get; private set; } = false;
        public ByteFrame Frame { get; set; }
        public string DeviceName { get; set; }
        private DrawableHelpers DrHlprs { get; set; }
        public object Lock { get; } = new object();
        #endregion

        public void RefreshCapturingState()
        {
            var isRunning = _cancellationTokenSource != null && IsRunning;
            var shouldBeRunning = true;
            _currentScreenIdex = Array.IndexOf(Screen.AllScreens, Screen.AllScreens.Where(s => s.DeviceName == DeviceName).FirstOrDefault());
            if (_currentScreenIdex == -1)
            {
                shouldBeRunning = false;
            }
            if (isRunning && !shouldBeRunning)
            {
                //stop it!
                _log.Debug("stopping the capturing");
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;

            }


            else if (!isRunning && shouldBeRunning)
            {
                //start it
                _log.Debug("starting the capturing for First Desktop Frame");
                _cancellationTokenSource = new CancellationTokenSource();
                _workerThread = new Thread(() => Run(_cancellationTokenSource.Token)) {
                    IsBackground = true,
                    Priority = ThreadPriority.BelowNormal,
                    Name = "DesktopDuplicatorReader"
                };
                _workerThread.Start();


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
        private IDirect3DDevice device;
        private BasicCapture capture;

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
        public async void Run(CancellationToken token)
        {
            //if (IsRunning) throw new Exception(nameof(DesktopDuplicatorReader) + " is already running!");

            IsRunning = true;
            NeededRefreshing = false;
            _log.Debug("Started Reading of First Desktop Frame.");
            //byte[] image = null;
            Rect frameSize = new Rect();
            Frame = new ByteFrame();


            try
            {

                device = Direct3D11Helper.CreateDevice();
                BitmapData bitmapData = new BitmapData();

                await StartHmonCapture();
                while (!token.IsCancellationRequested)
                {
                    var frameTime = Stopwatch.StartNew();
                    lock (Lock)
                    {
                        Frame = capture.CurrentFrame;
                    }
                    if (MainViewModel.IsRegionSelectionOpen)
                    {
                        MainViewModel.DesktopsPreviewUpdate(Frame, _currentScreenIdex);
                    }


                    //    // image.UnlockBits(bitmapData);
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

            }

            //finally
            //{
            //    //image?.Dispose();
            //    _desktopDuplicator?.Dispose();
            //    _desktopDuplicator = null;
            //    _log.Debug("Stopped Desktop Duplication Reader.");
            //    IsRunning = false;
            //    capture?.Dispose();
            //    GC.Collect();
            //}
        }

        public void Stop()
        {
            _log.Debug("Stop called for" + DeviceName);
            if (_workerThread == null) return;
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
            capture?.Dispose();
            GC.Collect();
            _workerThread?.Join();
            _workerThread = null;

        }

        public async Task StartHmonCapture()
        {
            MonitorInfo monitor = (from m in MonitorEnumerationHelper.GetMonitors()
                                   where m.DeviceName == DeviceName
                                   select m).First();
            GraphicsCaptureItem item = CaptureHelper.CreateItemForMonitor(monitor.Hmon);
            if (item != null)
            {
                await StartCaptureFromItem(item);
            }
        }
        public async Task StartCaptureFromItem(GraphicsCaptureItem item)
        {
            try
            {
                StopCapture();
                capture = new BasicCapture(device, item);
                await capture.StartCapture();
            }
            catch (Exception ex)
            {

            }

        }



        private int? _lastObservedHeight;
        private int? _lastObservedWidth;

        public void StopCapture()
        {
            capture?.Dispose();

        }


    }
}