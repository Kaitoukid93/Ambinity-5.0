using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight_effect_analyzer.Model
{
    public class Frame
    {
        public Frame(int numPixel)
        {
            BrightnessData = new byte[numPixel];
        }
        public Frame()
        {
            BrightnessData = new byte[256];
        }
        public byte[] BrightnessData { get; set; }

    }
}
