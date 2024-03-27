using adrilight_shared.Models.AppUser;
using adrilight_shared.Models.Language;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System.Globalization;
using Color = System.Windows.Media.Color;

namespace adrilight_shared.Settings
{
    public class GeneralSettings : ViewModelBase, IGeneralSettings
    {
        // private bool _autostart = true;
        private bool _isOpenRGBEnabled = false;
        private bool _isInBetaChanel = false;
        private bool _openRGBConfigRequested = true;
        private bool _isHardwareMonitorEnabled = true;
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
        private int _blackBarDetectionDelayTime = 3;
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
        private int _screenCapturingMethod = 0;
        private bool _isBlackBarDetectionEnabled = true;
        private bool _isPowerLimitEnabled = true;
        private bool _showOpenRGB = true;
        private bool _screenCapturingEnabled = true;
        private bool _audioCapturingEnabled = true;
        private string _openRGBVersion = "0.90";
        private bool _usingOpenRGB = true;
        private int _appLanguageIndex = 0;
        private LangModel _appCulture = new LangModel(new CultureInfo("en-US", false), "English", "");
        public GeneralSettings()
        {

        }
        public LangModel AppCulture { get => _appCulture; set { Set(() => AppCulture, ref _appCulture, value); } }
        public int AppLanguageIndex { get => _appLanguageIndex; set { Set(() => AppLanguageIndex, ref _appLanguageIndex, value); } }
        public bool IsHWMonitorEnabled { get => _isHardwareMonitorEnabled; set { Set(() => IsHWMonitorEnabled, ref _isHardwareMonitorEnabled, value); } }
        public bool ShowOpenRGB { get => _showOpenRGB; set { Set(() => ShowOpenRGB, ref _showOpenRGB, value); } }
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
        public bool IsBlackBarDetectionEnabled { get => _isBlackBarDetectionEnabled; set { Set(() => IsBlackBarDetectionEnabled, ref _isBlackBarDetectionEnabled, value); } }
        public bool IsPowerLimitEnabled { get => _isPowerLimitEnabled; set { Set(() => IsPowerLimitEnabled, ref _isPowerLimitEnabled, value); } }
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
        public int ScreenCapturingMethod { get => _screenCapturingMethod; set { Set(() => ScreenCapturingMethod, ref _screenCapturingMethod, value); } }
        public bool ScreenCapturingEnabled { get => _screenCapturingEnabled; set { Set(() => ScreenCapturingEnabled, ref _screenCapturingEnabled, value); } }
        public bool AudioCapturingEnabled { get => _audioCapturingEnabled; set { Set(() => AudioCapturingEnabled, ref _audioCapturingEnabled, value); } }
        public bool UsingOpenRGB { get => _usingOpenRGB; set { Set(() => UsingOpenRGB, ref _usingOpenRGB, value); } }
        public string OpenRGBVersion { get => _openRGBVersion; set { Set(() => OpenRGBVersion, ref _openRGBVersion, value); } }

        public int BlackBarDetectionDelayTime { get => _blackBarDetectionDelayTime; set { Set(() => BlackBarDetectionDelayTime, ref _blackBarDetectionDelayTime, value); } }
    }
}
