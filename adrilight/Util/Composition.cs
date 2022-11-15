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
    internal class Composition : ViewModelBase, IComposition
    {
        public Composition(string name, string owner, string type, string description, Motion[] layers,int duration, int totalFrame)
        {
            Name = name;
            Owner = owner;
            Type = type;
            Description = description;
            Layers = layers;
            Duration = duration;
            TotalFrame = totalFrame;
        }

        public string Name { get; set; }
        public string Owner { get; set; }
        public IMotionLayer[] Layers { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public int Duration { get; set; }
        public int TotalFrame { get; set; }
    }
}
