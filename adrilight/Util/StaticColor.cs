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
using adrilight.Helpers;
using adrilight.Settings;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Header;
using NAudio.SoundFont;
using Color = System.Windows.Media.Color;

namespace adrilight
{
    internal class StaticColor : ILightingEngine
    {
        private readonly ILogger _log = LogManager.GetCurrentClassLogger();

        public StaticColor(
            IGeneralSettings generalSettings,
            IDesktopFrame[] desktopFrame,
            MainViewViewModel mainViewViewModel,
            IControlZone zone
            )
        {
            GeneralSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));
            DesktopFrame = desktopFrame ?? throw new ArgumentNullException(nameof(desktopFrame));
            CurrentZone = zone as LEDSetup ?? throw new ArgumentNullException(nameof(zone));
            MainViewViewModel = mainViewViewModel ?? throw new ArgumentNullException(nameof(mainViewViewModel));


            GeneralSettings.PropertyChanged += PropertyChanged;
            CurrentZone.PropertyChanged += PropertyChanged;
            MainViewViewModel.PropertyChanged += PropertyChanged;


            Refresh();

            _log.Info($"DesktopDuplicatorReader created.");
        }




        /// <summary>
        /// dependency property
        /// </summary>
        private IGeneralSettings GeneralSettings { get; }

        private IDesktopFrame[] DesktopFrame { get; }
        public bool IsRunning { get; private set; }
        private LEDSetup CurrentZone { get; }
        private MainViewViewModel MainViewViewModel { get; }



        /// <summary>
        /// property changed event catching
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                //which property that require this engine to refresh
                case nameof(CurrentZone.CurrentActiveControlMode):
                case nameof(CurrentZone.IsInControlGroup):
                case nameof(CurrentZone.MaskedControlMode):
                case nameof(MainViewViewModel.IsRichCanvasWindowOpen):
                case nameof(MainViewViewModel.IsRegisteringGroup):
                    Refresh();
                    break;

            }
        }

        /// <summary>
        /// private properties
        /// </summary>

        private CancellationTokenSource _cancellationTokenSource;
        private Thread _workerThread;
        private LightingMode _currentLightingMode;
        private int? _currentScreenIndex;


        public void Refresh()
        {

            var isRunning = _cancellationTokenSource != null;

            var currentLightingMode = CurrentZone.IsInControlGroup ? CurrentZone.MaskedControlMode as LightingMode : CurrentZone.CurrentActiveControlMode as LightingMode;

            var shouldBeRunning =
                currentLightingMode.BasedOn == LightingModeEnum.StaticColor &&
                //this zone has to be enable, this could be done by stop setting the spots, but the this thread still alive, so...
                CurrentZone.IsEnabled == true &&
                //stop this engine when any surface or editor open because this could cause capturing fail
                MainViewViewModel.IsRichCanvasWindowOpen == false &&
                //registering group shoud be done
                MainViewViewModel.IsRegisteringGroup == false;

            // this is stop sign by one or some of the reason above
            if (isRunning && !shouldBeRunning)
            {
                //stop it!
                _log.Debug("stopping the Static Color Engine");
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;

            }
            // this is start sign
            else if (!isRunning && shouldBeRunning)
            {
                //start it
                //get current lighting mode confirm that based on desktop duplicator reader engine

                _currentLightingMode = currentLightingMode;


                _log.Debug("starting the Static Color Engine");
                _cancellationTokenSource = new CancellationTokenSource();
                _workerThread = new Thread(() => Run(_cancellationTokenSource.Token)) {
                    IsBackground = true,
                    Priority = ThreadPriority.BelowNormal,
                    Name = "StaticColor"
                };
                _workerThread.Start();
            }
        }
        public void Run(CancellationToken token)
        {

            _log.Debug("Started Desktop Duplication Reader.");
            Bitmap image = null;
            BitmapData bitmapData = new BitmapData();

            try
            {
                //get dependency properties from current lighting mode(based on screencapturing)
                var brightnessControl = _currentLightingMode.Parameters.Where(p => p.Type == ModeParameterEnum.Brightness).FirstOrDefault();
                var colorControl = _currentLightingMode.Parameters.Where(P=>P.Type == ModeParameterEnum.Color).FirstOrDefault();
                var currentStaticModeControl = _currentLightingMode.Parameters.Where(p => p.Type == ModeParameterEnum.StaticColorMode).FirstOrDefault();

                while (!token.IsCancellationRequested)
                {

                    //changing parameter on the fly define here
                    var brightness = brightnessControl.Value / 100d;


                    lock (CurrentZone.Lock)
                    {


                        Parallel.ForEach(CurrentZone.Spots
                            , spot =>
                            {


                            });
                        //}

                    }

                    image.UnlockBits(bitmapData);
                    //threadSleep for static mode is 1s, for breathing is 10ms
                    Thread.Sleep(10);
                }
            }
            finally
            {
                image?.Dispose();

                _log.Debug("Stopped the Static Color Engine");
                //IsRunning = false;
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
            int smoothingFactor = 2;
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
                if (_currentScreenIndex >= DesktopFrame.Length)
                {
                    HandyControl.Controls.MessageBox.Show("màn hình không khả dụng", "Sáng theo màn hình", MessageBoxButton.OK, MessageBoxImage.Error);
                    _currentScreenIndex = 0;
                }
                CurrentFrame = DesktopFrame[(int)_currentScreenIndex].Frame;
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