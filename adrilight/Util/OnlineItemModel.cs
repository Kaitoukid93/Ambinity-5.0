﻿using adrilight.Settings;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace adrilight.Util
{
    internal class OnlineItemModel : ViewModelBase, IOnlineItemModel // this for displaying on the store
    {

        public OnlineItemModel()
        {

        }
        private bool _isDownloading = false;
        public string Name { get; set; }
        public string Owner { get; set; } // the name of creator
        public string Type { get; set; } // ledsetup or color palette
        public string Description { get; set; }
        public string Path { get; set; }
        public BitmapImage Thumb { get; set; }
        public List<BitmapImage> Screenshots { get; set; }
        public string MarkDownDescription { get; set; }
        public List<DeviceType> TargetDevices { get; set; }
        public bool IsLocalExisted { get; set; }
        public string Version { get; set; }
        [JsonIgnore]
        public bool IsDownloading { get => _isDownloading; set { Set(() => IsDownloading, ref _isDownloading, value); } }
    }
}
