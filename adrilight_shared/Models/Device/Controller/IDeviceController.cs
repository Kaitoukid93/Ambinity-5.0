using adrilight_shared.Enums;
using adrilight_shared.Models.Device.Output;
using System.Collections.Generic;

namespace adrilight_shared.Models.Device.Controller
{
    public interface IDeviceController
    {
        /// <summary>
        /// information properties
        /// </summary>
        string Name { get; set; }
        string Description { get; set; }
        string Geometry { get; set; }
        string Executioner { get; set; } // store micro controller hardware data
        ControllerTypeEnum Type { get; set; }
        /// <summary>
        /// data properties
        /// </summary>
        List<IOutputSettings> Outputs { get; set; }

    }
}
