using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System.Windows.Media;

namespace adrilight.Util
{
    public class ColorCard : ViewModelBase, IParameterValue
    {
        public ColorCard() { }
        public string Name { get; set; }
        public string Description { get; set; }
        public ColorCard(Color startColor, Color stopColor)
        {

            StartColor = startColor;
            StopColor = stopColor;


        }

        private bool _isChecked = false;
        [JsonIgnore]
        public bool IsChecked { get => _isChecked; set { Set(() => IsChecked, ref _isChecked, value); } }
        [JsonIgnore]
        public string LocalPath { get; set; }
        public Color StartColor { get; set; }
        public Color StopColor { get; set; }
        [JsonIgnore]
        public string[] ColorCode => new string[2] { StartColor.ToString(), StopColor.ToString() };
    }
}
