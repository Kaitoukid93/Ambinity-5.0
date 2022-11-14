using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace adrilight.Util
{
    public interface IMotionLayer
    {
        string Name { get; set; }
        string Owner { get; set; }
        Motion Motion { get; set; }
        string Type { get; set; }
        string Description { get; set; }
        Color MotionBrush { get; set; }
        int StartFrame { get; set; }
    }
}
