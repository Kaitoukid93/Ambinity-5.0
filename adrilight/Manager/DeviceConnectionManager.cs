
using adrilight.Services.DataStream;
using adrilight.Services.OpenRGBService;
using adrilight_shared.Enums;
using adrilight_shared.Models.Device;
using System;


namespace adrilight.Manager
{
    public class DeviceConnectionManager
    {
        public DeviceConnectionManager(AmbinityClient ambinityClient)
        {
            _ambinityClient = ambinityClient ?? throw new ArgumentNullException(nameof(ambinityClient));
        }
        private AmbinityClient _ambinityClient;
        public IDataStream CreateDeviceStreamService(IDeviceSettings device)
        {

            switch (device.DeviceType.ConnectionTypeEnum)
            {
                case DeviceConnectionTypeEnum.Wired:
                    return new SerialStream();
                case DeviceConnectionTypeEnum.Wireless:
                    return null;
                case DeviceConnectionTypeEnum.OpenRGB:
                    return new OpenRGBStream(_ambinityClient);
            }
            return null;
        }
    }
}
