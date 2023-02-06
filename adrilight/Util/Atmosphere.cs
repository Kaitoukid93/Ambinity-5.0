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
using adrilight.Spots;

namespace adrilight
{
    internal class Atmosphere : IAtmosphere
    {
        // public static Color[] small = new Color[30];
        public static double _huePosIndex = 0;//index for rainbow mode only
        public static double _palettePosIndex = 0;//index for other custom palette

        private readonly NLog.ILogger _log = LogManager.GetCurrentClassLogger();

        public Atmosphere(IOutputSettings outputSettings)
        {
            OutputSettings = outputSettings ?? throw new ArgumentNullException(nameof(outputSettings));
            
            //SettingsViewModel = settingsViewModel ?? throw new ArgumentNullException(nameof(settingsViewModel));
            //Remove SettingsViewmodel from construction because now we pass SpotSet Dirrectly to MainViewViewModel
            OutputSettings.PropertyChanged += PropertyChanged;
            RefreshColorState();
            _log.Info($"Atmosphere Color Created");

        }
        //Dependency Inject//
        private IOutputSettings OutputSettings { get; }
       
        public bool IsRunning { get; private set; } = false;
        private CancellationTokenSource _cancellationTokenSource;

        private void PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(OutputSettings.OutputIsEnabled):
                case nameof(OutputSettings.OutputSelectedMode):
                case nameof(OutputSettings.OutputBrightness):
                    RefreshColorState();
                    break;
            }
        }
        private void RefreshColorState()
        {

            var isRunning = _cancellationTokenSource != null && IsRunning;
            var shouldBeRunning = OutputSettings.OutputIsEnabled && OutputSettings.OutputSelectedMode == 4;
            if (isRunning && !shouldBeRunning)
            {
                //stop it!
                _log.Debug("stopping the Atmosphere Color");
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
                    Name = "AtmosphereColorCreator"
                };
                thread.Start();
            }
        }




        
        public void Run(CancellationToken token)

        {
            if (IsRunning) throw new Exception(" Atmosphere Color is already running!");

            IsRunning = true;

            _log.Debug("Started Atmosphere Color.");

            try
            {

                while (!token.IsCancellationRequested)
                {
                    double brightness = OutputSettings.OutputBrightness / 100d;
                    int paletteSource = OutputSettings.OutputSelectedChasingPalette;
                    var numLED = OutputSettings.OutputLEDSetup.Spots.Count;
                    var devicePowerVoltage = OutputSettings.OutputPowerVoltage;
                    var devicePowerMiliamps = OutputSettings.OutputPowerMiliamps;
                    //   var colorOutput = new OpenRGB.NET.Models.Color[numLED];


                    OpenRGB.NET.Models.Color[] outputColor = new OpenRGB.NET.Models.Color[numLED];
                    int counter = 0;
                    Color startColor = OutputSettings.OutputAtmosphereStartColor;
                    Color stopColor = OutputSettings.OutputAtmosphereStopColor;
                    //var newcolor = GetHueGradient(numLED, hueStart, hueStop, 1.0, 1.0);
                    //lock (DeviceSpotSet.Lock)
                    //{



                        



                    //    //now fill new color to palette output to display and send out serial


                    //    foreach (var color in newcolor)
                    //    {
                    //        outputColor[counter++] = Brightness.applyBrightness(color, brightness, DeviceSpotSet.LEDSetup.Spots.Length, devicePowerMiliamps,devicePowerVoltage);

                    //    }
                    //    counter = 0;
                    //    foreach (IDeviceSpot spot in DeviceSpotSet.LEDSetup.Spots)
                    //    {
                    //        spot.SetColor(outputColor[counter].R, outputColor[counter].G, outputColor[counter].B, true);
                    //        counter++;

                    //    }



                    //}
                    Thread.Sleep(10); //motion speed
                }
               


            }
            catch (OperationCanceledException)
            {
                _log.Debug("OperationCanceledException catched. returning.");

                // return;
            }
            catch (Exception ex)
            {
                _log.Debug(ex, "Exception catched.");




                //allow the system some time to recover
                Thread.Sleep(500);
            }
            finally
            {


                _log.Debug("Stopped Atmosphere Color Creator.");
                IsRunning = false;
            }









        }



        public static IEnumerable<OpenRGB.NET.Models.Color> GetHueGradient(int amount, double hueStart = 0, double hueStop = 360.0,
                                                               double saturation = 1.0, double value = 1.0) =>
           Enumerable.Range(0, amount)
                     .Select(i => OpenRGB.NET.Models.Color.FromHsv(hueStart + ((hueStop-hueStart) / amount * i), saturation, value));

    }
}
