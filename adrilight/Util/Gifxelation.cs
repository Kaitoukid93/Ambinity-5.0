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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Color = System.Windows.Media.Color;

namespace adrilight
{
    internal class Gifxelation : ILightingEngine
    {
        private readonly ILogger _log = LogManager.GetCurrentClassLogger();
        public static Bitmap WorkingBitmap { get; set; }
        public static Bitmap LoadedStillBitmap { get; set; }
        public ByteFrame[] LoadedGifImage { get; set; }
        public Gifxelation(IGeneralSettings generalSettings,
             MainViewViewModel mainViewViewModel,
             IControlZone zone,
             IRainbowTicker rainbowTicker
            )
        {
            GeneralSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));
            CurrentZone = zone as LEDSetup ?? throw new ArgumentNullException(nameof(zone));
            MainViewViewModel = mainViewViewModel ?? throw new ArgumentNullException(nameof(mainViewViewModel));
            RainbowTicker = rainbowTicker ?? throw new ArgumentNullException(nameof(rainbowTicker));
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
        private IRainbowTicker RainbowTicker { get; }
        private MainViewViewModel MainViewViewModel { get; }
        public LightingModeEnum Type { get; } = LightingModeEnum.Gifxelation;


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
        private void OnCapturingRegionChanged(CapturingRegion region)
        {
            var left = region.ScaleX * LoadedGifImage[0].FrameWidth;
            var top = region.ScaleY * LoadedGifImage[0].FrameHeight;
            var width = region.ScaleWidth * LoadedGifImage[0].FrameWidth;
            var height = region.ScaleHeight * LoadedGifImage[0].FrameHeight;
            Rect zoneParrent = new Rect();
            if (CurrentZone.IsInControlGroup)
            {
                zoneParrent = CurrentZone.GroupRect;

            }
            else
            {
                zoneParrent = new Rect(CurrentZone.GetRect.Left, CurrentZone.GetRect.Top, CurrentZone.Width, CurrentZone.Height);
            }
            var zoneLeft = CurrentZone.GetRect.Left - zoneParrent.Left;
            var zoneTop = CurrentZone.GetRect.Top - zoneParrent.Top;
            var zoneRegion = new CapturingRegion(zoneLeft / zoneParrent.Width, zoneTop / zoneParrent.Height, CurrentZone.Width / zoneParrent.Width, CurrentZone.Height / zoneParrent.Height);

            _currentCapturingRegion = new Rectangle((int)(zoneRegion.ScaleX * width + left), (int)(zoneRegion.ScaleY * height + top), (int)(zoneRegion.ScaleWidth * width), (int)(zoneRegion.ScaleHeight * height));
        }
        private async Task OnSelectedGifChanged(IParameterValue value)
        {
            //set palette
            var gif = value as Gif;
            bool result = await Task.Run(() => LoadGifFromDisk(gif.Path));
            if (!result)
            {
                HandyControl.Controls.MessageBox.Show("File GIF bị lỗi", "Gif error", MessageBoxButton.OK, MessageBoxImage.Error);
                LoadedGifImage = null;
            }
        }
        #endregion
        /// <summary>
        /// private properties
        /// </summary>

        private CancellationTokenSource _cancellationTokenSource;
        private Thread _workerThread;
        private LightingMode _currentLightingMode;
        private double _brightness;
        private int _smoothFactor;
        private int _frameRate = 60;
        private Rectangle _currentCapturingRegion;

