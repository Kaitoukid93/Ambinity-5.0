using adrilight.DesktopDuplication;
using adrilight.Extensions;
using adrilight.Spots;
using System.Windows.Media;
using NLog;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Collections;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace adrilight
{
    internal sealed class DeviceSpotSet : IDeviceSpotSet
    {
        private ILogger _log = LogManager.GetCurrentClassLogger();
        private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "adrilight\\");
        private string JsonLEDSetupFileNameAndPath => Path.Combine(JsonPath, "adrilight-LEDSetups.json");
        public DeviceSpotSet(IOutputSettings outputSettings, IGeneralSettings generalSettings)
        {
            OutputSettings = outputSettings as OutputSettings ?? throw new ArgumentNullException(nameof(outputSettings));
            GeneralSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));

            OutputSettings.PropertyChanged += (_, e) => DecideRefresh(e.PropertyName);
            GeneralSettings.PropertyChanged += (_, e) => DecideRefresh(e.PropertyName);
            Refresh();

            _log.Info($"SpotSet created.");
        }

        private void DecideRefresh(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(OutputSettings.OutputNumLEDX):
                case nameof(OutputSettings.OutputNumLEDY):
                case nameof(OutputSettings.OutputNumLED):
                case nameof(OutputSettings.Width):
                case nameof(OutputSettings.Height):
                case nameof(GeneralSettings.IsProfileLoading):
                case nameof(OutputSettings.IsInSpotEditWizard):
                    //... there are more to come
                    Refresh();
                    break;


            }
        }


        public ILEDSetup LEDSetup { get; set; }



        public object Lock { get; } = new object();


        /// <summary>
        /// returns the number of leds
        /// </summary>











        private OutputSettings OutputSettings { get; }
        private IGeneralSettings GeneralSettings { get; }
        private void Refresh()
        {


            lock (OutputSettings.OutputLEDSetup.Lock)
            {   
                if(!GeneralSettings.IsProfileLoading)
                {
                   
                    OutputSettings.OutputLEDSetup = BuildLEDSetup(OutputSettings, GeneralSettings);
                    OutputSettings.OutputIsBuildingLEDSetup = false;
                }
              
            }

        }

        internal ILEDSetup BuildLEDSetup(IOutputSettings outputSettings, IGeneralSettings generalSettings) // general settings is for compare each device setting
        {
            int matrixWidth = outputSettings.OutputNumLEDX;
            int matrixHeight = outputSettings.OutputNumLEDY;
            int numLED = outputSettings.OutputNumLED;
            string name = outputSettings.OutputName;
            string owner = "Ambino";
            string description = "Default LED Setup for Ambino Basic Rev 2";
            string type = "ABRev2";
            int setupID = outputSettings.OutputID;
            int rectWidth = (int)(outputSettings as OutputSettings).Width;
            int rectHeight = (int)(outputSettings as OutputSettings).Height;

            IDeviceSpot[] spots = new DeviceSpot[numLED];
            List<IDeviceSpot> reorderedSpots = new List<IDeviceSpot>();

            //Create default spot
            var availableSpots = BuildMatrix(rectWidth, rectHeight, matrixWidth, matrixHeight);
            int counter = 0;
            ILEDSetup ledSetup = outputSettings.OutputLEDSetup;
            if(ledSetup != null)
            {
                int spacing = 1;   
                var compareWidth = (rectWidth - (spacing * (matrixWidth + 1))) / matrixWidth;
                var compareHeight = (rectHeight - (spacing * (matrixHeight + 1))) / matrixHeight;
                var spotSize = Math.Min(compareWidth, compareHeight);

                //double scale = Math.Min(scaleX, scaleY);
                foreach (var spot in ledSetup.Spots)
                    {

                    var x = spacing * spot.XIndex + (rectWidth - (matrixWidth * spotSize) - spacing * (matrixWidth - 1)) / 2 + spot.XIndex * spotSize;
                    var y = spacing * spot.YIndex + (rectHeight - (matrixHeight * spotSize) - spacing * (matrixHeight - 1)) / 2 + spot.YIndex * spotSize;
                    (spot as IDrawable).Top = y;
                    (spot as IDrawable).Left = x;
                    (spot as IDrawable).Width = spotSize;
                    (spot as IDrawable).Height = spotSize;
                    }
                  
                
            }
           
            else 
            {
                switch (outputSettings.OutputType)
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

                IDeviceSpot[] reorderedActiveSpots = new DeviceSpot[reorderedSpots.Count];

                foreach (var spot in reorderedSpots)
                {
                    reorderedActiveSpots[counter++] = spot;
                }

                ledSetup = new LEDSetup(name, owner, type, description, reorderedActiveSpots, matrixWidth, matrixHeight, setupID, rectWidth, rectHeight);
            }



            if (OutputSettings.IsInSpotEditWizard)
            {
                ledSetup = new LEDSetup(name, owner, type, description, BuildMatrix(rectWidth, rectHeight, matrixWidth, matrixHeight), matrixWidth, matrixHeight, setupID,rectWidth,rectHeight);
            }
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


        public List<LEDSetup> LoadSetupIfExist()
        {
            if (!File.Exists(JsonLEDSetupFileNameAndPath)) return null;

            var json = File.ReadAllText(JsonLEDSetupFileNameAndPath);

            var ledSetups = JsonConvert.DeserializeObject<List<LEDSetup>>(json);

            return ledSetups;
        }

        private IDeviceSpot[] BuildMatrix(double rectwidth, double rectheight, int spotsX, int spotsY)
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

                    spotSet[index] = new DeviceSpot(i, j, y, x, spotSize, spotSize, index, index, i, index,j, false, false,"genericCircle");
                    counter++;

                }
            }

            return spotSet;

        }




    }
}
