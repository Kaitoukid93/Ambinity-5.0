using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Threading.Tasks;
using adrilight.DesktopDuplication;
using NLog;
using Polly;
using System.Linq;
using System.Windows.Media.Imaging;
using adrilight.ViewModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using adrilight.Resources;
using adrilight.Util;
using GalaSoft.MvvmLight;
using Microsoft.Win32;

namespace adrilight
{
    internal class DesktopFrame : ViewModelBase, IDesktopFrame
    {
        private readonly ILogger _log = LogManager.GetCurrentClassLogger();

        public DesktopFrame(IGeneralSettings userSettings, MainViewViewModel mainViewViewModel, int screen)
        {
            UserSettings = userSettings ?? throw new ArgumentNullException(nameof(userSettings));
            
            ScreenToCapture = screen;

            MainViewModel = mainViewViewModel ?? throw new ArgumentNullException(nameof(mainViewViewModel));
            _retryPolicy = Policy.Handle<Exception>()
                .WaitAndRetryForever(ProvideDelayDuration);

            //  SystemEvents.DisplaySettingsChanged += new EventHandler(SystemEvents_DisplaySettingsChanged);


            UserSettings.PropertyChanged += PropertyChanged;
            // SettingsViewModel.PropertyChanged += PropertyChanged;
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

        //private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        //{
        //    RefreshCaptureSource();
        //}

        public bool IsRunning { get; private set; } = false;
        public bool NeededRefreshing { get; private set; } = false;

        private CancellationTokenSource _cancellationTokenSource;

        private Thread _workerThread;
        public ByteFrame Frame { get; set; }
        private int ScreenToCapture { get; set; }


        public void RefreshCaptureSource()
        {
            var isRunning = IsRunning;
            var shouldBeRunning = true;
            //  var shouldBeRefreshing = NeededRefreshing;
            if (isRunning && shouldBeRunning)
            {
                //start it
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;
                _desktopDuplicator?.Dispose();
                _desktopDuplicator = null;
                _log.Debug("Stopped Desktop Duplication Reader.");
                IsRunning = false;

                _log.Debug("starting the capturing");
                _cancellationTokenSource = new CancellationTokenSource();
                _workerThread = new Thread(() => Run(_cancellationTokenSource.Token)) {
                    IsBackground = true,
                    Priority = ThreadPriority.BelowNormal,
                    Name = "DesktopDuplicatorReader"
                };
                _workerThread.Start();

            }
        }
        public void RefreshCapturingState()
        {
            var isRunning = _cancellationTokenSource != null && IsRunning;
            var shouldBeRunning = true;
            //  var shouldBeRefreshing = NeededRefreshing;



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
            //if (IsRunning) throw new Exception(nameof(DesktopDuplicatorReader) + " is already running!");

            IsRunning = true;
            NeededRefreshing = false;
            _log.Debug("Started Reading of First Desktop Frame.");
            Bitmap image = null;
            Frame = new ByteFrame();


            try
            {


                BitmapData bitmapData = new BitmapData();

                while (!token.IsCancellationRequested)
                {
                    var isPreviewRunning = MainViewModel.IsSplitLightingWindowOpen;
                    var frameTime = Stopwatch.StartNew();
                    // var context = new Context();
                    // context.Add("image", image);
                    var newImage = _retryPolicy.Execute(() => GetNextFrame(image));
                    TraceFrameDetails(newImage);
                    if (newImage == null)
                    {
                        //there was a timeout before there was the next frame, simply retry!
                        continue;
                    }
                    image = newImage;

                    // Lock the bitmap's bits.  
                    var rect = new System.Drawing.Rectangle(0, 0, image.Width, image.Height);
                    System.Drawing.Imaging.BitmapData bmpData =
                        image.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                        image.PixelFormat);

                    // Get the address of the first line.
                    IntPtr ptr = bmpData.Scan0;

                    // Declare an array to hold the bytes of the bitmap.
                    int bytes = Math.Abs(bmpData.Stride) * image.Height;
                    byte[] rgbValues = new byte[bytes];

                    // Copy the RGB values into the array.
                    System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);
                    Frame.Frame = rgbValues;
                    Frame.FrameWidth = image.Width;
                    Frame.FrameHeight = image.Height;
                    //if(isPreviewRunning)
                    //    MainViewModel.ShaderImageUpdate(Frame);



                    image.UnlockBits(bitmapData);
                    int minFrameTimeInMs = 1000 / 60;
                    var elapsedMs = (int)frameTime.ElapsedMilliseconds;
                    if (elapsedMs < minFrameTimeInMs)
                    {
                        Thread.Sleep(minFrameTimeInMs - elapsedMs);
                    }
                }
            }


            finally
            {
                image?.Dispose();
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

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
            _workerThread?.Join();
            _workerThread = null;
        }






        private int? _lastObservedHeight;
        private int? _lastObservedWidth;

        private void TraceFrameDetails(Bitmap image)
        {
            //there are many frames per second and we need to extract useful information and only log those!
            if (image == null)
            {
                //if the frame is null, this can mean two things. the timeout from the desktop duplication api was reached
                //before the monitor content changed or there was some other error.
            }
            else
            {
                if (_lastObservedHeight != null && _lastObservedWidth != null
                    && (_lastObservedHeight != image.Height || _lastObservedWidth != image.Width))
                {
                    _log.Debug("The frame size changed from {0}x{1} to {2}x{3}"
                        , _lastObservedWidth, _lastObservedHeight
                        , image.Width, image.Height);

                }
                _lastObservedWidth = image.Width;
                _lastObservedHeight = image.Height;

            }
        }






        private Bitmap GetNextFrame(Bitmap reusableBitmap)
        {



            if (_desktopDuplicator == null)
            {
                try
                {
                    _desktopDuplicator = new DesktopDuplicator(0, ScreenToCapture);
                }
                catch (Exception ex)
                {
                    if (ex.Message == "Unknown, just retry")
                    {
                        _log.Error(ex, "could be secure desktop");
                    }
                    //  UserSettings.ShouldbeRunning = false;
                    //  RaisePropertyChanged(() => UserSettings.ShouldbeRunning);

                    // _desktopDuplicator.Dispose();
                    // _desktopDuplicator = null;
                    GC.Collect();
                    return null;

                }

            }

            try
            {
                var latestFrame = _desktopDuplicator.GetLatestFrame(reusableBitmap);
                if (reusableBitmap != null && (reusableBitmap.Width != latestFrame.Width || reusableBitmap.Height != latestFrame.Height))
                {
                    //new resolution detected, change current top left width height to match
                    double scaleX = (double)latestFrame.Width / (double)reusableBitmap.Width;
                    double scaleY = (double)latestFrame.Height / (double)reusableBitmap.Height;
                    foreach(var device in MainViewModel.AvailableDevices.Where(d=>!(d.IsDummy)))
                    {
                        foreach(var output in device.AvailableOutputs)
                        {
                            output.OutputLEDSetup.OnResolutionChanged(scaleX,scaleY);
                        }
                    }

                }
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


                _desktopDuplicator.Dispose();
                _desktopDuplicator = null;
                GC.Collect();
                return null;
            }
        }




    }
}