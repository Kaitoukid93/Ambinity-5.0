using adrilight.Resources;
using adrilight.Util;
using adrilight.View;
using adrilight.ViewModel;
using Ninject.Modules;
using System.Windows.Forms;

namespace adrilight.Ninject
{
    class DeviceSettingsInjectModule : NinjectModule
    {
        public override void Load()
        {
            var settingsManager = new UserSettingsManager();
            var generalSettings = settingsManager.LoadIfExists() ?? settingsManager.MigrateOrDefault();
            //var existedDevices = settingsManager.LoadDeviceIfExists();
            Bind<IGeneralSettings>().ToConstant(generalSettings);
            Bind<MainViewViewModel>().ToSelf().InSingletonScope();
            Bind<MainView>().ToSelf().InSingletonScope();
            Bind<IAmbinityClient>().To<AmbinityClient>().InSingletonScope();
            Bind<ISerialDeviceDetection>().To<SerialDeviceDetection>().InSingletonScope();
            Bind<IHWMonitor>().To<HWMonitor>().InSingletonScope();
            Bind<IContext>().To<WpfContext>().InSingletonScope();
            Bind<IDeviceDiscovery>().To<DeviceDiscovery>().InSingletonScope();


            if (generalSettings.IsMultipleScreenEnable)
                foreach (var screen in Screen.AllScreens)
                {
                    Bind<ICaptureEngine>().To<DesktopFrame>().InSingletonScope().Named(screen.DeviceName).WithConstructorArgument("deviceName", screen.DeviceName);
                }
            else
                Bind<ICaptureEngine>().To<DesktopFrame>().InSingletonScope().Named(Screen.AllScreens[0].DeviceName).WithConstructorArgument("deviceName", Screen.AllScreens[0].DeviceName);
            Bind<ICaptureEngine>().To<AudioFrame>().InSingletonScope();
            Bind<IRainbowTicker>().To<RainbowTicker>().InSingletonScope();

        }

    }
}


