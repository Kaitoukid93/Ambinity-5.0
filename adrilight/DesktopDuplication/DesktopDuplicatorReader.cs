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
using adrilight.Spots;
using System.Windows;

namespace adrilight
{
    internal class DesktopDuplicatorReader : IDesktopDuplicatorReader
    {
        private readonly ILogger _log = LogManager.GetCurrentClassLogger();

        public DesktopDuplicatorReader(IGeneralSettings userSettings,
            IDesktopFrame[] desktopFrame,
             MainViewViewModel mainViewViewModel,
             IOutputSettings outputSettings
            )
        {
            UserSettings = userSettings ?? throw new ArgumentNullException(nameof(userSettings));


            DesktopFrame = desktopFrame ?? throw new ArgumentNullException(nameof(desktopFrame));

            OutputSettings = outputSettings ?? throw new ArgumentNullException(nameof(outputSettings));
            // GraphicAdapter = graphicAdapter;
            // Output = output;
            MainViewViewModel = mainViewViewModel ?? throw new ArgumentNullException(nameof(mainViewViewModel));
            // SettingsViewModel = settingsViewModel ?? throw new ArgumentNullException(nameof(settingsViewModel));
            _retryPolicy = Policy.Handle<Exception>().WaitAndRetryForever(ProvideDelayDuration);

            UserSettings.PropertyChanged += PropertyChanged;
            OutputSettings.PropertyChanged += PropertyChanged;
            MainViewViewModel.PropertyChanged += PropertyChanged;
            // SettingsViewModel.PropertyChanged += PropertyChanged;
            RefreshCapturingState();

            _log.Info($"DesktopDuplicatorReader created.");
        }

        private void PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {

                
                case nameof(OutputSettings.OutputSelectedMode):
                case nameof(OutputSettings.OutputIsLoadingProfile):
                case nameof(OutputSettings.OutputIsEnabled):
                case nameof(OutputSettings.OutputParrentIsEnable):
                case nameof(OutputSettings.IsInSpotEditWizard):
                case nameof(OutputSettings.OutputLEDSetup.IsScreenCaptureEnabled):
                case nameof(MainViewViewModel.IsRichCanvasWindowOpen):
                    RefreshCapturingState();
                    break;

            }
        }

        //public bool IsRunning { get; private set; } = false;
        public bool NeededRefreshing { get; private set; } = false;
        private MainViewViewModel MainViewViewModel { get; }
        private CancellationTokenSource _cancellationTokenSource;

        private Thread _workerThread;
        // public int GraphicAdapter;
        // public int Output;

