/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:adrilight"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"
  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using adrilight.ViewModel.Automation;
using adrilight.ViewModel.Dashboard;
using adrilight.ViewModel.Profile;
using adrilight_shared.ViewModel;
using GalaSoft.MvvmLight;
using Ninject;
using System;

namespace adrilight.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    internal class ViewModelLocator
    {
        private readonly IKernel kernel;

        public ViewModelLocator()
        {
            if (!ViewModelBase.IsInDesignModeStatic)
                throw new InvalidOperationException("the parameter-less constructor of ViewModelLocator is expected to only ever be called in design time!");

            this.kernel = App.SetupDependencyInjection(true);

        }
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator(IKernel kernel)
        {
            this.kernel = kernel ?? throw new System.ArgumentNullException(nameof(kernel));
        }

        #region MainView
        public MainViewModel MainViewModel {
            get
            {
                return kernel.Get<MainViewModel>();
            }
        }
        public DashboardViewModel DashboardViewModel {
            get
            {
                return kernel.Get<DashboardViewModel>();
            }
        }
        #endregion

        #region Debug
        public DebugViewModel DebugViewModel {
            get
            {
                return kernel.Get<DebugViewModel>();
            }
        }
        #endregion

        #region Device control 

        public DeviceCanvasViewModel DeviceCanvasViewModel {
            get
            {
                return kernel.Get<DeviceCanvasViewModel>();
            }
        }
        public DeviceControlViewModel DeviceControlViewModel {
            get
            {
                return kernel.Get<DeviceControlViewModel>();
            }
        }
        public EffectControlViewModel EffectControlViewModel {
            get
            {
                return kernel.Get<EffectControlViewModel>();
            }
        }
        public VerticalMenuControlViewModel VerticalMenuControlViewModel {
            get
            {
                return kernel.Get<VerticalMenuControlViewModel>();
            }
        }
        #endregion

        #region Device manager
        public DeviceManagerViewModel DeviceManagerViewModel {
            get
            {
                return kernel.Get<DeviceManagerViewModel>();
            }
        }
        public DeviceCollectionViewModel DeviceCollectionViewModel {
            get
            {
                return kernel.Get<DeviceCollectionViewModel>();
            }
        }
        public DeviceAdvanceSettingsViewModel DeviceAdvanceSettingsViewModel {
            get
            {
                return kernel.Get<DeviceAdvanceSettingsViewModel>();
            }
        }
        #endregion

        #region LightingProfileManager
        public LightingProfileManagerViewModel LightingProfileManagerViewModel {
            get
            {
                return kernel.Get<LightingProfileManagerViewModel>();
            }
        }
        public LightingProfilePlaylistEditorViewModel LightingProfilePlaylistEditorViewModel {
            get
            {
                return kernel.Get<LightingProfilePlaylistEditorViewModel>();
            }
        }
        public LightingProfileCollectionViewModel LightingProfileCollectionViewModel {
            get
            {
                return kernel.Get<LightingProfileCollectionViewModel>();
            }
        }
        public LightingProfilePlayerViewModel LightingProfilePlayerViewModel {
            get
            {
                return kernel.Get<LightingProfilePlayerViewModel>();
            }
        }
        #endregion

        #region Automation
        public AutomationManagerViewModel AutomationManagerViewModel {
            get
            {
                return kernel.Get<AutomationManagerViewModel>();
            }
        }
        public AutomationDialogViewModel AutomationDialogViewModel {
            get
            {
                return kernel.Get<AutomationDialogViewModel>();
            }
        }
        public AutomationEditorViewModel AutomationEditorViewModel {
            get
            {
                return kernel.Get<AutomationEditorViewModel>();
            }
        }
        public HotKeySelectionViewModel HotKeySelectionViewModel {
            get
            {
                return kernel.Get<HotKeySelectionViewModel>();
            }
        }
        #endregion
        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}