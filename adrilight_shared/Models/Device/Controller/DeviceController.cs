using System.Collections.Generic;

namespace adrilight.Settings
{
    public class DeviceController : IDeviceController
    {
        public DeviceController()
        {
            Outputs = new List<IOutputSettings>();
        }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Geometry { get; set; }
        public string Executioner { get; set; }
        public ControllerTypeEnum Type { get; set; }
        public List<IOutputSettings> Outputs { get; set; }
    }
}
