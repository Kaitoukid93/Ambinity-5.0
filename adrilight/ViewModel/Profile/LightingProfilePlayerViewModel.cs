using adrilight.Ticker;
using adrilight_shared.Models.Lighting;
using GalaSoft.MvvmLight;
using System;


namespace adrilight.ViewModel.Profile
{
    public class LightingProfilePlayerViewModel : ViewModelBase
    {
        #region Construct
        public LightingProfilePlayerViewModel(PlaylistDecoder decoder)
        {
            _decoder = decoder;
            _decoder.CurrentPlayingProfileChanged += _decoder_CurrentPlayingProfileChanged;
            _decoder.IsRunningPropertyChanged += _decoder_IsRunningPropertyChanged;
            _decoder.PlaylistChanged += _decoder_PlaylistChanged;
        }

        private void _decoder_PlaylistChanged(LightingProfilePlaylist playlist)
        {
            CurrentPlayingPlaylist = playlist;
        }

        private void _decoder_IsRunningPropertyChanged(bool isRunning)
        {
            IsPlaylistRunning = isRunning;
        }

        private void _decoder_CurrentPlayingProfileChanged(LightingProfile profile)
        {
            CurrentPlayingProfile = profile;
        }
        #endregion
        #region Properties
        private bool _isPlaylistRunning;
        private PlaylistDecoder _decoder;
        private LightingProfile _currentPlayingProfile;
        public LightingProfile CurrentPlayingProfile {
            get
            {
                return _currentPlayingProfile;
            }
            set
            {
                _currentPlayingProfile = value;
                RaisePropertyChanged();
            }
        }   
        private LightingProfilePlaylist _currentPlayingPlaylist;
        public LightingProfilePlaylist CurrentPlayingPlaylist {
            get
            {
                return _currentPlayingPlaylist;
            }
            set
            {
                _currentPlayingPlaylist = value;
                RaisePropertyChanged();
            }
        }
        public bool IsPlaylistRunning {
            get
            {
                return _isPlaylistRunning;
            }
            set
            {
                _isPlaylistRunning = value;
                RaisePropertyChanged();
            }
        }
        public void Dispose()
        {
            if (_decoder == null)
                return;
            _decoder.CurrentPlayingProfileChanged -= _decoder_CurrentPlayingProfileChanged;
            _decoder.IsRunningPropertyChanged -= _decoder_IsRunningPropertyChanged;
            _decoder.PlaylistChanged -= _decoder_PlaylistChanged;
            GC.SuppressFinalize(this);
        }
            #endregion
            #region Methods
            #endregion
            #region Command
            #endregion
        }
    }
