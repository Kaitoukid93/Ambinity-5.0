using OpenRGB;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Windows.Media.Color;
using adrilight.Util;
using adrilight.ViewModel;
using System.Threading;
using NLog;
using System.Threading.Tasks;
using System.Diagnostics;
using adrilight.Spots;
using System.IO;
using adrilight.DesktopDuplication;
using Polly;
using System.Runtime.InteropServices;
using System.Windows;

namespace adrilight
{
    internal class Gifxelation : IGifxelation
    {
        private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "adrilight\\");
        private string JsonGifsFileNameAndPath => Path.Combine(JsonPath, "Gif");
        private string testGifPath => Path.Combine(JsonGifsFileNameAndPath, "Rainbow.gif");

        private readonly NLog.ILogger _log = LogManager.GetCurrentClassLogger();
        public enum LoadState { None, Still, Gif };
        public static LoadState ImageLoadState { get; set; } = LoadState.None;
        public static Rectangle ImageRect { get; set; }
        public static Bitmap WorkingBitmap { get; set; }
        public static Bitmap LoadedStillBitmap { get; set; }
        public  ByteFrame[] LoadedGifImage { get; set; }
        public static FrameDimension LoadedGifFrameDim { get; set; }
        public static int LoadedGifFrameCount { get; set; }
        public static int GifMillisconds { get; set; } = 0;
        public Gifxelation(IOutputSettings outputSettings, IRainbowTicker rainbowTicker, IGeneralSettings generalSettings, MainViewViewModel mainViewViewModel)
        {
            OutputSettings = outputSettings ?? throw new ArgumentNullException(nameof(outputSettings));
            GeneralSettings = generalSettings ?? throw new ArgumentException(nameof(generalSettings));
            // OutputSpotSet = outputSpotSet ?? throw new ArgumentException(nameof(outputSpotSet));


            RainbowTicker = rainbowTicker ?? throw new ArgumentNullException(nameof(rainbowTicker));
            MainViewViewModel = mainViewViewModel ?? throw new ArgumentNullException(nameof(mainViewViewModel));

            _retryPolicy = Policy.Handle<Exception>().WaitAndRetryForever(ProvideDelayDuration);
            OutputSettings.PropertyChanged += PropertyChanged;
            GeneralSettings.PropertyChanged += PropertyChanged;
            MainViewViewModel.PropertyChanged += PropertyChanged;
            inSync = OutputSettings.OutputIsSystemSync;
            RefreshColorState();
            _log.Info($"RainbowColor Created");

        }
        //Dependency Injection//
        private IOutputSettings OutputSettings { get; }
        public ByteFrame Frame { get; set; }
        private MainViewViewModel MainViewViewModel { get; }
        private IRainbowTicker RainbowTicker { get; }
        private IGeneralSettings GeneralSettings { get; }
        private bool inSync { get; set; }
        // private IDeviceSpotSet OutputSpotSet { get; }

        private Color[] colorBank = new Color[256];
        public bool IsRunning { get; private set; } = false;
        private CancellationTokenSource _cancellationTokenSource;

        private void PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(OutputSettings.OutputIsEnabled):
                case nameof(OutputSettings.OutputSelectedMode):
                case nameof(OutputSettings.OutputParrentIsEnable):

                    RefreshColorState();
                    break;
                
                case nameof(OutputSettings.IsInSpotEditWizard):
                case nameof(OutputSettings.OutputIsSystemSync):
                case nameof(OutputSettings.OutputSelectedGif):


