using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace adrilight.Util
{
    public interface IComposition
    {
        string Name { get; set; }
        string Owner { get; set; }
        Motion[] Layers { get; set; }
        string Type { get; set; }
        string Description { get; set; }
        int Duration { get; set; }
        int TotalFrame { get; set; }
    }
}
