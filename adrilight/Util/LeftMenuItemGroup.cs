using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.Util
{
    internal class LeftMenuItemGroup : ILeftMenuItemGroup
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string SelectedIndex { get; set; }
        public List<IDeviceSettings> Devices { get; set; }
    }
}