                    ColorPaletteChanged();
                    break;


            }
        }


        private void RefreshColorState()
        {

            var isRunning = _cancellationTokenSource != null && IsRunning;
            var shouldBeRunning = OutputSettings.OutputIsEnabled && OutputSettings.OutputParrentIsEnable && OutputSettings.OutputSelectedMode == 4 && OutputSettings.IsInSpotEditWizard == false;

            if (isRunning && !shouldBeRunning)
            {
                //stop it!
                _log.Debug("stopping the Rainbow Color");
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;
            }
            else if (!isRunning && shouldBeRunning)
            {
                //start it
                _log.Debug("starting the Rainbow Color");
                _cancellationTokenSource = new CancellationTokenSource();
                var thread = new Thread(() => Run(_cancellationTokenSource.Token)) {
                    IsBackground = true,
                    Priority = ThreadPriority.BelowNormal,
                    Name = "RainbowColorCreator" + OutputSettings.OutputUniqueID
                };
                thread.Start();
            }
        }
        private void ColorPaletteChanged()
        {
            var isRunning = _cancellationTokenSource != null && IsRunning;
            var shouldBeRunning = OutputSettings.OutputIsEnabled && OutputSettings.OutputParrentIsEnable && OutputSettings.OutputSelectedMode == 4 && OutputSettings.IsInSpotEditWizard == false;


            if (isRunning && shouldBeRunning)
            {
                if (OutputSettings.OutputSelectedGif == null)
                //load test
                {
                    LoadGifFromDisk(testGifPath);

                }
                else
                {
                    bool result = LoadGifFromDisk(OutputSettings.OutputSelectedGif.Source);
                    if (!result)
                    {
                        HandyControl.Controls.MessageBox.Show("Ảnh động được chọn cho " + OutputSettings.OutputName + " đã bị xóa, vui long chọn ảnh khác", "Gif is not available", MessageBoxButton.OK, MessageBoxImage.Error);
                        LoadGifFromDisk(testGifPath);
                    }
                }


                //if(isInEditWizard)
                //    colorBank = GetColorGradientfromPalette(DefaultColorCollection.black).ToArray();
            }

        }


        private readonly Policy _retryPolicy;



        public void Run(CancellationToken token)

        {

            if (IsRunning) throw new Exception(" Rainbow Color is already running!");

            IsRunning = true;

            _log.Debug("Started Rainbow Color.");


            if (OutputSettings.OutputSelectedGif == null)
            //load test
            {
                LoadGifFromDisk(testGifPath);

            }
            else
            {
                bool result = LoadGifFromDisk(OutputSettings.OutputSelectedGif.Source);
                if(!result)
                {
                    HandyControl.Controls.MessageBox.Show("Ảnh động được chọn cho " + OutputSettings.OutputName + " đã bị xóa, vui long chọn ảnh khác", "Gif is not available", MessageBoxButton.OK, MessageBoxImage.Error);
                    LoadGifFromDisk(testGifPath);
                }
            }

            Bitmap image = null;
            BitmapData bitmapData = new BitmapData();
            Frame = new ByteFrame();
            try
            {


                //try load selected gif
                var numLED = OutputSettings.OutputLEDSetup.Spots.Length * OutputSettings.LEDPerSpot * OutputSettings.LEDPerLED;
                var outputPowerVoltage = OutputSettings.OutputPowerVoltage;
                var outputPowerMiliamps = OutputSettings.OutputPowerMiliamps;


                int _gifFrameIndex = 0;
                while (!token.IsCancellationRequested)
                {

                    var currentOutput = MainViewViewModel.CurrentOutput;
                    bool outputIsSelected = false;
                    if (currentOutput != null && currentOutput.OutputUniqueID == OutputSettings.OutputUniqueID)
                        outputIsSelected = true;
                    bool isPreviewRunning = MainViewViewModel.IsSplitLightingWindowOpen && outputIsSelected;

                    if (_gifFrameIndex >= LoadedGifImage.Length - 1)
                        _gifFrameIndex = 0;
                    else
                        _gifFrameIndex++;
                    var newImage = _retryPolicy.Execute(() => GetNextFrame(image, _gifFrameIndex, isPreviewRunning));
                    var scaleWidth = OutputSettings.OutputRectangleScaleWidth;
                    var scaleHeight = OutputSettings.OutputRectangleScaleHeight;
                    var scaleX = OutputSettings.OutputRectangleScaleLeft;
                    var scaleY = OutputSettings.OutputRectangleScaleTop;
                    var virtualWidth = (OutputSettings as OutputSettings).Width;
                    var virtualHeight = (OutputSettings as OutputSettings).Height;
                    var brightness = OutputSettings.OutputBrightness/100d;
                    var speed = OutputSettings.OutputGifSpeed;
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
                        image.LockBits(new Rectangle(x, y, width, height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb, bitmapData);
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

                        Parallel.ForEach(OutputSettings.OutputLEDSetup.Spots
                        , spot =>
                        {
                            const int numberOfSteps = 15;
                            int stepx = Math.Max(1, (int)(spot as IDrawable).Width / numberOfSteps);
                            int stepy = Math.Max(1, (int)(spot as IDrawable).Height / numberOfSteps);
                            Rectangle actualRectangle = new Rectangle(
                                (int)(width * (spot as IDrawable).Left / virtualWidth),
                                (int)(height * (spot as IDrawable).Top / virtualHeight),
                                (int)(width * (spot as IDrawable).Width / virtualWidth),
                                (int)(height * (spot as IDrawable).Height / virtualHeight));

                            GetAverageColorOfRectangularRegion(actualRectangle, stepy, stepx, bitmapData,
                                  out int sumR, out int sumG, out int sumB, out int count);

                            var countInverse = 1f / count;

                            ApplyColorCorrections(sumR * countInverse, sumG * countInverse, sumB * countInverse
                                , out byte finalR, out byte finalG, out byte finalB, true
                                , OutputSettings.OutputSaturationThreshold, spot.Red, spot.Green, spot.Blue);

                            var spotColor = new OpenRGB.NET.Models.Color(finalR, finalG, finalB);

                            var semifinalSpotColor = Brightness.applyBrightness(spotColor, brightness, numLED, outputPowerMiliamps, outputPowerVoltage);
                            ApplySmoothing(semifinalSpotColor.R, semifinalSpotColor.G, semifinalSpotColor.B
                                , out byte RealfinalR, out byte RealfinalG, out byte RealfinalB,
                             spot.Red, spot.Green, spot.Blue);
                            if (!OutputSettings.IsInSpotEditWizard)
                                spot.SetColor(RealfinalR, RealfinalG, RealfinalB, isPreviewRunning);

                        });


                    }
                    image.UnlockBits(bitmapData);
                    Thread.Sleep(speed);


                }
            }
            catch (OperationCanceledException)
            {
                _log.Debug("OperationCanceledException catched. returning.");


            }
            catch (Exception ex)
            {
                _log.Debug(ex, "Exception catched.");

                //allow the system some time to recover
                Thread.Sleep(500);
            }
            finally
            {

                image?.Dispose();

                _log.Debug("Stopped Desktop Duplication Reader.");
                IsRunning = false;
                GC.Collect();

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


        public  bool LoadGifFromDisk(string path)
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


        public static void DisposeWorkingBitmap()
        {
            if (WorkingBitmap != null)
                WorkingBitmap.Dispose();
        }
        public static void DisposeStill()
        {
            if (LoadedStillBitmap != null)
                LoadedStillBitmap.Dispose();
            if (WorkingBitmap != null)
                WorkingBitmap.Dispose();
        }

        private Bitmap GetNextFrame(Bitmap ReusableBitmap, int frameIndex, bool isPreviewRunning)
        {

            ///
            //ReusableBitmap: save memory and cpu when using old bitmap if they has the same dimension
            // FrameIndex: current frame need to retrieve from Gif image
            //isPreviewRunning: decide to update the view



            try
            {
                // get current working bitmap at frameIndex
                Bitmap CurrentGifImage;
                var currentFrame = LoadedGifImage[frameIndex];
                if (isPreviewRunning)
                {
                    MainViewViewModel.SetGifxelationPreviewImage(currentFrame);
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
                        //do the scale for current device position
                        //double scaleX = (double)currentFrame.FrameWidth / (double)ReusableBitmap.Width;
                        //double scaleY = (double)currentFrame.FrameHeight / (double)ReusableBitmap.Height;
                        //var x = (double)OutputSettings.OutputRectangle.X * scaleX;
                        //var y = (double)OutputSettings.OutputRectangle.Y * scaleX;
                        //var width = OutputSettings.OutputRectangle.Width * scaleX;
                        //var height = OutputSettings.OutputRectangle.Height * scaleY;

                        //OutputSettings.OutputRectangle = new Rectangle((int)x, (int)y, (int)width, (int)height);


                    }
                    else //this is when app start
                    {
                        CurrentGifImage = new Bitmap(currentFrame.FrameWidth, currentFrame.FrameHeight, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                        //var width = OutputSettings.OutputRectangleScaleWidth * currentFrame.FrameWidth;
                        //var height = OutputSettings.OutputRectangleScaleHeight * currentFrame.FrameHeight;
                        //var x = OutputSettings.OutputRectangleScaleLeft * currentFrame.FrameWidth;
                        //var y = OutputSettings.OutputRectangleScaleTop * currentFrame.FrameHeight;
                        //OutputSettings.OutputRectangle = new Rectangle((int)x, (int)y, (int)width, (int)height);

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
