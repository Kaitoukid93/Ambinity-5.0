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
using System.Windows;
using System.Windows.Forms;

namespace adrilight
{
    internal class DesktopFrameDXGI : ViewModelBase, ICaptureEngine
    {
        private readonly ILogger _log = LogManager.GetCurrentClassLogger();

        public DesktopFrameDXGI(IGeneralSettings userSettings, MainViewViewModel mainViewViewModel, string deviceName)
        {
            UserSettings = userSettings ?? throw new ArgumentNullException(nameof(userSettings));
            DeviceName = deviceName;
            MainViewModel = mainViewViewModel ?? throw new ArgumentNullException(nameof(mainViewViewModel));
            _retryPolicy = Policy.Handle<Exception>()
                .WaitAndRetryForever(ProvideDelayDuration);
            RefreshCapturingState();
            _log.Info($"DesktopDuplicatorReader created.");
        }

        #region private field
        private Thread _workerThread;
        private CancellationTokenSource _cancellationTokenSource;
        private int _currentScreenIdex => Array.IndexOf(Screen.AllScreens, Screen.AllScreens.Where(s => s.DeviceName == DeviceName).FirstOrDefault());
        #endregion

        #region public properties
        public bool IsRunning { get; private set; } = false;
        public ByteFrame Frame { get; set; }
        public string DeviceName { get; set; }
        private DrawableHelpers DrHlprs { get; set; }
        public object Lock { get; } = new object();
        #endregion

        public void RefreshCapturingState()
        {
            var isRunning = _cancellationTokenSource != null && IsRunning;
            var shouldBeRunning = true;
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

        private DesktopDuplicator _desktopDuplicator;

        public void Run(CancellationToken token)
        {
            IsRunning = true;
            _log.Debug("Started Reading of First Desktop Frame.");
            Frame = new ByteFrame();
            try
            {
                BitmapData bitmapData = new BitmapData();

                while (!token.IsCancellationRequested)
                {
                    var frameTime = Stopwatch.StartNew();
                    var frame = _retryPolicy.Execute(() => GetNextFrame());
                    if (frame == null)
                    {
                        //there was a timeout before there was the next frame, simply retry!
                        continue;
                    }

                    lock (Lock)
                    {
                        Frame = frame;
                    }
                    if (MainViewModel.IsRegionSelectionOpen)
                    {
                        MainViewModel.DesktopsPreviewUpdate(Frame, _currentScreenIdex);
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

            }

            finally
            {
                //image?.Dispose();
                _desktopDuplicator?.Dispose();
                _desktopDuplicator = null;
                _log.Debug("Stopped Desktop Duplication Reader.");
                IsRunning = false;
                GC.Collect();
            }
        }


        public void Stop()
        {
            _log.Debug("Stop called for First Desktop Frame");
            if (_workerThread == null) return;
            _desktopDuplicator?.Dispose();
            _desktopDuplicator = null;
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
            GC.Collect();
            _workerThread?.Join();
            _workerThread = null;

        }

        private int? _lastObservedHeight;
        private int? _lastObservedWidth;
        private ByteFrame GetNextFrame()
        {



            if (_desktopDuplicator == null)
            {

                _desktopDuplicator = new DesktopDuplicator(0, _currentScreenIdex);

            }

            try
            {
                var latestFrame = _desktopDuplicator.GetLatestFrame();
                return latestFrame;
            }
            catch (Exception ex)
            {
                if (ex.Message != "_outputDuplication is null" && ex.Message != "Access Lost, resolution might be changed" && ex.Message != "Invalid call, might be retrying" && ex.Message != "Failed to release frame.")
                {
                    _log.Error(ex, "GetNextFrame() failed.");

                    // throw;
                }
                else if (ex.Message == "Access Lost, resolution might be changed")
                {
                    _log.Error(ex, "Access Lost, retrying");

                }
                else if (ex.Message == "Invalid call, might be retrying")
                {
                    _log.Error(ex, "Invalid Call Lost, retrying");
                }
                else if (ex.Message == "Failed to release frame.")
                {
                    _log.Error(ex, "Failed to release frame.");
                }

                else
                {
                    throw new DesktopDuplicationException("Unknown Device Error", ex);
                }


                _desktopDuplicator?.Dispose();
                _desktopDuplicator = null;
                GC.Collect();
                return null;
            }
        }

    }
}