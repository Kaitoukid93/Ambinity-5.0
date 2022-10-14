using adrilight.Resources;
using adrilight.Spots;
using adrilight.Util;
using adrilight.View;
using adrilight.ViewModel;
using Ninject.Modules;
using System.Collections.Generic;
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
            var existedDevices = settingsManager.LoadDeviceIfExists();
            Bind<IGeneralSettings>().ToConstant(generalSettings);
            Bind<MainViewViewModel>().ToSelf().InSingletonScope();
            Bind<MainView>().ToSelf().InSingletonScope();
            Bind<IAmbinityClient>().To<AmbinityClient>().InSingletonScope();
            Bind<ISerialDeviceDetection>().To<SerialDeviceDetection>().InSingletonScope();
            Bind<IAudioFrame>().To<AudioFrame>().InSingletonScope();
            Bind<IHWMonitor>().To<HWMonitor>().InSingletonScope();
            Bind<IContext>().To<WpfContext>().InSingletonScope();

            int index = 0;
            foreach (var screen in Screen.AllScreens)
            {
                Bind<IDesktopFrame>().To<DesktopFrame>().InSingletonScope().WithConstructorArgument("screen", index++);
            }



            Bind<IRainbowTicker>().To<RainbowTicker>().InSingletonScope();


            if (existedDevices != null)
            {
                if (existedDevices.Count > 0)
                {
                    foreach (var device in existedDevices)
                    {
                        var iD = device.DeviceUID.ToString();

                        Bind<IDeviceSettings>().ToConstant(device).Named(iD);




                        foreach (var output in device.AvailableOutputs)
                        {
                            var outputID = iD + output.OutputID.ToString();
                            Bind<IOutputSettings>().ToConstant(output).Named(outputID);

                        }

                        var unionOutput = device.UnionOutput;

                        if (unionOutput != null)
                        {
                            var unionOutputID = iD + unionOutput.OutputID.ToString();
                            Bind<IOutputSettings>().ToConstant(unionOutput).Named(unionOutputID);

                        }


                    }
                }
            }
            else
            {
                // require user to add device then restart the app
            }

        }


    }
}


