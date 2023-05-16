using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.Settings
{
    public class DeviceType
    {
        public DeviceType(DeviceTypeEnum deviceType)
        {

            Type = deviceType;
        }
        public DeviceType()
        {

        }
        public DeviceTypeEnum Type { get; set; }
        public string Name {
            get
            {
                switch (Type)
                {
                    case DeviceTypeEnum.AmbinoBasic:
                        return "Ambino Basic";
                    case DeviceTypeEnum.AmbinoEDGE:
                        return "Ambino EDGE";
                    case DeviceTypeEnum.AmbinoFanHub:
                        return "Ambino FanHUB";
                    case DeviceTypeEnum.AmbinoRainPowPro:
                        return "Ambino RainPow Pro";
                    case DeviceTypeEnum.AmbinoHUBV2:
                        return "Ambino HUBV2";
                    case DeviceTypeEnum.AmbinoHUBV3:
                        return "Ambino HUBV3";
                    case DeviceTypeEnum.Mouse:
                        return "Mouse";
                    case DeviceTypeEnum.Keyboard:
                        return "Keyboard";
                    case DeviceTypeEnum.Motherboard:
                        return "Motherboard";
                    case DeviceTypeEnum.Light:
                        return "Light";
                    case DeviceTypeEnum.HeadsetStand:
                        return "Headset Stand";
                    case DeviceTypeEnum.Cooler:
                        return "Cooler";
                    case DeviceTypeEnum.Dram:
                        return "Dram";
                    case DeviceTypeEnum.Gamepad:
                        return "Gamepad";
                    case DeviceTypeEnum.Headset:
                        return "Headset";
                    case DeviceTypeEnum.Mousemat:
                        return "Mousemat";
                    case DeviceTypeEnum.Speaker:
                        return "Speaker";
                    case DeviceTypeEnum.Unknown:
                        return "Other";
                    case DeviceTypeEnum.Virtual:
                        return "Virtual";
                    case DeviceTypeEnum.Ledstrip:
                        return "Ledstrip";
                }
                return "generaldevice";
            }
        }
        public string Description { get; set; }
        public bool Selected { get; set; } // for the sake of checkCombobox only
        public string Geometry {
            get
            {
                switch (Type)
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
