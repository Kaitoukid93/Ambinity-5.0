using adrilight_shared.Enums;
using adrilight_shared.Models.TickData;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;

namespace adrilight_shared.Models.ControlMode.ModeParameters.ParameterValues
{
    public class ChasingPattern : ViewModelBase, IParameterValue
    {

        public ChasingPattern()
        {

        }
        private Tick _tick;
        public string Name { get; set; }
        public string Owner { get; set; }
        public ChasingPatternTypeEnum Type { get; set; }
        public string Description { get; set; }
        public Tick Tick { get => _tick; set { Set(() => Tick, ref _tick, value); } }
        private bool _isChecked = false;
        private bool _isDeleteable = true;
        private bool _isVisible = true;
        [JsonIgnore]
        public string ThumbPath { get; set; }
        [JsonIgnore]
        public bool IsVisible { get => _isVisible; set { Set(() => IsVisible, ref _isVisible, value); } }
        [JsonIgnore]
        public bool IsDeleteable { get => _isDeleteable; set { Set(() => IsDeleteable, ref _isDeleteable, value); } }
        [JsonIgnore]
        public bool IsChecked { get => _isChecked; set { Set(() => IsChecked, ref _isChecked, value); } }
        [JsonIgnore]
        public string LocalPath { get; set; }
        [JsonIgnore]
        public string InfoPath { get; set; }
        [JsonIgnore]
        public bool IsSelected { get; set; }
        [JsonIgnore]
        public bool IsEditing { get; set; }
        [JsonIgnore]
        public bool IsPinned { get; set; }
    }
}

