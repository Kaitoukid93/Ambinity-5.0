using adrilight_shared.Model.VerticalMenu;
using adrilight_shared.Models.Device;
using adrilight_shared.Models.Device.Group;
using adrilight_shared.Models.Drawable;
using System;
using System.Collections.Generic;

namespace adrilight_shared.Models.Stores
{
    public class NavigationEvent
    {
        public event Action<IDeviceSettings> NavigateToDeviceControlEvent;
      
        public void NavigateToDeviceControl(IDeviceSettings selectedDevice)
        {
            NavigateToDeviceControlEvent?.Invoke(selectedDevice);
        }
     
    }
}
