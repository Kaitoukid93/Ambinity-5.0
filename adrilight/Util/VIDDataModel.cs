﻿using GalaSoft.MvvmLight;
using Newtonsoft.Json;

namespace adrilight.Util
{
    public class VIDDataModel : ViewModelBase, IParameterValue
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public VIDType ExecutionType { get; set; }
        public string Geometry { get; set; }
        public VIDDirrection Dirrection { get; set; }
        private bool _isChecked = false;
        [JsonIgnore]
        public bool IsChecked { get => _isChecked; set { Set(() => IsChecked, ref _isChecked, value); } }
        [JsonIgnore]
        public string LocalPath { get; set; }
    }
}
