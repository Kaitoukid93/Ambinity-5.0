using adrilight.Settings;
using adrilight.Spots;
using adrilight.Util;
using adrilight.ViewModel;
using adrilight_effect_analyzer.Model;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Color = System.Windows.Media.Color;

namespace adrilight
{
    internal class OutputSettings : ViewModelBase, IOutputSettings, IDrawable
    {

        private string _outputName;
        private OutputTypeEnum _outputType;
        private int _outputID;
        private bool _isVissible = true;
        private string _outputDescription;
        private string _targetDevice;


        private string _outputUniqueID;
        private string _outputRGBLEDOrder;
        private bool _outputIsVisible;
        //private int _outputBrightness;
        private int _outputPowerVoltage;
        private int _outputPowerMiliamps;
        //private byte _outputSaturationThreshold;
        //private bool _outputUseLinearLighting = true;
        private bool _outputIsSelected = false;
        private bool _isEnabled;
        private ISlaveDevice _slaveDevice;

        //private Color _outputAtmosphereStartColor;
        //private Color _outputAtmosphereStopColor;
        //private string _outputAtmosphereMode;
        //private int _outputSelectedMusicMode;
        //private int _outputSelectedMusicPalette;

        //private int _outputSelectedAudioDevice;

        //private int _outputSelectedChasingPalette;
        //private int _outputSelectedMode;
        //private int _outputPaletteSpeed = 20;
        //private int _outputPaletteBlendStep;
        //private Color _outputStaticColor;
        //private int _outputStaticColorMode = 0;
        //private int _outputStaticColorGradientMode = 0;
        //private int _outputScreenCapturePosition = 0;
        //private int _outputScreenCaptureWB = 0;
        //private int _outputMusicDancingMode = 0;
        //private int _outputColorPaletteMode = 0;
        //private int _outputBreathingSpeed = 20000;

        //private IColorPalette _outputCurrentActivePalette;
        private IControlZone[] _controlableZone;
        //private bool _isInSpotEditWizard = false;
        private string _geometry = "generaldevice";
        //private int _outputSmoothness = 2;
        //private int _outputPaletteChasingPosition = 2;
        //private int _outputScreenCaptureWBRed = 100;
        //private int _outputScreenCaptureWBGreen = 100;
        //private int _outputScreenCaptureWBBlue = 100;
        //private int _outputScreenCapturePositionIndex = 0;
    
        //private int _outputMusicSensitivity = 10;
        //private int _outputMusicVisualizerFreq = 0;
        //private bool _outputIsSystemSync = true;
        //private bool _isBrightnessPopupOpen = false;
 
        //private int _vUOrientation = 0;
        //private int _vUMode = 0;
        //private IGifCard _outputSelectedGif = null;
        //private int _outputSelectedGifIndex = 0;
        //private MotionCard _outputSelectedMotion;


        public bool IsLoadingProfile { get; set; }
        public OutputSettings()
        {
            //AvailableLightingMode = new List<ILightingMode>();
            VisualProperties = new VisualProperties();
            Scale = new System.Windows.Point(1, 1);
        }

        //private int _outputGifSpeed = 20;
        //private IGradientColorCard _outputSelectedGradient = new GradientColorCard("default", "application", "unknown", "auto create", Color.FromRgb(255, 127, 0), Color.FromRgb(0, 127, 255));
        public string OutputName { get => _outputName; set { Set(() => OutputName, ref _outputName, value); } }
        //[Reflectable]
        //public int VUOrientation { get => _vUOrientation; set { Set(() => VUOrientation, ref _vUOrientation, value); } }
        //[Reflectable]
        //public int OutputGifSpeed { get => _outputGifSpeed; set { Set(() => OutputGifSpeed, ref _outputGifSpeed, value); } }
       
        public string TargetDevice { get => _targetDevice; set { Set(() => TargetDevice, ref _targetDevice, value); } }
        //[Reflectable]
        //public int VUMode { get => _vUMode; set { Set(() => VUMode, ref _vUMode, value); } }

        private double _actualWidth;
        private double _actualHeight;
        public double ActualWidth { get => _actualWidth; set { Set(() => ActualWidth, ref _actualWidth, value); } }

