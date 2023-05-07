using System.Collections.Generic;
using System.Windows.Documents;

namespace adrilight.Settings
{
    public class ControllerData
    {
        public ControllerData() { Outputs = new List<OutputData>(); }
        public IDeviceController Controller { get; set; }
        public List<OutputData> Outputs { get; set; }
    }
}