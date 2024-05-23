using adrilight.Services.CaptureEngine;
using adrilight.Services.LightingEngine;
using adrilight.Ticker;
using adrilight_shared.Models.ControlMode.Mode;
using adrilight_shared.Models.Device;
using adrilight_shared.Settings;
using System.Collections.Generic;

namespace adrilight.Manager
{
    public class DeviceLightingServiceManager
    {
        public DeviceLightingServiceManager(
            IGeneralSettings generalSettings,
            ICaptureEngine[] desktopFrame,
            RainbowTicker rainbowTicker)
        {
            _desktopFrames = desktopFrame;
            _generalSettings = generalSettings;
            _rainbowTicker = rainbowTicker;
        }
        private ICaptureEngine[] _desktopFrames;
        private IGeneralSettings _generalSettings;
        private RainbowTicker _rainbowTicker;
        public List<ILightingEngine> CreateLightingService(IDeviceSettings device)
        {
            List<ILightingEngine> engines = new List<ILightingEngine>();
            //create new engine for each zone
            foreach (var slaveDevice in device.AvailableLightingDevices)
            {
                foreach (var zone in slaveDevice.ControlableZones)
                {
                    var procs = new List<ILightingEngine>() { new DesktopDuplicatorReader(_generalSettings, _desktopFrames, zone),
                        new StaticColor(_generalSettings, zone, _rainbowTicker),
                        new Rainbow(_generalSettings, zone, device, _rainbowTicker),
                        new Animation(_generalSettings, zone, device, _rainbowTicker),
                        new Music(_generalSettings, _desktopFrames, zone, _rainbowTicker),
                        new Gifxelation(_generalSettings, zone, _rainbowTicker) };
                    foreach (var proc in procs)
                    {
                        if ((zone.CurrentActiveControlMode as LightingMode).BasedOn == proc.Type)
                            proc.Refresh();
                    }

                }
            }

            return engines;
        }

        private static void SuspendLightingServices(IDeviceSettings device)
        {

        }

        private static void ResumeLightingService(IDeviceSettings device)
        {

        }

    }
}
