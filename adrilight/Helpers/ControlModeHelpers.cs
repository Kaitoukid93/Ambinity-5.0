using adrilight.Util;
using LiveCharts.Defaults;
using LiveCharts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls.WebParts;
using System.Windows.Media;

namespace adrilight.Helpers
{
    public class ControlModeHelpers
    {
        #region default lighting mode by ambino
        /// <summary>
        /// Legacy ColorPalette mode
        /// </summary>
        /// 

        public IControlZone MakeZoneControlable(IControlZone zone)
        {
            zone.AvailableControlMode = new List<IControlMode>();
            zone.AvailableControlMode.Add(ScreenCapturing);
            zone.AvailableControlMode.Add(ColorPalette);
            zone.AvailableControlMode.Add(MusicReactive);
            zone.AvailableControlMode.Add(StaticColor);
            return zone;
        }
        public IControlMode ColorPalette {
            get
            {
                return new LightingMode() {
                    Name = "Color Palette",
                    BasedOn = LightingModeEnum.Rainbow,
                    Creator = "ambino",
                    Owner = "ambino",
                    Description = "Sáng theo dải màu với chuyển động tùy chọn",
                    Parameters = { ChasingPatterns, GenericDirrectionParameter, GenericBrightnessParameter, GenericSpeedParameter(0, 100) }

                };
            }
        }
        public IControlMode ScreenCapturing {
            get
            {
                return new LightingMode() {
                    Name = "Screen Capturing",
                    BasedOn = LightingModeEnum.ScreenCapturing,
                    Creator = "ambino",
                    Owner = "ambino",
                    Description = "Sáng theo màn hình với vị trí cài sẵn",
                    Parameters = { GenericBrightnessParameter, UseLinearLighting }

                };
            }
        }
        /// <summary>
        /// Legacy Music mode
        /// </summary>
        public IControlMode MusicReactive {
            get
            {
                return new LightingMode() {
                    Name = "Music Reactive",
                    BasedOn = LightingModeEnum.Rainbow,
                    Creator = "ambino",
                    Owner = "ambino",
                    Description = "Màu LED chuyển động theo nhạc",
                    Parameters = { GenericDirrectionParameter, GenericBrightnessParameter, GenericSpeedParameter(0, 100) }

                };
            }
        }
        public IControlMode StaticColor {
            get
            {
                return new LightingMode() {
                    Name = "Static Color",
                    BasedOn = LightingModeEnum.StaticColor,
                    Creator = "ambino",
                    Owner = "ambino",
                    Description = "Tất cả LED sáng cùng một màu",
                    Parameters = { GenericBrightnessParameter, GenericColorSelectionParameter }
                };
            }
        }

        public IControlMode FanSpeedAuto {
            get
            {
                return new PWMMode() {
                    Name = "Auto Speed",
                    BasedOn = PWMModeEnum.auto,
                    Creator = "ambino",
                    Owner = "ambino",
                    Description = "Tốc độ Fan thay đổi theo nhiệt độ",
                    //add more options to auto fanspeed such as quiet mode, extreme mode

                };
            }
        }
        public IControlMode FanSpeedManual {
            get
            {
                return new PWMMode() {
                    Name = "Manual Speed",
                    BasedOn = PWMModeEnum.manual,
                    Creator = "ambino",
                    Owner = "ambino",
                    Description = "Cố định tốc độ Fan",
                    Parameters = { GenericSpeedParameter(0, 100) }

                };
            }
        }

        #endregion


        #region default lightingmode parameter defined by ambino
        public IModeParameter GenericSpeedParameter(int min, int max)
        {

            return new ModeParameter() {

                Name = "Speed",
                Description = "Speed of Motion",
                Type = ModeParameterEnum.Speed,
                Template = ModeParameterTemplateEnum.ValueSlider,
                Value = 50,
                MinValue = min,
                MaxValue = max


            };
        }
        public IModeParameter GenericChartVisualizationParameter {
            get
            {
                return new ModeParameter() {

                    Name = "Auto Speed",
                    Description = "Speed of Motion",
                    Type = ModeParameterEnum.Speed,
                    Template = ModeParameterTemplateEnum.ChartVisualization,
                };
            }
        }
        public IModeParameter UseLinearLighting {
            get
            {
                return new ModeParameter() {

                    Name = "Linear Lighting",
                    Description = "The Light Changes with linear brightness",
                    Type = ModeParameterEnum.LinearLighting,
                    Template = ModeParameterTemplateEnum.ToggleOnOff,
                    Value = 0,



                };
            }
        }
        public IModeParameter GenericDirrectionParameter {
            get
            {
                return new ModeParameter() {

                    Name = "Dirrection",
                    Description = "Speed of Motion",
                    Type = ModeParameterEnum.Direction,
                    Template = ModeParameterTemplateEnum.ListSelection,
                    Value = 1,
                    AvailableValueLocalPath = ""

                };
            }
        }
        public IModeParameter GenericColorSelectionParameter {
            get
            {
                return new ModeParameter() {

                    Name = "Colors",
                    Description = "Available Color",
                    Type = ModeParameterEnum.Color,
                    Template = ModeParameterTemplateEnum.ListSelection,
                    Value = 1,
                    AvailableValueLocalPath = ""

                };
            }
        }
        public IModeParameter GenericBrightnessParameter {
            get
            {
                return new ModeParameter() {

                    Name = "Brightness",
                    Description = "Brightness of LEDs",
                    Type = ModeParameterEnum.Brightness,
                    Template = ModeParameterTemplateEnum.ValueSlider,
                    Value = 50,
                    MinValue = 20,
                    MaxValue = 100

                };
            }
        }
        /// <summary>
        /// Use this for rainbow engine to select chasing pattern from database
        /// </summary>
        public IModeParameter ChasingPatterns {
            get
            {
                return new ModeParameter() {

                    Name = "Pattern",
                    Description = "The motion to be colored",
                    Type = ModeParameterEnum.ChasingPattern,
                    Template = ModeParameterTemplateEnum.ListSelection,
                    Value = 50,
                    AvailableValueLocalPath = ""

                };
            }
        }
        /// <summary>
        /// Use this for rainbow engine to select chasing pattern from database
        /// </summary>
        public IModeParameter ColorMode {
            get
            {
                return new ModeParameter() {

                    Name = "ColorMode",
                    Description = "How the color being used",
                    Type = ModeParameterEnum.ColorMode,
                    Template = ModeParameterTemplateEnum.ListSelection,
                    Value = 0,
                    AvailableValueLocalPath = ""


                };
            }
        }
        /// <summary>
        /// Select screen for capturing
        /// </summary>
        public IModeParameter ScreenSelection {
            get
            {
                return new ModeParameter() {

                    Name = "Chọn màn hình",
                    Description = "Chọn màn hình để capture",
                    Type = ModeParameterEnum.ScreenIndex,
                    Template = ModeParameterTemplateEnum.ListSelection,
                    Value = 0,
                };
            }
        }
        #endregion
    }
}