        public void RefreshCaptureSource()
        {
            var isRunning = _cancellationTokenSource != null;
            var shouldBeRunning = true;
            //  var shouldBeRefreshing = NeededRefreshing;
            if (isRunning && shouldBeRunning)
            {
                //start it

                //IsRunning = false;
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;
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
            var isRunning = _cancellationTokenSource != null;
            var shouldBeRunning = OutputSettings.OutputIsEnabled &&
                OutputSettings.OutputParrentIsEnable &&
                OutputSettings.OutputSelectedMode == 0 &&
                OutputSettings.IsInSpotEditWizard == false &&
                OutputSettings.OutputIsLoadingProfile == false &&
                MainViewViewModel.IsRichCanvasWindowOpen == false&&
                OutputSettings.OutputLEDSetup.IsScreenCaptureEnabled==true;
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




        private IGeneralSettings UserSettings { get; }


        private IDesktopFrame[] DesktopFrame { get; }

        private IOutputSettings OutputSettings { get; }




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




        public void Run(CancellationToken token)
        {
            //if (IsRunning) throw new Exception(nameof(DesktopDuplicatorReader) + " is already running!");

            //IsRunning = true;
            //NeededRefreshing = false;
            _log.Debug("Started Desktop Duplication Reader.");
            Bitmap image = null;
            BitmapData bitmapData = new BitmapData();


            try
            {



                while (!token.IsCancellationRequested)
                {
                    var currentOutput = MainViewViewModel.CurrentOutput;
                    bool outputIsSelected = false;
                    if (currentOutput != null && currentOutput.OutputUniqueID == OutputSettings.OutputUniqueID)
                        outputIsSelected = true;
                    bool isPreviewRunning = MainViewViewModel.IsSplitLightingWindowOpen && outputIsSelected;

                    var frameTime = Stopwatch.StartNew();
                    var newImage = _retryPolicy.Execute(() => GetNextFrame(image, isPreviewRunning));
                    TraceFrameDetails(newImage);
                    var scaleWidth = OutputSettings.OutputLEDSetup.ScaleWidth;
                    var scaleHeight = OutputSettings.OutputLEDSetup.ScaleHeight;
                    var scaleX = OutputSettings.OutputLEDSetup.ScaleLeft;
                    var scaleY = OutputSettings.OutputLEDSetup.ScaleTop;
                    var virtualWidth = (OutputSettings.OutputLEDSetup as LEDSetup).Width;
                    var virtualHeight = (OutputSettings.OutputLEDSetup as LEDSetup).Height;
                    var brightness = OutputSettings.OutputBrightness / 100d;
                    var devicePowerVoltage = OutputSettings.OutputPowerVoltage;
                    var devicePowerMiliamps = OutputSettings.OutputPowerMiliamps;

                    var numLED = OutputSettings.OutputLEDSetup.Spots.Count * OutputSettings.LEDPerSpot * OutputSettings.LEDPerLED;


                    if (newImage == null)
                    {
                        //there was a timeout before there was the next frame, simply retry!
                        continue;
                    }
                    image = newImage;
                    var x = (int)(scaleX * image.Width);
                    var y = (int)(scaleY * image.Height);
                    var width = (int)(scaleWidth * image.Width);
                    var height = (int)(scaleHeight * image.Height);
                   
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




                    lock (OutputSettings.OutputLEDSetup.Lock)
                    {
                        var useLinearLighting = OutputSettings.OutputUseLinearLighting;

                        //var imageRectangle = new Rectangle(0, 0, image.Width, image.Height);

                        //if (imageRectangle.Width != DeviceSpotSet.ExpectedScreenWidth || imageRectangle.Height != DeviceSpotSet.ExpectedScreenHeight)
                        //{
                        //    //the screen was resized or this is some kind of powersaving state
                        //    DeviceSpotSet.IndicateMissingValues();

                        //    continue;
                        //}
                        //else
                        //{
                        Parallel.ForEach(OutputSettings.OutputLEDSetup.Spots
                            , spot =>
                            {
                                const int numberOfSteps = 15;
                                int stepx = Math.Max(1, (int)(width * spot.ScaleWidth) / numberOfSteps);
                                int stepy = Math.Max(1, (int)(height * spot.ScaleHeight) / numberOfSteps);
                                Rectangle actualRectangle = new Rectangle(
                                    (int)(width * spot.ScaleLeft),
                                    (int)(height * spot.ScaleTop),
                                    (int)(width * spot.ScaleWidth),
                                    (int)(height * spot.ScaleHeight));
                                GetAverageColorOfRectangularRegion(actualRectangle, stepy, stepx, bitmapData,
                                    out int sumR, out int sumG, out int sumB, out int count);

                                var countInverse = 1f / count;

                                ApplyColorCorrections(sumR * countInverse, sumG * countInverse, sumB * countInverse
                                    , out byte finalR, out byte finalG, out byte finalB, useLinearLighting
                                    , OutputSettings.OutputSaturationThreshold, spot.Red, spot.Green, spot.Blue);

                                var spotColor = new OpenRGB.NET.Models.Color(finalR, finalG, finalB);

                                var semifinalSpotColor = Brightness.applyBrightness(spotColor, brightness, numLED, devicePowerMiliamps, devicePowerVoltage);
                                ApplySmoothing(semifinalSpotColor.R, semifinalSpotColor.G, semifinalSpotColor.B
                                    , out byte RealfinalR, out byte RealfinalG, out byte RealfinalB,
                                 spot.Red, spot.Green, spot.Blue);
                                if (!OutputSettings.IsInSpotEditWizard)
                                    spot.SetColor(RealfinalR, RealfinalG, RealfinalB, isPreviewRunning);

                            });
                        //}

                    }



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

                _log.Debug("Stopped Desktop Duplication Reader.");
                //IsRunning = false;
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
            r *= OutputSettings.OutputScreenCaptureWBRed / 100f;
            g *= OutputSettings.OutputScreenCaptureWBGreen / 100f;
            b *= OutputSettings.OutputScreenCaptureWBBlue / 100f;

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
            int smoothingFactor = OutputSettings.OutputSmoothness;


            semifinalR = (byte)((r + smoothingFactor * lastColorR) / (smoothingFactor + 1));
            semifinalG = (byte)((g + smoothingFactor * lastColorG) / (smoothingFactor + 1));
            semifinalB = (byte)((b + smoothingFactor * lastColorB) / (smoothingFactor + 1));
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
                if(OutputSettings.OutputLEDSetup.OutputSelectedDisplay>= DesktopFrame.Length)
                {
                    HandyControl.Controls.MessageBox.Show( "màn hình không khả dụng","Sáng theo màn hình", MessageBoxButton.OK, MessageBoxImage.Error);
                    CurrentFrame = DesktopFrame.FirstOrDefault().Frame;
                    OutputSettings.OutputLEDSetup.OutputSelectedDisplay = 0;
                }
                else
                {
                    CurrentFrame = DesktopFrame[OutputSettings.OutputLEDSetup.OutputSelectedDisplay].Frame;
                }
               
                if (isPreviewRunning)
                {
                    
                      MainViewViewModel.ShaderImageUpdate(CurrentFrame);
                   
                   
                }

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
                        //new resolution detected, change current top left width height to match
                        //double scaleX = (double)CurrentFrame.FrameWidth / (double)ReusableBitmap.Width;
                        //double scaleY = (double)CurrentFrame.FrameHeight / (double)ReusableBitmap.Height;
                        //(OutputSettings as OutputSettings).Width *= scaleX;
                        //(OutputSettings as OutputSettings).Height *= scaleY;
                        //(OutputSettings as OutputSettings).Left *= scaleX;
                        //(OutputSettings as OutputSettings).Top *= scaleY;

                        //OutputSettings.OutputRectangle = new Rectangle((int)x, (int)y, (int)width, (int)height);


                    }
                    else //this is when app start
                    {
                        DesktopImage = new Bitmap(CurrentFrame.FrameWidth, CurrentFrame.FrameHeight, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                        //var width = OutputSettings.OutputRectangleScaleWidth * CurrentFrame.FrameWidth;
                        //var height = OutputSettings.OutputRectangleScaleHeight * CurrentFrame.FrameHeight;
                        //var x = OutputSettings.OutputRectangleScaleLeft * CurrentFrame.FrameWidth;
                        //var y = OutputSettings.OutputRectangleScaleTop * CurrentFrame.FrameHeight;
                        //OutputSettings.OutputRectangle = new Rectangle((int)x, (int)y, (int)width, (int)height);

                    }

                    var DesktopImageBitmapData = DesktopImage.LockBits(new Rectangle(0, 0, CurrentFrame.FrameWidth, CurrentFrame.FrameHeight), ImageLockMode.WriteOnly, DesktopImage.PixelFormat);
                    IntPtr pixelAddress = DesktopImageBitmapData.Scan0;




                    Marshal.Copy(CurrentFrame.Frame, 0, pixelAddress, CurrentFrame.Frame.Length);



                    DesktopImage.UnlockBits(DesktopImageBitmapData);

                    return DesktopImage;
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


                //_desktopDuplicator.Dispose();
                //_desktopDuplicator = null;
                GC.Collect();
                return null;
            }
        }

        public void Stop()
        {
            _log.Debug("Stop called.");
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