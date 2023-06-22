using adrilight.Util;
using GalaSoft.MvvmLight;
using System.Drawing;

namespace adrilight.Settings
{
    public class PWMOutput : ViewModelBase, IOutputSettings
    {
        private string _outputName;
        private OutputTypeEnum _outputType;
        private int _outputID;
        private bool _isVissible = true;
        private string _outputDescription;
        private string _targetDevice;
        private string _outputUniqueID;
        private bool _outputIsVisible;
        private int _outputPowerVoltage;
        private int _outputPowerMiliamps;
        private bool _outputIsSelected = false;
        private bool _isEnabled;
        private ISlaveDevice _slaveDevice;
        private string _geometry = "generaldevice";

        public PWMOutput()
        {

        }
        public bool IsLoadingProfile { get; set; }
        public string OutputName { get => _outputName; set { Set(() => OutputName, ref _outputName, value); } }
        public string TargetDevice { get => _targetDevice; set { Set(() => TargetDevice, ref _targetDevice, value); } }
        public int OutputID { get => _outputID; set { Set(() => OutputID, ref _outputID, value); } }
        public OutputTypeEnum OutputType { get => _outputType; set { Set(() => OutputType, ref _outputType, value); } }
        public string OutputDescription { get => _outputDescription; set { Set(() => OutputDescription, ref _outputDescription, value); } }
        public bool IsVissible { get => _isVissible; set { Set(() => IsVissible, ref _isVissible, value); } }
        public bool OutputIsSelected { get => _outputIsSelected; set { Set(() => OutputIsSelected, ref _outputIsSelected, value); } }
        public string OutputUniqueID { get => _outputUniqueID; set { Set(() => OutputUniqueID, ref _outputUniqueID, value); } }
        public bool OutputIsVisible { get => _outputIsVisible; set { Set(() => OutputIsVisible, ref _outputIsVisible, value); } }
        public int OutputPowerVoltage { get => _outputPowerVoltage; set { Set(() => OutputPowerVoltage, ref _outputPowerVoltage, value); } }
        public int OutputPowerMiliamps { get => _outputPowerMiliamps; set { Set(() => OutputPowerMiliamps, ref _outputPowerMiliamps, value); } }
        public bool IsEnabled { get => _isEnabled; set { Set(() => IsEnabled, ref _isEnabled, value); } }
        public ISlaveDevice SlaveDevice { get => _slaveDevice; set { Set(() => SlaveDevice, ref _slaveDevice, value); } }
        public string Geometry { get => _geometry; set { Set(() => Geometry, ref _geometry, value); } }
        public string OutputInterface { get; set; }
        public int DisplayOutputID => OutputID + 1;
        public Rectangle Rectangle { get; set; }



        /// <summary>
        /// shortcuts for setting and getting value
        /// </summary>
        /// <param name="value"></param>
        public void SetPWM(IControlZone zone, int pwmValue)
        {
            var currentPWMMode = zone.CurrentActiveControlMode as PWMMode;
            currentPWMMode.SetPWM(pwmValue);


        }
        public int GetPWM(IControlZone zone)
        {
            var currentPWMMode = zone.CurrentActiveControlMode as PWMMode;
            return currentPWMMode.GetPWMValue();
        }
        public PWMModeEnum GetCurrentPWMMode(IControlZone zone)
        {
            var currentPWMMode = zone.CurrentActiveControlMode as PWMMode;
            return currentPWMMode.BasedOn;
        }
    }
}
