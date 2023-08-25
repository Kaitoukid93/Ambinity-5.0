using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace adrilight.Settings
{
    class XmlApiResponse
    {
        public byte Brightness { get; set; } = 128;
        public bool IsOn { get; set; } = false;
        public Color LightColor { get; set; }
        public string Name { get; set; } = "";
    }
}
