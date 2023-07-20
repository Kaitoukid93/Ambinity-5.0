using adrilight.Resources;
using adrilight.Util;
using adrilight.View;
using adrilight.ViewModel;
using Microsoft.Win32;
using Ninject.Modules;
using Serilog;
using System.Windows.Forms;

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


            if (generalSettings.IsMultipleScreenEnable)
                foreach (var screen in Screen.AllScreens)
                {
                    if (generalSettings.ScreenCapturingMethod == 0)
                    {
                        if (osBuild == "22000" || osBuild == "22621")
                        {
                            Log.Information("This is Windows 11 Machine, Injecting WCG", osBuild);
                            Bind<ICaptureEngine>().To<DesktopFrame>().InSingletonScope().Named(screen.DeviceName).WithConstructorArgument("deviceName", screen.DeviceName);
                        }
                        else
                        {
                            Log.Information("This is Windows 10 Machine, Injecting DXGI", osBuild);
                            Bind<ICaptureEngine>().To<DesktopFrameDXGI>().InSingletonScope().Named(screen.DeviceName).WithConstructorArgument("deviceName", screen.DeviceName);
                        }
                    }
                    else if (generalSettings.ScreenCapturingMethod == 1) //DXGI
                    {
                        Log.Information("Manual Capturing Method Selection, Injecting DXGI", osBuild);
                        Bind<ICaptureEngine>().To<DesktopFrameDXGI>().InSingletonScope().Named(screen.DeviceName).WithConstructorArgument("deviceName", screen.DeviceName);
                    }
                    else if (generalSettings.ScreenCapturingMethod == 2) //WGC
                    {
                        Log.Information("Manual Capturing Method Selection, Injecting WCG", osBuild);
                        Bind<ICaptureEngine>().To<DesktopFrame>().InSingletonScope().Named(screen.DeviceName).WithConstructorArgument("deviceName", screen.DeviceName);
                    }


                }
            else
            {
                if (generalSettings.ScreenCapturingMethod == 0)
                {
                    if (osBuild == "22000" || osBuild == "22621")
                    {
                        Log.Information("This is Windows 11 Machine, Injecting WCG", osBuild);
                        Bind<ICaptureEngine>().To<DesktopFrame>().InSingletonScope().Named(Screen.AllScreens[0].DeviceName).WithConstructorArgument("deviceName", Screen.AllScreens[0].DeviceName);
                    }
                    else
                    {
                        Log.Information("This is Windows 10 Machine, Injecting DXGI", osBuild);
                        Bind<ICaptureEngine>().To<DesktopFrameDXGI>().InSingletonScope().Named(Screen.AllScreens[0].DeviceName).WithConstructorArgument("deviceName", Screen.AllScreens[0].DeviceName);
                    }
                }
                else if (generalSettings.ScreenCapturingMethod == 1) //DXGI
                {
                    Log.Information("Manual Capturing Method Selection, Injecting DXGI", osBuild);
                    Bind<ICaptureEngine>().To<DesktopFrameDXGI>().InSingletonScope().Named(Screen.AllScreens[0].DeviceName).WithConstructorArgument("deviceName", Screen.AllScreens[0].DeviceName);
                }
                else if (generalSettings.ScreenCapturingMethod == 2) //WGC
                {
                    Log.Information("Manual Capturing Method Selection, Injecting WCG", osBuild);
                    Bind<ICaptureEngine>().To<DesktopFrame>().InSingletonScope().Named(Screen.AllScreens[0].DeviceName).WithConstructorArgument("deviceName", Screen.AllScreens[0].DeviceName);
                }
            }
            Bind<ICaptureEngine>().To<AudioFrame>().InSingletonScope();
            Bind<RainbowTicker>().ToSelf().InSingletonScope();

        }

    }
}


