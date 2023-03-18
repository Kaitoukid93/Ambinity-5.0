using adrilight.Util;
using adrilight.ViewModel;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.Settings
{
    public class LightingOutput : ViewModelBase, IOutputSettings
    {
        private string _outputName;
        private string _outputType;
        private int _outputID;
        private bool _isVissible = true;
        private string _outputDescription;
        private string _targetDevice;
        private string _outputUniqueID;
        private string _outputRGBLEDOrder;
        private bool _outputIsVisible;
        private int _outputPowerVoltage;
        private int _outputPowerMiliamps;
        private bool _outputIsSelected = false;
        private bool _isEnabled;
        private IControlZone[] _controlableZone;
        private string _geometry = "generaldevice";

        public LightingOutput()
        {

        }

        public string OutputName { get => _outputName; set { Set(() => OutputName, ref _outputName, value); } }
        public string TargetDevice { get => _targetDevice; set { Set(() => TargetDevice, ref _targetDevice, value); } }
        public int OutputID { get => _outputID; set { Set(() => OutputID, ref _outputID, value); } }
        public string OutputType { get => _outputType; set { Set(() => OutputType, ref _outputType, value); } }
        public string OutputDescription { get => _outputDescription; set { Set(() => OutputDescription, ref _outputDescription, value); } }
        public bool IsVissible { get => _isVissible; set { Set(() => IsVissible, ref _isVissible, value); } }
        public bool OutputIsSelected { get => _outputIsSelected; set { Set(() => OutputIsSelected, ref _outputIsSelected, value); } }
        public string OutputUniqueID { get => _outputUniqueID; set { Set(() => OutputUniqueID, ref _outputUniqueID, value); } }
        public string OutputRGBLEDOrder { get => _outputRGBLEDOrder; set { Set(() => OutputRGBLEDOrder, ref _outputRGBLEDOrder, value); } }
        public bool OutputIsVisible { get => _outputIsVisible; set { Set(() => OutputIsVisible, ref _outputIsVisible, value); } }
        public int OutputPowerVoltage { get => _outputPowerVoltage; set { Set(() => OutputPowerVoltage, ref _outputPowerVoltage, value); } }
        public int OutputPowerMiliamps { get => _outputPowerMiliamps; set { Set(() => OutputPowerMiliamps, ref _outputPowerMiliamps, value); } }
        public bool IsEnabled { get => _isEnabled; set { Set(() => IsEnabled, ref _isEnabled, value); } }
        public IControlZone[] ControlableZone { get => _controlableZone; set { Set(() => ControlableZone, ref _controlableZone, value); } }
        public string Geometry { get => _geometry; set { Set(() => Geometry, ref _geometry, value); } }
        public string OutputInterface { get; set; }





        /// <summary>
        /// shortcuts for setting and getting value
        /// </summary>
        /// <param name="value"></param>
        public void SetBrightness(IControlZone zone, int value)
        {
            var currentLightingMode = zone.CurrentActiveControlMode as LightingMode;
            currentLightingMode.SetBrightness(value);
        }
        public int GetBrightness(IControlZone zone)
        {
            var currentLightingMode = zone.CurrentActiveControlMode as LightingMode;
            return currentLightingMode.GetBrightness();

        }
        public LightingModeEnum GetCurrentActiveLightingMode(IControlZone zone)
        {
            return (zone.CurrentActiveControlMode as LightingMode).BasedOn;
        }
      
    }
}
