using adrilight.Spots;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using adrilight.Util;
using System.Windows.Input;
using adrilight.ViewModel;
using System.Windows;

namespace adrilight
{
    public interface IOutputSettings : INotifyPropertyChanged 
    {
        //bool Autostart { get; set; }

       
       
       
     
        string OutputName { get; set; }
        string OutputDescription { get; set; }
        //System.Drawing.Rectangle OutputRectangle { get; set; }
        //System.Drawing.Rectangle PreviewRectangle { get; set; }
        /// <summary>
        /// output capture position rev 2.0, using scale only
        /// </summary>
   
        bool IsVissible { get; set; }

      
        int OutputID { get; set; }
        int VUOrientation { get; set; }
        int VUMode { get; set; }
        string OutputType { get; set; }
        string TargetDevice { get; set; }
        bool OutputIsLoadingProfile { get; set; }
        bool OutputIsBuildingLEDSetup { get; set; }
        int OutputGifSpeed { get; set; }
        IGifCard OutputSelectedGif { get; set; }
        string OutputUniqueID { get; set; }
        string OutputRGBLEDOrder { get; set; }
        bool OutputIsVisible { get; set; }
        int OutputBrightness { get; set; }
        int OutputPowerVoltage { get; set; }
        int OutputPowerMiliamps { get; set; }
        byte OutputSaturationThreshold { get; set; }
        int OutputSmoothness { get; set; }
        int OutputScreenCapturePositionIndex { get; set; }
        bool OutputUseLinearLighting { get; set; }
        bool OutputIsSystemSync { get; set; }
        bool OutputIsPreviewRunning { get; set; }
        bool OutputIsEnabled { get; set; }
        bool IsBrightnessPopupOpen { get; set; }
        int LEDPerSpot { get; set; }
        int LEDPerLED { get; set; }
        Color OutputAtmosphereStartColor { get; set; }
        Color OutputAtmosphereStopColor { get; set; }
        string OutputAtmosphereMode { get; set; }
        //string SelectedEffect { get; set; }
        int OutputSelectedMusicMode { get; set; }
        int OutputMusicSensitivity { get; set; }
        int OutputSelectedMusicPalette { get; set; }
        Color[] OutputSentryModeColorSource { get; set; }
        int OutputSelectedAudioDevice { get; set; }
        int OutputColorPaletteMode { get; set; }
        int OutputSelectedDisplay { get; set; }
        int OutputSelectedMode { get; set; }
        bool IsInSpotEditWizard { get; set; }
        string Geometry { get; set; }
        int OutputMusicVisualizerFreq { get; set; }
     
     






        //rainbow settings//
        int OutputSelectedChasingPalette { get; set; }
        int OutputSelectedGifIndex { get; set; }
        int OutputPaletteSpeed { get; set; }
        int OutputPaletteChasingPosition { get; set; }
        int OutputPaletteBlendStep { get; set; } // auto adjust step based on numLED
        //rainbow settings//

        //static color settings//
        Color OutputStaticColor { get; set; }
        int OutputStaticColorMode { get; set; }
        int OutputStaticColorGradientMode { get; set; }
        int OutputScreenCapturePosition { get; set; }
        int OutputScreenCaptureWB { get; set; }
        int OutputScreenCaptureWBRed { get; set; }
        int OutputScreenCaptureWBGreen { get; set; }
        int OutputScreenCaptureWBBlue { get; set; }
        int OutputMusicDancingMode { get; set; }
        int OutputBreathingSpeed { get; set; }
        int SleepMode { get; set; }
        bool OutputIsSelected { get; set; }
      
        //static color settings//
        IGradientColorCard OutputSelectedGradient { get; set; }
         bool OutputParrentIsEnable { get; set; }

        IColorPalette OutputCurrentActivePalette { get; set; }
        ILEDSetup OutputLEDSetup { get; set; }
        //void SetRectangle(System.Drawing.Rectangle rectangle);
        //void SetPreviewRectangle(System.Drawing.Rectangle rectangle);

        
        


    }
}
