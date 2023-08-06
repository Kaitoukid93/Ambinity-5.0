using adrilight.Resources;
using adrilight.Util;
using adrilight.View;
using adrilight.ViewModel;
using Microsoft.Win32;
using Ninject.Modules;
using Serilog;

namespace adrilight.Ninject
{
    class DeviceSettingsInjectModule : NinjectModule
    {
        public override void Load()
        {
            var settingsManager = new UserSettingsManager();
            var generalSettings = settingsManager.LoadIfExists() ?? settingsManager.MigrateOrDefault();
            string HKLMWinNTCurrent = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion";
            string osBuild = Registry.GetValue(HKLMWinNTCurrent, "CurrentBuildNumber", "").ToString();
            Bind<IGeneralSettings>().ToConstant(generalSettings);
            Bind<MainViewViewModel>().ToSelf().InSingletonScope();
            Bind<MainView>().ToSelf().InSingletonScope();
            Bind<IAmbinityClient>().To<AmbinityClient>().InSingletonScope();
            Bind<SerialDeviceDetection>().ToSelf().InSingletonScope();
            Bind<HWMonitor>().ToSelf().InSingletonScope();
            Bind<IContext>().To<WpfContext>().InSingletonScope();
            Bind<DeviceDiscovery>().ToSelf().InSingletonScope();




            if (generalSettings.ScreenCapturingMethod == 0)
            {
                if (osBuild == "22000" || osBuild == "22621")
                {
                    Log.Information("This is Windows 11 Machine, Injecting WCG", osBuild);
                    Bind<ICaptureEngine>().To<DesktopFrame>().InSingletonScope();
                }
                else
                {
                    Log.Information("This is Windows 10 Machine, Injecting DXGI", osBuild);
                    Bind<ICaptureEngine>().To<DesktopFrameDXGI>().InSingletonScope();
                }
            }
            else if (generalSettings.ScreenCapturingMethod == 1) //DXGI
            {
                Log.Information("Manual Capturing Method Selection, Injecting DXGI", osBuild);
                Bind<ICaptureEngine>().To<DesktopFrameDXGI>().InSingletonScope();
            }
            else if (generalSettings.ScreenCapturingMethod == 2) //WGC
            {
                Log.Information("Manual Capturing Method Selection, Injecting WCG", osBuild);
                Bind<ICaptureEngine>().To<DesktopFrame>().InSingletonScope();
            }




            Bind<ICaptureEngine>().To<AudioFrame>().InSingletonScope();
            Bind<RainbowTicker>().ToSelf().InSingletonScope();

        }

    }
}


