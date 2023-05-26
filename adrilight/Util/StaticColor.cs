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
using MoreLinq;
using SharpDX.Direct2D1.Effects;
using adrilight.Util.ModeParameters;

namespace adrilight
{
    internal class StaticColor : ILightingEngine
    {
        private readonly ILogger _log = LogManager.GetCurrentClassLogger();

        public StaticColor(
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
           // Refresh();
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
        public LightingModeEnum Type { get; } = LightingModeEnum.StaticColor;
        /// <summary>
        /// breathing constant value
        /// </summary>
        float gamma = 0.14f; // affects the width of peak (more or less darkness)
        float beta = 0.5f; // shifts the gaussian to be symmetric
        float ii = 0f;

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
                    var isRunning = _cancellationTokenSource != null;
                    if (isRunning || (CurrentZone.CurrentActiveControlMode as LightingMode).BasedOn == Type)
                        Refresh();
                    break;
                //case nameof(MainViewViewModel.IsRichCanvasWindowOpen):
                ////case nameof(MainViewViewModel.IsRegisteringGroup):
                ////case nameof(_colorControl):
                //    Refresh();
                  //  break;
                case nameof(GeneralSettings.BreathingSpeed):
                    OnSystemBreathingSpeedChanged(GeneralSettings.BreathingSpeed);
                    break;
            }
        }

        /// <summary>
        /// private properties
        /// </summary>

        private CancellationTokenSource _cancellationTokenSource;
        private Thread _workerThread;
        private LightingMode _currentLightingMode;

        private ListSelectionParameter _colorControl;
        private ToggleParameter _breathingControl;
        private SliderParameter _brightnessControl;

        private ColorCard _color;
        private bool _isSystemSync;
        private bool _isBreathing;
        private double _brightness;
        private int _breathingSpeed;
        private Color[] _colors;
        private int _displayUpdateRate = 25;
        private int _frameRate = 60;
        #region Properties changed event handler 
        private void OnSystemBreathingSpeedChanged(int value)
        {
            if (_breathingControl != null)
            {
                _breathingControl.SubParams[1].Value = value;
            }
        }

        private void OnSystemSyncValueChanged(bool value)
        {
            _isSystemSync = value;
            if (value)
            {
                _breathingControl.SubParams[0].IsEnabled = false;
                _breathingControl.SubParams[1].IsEnabled = true;
                _breathingControl.SubParams[1].Value = GeneralSettings.BreathingSpeed;

            }
            else
            {
                _breathingControl.SubParams[0].IsEnabled = true;
                _breathingControl.SubParams[1].IsEnabled = false;

            }
        }
        private void OnSelectedColorValueChanged(IParameterValue value)
        {
            var colorCard = value as ColorCard;
            int numColors;
            if (CurrentZone.Spots.Count < 2)
                numColors = 2;
            else
            {
                numColors = CurrentZone.Spots.Count();
            }
            _colors = GetColorGradient(colorCard.StartColor, colorCard.StopColor, numColors).ToArray();
        }
        private void OnIsBreathingValueChanged(bool value)
        {
            _isBreathing = value;
            if (!value)
            {
                _brightness = _brightnessControl.Value / 100d;
                _brightnessControl.IsEnabled = true;
            }
            else
            {
                _brightnessControl.IsEnabled = false;
            }
        }
        private void OnBrightnessValueChanged(int value)
        {
            _brightness = value / 100d;
        }
        private void OnBreathingSpeedValueChanged(int value)
        {
            _breathingSpeed = 2000 - value;
        }
        private void OnSystemSyncBreathingSpeedValueChange(int value)
        {
            GeneralSettings.BreathingSpeed = value;
        }

        #endregion

