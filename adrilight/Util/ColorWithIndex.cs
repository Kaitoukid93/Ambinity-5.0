using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace adrilight.Util
{
    internal class ColorWithIndex : IColorWithIndex
    {
        public  ColorWithIndex(Color color, int index)
        {
            Color = color;
            Index = index;
        }
        
        public Color Color { get; set; }
        public int Index { get; set; }
    }
}
