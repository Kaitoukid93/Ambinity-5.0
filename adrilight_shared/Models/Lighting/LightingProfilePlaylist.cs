using adrilight_shared.Models.DashboardItem;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Linq;
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
            Name = name;
            CommandSetup();
        }
        #endregion
        #region Commands
        [JsonIgnore]
        public ICommand PlaySelectedProfileFromListCommand { get; set; }
        [JsonIgnore]
        public ICommand PlayCommand { get; set; }
        #endregion
        #region Properties
        private bool _isPinned = false;
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
        private bool _repeat;
        [JsonIgnore]
        public bool IsPlaying { get => _isPlaying; set { Set(() => IsPlaying, ref _isPlaying, value); } }
        public bool Shuffle { get => _shuffle; set { Set(() => Shuffle, ref _shuffle, value); } }
        public bool Repeat { get => _repeat; set { Set(() => Repeat, ref _repeat, value); } }
        public int CurrentPlayingProfileIndex { get => _currentPlayingProfileIndex; set { Set(() => CurrentPlayingProfileIndex, ref _currentPlayingProfileIndex, value); } }
        public ObservableCollection<string> LightingProfilesUID { get; set; }
        [JsonIgnore]
        public DataCollection LightingProfiles { get; set; }
        [JsonIgnore]
        public bool IsSelected { get => _isSelected; set { Set(() => IsSelected, ref _isSelected, value); } }
        [JsonIgnore]
        public bool IsEditing { get => _isEditing; set { Set(() => IsEditing, ref _isEditing, value); } }
        [JsonIgnore]
        public bool IsDeleteable { get => _isDeleteable; set { Set(() => IsDeleteable, ref _isDeleteable, value); } }
        [JsonIgnore]
        public bool IsChecked { get => _isChecked; set { Set(() => IsChecked, ref _isChecked, value); } }
        [JsonIgnore]
        public LightingProfile CurrentPlayingLightingProfile => LightingProfiles.Items[CurrentPlayingProfileIndex] as LightingProfile;
        #endregion
        #region Methods
        private void CommandSetup()
        {
            PlaySelectedProfileFromListCommand = new Models.RelayCommand.RelayCommand<LightingProfile>((p) =>
            {
                return true;
            }, (p) =>
            {
                CurrentPlayingProfileIndex = LightingProfiles.Items.IndexOf(p);
                RaisePropertyChanged(nameof(IsPlaying));
            }

         );

        }
        public void LoadLightingProfiles(ObservableCollection<LightingProfile> availableProfiles)
        {
            LightingProfiles = new DataCollection();
            if (availableProfiles == null)
                return;
            foreach (var profileUID in LightingProfilesUID)
            {
                var match = availableProfiles.Where(p => p.ProfileUID == profileUID).FirstOrDefault();
                if (match != null)
                {
                    LightingProfiles.AddItems(match);
                }

            }
        }
        public void ResetProfilesPlayingState()
        {
            //IsPlaying = false;
            if (LightingProfiles == null)
                return;
            foreach (var item in LightingProfiles.Items)
            {
                (item as LightingProfile).IsPlaying = false;
            }
        }
        #endregion

    }
}
