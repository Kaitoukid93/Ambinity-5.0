﻿using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.Util
{
    public class VIDDataModel: ViewModelBase, IParameterValue
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public VIDType ExecutionType { get; set; }
        public string Geometry { get; set; }
        public VIDDirrection Dirrection { get; set; }
    }
}