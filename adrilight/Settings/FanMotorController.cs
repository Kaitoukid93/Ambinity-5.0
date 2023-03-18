using adrilight.Util;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace adrilight.Settings
{
    public class FanMotorController : ViewModelBase, IControlZone
    {
  



        public BitmapImage Thumb { get; set; }



        public FanMotorController()
        {
            AvailableControlMode = new List<IControlMode>();
        }
        private int _currentActiveControlModeIndex;
        public string Name { get; set; }
        public string Description { get; set; }
        public List<IControlMode> AvailableControlMode { get; set; }
        [JsonIgnore]
        public IControlMode CurrentActiveControlMode { get; set; }

        public int CurrentActiveControlModeIndex { get => _currentActiveControlModeIndex; set { if (value >= 0) Set(() => CurrentActiveControlModeIndex, ref _currentActiveControlModeIndex, value); OnActiveControlModeChanged(); } }

        private void OnActiveControlModeChanged()
        {
            if (CurrentActiveControlModeIndex >= 0)
            {
                CurrentActiveControlMode = AvailableControlMode[CurrentActiveControlModeIndex];
                RaisePropertyChanged(nameof(CurrentActiveControlMode));
            }
        }


    }
}
