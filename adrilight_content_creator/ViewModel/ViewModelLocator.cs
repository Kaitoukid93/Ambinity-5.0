using adrilight_shared.ViewModel;
using GalaSoft.MvvmLight;
using Ninject;
using System;

namespace adrilight_content_creator.ViewModel
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

            //this.kernel = App.SetupDependencyInjection(true);

        }
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator(IKernel kernel)
        {
            this.kernel = kernel ?? throw new System.ArgumentNullException(nameof(kernel));
        }

        public MainViewModel MainViewModel
        {
            get
            {
                return kernel.Get<MainViewModel>();
            }
        }
        public DeviceUtilViewModel DeviceUtilViewModel
        {
            get { return kernel.Get<DeviceUtilViewModel>(); }
        }
        public DeviceExporterViewModel DeviceExporterViewModel
        {
            get
            {
                return kernel.Get<DeviceExporterViewModel>();
            }
        }
        public OutputMappingViewModel OutputMappingViewModel
        {
            get
            {
                return kernel.Get<OutputMappingViewModel>();
            }
        }
        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}