using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace adrilight
{
    public interface IGeneralSettings : INotifyPropertyChanged
    {


        bool Autostart { get; set; }
        bool NotificationEnabled { get; set; }
        int SelectedAudioDevice { get; set; }
        bool OpenRGBConfigRequested { get; set; }
        bool IsInBetaChanel { get; set; }


        bool IsOpenRGBEnabled { get; set; }
        bool IsProfileLoading { get; set; }
        bool StartMinimized { get; set; }
        bool HotkeyEnable { get; set; }
        bool DriverRequested { get; set; }
        int SystemRainbowSpeed { get; set; }
        int SystemRainbowMaxTick { get; set; }
        int ThemeIndex { get; set; }
        Color AccentColor { get; set; }
        int SystemMusicSpeed { get; set; }
        int SystemMusicMaxTick { get; set; }
        int BreathingSpeed { get; set; }


    }
}
