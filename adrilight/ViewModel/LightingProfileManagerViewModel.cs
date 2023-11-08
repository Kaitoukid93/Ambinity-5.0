using adrilight_shared.Models;
using adrilight_shared.Models.Lighting;
using GalaSoft.MvvmLight;

namespace adrilight.ViewModel
{
    public class LightingProfileManagerViewModel : ViewModelBase
    {
        public LightingProfileManagerViewModel(DataCollection profiles, DataCollection playlists)
        {
            AvailableLightingProfiles = profiles;
            AvailableLightingProfilePlaylists = playlists;

        }
        public DataCollection AvailableLightingProfiles { get; set; }
        public DataCollection AvailableLightingProfilePlaylists { get; set; }
        private LightingProfile _currentSelectedLightingProfile;
        public LightingProfile CurrentSelectedLightingProfile {
            get
            {
                return _currentSelectedLightingProfile;
            }
            set
            {
                _currentSelectedLightingProfile = value;
                RaisePropertyChanged();
            }
        }
        private int _currentProfileTime;
        public int CurrentProfileTime {
            get
            {
                return _currentProfileTime;
            }
            set
            {
                _currentProfileTime = value;
                RaisePropertyChanged();
            }
        }
    }
}
