using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace adrilight.Util
{
    public interface IGradientColorCard 
    {
        string Name { get; set; }
        string Owner { get; set; }
        Color StartColor { get; set; }
        Color StopColor { get; set; }
        string Type { get; set; }
        string Description { get; set; }
    }
}
