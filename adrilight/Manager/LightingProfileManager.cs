using adrilight.Ticker;
using adrilight_shared.Models.Device;
using adrilight_shared.Models.Lighting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.Manager
{
    public class LightingProfileManager
    {
        #region Construct
        public LightingProfileManager(PlaylistDecoder decoder,LightingProflileDBManager dbManager)
        {
            _decoder = decoder;
            _dbManager = dbManager;
            LoadData();
        }
        #endregion
        #region Events
        #endregion
        #region Properties
        private PlaylistDecoder _decoder;
        private List<LightingProfile> _availableProfiles;
        private LightingProflileDBManager _dbManager;
        private List<LightingProfilePlaylist> _availablePlaylists;
        public List<LightingProfilePlaylist> AvailablePlaylists {
            get
            {
                return _availablePlaylists;
            }
        }
        public List<LightingProfile> AvailableProfiles {
            get
            {
                return _availableProfiles;
            }
        }
        #endregion

        #region Methods
        private void LoadData()
        {
            _dbManager.CreateDefault();
            _availableProfiles = _dbManager.LoadLightingProfileIfExist();
            _availablePlaylists = _dbManager.LoadLightingProfilePlaylistIfExist();
        }
        public void ActivateProfile(string profileUID, IDeviceSettings targetDevice)
        {
            var profile = _availableProfiles.Where(p => (p as LightingProfile).ProfileUID == profileUID).FirstOrDefault() as LightingProfile;
            if (profile != null)
            {
                _decoder.Play(profile, targetDevice);
            }
        }
        //play profile for all device
        public void ActivateProfile(LightingProfile profile)
        {
            _decoder.Play(profile);
        }
        public void ActivateProfile(LightingProfile profile, IDeviceSettings targetDevice)
        {

        }
        public void ActivatePlaylist(LightingProfilePlaylist playlist)
        {
            _decoder.Play(playlist);
        }
        public void DeActivatePlaylist(LightingProfilePlaylist playlist)
        {
            _decoder.Stop();
        }
        public void WindowsStatusChanged(bool status)
        {
            _decoder.WindowsStatusChanged(status);
        }
        #endregion

    }
}
