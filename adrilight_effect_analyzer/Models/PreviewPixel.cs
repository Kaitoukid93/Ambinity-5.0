using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight_effect_analyzer.Models
{
    internal class PreviewPixel : ViewModelBase,IPreviewPixel
    {
        public PreviewPixel(int x, int y, int top, int left, int width, int height)
        {
            Rectangle = new Rectangle(top, left, width, height);

        }

        public Rectangle Rectangle { get; set; }

        public void SetColor(byte red, byte green, byte blue, bool raiseEvents)
        {
            Red = red;
            Green = green;
            Blue = blue;
 

            if (raiseEvents)
            {
                RaisePropertyChanged(nameof(OnDemandColor));
             
            }
        }
        public byte Red { get; private set; }
        public byte Green { get; private set; }
        public byte Blue { get; private set; }

        public System.Windows.Media.Color OnDemandColor => System.Windows.Media.Color.FromRgb(Red, Green, Blue);

    }
}
