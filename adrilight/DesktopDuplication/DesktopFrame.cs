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
using SharpDX.DXGI;
using adrilight.Settings;
using NAudio.SoundFont;
using System.Collections.Generic;
using SharpDX.DirectWrite;
using adrilight.Spots;
using System.Windows;

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

        #region private field
        private Thread _workerThread;
        private CancellationTokenSource _cancellationTokenSource;
        private int _currentScreenIdex => Array.IndexOf(Screen.AllScreens, Screen.AllScreens.Where(s => s.DeviceName == DeviceName).FirstOrDefault());
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
            //  var shouldBeRefreshing = NeededRefreshing;

            //safety check about resolution and screen
            var currentScreen = Screen.AllScreens.Where(s => s.DeviceName == DeviceName).FirstOrDefault();
            var currentRect = currentScreen.Bounds;
            if (UserSettings.Screens == null)
            {
                // no screen recorded before
                UserSettings.Screens = new List<DesktopScreen>();
                foreach (var screen in Screen.AllScreens)
                    UserSettings.Screens.Add(new DesktopScreen() { Name = screen.DeviceName, Rectangle = screen.Bounds });
                MainViewModel.WriteSettingJson(UserSettings);

            }
            var lastScreen = UserSettings.Screens.Where(s => s.Name == DeviceName).FirstOrDefault();



            if (lastScreen != null)
            {
                var lastRect = new Rect(lastScreen.Rectangle.Left, lastScreen.Rectangle.Top, lastScreen.Rectangle.Width, lastScreen.Rectangle.Height);
                //screen size change when the app is not alive, update components
                if (lastRect.Width != currentRect.Width || lastRect.Height != currentRect.Height || lastRect.Left != currentRect.Left || lastRect.Top != currentRect.Top)
                {
                    lastScreen.Rectangle = currentRect;
                    MainViewModel.WriteSettingJson(UserSettings);
                    //get rect scale

                    foreach (var device in MainViewModel.AvailableDevices)
                    {
                        foreach (var slaveDevice in device.AvailableLightingDevices)
                        {

                            HandleResolutionChange(lastRect, new Rect(currentScreen.Bounds.Left, currentScreen.Bounds.Top, currentScreen.Bounds.Width, currentScreen.Bounds.Height), slaveDevice);

                        }
                    }
                    //update zone list



                }
            }
            else
            {

                UserSettings.Screens.Add(new DesktopScreen() { Name = currentScreen.DeviceName, Rectangle = currentScreen.Bounds });
                MainViewModel.WriteSettingJson(UserSettings);

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
        //this is called when resolution has changed and we need to update out components
        private void HandleResolutionChange(Rect lastRect, Rect currentRect, ISlaveDevice Device)
        {
            var scaleX = currentRect.Width / lastRect.Width;
            var scaleY = currentRect.Height / lastRect.Height;
            var deltaX = currentRect.Left - lastRect.Left;
            var deltaY = currentRect.Top - lastRect.Top;
            var ledDevice = Device as ARGBLEDSlaveDevice;
            foreach (var zone in ledDevice.ControlableZones)
            {
                var ledZone = zone as LEDSetup;
                if (CheckRectangle(lastRect, ledZone.GetRect))
                {
                    //only scale the zone that in this screen
                    //move them out first
                    var translatedLeft = ledZone.GetRect.Left - lastRect.Left;
                    var translatedTop = ledZone.GetRect.Top - lastRect.Top;
                    translatedLeft *= scaleX;
                    translatedTop *= scaleY;
                    ledZone.SetScale(scaleX, scaleY, false);
                    //move it as the screen shift
                    ledZone.Left = translatedLeft + currentRect.Left;
                    ledZone.Top = translatedTop + currentRect.Top;
                }
                else
                {
                    //with normal zone, simple get them out of slavedevice
                    ledZone.Left = ledZone.GetRect.Left;
                    ledZone.Top = ledZone.GetRect.Top;
                }


            }
            //now we combine with unaffected zone and get newbound
            ledDevice.UpdateSizeByChild(true);
            //reset zone offset
            foreach (var zone in ledDevice.ControlableZones)
            {
                var ledZone = zone as LEDSetup;
                ledZone.Left -= ledDevice.Left;
                ledZone.Top -= ledDevice.Top;
                ledZone.OffsetX = ledDevice.Left;
                ledZone.OffsetY = ledDevice.Top;
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
            //byte[] image = null;
            Rect frameSize = new Rect();
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
                    var frame = _retryPolicy.Execute(() => GetNextFrame());
                    //TraceFrameDetails(newImage);
                    if (frame == null)
                    {
                        //there was a timeout before there was the next frame, simply retry!
                        continue;
                    }
                    //image = newImage;

                    // Lock the bitmap's bits.  
                    //var rect = new System.Drawing.Rectangle(0, 0, image.Width, image.Height);
                   // System.Drawing.Imaging.BitmapData bmpData =
                      //  image.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                      //  image.PixelFormat);

                    // Get the address of the first line.
                   // IntPtr ptr = bmpData.Scan0;

                    // Declare an array to hold the bytes of the bitmap.
                   // int bytes = Math.Abs(bmpData.Stride) * image.Height;
                   // byte[] rgbValues = new byte[bytes];

                    // Copy the RGB values into the array.
                    //System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);
                    lock (Lock)
                    {
                        Frame = frame;
                    }

                    //if(isPreviewRunning)
                    //    MainViewModel.ShaderImageUpdate(Frame);
                    if (MainViewModel.IsRichCanvasWindowOpen)
                    {
                        MainViewModel.DesktopsPreviewUpdate(Frame, _currentScreenIdex);
                    }


                   // image.UnlockBits(bitmapData);
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


                _desktopDuplicator.Dispose();
                _desktopDuplicator = null;
                GC.Collect();
                return null;
            }
        }




    }
}