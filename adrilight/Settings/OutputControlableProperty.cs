using adrilight.Util;
using System.Collections.Generic;
using System.Windows.Documents;

namespace adrilight
{
    public class OutputControlableProperty
    {
        public OutputControlableProperty() { }
        public string Name { get; set; }
        public string Description { get; set; }
        public OutputControlablePropertyEnum Type { get; set; }
        public List<object> AvailableControlMode { get; set; }
        public  int _currentActiveLightingModeIndex;
      
        public string Icon { get; set; }
    }
}