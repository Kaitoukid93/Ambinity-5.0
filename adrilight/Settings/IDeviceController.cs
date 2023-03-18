using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.Settings
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
        IOutputSettings[] Outputs { get; set; }

    }
}
