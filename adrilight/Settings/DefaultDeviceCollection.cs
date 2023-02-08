using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.Settings
{

    internal class DefaultDeviceCollection
    {
        private static string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "adrilight\\");
        private static string JsonFWToolsFileNameAndPath => Path.Combine(JsonPath, "FWTools");
        private static string FanHubFW => Path.Combine(JsonFWToolsFileNameAndPath, "ABFANHUB.hex");
        private static string FanHubFWVersion => Path.Combine(JsonFWToolsFileNameAndPath, "ABFANHUB.json");
        private static string ABBASICFW => Path.Combine(JsonFWToolsFileNameAndPath, "ABBASIC.hex");
        private static string ABBASICFWVersion => Path.Combine(JsonFWToolsFileNameAndPath, "ABBASIC.json");
        private static string ABBASICPWLEDFW => Path.Combine(JsonFWToolsFileNameAndPath, "ABBASICPWLED.hex");
        private static string ABBASICPWLEDFWVersion => Path.Combine(JsonFWToolsFileNameAndPath, "ABBASICPWLED.json");
        private static string ABEDGEFW => Path.Combine(JsonFWToolsFileNameAndPath, "ABEDGE.hex");
        private static string ABEDGEFWVersion => Path.Combine(JsonFWToolsFileNameAndPath, "ABEDGE.json");
        private static string ABRAINPOWFW => Path.Combine(JsonFWToolsFileNameAndPath, "ABRP.hex");
        private static string ABRAINPOWFWVersion => Path.Combine(JsonFWToolsFileNameAndPath, "ABRP.json");
        private static string ABHUBV3FW => Path.Combine(JsonFWToolsFileNameAndPath, "ABHUBV3.hex");
        private static string ABHUBV3FWVersion => Path.Combine(JsonFWToolsFileNameAndPath, "ABHUBV3.json");
        //public static List<DeviceSettings> AvailableDefaultDevice()
        //{
        //    return new List<DeviceSettings> { ambinoBasic24, ambinoBasic27, ambinoBasic29, ambinoBasic32, ambinoBasic34, ambinoEdge1m2, ambinoEdge2m, ambinoFanHub, ambinoHUBV3,ambinoBasicPWLED };
        //}

        public DeviceSettings AmbinoBasicCore {
            get
            {
                return new DeviceSettings {
                    DeviceName = "Ambino Basic",
                    DeviceSerial = "ABBASIC",
                    DeviceType = "ABBASIC",
                    Manufacturer = "Ambino Vietnam",
                    FirmwareVersion = "1.0.0",
                    ProductionDate = "2022",
                    DeviceConnectionType = "wired",
                    IsVisible = true,
                    IsEnabled = true,
                    OutputPort = "Không có",
                    FwLocation = ABBASICFW,
                    RequiredFwVersion = ABBASICFWVersion,
                    Geometry = "ambinobasic",
                    DeviceUID = Guid.NewGuid().ToString(),
                    IsTransferActive = true,
                    AvailableOutputs = new OutputSettings[] { DefaulOutputCollection.AvailableDefaultOutputsForAmbinoBasic.Last() }
                };
            }


        }
        public DeviceSettings AmbinoEDGECore {
            get
            {
                return new DeviceSettings {
                    DeviceName = "Ambino EDGE",
                    DeviceSerial = "ABEDGE",
                    DeviceType = "ABEDGE",
                    Manufacturer = "Ambino Vietnam",
                    DeviceConnectionType = "wired",
                    FirmwareVersion = "1.0.0",
                    DeviceUID = Guid.NewGuid().ToString(),
                    ProductionDate = "2022",
                    IsVisible = true,
                    IsEnabled = true,
                    OutputPort = "Không có",
                    FwLocation = ABEDGEFW,
                    RequiredFwVersion = ABEDGEFWVersion,
                    Geometry = "ambinoedge",
                    IsTransferActive = true,
                    AvailableOutputs = new OutputSettings[] { DefaulOutputCollection.AmbinoOutput(0, "LED Cạnh Bàn", "ledstrip", "adrilight.AmbinoFactoryValue.Basic24.json") }

                };

            }
        }
        public DeviceSettings ambinoBasic27 {
            get
            {
                return new DeviceSettings {
                    DeviceName = "Ambino Basic 27 inch",
                    DeviceSerial = "ABBASIC27",
                    DeviceType = "ABBASIC",
                    DeviceConnectionType = "wired",
                    Manufacturer = "Ambino Vietnam",
                    DeviceUID = Guid.NewGuid().ToString(),
                    FirmwareVersion = "1.0.0",
                    ProductionDate = "2022",
                    IsVisible = true,
                    IsEnabled = true,
                    OutputPort = "Không có",
                    FwLocation = ABBASICFW,
                    RequiredFwVersion = ABBASICFWVersion,
                    Geometry = "ambinobasic",
                    IsTransferActive = true,
                    AvailableOutputs = new OutputSettings[] { DefaulOutputCollection.AmbinoOutput(0, "LED Màn hình 27", "27inch", "adrilight.AmbinoFactoryValue.Basic24.json") }
                };

            }
        }

        public DeviceSettings ambinoBasicPWLED {
            get
            {
                return new DeviceSettings {
                    DeviceName = "Ambino Basic",
                    DeviceSerial = "ABBASICPWLED",
                    DeviceType = "ABBASIC",
                    Manufacturer = "Ambino Vietnam",
                    DeviceUID = Guid.NewGuid().ToString(),
                    FirmwareVersion = "1.0.0",
                    ProductionDate = "2022",
                    DeviceConnectionType = "wired",
                    IsVisible = true,
                    IsEnabled = true,
                    OutputPort = "Không có",
                    FwLocation = ABBASICPWLEDFW,
                    RequiredFwVersion = ABBASICPWLEDFWVersion,
                    Geometry = "ambinobasic",
                    IsTransferActive = true,
                    AvailableOutputs = new OutputSettings[] { DefaulOutputCollection.AmbinoOutput(0, "LED Màn hình 24", "24inch", "adrilight.AmbinoFactoryValue.Basic24.json") }
                };
            }


        }
        public DeviceSettings ambinoBasic29 {
            get
            {
                return new DeviceSettings {
                    DeviceName = "Ambino Basic 29 inch",
                    DeviceSerial = "ABBASIC29",
                    DeviceType = "ABBASIC",
                    Manufacturer = "Ambino Vietnam",
                    DeviceUID = Guid.NewGuid().ToString(),
                    FirmwareVersion = "1.0.0",
                    ProductionDate = "2022",
                    DeviceConnectionType = "wired",
                    IsVisible = true,
                    IsEnabled = true,
                    OutputPort = "Không có",
                    FwLocation = ABBASICFW,
                    RequiredFwVersion = ABBASICFWVersion,
                    Geometry = "ambinobasic",
                    IsTransferActive = true,
                    AvailableOutputs = new OutputSettings[] { DefaulOutputCollection.AmbinoOutput(0, "LED Màn hình 29", "29inch", "adrilight.AmbinoFactoryValue.Basic24.json") }
                };
            }
        }


        public DeviceSettings ambinoBasic32 {
            get
            {
                return new DeviceSettings {
                    DeviceName = "Ambino Basic 32 inch",
                    DeviceSerial = "ABBASIC32",
                    DeviceType = "ABBASIC",
                    Manufacturer = "Ambino Vietnam",
                    DeviceUID = Guid.NewGuid().ToString(),
                    FirmwareVersion = "1.0.0",
                    ProductionDate = "2022",
                    DeviceConnectionType = "wired",
                    IsVisible = true,
                    IsEnabled = true,
                    OutputPort = "Không có",
                    FwLocation = ABBASICFW,
                    RequiredFwVersion = ABBASICFWVersion,
                    Geometry = "ambinobasic",
                    IsTransferActive = true,
                    AvailableOutputs = new OutputSettings[] { DefaulOutputCollection.AmbinoOutput(0, "LED Màn hình 32", "32inch", "adrilight.AmbinoFactoryValue.Basic24.json") }

                };
            }
        }
        public DeviceSettings ambinoBasic34 {
            get
            {
                return new DeviceSettings {
                    DeviceName = "Ambino Basic 34 inch",
                    DeviceSerial = "ABBASIC34",
                    DeviceType = "ABBASIC",
                    Manufacturer = "Ambino Vietnam",
                    DeviceConnectionType = "wired",
                    FirmwareVersion = "1.0.0",
                    DeviceUID = Guid.NewGuid().ToString(),
                    ProductionDate = "2022",
                    IsVisible = true,
                    IsEnabled = true,
                    OutputPort = "Không có",
                    FwLocation = ABBASICFW,
                    RequiredFwVersion = ABBASICFWVersion,
                    Geometry = "ambinobasic",
                    IsTransferActive = true,
                    AvailableOutputs = new OutputSettings[] { DefaulOutputCollection.AmbinoOutput(0, "LED Màn hình 34", "34inch", "adrilight.AmbinoFactoryValue.Basic24.json") }

                };
            }
        }
        public DeviceSettings ambinoEdge1m2 {
            get
            {
                return new DeviceSettings {
                    DeviceName = "Ambino EDGE 1.2m",
                    DeviceSerial = "ABEDGE1.2",
                    DeviceType = "ABEDGE",
                    Manufacturer = "Ambino Vietnam",
                    DeviceConnectionType = "wired",
                    FirmwareVersion = "1.0.0",
                    DeviceUID = Guid.NewGuid().ToString(),
                    ProductionDate = "2022",
                    IsVisible = true,
                    IsEnabled = true,
                    OutputPort = "Không có",
                    FwLocation = ABEDGEFW,
                    RequiredFwVersion = ABEDGEFWVersion,
                    Geometry = "ambinoedge",
                    IsTransferActive = true,
                    AvailableOutputs = new OutputSettings[] { DefaulOutputCollection.AmbinoOutput(0, "LED Cạnh Bàn", "ledstrip", "adrilight.AmbinoFactoryValue.Basic24.json") }

                };

            }
        }
        public DeviceSettings ambinoEdge2m {
            get
            {
                return new DeviceSettings {
                    DeviceName = "Ambino EDGE 2m",
                    DeviceSerial = "ABEDGE2.0",
                    DeviceType = "ABEDGE",
                    Manufacturer = "Ambino Vietnam",
                    DeviceConnectionType = "wired",
                    FirmwareVersion = "1.0.0",
                    ProductionDate = "2022",
                    IsVisible = true,
                    DeviceUID = Guid.NewGuid().ToString(),
                    IsEnabled = true,
                    OutputPort = "Không có",
                    FwLocation = ABEDGEFW,
                    RequiredFwVersion = ABEDGEFWVersion,
                    Geometry = "ambinoedge",
                    IsTransferActive = true,
                    AvailableOutputs = new OutputSettings[] { DefaulOutputCollection.AmbinoOutput(0, "LED Cạnh Bàn", "ledstrip", "adrilight.AmbinoFactoryValue.Basic24.json") }

                };
            }
        }
        public DeviceSettings ambinoFanHub {
            get
            {
                return new DeviceSettings {
                    DeviceName = "Ambino FanHub",
                    DeviceSerial = "ABFANHUB",
                    DeviceType = "ABFANHUB",
                    Manufacturer = "Ambino Vietnam",
                    DeviceConnectionType = "wired",
                    FirmwareVersion = "1.0.0",
                    ProductionDate = "2022",
                    IsVisible = true,
                    DeviceUID = Guid.NewGuid().ToString(),
                    IsEnabled = true,
                    OutputPort = "Không có",
                    FwLocation = FanHubFW,
                    RequiredFwVersion = FanHubFWVersion,
                    Geometry = "ambinofanhub",
                    IsTransferActive = true,
                    AvailableOutputs = new OutputSettings[] {  DefaulOutputCollection.AmbinoOutput(0, "Fan1", "fan", "adrilight.AmbinoFactoryValue.Basic24.json"),
                                                               DefaulOutputCollection.AmbinoOutput(1, "Fan2", "fan", "adrilight.AmbinoFactoryValue.Basic24.json"),
                                                               DefaulOutputCollection.AmbinoOutput(2, "Fan3", "fan", "adrilight.AmbinoFactoryValue.Basic24.json"),
                                                               DefaulOutputCollection.AmbinoOutput(3, "Fan4", "fan", "adrilight.AmbinoFactoryValue.Basic24.json"),
                                                               DefaulOutputCollection.AmbinoOutput(4, "Fan5", "fan", "adrilight.AmbinoFactoryValue.Basic24.json"),
                                                               DefaulOutputCollection.AmbinoOutput(5, "Fan6", "fan", "adrilight.AmbinoFactoryValue.Basic24.json"),
                                                               DefaulOutputCollection.AmbinoOutput(6, "Fan7", "fan", "adrilight.AmbinoFactoryValue.Basic24.json"),
                                                               DefaulOutputCollection.AmbinoOutput(7, "Fan8", "fan", "adrilight.AmbinoFactoryValue.Basic24.json"),
                                                               DefaulOutputCollection.AmbinoOutput(8, "Fan9", "fan", "adrilight.AmbinoFactoryValue.Basic24.json"),
                                                               DefaulOutputCollection.AmbinoOutput(9, "Fan10", "fan", "adrilight.AmbinoFactoryValue.Basic24.json")
            },




                };
            }
        }
        public DeviceSettings ambinoHUBV2 {
            get
            {
                return new DeviceSettings {
                    DeviceName = "Ambino HUBV2",
                    DeviceSerial = "ABHUBV2",
                    DeviceType = "ABHUBV2",
                    Manufacturer = "Ambino Vietnam",
                    FirmwareVersion = "1.0.7",
                    DeviceUID = Guid.NewGuid().ToString(),
                    ProductionDate = "2020",
                    IsVisible = true,
                    IsEnabled = true,
                    DeviceConnectionType = "wired",
                    OutputPort = "Không có",
                    Geometry = "ambinohub",
                    IsTransferActive = true,
                    AvailableOutputs = new OutputSettings[] { DefaulOutputCollection.GenericLEDStrip(0, 16,"Dải LED 1", 1,true,"ledstrip"),
                                                      DefaulOutputCollection.GenericLEDStrip(1, 16, "Dải LED 2", 1,true,"ledstrip"),
                                                     DefaulOutputCollection.AmbinoOutput(2, "LED Màn hình 24", "24inch", "adrilight.AmbinoFactoryValue.Basic24.json"),
                                                      DefaulOutputCollection.AmbinoOutput(3, "LED Màn hình 24", "24inch", "adrilight.AmbinoFactoryValue.Basic24.json"),
                                                      DefaulOutputCollection.AmbinoOutput(4, "LED Màn hình 24", "24inch", "adrilight.AmbinoFactoryValue.Basic24.json"),
                                                       DefaulOutputCollection.AmbinoOutput(5, "LED Cạnh Bàn", "ledstrip", "adrilight.AmbinoFactoryValue.Basic24.json")

            }

                };
            }
        }
        public DeviceSettings ambinoHUBV3 {
            get
            {
                return new DeviceSettings {
                    DeviceName = "Ambino HUBV3",
                    DeviceSerial = "ABHUBV3",
                    DeviceType = "ABHUBV3",
                    Manufacturer = "Ambino Vietnam",
                    FirmwareVersion = "1.0.0",
                    ProductionDate = "2022",
                    DeviceUID = Guid.NewGuid().ToString(),
                    IsVisible = true,
                    IsEnabled = true,
                    OutputPort = "Không có",
                    DeviceConnectionType = "wired",
                    FwLocation = ABHUBV3FW,
                    RequiredFwVersion = ABHUBV3FWVersion,
                    Geometry = "ambinohubv3",
                    IsTransferActive = true,
                    AvailableOutputs = new OutputSettings[] { DefaulOutputCollection.GenericLEDStrip(0, 32,"Dải LED 1", 1,true,"ledstrip"),
                                                      DefaulOutputCollection.GenericLEDStrip(1, 32,"Dải LED 2", 1,true,"ledstrip"),
                                                      DefaulOutputCollection.GenericLEDStrip(2, 32,"Dải LED 3", 1,true,"ledstrip"),
                                                      DefaulOutputCollection.GenericLEDStrip(3, 32,"Dải LED 4", 1,true,"ledstrip"),
                                                      DefaulOutputCollection.GenericLEDStrip(4, 1,"12v RGB", 1,true,"ledstrip")



            }

                };
            }
        }
        public DeviceSettings ambinoRainPow {
            get
            {
                return new DeviceSettings {
                    DeviceName = "Ambino RainPow",
                    DeviceSerial = "ABRP",
                    DeviceType = "ABRP",
                    Manufacturer = "Ambino Vietnam",
                    DeviceConnectionType = "wired",
                    FirmwareVersion = "1.0.0",
                    ProductionDate = "2022",
                    IsVisible = true,
                    DeviceUID = Guid.NewGuid().ToString(),
                    IsEnabled = true,
                    OutputPort = "Không có",
                    FwLocation = ABRAINPOWFW,
                    RequiredFwVersion = ABRAINPOWFWVersion,
                    Geometry = "ambinohubv3",
                    IsTransferActive = true,
                    AvailableOutputs = new OutputSettings[] { DefaulOutputCollection.GenericLEDStrip(0, 20,"Dây LED 1",1,true,"ledstrip"),
                                                      DefaulOutputCollection.GenericLEDStrip(1, 20,"Dây LED 2",1,true,"ledstrip"),
                                                      DefaulOutputCollection.GenericLEDStrip(2, 20,"Dây LED 3",1,true,"ledstrip"),
                                                      DefaulOutputCollection.GenericLEDStrip(3, 20,"Dây LED 4",1,true,"ledstrip"),
                                                      DefaulOutputCollection.GenericLEDStrip(4, 20,"Dây LED 5",1,true,"ledstrip"),
                                                      DefaulOutputCollection.GenericLEDStrip(5, 20,"Dây LED 6",1,true,"ledstrip")



            }

                };
            }
        }
    }
}
