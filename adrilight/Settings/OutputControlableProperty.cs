using adrilight.Util;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Documents;

namespace adrilight
{
    internal class OutputControlableProperty : ViewModelBase, IOutputControlableProperty
    {
        public OutputControlableProperty()
        {
            AvailableControlMode = new List<IControlMode>();
        }
        private int _currentActiveControlModeIndex;
        private IControlMode _currentActiveControlMode;
        public string Name { get; set; }
        public string Description { get; set; }
        public OutputControlablePropertyEnum Type { get; set; }
        /// <summary>
        /// this list contains all available control mode ex: auto-manual,music-rainbow-capturing...
        /// </summary>
        public List<IControlMode> AvailableControlMode { get; set; }
        [JsonIgnore]
        public IControlMode CurrentActiveControlMode { get; set; }




        public int CurrentActiveControlModeIndex { get => _currentActiveControlModeIndex; set { if(value>=0) Set(() => CurrentActiveControlModeIndex, ref _currentActiveControlModeIndex, value); OnActiveControlModeChanged(); } }

        private void OnActiveControlModeChanged()
        {
            if (CurrentActiveControlModeIndex >= 0)
            {
                CurrentActiveControlMode = AvailableControlMode[CurrentActiveControlModeIndex];
                RaisePropertyChanged(nameof(CurrentActiveControlMode));
            }
        }

        public string Icon { get; set; }
    }
}