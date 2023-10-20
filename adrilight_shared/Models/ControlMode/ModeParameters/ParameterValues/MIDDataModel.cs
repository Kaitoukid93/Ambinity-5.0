using adrilight_shared.Enums;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;

namespace adrilight_shared.Models.ControlMode.ModeParameters.ParameterValues
{
    public class MIDDataModel : ViewModelBase, IParameterValue
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public VIDType ExecutionType { get; set; }
        public string Geometry { get; set; }
        public MIDFrequency Frequency { get; set; }
        private bool _isChecked = false;
        private bool _isDeleteable = true;
        private bool _isVisible = true;
        [JsonIgnore]
        public bool IsVisible { get => _isVisible; set { Set(() => IsVisible, ref _isVisible, value); } }
        [JsonIgnore]
        public bool IsDeleteable { get => _isDeleteable; set { Set(() => IsChecked, ref _isDeleteable, value); } }
        [JsonIgnore]
        public bool IsChecked { get => _isChecked; set { Set(() => IsChecked, ref _isChecked, value); } }
        [JsonIgnore]
        public string LocalPath { get; set; }
        [JsonIgnore]
        public string InfoPath { get; set; }
    }
}
