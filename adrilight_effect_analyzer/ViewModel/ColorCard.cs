using Newtonsoft.Json;
using System.Windows.Media;

namespace adrilight_effect_analyzer.ViewModel
{
    public class ColorCard
    {
        public ColorCard(Color startColor, Color stopColor)
        {

            StartColor = startColor;
            StopColor = stopColor;


        }


        public Color StartColor { get; set; }
        public Color StopColor { get; set; }
        [JsonIgnore]
        public string[] ColorCode => new string[2] { StartColor.ToString(), StopColor.ToString() };
    }
}