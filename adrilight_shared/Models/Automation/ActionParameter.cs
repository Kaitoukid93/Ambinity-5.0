using GalaSoft.MvvmLight;

namespace adrilight_shared.Models.Automation
{
    public class ActionParameter : ViewModelBase
    {
        private string _name;
        private string _geometry;

        private string _type;
        private object _value;//represent UID of profile or state of brightness
        //private string _targetDeviceType;
        public string Name { get => _name; set { Set(() => Name, ref _name, value); } }
        public string Geometry { get => _geometry; set { Set(() => Geometry, ref _geometry, value); } }

        public string Type { get => _type; set { Set(() => Type, ref _type, value); } }
        //public string TargetDeviceType { get => _targetDeviceType; set { Set(() => TargetDeviceType, ref _targetDeviceType, value); } }
        public object Value { get => _value; set { Set(() => Value, ref _value, value); } }// this represent profile UID or state of brightness control???
    }
}
