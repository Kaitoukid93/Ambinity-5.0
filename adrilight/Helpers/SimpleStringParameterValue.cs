using adrilight.Util;
using GalaSoft.MvvmLight;
using System.Collections.Generic;

namespace adrilight.Helpers
{
    public class DancingModeParameterValue : ViewModelBase, IParameterValue
    {
        public string Name { get; set; }
        public string Description { get; set; }   
        public DancingModeEnum Type { get; set; }
    }
}