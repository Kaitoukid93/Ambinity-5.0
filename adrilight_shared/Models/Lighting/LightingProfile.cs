using adrilight_shared.Models.ControlMode.Mode;
using adrilight_shared.Models.DashboardItem;
using adrilight_shared.Models.Device;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace adrilight_shared.Models.Lighting
{
    public class LightingProfile : ViewModelBase, IDashboardItem, IGenericCollectionItem
    {
        public LightingProfile()
        {

        }
        private bool _isPinned = false;
        public bool IsPinned { get => _isPinned; set { Set(() => IsPinned, ref _isPinned, value); } }
        private bool _isSelected;
        private bool _isEditing;
        public string Name { get; set; }
        public string Description { get; set; }
        public string Owner { get; set; }
        public string ProfileUID { get; set; }
        private bool _isChecked = false;
        private bool _isDeleteable = true;
        private bool _isPlaying;
        private TimeSpan _duration = TimeSpan.FromSeconds(60);
        public LightingMode ControlMode { get; set; }
        public List<string> TargetDevicesUID { get; set; }
        public TimeSpan Duration { get => _duration; set { Set(() => Duration, ref _duration, value); } }
        [JsonIgnore]
        public ObservableCollection<IDeviceSettings> TargetDevices { get; set; }
        [JsonIgnore]
        public bool IsSelected { get => _isSelected; set { Set(() => IsSelected, ref _isSelected, value); } }
        [JsonIgnore]
        public bool IsPlaying { get => _isPlaying; set { Set(() => IsPlaying, ref _isPlaying, value); } }
        [JsonIgnore]
        public bool IsDeleteable { get => _isDeleteable; set { Set(() => IsDeleteable, ref _isDeleteable, value); } }
        [JsonIgnore]
        public bool IsChecked { get => _isChecked; set { Set(() => IsChecked, ref _isChecked, value); } }
        [JsonIgnore]
        public bool IsEditing { get => _isEditing; set { Set(() => IsEditing, ref _isEditing, value); } }
        public void Stop()
        {
            if (IsPlaying)
                IsPlaying = false;
        }
    }
}
