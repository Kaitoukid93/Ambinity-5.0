using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight_effect_analyzer.Model
{
    public class Motion 
    {
        public Motion(int FrameCount)
        {
            Frames = new Frame[FrameCount];
        }
        public Motion(string name)
        { 
            Name = name;
        }
        public Motion()
        {

        }
        public Frame[] Frames { get; set; }
        public string GUID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Owner { get; set; }

    }
}
