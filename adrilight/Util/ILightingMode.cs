using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.Util
{
    public interface ILightingMode
    {
        string Name { get; set; }
        string Description { get; set; }
        string Geometry { get; set; }
    }
}
