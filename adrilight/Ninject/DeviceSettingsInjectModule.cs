using adrilight.Manager;
using adrilight.Resources;
using adrilight.Services.CaptureEngine;
using adrilight.Services.CaptureEngine.ScreenCapture;
using adrilight.Services.DeviceDiscoveryServices;
using adrilight.Services.OpenRGBService;
using adrilight.Ticker;
using adrilight.Util;
using adrilight.View;
using adrilight.ViewModel;
using adrilight.ViewModel.Automation;
using adrilight.ViewModel.Dashboard;
using adrilight.ViewModel.DeviceManager;
using adrilight.ViewModel.Profile;
using adrilight_shared.Models.Automation;
using adrilight_shared.Models.Device;
using adrilight_shared.Models.KeyboardHook;
using adrilight_shared.Models.Stores;
using adrilight_shared.Services;
using adrilight_shared.Settings;
using adrilight_shared.ViewModel;
using Microsoft.Win32;
using Ninject.Modules;
using Serilog;
using static adrilight.View.AllDeviceView;
using static adrilight.View.DeviceControlView;
using static adrilight.View.PlayListCollectionView;
using static adrilight.View.PlaylistEditorView;
using static adrilight.View.ProfileCollectionView;

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
            
            //device control 
            Bind<DeviceCanvasViewModel>().ToSelf().InSingletonScope();
            Bind<DeviceControlViewModel>().ToSelf().InSingletonScope();
            Bind<EffectControlViewModel>().ToSelf().InSingletonScope();
            Bind<VerticalMenuControlViewModel>().ToSelf().InSingletonScope();
            Bind<DeviceControlEvent>().ToSelf().InSingletonScope();
            ////
            Bind<DeviceManagerViewModel>().ToSelf().InSingletonScope();
            Bind<DeviceCollectionViewModel>().ToSelf().InSingletonScope();
            Bind<DeviceAdvanceSettingsViewModel>().ToSelf().InSingletonScope();
            Bind<DeviceDBManager>().ToSelf().InSingletonScope();
            Bind<DeviceHardwareSettings>().ToSelf().InSingletonScope();

            Bind<LightingProfileManagerViewModel>().ToSelf().InSingletonScope();
            Bind<LightingProfilePlaylistEditorViewModel>().ToSelf().InSingletonScope();
            Bind<LightingProfileCollectionViewModel>().ToSelf().InSingletonScope();
            Bind<LightingProfilePlayerViewModel>().ToSelf().InSingletonScope();

            Bind<MainView>().ToSelf().InSingletonScope();
            Bind<AmbinityClient>().ToSelf().InSingletonScope();
            Bind<SerialDeviceDetection>().ToSelf().InSingletonScope();
            Bind<HWMonitor>().ToSelf().InSingletonScope();
            Bind<IContext>().To<WpfContext>().InSingletonScope();
            Bind<DeviceDiscovery>().ToSelf().InSingletonScope();
            Bind<DBmanager>().ToSelf().InSingletonScope();
            Bind<DeviceConstructor>().ToSelf().InSingletonScope();  

            Bind<AutomationManagerViewModel>().ToSelf().InSingletonScope();
            Bind<AutomationDialogViewModel>().ToSelf().InSingletonScope();
            Bind<AutomationEditorViewModel>().ToSelf().InSingletonScope();
            Bind<AutomationCollectionView>().ToSelf().InSingletonScope();
            Bind<AutomationManager>().ToSelf().InSingletonScope();
            Bind<AutomationExecutor>().ToSelf().InSingletonScope();

            Bind<IDialogService>().To<DialogService>().InSingletonScope();

            //binding view
            Bind<ISelectableViewPart>().To<DeviceControlViewSelectableViewPart>();
            Bind<ISelectableViewPart>().To<AllDeviceViewSelectableViewPart>();
            Bind<ISelectablePage>().To<DeviceCollectionViewPage>().WhenInjectedInto(typeof(DeviceManagerViewModel));
            Bind<ISelectablePage>().To<DeviceAdvanceSettingsViewPage>().WhenInjectedInto(typeof(DeviceManagerViewModel));
            Bind<ISelectablePage>().To<DeviceLoadingViewPage>().WhenInjectedInto(typeof(DeviceManagerViewModel));

            Bind<ISelectablePage>().To<PlayListCollectionViewPage>().WhenInjectedInto(typeof(LightingProfileManagerViewModel));
            Bind<ISelectablePage>().To<ProfileCollectionViewPage>().WhenInjectedInto(typeof(LightingProfileManagerViewModel));
            Bind<ISelectablePage>().To<PlaylistEditorViewPage>().WhenInjectedInto(typeof(LightingProfileManagerViewModel));
            Bind< KeyboardHookManagerSingleton>().ToSelf().InSingletonScope();
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


