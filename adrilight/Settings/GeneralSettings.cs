using adrilight.Settings;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using Color = System.Windows.Media.Color;

namespace adrilight
{
    internal class GeneralSettings : ViewModelBase, IGeneralSettings
    {
        // private bool _autostart = true;
        private bool _isOpenRGBEnabled = false;
        private bool _isInBetaChanel = false;
        private bool _openRGBConfigRequested = true;
        private bool _autostart = true;
        private int _systemRainbowSpeed = 10;
        private int _systemFramePlaybackSpeed = 1;
        private int _systemRainbowMaxTick = 1023;
        private int _systemMusicMaxTick = 1023;
        private int _breathingSpeed = 20000;
        private bool _isProfileLoading = false;
        private bool _startMinimized = false;
        private bool _notificationEnabled = true;
        private int _selectedAudioDevice = 0;
        private Color _accentColor = Color.FromArgb(255, 185, 130, 251);
        private int _deviceDiscoveryMode = 0;
        private bool _openRGBAskAgain = true;
        private bool _hwMonitorAskAgain = true;
        private int _themeIndex = 0;
        private bool _hotkeyEnable = true;
        private bool _driverRequested = true;
        private int _systemMusicSpeed = 3;
        private bool _updaterAskAgain = true;
        private bool _audioDeviceAskAgain = true;
        private bool _isMultipleScreenEnable = true;
        private int _startupDelaySecond = 0;
        private AppUser _currentAppUser;
        public GeneralSettings()
        {

        }
        public int StartupDelaySecond { get => _startupDelaySecond; set { Set(() => StartupDelaySecond, ref _startupDelaySecond, value); } }
        public bool IsMultipleScreenEnable { get => _isMultipleScreenEnable; set { Set(() => IsMultipleScreenEnable, ref _isMultipleScreenEnable, value); } }
        public int SelectedAudioDevice { get => _selectedAudioDevice; set { Set(() => SelectedAudioDevice, ref _selectedAudioDevice, value); } }
        public int DeviceDiscoveryMode { get => _deviceDiscoveryMode; set { Set(() => DeviceDiscoveryMode, ref _deviceDiscoveryMode, value); } }
        public int ThemeIndex { get => _themeIndex; set { Set(() => ThemeIndex, ref _themeIndex, value); } }
        public int SystemRainbowMaxTick { get => _systemRainbowMaxTick; set { Set(() => SystemRainbowMaxTick, ref _systemRainbowMaxTick, value); } }
        public int BreathingSpeed { get => _breathingSpeed; set { Set(() => SystemPlaybackSpeed, ref _breathingSpeed, value); } }
        public int SystemPlaybackSpeed { get => _systemFramePlaybackSpeed; set { Set(() => BreathingSpeed, ref _systemFramePlaybackSpeed, value); } }
        public int SystemMusicMaxTick { get => _systemMusicMaxTick; set { Set(() => SystemMusicMaxTick, ref _systemMusicMaxTick, value); } }
        private int _limitFps = 100;
        public bool StartMinimized { get => _startMinimized; set { Set(() => StartMinimized, ref _startMinimized, value); } }
        public bool UpdaterAskAgain { get => _updaterAskAgain; set { Set(() => UpdaterAskAgain, ref _updaterAskAgain, value); } }
        public bool AudioDeviceAskAgain { get => _audioDeviceAskAgain; set { Set(() => AudioDeviceAskAgain, ref _audioDeviceAskAgain, value); } }
        public bool OpenRGBAskAgain { get => _openRGBAskAgain; set { Set(() => OpenRGBAskAgain, ref _openRGBAskAgain, value); } }
        public bool HWMonitorAskAgain { get => _hwMonitorAskAgain; set { Set(() => HWMonitorAskAgain, ref _hwMonitorAskAgain, value); } }
        public bool HotkeyEnable { get => _hotkeyEnable; set { Set(() => HotkeyEnable, ref _hotkeyEnable, value); } }
        public bool Autostart { get => _autostart; set { Set(() => Autostart, ref _autostart, value); } }
        public Color AccentColor { get => _accentColor; set { Set(() => AccentColor, ref _accentColor, value); } }
        [JsonIgnore]
        public bool IsOpenRGBEnabled { get => _isOpenRGBEnabled; set { Set(() => IsOpenRGBEnabled, ref _isOpenRGBEnabled, value); } }
        public bool DriverRequested { get => _driverRequested; set { Set(() => DriverRequested, ref _driverRequested, value); } }
        public bool OpenRGBConfigRequested { get => _openRGBConfigRequested; set { Set(() => OpenRGBConfigRequested, ref _openRGBConfigRequested, value); } }
        public bool NotificationEnabled { get => _notificationEnabled; set { Set(() => NotificationEnabled, ref _notificationEnabled, value); } }
        public int LimitFps { get => _limitFps; set { Set(() => LimitFps, ref _limitFps, value); } }
        public int SystemRainbowSpeed { get => _systemRainbowSpeed; set { Set(() => SystemRainbowSpeed, ref _systemRainbowSpeed, value); } }
        public int SystemMusicSpeed { get => _systemMusicSpeed; set { Set(() => SystemMusicSpeed, ref _systemMusicSpeed, value); } }
        public bool IsProfileLoading { get => _isProfileLoading; set { Set(() => IsProfileLoading, ref _isProfileLoading, value); } }
        public bool IsInBetaChanel { get => _isInBetaChanel; set { Set(() => IsInBetaChanel, ref _isInBetaChanel, value); } }
        public AppUser CurrentAppUser { get => _currentAppUser; set { Set(() => CurrentAppUser, ref _currentAppUser, value); } }
    }
}
