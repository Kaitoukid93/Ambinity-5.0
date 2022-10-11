using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.Settings
{
    internal class DeviceCatergory : IDeviceCatergory
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public IDeviceSettings[] Devices { get; set; }
        public string Geometry { get; set; }
    }
}