        public double ActualHeight { get => _actualHeight; set { Set(() => ActualHeight, ref _actualHeight, value); } }
        public int OutputID { get => _outputID; set { Set(() => OutputID, ref _outputID, value); } }
        public OutputTypeEnum OutputType { get => _outputType; set { Set(() => OutputType, ref _outputType, value); } }
        public string OutputDescription { get => _outputDescription; set { Set(() => OutputDescription, ref _outputDescription, value); } }
        //public bool IsBrightnessPopupOpen { get => _isBrightnessPopupOpen; set { Set(() => IsBrightnessPopupOpen, ref _isBrightnessPopupOpen, value); } }

        //[Reflectable]
        //public IGradientColorCard OutputSelectedGradient { get => _outputSelectedGradient; set { Set(() => OutputSelectedGradient, ref _outputSelectedGradient, value); } }
        //[Reflectable]
        //public int OutputSelectedGifIndex { get => _outputSelectedGifIndex; set { Set(() => OutputSelectedGifIndex, ref _outputSelectedGifIndex, value); } }
        //[Reflectable]
        //public IGifCard OutputSelectedGif { get => _outputSelectedGif; set { Set(() => OutputSelectedGif, ref _outputSelectedGif, value); } }
        public bool IsVissible { get => _isVissible; set { Set(() => IsVissible, ref _isVissible, value); } }
        public bool OutputIsSelected { get => _outputIsSelected; set { Set(() => OutputIsSelected, ref _outputIsSelected, value); } }
        public Rectangle Rectangle { get; set; }
        public string OutputUniqueID { get => _outputUniqueID; set { Set(() => OutputUniqueID, ref _outputUniqueID, value); } }
        //[Reflectable]
       
        public bool OutputIsVisible { get => _outputIsVisible; set { Set(() => OutputIsVisible, ref _outputIsVisible, value); } }
        //[Reflectable]
        //public int OutputBrightness { get => _outputBrightness; set { Set(() => OutputBrightness, ref _outputBrightness, value); } }
        public int OutputPowerVoltage { get => _outputPowerVoltage; set { Set(() => OutputPowerVoltage, ref _outputPowerVoltage, value); } }
        public int OutputPowerMiliamps { get => _outputPowerMiliamps; set { Set(() => OutputPowerMiliamps, ref _outputPowerMiliamps, value); } }
        //[Reflectable]
        //public byte OutputSaturationThreshold { get => _outputSaturationThreshold; set { Set(() => OutputSaturationThreshold, ref _outputSaturationThreshold, value); } }
        //[Reflectable]
        //public bool OutputUseLinearLighting { get => _outputUseLinearLighting; set { Set(() => OutputUseLinearLighting, ref _outputUseLinearLighting, value); } }
        //[Reflectable]
        public bool IsEnabled { get => _isEnabled; set { Set(() => IsEnabled, ref _isEnabled, value); } }
     
        //[Reflectable]
        //public Color OutputAtmosphereStartColor { get => _outputAtmosphereStartColor; set { Set(() => OutputAtmosphereStartColor, ref _outputAtmosphereStartColor, value); } }
        //[Reflectable]
        //public Color OutputAtmosphereStopColor { get => _outputAtmosphereStopColor; set { Set(() => OutputAtmosphereStopColor, ref _outputAtmosphereStopColor, value); } }
        //[Reflectable]
        //public string OutputAtmosphereMode { get => _outputAtmosphereMode; set { Set(() => OutputAtmosphereMode, ref _outputAtmosphereMode, value); } }
        //[Reflectable]
        ////string SelectedEffect { get; set; }
        //public int OutputSelectedMusicMode { get => _outputSelectedMusicMode; set { Set(() => OutputSelectedMusicMode, ref _outputSelectedMusicMode, value); } }
        //[Reflectable]
        //public int OutputSelectedMode { get => _outputSelectedMode; set { Set(() => OutputSelectedMode, ref _outputSelectedMode, value); } }





        //[Reflectable]
        //public int OutputSelectedMusicPalette { get => _outputSelectedMusicPalette; set { Set(() => OutputSelectedMusicPalette, ref _outputSelectedMusicPalette, value); } }
        //[Reflectable]
       
