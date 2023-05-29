using adrilight.DesktopDuplication;
using adrilight.Spots;
using adrilight.Util;
using adrilight.Util.ModeParameters;
using adrilight.ViewModel;
using NLog;
using Polly;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Color = System.Windows.Media.Color;

namespace adrilight
{
    internal class DesktopDuplicatorReader : ILightingEngine
    {
        private readonly ILogger _log = LogManager.GetCurrentClassLogger();

        public DesktopDuplicatorReader(IGeneralSettings generalSettings,
            ICaptureEngine[] desktopFrame,
             MainViewViewModel mainViewViewModel,
             IControlZone zone
            )
        {
            GeneralSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));
            DesktopFrame = desktopFrame ?? throw new ArgumentNullException(nameof(desktopFrame));
            CurrentZone = zone as LEDSetup ?? throw new ArgumentNullException(nameof(zone));
            MainViewViewModel = mainViewViewModel ?? throw new ArgumentNullException(nameof(mainViewViewModel));
            _retryPolicy = Policy.Handle<Exception>().WaitAndRetryForever(ProvideDelayDuration);


            GeneralSettings.PropertyChanged += PropertyChanged;
            CurrentZone.PropertyChanged += PropertyChanged;
            MainViewViewModel.PropertyChanged += PropertyChanged;


            // Refresh();

