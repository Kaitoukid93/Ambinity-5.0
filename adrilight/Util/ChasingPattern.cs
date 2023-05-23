﻿using adrilight_effect_analyzer.Model;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.Util
{
    public  class ChasingPattern:ViewModelBase, IParameterValue
    {
      
        public ChasingPattern()
        {

        }
        private Tick _tick;
        public string Name { get; set; }
        public string Owner { get; set; }
        public ChasingPatternTypeEnum Type { get; set; }
        public string Description { get; set; }
        public string Path { get; set; }
        public Tick Tick { get => _tick; set { Set(() => Tick, ref _tick, value); } }
    } 
}
