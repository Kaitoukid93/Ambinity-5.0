using adrilight.Manager;
using adrilight.ViewModel;
using adrilight_shared.Helpers;
using adrilight_shared.Models.ControlMode.Mode;
using adrilight_shared.Models.Device;
using adrilight_shared.Models.Lighting;
using adrilight_shared.Settings;
using GalaSoft.MvvmLight;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace adrilight.Ticker
{
    public class PlaylistDecoder
    {
        public event Action<LightingProfile> CurrentPlayingProfileChanged;
        public event Action<bool> IsRunningPropertyChanged;
        public event Action<LightingProfilePlaylist> PlaylistChanged;
        public PlaylistDecoder(IGeneralSettings generaSettings,DeviceManager deviceManager)
        {
            GeneralSettings = generaSettings ?? throw new ArgumentNullException(nameof(generaSettings));
            _deviceManager = deviceManager;
        }

        #region private field
        private LightingProfilePlaylist _selectedPlaylist;
        private DeviceManager _deviceManager;
        private static bool _isWindowOpen;
        #endregion

        #region public properties
        private bool _isRunning;
        public bool IsRunning {
            get
            {
                return _isRunning;
            }
            set
            {
                _isRunning = value;
                IsRunningPropertyChanged?.Invoke(_isRunning);
            }
        }
        public bool NeededRefreshing { get; private set; } = false;
        public object Lock { get; } = new object();
        #endregion

        public void Init()
        {

        }
        public async Task Play(LightingProfilePlaylist playlist)
        {
            var valid = playlist != null && playlist.LightingProfiles != null && playlist.LightingProfiles != null && playlist.LightingProfiles.Count > 0;
            if (!valid)
                return;
            _selectedPlaylist?.StopPlaylist();
            _selectedPlaylist = playlist;
            PlaylistChanged?.Invoke(playlist);
            _selectedPlaylist.IsPlaying = true;
            _selectedPlaylist.ResetProfilesPlayingState();
            _selectedPlaylist.CurrentPlayingProfileIndex = 0;
            Log.Information("Current Playing Profile :" +
                             _selectedPlaylist.CurrentPlayingLightingProfile.Name);
            //ViewModel.CurrentProfileTime = 0;
            //MainViewModel.ActivateCurrentLightingProfile(_selectedPlaylist.CurrentPlayingLightingProfile);
            _currentPlayingProfile?.Stop();
            await ActivateCurrentLightingProfile(_selectedPlaylist.CurrentPlayingLightingProfile, true);
            IsRunning = true;

        }
        public async Task Play(LightingProfile profile)
        {
            _currentPlayingProfile?.Stop();
            _selectedPlaylist?.StopPlaylist();
            await ActivateCurrentLightingProfile(profile, false);
            IsRunning = false;
        }
        public void Play(LightingProfile profile, IDeviceSettings device)
        {
            _currentPlayingProfile?.Stop();
            _selectedPlaylist?.StopPlaylist();
            IsRunning = false;
            ActivateCurrentLightingProfileForSpecificDevice(profile, device);
        }
        private void StopTimer()
        {
            _timer?.Stop();
            _subTimer?.Stop();
        }
        private void StartTimer()
        {
            _timer?.Start();
            _subTimer?.Start();
        }
        private IGeneralSettings GeneralSettings { get; set; }
        private static System.Timers.Timer _timer;
        private static System.Timers.Timer _subTimer;
        private static TimeSpan _currentTimeSpan;
        private static LightingProfile _currentPlayingProfile;
        private void SetTimer(TimeSpan profileDuration)
        {
            _timer?.Stop();
            _subTimer?.Stop();
            _timer?.Dispose();
            _subTimer?.Dispose();
            // Create a timer with a two second interval.
            _timer = new System.Timers.Timer(profileDuration.TotalMilliseconds);
            // Hook up the Elapsed event for the timer. 
            _timer.Elapsed += OnTimedEvent;
            _timer.AutoReset = true;
            _timer.Enabled = true;
            SetSubTimer();
        }
        private static void SetSubTimer()
        {
            // Create a timer with a two second interval.
            if (_subTimer != null)
            {
                _subTimer.Stop();
                _subTimer.Dispose();
            }

            _subTimer = new System.Timers.Timer(1000);
            // Hook up the Elapsed event for the timer. 
            _subTimer.Elapsed += SubTimerElapsed;
            _subTimer.AutoReset = true;
            _subTimer.Enabled = true;
        }
        private static void SubTimerElapsed(Object source, ElapsedEventArgs e)
        {
            _currentTimeSpan = _currentTimeSpan.Subtract(TimeSpan.FromSeconds(1));
            if (_isWindowOpen)
                _currentPlayingProfile.CurrentPlayingProgress = (int)((_currentPlayingProfile.Duration.TotalMilliseconds - _currentTimeSpan.TotalMilliseconds) * 100 / _currentPlayingProfile.Duration.TotalMilliseconds);
        }
        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            //_selectedPlaylist.CurrentPlayingLightingProfile.IsPlaying = false;
            if (_selectedPlaylist.Shuffle)
            {
                Random r = new Random();
                _selectedPlaylist.CurrentPlayingProfileIndex = r.Next(0, _selectedPlaylist.LightingProfiles.Count - 1);
            }
            else
            {

                if (_selectedPlaylist.CurrentPlayingProfileIndex < _selectedPlaylist.LightingProfiles.Count - 1)
                    _selectedPlaylist.CurrentPlayingProfileIndex++;
                else
                {
                    if (_selectedPlaylist.Repeat)
                        _selectedPlaylist.CurrentPlayingProfileIndex = 0;
                    else
                    {
                        Stop();
                        return;
                    }
                }
            }


            _currentPlayingProfile?.Stop();
            ActivateCurrentLightingProfile(_selectedPlaylist.CurrentPlayingLightingProfile, true);
        }
        public void Stop()
        {
            _selectedPlaylist?.StopPlaylist();
            _selectedPlaylist.CurrentPlayingLightingProfile.IsPlaying = false;
            _timer.Stop();
            _subTimer.Stop();
            IsRunning = false;
        }
        private void ActivateCurrentLightingProfileForSpecificDevice(LightingProfile profile, IDeviceSettings device)
        {
            if (!device.IsEnabled)
                return;
            StopTimer();
            var lightingMode = ObjectHelpers.Clone<LightingMode>(profile.ControlMode as LightingMode);
            Task.Run(() => device.ActivateControlMode(lightingMode));
            Log.Information("Lighting Profile Activated: " + profile.Name + " for " + device.DeviceName);
        }
        private async Task ActivateCurrentLightingProfile(LightingProfile profile, bool isQeued)
        {
            if (profile == null)
                return;
            profile.IsPlaying = true;
            _currentPlayingProfile = profile;
            CurrentPlayingProfileChanged?.Invoke(_currentPlayingProfile);
            if (isQeued)
            {
                //use timer
                if (_timer == null)
                    SetTimer(profile.Duration);
                else
                {
                    StopTimer();
                    _timer.Interval = profile.Duration.TotalMilliseconds;
                    StartTimer();
                }
                _currentTimeSpan = profile.Duration;
            }
            else
            {
                StopTimer();
            }
            foreach (var device in _deviceManager.AvailableDevices)
            {
                if (!device.IsEnabled)
                    continue;
                device.TurnOnLED();
                var lightingMode = ObjectHelpers.Clone<LightingMode>(profile.ControlMode as LightingMode);
                await device.ActivateControlMode(lightingMode);
                Log.Information("Lighting Profile Activated: " + profile.Name + " for " + device.DeviceName);
            }
        }
        public void WindowsStatusChanged(bool status)
        {
            if (status)
                _isWindowOpen = true;
            else
                _isWindowOpen = false;
        }
    }
}