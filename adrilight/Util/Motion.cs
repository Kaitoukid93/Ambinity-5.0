using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace adrilight.Util
{
    public class Motion
    {

        public Frame[] FrameData { get; set; }
        public int FrameCount { get; set; }
        public Color SolidColorBrush { get; set; }
        public IColorPalette ColorPaletteBrush { get; set; }
        public int ColorMode { get; set; }


    }
}