        private SliderParameter _brightnessControl;
        private SliderParameter _smoothingControl;
        private CapturingRegionSelectionButtonParameter _regionControl;
        private ListSelectionParameter _gifControl;
        public void Refresh()
        {
            if (CurrentZone.CurrentActiveControlMode == null)
            {
                return;
            }
            var isRunning = _cancellationTokenSource != null;

            var currentLightingMode = CurrentZone.CurrentActiveControlMode as LightingMode;

            var shouldBeRunning =
                currentLightingMode.BasedOn == LightingModeEnum.Gifxelation &&
                //this zone has to be enable, this could be done by stop setting the spots, but the this thread still alive, so...
                CurrentZone.IsEnabled == true &&
                //stop this engine when any surface or editor open because this could cause capturing fail
                MainViewViewModel.IsRichCanvasWindowOpen == false;
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
        public async void Init()
        {
            //get dependency properties from current lighting mode(based on screencapturing)
            _brightnessControl = _currentLightingMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.Brightness).FirstOrDefault() as SliderParameter;
            _brightnessControl.PropertyChanged += (_, __) => OnBrightnessPropertyChanged(_brightnessControl.Value);
            _gifControl = _currentLightingMode.Parameters.Where(P => P.ParamType == ModeParameterEnum.Gifs).FirstOrDefault() as ListSelectionParameter;
            _smoothingControl = _currentLightingMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.Smoothing).FirstOrDefault() as SliderParameter;
            _smoothingControl.PropertyChanged += (_, __) => OnSmoothingPropertyChanged(_smoothingControl.Value);
            _regionControl = _currentLightingMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.CapturingRegion).FirstOrDefault() as CapturingRegionSelectionButtonParameter;
            _regionControl.PropertyChanged += (_, __) =>
            {
                switch (__.PropertyName)
                {
                    case nameof(_regionControl.CapturingRegion):
                        OnCapturingRegionChanged(_regionControl.CapturingRegion);
                        break;
                }
            };
            _gifControl.PropertyChanged += async (_, __) =>
            {
                switch (__.PropertyName)
                {
                    case nameof(_gifControl.SelectedValue):
                        await OnSelectedGifChanged(_gifControl.SelectedValue);
                        break;
                }
            };
            _gifControl.LoadAvailableValues();
            if (_gifControl.SelectedValue == null)
            {
                _gifControl.SelectedValue = _gifControl.AvailableValues.First();
            }
            await OnSelectedGifChanged(_gifControl.SelectedValue);
            OnBrightnessPropertyChanged(_brightnessControl.Value);
            OnSmoothingPropertyChanged(_smoothingControl.Value);

        }
        public void Run(CancellationToken token)
        {

            _log.Debug("Started Desktop Duplication Reader.");
            Bitmap image = null;
            BitmapData bitmapData = new BitmapData();
            IsRunning = true;
            int _gifFrameIndex = 0;
            try
            {
                int updateIntervalCounter = 0;
                while (!token.IsCancellationRequested)
                {
                    //this indicator that user is opening this device and we need raise event when color update on each spot
                    if (LoadedGifImage == null)
                        continue;
                    if (_gifFrameIndex >= LoadedGifImage.Length - 1)
                        _gifFrameIndex = 0;
                    else
                        _gifFrameIndex++;
                    var frameTime = Stopwatch.StartNew();
                    bool isPreviewRunning = MainViewViewModel.IsRegionSelectionOpen;
                    var newImage = _retryPolicy.Execute(() => GetNextFrame(image, _gifFrameIndex, isPreviewRunning));
                    TraceFrameDetails(newImage);
                    if (newImage == null)
                    {
                        //there was a timeout before there was the next frame, simply retry!
                        continue;
                    }
                    if (image != null && (newImage.Width != image.Width || newImage.Height != image.Height))
                    {
                        OnCapturingRegionChanged(_regionControl.CapturingRegion);
                    }
                    else
                    {
                        OnCapturingRegionChanged(_regionControl.CapturingRegion);
                    }

                    image = newImage;
                    try
                    {
                        image.LockBits(_currentCapturingRegion, ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb, bitmapData);
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



                        foreach (var spot in CurrentZone.Spots)
                        {
                            var left = ((spot as DeviceSpot).Left / CurrentZone.Width) * _currentCapturingRegion.Width;
                            var top = ((spot as DeviceSpot).Top / CurrentZone.Height) * _currentCapturingRegion.Height;
                            var width = Math.Max(1, ((spot as DeviceSpot).Width / CurrentZone.Width) * _currentCapturingRegion.Width);
                            var height = Math.Max(1, ((spot as DeviceSpot).Height / CurrentZone.Height) * _currentCapturingRegion.Height);
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
                                , out byte finalR, out byte finalG, out byte finalB, true
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

                        }

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
        private void NextTick()
        {
            //stop ticking color if zone color source is solid or color use is static
            if (!CurrentZone.IsInControlGroup)
            {
                if (_ticks[0].CurrentTick < _ticks[0].MaxTick - _ticks[0].TickSpeed)
                    _ticks[0].CurrentTick += _ticks[0].TickSpeed;
                else
                {
                    _ticks[0].CurrentTick = 0;
                }
                if (!_shouldBeMoving)
                {
                    _ticks[1].CurrentTick = 0;
                }
                else
                {
                    if (_ticks[1].CurrentTick < _ticks[1].MaxTick - _ticks[1].TickSpeed)
                        _ticks[1].CurrentTick += _ticks[1].TickSpeed;
                    else
                    {
                        _ticks[1].CurrentTick = 0;
                    }
                }

            }
            else
            {
                if (!_shouldBeMoving)
                {
                    _ticks[1].IsRunning = false;
                }
                else
                {
                    _ticks[1].IsRunning = true;
                }
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
        public bool LoadGifFromDisk(string path)
        {
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    using (System.Drawing.Image imageToLoad = System.Drawing.Image.FromStream(fs))
                    {
                        var frameDim = new FrameDimension(imageToLoad.FrameDimensionsList[0]);
                        var frameCount = imageToLoad.GetFrameCount(frameDim);
                        LoadedGifImage = new ByteFrame[frameCount];
                        for (int i = 0; i < frameCount; i++)
                        {
                            imageToLoad.SelectActiveFrame(frameDim, i);

                            var resizedBmp = new Bitmap(imageToLoad, 240, 135);

                            var rect = new System.Drawing.Rectangle(0, 0, resizedBmp.Width, resizedBmp.Height);
                            System.Drawing.Imaging.BitmapData bmpData =
                                resizedBmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                                resizedBmp.PixelFormat);

                            // Get the address of the first line.
                            IntPtr ptr = bmpData.Scan0;

                            // Declare an array to hold the bytes of the bitmap.
                            int bytes = Math.Abs(bmpData.Stride) * resizedBmp.Height;
                            byte[] rgbValues = new byte[bytes];

                            // Copy the RGB values into the array.
                            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);
                            var frame = new ByteFrame();
                            frame.Frame = rgbValues;
                            frame.FrameWidth = resizedBmp.Width;
                            frame.FrameHeight = resizedBmp.Height;


                            LoadedGifImage[i] = frame;
                            resizedBmp.UnlockBits(bmpData);

                        }
                        imageToLoad.Dispose();
                        fs.Close();
                        GC.Collect();

                        return true;
                    }

                }

            }
            catch (Exception)
            {
                return false;
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
        private Bitmap GetNextFrame(Bitmap ReusableBitmap, int frameIndex, bool isPreviewRunning)
        {
            try
            {
                // get current working bitmap at frameIndex
                Bitmap CurrentGifImage;
                if (LoadedGifImage == null)
                {
                    return null;
                }
                var currentFrame = LoadedGifImage[frameIndex];
                if (isPreviewRunning)
                {
                    // MainViewViewModel.DesktopsPreviewUpdate(currentFrame);
                }
                if (currentFrame == null)
                {
                    return null;
                }
                else
                {
                    if (ReusableBitmap != null && ReusableBitmap.Width == currentFrame.FrameWidth && ReusableBitmap.Height == currentFrame.FrameHeight)
                    {
                        CurrentGifImage = ReusableBitmap;

                    }
                    else if (ReusableBitmap != null && (ReusableBitmap.Width != currentFrame.FrameWidth || ReusableBitmap.Height != currentFrame.FrameHeight))
                    {
                        CurrentGifImage = new Bitmap(currentFrame.FrameWidth, currentFrame.FrameHeight, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                    }
                    else //this is when app start
                    {
                        CurrentGifImage = new Bitmap(currentFrame.FrameWidth, currentFrame.FrameHeight, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                    }
                    var GifImageBitmapData = CurrentGifImage.LockBits(new Rectangle(0, 0, currentFrame.FrameWidth, currentFrame.FrameHeight), ImageLockMode.WriteOnly, CurrentGifImage.PixelFormat);
                    IntPtr pixelAddress = GifImageBitmapData.Scan0;
                    Marshal.Copy(currentFrame.Frame, 0, pixelAddress, currentFrame.Frame.Length);
                    CurrentGifImage.UnlockBits(GifImageBitmapData);
                    return CurrentGifImage;
                }
            }
            catch (Exception ex)
            {

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