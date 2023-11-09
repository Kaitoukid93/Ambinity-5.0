﻿using adrilight.ViewModel;
using adrilight_shared.Helpers;
using adrilight_shared.Models.ControlMode.Mode;
using adrilight_shared.Models.Device;
using adrilight_shared.Models.Lighting;
using adrilight_shared.Settings;
using GalaSoft.MvvmLight;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Timers;

namespace adrilight.Ticker
{
    public class PlaylistDecoder : ViewModelBase
    {

        public PlaylistDecoder(IGeneralSettings generaSettings, IDeviceSettings[] devices)
        {
            GeneralSettings = generaSettings ?? throw new ArgumentNullException(nameof(generaSettings));
            AvailableDevices = new ObservableCollection<IDeviceSettings>();
            foreach (var device in devices)
            {
                AvailableDevices.Add(device);
            }
        }

        #region private field
        private LightingProfilePlaylist _selectedPlaylist;
        private CancellationTokenSource _cancellationTokenSource;
        public ObservableCollection<IDeviceSettings> AvailableDevices { get; }
        #endregion

        #region public properties
        public bool IsRunning { get; private set; } = false;
        public bool NeededRefreshing { get; private set; } = false;
        public object Lock { get; } = new object();
        #endregion
        public void Play(LightingProfilePlaylist playlist)
        {
            if (playlist == null)
                return;
            _selectedPlaylist = playlist;
            _selectedPlaylist.IsPlaying = true;
            _selectedPlaylist.ResetProfilesPlayingState();
            _selectedPlaylist.CurrentPlayingProfileIndex = 0;
            Log.Information("Current Playing Profile :" +
                             _selectedPlaylist.CurrentPlayingLightingProfile.Name);
            //ViewModel.CurrentProfileTime = 0;
            //MainViewModel.ActivateCurrentLightingProfile(_selectedPlaylist.CurrentPlayingLightingProfile);
            _currentPlayingProfile?.Stop();
            ActivateCurrentLightingProfile(_selectedPlaylist.CurrentPlayingLightingProfile, true);

        }
        public void Play(LightingProfile profile)
        {
            _currentPlayingProfile?.Stop();
            ActivateCurrentLightingProfile(profile, false);
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
        private static LightingProfileManagerViewModel ViewModel { get; set; }
        private static System.Timers.Timer _timer;
        private static System.Timers.Timer _subTimer;
        private static TimeSpan _currentTimeSpan;
        private LightingProfile _currentPlayingProfile;
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
            Log.Information("Current Profile time left: " + _currentTimeSpan.Subtract(TimeSpan.FromSeconds(1)));
            _currentTimeSpan = _currentTimeSpan.Subtract(TimeSpan.FromSeconds(1));
            // ViewModel.CurrentProfileTime = (int)((MainViewModel.CurrentSelectedPlaylist.CurrentPlayingLightingProfile.Duration.TotalMilliseconds - _currentTimeSpan.TotalMilliseconds) * 100 / MainViewModel.CurrentSelectedPlaylist.CurrentPlayingLightingProfile.Duration.TotalMilliseconds);
        }
        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            //_selectedPlaylist.CurrentPlayingLightingProfile.IsPlaying = false;
            if (_selectedPlaylist.Shuffle)
            {
                Random r = new Random();
                _selectedPlaylist.CurrentPlayingProfileIndex = r.Next(0, _selectedPlaylist.LightingProfiles.Items.Count - 1);
            }
            else
            {

                if (_selectedPlaylist.CurrentPlayingProfileIndex < _selectedPlaylist.LightingProfiles.Items.Count - 1)
                    _selectedPlaylist.CurrentPlayingProfileIndex++;
                else
                {
                    if (_selectedPlaylist.Repeat)
                        _selectedPlaylist.CurrentPlayingProfileIndex = 0;
                    else
                    {
                        _selectedPlaylist.IsPlaying = false;
                        _selectedPlaylist.CurrentPlayingLightingProfile.IsPlaying = false;
                        _timer.Stop();
                        _subTimer.Stop();
                        return;
                    }
                }
            }


            _currentPlayingProfile?.Stop();
            ActivateCurrentLightingProfile(_selectedPlaylist.CurrentPlayingLightingProfile, true);
        }
        public void Stop()
        {
            Log.Information("Current running playlist paused");
            _timer?.Stop();
            _subTimer?.Stop();
        }
        private void ActivateCurrentLightingProfile(LightingProfile profile, bool isQeued)
        {
            if (profile == null)
                return;
            profile.IsPlaying = true;
            _currentPlayingProfile = profile;
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
            foreach (var device in AvailableDevices)
            {
                var lightingMode = ObjectHelpers.Clone<LightingMode>(profile.ControlMode as LightingMode);
                device.ActivateControlMode(lightingMode);
                Log.Information("Lighting Profile Activated: " + profile.Name + " for " + device.DeviceName);
            }
        }
    }
}