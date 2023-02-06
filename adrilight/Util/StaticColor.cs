using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Controls;
using System.Threading;
using Castle.Core.Logging;
using NLog;
using adrilight.ViewModel;
using System.Diagnostics;
using adrilight.Spots;

namespace adrilight.Util
{
    internal class StaticColor : IStaticColor
    {


        private readonly NLog.ILogger _log = LogManager.GetCurrentClassLogger();

        
        public StaticColor(IOutputSettings outputSettings,IGeneralSettings generalSettings, MainViewViewModel mainViewViewModel, IRainbowTicker rainbowTicker)
        {
            OutputSettings = outputSettings ?? throw new ArgumentNullException(nameof(outputSettings));
            MainViewViewModel = mainViewViewModel ?? throw new ArgumentNullException(nameof(mainViewViewModel));
            GeneralSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));
            RainbowTicker = rainbowTicker ?? throw new ArgumentNullException(nameof(rainbowTicker));
            //SettingsViewModel = settingsViewModel ?? throw new ArgumentNullException(nameof(settingsViewModel));
            //Remove SettingsViewmodel from construction because now we pass SpotSet Dirrectly to MainViewViewModel
            OutputSettings.PropertyChanged += PropertyChanged;
            RefreshColorState();
            _log.Info($"Static Color Created");

        }


        private void PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(OutputSettings.OutputIsEnabled):
                case nameof(OutputSettings.OutputSelectedMode):
                case nameof(OutputSettings.OutputParrentIsEnable):
                    RefreshColorState();
                    break;
                case nameof(OutputSettings.OutputStaticColor):
                case nameof(OutputSettings.OutputSelectedGradient):
                    SolidColorChanged();
                    break;

            }
        }

        //DependencyInjection//
        private IOutputSettings OutputSettings { get; }
        private IGeneralSettings GeneralSettings { get; }
        private IRainbowTicker RainbowTicker { get; }
        private MainViewViewModel MainViewViewModel { get; }
        private Color[] colorBank = new Color[256];
        public bool IsRunning { get; private set; } = false;
        private CancellationTokenSource _cancellationTokenSource;
        private void RefreshColorState()
        {
            var isRunning = _cancellationTokenSource != null && IsRunning;
            var shouldBeRunning = OutputSettings.OutputIsEnabled && OutputSettings.OutputParrentIsEnable && OutputSettings.OutputSelectedMode == 3 && OutputSettings.IsInSpotEditWizard == false;
            if (isRunning && !shouldBeRunning)
            {
                //stop it!
                _log.Debug("stopping the StaticColor");
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;
            }


            else if (!isRunning && shouldBeRunning)
            {
                //start it
                _log.Debug("starting the StaticColor");
                _cancellationTokenSource = new CancellationTokenSource();
                var thread = new Thread(() => Run(_cancellationTokenSource.Token)) {
                    IsBackground = true,
                    Priority = ThreadPriority.BelowNormal,
                    Name = "StaticColorCreator"
                };
                thread.Start();
            }
        }
        private void SolidColorChanged()
        {
            var isRunning = _cancellationTokenSource != null && IsRunning;
            var shouldBeRunning = OutputSettings.OutputIsEnabled && OutputSettings.OutputParrentIsEnable && OutputSettings.OutputSelectedMode == 3 && OutputSettings.IsInSpotEditWizard == false;


            if (isRunning && shouldBeRunning)
            {
                // rainbow is running and we need to change the color bank
                var startColor = OutputSettings.OutputSelectedGradient.StartColor;
                var stopColor = OutputSettings.OutputSelectedGradient.StopColor;
                var gradientPalette = new Color[] { startColor, stopColor };
                colorBank = GetColorGradientfromPalette(gradientPalette, 256).ToArray();
                
                //if(isInEditWizard)
                //    colorBank = GetColorGradientfromPalette(DefaultColorCollection.black).ToArray();
            }

        }



        public void Run(CancellationToken token)//static color creator
        {
            int point = 0;
            if (IsRunning) throw new Exception(" Static Color is already running!");

            IsRunning = true;

            _log.Debug("Started Static Color.");

            Color defaultColor = Color.FromRgb(127, 127, 0);


            try
            {


                
                float gamma = 0.14f; // affects the width of peak (more or less darkness)
                float beta = 0.5f; // shifts the gaussian to be symmetric
                float ii = 0f;
                var startColor = OutputSettings.OutputSelectedGradient.StartColor;
                var stopColor = OutputSettings.OutputSelectedGradient.StopColor;
                var gradientPalette = new Color[] { startColor, stopColor };
                colorBank = GetColorGradientfromPalette(gradientPalette, 256).ToArray();
                while (!token.IsCancellationRequested)
                {
                    var numLED = OutputSettings.OutputLEDSetup.Spots.Count;
                    Color currentStaticColor = OutputSettings.OutputStaticColor;
                    var colorOutput = new OpenRGB.NET.Models.Color[numLED];
                    double peekBrightness = 0.0;
                    int breathingSpeed = OutputSettings.OutputBreathingSpeed;
                    var outputPowerVoltage = OutputSettings.OutputPowerVoltage;
                    var outputPowerMiliamps = OutputSettings.OutputPowerMiliamps;
                    bool outputIsSelected = false;
                    var currentOutput = MainViewViewModel.CurrentOutput;
                  
                    if (currentOutput != null && currentOutput.OutputUniqueID == OutputSettings.OutputUniqueID)
                        outputIsSelected = true;
                    bool isPreviewRunning = MainViewViewModel.IsSplitLightingWindowOpen && outputIsSelected;

                    
                    lock (OutputSettings.OutputLEDSetup.Lock)
                    {
                        switch(OutputSettings.OutputStaticColorMode)
                        {
                            case 0:
                                foreach (var spot in OutputSettings.OutputLEDSetup.Spots)
                                {
                                    var brightness = OutputSettings.OutputBrightness / 100d;
                                    var newColor = new OpenRGB.NET.Models.Color(currentStaticColor.R, currentStaticColor.G, currentStaticColor.B);
                                    var outputColor = Brightness.applyBrightness(newColor, brightness, numLED, outputPowerMiliamps, outputPowerVoltage);
                                    if (!OutputSettings.IsInSpotEditWizard)
                                        spot.SetColor(outputColor.R, outputColor.G, outputColor.B, isPreviewRunning);

                                }
                                break;
                            case 1:

                                var breathingbrightness = 1.0d;
                                if (OutputSettings.OutputIsSystemSync)
                                {
                                    breathingbrightness = RainbowTicker.BreathingBrightnessValue;

                                }
                                else
                                {
                                    float smoothness_pts = (float)OutputSettings.OutputBreathingSpeed;
                                    double pwm_val = 255.0 * (Math.Exp(-(Math.Pow(((ii++ / smoothness_pts) - beta) / gamma, 2.0)) / 2.0));
                                    if (ii > smoothness_pts)
                                        ii = 0f;

                                    breathingbrightness = pwm_val / 255d;
                                }
                                foreach (var spot in OutputSettings.OutputLEDSetup.Spots)
                                {
                                  
                                   
                                    var newColor = new OpenRGB.NET.Models.Color(currentStaticColor.R, currentStaticColor.G, currentStaticColor.B);
                                    var outputColor = Brightness.applyBrightness(newColor, breathingbrightness, numLED, outputPowerMiliamps, outputPowerVoltage);
                                    ApplySmoothing(outputColor.R, outputColor.G, outputColor.B, out byte FinalR, out byte FinalG, out byte FinalB, spot.Red, spot.Green, spot.Blue);
                                    if (!OutputSettings.IsInSpotEditWizard)
                                        spot.SetColor(FinalR, FinalG, FinalB, isPreviewRunning);
                                }
                                break;
                            case 2:

                                foreach (var spot in OutputSettings.OutputLEDSetup.Spots)
                                {
                                    var position = spot.VID;
                                    
                                    int n = 0;
                                    if (position >= colorBank.Length)
                                        //position = Math.Abs(colorBank.Length - position);
                                        n = position / colorBank.Length;
                                    position -= n * colorBank.Length; // run with VID
                                    var brightness = OutputSettings.OutputBrightness / 100d;                                   
                                    var newColor = new OpenRGB.NET.Models.Color(colorBank[position].R, colorBank[position].G, colorBank[position].B);
                                    var outputColor = Brightness.applyBrightness(newColor, brightness, numLED, outputPowerMiliamps, outputPowerVoltage);
                                    if (!OutputSettings.IsInSpotEditWizard)
                                        spot.SetColor(outputColor.R, outputColor.G, outputColor.B, isPreviewRunning);
                                }


                                break;
                        }
                      
                    }


                    Thread.Sleep(5);




                }
                //motion speed

            }


            catch (OperationCanceledException)
            {
                _log.Debug("OperationCanceledException catched. returning.");

                return;
            }
            catch (Exception ex)
            {
                _log.Debug(ex, "Exception catched.");
       
                Thread.Sleep(500);
            }
            finally
            {


                _log.Debug("Stopped Static Color Creator.");
                IsRunning = false;
            }

        }


        private void ApplySmoothing(float r, float g, float b, out byte semifinalR, out byte semifinalG, out byte semifinalB,
  byte lastColorR, byte lastColorG, byte lastColorB)
        {
            ;

            semifinalR = (byte)((r + 7 * lastColorR) / (7 + 1));
            semifinalG = (byte)((g + 7 * lastColorG) / (7 + 1));
            semifinalB = (byte)((b + 7 * lastColorB) / (7 + 1));
        }

        public static IEnumerable<Color> GetColorGradient(Color from, Color to, int totalNumberOfColors)
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
        }

    }
}