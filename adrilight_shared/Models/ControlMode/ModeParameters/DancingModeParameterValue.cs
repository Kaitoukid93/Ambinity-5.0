using adrilight_shared.Enums;
using adrilight_shared.Models.ControlMode.ModeParameters.ParameterValues;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;

namespace adrilight_shared.Models.ControlMode.ModeParameters
{
    public class DancingModeParameterValue : ViewModelBase, IParameterValue
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DancingModeEnum Type { get; set; }
        private bool _isChecked = false;
        private bool _isDeleteable = true;
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