        //[Reflectable]
        //public int OutputSelectedAudioDevice { get => _outputSelectedAudioDevice; set { Set(() => OutputSelectedAudioDevice, ref _outputSelectedAudioDevice, value); } } 
        //[Reflectable]
        //public int OutputSelectedChasingPalette { get => _outputSelectedChasingPalette; set { Set(() => OutputSelectedChasingPalette, ref _outputSelectedChasingPalette, value); } }
        //[Reflectable]
        //public int OutputPaletteSpeed { get => _outputPaletteSpeed; set { Set(() => OutputPaletteSpeed, ref _outputPaletteSpeed, value); } }
        //[Reflectable]
        //public int OutputPaletteBlendStep { get => _outputPaletteBlendStep; set { Set(() => OutputPaletteBlendStep, ref _outputPaletteBlendStep, value); } }
        //[Reflectable]
        //public Color OutputStaticColor { get => _outputStaticColor; set { Set(() => OutputStaticColor, ref _outputStaticColor, value); } }
        //[Reflectable]
        //public int OutputStaticColorMode { get => _outputStaticColorMode; set { Set(() => OutputStaticColorMode, ref _outputStaticColorMode, value); } }
        ///// <summary>
        ///// this section contain all reflectable property that can apply for multiple outputs of the device. Why? Because you will not want to change the name of different output when changing name
        ///// of current selected output
        ///// </summary>
        //[Reflectable]
        //public int OutputColorPaletteMode { get => _outputColorPaletteMode; set { Set(() => OutputColorPaletteMode, ref _outputColorPaletteMode, value); } }
        //[Reflectable]
        //public int OutputStaticColorGradientMode { get => _outputStaticColorGradientMode; set { Set(() => OutputStaticColorGradientMode, ref _outputStaticColorGradientMode, value); } }
        //public int OutputScreenCapturePosition { get => _outputScreenCapturePosition; set { Set(() => OutputScreenCapturePosition, ref _outputScreenCapturePosition, value); } }
        //[Reflectable]
        //public int OutputScreenCaptureWB { get => _outputScreenCaptureWB; set { Set(() => OutputScreenCaptureWB, ref _outputScreenCaptureWB, value); } }
        //[Reflectable]
        //public int OutputMusicDancingMode { get => _outputMusicDancingMode; set { Set(() => OutputMusicDancingMode, ref _outputMusicDancingMode, value); } }
        //[Reflectable]
        //public int OutputBreathingSpeed { get => _outputBreathingSpeed; set { Set(() => OutputBreathingSpeed, ref _outputBreathingSpeed, value); } }
        //[Reflectable]
        //public IColorPalette OutputCurrentActivePalette { get => _outputCurrentActivePalette; set { Set(() => OutputCurrentActivePalette, ref _outputCurrentActivePalette, value); } }
        //[Reflectable]
      
        //public bool IsInSpotEditWizard { get => _isInSpotEditWizard; set { Set(() => IsInSpotEditWizard, ref _isInSpotEditWizard, value); } }
        //[Reflectable]
        public string Geometry { get => _geometry; set { Set(() => Geometry, ref _geometry, value); } }
        //[Reflectable]
        //public int OutputSmoothness { get => _outputSmoothness; set { Set(() => OutputSmoothness, ref _outputSmoothness, value); } }
        //[Reflectable]
        //public int OutputMusicVisualizerFreq { get => _outputMusicVisualizerFreq; set { Set(() => OutputMusicVisualizerFreq, ref _outputMusicVisualizerFreq, value); } }
        //[Reflectable]
        //public int OutputPaletteChasingPosition { get => _outputPaletteChasingPosition; set { Set(() => OutputPaletteChasingPosition, ref _outputPaletteChasingPosition, value); } }
        //[Reflectable]
        //public int OutputScreenCaptureWBRed { get => _outputScreenCaptureWBRed; set { Set(() => OutputScreenCaptureWBRed, ref _outputScreenCaptureWBRed, value); } }
        //[Reflectable]
        //public int OutputScreenCaptureWBGreen { get => _outputScreenCaptureWBGreen; set { Set(() => OutputScreenCaptureWBGreen, ref _outputScreenCaptureWBGreen, value); } }
        //[Reflectable]
        //public int OutputScreenCaptureWBBlue { get => _outputScreenCaptureWBBlue; set { Set(() => OutputScreenCaptureWBBlue, ref _outputScreenCaptureWBBlue, value); } }
        //[Reflectable]
        //public int OutputMusicSensitivity { get => _outputMusicSensitivity; set { Set(() => OutputMusicSensitivity, ref _outputMusicSensitivity, value); } }
        //[Reflectable]
        //public int OutputScreenCapturePositionIndex { get => _outputScreenCapturePositionIndex; set { Set(() => OutputScreenCapturePositionIndex, ref _outputScreenCapturePositionIndex, value); } }


