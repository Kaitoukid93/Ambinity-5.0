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
        [JsonIgnore]
        public bool IsChecked { get => _isChecked; set { Set(() => IsChecked, ref _isChecked, value); } }
        public string LocalPath { get; set; }
    }
}

