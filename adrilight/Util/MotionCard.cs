﻿using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using TimeLineTool;

namespace adrilight.Util
{
    public class MotionCard : ITimeLineDataItem // for displaying motion at rainbow control panel
    {
       

        public string Owner { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string Source { get; set; } // to load motion


        //timeline data item inheritance
        public double StartFrame { get; set; }
        public double EndFrame { get; set; }
        public Boolean TimelineViewExpanded { get; set; }
        public String Name { get; set; }
        public double TrimStart { get; set; }
        public double TrimEnd { get; set; }
        public double OriginalDuration { get; set; }
    }
}
