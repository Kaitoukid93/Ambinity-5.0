using adrilight.Manager;
using adrilight.Resources;
using adrilight.Services.CaptureEngine;
using adrilight.Services.CaptureEngine.ScreenCapture;
using adrilight.Services.DeviceDiscoveryServices;
using adrilight.Services.OpenRGBService;
using adrilight.Ticker;
using adrilight.Util;
using adrilight.View;
using adrilight.View.Screens.Store;
using adrilight.ViewModel;
using adrilight.ViewModel.AdrilightStore;
using adrilight.ViewModel.Automation;
using adrilight.ViewModel.Dashboard;
using adrilight.ViewModel.Profile;
using adrilight.ViewModel.Splash;
using adrilight_shared.Models.Automation;
using adrilight_shared.Models.Device;
using adrilight_shared.Models.KeyboardHook;
using adrilight_shared.Models.Lighting;
using adrilight_shared.Models.Store;
using adrilight_shared.Models.Stores;
using adrilight_shared.Services;
using adrilight_shared.Services.AdrilightStoreService;
using adrilight_shared.Settings;
using adrilight_shared.ViewModel;
using Microsoft.Win32;
using Ninject.Modules;
using Renci.SshNet;
using Serilog;
using System.CodeDom.Compiler;
using static adrilight.View.AutomationCollectionView;
using static adrilight.View.AutomationEditorView;
using static adrilight.View.DashboardView;
using static adrilight.View.DeviceControlView;
using static adrilight.View.OnlineItemDetailView;
using static adrilight.View.PlaylistEditorView;
using static adrilight.View.Screens.LightingProfile.ManagerCollectionView;
using static adrilight.View.Screens.Store.StoreItemsCollectionView;
using static adrilight.View.StoreHomePageView;

namespace adrilight.Ninject
{
    class ModuleInjection : NinjectModule
    {
        
        public override void Load()
        {
            var settingsManager = new UserSettingsManager();
            var generalSettings = settingsManager.LoadIfExists() ?? settingsManager.MigrateOrDefault();
            string HKLMWinNTCurrent = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion";
            string osBuild = Registry.GetValue(HKLMWinNTCurrent, "CurrentBuildNumber", "").ToString();

            Bind<SplashScreenViewModel>().ToSelf().InSingletonScope();

            Bind<DashboardViewModel>().ToSelf().InSingletonScope();
            Bind<MainViewModel>().ToSelf().InSingletonScope();
            //provider
            Bind<IGeneralSettings>().ToConstant(generalSettings);   
            //device control 
            Bind<DeviceCanvasViewModel>().ToSelf().InSingletonScope();
            Bind<DeviceControlViewModel>().ToSelf().InSingletonScope();
            Bind<EffectControlViewModel>().ToSelf().InSingletonScope();
            Bind<VerticalMenuControlViewModel>().ToSelf().InSingletonScope();
            Bind<DeviceControlEvent>().ToSelf().InSingletonScope();
            ////device manager
            Bind<DeviceManager>().ToSelf().InSingletonScope();
            Bind<DeviceDBManager>().ToSelf().InSingletonScope();
            Bind<DeviceConnectionManager>().ToSelf().InSingletonScope();
            Bind<DeviceManagerViewModel>().ToSelf().InSingletonScope();
            Bind<DeviceCollectionViewModel>().ToSelf().InSingletonScope();
            Bind<DeviceAdvanceSettingsViewModel>().ToSelf().InSingletonScope();
            Bind<DeviceHardwareSettings>().ToSelf().InSingletonScope();
            Bind<DeviceLightingServiceManager>().ToSelf().InSingletonScope();

            //ftp client
            Bind<AdrilightDeviceManagerSFTPClient>().ToSelf().InSingletonScope();
            Bind<AdrilightStoreSFTPClient>().ToSelf().InSingletonScope();
            //profile manager
            Bind<LightingProfileManagerViewModel>().ToSelf().InSingletonScope();
            Bind<LightingProfilePlaylistEditorViewModel>().ToSelf().InSingletonScope();
            Bind<LightingProfileCollectionViewModel>().ToSelf().InSingletonScope();
            Bind<LightingProfilePlayerViewModel>().ToSelf().InSingletonScope();
            Bind<LightingProflileDBManager>().ToSelf().InSingletonScope();

            //service
            Bind<AmbinityClient>().ToSelf().InSingletonScope();
            Bind<HWMonitor>().ToSelf().InSingletonScope();
            Bind<IContext>().To<WpfContext>().InSingletonScope();
            Bind<DeviceDiscovery>().ToSelf().InSingletonScope();
            Bind<DBmanager>().ToSelf().InSingletonScope();
            Bind<DeviceConstructor>().ToSelf().InSingletonScope();  

            //automation
            Bind<AutomationManagerViewModel>().ToSelf().InSingletonScope();
            Bind<AutomationDialogViewModel>().ToSelf().InSingletonScope();
            Bind<AutomationEditorViewModel>().ToSelf().InSingletonScope();
            Bind<AutomationCollectionView>().ToSelf().InSingletonScope();
            Bind<AutomationCollectionViewModel>().ToSelf().InSingletonScope();
            Bind<AutomationManager>().ToSelf().InSingletonScope();
            Bind<AutomationExecutor>().ToSelf().InSingletonScope();
            Bind<AutomationDBManager>().ToSelf().InSingletonScope();

            //store
            Bind<AdrilightStoreHomePageViewModel>().ToSelf().InSingletonScope();
            Bind<AdrilightStoreItemDetailViewModel>().ToSelf().InSingletonScope();
            Bind<AdrilightStoreItemsCollectionViewModel>().ToSelf().InSingletonScope();
            Bind<SearchBarViewModel>().ToSelf().InSingletonScope();
            Bind<StoreCategoriesViewModel>().ToSelf().InSingletonScope();
            Bind<AdrilightStoreViewModel>().ToSelf().InSingletonScope();

            Bind<IDialogService>().To<DialogService>().InSingletonScope();

            //binding view
            Bind<ISelectableViewPart>().To<DashboardViewSelectableViewPart>();
            Bind<ISelectableViewPart>().To<DeviceControlViewSelectableViewPart>();
            Bind<ISelectablePage>().To<DeviceCollectionViewPage>().WhenInjectedInto(typeof(DeviceManagerViewModel));
            Bind<ISelectablePage>().To<DeviceAdvanceSettingsViewPage>().WhenInjectedInto(typeof(DeviceManagerViewModel));
            Bind<ISelectablePage>().To<DeviceLoadingViewPage>().WhenInjectedInto(typeof(DeviceManagerViewModel));

            Bind<ISelectablePage>().To<ManagerCollectionViewPage>().WhenInjectedInto(typeof(LightingProfileManagerViewModel));
            Bind<ISelectablePage>().To<PlaylistEditorViewPage>().WhenInjectedInto(typeof(LightingProfileManagerViewModel));


            Bind<ISelectablePage>().To<AutomationCollectionViewPage>().WhenInjectedInto(typeof(AutomationManagerViewModel));
            Bind<ISelectablePage>().To<AutomationEditorViewPage>().WhenInjectedInto(typeof(AutomationManagerViewModel));

            Bind<ISelectablePage>().To<StoreHomePageViewPage>().WhenInjectedInto(typeof(AdrilightStoreViewModel));
            Bind<ISelectablePage>().To<StoreItemsCollectionViewPage>().WhenInjectedInto(typeof(AdrilightStoreViewModel));
            Bind<ISelectablePage>().To<OnlineItemDetailViewPage>().WhenInjectedInto(typeof(AdrilightStoreViewModel));

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


