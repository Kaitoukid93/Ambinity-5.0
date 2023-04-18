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
using System.Net;
using MathNet.Numerics.Distributions;
using ColorPalette = adrilight.Util.ColorPalette;
using SixLabors.ImageSharp;
using HandyControl.Tools.Extension;

namespace adrilight
{
    internal class Rainbow : ILightingEngine
    {
        private readonly ILogger _log = LogManager.GetCurrentClassLogger();

        public Rainbow(
            IGeneralSettings generalSettings,
            MainViewViewModel mainViewViewModel,
            IControlZone zone,
            IRainbowTicker rainbowTicker
            )
        {
            GeneralSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));
            CurrentZone = zone as LEDSetup ?? throw new ArgumentNullException(nameof(zone));
            MainViewViewModel = mainViewViewModel ?? throw new ArgumentNullException(nameof(mainViewViewModel));
            RainbowTicker = rainbowTicker ?? throw new ArgumentNullException(nameof(rainbowTicker));
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
        public bool IsRunning { get; private set; }
        private LEDSetup CurrentZone { get; }
        private MainViewViewModel MainViewViewModel { get; }
        private IRainbowTicker RainbowTicker { get; }

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
                case nameof(_colorControl):
                    Refresh();
                    break;
                case nameof(GeneralSettings.SystemRainbowSpeed):
                    OnSystemSpeedChanged(GeneralSettings.SystemRainbowSpeed);
                    break;
            }
        }

        /// <summary>
        /// private properties
        /// </summary>

        private CancellationTokenSource _cancellationTokenSource;
        private Thread _workerThread;
        private LightingMode _currentLightingMode;
        private IModeParameter _colorControl;
        private IModeParameter _brightnessControl;
        private IModeParameter _speedControl;
        private IModeParameter _systemSyncControl;
        private ColorPalette _palette;
        private Color[] _colorBank;
        private bool _isSystemSync;
        private bool _isBreathing;
        private double _brightness;
        private double _speed;


        #region Properties changed event handler 
        private void OnSystemSpeedChanged(int value)
        {
            _speedControl.Value = value;
        }

        private void OnSystemSyncValueChanged(bool value)
        {
            _isSystemSync = value;
            if (value)
            {
                //hide native speed control
                _speedControl.IsEnabled = false;

            }
            else
            {
                _speedControl.IsEnabled = true;
            }
        }
        private void OnSelectedPaletteChanged(IParameterValue value)
        {
            _palette = value as ColorPalette;
            _colorBank = GetColorGradientfromPaletteWithFixedColorPerGap(_palette.Colors, 50).ToArray();
        }

        private void OnBrightnessValueChanged(int value)
        {
            _brightness = value / 100d;
        }
        private void OnSpeedChanged(int value)
        {
            _speed = value / 5d;
        }
        #endregion

        public void Refresh()
        {

            var isRunning = _cancellationTokenSource != null;

            var currentLightingMode = CurrentZone.IsInControlGroup ? CurrentZone.MaskedControlMode as LightingMode : CurrentZone.CurrentActiveControlMode as LightingMode;

            var shouldBeRunning =
                currentLightingMode.BasedOn == LightingModeEnum.Rainbow &&
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
                #region registering params
                _currentLightingMode = currentLightingMode;
                _speedControl = _currentLightingMode.Parameters.Where(P => P.ParamType == ModeParameterEnum.Speed).FirstOrDefault();
                _colorControl = _currentLightingMode.Parameters.Where(P => P.ParamType == ModeParameterEnum.Palette).FirstOrDefault();
                _colorControl.PropertyChanged += (_, __) => OnSelectedPaletteChanged(_colorControl.SelectedValue);
                _brightnessControl = _currentLightingMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.Brightness).FirstOrDefault();
                _speedControl.PropertyChanged += (_, __) => OnSpeedChanged(_speedControl.Value);
                _brightnessControl.PropertyChanged += (_, __) => OnBrightnessValueChanged(_brightnessControl.Value);
                _systemSyncControl = _currentLightingMode.Parameters.Where(P => P.ParamType == ModeParameterEnum.IsSystemSync).FirstOrDefault();
                _systemSyncControl.PropertyChanged += (_, __) => OnSystemSyncValueChanged(_systemSyncControl.Value == 1 ? true : false);
                #endregion
                _log.Debug("starting the Static Color Engine");
                _cancellationTokenSource = new CancellationTokenSource();
                _workerThread = new Thread(() => Run(_cancellationTokenSource.Token)) {
                    IsBackground = true,
                    Priority = ThreadPriority.BelowNormal,
                    Name = "Rainbow"
                };
                _workerThread.Start();
            }


        }
        public void Run(CancellationToken token)
        {

            _log.Debug("Started Rainbow engine.");


            try
            {
                //get properties value for first time running


                _palette = _colorControl.SelectedValue as ColorPalette;
                _colorBank = GetColorGradientfromPaletteWithFixedColorPerGap(_palette.Colors, 50).ToArray();
                _brightness = _brightnessControl.Value / 100d;
                _speed = _speedControl.Value / 5d;
                double StartIndex = 0d;
                int OutputStartIndex = 0;
                while (!token.IsCancellationRequested)
                {
                    bool isPreviewRunning = MainViewViewModel.IsLiveViewOpen;

                    StartIndex += _speed;
                    if (StartIndex > GeneralSettings.SystemRainbowMaxTick)
                    {
                        StartIndex = 0;
                    }
                    if (_isSystemSync)
                    {
                        OutputStartIndex = (int)RainbowTicker.RainbowStartIndex;
                    }
                    else
                    {
                        OutputStartIndex = (int)StartIndex;
                    }

                    lock (CurrentZone.Lock)
                    {

                        int position = 0;
                        foreach (var spot in CurrentZone.Spots)
                        {

                            position = OutputStartIndex + spot.VID;
                            int n = 0;
                            if (position >= _colorBank.Length)
                                //position = Math.Abs(colorBank.Length - position);
                                n = position / _colorBank.Length;
                            position -= n * _colorBank.Length; // run with VID
                            if(spot.HasVID)
                            spot.SetColor((byte)(_brightness * _colorBank[position].R), (byte)(_brightness * _colorBank[position].G), (byte)(_brightness * _colorBank[position].B), isPreviewRunning);
                            else
                            {
                                spot.SetColor(0, 0, 0, isPreviewRunning);
                            }
                        }



                    }
                    Thread.Sleep(10);
                }
            }
            finally
            {

                _log.Debug("Stopped the Static Color Engine");
                //IsRunning = false;
                GC.Collect();
            }
        }


        public static List<Color> GetColorGradient(Color from, Color to, int totalNumberOfColors)
        {
            if (totalNumberOfColors < 2)
            {
                throw new ArgumentException("Gradient cannot have less than two colors.", nameof(totalNumberOfColors));
            }
            var colorList = new List<Color>();
            double diffA = to.A - from.A;
            double diffR = to.R - from.R;
            double diffG = to.G - from.G;
            double diffB = to.B - from.B;

            var steps = totalNumberOfColors - 1;

            var stepA = diffA / steps;
            var stepR = diffR / steps;
            var stepG = diffG / steps;
            var stepB = diffB / steps;



            for (var i = 1; i < steps; ++i)
            {
                colorList.Add(Color.FromArgb(
                     (byte)(c(from.A, stepA)),
                     (byte)(c(from.R, stepR)),
                     (byte)(c(from.G, stepG)),
                     (byte)(c(from.B, stepB))));

                int c(int fromC, double stepC)
                {
                    return (int)Math.Round(fromC + stepC * i);
                }
            }
            colorList.Add(to);
            colorList.Insert(0, from);
            return colorList;

        }
        public static IEnumerable<Color> GetColorGradientfromPalette(Color[] colorCollection, int colorNum)
        {
            var colors = new List<Color>();
            int colorPerGap = colorNum / (colorCollection.Count() - 1);

            for (int i = 0; i < colorCollection.Length - 1; i++)
            {
                var gradient = GetColorGradient(colorCollection[i], colorCollection[i + 1], colorPerGap);
                colors = colors.Concat(gradient).ToList();
            }
            int remainTick = colorNum - colors.Count();
            colors = colors.Concat(colors.Take(remainTick).ToList()).ToList();
            return colors;


            // new update, create free amount of color????
        }
        public static IEnumerable<Color> GetColorGradientfromPaletteWithFixedColorPerGap(Color[] colorCollection, int colorPerGap)
        {
            var colors = new List<Color>();


            for (int i = 0; i < colorCollection.Length - 1; i++)
            {
                var gradient = GetColorGradient(colorCollection[i], colorCollection[i + 1], colorPerGap);
                colors = colors.Concat(gradient).ToList();
            }
            var lastGradient = GetColorGradient(colorCollection[15], colorCollection[0], colorPerGap);
            colors = colors.Concat(lastGradient).ToList();
            return colors;

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


    }
}