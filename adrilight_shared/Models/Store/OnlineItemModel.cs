using adrilight_shared.Enums;
using adrilight_shared.Models.Device;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace adrilight_shared.Models.Store
{
    public class OnlineItemModel : ViewModelBase // this for displaying on the store
    {

        public OnlineItemModel()
        {

        }
        private bool _isDownloading = false;
        private bool _isLocalExisted = false;
        private bool _isUpgradeAvailable = false;
        private BitmapImage _thumb;
        private string _gravatarID;
        private string _name;
        private OnlineItemAvatarTypeEnum _avatarType = OnlineItemAvatarTypeEnum.Image;
        public string Name { get => _name; set { Set(() => Name, ref _name, value); } }
        public string Owner { get; set; } // the name of creator
        public string Type { get; set; } // ledsetup or color palette
        public string Description { get; set; }
        public string Path { get; set; }
        public BitmapImage Thumb { get => _thumb; set { Set(() => Thumb, ref _thumb, value); } }
        public string GravatarID { get => _gravatarID; set { Set(() => GravatarID, ref _gravatarID, value); } }
        public OnlineItemAvatarTypeEnum AvatarType { get => _avatarType; set { Set(() => AvatarType, ref _avatarType, value); } }
        public List<BitmapImage> Screenshots { get; set; }
        public string MarkDownDescription { get; set; }
        public List<DeviceType> TargetDevices { get; set; }
        [JsonIgnore]
        public bool IsLocalExisted { get => _isLocalExisted; set { Set(() => IsLocalExisted, ref _isLocalExisted, value); } }
        [JsonIgnore]
        public bool IsUpgradeAvailable { get => _isUpgradeAvailable; set { Set(() => IsUpgradeAvailable, ref _isUpgradeAvailable, value); } }
        public string Version { get; set; }
        [JsonIgnore]
        public bool IsDownloading { get => _isDownloading; set { Set(() => IsDownloading, ref _isDownloading, value); } }
    }
}
