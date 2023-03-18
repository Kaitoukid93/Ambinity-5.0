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
    internal class ColorPalette : ViewModelBase, IColorPalette, IOnlineItemModel
    {
        public ColorPalette(string name, string owner, string type, string description, System.Windows.Media.Color[] colors)
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
        public string Path { get; set; }
        public BitmapImage Thumb { get; set; }
        public List<BitmapImage> Screenshots { get; set; }
        public string MarkDownDescription { get; set; }
        public string SubType { get; set; }
        public string FormatVersion { get; set; }
        public void SetColor(int index, Color color)
        {
            Colors[index] = color;


            RaisePropertyChanged(nameof(Colors));


        }
    }
}
