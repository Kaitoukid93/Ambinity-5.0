﻿using GalaSoft.MvvmLight;
using Newtonsoft.Json;

namespace adrilight.Util
{
    public class ChasingPattern : ViewModelBase, IParameterValue
    {

        public ChasingPattern()
        {

        }
        private Tick _tick;
        public string Name { get; set; }
        public string Owner { get; set; }
        public ChasingPatternTypeEnum Type { get; set; }
        public string Description { get; set; }
        public Tick Tick { get => _tick; set { Set(() => Tick, ref _tick, value); } }
        private bool _isChecked = false;
        [JsonIgnore]
        public bool IsChecked { get => _isChecked; set { Set(() => IsChecked, ref _isChecked, value); } }
        public string LocalPath { get; set; }
    }
}
