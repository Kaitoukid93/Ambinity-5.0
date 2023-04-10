using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using HandyControl.Tools.Extension;

namespace adrilight.Util
{
    internal class ColorCard : IPrameterValue
    {
        public ColorCard() { }
        public string Name { get; set; }
        public string Description { get; set; }
        public ColorCard(Color startColor, Color stopColor )
        {
           
            StartColor = startColor;
            StopColor = stopColor;


        }


    public Color StartColor { get; set; }
    public Color StopColor { get; set; }
        [JsonIgnore]
        public string[] ColorCode => new string[2] {StartColor.ToString(),StopColor.ToString() };
    }
}
