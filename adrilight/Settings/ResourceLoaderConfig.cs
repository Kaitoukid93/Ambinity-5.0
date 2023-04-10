using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.Settings
{
    public class ResourceLoaderConfig
    {
        public ResourceLoaderConfig(string type, DeserializeMethodEnum methodEnum)
        {
            DataType = type;
            MethodEnum = methodEnum;
        }
        public string DataType { get; set; }

        public DeserializeMethodEnum MethodEnum { get; set; }
    }
}
