using adrilight.Helpers;
using adrilight.ViewModel;
using GalaSoft.MvvmLight;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.Settings
{
    public class DeviceProfile : ViewModelBase
    {
        public DeviceProfile() { }
        public string Name { get; set; }
        public DeviceTypeEnum DeviceType { get; set; }
        public string Owner { get; set; }
        public string Description { get; set; }
        public string Geometry { get; set; }
        public string ProfileUID { get; set; }
        public IDeviceSettings DeviceSettings { get; set; }
        public void SaveProfile(IDeviceSettings device)
        {
            device.IsLoadingProfile = true;
            DeviceSettings = ObjectHelpers.Clone<DeviceSettings>(device as DeviceSettings);
            device.IsLoadingProfile = false;
            //DeviceSettings.AvailableControllers = new List<IDeviceController>();
            //foreach (var controller in device.AvailableControllers)
            //{
            //    var controllerData = ObjectHelpers.Clone<DeviceController>(controller as DeviceController);
            //    foreach (var output in controller.Outputs)
            //    {

            //        var outputData = ObjectHelpers.Clone<OutputSettings>(output as OutputSettings);
            //        switch (output.OutputType)
            //        {
            //            case OutputTypeEnum.PWMOutput:
            //                outputData.SlaveDevice = ObjectHelpers.Clone<PWMMotorSlaveDevice>(output.SlaveDevice as PWMMotorSlaveDevice);
            //                break;
            //            case OutputTypeEnum.ARGBLEDOutput:
            //                outputData.SlaveDevice = ObjectHelpers.Clone<ARGBLEDSlaveDevice>(output.SlaveDevice as ARGBLEDSlaveDevice);
            //                break;
            //        }
            //        controllerData.Outputs.Add(outputData);
            //    }
            //    DeviceSettings.AvailableControllers.Add(controllerData);
            //}

        }
    }
}
