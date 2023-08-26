using adrilight_shared.Enums;
using adrilight_shared.Models.ControlMode.Mode;
using adrilight_shared.Models.ControlMode.ModeParameters;
using adrilight_shared.Models.Device.Zone;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace adrilight_shared.Models.ControlMode
{
    public class ControlModeHelpers
    {
        private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "adrilight\\");
        private string ColorsCollectionFolderPath => Path.Combine(JsonPath, "Colors");
        private string PalettesCollectionFolderPath => Path.Combine(JsonPath, "ColorPalettes");
        private string VIDCollectionFolderPath => Path.Combine(JsonPath, "VID");
        private string MIDCollectionFolderPath => Path.Combine(JsonPath, "MID");
        private string ChasingPatternsCollectionFolderPath => Path.Combine(JsonPath, "ChasingPatterns");
        #region default lighting mode by ambino
        public IControlZone MakeZoneControlable(IControlZone zone)
        {
            zone.AvailableControlMode = new List<IControlMode>();
            zone.AvailableControlMode.Add(ScreenCapturing);
            zone.AvailableControlMode.Add(ColorPalette);
            zone.AvailableControlMode.Add(MusicReactive);
            zone.AvailableControlMode.Add(StaticColor);
            zone.AvailableControlMode.Add(Animation);
            zone.AvailableControlMode.Add(Gifxelation);
            return zone;
        }
        public IControlMode ColorPalette
        {
            get
            {
                return new LightingMode()
                {
                    Name = "Color Palette",
                    Geometry = "colorpalette",
                    BasedOn = LightingModeEnum.Rainbow,
                    Creator = "ambino",
                    Owner = "ambino",
                    Description = "Sáng theo dải màu với chuyển động tùy chọn",
                    Parameters = { IsEnabled, GenericBrightnessParameter, GenericColorPaletteSelectionParameter, GenericVIDSelectParameter, GenericSpeedParameter(0, 100, 20), IsSystemSync }

                };
            }
        }
        public IControlMode Animation
        {
            get
            {
                return new LightingMode()
                {
                    Name = "Animation",
                    Geometry = "animation",
                    BasedOn = LightingModeEnum.Animation,
                    Creator = "ambino",
                    Owner = "ambino",
                    Description = "LED chuyển động với màu tùy chọn",
                    Parameters = { IsEnabled, GenericBrightnessParameter, ChasingPatterns, GenericColorPaletteAndSolidColorSelectionParameter, GenericSpeedParameter(0, 20, 10) }
                };
            }
        }
        public IControlMode Gifxelation
        {
            get
            {
                return new LightingMode()
                {
                    Name = "Gifxelation",
                    Geometry = "gifxelation",
                    BasedOn = LightingModeEnum.Gifxelation,
                    Creator = "ambino",
                    Owner = "ambino",
                    Description = "LED sáng theo ảnh động Gif",
                    Parameters = { IsEnabled, GenericBrightnessParameter, Gifs, GenericSpeedParameter(0, 10, 1), GenericSmoothParameter, GenericLaunchGifRegionSelectionButtonParameter }
                };
            }
        }
        public IControlMode ScreenCapturing
        {
            get
            {
                return new LightingMode()
                {
                    Name = "Screen Capturing",
                    Geometry = "screencapture",
                    BasedOn = LightingModeEnum.ScreenCapturing,
                    Creator = "ambino",
                    Owner = "ambino",
                    Description = "Sáng theo màn hình với vị trí cài sẵn",
                    Parameters = { IsEnabled, GenericBrightnessParameter, GenericSmoothParameter, UseLinearLighting, GenericLaunchScreenRegionSelectionButtonParameter }

                };
            }
        }
        /// <summary>
        /// Legacy Music mode
        /// </summary>
        public IControlMode MusicReactive
        {
            get
            {
                return new LightingMode()
                {
                    Name = "Music Reactive",
                    Geometry = "music",
                    BasedOn = LightingModeEnum.MusicCapturing,
                    Creator = "ambino",
                    Owner = "ambino",
                    Description = "Màu LED chuyển động theo nhạc",
                    Parameters = { IsEnabled, GenericBrightnessParameter, GenericColorPaletteAndSolidColorSelectionParameter, GenericMIDSelectParameter, GenericLaunchAudioDeviceSelectionButtonParameter }

                };
            }
        }
        public IControlMode StaticColor
        {
            get
            {
                return new LightingMode()
                {
                    Name = "Static Color",
                    Geometry = "genericCircle",
                    BasedOn = LightingModeEnum.StaticColor,
                    Creator = "ambino",
                    Owner = "ambino",
                    Description = "Tất cả LED sáng cùng một màu",
                    Parameters = { IsEnabled, GenericBrightnessParameter, GenericColorSelectionParameter, Breathing }
                };
            }
        }

        public IControlMode FanSpeedAuto
        {
            get
            {
                return new PWMMode()
                {
                    Name = "Auto Speed",
                    Geometry = "autoSpeed",
                    BasedOn = PWMModeEnum.auto,
                    Creator = "ambino",
                    Owner = "ambino",
                    Description = "Tốc độ Fan thay đổi theo nhiệt độ",
                    //add more options to auto fanspeed such as quiet mode, extreme mode

                };
            }
        }
        public IControlMode FanSpeedManual
        {
            get
            {
                return new PWMMode()
                {
                    Name = "Manual Speed",
                    Geometry = "manualSpeed",
                    BasedOn = PWMModeEnum.manual,
                    Creator = "ambino",
                    Owner = "ambino",
                    Description = "Cố định tốc độ Fan",
                    Parameters = { GenericSpeedParameter(20, 100, 80) }

                };
            }
        }

        #endregion


        #region default lightingmode parameter defined by ambino
        public SliderParameter GenericSpeedParameter(int min, int max, int defaultValue)
        {

            return new SliderParameter(min, max, defaultValue, ModeParameterEnum.Speed)
            {
                Name = "Speed",
                Description = "Speed of Motion",
                Geometry = "speed"
            };

        }

        public IModeParameter UseLinearLighting
        {
            get
            {
                return new ToggleParameter(1, ModeParameterEnum.LinearLighting)
                {

                    Name = "Linear Lighting",
                    Description = "The Light Changes with linear brightness",
                };
            }
        }
        public IModeParameter IsSystemSync
        {
            get
            {
                return new ToggleParameter(1, ModeParameterEnum.IsSystemSync)
                {

                    Name = "System Sync",
                    Description = "Đồng bộ với tốc độ hệ thống",
                    SubParams = new ObservableCollection<SubParameter>() {
                        new SubParameter("System Speed",ModeParameterTemplateEnum.ValueSlider,"Speed","Speed",20,100,0){
                            Description = "Tốc độ này sẽ kéo theo toàn bộ các vùng đã bật System Sync"
                        }

                    }
                };
            }
        }
        public IModeParameter Breathing
        {
            get
            {
                return new ToggleParameter(0, ModeParameterEnum.Breathing)
                {

                    Name = "Breathing",
                    Description = "LED sáng dần và tắt dần theo tốc độ",
                    SubParams = new ObservableCollection<SubParameter>() {
                        new SubParameter("Speed", ModeParameterTemplateEnum.ValueSlider, "Speed", "Speed", 100, 1950, 0) {
                            Description = "Tốc độ này độc lập đối với vùng được chọn"
                        },
                        new SubParameter("System Speed",ModeParameterTemplateEnum.ValueSlider,"Speed","Speed",100,1950,0){
                            Description = "Tốc độ này sẽ kéo theo toàn bộ các vùng đã bật System Sync"
                        },
                        new SubParameter("System Sync", ModeParameterTemplateEnum.ToggleOnOff, "sync", "sync", 0, 0, 0) {
                            Description = "Đồng bộ với tốc độ hệ thống"
                        }
                    }



                };
            }
        }
        public IModeParameter IsEnabled
        {
            get
            {
                return new ToggleParameter(1, ModeParameterEnum.IsEnabled)
                {

                    Name = "Enable",
                    Description = "Bật hoặc tắt chế độ này",
                };
            }
        }
        public IModeParameter GenericVIDSelectParameter
        {
            get
            {
                return new ListSelectionParameter(ModeParameterEnum.VID)
                {

                    Name = "Chiều chạy",
                    Description = "Chọn chiều chạy của hiệu ứng",
                    DataSourceLocaFolderNames = new List<string>() { "VID" },
                    SubParams = new ObservableCollection<SubParameter>() {
                        new SubParameter("Intensity",ModeParameterTemplateEnum.ValueSlider,"intensity","intensity",48,100,0), // only show in system generated mode, act as virtual brush intensity
                        new SubParameter("Vẽ chiều chạy mới",ModeParameterTemplateEnum.PushButtonAction,"Add VID","Add",0,0,0), //only show in custom mode
                    }

                };
            }
        }
        public IModeParameter GenericMIDSelectParameter
        {
            get
            {
                return new ListSelectionParameter(ModeParameterEnum.MID)
                {

                    Name = "Tần số",
                    Description = "Chọn tần số cho từng vùng LED",
                    DataSourceLocaFolderNames = new List<string>() { "MID" },
                    SubParams = new ObservableCollection<SubParameter>() {
                        new SubParameter("Chọn tần số chi tiết hơn",ModeParameterTemplateEnum.PushButtonAction,"Add FID","Add",0,0,0),
                        new SubParameter("Visualizer",ModeParameterTemplateEnum.ListSelection,"Dancing Mode","",0,0,0){ AvailableValue = new List<string>(){ "Brightness", "VU Metter"}},
                        new SubParameter("VU mode",ModeParameterTemplateEnum.ListSelection,"Vu Mode","",0,0,0){ AvailableValue = new List<string>(){ "Normal", "Inverse", "Floating"}},
                    }

                };
            }
        }

        public IModeParameter GenericColorSelectionParameter
        {
            get
            {
                return new ListSelectionParameter(ModeParameterEnum.Color)
                {

                    Name = "Colors",
                    Description = "Available Colors",
                    DataSourceLocaFolderNames = new List<string>() { "Colors" },
                    SubParams = new ObservableCollection<SubParameter>() {
                        new SubParameter("Custom Color",ModeParameterTemplateEnum.PushButtonAction,"Add Color","Add",0,0,0)
                    }

                };
            }
        }
        public IModeParameter GenericColorPaletteSelectionParameter
        {
            get
            {
                return new ListSelectionParameter(ModeParameterEnum.Palette)
                {

                    Name = "Palettes",
                    Description = "Available Palettes",
                    DataSourceLocaFolderNames = new List<string>() { "ColorPalettes" },
                    SubParams = new ObservableCollection<SubParameter>() {
                        new SubParameter("Custom Palette",ModeParameterTemplateEnum.PushButtonAction,"Add Palette","Add",0,0,0),
                         new SubParameter("Export Palette",ModeParameterTemplateEnum.PushButtonAction,"Export Palette","Export",0,0,0)
                    }

                };
            }
        }
        public IModeParameter GenericColorPaletteAndSolidColorSelectionParameter
        {
            get
            {
                return new ListSelectionParameter(ModeParameterEnum.MixedColor)
                {

                    Name = "Colors",
                    Description = "Available Colors",
                    DataSourceLocaFolderNames = new List<string>() { "ColorPalettes", "Colors" },
                    SubParams = new ObservableCollection<SubParameter>() {
                        new SubParameter("Palette Color Use",ModeParameterTemplateEnum.ListSelection,"Color Mode","",0,0,0){ AvailableValue = new List<string>(){"Static","Moving"}}, // include static palette, moving palette, cyclic palette color
                        new SubParameter("Speed Of Change",ModeParameterTemplateEnum.ValueSlider,"Speed of change","",1,5,1),
                        new SubParameter("Color Intensity",ModeParameterTemplateEnum.ValueSlider,"Color Intensity","",5,16,2),

                    }

                };
            }
        }

        public IModeParameter GenericBrightnessParameter
        {
            get
            {
                return new SliderParameter(10, 100, 60, ModeParameterEnum.Brightness)
                {

                    Name = "Brightness",
                    Geometry = "brightness",
                    Description = "Brightness of LEDs",

                };
            }
        }
        public IModeParameter GenericSmoothParameter
        {
            get
            {
                return new SliderParameter(1, 7, 2, ModeParameterEnum.Smoothing)
                {

                    Name = "Smoothing",
                    Geometry = "smooth",
                    Description = "Smooth factor of LEDs",

                };
            }
        }
        public IModeParameter GenericLaunchScreenRegionSelectionButtonParameter
        {
            get
            {
                return new CapturingRegionSelectionButtonParameter(new CapturingRegion(0, 0, 1.0, 1.0), "screenRegionSelection")
                {

                    Name = "Cài đặt vị trí",
                    Description = "Mở cửa sổ cài đặt vị trí capture",
                    Geometry = "position"

                };
            }
        }
        public IModeParameter GenericLaunchGifRegionSelectionButtonParameter
        {
            get
            {
                return new CapturingRegionSelectionButtonParameter(new CapturingRegion(0, 0, 1.0, 1.0), "gifRegionSelection")
                {

                    Name = "Cài đặt vị trí",
                    Description = "Mở cửa sổ cài đặt vị trí capture",
                    Geometry = "position"

                };
            }
        }
        public IModeParameter GenericLaunchAudioDeviceSelectionButtonParameter
        {
            get
            {
                return new AudioDeviceSelectionButtonParameter("audioDevice")
                {

                    Name = "Chọn đầu ra âm thanh",
                    Description = "Đầu ra âm thanh cần capture, thay đổi có ảnh hưởng đế tất cả thiết bị",
                    Geometry = "speaker"

                };
            }
        }
        public IModeParameter ChasingPatterns
        {
            get
            {
                return new ListSelectionParameter(ModeParameterEnum.ChasingPattern)
                {
                    Name = "Pattern",
                    Description = "The motion to be colored",
                    DataSourceLocaFolderNames = new List<string>() { "ChasingPatterns" },
                    SubParams = new ObservableCollection<SubParameter>() {
                         new SubParameter("Import Pattern",ModeParameterTemplateEnum.PushButtonAction,"Import Pattern","Import",0,0,0),
                         new SubParameter("Export Pattern",ModeParameterTemplateEnum.PushButtonAction,"Export Pattern","Export",0,0,0)
                    }
                };
            }
        }
        public IModeParameter Gifs
        {
            get
            {
                return new ListSelectionParameter(ModeParameterEnum.Gifs)
                {
                    Name = "Gifs",
                    Description = "Các ảnh động có thể chọn",
                    DataSourceLocaFolderNames = new List<string>() { "Gifs" },
                    SubParams = new ObservableCollection<SubParameter>() {
                         new SubParameter("Import Gif",ModeParameterTemplateEnum.PushButtonAction,"Import Gif","Import",0,0,0),
                         new SubParameter("Export Gif",ModeParameterTemplateEnum.PushButtonAction,"Export Gif","Export",0,0,0)
                    }
                };
            }
        }
        #endregion
    }
}

