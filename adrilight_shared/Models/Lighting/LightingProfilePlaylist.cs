using adrilight_shared.Models.DashboardItem;
using adrilight_shared.Models.ItemsCollection;
using adrilight_shared.Services;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Input;

namespace adrilight_shared.Models.Lighting
{
    public class LightingProfilePlaylist : ViewModelBase, IDashboardItem, IGenericCollectionItem
    {
        #region Construct
        public LightingProfilePlaylist()
        {

        }
        public LightingProfilePlaylist(string name)
        {

            _dialogService = new DialogService();
            Name = name;
            LightingProfilesUID = new ObservableCollection<string>();
            LightingProfiles = new ObservableCollection<LightingProfile>();

        }
        #endregion
        #region Commands
        [JsonIgnore]
        public ICommand SetProfileDurationCommand { get; set; }
        #endregion
        #region Properties
        private bool _isPinned = false;
        private DialogService _dialogService;
        public bool IsPinned { get => _isPinned; set { Set(() => IsPinned, ref _isPinned, value); } }
        private string _name;
        public string Name { get => _name; set { Set(() => Name, ref _name, value); } }
        private bool _isChecked = false;
        private bool _isDeleteable = true;
        private bool _isSelected;
        private bool _isEditing;
        private bool _isPlaying;
        private int _eachProfileDuration;
        private int _currentPlayingProfileIndex;
        private bool _shuffle;
        private bool _repeat = true;
        [JsonIgnore]
        public string LocalPath { get; set; }
        public bool IsPlaying { get => _isPlaying; set { Set(() => IsPlaying, ref _isPlaying, value); } }
        public bool Shuffle { get => _shuffle; set { Set(() => Shuffle, ref _shuffle, value); } }
        public bool Repeat { get => _repeat; set { Set(() => Repeat, ref _repeat, value); } }
        public int CurrentPlayingProfileIndex { get => _currentPlayingProfileIndex; set { Set(() => CurrentPlayingProfileIndex, ref _currentPlayingProfileIndex, value); } }
        public ObservableCollection<string> LightingProfilesUID { get; set; }
        [JsonIgnore]
        public ObservableCollection<string> CollectionName
        {
            get
            {
                return GetCollectionName();
            }
        }

        [JsonIgnore]
        public ObservableCollection<LightingProfile> LightingProfiles { get; set; }
        [JsonIgnore]
        public bool IsSelected { get => _isSelected; set { Set(() => IsSelected, ref _isSelected, value); } }
        [JsonIgnore]
        public bool IsEditing { get => _isEditing; set { Set(() => IsEditing, ref _isEditing, value); } }
        public bool IsDeleteable { get => _isDeleteable; set { Set(() => IsDeleteable, ref _isDeleteable, value); } }
        [JsonIgnore]
        public bool IsChecked { get => _isChecked; set { Set(() => IsChecked, ref _isChecked, value); } }
        [JsonIgnore]
        public LightingProfile CurrentPlayingLightingProfile => LightingProfiles[CurrentPlayingProfileIndex > LightingProfiles.Count ? 0 : CurrentPlayingProfileIndex] as LightingProfile;
        [JsonIgnore]
        public string InfoPath { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        #endregion
        #region Methods
        public void LoadLightingProfiles(List<LightingProfile> availableProfiles)
        {
            LightingProfiles = new ObservableCollection<LightingProfile>();
            if (availableProfiles == null)
                return;
            if (LightingProfilesUID == null)
                return;
            foreach (var profileUID in LightingProfilesUID)
            {
                var match = availableProfiles.Where(p => (p as LightingProfile).ProfileUID == profileUID).FirstOrDefault();
                if (match != null)
                {
                    LightingProfiles.Add(match);
                }

            }
        }
        public void GetProfilesUID()
        {
            if (LightingProfiles == null)
                return;
            LightingProfilesUID = new ObservableCollection<string>();
            foreach (LightingProfile item in LightingProfiles)
            {
                LightingProfilesUID.Add(item.ProfileUID);
            }
        }
        private ObservableCollection<string> GetCollectionName()
        {
            var names = new ObservableCollection<string>();
            if (LightingProfiles == null || LightingProfiles == null)
                return names;
            if (LightingProfiles.Count >= 4)
            {
                foreach (var item in LightingProfiles.Take(4))
                {
                    names.Add(item.Name);
                }
            }
            else
            {
                foreach (var item in LightingProfiles)
                {
                    names.Add(item.Name);
                }
            }
            return names;
        }
        public void SetProfileDuration(int value)
        {
            if (LightingProfiles == null || LightingProfiles == null)
                return;
            foreach (var profile in LightingProfiles)
                (profile as LightingProfile).Duration = System.TimeSpan.FromSeconds(value);
        }

        public void ResetProfilesPlayingState()
        {
            //IsPlaying = false;
            if (LightingProfiles == null)
                return;
            foreach (var item in LightingProfiles)
            {
                (item as LightingProfile).IsPlaying = false;
            }
        }
        public void StopPlaylist()
        {
            IsPlaying = false;
        }
        #endregion
    }
}
