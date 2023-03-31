using adrilight.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.Settings
{
    internal class DeviceAdvanceController : IDeviceController
    {
        public DeviceAdvanceController()
        {
            Outputs = new List<IOutputSettings>();
        }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Geometry { get; set; }
        public string Executioner { get; set; }
        public ControllerTypeEnum Type { get; set; }
        public List<IModeParameter> Parameters { get; set; }
        public List<IOutputSettings> Outputs { get; set; }
    }
}
