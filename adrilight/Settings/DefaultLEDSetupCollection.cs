using adrilight.Spots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
namespace adrilight.Settings
{
    internal class DefaultLEDSetupCollection
    {
        public static ILEDSetup defaultLED;
        public DefaultLEDSetupCollection()
        {
            defaultLED = BuildLEDSetup(11, 7, 32, "123", "456", "789", "ABRev2", 240, 135, "ABRev2");
        }
        internal ILEDSetup BuildLEDSetup(int matrixWidth, int matrixHeight, int numLED, string name, string owner, string description, string type, int rectWidth, int rectHeight, string outputType) // general settings is for compare each device setting
        {

           

            IDeviceSpot[] spots = new DeviceSpot[numLED];
            List<IDeviceSpot> reorderedSpots = new List<IDeviceSpot>();

            //Create default spot
            var availableSpots = BuildMatrix(rectWidth, rectHeight, matrixWidth, matrixHeight);
            int counter = 0;
            switch (outputType)
            {
                case "ABRev2":
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
                case "Keyboard":
                    foreach (var spot in availableSpots)
                    {
                        spot.IsActivated = true;
                        reorderedSpots.Add(spot);
                    }
                    break;
                case "ABEDGE":
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

            ILEDSetup ledSetup = new LEDSetup(name, owner, type, description, reorderedActiveSpots, matrixWidth, matrixHeight, 0,100,100);


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
        private IDeviceSpot[] BuildMatrix(int rectwidth, int rectheight, int spotsX, int spotsY)
        {
            int spacing = 3;
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

                    spotSet[index] = new DeviceSpot(i, j, x, y, spotSize, spotSize, index, index, index, index,j, false,false, "genericCircle");
                    counter++;

                }
            }

            return spotSet;

        }
    }
}
