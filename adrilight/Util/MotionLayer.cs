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
    internal class MotionLayer : ViewModelBase, IMotionLayer
    {
        public MotionLayer(string name, string owner, string type, string description, Motion[] motion, int totalFrame)
        {
            Name = name;
            Owner = owner;
            Type = type;
            Description = description;
            Motion = motion;
            
            TotalFrame = totalFrame;
        }

        public string Name { get; set; }
        public string Owner { get; set; }
        public Motion[] Motion { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
       
        public int TotalFrame { get; set; }
    }
}
