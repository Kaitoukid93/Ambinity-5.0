﻿using adrilight.Resources;
using adrilight.Settings;
using adrilight.Spots;
using adrilight.Util;
using adrilight.View;
using adrilight.ViewModel;
using Emgu.CV.Ocl;
using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Un4seen.BassWasapi;

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
            Bind<IAudioFrame>().To<AudioFrame>().InSingletonScope();
            Bind<IHWMonitor>().To<HWMonitor>().InSingletonScope();
            Bind<IContext>().To<WpfContext>().InSingletonScope();
            Bind<IDeviceDiscovery>().To<DeviceDiscovery>().InSingletonScope();

         
            if (generalSettings.IsMultipleScreenEnable)
                foreach (var screen in Screen.AllScreens)
                {
                    Bind<IDesktopFrame>().To<DesktopFrame>().InSingletonScope().Named(screen.DeviceName).WithConstructorArgument("screen", screen.DeviceName);
                }
            else
                Bind<IDesktopFrame>().To<DesktopFrame>().InSingletonScope().Named(Screen.AllScreens[0].DeviceName).WithConstructorArgument("screen", Screen.AllScreens[0].DeviceName);

            Bind<IRainbowTicker>().To<RainbowTicker>().InSingletonScope();


          
        }


    }
}


