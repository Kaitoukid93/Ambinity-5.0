using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.Util
{
    public class VIDDataModel:IParameterValue
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public VIDType ExecutionType { get; set; }
        public string Geometry { get; set; }
    }
}
