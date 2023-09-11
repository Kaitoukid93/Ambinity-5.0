using GalaSoft.MvvmLight;
using Newtonsoft.Json;

namespace adrilight_shared.Models.ControlMode.ModeParameters.ParameterValues
{
    public class ColorCard : ViewModelBase, IParameterValue
    {
        public ColorCard() { }
        public string Name { get; set; }
        public string Description { get; set; }
        public ColorCard(System.Windows.Media.Color startColor, System.Windows.Media.Color stopColor)
        {

            StartColor = startColor;
            StopColor = stopColor;


        }

        private bool _isChecked = false;
        [JsonIgnore]
        public bool IsChecked { get => _isChecked; set { Set(() => IsChecked, ref _isChecked, value); } }
        [JsonIgnore]
        public string LocalPath { get; set; }
        [JsonIgnore]
        public string InfoPath { get; set; }
        public System.Windows.Media.Color StartColor { get; set; }
        public System.Windows.Media.Color StopColor { get; set; }
        [JsonIgnore]
        public string[] ColorCode => new string[2] { StartColor.ToString(), StopColor.ToString() };
    }
}
