using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.Settings
{
    public interface IDeviceCatergory
    {
        string Name { get; set; }
        string Description { get; set; }
        IDeviceSettings[] Devices { get; set; }
        string Geometry { get; set; }
    }
}
