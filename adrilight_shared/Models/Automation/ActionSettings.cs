using GalaSoft.MvvmLight;

namespace adrilight.Settings.Automation
{
    public class ActionSettings : ViewModelBase
    {
        public ActionSettings()
        {

        }
        private string _targetDeviceUID;
        private string _targetDeviceType;
        private string _targetDeviceName;

        private ActionType _actionType;
        private ActionParameter _actionParameter;
        public string TargetDeviceUID { get => _targetDeviceUID; set { Set(() => TargetDeviceUID, ref _targetDeviceUID, value); } }
        public string TargetDeviceType { get => _targetDeviceType; set { Set(() => TargetDeviceType, ref _targetDeviceType, value); } }
        public string TargetDeviceName { get => _targetDeviceName; set { Set(() => TargetDeviceName, ref _targetDeviceName, value); } }
        public ActionType ActionType { get => _actionType; set { Set(() => ActionType, ref _actionType, value); } }
        public ActionParameter ActionParameter { get => _actionParameter; set { Set(() => ActionParameter, ref _actionParameter, value); } }



    }
}