        //[Reflectable]
        //public MotionCard OutputSelectedMotion { get => _outputSelectedMotion; set { Set(() => OutputSelectedMotion, ref _outputSelectedMotion, value); } }



        
        //public List<ILightingMode> AvailableLightingMode { get => _availableLightingMode; set { Set(() => AvailableLightingMode, ref _availableLightingMode, value); } }
        //public ILightingMode CurrentActiveLightingMode => AvailableLightingMode[CurrentActiveLightingModeIndex];
        //public int CurrentActiveLightingModeIndex { get => _currentActiveLightingModeIndex; set { Set(() => CurrentActiveLightingModeIndex, ref _currentActiveLightingModeIndex, value); } }

     
       
     
        public string OutputInterface { get; set; }
        public ISlaveDevice SlaveDevice { get => _slaveDevice; set { Set(() => SlaveDevice, ref _slaveDevice, value); } }

        public bool ShouldSerializeAvailableControllers()
        {
            return IsLoadingProfile;
        }

        /// <summary>
        /// Idrawable implementation
        /// </summary>
        private double _top = 0;
        private double _left = 0;
        private bool _isSelected;
        private bool _isSelectable = true;
        private bool _isDraggable = true;
        private double _width = 100;
        private double _height = 100;
        private VisualProperties _visualProperties;
        private bool _shouldBringIntoView;
        private System.Windows.Point _directionPoint;
        private RelayCommand<double> leftChangedCommand;
        private RelayCommand<double> topChangedCommand;
        private double _angle = 0;
        private bool _hasCustomBehavior;
        private string _name;

        private bool _isDeleteable;
        private bool _isResizeable;
        private double _scaleTop;
        private double _scaleLeft;
        private double _scaleWidth = 1;
        private double _scaleHeight = 1;

        public bool IsDeleteable { get => _isDeleteable; set { Set(() => IsDeleteable, ref _isDeleteable, value); } }
        public bool IsResizeable { get => _isResizeable; set { Set(() => IsResizeable, ref _isResizeable, value); } }
        public double CenterX => Width / 2 + Left;
        public double CenterY => Height / 2 + Top;
        public string Name { get; set; }
        public Type DataType => typeof(OutputSettings);
        public double Angle { get => _angle; set { Set(() => Angle, ref _angle, value); OnRotationChanged(); } }
        public double Top { get => _top; set { Set(() => Top, ref _top, value); } }

        public double Left { get => _left; set { Set(() => Left, ref _left, value); } }

        public bool IsSelected { get => _isSelected; set { Set(() => IsSelected, ref _isSelected, value); OnIsSelectedChanged(value); } }

        public double Width { get => _width; set { Set(() => Width, ref _width, value); OnWidthUpdated(); } }

        public double Height { get => _height; set { Set(() => Height, ref _height, value); OnHeightUpdated(); } }

        public VisualProperties VisualProperties { get => _visualProperties; set { Set(() => VisualProperties, ref _visualProperties, value); } }

        public bool IsSelectable { get => _isSelectable; set { Set(() => IsSelectable, ref _isSelectable, value); } }

        public bool IsDraggable { get => _isDraggable; set { Set(() => IsDraggable, ref _isDraggable, value); } }

        public bool HasCustomBehavior { get => _hasCustomBehavior; set { Set(() => HasCustomBehavior, ref _hasCustomBehavior, value); } }

