using adrilight_shared.Enum;

namespace adrilight_shared.Models.ControlMode
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
