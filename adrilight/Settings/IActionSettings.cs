using adrilight.Spots;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using adrilight.Util;

namespace adrilight
{
    public interface IActionSettings : INotifyPropertyChanged
    {
      
        string TargetDeviceUID { get; set; }
        string TargetDeviceName { get; set; }
        string TargetDeviceType { get; set; }

        ActionType ActionType { get; set; }
      
        IActionParameter ActionParameter{ get; set; }
        
    }
}
