using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.Helpers
{
    public class ZoneData
    {
        public ZoneData(string name,int numLEDX, int numLEDY, int width, int height)
        {
            NumLEDX = numLEDX;
            NumLEDY = numLEDY;
            Width = width;
            Height = height;
            Name = name;
        }
        public int NumLEDX { get; set; }
        public string Name { get; set; }
        public int NumLEDY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
