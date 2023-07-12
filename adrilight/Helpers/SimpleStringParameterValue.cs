using adrilight.Util;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;

namespace adrilight.Helpers
{
    public class DancingModeParameterValue : ViewModelBase, IParameterValue
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DancingModeEnum Type { get; set; }
        private bool _isChecked = false;
        [JsonIgnore]
        public bool IsChecked { get => _isChecked; set { Set(() => IsChecked, ref _isChecked, value); } }
        [JsonIgnore]
        public string LocalPath { get; set; }
    }
}