        public void Refresh()
        {

            var isRunning = _cancellationTokenSource != null;

             _currentLightingMode = CurrentZone.CurrentActiveControlMode as LightingMode;

            var shouldBeRunning =
                _currentLightingMode.BasedOn == LightingModeEnum.StaticColor &&
                //this zone has to be enable, this could be done by stop setting the spots, but the this thread still alive, so...
                CurrentZone.IsEnabled == true &&
                //stop this engine when any surface or editor open because this could cause capturing fail
                MainViewViewModel.IsRichCanvasWindowOpen == false;
                ////registering group shoud be done
                //MainViewViewModel.IsRegisteringGroup == false;

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
                Init();
                _log.Debug("starting the Static Color Engine");
                _cancellationTokenSource = new CancellationTokenSource();
                _workerThread = new Thread(() => Run(_cancellationTokenSource.Token)) {
                    IsBackground = true,
                    Priority = ThreadPriority.BelowNormal,
                    Name = "StaticColor"
                };
                _workerThread.Start();
            }
            else if (isRunning && shouldBeRunning)
            {
                Init();
            }

        }
        public void Init()
        {
            #region registering params
            _colorControl = _currentLightingMode.Parameters.Where(P => P.ParamType == ModeParameterEnum.Color).FirstOrDefault() as ListSelectionParameter;
            _colorControl.PropertyChanged += (_, __) =>
            {
                switch (__.PropertyName)
                {
                    case nameof(_colorControl.SelectedValue):
                        OnSelectedColorValueChanged(_colorControl.SelectedValue);
                        break;
                }
            };
            _breathingControl = _currentLightingMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.Breathing).FirstOrDefault() as ToggleParameter;
            _brightnessControl = _currentLightingMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.Brightness).FirstOrDefault() as SliderParameter;
            _brightnessControl.PropertyChanged += (_, __) => OnBrightnessValueChanged(_brightnessControl.Value);
            _breathingControl.PropertyChanged += (_, __) => OnIsBreathingValueChanged(_breathingControl.Value == 1 ? true : false);
            _breathingControl.SubParams[0].PropertyChanged += (_, __) => OnBreathingSpeedValueChanged(_breathingControl.SubParams[0].Value);
            _breathingControl.SubParams[2].PropertyChanged += (_, __) => OnSystemSyncValueChanged(_breathingControl.SubParams[2].Value == 1 ? true : false);
            _breathingControl.SubParams[1].PropertyChanged += (_, __) => OnSystemSyncBreathingSpeedValueChange(_breathingControl.SubParams[1].Value);
            _colorControl.LoadAvailableValues();
            #endregion
            //safety check
            if (_colorControl.SelectedValue == null)
            {
                _colorControl.SelectedValue = _colorControl.AvailableValues.First();
            }
            OnIsBreathingValueChanged(_breathingControl.Value == 1 ? true : false);
            OnSystemSyncValueChanged(_breathingControl.SubParams[2].Value == 1 ? true : false);
            OnSelectedColorValueChanged(_colorControl.SelectedValue);
            OnBreathingSpeedValueChanged(_breathingControl.SubParams[0].Value);
            OnBrightnessValueChanged(_brightnessControl.Value);
        }
        public void Run(CancellationToken token)
        {

            _log.Debug("Started Static Color.");

            IsRunning = true;
            int updateIntervalCounter = 0;
            try
            {     
                
                while (!token.IsCancellationRequested)
                {
                    var startIndex = CurrentZone.Spots.MinBy(s => s.Index).FirstOrDefault().Index;
                    bool shouldViewUpdate = MainViewViewModel.IsLiveViewOpen && MainViewViewModel.IsAppActivated && updateIntervalCounter > _frameRate/_displayUpdateRate ;
                    if(shouldViewUpdate)
                        updateIntervalCounter = 0;
                    if (_isBreathing)
                    {
                        if (_isSystemSync)
                        {
                            _brightness = RainbowTicker.BreathingBrightnessValue;

                        }
                        else
                        {
                            float smoothness_pts = (float)_breathingSpeed;
                            double pwm_val = 255.0 * (Math.Exp(-(Math.Pow(((ii++ / smoothness_pts) - beta) / gamma, 2.0)) / 2.0));
                            if (ii > smoothness_pts)
                                ii = 0f;

                            _brightness = pwm_val / 255d;
                        }

                    }
                    
                    if( updateIntervalCounter > 0 )
                    {

                    }
                    lock (CurrentZone.Lock)
                    {
                        foreach(var spot in CurrentZone.Spots)
                        {
                            ApplySmoothing(_colors[spot.Index - startIndex].R, _colors[spot.Index - startIndex].G, _colors[spot.Index - startIndex].B, out byte FinalR, out byte FinalG, out byte FinalB, spot.Red, spot.Green, spot.Blue);
                            spot.SetColor((byte)(_brightness * FinalR), (byte)(_brightness * FinalG), (byte)(_brightness * FinalB), false);
                        }
   
                    }

                    //threadSleep for static mode is 1s, for breathing is 10ms
                    var sleepTime = 1000 / _frameRate;
                    Thread.Sleep(sleepTime);
                    updateIntervalCounter++;
                }
            }
            finally
            {

                _log.Debug("Stopped the Static Color Engine");
                IsRunning = false;
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