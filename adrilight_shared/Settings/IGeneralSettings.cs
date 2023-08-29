using adrilight_shared.Models.AppUser;
using System.ComponentModel;
using Color = System.Windows.Media.Color;

namespace adrilight_shared.Settings
{
    public interface IGeneralSettings : INotifyPropertyChanged
    {


        bool Autostart { get; set; }
        bool ShowOpenRGB { get; set; }
        bool IsBlackBarDetectionEnabled { get; set; }
        bool IsPowerLimitEnabled { get; set; }
        int ScreenCapturingMethod { get; set; }
        bool NotificationEnabled { get; set; }
        int SelectedAudioDevice { get; set; }
        bool OpenRGBConfigRequested { get; set; }
        bool IsInBetaChanel { get; set; }
        bool IsMultipleScreenEnable { get; set; }
        int DeviceDiscoveryMode { get; set; }
        bool OpenRGBAskAgain { get; set; }
        bool HWMonitorAskAgain { get; set; }
        bool UpdaterAskAgain { get; set; }
        bool AudioDeviceAskAgain { get; set; }
        int StartupDelaySecond { get; set; }
        bool IsOpenRGBEnabled { get; set; }
        bool IsProfileLoading { get; set; }
        bool StartMinimized { get; set; }
        bool HotkeyEnable { get; set; }
        bool DriverRequested { get; set; }
        int SystemRainbowSpeed { get; set; }
        int SystemPlaybackSpeed { get; set; }
        int SystemRainbowMaxTick { get; set; }
        int ThemeIndex { get; set; }
        Color AccentColor { get; set; }
        int SystemMusicSpeed { get; set; }
        int SystemMusicMaxTick { get; set; }
        int BreathingSpeed { get; set; }
        AppUser CurrentAppUser { get; set; }

    }
}
