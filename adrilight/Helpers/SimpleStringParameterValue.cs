using adrilight.Util;
using System.Collections.Generic;

namespace adrilight.Helpers
{
    public class DancingModeParameterValue : IParameterValue
    {
        public string Name { get; set; }
        public string Description { get; set; }   
        public DancingModeEnum Type { get; set; }
    }
}