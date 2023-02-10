using adrilight.Spots;
using adrilight.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;

namespace adrilight.Settings
{
    internal class DefaulOutputCollection
    {
      
        public static List<OutputSettings> AvailableDefaultOutputsForAmbinoDevices {
            get
            {
                return new List<OutputSettings> { AmbinoBasic(0, "LED Màn hình 24", "24inch","adrilight.AmbinoFactoryValue.ABBasic24.json"),
                                                   AmbinoBasic(0, "LED Màn hình 27", "27inch","adrilight.AmbinoFactoryValue.ABBasic27.json"),
                                                   AmbinoBasic(0, "LED Màn hình 29", "29inch","adrilight.AmbinoFactoryValue.ABBasic29.json"),
                                                   AmbinoBasic(0, "LED Màn hình 32", "32inch","adrilight.AmbinoFactoryValue.ABBasic32.json"),
                                                   AmbinoBasic(0, "LED Màn hình 34", "34inch","adrilight.AmbinoFactoryValue.ABBasic34.json"),
                                                   AmbinoEdge(0, "LED Cạnh Bàn 1M2", "ledstrip", "adrilight.AmbinoFactoryValue.ABEDGE1M2.json"),
                                                   AmbinoEdge(0, "LED Cạnh Bàn 2M", "ledstrip", "adrilight.AmbinoFactoryValue.ABEDGE2M.json"),
                                                    AmbinoA1(0, "Ambino Infinifan", "aba1", "adrilight.AmbinoFactoryValue.ABA1.json"),
                                                    GenericFan("Generic Fan", 0, 5,5,true),
        };
            }
        }
        private static LEDSetup ReadFactoryLEDSetup (string resourceName)
        {
            LEDSetup ledSetup = new LEDSetup();
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream resource = assembly.GetManifestResourceStream(resourceName))
            {
                if (resource == null)
                {
                    throw new ArgumentException("No such resource", "resourceName");
                }
                using (StreamReader reader = new StreamReader(resource))
                {
                    string json = reader.ReadToEnd(); //Make string equal to full file
                    try
                    {
                        ledSetup = JsonConvert.DeserializeObject<LEDSetup>(json, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
                    }
                    catch (Exception)
                    {
                        HandyControl.Controls.MessageBox.Show("Corrupted or incompatible data File!!!", "LEDSetup Import", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            return ledSetup;
        }
        public static OutputSettings AmbinoBasic(int id, string name, string geometry, string resource)
        {

            var ledSetup = ReadFactoryLEDSetup (resource);  
            var outputSettings = new OutputSettings { //24 inch led frame for Ambino Basic
                OutputName = name,
                TargetDevice = "ABBASIC",
                Geometry = geometry,
                OutputID = id,
                OutputType = "Frame",
                //OutputRectangle = new System.Drawing.Rectangle(0, 0, 20 * numLEDX, 20 * numLEDY),
                OutputUniqueID = "",
                OutputRGBLEDOrder = "GRB",
                OutputIsVisible = true,
                OutputBrightness = 80,
                OutputPowerVoltage = 5,
                OutputPowerMiliamps = 900,
                OutputSaturationThreshold = 10,
                OutputUseLinearLighting = true,
                OutputIsEnabled = true,
                OutputAtmosphereStartColor = Color.FromRgb(255, 0, 0),
                OutputAtmosphereStopColor = Color.FromRgb(255, 0, 0),
                OutputAtmosphereMode = "Dirrect",
                OutputSelectedMusicMode = 0,
                OutputSelectedMusicPalette = 0,
                OutputSelectedMode = 1,
                OutputSelectedAudioDevice = 0,
                OutputSelectedDisplay = 0,
                OutputSelectedChasingPalette = 0,
                OutputPaletteSpeed = 1,
                OutputPaletteBlendStep = 16,
                OutputStaticColor = Color.FromRgb(0, 255, 0),
                OutputBreathingSpeed = 20000,
                LEDPerLED = 2,
                OutputCurrentActivePalette = new ColorPalette("Full Rainbow", "Zooey", "RGBPalette16", "Full Color Spectrum", DefaultColorCollection.rainbow),
                OutputLEDSetup = ledSetup
            // create ledsetup if neccesary

        };
            return outputSettings;
        }

        public static OutputSettings AmbinoEdge(int id, string name, string geometry, string resource)
        {
            var ledSetup = ReadFactoryLEDSetup(resource);
            var outputSettings = new OutputSettings { //24 inch led frame for Ambino Basic
                OutputName = name,
                TargetDevice = "ABEDGE",
                Geometry = geometry,
                OutputID = id,
                OutputType = "Strip",            
                OutputUniqueID = "",
                OutputRGBLEDOrder = "GRB",
                OutputIsVisible = true,
                OutputBrightness = 80,
                OutputPowerVoltage = 5,
                OutputPowerMiliamps = 900,
                OutputSaturationThreshold = 10,
                OutputUseLinearLighting = true,
                OutputIsEnabled = true,
                OutputAtmosphereStartColor = Color.FromRgb(255, 0, 0),
                OutputAtmosphereStopColor = Color.FromRgb(255, 0, 0),
                OutputAtmosphereMode = "Dirrect",
                OutputSelectedMusicMode = 0,
                OutputSelectedMusicPalette = 0,
                OutputSelectedMode = 1,
                OutputSelectedAudioDevice = 0,
                OutputSelectedDisplay = 0,
                OutputSelectedChasingPalette = 0,
                OutputPaletteSpeed = 1,
                OutputPaletteBlendStep = 16,
                OutputStaticColor = Color.FromRgb(0, 255, 0),
                OutputBreathingSpeed = 20000,
                LEDPerLED = 2,
                OutputCurrentActivePalette = new ColorPalette("Full Rainbow", "Zooey", "RGBPalette16", "Full Color Spectrum", DefaultColorCollection.rainbow),
                OutputLEDSetup = ledSetup
                // create ledsetup if neccesary

            };
            return outputSettings;
        }

        public static OutputSettings GenericLEDStrip(int id, int numLED, string name, int ledPerSpot, bool isEnabled, string geometry)
        {

            var outputSettings = new OutputSettings { //24 inch led frame for Ambino Basic
                OutputName = name,
                OutputID = id,
                Geometry = geometry,
                OutputType = "Strip",
 
                //OutputRectangle = new System.Drawing.Rectangle(0, 0, 20*numLED, 20),
                OutputUniqueID = "",
                OutputRGBLEDOrder = "GRB",
                OutputIsVisible = true,
                OutputBrightness = 80,
                OutputPowerVoltage = 5,
                OutputPowerMiliamps = 200,
                OutputSaturationThreshold = 10,
                OutputUseLinearLighting = true,
                OutputIsEnabled = isEnabled,
                OutputAtmosphereStartColor = Color.FromRgb(255, 0, 0),
                OutputAtmosphereStopColor = Color.FromRgb(255, 0, 0),
                OutputAtmosphereMode = "Dirrect",
                OutputSelectedMusicMode = 0,
                OutputSelectedMusicPalette = 0,
                OutputSelectedMode = 1,
                OutputSelectedAudioDevice = 0,
                OutputSelectedDisplay = 0,
                OutputSelectedChasingPalette = 0,
                OutputPaletteSpeed = 1,
                OutputPaletteBlendStep = 16,
                OutputStaticColor = Color.FromRgb(0, 255, 0),
                OutputBreathingSpeed = 20000,
                LEDPerSpot = ledPerSpot,
                OutputCurrentActivePalette = new ColorPalette("Full Rainbow", "Zooey", "RGBPalette16", "Full Color Spectrum", DefaultColorCollection.rainbow),
                OutputLEDSetup = BuildLEDSetup(numLED, 1, "Strip", id, 20 * numLED, 20, "Strip")
                // create ledsetup if neccesary

            };
            return outputSettings;
        }
        public static OutputSettings GenericLEDMatrix(int id, int numLEDX, int numLEDY, string name, int ledPerSpot, bool isEnabled, string geometry)
        {

            var outputSettings = new OutputSettings { //24 inch led frame for Ambino Basic
                OutputName = name,
                Geometry = geometry,
                OutputID = id,
                OutputType = "Matrix",

                //OutputRectangle = new System.Drawing.Rectangle(0, 0, 20 * numLEDX, 20 * numLEDY),
                OutputUniqueID = "",
                OutputRGBLEDOrder = "GRB",
                OutputIsVisible = true,
                OutputBrightness = 80,
                OutputPowerVoltage = 5,
                OutputPowerMiliamps = 900,
                OutputSaturationThreshold = 10,
                OutputUseLinearLighting = true,
                OutputIsEnabled = isEnabled,
                OutputAtmosphereStartColor = Color.FromRgb(255, 0, 0),
                OutputAtmosphereStopColor = Color.FromRgb(255, 0, 0),
                OutputAtmosphereMode = "Dirrect",
                OutputSelectedMusicMode = 0,
                OutputSelectedMusicPalette = 0,
                OutputSelectedMode = 1,
                OutputSelectedAudioDevice = 0,
                OutputSelectedDisplay = 0,
                OutputSelectedChasingPalette = 0,
                OutputPaletteSpeed = 1,
                OutputPaletteBlendStep = 16,
                OutputStaticColor = Color.FromRgb(0, 255, 0),
                OutputBreathingSpeed = 20000,
                LEDPerSpot = ledPerSpot,
                OutputCurrentActivePalette = new ColorPalette("Full Rainbow", "Zooey", "RGBPalette16", "Full Color Spectrum", DefaultColorCollection.rainbow),
                OutputLEDSetup = BuildLEDSetup(numLEDX, numLEDY, "Frame", id, 20 * numLEDX, 20 * numLEDY, "Matrix")
                // create ledsetup if neccesary

            };
            return outputSettings;
        }
        public static OutputSettings GenericRectangle(string name, int id, int numLEDX, int numLEDY)
        {

            var outputSettings = new OutputSettings { //24 inch led frame for Ambino Basic
                OutputName = name,
                OutputID = id,
                OutputType = "Frame",
                //OutputRectangle = new System.Drawing.Rectangle(0, 0, 20 * numLEDX, 20 * numLEDY),
                OutputUniqueID = "",
                OutputRGBLEDOrder = "GRB",
                OutputIsVisible = true,
                OutputBrightness = 80,
                OutputPowerVoltage = 5,
                OutputPowerMiliamps = 900,
                OutputSaturationThreshold = 10,
                OutputUseLinearLighting = true,
                OutputIsEnabled = true,
                OutputAtmosphereStartColor = Color.FromRgb(255, 0, 0),
                OutputAtmosphereStopColor = Color.FromRgb(255, 0, 0),
                OutputAtmosphereMode = "Dirrect",
                OutputSelectedMusicMode = 0,
                OutputSelectedMusicPalette = 0,
                OutputSelectedMode = 1,
                OutputSelectedAudioDevice = 0,
                OutputSelectedDisplay = 0,
                OutputSelectedChasingPalette = 0,
                OutputPaletteSpeed = 1,
                OutputPaletteBlendStep = 16,
                OutputStaticColor = Color.FromRgb(0, 255, 0),
                OutputBreathingSpeed = 20000,
                OutputCurrentActivePalette = new ColorPalette("Full Rainbow", "Zooey", "RGBPalette16", "Full Color Spectrum", DefaultColorCollection.rainbow),
                OutputLEDSetup = BuildLEDSetup(numLEDX, numLEDY, "Frame", id, 20 * numLEDX, 20 * numLEDY, "Frame")
                // create ledsetup if neccesary

            };
            return outputSettings;
        }
        public static OutputSettings GenericFan(string name, int id, int numLEDX, int numLEDY, bool isEnabled)
        {

            var outputSettings = new OutputSettings { //24 inch led frame for Ambino Basic
                OutputName = name,
                OutputID = id,
                OutputType = "Frame",
                TargetDevice = "ABFANHUB",
                //OutputRectangle = new System.Drawing.Rectangle(0, 0, 20 * numLEDX, 20 * numLEDY),
                OutputUniqueID = "",
                OutputRGBLEDOrder = "GRB",
                OutputIsVisible = true,
                Geometry = "Fan",
                OutputBrightness = 80,
                OutputPowerVoltage = 5,
                OutputPowerMiliamps = 1500,
                OutputSaturationThreshold = 10,
                OutputUseLinearLighting = true,
                OutputIsEnabled = isEnabled,
                OutputAtmosphereStartColor = Color.FromRgb(255, 0, 0),
                OutputAtmosphereStopColor = Color.FromRgb(255, 0, 0),
                OutputAtmosphereMode = "Dirrect",
                OutputSelectedMusicMode = 0,
                OutputSelectedMusicPalette = 0,
                OutputSelectedMode = 1,
                OutputSelectedAudioDevice = 0,
                OutputSelectedDisplay = 0,
                OutputSelectedChasingPalette = 0,
                OutputPaletteSpeed = 1,
                OutputPaletteBlendStep = 16,
                OutputStaticColor = Color.FromRgb(0, 255, 0),
                OutputBreathingSpeed = 20000,
                OutputCurrentActivePalette = new ColorPalette("Full Rainbow", "Zooey", "RGBPalette16", "Full Color Spectrum", DefaultColorCollection.rainbow),
                OutputLEDSetup = BuildLEDSetup(numLEDX, numLEDY, "Frame", id, 20 * numLEDX, 20 * numLEDY, "Frame")
                // create ledsetup if neccesary

            };
            return outputSettings;
        }
        public static OutputSettings AmbinoA1(int id, string name, string geometry, string resource)
        {
            var ledSetup = ReadFactoryLEDSetup(resource);
            var outputSettings = new OutputSettings { //24 inch led frame for Ambino Basic
                OutputName = name,
                TargetDevice = "ABFANHUB",
                Geometry = geometry,
                OutputID = id,
                OutputType = "Fan",
                OutputUniqueID = "",
                OutputRGBLEDOrder = "GRB",
                OutputIsVisible = true,
                OutputBrightness = 80,
                OutputPowerVoltage = 5,
                OutputPowerMiliamps = 1500,
                OutputSaturationThreshold = 10,
                OutputUseLinearLighting = true,
                OutputIsEnabled = true,
                OutputAtmosphereStartColor = Color.FromRgb(255, 0, 0),
                OutputAtmosphereStopColor = Color.FromRgb(255, 0, 0),
                OutputAtmosphereMode = "Dirrect",
                OutputSelectedMusicMode = 0,
                OutputSelectedMusicPalette = 0,
                OutputSelectedMode = 1,
                OutputSelectedAudioDevice = 0,
                OutputSelectedDisplay = 0,
                OutputSelectedChasingPalette = 0,
                OutputPaletteSpeed = 1,
                OutputPaletteBlendStep = 16,
                OutputStaticColor = Color.FromRgb(0, 255, 0),
                OutputBreathingSpeed = 20000,
                LEDPerLED = 1,
                OutputCurrentActivePalette = new ColorPalette("Full Rainbow", "Zooey", "RGBPalette16", "Full Color Spectrum", DefaultColorCollection.rainbow),
                OutputLEDSetup = ledSetup
                // create ledsetup if neccesary

            };
            return outputSettings;
        }
        private static ILEDSetup BuildLEDSetup(int matrixWidth, int matrixHeight, string name, int setupID, double width, double height, string outputType) // general settings is for compare each device setting
        {

            string owner = "Ambino";
            string description = "Default LED Setup for Ambino Basic Rev 2";
            string type = "ABRev2";



            List<IDeviceSpot> reorderedSpots = new List<IDeviceSpot>();

            //Create default spot
            var availableSpots = BuildMatrix(width, height, matrixWidth, matrixHeight);
            int counter = 0;


            switch (outputType)
            {
                case "Frame":
                    for (var i = 0; i < matrixHeight; i++) // bottom right ( default ambino basic start point) go up to top right
                    {
                        var spot = availableSpots[availableSpots.Length - 1 - matrixWidth * i];
                        spot.IsActivated = true;
                        spot.id = counter++;
                        reorderedSpots.Add(spot);
                    }
                    for (var i = 0; i < matrixWidth - 1; i++) // top right go left to top left
                    {
                        var spot = availableSpots[matrixWidth - 2 - i];
                        spot.IsActivated = true;
                        spot.id = counter++;
                        reorderedSpots.Add(spot);
                    }
                    for (var i = 0; i < matrixHeight - 1; i++) // top left go down to bottom left
                    {
                        var spot = availableSpots[matrixWidth * (i + 1)];
                        spot.IsActivated = true;
                        spot.id = counter++;
                        reorderedSpots.Add(spot);
                    }
                    for (var i = 0; i < matrixWidth - 2; i++) // top left go down to bottom left
                    {
                        var spot = availableSpots[matrixWidth * (matrixHeight - 1) + i + 1];
                        spot.IsActivated = true;
                        spot.id = counter++;
                        reorderedSpots.Add(spot);

                    }
                    break;
                case "Matrix":
                    foreach (var spot in availableSpots)
                    {
                        spot.IsActivated = true;
                        reorderedSpots.Add(spot);
                    }
                    break;
                case "Strip":
                    foreach (var spot in availableSpots)
                    {
                        if (spot.YIndex == 0)
                            spot.IsActivated = true;

                        reorderedSpots.Add(spot);
                    }
                    break;


            }

            counter = 0;

            ObservableCollection<IDeviceSpot> reorderedActiveSpots = new ObservableCollection<IDeviceSpot>();

            foreach (var spot in reorderedSpots)
            {
                spot.SetVID(spot.VID * (256 / reorderedSpots.Count()));
                reorderedActiveSpots.Add(spot);
            }
            var screenWidth = Screen.PrimaryScreen.Bounds.Width;
            var screenHeight = Screen.PrimaryScreen.Bounds.Height;
            var scaleWidth = width / screenWidth;
            var scaleHeight= height/screenHeight;
            ILEDSetup ledSetup = new LEDSetup(name, owner, type, description, reorderedActiveSpots, setupID, width, height,scaleWidth,scaleHeight);




            //if (availableLEDSetups != null)
            //{
            //    foreach (var ledsetup in availableLEDSetups)
            //    {
            //        if (ledsetup.SetupID == outputSettings.OutputUniqueID)//found match
            //            ledSetup = ledsetup;

            //    }
            //}



            return ledSetup;
        }

        private static IDeviceSpot[] BuildMatrix(double rectwidth, double rectheight, int spotsX, int spotsY)
        {
            int spacing = 1;
            if (spotsX == 0)
                spotsX = 1;
            if (spotsY == 0)
                spotsY = 1;
            IDeviceSpot[] spotSet = new DeviceSpot[spotsX * spotsY];
            var compareWidth = (rectwidth - (spacing * (spotsX + 1))) / spotsX;
            var compareHeight = (rectheight - (spacing * (spotsY + 1))) / spotsY;
            var spotSize = Math.Min(compareWidth, compareHeight);


            //var startPoint = (Math.Max(rectheight,rectwidth) - spotSize * Math.Min(spotsX, spotsY))/2;
            var counter = 0;




            for (var j = 0; j < spotsY; j++)
            {
                for (var i = 0; i < spotsX; i++)
                {
                    var x = spacing * i + (rectwidth - (spotsX * spotSize) - spacing * (spotsX - 1)) / 2 + i * spotSize;
                    var y = spacing * j + (rectheight - (spotsY * spotSize) - spacing * (spotsY - 1)) / 2 + j * spotSize;
                    var index = counter;
                    double scaleLeft = x / rectwidth;
                    double scaleTop = y / rectheight;
                    double scaleWidth = spotSize / rectwidth;
                    double scaleHeight = spotSize / rectheight;
                    spotSet[index] = new DeviceSpot(i, j, x, y, spotSize, spotSize,scaleTop,scaleLeft,scaleWidth,scaleHeight, index, index, i, index, j, false, false, "genericSquare");
                    counter++;

                }
            }

            return spotSet;

        }
    }
}

