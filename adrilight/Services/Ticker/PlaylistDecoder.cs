using adrilight.ViewModel;
using adrilight_shared.Models.Lighting;
using adrilight_shared.Settings;
using GalaSoft.MvvmLight;
using Serilog;
using System;
using System.ComponentModel;
using System.Threading;
using System.Timers;

namespace adrilight.Ticker
{
    internal class PlaylistDecoder : ViewModelBase
    {

        public PlaylistDecoder(IGeneralSettings generaSettings, MainViewViewModel mainViewViewModel)
        {
            GeneralSettings = generaSettings ?? throw new ArgumentNullException(nameof(generaSettings));

            MainViewModel = mainViewViewModel ?? throw new ArgumentNullException(nameof(mainViewViewModel));
            MainViewModel.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(MainViewModel.CurrentSelectedPlaylist):
                        //Init();
                        if (_selectedPlaylist != null)
                        {
                            _selectedPlaylist.PropertyChanged -= SelectedPlaylistChanged;
                            // _selectedPlaylist.ResetPlayingState();
                        }
                        _selectedPlaylist = MainViewModel.CurrentSelectedPlaylist;
                        _selectedPlaylist.PropertyChanged += SelectedPlaylistChanged;
                        break;
                }
            };
            //Init();
        }

        #region private field
        private LightingProfilePlaylist _selectedPlaylist;
        private CancellationTokenSource _cancellationTokenSource;
        #endregion

        #region public properties
        public bool IsRunning { get; private set; } = false;
        public bool NeededRefreshing { get; private set; } = false;
        public object Lock { get; } = new object();
        #endregion
        private void SelectedPlaylistChanged(object s, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(_selectedPlaylist.IsPlaying):
                    if (!_selectedPlaylist.IsPlaying)
                        Stop();
                    else
                        Play();
                    break;

            }
        }
        private void Play()
        {
            if (_selectedPlaylist == null)
                return;
            _selectedPlaylist.ResetProfilesPlayingState();
            if (_timer == null)
                SetTimer(_selectedPlaylist.CurrentPlayingLightingProfile.Duration);
            else
            {
                _timer.Interval = _selectedPlaylist.CurrentPlayingLightingProfile.Duration.TotalMilliseconds;
                StartTimer();
            }
            _currentTimeSpan = _selectedPlaylist.CurrentPlayingLightingProfile.Duration;
            _selectedPlaylist.CurrentPlayingLightingProfile.IsPlaying = true;
            Log.Information("Current Playing Profile :" +
                             _selectedPlaylist.CurrentPlayingLightingProfile.Name);
            MainViewModel.CurrentPlayingTimePercentage = 0;
            MainViewModel.ActivateCurrentLightingProfile(_selectedPlaylist.CurrentPlayingLightingProfile);

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
        private static MainViewViewModel MainViewModel { get; set; }
        private static System.Timers.Timer _timer;
        private static System.Timers.Timer _subTimer;
        private static TimeSpan _currentTimeSpan;
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
            MainViewModel.CurrentPlayingTimePercentage = (int)((MainViewModel.CurrentSelectedPlaylist.CurrentPlayingLightingProfile.Duration.TotalMilliseconds - _currentTimeSpan.TotalMilliseconds) * 100 / MainViewModel.CurrentSelectedPlaylist.CurrentPlayingLightingProfile.Duration.TotalMilliseconds);
        }
        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            if (_selectedPlaylist.CurrentPlayingProfileIndex < _selectedPlaylist.LightingProfiles.Items.Count - 1)
                _selectedPlaylist.CurrentPlayingProfileIndex++;
            else
            {
                if (_selectedPlaylist.Repeat)
                    _selectedPlaylist.CurrentPlayingProfileIndex = 0;
                else
                {
                    _selectedPlaylist.CurrentPlayingLightingProfile.IsPlaying = false;
                    _timer.Stop();
                    _subTimer.Stop();
                    return;
                }
            }
            Play();
        }
        public void Stop()
        {
            Log.Information("Current running playlist paused");
            _timer?.Stop();
            _subTimer?.Stop();
        }

    }
}