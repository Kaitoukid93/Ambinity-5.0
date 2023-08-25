﻿using adrilight.Models.ControlMode.Enum;
using adrilight.Models.ControlMode.ModeParameters;
using adrilight.Util;
using adrilight.Util.ModeParameters;
using adrilight.ViewModel;
using MoreLinq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Color = System.Windows.Media.Color;

namespace adrilight
{
    internal class StaticColor : ILightingEngine
    {

        public StaticColor(
            IGeneralSettings generalSettings,
            MainViewViewModel mainViewViewModel,
            IControlZone zone,
            RainbowTicker rainbowTicker
            )
        {
            GeneralSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));
            CurrentZone = zone as LEDSetup ?? throw new ArgumentNullException(nameof(zone));
            MainViewViewModel = mainViewViewModel ?? throw new ArgumentNullException(nameof(mainViewViewModel));
            RainbowTicker = rainbowTicker ?? throw new ArgumentNullException(nameof(rainbowTicker));
            GeneralSettings.PropertyChanged += PropertyChanged;
            CurrentZone.PropertyChanged += PropertyChanged;
            MainViewViewModel.PropertyChanged += PropertyChanged;
        }

        /// <summary>
        /// dependency property
        /// </summary>
        private IGeneralSettings GeneralSettings { get; }
        public bool IsRunning { get; private set; }
        private LEDSetup CurrentZone { get; }
        private MainViewViewModel MainViewViewModel { get; }
        private RainbowTicker RainbowTicker { get; }
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
        private ToggleParameter _enableControl;

        private enum DimMode { Up, Down };
        private DimMode _dimMode;
        private double _dimFactor;

        private ColorCard _color;
        private bool _isSystemSync;
        private bool _isBreathing;
        private bool _isEnable;
        private double _brightness;
        private int _breathingSpeed;
        private Color[] _colors;
        private int _frameRate = 60;
        #region Properties changed event handler 
        private void EnableChanged(bool value)
        {
            _isEnable = value;
            if (value)
            {
                _dimMode = DimMode.Up;
                _dimFactor = 0.00;
                _currentLightingMode.Parameters.Except(new List<IModeParameter>() { _enableControl }).ForEach(p => p.IsEnabled = true);
            }
            else
            {
                _dimMode = DimMode.Down;
                _dimFactor = 1.00;
                _currentLightingMode.Parameters.Except(new List<IModeParameter>() { _enableControl }).ForEach(p => p.IsEnabled = false);
            }
        }
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
            if (value == null)
                return;
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
            if (CurrentZone.CurrentActiveControlMode == null)
            {
                return;
            }
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
                Log.Information("stopping the Static Color Engine");
                _dimMode = DimMode.Down;
                _dimFactor = 1.00;
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;

            }
            // this is start sign
            else if (!isRunning && shouldBeRunning)
            {
                //start it
                Init();
                Log.Information("starting the Static Color Engine");
                _dimMode = DimMode.Up;
                _dimFactor = 0.00;
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
            _enableControl = _currentLightingMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.IsEnabled).FirstOrDefault() as ToggleParameter;
            _enableControl.PropertyChanged += (_, __) => EnableChanged(_enableControl.Value == 1 ? true : false);
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
            EnableChanged(_enableControl.Value == 1 ? true : false);
            OnIsBreathingValueChanged(_breathingControl.Value == 1 ? true : false);
            OnSystemSyncValueChanged(_breathingControl.SubParams[2].Value == 1 ? true : false);
            OnSelectedColorValueChanged(_colorControl.SelectedValue);
            OnBreathingSpeedValueChanged(_breathingControl.SubParams[0].Value);
            OnBrightnessValueChanged(_brightnessControl.Value);
        }
        public void Run(CancellationToken token)
        {
            //wait other thread finishing
            Thread.Sleep(500);
            IsRunning = true;
            Log.Information("Static Color Engine Is Running");
            int updateIntervalCounter = 0;
            try
            {

                while (!token.IsCancellationRequested)
                {

                    if (MainViewViewModel.IsRichCanvasWindowOpen)
                    {
                        Thread.Sleep(100);
                        continue;
                    }
                    var startIndex = CurrentZone.Spots.MinBy(s => s.Index).FirstOrDefault().Index;

                    if (_isBreathing)
                    {
                        if (_isSystemSync)
                        {
                            _brightness = RainbowTicker.BreathingBrightnessValue;

                        }
                        else
                        {
                            float smoothness_pts = (float)_breathingSpeed;
                            double pwm_val = 255.0 * (1.0 - Math.Abs((2.0 * (ii++ / smoothness_pts)) - 1.0));
                            if (ii > smoothness_pts)
                                ii = 0f;

                            _brightness = pwm_val / 255d;
                        }

                    }

                    if (updateIntervalCounter > 0)
                    {

                    }
                    lock (CurrentZone.Lock)
                    {
                        DimLED();
                        foreach (var spot in CurrentZone.Spots)
                        {
                            var index = spot.Index - startIndex;
                            if (index >= _colors.Length)
                            {
                                index = 0;
                            }
                            byte colorR = 0;
                            byte colorG = 0;
                            byte colorB = 0;
                            if (_dimMode == DimMode.Down)
                            {
                                //keep same last color
                                colorR = spot.Red;
                                colorG = spot.Green;
                                colorB = spot.Blue;
                            }
                            else if (_dimMode == DimMode.Up)
                            {
                                colorR = _colors[index].R;
                                colorG = _colors[index].G;
                                colorB = _colors[index].B;
                            }
                            ApplySmoothing((float)(colorR * _brightness * _dimFactor), (float)(colorG * _brightness * _dimFactor), (float)(colorB * _brightness * _dimFactor), out byte FinalR, out byte FinalG, out byte FinalB, spot.Red, spot.Green, spot.Blue);
                            spot.SetColor(FinalR, FinalG, FinalB, false);

                        }

                    }

                    //threadSleep for static mode is 1s, for breathing is 10ms
                    var sleepTime = 1000 / _frameRate;
                    Thread.Sleep(sleepTime);
                    updateIntervalCounter++;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, this.ToString());
            }
            finally
            {

                Log.Information("Stopped the Static Color Engine");
                IsRunning = false;
                GC.Collect();
            }
        }

        private void DimLED()
        {
            if (_dimMode == DimMode.Down)
            {
                if (_dimFactor >= 0.1)
                    _dimFactor -= 0.1;
            }
            else if (_dimMode == DimMode.Up)
            {
                if (_dimFactor <= 0.99)
                    _dimFactor += 0.01;
                //_dimMode = DimMode.Down;
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
            Log.Information("Stop called for Static Color Engine");
            //CurrentZone.FillSpotsColor(Color.FromRgb(0, 0, 0));
            if (_workerThread == null) return;
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
            _workerThread?.Join();
            _workerThread = null;

        }


    }
}