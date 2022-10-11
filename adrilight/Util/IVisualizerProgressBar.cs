using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace adrilight.Util
{
    public interface IVisualizerProgressBar
    {
        float Value  { get; set; }
       
        Color OndemandColor { get; set; }
       
        void SetColor(Color color);
        void SetValue(float value);
    }
}
