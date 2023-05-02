﻿using adrilight.Settings;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Media;
using Color = System.Windows.Media.Color;

namespace adrilight
{
    public interface IGeneralSettings : INotifyPropertyChanged
    {


        bool Autostart { get; set; }
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
        bool FrimwareUpgradeIsInProgress { get; set; }
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
        List<DesktopScreen> Screens { get; set; }
        int BreathingSpeed { get; set; }


    }
}
