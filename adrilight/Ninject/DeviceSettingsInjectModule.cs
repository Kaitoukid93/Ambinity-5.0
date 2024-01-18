﻿using adrilight.Manager;
using adrilight.Resources;
using adrilight.Services.CaptureEngine;
using adrilight.Services.CaptureEngine.ScreenCapture;
using adrilight.Services.DeviceDiscoveryServices;
using adrilight.Services.OpenRGBService;
using adrilight.Ticker;
using adrilight.Util;
using adrilight.View;
using adrilight.ViewModel;
using adrilight_shared.Models.Stores;
using adrilight_shared.Services;
using adrilight_shared.Settings;
using Microsoft.Win32;
using Ninject.Modules;
using Serilog;
using static adrilight.View.AllDeviceView;
using static adrilight.View.DeviceControlView;

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
            Bind<LightingProfileManagerViewModel>().ToSelf().InSingletonScope();
            Bind<DeviceManagerViewModel>().ToSelf().InSingletonScope();
            Bind<MainView>().ToSelf().InSingletonScope();
            Bind<IAmbinityClient>().To<AmbinityClient>().InSingletonScope();
            Bind<SerialDeviceDetection>().ToSelf().InSingletonScope();
            Bind<HWMonitor>().ToSelf().InSingletonScope();
            Bind<IContext>().To<WpfContext>().InSingletonScope();
            Bind<DeviceDiscovery>().ToSelf().InSingletonScope();
            Bind<DBmanager>().ToSelf().InSingletonScope();
            Bind<IDialogService>().To<DialogService>().InSingletonScope();
            Bind<CollectionItemStore>().ToSelf().InSingletonScope();

            //binding view
            Bind<ISelectableViewPart>().To<DeviceControlViewSelectableViewPart>();
            Bind<ISelectableViewPart>().To<AllDeviceViewSelectableViewPart>();
            Bind<ISelectablePage>().To<DeviceCollectionViewPage>();
            Bind<ISelectablePage>().To<DeviceAdvanceSettingsViewPage>();
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
            Bind<PlaylistDecoder>().ToSelf().InSingletonScope();
        }

    }
}


