using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.Util
{
    public interface ILeftMenuItemGroup
    {
        string Name { get; set; }
        string Description { get; set; }
        string SelectedIndex { get; set; }
        List<IDeviceSettings> Devices { get; set; }


    }
}
