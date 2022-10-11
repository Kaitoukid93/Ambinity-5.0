using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace adrilight
{
    internal class GeneralSettings : ViewModelBase, IGeneralSettings
    {
        // private bool _autostart = true;
        private bool _isOpenRGBEnabled = false;
        private bool _autostart = true;
        private int _systemRainbowSpeed = 10;
        private int _systemRainbowMaxTick = 1023;
        private int _systemMusicMaxTick = 1023;
        private int _breathingSpeed = 20000;
        private bool _isProfileLoading = false;
        private bool _startMinimized = false;
        private bool _notificationEnabled = true;
        private int _selectedAudioDevice = 0;
        private Brush _accentColor = Brushes.BlueViolet;

        private int _themeIndex = 0;
        private bool _hotkeyEnable = true;
        private bool _driverRequested = true;
        private int _systemMusicSpeed = 3;

        public int SelectedAudioDevice { get => _selectedAudioDevice; set { Set(() => SelectedAudioDevice, ref _selectedAudioDevice, value); } }
        public int ThemeIndex { get => _themeIndex; set { Set(() => ThemeIndex, ref _themeIndex, value); } }
        public int SystemRainbowMaxTick { get => _systemRainbowMaxTick; set { Set(() => SystemRainbowMaxTick, ref _systemRainbowMaxTick, value); } }
        public int BreathingSpeed { get => _breathingSpeed; set { Set(() => BreathingSpeed, ref _breathingSpeed, value); } }
        public int SystemMusicMaxTick { get => _systemMusicMaxTick; set { Set(() => SystemMusicMaxTick, ref _systemMusicMaxTick, value); } }
        private int _limitFps = 100;
        public bool StartMinimized { get => _startMinimized; set { Set(() => StartMinimized, ref _startMinimized, value); } }
        public bool HotkeyEnable { get => _hotkeyEnable; set { Set(() => HotkeyEnable, ref _hotkeyEnable, value); } }
        public bool Autostart { get => _autostart; set { Set(() => Autostart, ref _autostart, value); } }
        public Brush AccentColor { get => _accentColor; set { Set(() => AccentColor, ref _accentColor, value); } }
        public bool IsOpenRGBEnabled { get => _isOpenRGBEnabled; set { Set(() => IsOpenRGBEnabled, ref _isOpenRGBEnabled, value); } }
        public bool DriverRequested { get => _driverRequested; set { Set(() => DriverRequested, ref _driverRequested, value); } }
        public bool NotificationEnabled { get => _notificationEnabled; set { Set(() => NotificationEnabled, ref _notificationEnabled, value); } }
        public int LimitFps { get => _limitFps; set { Set(() => LimitFps, ref _limitFps, value); }  }
        public int SystemRainbowSpeed { get => _systemRainbowSpeed; set { Set(() => SystemRainbowSpeed, ref _systemRainbowSpeed, value); } }
        public int SystemMusicSpeed { get => _systemMusicSpeed; set { Set(() => SystemMusicSpeed, ref _systemMusicSpeed, value); } }
        public bool IsProfileLoading { get => _isProfileLoading; set { Set(() => IsProfileLoading, ref _isProfileLoading, value); } }
    }
}
