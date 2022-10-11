using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.Util
{
    internal class LightingMode: ILightingMode
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Geometry { get; set; }
    }
}
