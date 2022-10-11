using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace adrilight.Util
{
    internal class GradientColorCard : IGradientColorCard
    {
        public GradientColorCard(string name, string owner, string type, string description, Color startColor, Color stopColor )
        {
            Name = name;
            Owner = owner;
            Type = type;
            Description = description;
            StartColor = startColor;
            StopColor = stopColor;


        }

    public string Name { get; set; }
    public string Owner { get; set; }
    public Color StartColor { get; set; }
    public Color StopColor { get; set; }
    public string Type { get; set; }
    public string Description { get; set; }
    }
}
