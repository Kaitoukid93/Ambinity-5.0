using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.Settings
{
    internal class DeviceController : IDeviceController
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Geometry { get; set; }
        public string Executioner { get; set; }
        public ControllerTypeEnum Type { get; set; }
        public IOutputSettings[] Outputs { get; set; }
    }
}
