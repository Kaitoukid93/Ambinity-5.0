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
    public interface IActionParameter : INotifyPropertyChanged
    {
      
        
        string Type { get; set; }
        //string TargetDeviceType { get; set; }
        string Name { get; set; } 
        object Value { get; set; }      
        string Geometry { get; set; }
        
        
    }
}