        public bool ShouldBringIntoView { get => _shouldBringIntoView; set { Set(() => ShouldBringIntoView, ref _hasCustomBehavior, value); } }

        public System.Windows.Point Scale { get => _directionPoint; set { Set(() => Scale, ref _directionPoint, value); } }

        public ICommand LeftChangedCommand => leftChangedCommand ??= new RelayCommand<double>(OnLeftChanged);

        public ICommand TopChangedCommand => topChangedCommand ??= new RelayCommand<double>(OnTopChanged);
        public Rectangle GetRect => new Rectangle((int)(Left), (int)(Top), (int)Width, (int)Height);
        protected virtual void OnLeftChanged(double delta) { }

        protected virtual void OnTopChanged(double delta) { }

        protected virtual void OnWidthUpdated() { }

        protected virtual void OnHeightUpdated() { }

        protected virtual void OnRotationChanged() { }

        protected virtual void OnIsSelectedChanged(bool value) { }

        public virtual void OnDrawingEnded(Action<object> callback = default) { }



        //[Reflectable]
        //public bool OutputIsSystemSync { get => _outputIsSystemSync; set { Set(() => OutputIsSystemSync, ref _outputIsSystemSync, value); } }
        //public void SetRectangle(System.Drawing.Rectangle rectangle)
        //{
        //    OutputRectangle = rectangle;
        //    RaisePropertyChanged(nameof(OutputRectangle));
        //}
        //public void SetPreviewRectangle(System.Drawing.Rectangle rectangle)
        //{
        //    PreviewRectangle = rectangle;
        //    RaisePropertyChanged(nameof(PreviewRectangle));
        //}

        /// <summary>
        /// shortcuts for setting value
        /// </summary>
        /// <param name="value"></param>
        public void SetBrightness(int value)
        {
            //brightness up current active mode
            //controlable lighting property

            //var lightingControl = SlaveDevice.ControlableZones.Where(z=>z.GetType() == typeof(lighti))
            //if (lightingControl != null)
            //{
            //    var currentLightingMode = lightingControl.CurrentActiveControlMode as LightingMode;
            //    currentLightingMode.SetBrightness(value);
            //}

        }
        public int GetBrightness()
        {
            //var lightingControl = ControlableProperties.Where(c => c.Type == OutputControlablePropertyEnum.Lighting).FirstOrDefault();
            //int brightness = 0;
            //if (lightingControl != null)
            //{
            //    var currentLightingMode = lightingControl.CurrentActiveControlMode as LightingMode;
            //    brightness = currentLightingMode.GetBrightness();
            //}
            //return brightness;
            return 0;
        }
        public void SetSpeed(int speedValue)
        {
            //var speedControl = ControlableProperties.Where(c => c.Type == OutputControlablePropertyEnum.Speed).FirstOrDefault();
            //if (speedControl != null)
            //{
            //    var currentSpeedMode = speedControl.CurrentActiveControlMode as SpeedMode;
            //    currentSpeedMode.SetSpeed(speedValue);
            //}


        }
        public int GetSpeed()
        {
            //var speedControl = ControlableProperties.Where(c => c.Type == OutputControlablePropertyEnum.Speed).FirstOrDefault();
            //int speed = 0;
            //if (speedControl != null)
            //{
            //    var currentSpeedMode = speedControl.CurrentActiveControlMode as SpeedMode;
            //    speed = currentSpeedMode.GetSpeed();
            //}

            //return speed;
            return 0;
        }
        public int GetCurrentSpeedMode() {
            //SpeedModeEnum speedMode = SpeedModeEnum.manual;
            //var speedControl = ControlableProperties.Where(c => c.Type == OutputControlablePropertyEnum.Speed).FirstOrDefault();  
            //if (speedControl != null)
            //{
            //    var currentSpeedMode = speedControl.CurrentActiveControlMode as SpeedMode;
            //    speedMode = currentSpeedMode.SpeedType;
            //}
            return 0 ;
        }

        public bool SetScale(double scaleX, double scaleY, bool keepOrigin) => throw new NotImplementedException();
    }
}
