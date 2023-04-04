using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.Settings
{
    public class DeviceTypeDataEnum : IOnlineItemSubType
    {
        public DeviceTypeDataEnum(string name, DeviceTypeEnum deviceType)
        {

            Name = name;
            DeviceType = deviceType;
        }

        public string Name { get; set; }
        public DeviceTypeEnum DeviceType { get; set; }
        public string Description { get; set; }
        public string Geometry {
            get
            {
                switch (DeviceType)
                {
                    case DeviceTypeEnum.AmbinoBasic:
                        return "ambinobasic";
                    case DeviceTypeEnum.AmbinoEDGE:
                        return "ambinoedge";
                    case DeviceTypeEnum.AmbinoFanHub:
                        return "ambinofanhub";
                    case DeviceTypeEnum.AmbinoRainPowPro:
                        return "generaldevice";
                    case DeviceTypeEnum.AmbinoHUBV2:
                        return "ambinohub";
                    case DeviceTypeEnum.AmbinoHUBV3:
                        return "ambinohubv3";
                }
                return "generaldevice";
            }
        }

        public string ConnectionGeometry {
            get
            {
                switch (ConnectionTypeEnum)
                {
                    case DeviceConnectionTypeEnum.Wired:
                        return "connection";

                    case DeviceConnectionTypeEnum.Wireless:
                        return "connection";

                    case DeviceConnectionTypeEnum.OpenRGB:
                        return "orgb";


                }
                return "connection";
            }
        }
        public DeviceConnectionTypeEnum ConnectionTypeEnum { get; set; }
    }
}
