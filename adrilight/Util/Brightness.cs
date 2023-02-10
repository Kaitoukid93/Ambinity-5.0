using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Un4seen.Bass;
using Un4seen.BassWasapi;
using System.Linq;
using System.Windows.Controls;
using OpenRGB.NET;



namespace adrilight.Util
{
    internal class Brightness 
    {

        //this class take final form of color collection and apply the general brightness before display to the screen or send out to serial port
        public static OpenRGB.NET.Models.Color applyBrightness(OpenRGB.NET.Models.Color inputColor, double brightness,int numLED, int powerSuplyMiliamps, int powersupplyVoltage)
        {
            if(numLED==0)
                return OpenRGB.NET.Models.Color.FromHsv(1.0, 1.0, 1.0);
            var currentPerLED = 60;// current to full bright a white LED
            var maxPowerPerLED = currentPerLED * powersupplyVoltage / 1000.0;
            double brightnessFactor = 1.0;
            // caculate max power of current device's power supply 
            double maxPower = powerSuplyMiliamps * powersupplyVoltage / 1000.0;
            // caculate power draw by current device's led setup
            double currentLEDMaxPower = maxPower / numLED;
            var percent = brightness / 1.0;
            double currentLEDRequiredPower =  maxPowerPerLED * (inputColor.R + inputColor.G + inputColor.B) / 765;


            // caculate current power factor
            if(currentLEDRequiredPower > currentLEDMaxPower)
             brightnessFactor = currentLEDMaxPower / currentLEDRequiredPower;

            //now we can turn this color to HSV
            var hue = inputColor.ToHsv().h;
            var saturation = inputColor.ToHsv().s;
            var value = inputColor.ToHsv().v;
            //apply brightness value
            if (brightness > 1.0)
                brightness = 1.0;
            
            var returnColor = OpenRGB.NET.Models.Color.FromHsv(hue, saturation, percent*value * brightnessFactor);
            return returnColor;
        }

    }
}