            _log.Info($"DesktopDuplicatorReader created.");
        }




        /// <summary>
        /// dependency property
        /// </summary>
        private IGeneralSettings GeneralSettings { get; }

        private ICaptureEngine[] DesktopFrame { get; }
        public bool IsRunning { get; private set; }
        private LEDSetup CurrentZone { get; }

        private MainViewViewModel MainViewViewModel { get; }
        public LightingModeEnum Type { get; } = LightingModeEnum.ScreenCapturing;


        /// <summary>
        /// property changed event catching
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        #region PropertyChanged events
        private void PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                //which property that require this engine to refresh
                case nameof(CurrentZone.CurrentActiveControlMode):
                    var isRunning = _cancellationTokenSource != null;
                    if (isRunning || (CurrentZone.CurrentActiveControlMode as LightingMode).BasedOn == Type)
                        Refresh();
                    break;
                //case nameof(CurrentZone.MaskedControlMode):
                case nameof(MainViewViewModel.IsRichCanvasWindowOpen):
                    // case nameof(MainViewViewModel.IsRegisteringGroup):
                    Refresh();
                    break;

            }
        }
        private void OnBrightnessPropertyChanged(int value)
        {
            _brightness = value / 100d;

        }
        private void OnSmoothingPropertyChanged(int value)
        {
            _smoothFactor = value;
        }
        private void OnUseLinearLightingPropertyChanged(bool value)
        {
            _useLinearLighting = value;

        }
        #endregion
        /// <summary>
        /// private properties
        /// </summary>

        private CancellationTokenSource _cancellationTokenSource;
        private Thread _workerThread;
        private LightingMode _currentLightingMode;
        private int? _currentScreenIndex;
        private bool _useLinearLighting;
        private double _brightness;
        private int _smoothFactor;
        private int _displayUpdateRate = 25;
        private int _frameRate = 60;

        private SliderParameter _brightnessControl;
        private SliderParameter _smoothingControl;
        private ToggleParameter _useLinearLightingControl;
        public void Refresh()
        {
            if (CurrentZone.CurrentActiveControlMode == null)
            {
                return;
            }
            //find out which screen this zone belongs to
            var actualLeft = CurrentZone.Left + CurrentZone.OffsetX;
            var actualTop = CurrentZone.Top + CurrentZone.OffsetY;
            var width = CurrentZone.Width;
            var height = CurrentZone.Height;
            _currentScreenIndex = null;
            foreach (var screen in Screen.AllScreens)
            {
                var screenRect = new Rectangle(
                    (int)screen.Bounds.Left,
                    (int)screen.Bounds.Top,
                    (int)screen.Bounds.Width,
                    (int)screen.Bounds.Height);
                var zoneRect = new Rectangle(
                    (int)actualLeft,
                    (int)actualTop,
                    (int)width,
                    (int)height);
                if (Rectangle.Intersect(screenRect, zoneRect).Equals(zoneRect))
                    _currentScreenIndex = Array.IndexOf(Screen.AllScreens, screen);

            }
            var isRunning = _cancellationTokenSource != null;

            var currentLightingMode = CurrentZone.CurrentActiveControlMode as LightingMode;

            var shouldBeRunning =
                currentLightingMode.BasedOn == LightingModeEnum.ScreenCapturing &&
                //this zone has to be enable, this could be done by stop setting the spots, but the this thread still alive, so...
                CurrentZone.IsEnabled == true &&
                //stop this engine when any surface or editor open because this could cause capturing fail
                MainViewViewModel.IsRichCanvasWindowOpen == false &&
                //stop this engine when this zone is outside or atleast there is 1 pixel outside of the screen region
                CurrentZone.IsInsideScreen == true &&
                //this has to be inside of a screen
                _currentScreenIndex != null;
            ////registering group shoud be done
            //MainViewViewModel.IsRegisteringGroup == false;

            // this is stop sign by one or some of the reason above
            if (isRunning && !shouldBeRunning)
            {
                //stop it!
                _log.Debug("stopping the capturing");
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;

            }
            // this is start sign
            else if (!isRunning && shouldBeRunning)
            {
                //start it
                //get current lighting mode confirm that based on desktop duplicator reader engine

                _currentLightingMode = currentLightingMode;
                Init();
                _log.Debug("starting the capturing");
                _cancellationTokenSource = new CancellationTokenSource();
                _workerThread = new Thread(() => Run(_cancellationTokenSource.Token)) {
                    IsBackground = true,
                    Priority = ThreadPriority.BelowNormal,
                    Name = "DesktopDuplicatorReader"
                };
                _workerThread.Start();
            }
            else if (isRunning && shouldBeRunning)
            {
                Init();
            }
        }
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
                // DeviceSpotSet.IndicateMissingValues();
                return TimeSpan.FromMilliseconds(5000d / 256);
            }
            return TimeSpan.FromMilliseconds(1000);
        }
        public void Init()
        {
            //get dependency properties from current lighting mode(based on screencapturing)
            _brightnessControl = _currentLightingMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.Brightness).FirstOrDefault() as SliderParameter;
            _brightnessControl.PropertyChanged += (_, __) => OnBrightnessPropertyChanged(_brightnessControl.Value);
            _smoothingControl = _currentLightingMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.Smoothing).FirstOrDefault() as SliderParameter;
            _smoothingControl.PropertyChanged += (_, __) => OnSmoothingPropertyChanged(_smoothingControl.Value);
            _useLinearLightingControl = _currentLightingMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.LinearLighting).FirstOrDefault() as ToggleParameter;
            _useLinearLightingControl.PropertyChanged += (_, __) => OnUseLinearLightingPropertyChanged(_useLinearLightingControl.Value == 1 ? true : false);
            OnBrightnessPropertyChanged(_brightnessControl.Value);
            OnUseLinearLightingPropertyChanged(_useLinearLightingControl.Value == 1 ? true : false);
            OnSmoothingPropertyChanged(_smoothingControl.Value);
        }
        public void Run(CancellationToken token)
        {

            _log.Debug("Started Desktop Duplication Reader.");
            Bitmap image = null;
            BitmapData bitmapData = new BitmapData();
            IsRunning = true;
            try
            {
                //get current zone size and position respect to screen size scaled 8.0
                var screenLeft = Screen.AllScreens[(int)_currentScreenIndex].Bounds.Left;
                var screenTop = Screen.AllScreens[(int)_currentScreenIndex].Bounds.Top;
                var x = (int)((CurrentZone.Left + CurrentZone.OffsetX - screenLeft) / 8.0);
                var y = (int)((CurrentZone.Top + CurrentZone.OffsetY - screenTop) / 8.0);
                var width = (int)(CurrentZone.Width / 8.0) >= 1 ? (int)(CurrentZone.Width / 8.0) : 1;
                var height = (int)(CurrentZone.Height / 8.0) >= 1 ? (int)(CurrentZone.Height / 8.0) : 1;
                int updateIntervalCounter = 0;
                while (!token.IsCancellationRequested)
                {

                    //this indicator that user is opening this device and we need raise event when color update on each spot
                    bool shouldViewUpdate = MainViewViewModel.IsLiveViewOpen && MainViewViewModel.IsAppActivated && updateIntervalCounter > _frameRate / _displayUpdateRate;
                    if (shouldViewUpdate)
                        updateIntervalCounter = 0;
                    var frameTime = Stopwatch.StartNew();
                    var newImage = _retryPolicy.Execute(() => GetNextFrame(image, shouldViewUpdate));
                    TraceFrameDetails(newImage);
                    if (newImage == null)
                    {
                        //there was a timeout before there was the next frame, simply retry!
                        continue;
                    }
                    image = newImage;
                    try
                    {
                        image.LockBits(new Rectangle(x, y, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb, bitmapData);
                    }

                    catch (System.ArgumentException)
                    {
                        //usually the rectangle is jumping out of the image due to new profile, we recreate the rectangle based on the scale
                        // or simply dispose the image and let GetNextFrame handle the rectangle recreation
                        image = null;
                        continue;
                    }


                    lock (CurrentZone.Lock)
                    {



                        Parallel.ForEach(CurrentZone.Spots
                            , spot =>
                            {
                                var left = (spot as DeviceSpot).Left / 8.0;
                                var top = (spot as DeviceSpot).Top / 8.0;
                                var width = (spot as DeviceSpot).Width / 8.0;
                                var height = (spot as DeviceSpot).Height / 8.0;
                                const int numberOfSteps = 15;
                                int stepx = Math.Max(1, (int)(width) / numberOfSteps);
                                int stepy = Math.Max(1, (int)(height) / numberOfSteps);
                                Rectangle actualRectangle = new Rectangle(
                                    (int)(left),
                                    (int)(top),
                                    (int)(width),
                                    (int)(height));
                                GetAverageColorOfRectangularRegion(actualRectangle, stepy, stepx, bitmapData,
                                    out int sumR, out int sumG, out int sumB, out int count);

                                var countInverse = 1f / count;

                                ApplyColorCorrections(sumR * countInverse, sumG * countInverse, sumB * countInverse
                                    , out byte finalR, out byte finalG, out byte finalB, _useLinearLighting
                                    , 10, spot.Red, spot.Green, spot.Blue);

                                var spotColor = new OpenRGB.NET.Models.Color(finalR, finalG, finalB);
                                ApplySmoothing(
                                    spotColor.R,
                                    spotColor.G,
                                    spotColor.B,
                                    out byte RealfinalR,
                                    out byte RealfinalG,
                                    out byte RealfinalB,
                                 spot.Red,
                                 spot.Green,
                                 spot.Blue);
                                spot.SetColor((byte)(RealfinalR * _brightness), (byte)(RealfinalG * _brightness), (byte)(RealfinalB * _brightness), false);

                            });

                    }

                    image.UnlockBits(bitmapData);
                    int minFrameTimeInMs = 1000 / _frameRate;
                    var elapsedMs = (int)frameTime.ElapsedMilliseconds;
                    if (elapsedMs < minFrameTimeInMs)
                    {
                        Thread.Sleep(minFrameTimeInMs - elapsedMs);
                    }
                    updateIntervalCounter++;
                }
            }
            finally
            {
                image?.Dispose();

                _log.Debug("Stopped Desktop Duplication Reader.");
                IsRunning = false;
                GC.Collect();
            }
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

        private void ApplyColorCorrections(float r, float g, float b, out byte finalR, out byte finalG, out byte finalB, bool useLinearLighting, byte saturationTreshold
         , byte lastColorR, byte lastColorG, byte lastColorB)
        {
            if (lastColorR == 0 && lastColorG == 0 && lastColorB == 0)
            {
                //if the color was black the last time, we increase the saturationThreshold to make flickering more unlikely
                saturationTreshold += 2;
            }
            if (r <= saturationTreshold && g <= saturationTreshold && b <= saturationTreshold)
            {
                //black
                finalR = finalG = finalB = 0;
                return;
            }

            //"white" on wall was 66,68,77 without white balance
            //white balance
            //todo: introduce settings for white balance adjustments
            r *= 100 / 100f;
            g *= 100 / 100f;
            b *= 100 / 100f;

            if (!useLinearLighting)
            {
                //apply non linear LED fading ( http://www.mikrocontroller.net/articles/LED-Fading )
                finalR = FadeNonLinear(r);
                finalG = FadeNonLinear(g);
                finalB = FadeNonLinear(b);
            }
            else
            {
                //output
                finalR = (byte)r;
                finalG = (byte)g;
                finalB = (byte)b;
            }
        }
        private void ApplySmoothing(float r, float g, float b, out byte semifinalR, out byte semifinalG, out byte semifinalB,
           byte lastColorR, byte lastColorG, byte lastColorB)
        {

            semifinalR = (byte)((r + _smoothFactor * lastColorR) / (_smoothFactor + 1));
            semifinalG = (byte)((g + _smoothFactor * lastColorG) / (_smoothFactor + 1));
            semifinalB = (byte)((b + _smoothFactor * lastColorB) / (_smoothFactor + 1));
        }

        private readonly byte[] _nonLinearFadingCache = Enumerable.Range(0, 2560)
            .Select(n => FadeNonLinearUncached(n / 10f))
            .ToArray();

        private byte FadeNonLinear(float color)
        {
            var cacheIndex = (int)(color * 10);
            return _nonLinearFadingCache[Math.Min(2560 - 1, Math.Max(0, cacheIndex))];
        }
        private static byte FadeNonLinearUncached(float color)
        {
            const float factor = 80f;
            return (byte)(256f * ((float)Math.Pow(factor, color / 256f) - 1f) / (factor - 1));
        }
        private Bitmap GetNextFrame(Bitmap ReusableBitmap, bool isPreviewRunning)
        {

            try
            {
                ByteFrame CurrentFrame = null;
                Bitmap DesktopImage;
                if (_currentScreenIndex >= DesktopFrame.Length)
                {
                    HandyControl.Controls.MessageBox.Show("màn hình không khả dụng", "Sáng theo màn hình", MessageBoxButton.OK, MessageBoxImage.Error);
                    _currentScreenIndex = 0;
                }
                var currentDesktop = DesktopFrame[(int)_currentScreenIndex];
                lock (currentDesktop.Lock)
                {
                    CurrentFrame = currentDesktop.Frame;


                    if (CurrentFrame.Frame == null)
                    {
                        return null;
                    }
                    else
                    {
                        if (ReusableBitmap != null && ReusableBitmap.Width == CurrentFrame.FrameWidth && ReusableBitmap.Height == CurrentFrame.FrameHeight)
                        {
                            DesktopImage = ReusableBitmap;

                        }
                        else if (ReusableBitmap != null && (ReusableBitmap.Width != CurrentFrame.FrameWidth || ReusableBitmap.Height != CurrentFrame.FrameHeight))
                        {
                            DesktopImage = new Bitmap(CurrentFrame.FrameWidth, CurrentFrame.FrameHeight, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                        }
                        else //this is when app start
                        {
                            DesktopImage = new Bitmap(CurrentFrame.FrameWidth, CurrentFrame.FrameHeight, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                        }
                        var DesktopImageBitmapData = DesktopImage.LockBits(new Rectangle(0, 0, CurrentFrame.FrameWidth, CurrentFrame.FrameHeight), ImageLockMode.WriteOnly, DesktopImage.PixelFormat);
                        IntPtr pixelAddress = DesktopImageBitmapData.Scan0;
                        Marshal.Copy(CurrentFrame.Frame, 0, pixelAddress, CurrentFrame.Frame.Length);
                        DesktopImage.UnlockBits(DesktopImageBitmapData);
                        return DesktopImage;
                    }
                }

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

                GC.Collect();
                return null;
            }
        }

        public void Stop()
        {
            _log.Debug("Stop called.");
            CurrentZone.FillSpotsColor(Color.FromRgb(0, 0, 0));
            if (_workerThread == null) return;

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
            _workerThread?.Join();
            _workerThread = null;

        }


        private unsafe void GetAverageColorOfRectangularRegion(Rectangle spotRectangle, int stepy, int stepx, BitmapData bitmapData, out int sumR, out int sumG,
            out int sumB, out int count)
        {
            sumR = 0;
            sumG = 0;
            sumB = 0;
            count = 0;

            var stepCount = spotRectangle.Width / stepx;
            var stepxTimes4 = stepx * 4;
            for (var y = spotRectangle.Top; y < spotRectangle.Bottom; y += stepy)
            {
                byte* pointer = (byte*)bitmapData.Scan0 + bitmapData.Stride * y + 4 * spotRectangle.Left;
                for (int i = 0; i < stepCount; i++)
                {
                    sumB += pointer[0];
                    sumG += pointer[1];
                    sumR += pointer[2];

                    pointer += stepxTimes4;
                }
                count += stepCount;
            }
        }

    }
}