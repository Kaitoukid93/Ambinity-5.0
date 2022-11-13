using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Drawing;
using Color = System.Windows.Media.Color;

namespace adrilight_effect_analyzer.Models
{
    public interface IPreviewPixel
    {
        byte Red { get; }
        byte Green { get; }
        byte Blue { get; }


        Color OnDemandColor { get; }

        Rectangle Rectangle { get; set; }


        void SetColor(byte red, byte green, byte blue, bool raiseEvents);


    }
}
