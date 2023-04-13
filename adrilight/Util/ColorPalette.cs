using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace adrilight.Util
{
    internal class ColorPalette : ViewModelBase, IParameterValue
    {
        public ColorPalette(string name, string owner, string type, string description, System.Windows.Media.Color[] colors )
        {
            Name = name;
            Owner = owner;
            Type = type;
            Description = description;
            Colors = colors;


        }
        public ColorPalette()
        {

        }

    public string Name { get; set; }
    public string Owner { get; set; }
    public System.Windows.Media.Color[] Colors { get; set; }
    public string Type { get; set; }
    public string Description { get; set; }
    public void SetColor(int index,Color color)
        {
            Colors[index] = color;
           RaisePropertyChanged(nameof(Colors));     
        }
    }
}
