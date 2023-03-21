using adrilight.DesktopDuplication;
using adrilight.Resources;
using adrilight.Settings;
using adrilight.Spots;
using adrilight.Util;
using adrilight.View;
using adrilight_effect_analyzer.Model;
using Dropbox.Api;
using Dropbox.Api.Files;
using DropBoxServer;
using Emgu.CV;
using FTPServer;
using GalaSoft.MvvmLight;
using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Themes;
using LiveCharts;
using MoreLinq;
using Newtonsoft.Json;
using NonInvasiveKeyboardHookLibrary;
using OpenRGB.NET.Models;
using Renci.SshNet;
using SharpDX.WIC;
using Squirrel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IdentityModel.Claims;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TimeLineTool;
using Un4seen.BassWasapi;
using static Dropbox.Api.TeamLog.AdminAlertSeverityEnum;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using Bitmap = System.Drawing.Bitmap;
using BitmapSource = System.Windows.Media.Imaging.BitmapSource;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using ColorPalette = adrilight.Util.ColorPalette;
using NotifyIcon = HandyControl.Controls.NotifyIcon;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using Point = System.Windows.Point;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;
using SplashScreen = adrilight.View.SplashScreen;
using MdXaml;
using HandyControl.Tools;
using adrilight.Helpers;
using SharpDX.DXGI;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Header;
using NAudio.Gui;
using System.Windows.Media.Media3D;

namespace adrilight.ViewModel
{
    public class MainViewViewModel : BaseViewModel
    {
        private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "adrilight\\");

        private string JsonDeviceFileNameAndPath => Path.Combine(JsonPath, "adrilight-deviceInfos.json");
        private string JsonGeneralFileNameAndPath => Path.Combine(JsonPath, "adrilight-settings.json");
        private string JsonSolidColorFileNameAndPath => Path.Combine(JsonPath, "adrilight-solidColor.json");
        private string JsonDeviceProfileFileNameAndPath => Path.Combine(JsonPath, "adrilight-deviceProfiles.json");
        private string JsonAutomationFileNameAndPath => Path.Combine(JsonPath, "adrilight-automations.json");
        private string JsonOpenRGBDevicesFileNameAndPath => Path.Combine(JsonPath, "adrilight-openrgbdevices.json");
        private string JsonGifsFileNameAndPath => Path.Combine(JsonPath, "Gif");
        private string JsonFWToolsFileNameAndPath => Path.Combine(JsonPath, "FWTools");

        private string JsonFWToolsFWListFileNameAndPath => Path.Combine(JsonPath, "adrilight-fwlist.json");
        private string JsonGifsCollectionFileNameAndPath => Path.Combine(JsonPath, "adrilight-gifCollection.json");
        private string JsonGroupFileNameAndPath => Path.Combine(JsonPath, "adrilight-groupInfos.json");

        private string JsonGradientFileNameAndPath => Path.Combine(JsonPath, "adrilight-GradientCollection.json");

        private const string ADRILIGHT_RELEASES = "https://github.com/Kaitoukid93/Ambinity_Developer_Release";
        #region default lighting mode by ambino
        /// <summary>
        /// Legacy ColorPalette mode
        /// </summary>
        public IControlMode ColorPalette {
            get
            {
                return new LightingMode() {
                    Name = "Color Palette",
                    BasedOn = LightingModeEnum.Rainbow,
                    Creator = "ambino",
                    Owner = "ambino",
                    Description = "Sáng theo dải màu với chuyển động tùy chọn",
                    Parameters = { ChasingPatterns, GenericDirrectionParameter, GenericBrightnessParameter, GenericSpeedParameter }

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
                    Parameters = { GenericDirrectionParameter, GenericBrightnessParameter, GenericSpeedParameter }

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
                    Description = "Tốc độ Fan thay đổi theo nhiệt độ"
                    //Parameters = { ChasingPatterns, GenericDirrectionParameter, GenericBrightnessParameter, GenericSpeedParameter }

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
                    Parameters = { GenericSpeedParameter }

                };
            }
        }

        #endregion


        #region default lightingmode parameter defined by ambino
        public IModeParameter GenericSpeedParameter {
            get
            {
                return new ModeParameter() {

                    Name = "Speed",
                    Description = "Speed of Motion",
                    Type = ModeParameterEnum.Speed,
                    Template = ModeParameterTemplateEnum.ValueSlider,
                    Value = 50,
                    MinValue = 0,
                    MaxValue = 100

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
                    AvailableValue = new List<object>() { "Foward", "Reverse" }

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
                    AvailableValue = new List<object>() { "bouncing", "fucking", "humping" }

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
                    AvailableValue = new List<object>() { "Solid", "Random", "Cyclic", "Full" }


                };
            }
        }
        #endregion


        #region database local folder paths
        private string PalettesCollectionFolderPath => Path.Combine(JsonPath, "ColorPalettes");
        private string AnimationsCollectionFolderPath => Path.Combine(JsonPath, "Animations");
        private string ChasingPatternsCollectionFolderPath => Path.Combine(JsonPath, "ChasingPatterns");
        private string AutomationsCollectionFolderPath => Path.Combine(JsonPath, "Automations");
        private string DevicesCollectionFolderPath => Path.Combine(JsonPath, "Devices");
        private string LEDSetupsCollectionFolderPath => Path.Combine(JsonPath, "LEDSetups");
        private string GradientsCollectionFolderPath => Path.Combine(JsonPath, "Gradients");
        private string SolidColorsCollectionFolderPath => Path.Combine(JsonPath, "SolidColors");

        #endregion


        #region constant string

        public const string ImagePathFormat = "pack://application:,,,/adrilight;component/View/Images/{0}";
        public const string dashboard = "Dashboard";
        public const string deviceSetting = "Device Settings";
        public const string appSetting = "App Settings";
        public const string faq = "FAQ";
        public const string general = "General";
        public const string lighting = "Lighting";
        public const string canvasLighting = "Canvas Lighting";
        public const string groupLighting = "Group Lighting";

        #endregion constant string

        #region property

        private ObservableCollection<VerticalMenuItem> _menuItems;

        public ObservableCollection<VerticalMenuItem> MenuItems {
            get { return _menuItems; }
            set
            {
                if (_menuItems == value) return;
                _menuItems = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<int> _availableBaudrates;

        public ObservableCollection<int> AvailableBaudrates {
            get
            {
                return _availableBaudrates;
            }

            set
            {
                _availableBaudrates = value;
                RaisePropertyChanged(nameof(AvailableBaudrates));
            }
        }
        private ObservableCollection<SystemTrayContextMenu> _availableContextMenus;

        public ObservableCollection<SystemTrayContextMenu> AvailableContextMenus {
            get
            {
                return _availableContextMenus;
            }

            set
            {
                _availableContextMenus = value;
                RaisePropertyChanged(nameof(AvailableContextMenus));
            }
        }



        private List<IActionParameter> _availableParametersforCurrentAction;

        public List<IActionParameter> AvailableParametersforCurrentAction {
            get
            {
                return _availableParametersforCurrentAction;
            }

            set
            {
                _availableParametersforCurrentAction = value;
                RaisePropertyChanged();
            }
        }

        public List<string> AvailableRGBOrders {
            get
            {
                return new List<string>() {
                    "RGB",
                    "RBG",
                    "GRB",
                    "GBR",
                    "BGR",
                    "BRG"
                };
            }
        }

        private List<IActionParameter> _allAvailableParametersforCurrentAction;

        public List<IActionParameter> AllAvailableParametersforCurrentAction {
            get
            {
                return _allAvailableParametersforCurrentAction;
            }

            set
            {
                _allAvailableParametersforCurrentAction = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<ActionType> _availableActionsforCurrentDevice;

        public ObservableCollection<ActionType> AvailableActionsforCurrentDevice {
            get
            {
                return _availableActionsforCurrentDevice;
            }

            set
            {
                _availableActionsforCurrentDevice = value;

                RaisePropertyChanged();
            }
        }

        private IActionSettings _currentSelectedAction;

        public IActionSettings CurrentSelectedAction {
            get
            {
                return _currentSelectedAction;
            }

            set
            {
                _currentSelectedAction = value;
            }
        }
        private ObservableCollection<KeyModel> _currentSelectedShortKeys;
        public ObservableCollection<KeyModel> CurrentSelectedShortKeys {
            get { return _currentSelectedShortKeys; }
            set
            {
                _currentSelectedShortKeys = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<string> _currentSelectedModifiers;
        public ObservableCollection<string> CurrentSelectedModifiers {
            get { return _currentSelectedModifiers; }
            set
            {
                _currentSelectedModifiers = value;
                RaisePropertyChanged();
            }
        }

        private IDeviceFirmware _currentSelectedFirmware;

        public IDeviceFirmware CurrentSelectedFirmware {
            get
            {
                return _currentSelectedFirmware;
            }

            set
            {
                _currentSelectedFirmware = value;
                RaisePropertyChanged();
            }
        }

        private IActionParameter _selectedParameter;

        public IActionParameter SelectedParameter {
            get { return _selectedParameter; }
            set { _selectedParameter = value; }
        }

        private string _selectedActionType;







        private string _buildVersion = "";

        public string BuildVersion {
            get { return _buildVersion; }
            set
            {
                if (_buildVersion == value) return;
                _buildVersion = value;
                RaisePropertyChanged();
            }
        }

        private DateTime? _lastUpdate;

        public DateTime? LastUpdate {
            get { return _lastUpdate; }
            set
            {
                if (_lastUpdate == value) return;
                _lastUpdate = value;
                RaisePropertyChanged();
            }
        }

        private string _author = "";

        public string Author {
            get { return _author; }
            set
            {
                if (_author == value) return;
                _author = value;
                RaisePropertyChanged();
            }
        }

        private string _git = "";

        public string Git {
            get { return _git; }
            set
            {
                if (_git == value) return;
                _git = value;
                RaisePropertyChanged();
            }
        }

        private string _faq = "";

        public string FAQ {
            get { return _faq; }
            set
            {
                if (_faq == value) return;
                _faq = value;
                RaisePropertyChanged();
            }
        }

        private string _appName = "";

        public string AppName {
            get { return _appName; }
            set
            {
                if (_appName == value) return;
                _appName = value;
                RaisePropertyChanged();
            }
        }

        private IDeviceSettings _currentDevice;

        public IDeviceSettings CurrentDevice {
            get { return _currentDevice; }
            set
            {
                if (_currentDevice == value) return;

                _currentDevice = value;

                RaisePropertyChanged();
            }
        }

        private IOutputSettings _currentOutput;

        public IOutputSettings CurrentOutput {
            get { return _currentOutput; }
            set
            {
                _currentOutput = value;
                CurrentOutput.PropertyChanged += (s, e) =>
                {
                    switch (e.PropertyName)
                    {


                        //case nameof(CurrentOutput.OutputMusicSensitivity):
                        //    SensitivityThickness = new Thickness(0, 0, 0, CurrentOutput.OutputMusicSensitivity + 15);
                        //    RaisePropertyChanged(nameof(SensitivityThickness));
                        //    break;

                        //case nameof(CurrentOutput.OutputMusicVisualizerFreq):
                        //    if (CurrentOutput.OutputMusicVisualizerFreq == 0) // bass configuration
                        //    {
                        //        foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
                        //        {
                        //            spot.SetMID(20);
                        //        }
                        //    }
                        //    else if (CurrentOutput.OutputMusicVisualizerFreq == 1)// mid configuration
                        //    {
                        //        foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
                        //        {
                        //            spot.SetMID(100);
                        //        }
                        //    }
                        //    else if (CurrentOutput.OutputMusicVisualizerFreq == 2)// treble configuration
                        //    {
                        //        foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
                        //        {
                        //            spot.SetMID(500);
                        //        }
                        //    }
                        //    else if (CurrentOutput.OutputMusicVisualizerFreq == 3)// Full range configuration
                        //    {
                        //        foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
                        //        {
                        //            var mid = (spot.id * 1024 / CurrentOutput.OutputLEDSetup.Spots.Count);
                        //            spot.SetMID(mid);
                        //        }
                        //    }
                        //    RaisePropertyChanged(nameof(SensitivityThickness));
                        //    break;

                        //case nameof(CurrentOutput.OutputPaletteChasingPosition):
                        //    switch (CurrentOutput.OutputPaletteChasingPosition)
                        //    {
                        //        case 0:
                        //            foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
                        //            {
                        //                spot.SetStroke(0.5);
                        //            }
                        //            SetIDMode = "VID";
                        //            ProcessSelectedSpotsWithRange(0, 64);
                        //            foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
                        //            {
                        //                spot.SetStroke(0);
                        //            }
                        //            break;

                        //        case 1:
                        //            foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
                        //            {
                        //                spot.SetStroke(0.5);
                        //            }
                        //            SetIDMode = "VID";
                        //            ProcessSelectedSpotsWithRange(0, 128);
                        //            foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
                        //            {
                        //                spot.SetStroke(0);
                        //            }
                        //            break;

                        //        case 2:
                        //            foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
                        //            {
                        //                spot.SetStroke(0.5);
                        //            }
                        //            SetIDMode = "VID";
                        //            ProcessSelectedSpotsWithRange(0, 256);
                        //            foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
                        //            {
                        //                spot.SetStroke(0);
                        //            }
                        //            break;

                        //        case 3:
                        //            foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
                        //            {
                        //                spot.SetStroke(0.5);
                        //            }
                        //            SetIDMode = "VID";
                        //            ProcessSelectedSpotsWithRange(0, 512);
                        //            foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
                        //            {
                        //                spot.SetStroke(0);
                        //            }
                        //            break;

                        //        case 4:
                        //            foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
                        //            {
                        //                spot.SetStroke(0.5);
                        //            }
                        //            SetIDMode = "VID";
                        //            ProcessSelectedSpotsWithRange(0, 1024);
                        //            foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
                        //            {
                        //                spot.SetStroke(0);
                        //            }
                        //            break;
                        //    }

                        //    break;

                        //case nameof(CurrentOutput.OutputIsEnabled):
                        //    if (CurrentDevice.AvailableOutputs.Length == 1)
                        //        CurrentDevice.IsEnabled = CurrentOutput.OutputIsEnabled;
                        //    break;
                    }
                    //WriteDeviceInfoJson();
                };

                RaisePropertyChanged();
            }
        }

        public ICommand CompositionNextFrameCommand { get; set; }
        public ICommand SelectPIDCanvasItemCommand { get; set; }
        public ICommand SelectLiveViewItemCommand { get; set; }
        public ICommand LaunchAdrilightStoreItemCeatorWindowCommand { get; set; }
        public ICommand DownloadSelectedChasingPattern { get; set; }
        public ICommand DownloadSelectedPaletteCommand { get; set; }
        public ICommand CutSelectedMotionCommand { get; set; }
        public ICommand ToggleCompositionPlayingStateCommand { get; set; }
        public ICommand CompositionFrameStartOverCommand { get; set; }
        public ICommand CompositionPreviousFrameCommand { get; set; }
        public ICommand OpenRectangleScaleCommand { get; set; }
        public ICommand SetSelectedMotionCommand { get; set; }
        public ICommand OpenAvailableLedSetupForCurrentDeviceWindowCommand { get; set; }
        public ICommand NextOOTBCommand { get; set; }
        public ICommand OpenTutorialCommand { get; set; }
        public ICommand FinishOOTBCommand { get; set; }
        public ICommand PrevioustOOTBCommand { get; set; }
        public ICommand SkipOOTBCommand { get; set; }
        public ICommand PIDWindowClosingCommand { get; set; }
        public ICommand SurfaceEditorWindowClosingCommand { get; set; }
        public ICommand ClearPIDCanvasCommand { get; set; }
        public ICommand DeleteSelectedItemsCommand { get; set; }
        public ICommand OpenRectangleRotateCommand { get; set; }
        public ICommand AddItemsToPIDCanvasCommand { get; set; }
        public ICommand AddImageToPIDCanvasCommand { get; set; }
        public ICommand SaveCurretSurfaceLayoutCommand { get; set; }
        public ICommand SaveCurrentLEDSetupLayoutCommand { get; set; }
        public ICommand SetRandomOutputColorCommand { get; set; }
        public ICommand LockSelectedItemCommand { get; set; }
        public ICommand OpenAddNewDrawableItemCommand { get; set; }
        public ICommand UnlockSelectedItemCommand { get; set; }
        public ICommand SetSelectedItemLiveViewCommand { get; set; }
        public ICommand ResetToDefaultRectangleScaleCommand { get; set; }
        public ICommand AglignSelectedItemstoLeftCommand { get; set; }
        public ICommand SpreadItemLeftHorizontalCommand { get; set; }
        public ICommand SpreadItemRightHorizontalCommand { get; set; }

        public ICommand SpreadItemUpVerticalCommand { get; set; }
        public ICommand SpreadItemDownVerticalCommand { get; set; }
        public ICommand AglignSelectedItemstoTopCommand { get; set; }
        public ICommand ExportCurrentColorEffectCommand { get; set; }
        public ICommand AddSelectedItemToGroupCommand { get; set; }
        public ICommand ExitCurrentRunningAppCommand { get; set; }
        public ICommand ExecuteAutomationFromManagerCommand { get; set; }
        public ICommand UpdateAppCommand { get; set; }
        public ICommand OpenLEDSteupSelectionWindowsCommand { get; set; }
        public ICommand SetCurrentSelectedOutputForCurrentSelectedOutputCommand { get; set; }
        public ICommand OpenFanSpeedPlotWindowsCommand { get; set; }
        public ICommand OpenAvailableUpdateListWindowCommand { get; set; }
        public ICommand OpenExportNewColorEffectCommand { get; set; }
        public ICommand ResetAppCommand { get; set; }
        public ICommand ResetDefaultAspectRatioCommand { get; set; }
        public ICommand NextOutputCommand { get; set; }
        public ICommand PreviousOutputCommand { get; set; }
        public ICommand SetSelectedSpotIDLeftToRightCommand { get; set; }
        public ICommand SetSelectedSpotIDRightToLeftCommand { get; set; }
        public ICommand SetSelectedSpotIDBotToTopCommand { get; set; }
        public ICommand SetSelectedSpotIDTopToBotCommand { get; set; }
        public ICommand SetSelectedSpotIDNeutralCommand { get; set; }
        public ICommand SetSelectedSpotIDNeutralReverseCommand { get; set; }
        public ICommand ApplySpotImportDataCommand { get; set; }
        public ICommand ApplyOutputImportDataCommand { get; set; }
        public ICommand OpenSpotMapWindowCommand { get; set; }
        public ICommand OpenAboutWindowCommand { get; set; }
        public ICommand OpenSurfaceEditorWindowCommand { get; set; }
        public ICommand OpenPasswordDialogCommand { get; set; }
        public ICommand OpenAppSettingsWindowCommand { get; set; }
        public ICommand SetAllDeviceSelectedGradientColorCommand { get; set; }
        public ICommand SetAllOutputRectangleSizeCommand { get; set; }
        public ICommand SelecFirmwareForCurrentDeviceCommand { get; set; }
        public ICommand SetCurrentLEDSetupSentryColorCommand { get; set; }
        public ICommand DeleteSelectedGradientCommand { get; set; }
        public ICommand SetAllOutputSelectedGradientColorCommand { get; set; }
        public ICommand UpdateCurrentSelectedDeviceFirmwareCommand { get; set; }
        public ICommand ZerolAllCommand { get; set; }
        public ICommand SetActivePaletteAllOutputsCommand { get; set; }
        public ICommand SetAllOutputSelectedModeCommand { get; set; }
        public ICommand SetAllOutputSelectedSolidColorCommand { get; set; }
        public ICommand SetAllDeviceSelectedSolidColorCommand { get; set; }
        public ICommand SetAllDeviceSelectedModeCommand { get; set; }
        public ICommand SetAllOutputSelectedGifCommand { get; set; }
        public ICommand SetAllDeviceSelectedGifCommand { get; set; }
        public ICommand SetActivePaletteAllDevicesCommand { get; set; }
        public ICommand ShowBrightnessAdjustmentPopupCommand { get; set; }
        public ICommand SendCurrentDeviceSpeedCommand { get; set; }
        public ICommand LaunchDeleteSelectedDeviceWindowCommand { get; set; }
        public ICommand DeleteSelectedDeviceCommand { get; set; }
        public ICommand DeleteSelectedDevicesCommand { get; set; }
        public ICommand OpenFFTPickerWindowCommand { get; set; }
        public ICommand ScanSerialDeviceCommand { get; set; }
        public ICommand ScanOpenRGBDeviceCommand { get; set; }
        public ICommand SaveAllAutomationCommand { get; set; }
        public ICommand SaveCurrentSelectedAutomationShortkeyCommand { get; set; }
        public ICommand AddSelectedActionTypeToListCommand { get; set; }
        public ICommand DeleteSelectedActionFromListCommand { get; set; }
        public ICommand OpenAddNewAutomationCommand { get; set; }
        public ICommand OpenActionsManagerWindowCommand { get; set; }
        public ICommand OpenHotKeySelectionWindowCommand { get; set; }
        public ICommand OpenTargetDeviceSelectionWindowCommand { get; set; }
        public ICommand OpenTargetParamSelectionWindowCommand { get; set; }
        public ICommand OpenTargetActionSelectionWindowCommand { get; set; }
        public ICommand OpenAutomationValuePickerWindowCommand { get; set; }
        public ICommand OpenActionsEditWindowCommand { get; set; }
        public ICommand OpenAutomationManagerWindowCommand { get; set; }
        public ICommand OpenAmbinoStoreWindowCommand { get; set; }
        public ICommand SetCurrentActionTypeForSelectedActionCommand { get; set; }
        public ICommand SetCurrentActionTargetDeviceForSelectedActionCommand { get; set; }
        public ICommand SetCurrentActionParamForSelectedActionCommand { get; set; }
        public ICommand SetCurrentSelectedActionTypeColorValueCommand { get; set; }
        public ICommand OpenHardwareMonitorWindowCommand { get; set; }
        public ICommand SaveCurrentSelectedAutomationCommand { get; set; }

        public ICommand CoppyColorCodeCommand { get; set; }
        public ICommand DeleteSelectedSolidColorCommand { get; set; }
        public ICommand DeleteSelectedAutomationCommand { get; set; }
        public ICommand AddNewGradientCommand { get; set; }
        public ICommand SaveNewGradientCommand { get; set; }
        public ICommand ApplyCurrentOutputCapturingPositionCommand { get; set; }
        public ICommand LaunchWBAdjustWindowCommand { get; set; }
        public ICommand ExportCurrentProfileCommand { get; set; }
        public ICommand ImportProfileCommand { get; set; }
        public ICommand SaveCurrentProfileCommand { get; set; }
        public ICommand DeleteAttachedProfileCommand { get; set; }
        public ICommand SaveCurrentSelectedActionCommand { get; set; }
        public ICommand CreateNewProfileCommand { get; set; }
        public ICommand OpenProfileCreateCommand { get; set; }
        public ICommand ExportPIDCommand { get; set; }
        public ICommand ImportLEDSetupCommand { get; set; }
        public ICommand UnZoneItemsCommand { get; set; }
        public ICommand AddNewZoneCommand { get; set; }
        public ICommand OpenDeviceConnectionSettingsWindowCommand { get; set; }
        public ICommand OpenDeviceFirmwareSettingsWindowCommand { get; set; }
        public ICommand RenameSelectedItemCommand { get; set; }
        public ICommand SetSelectedItemScaleFactorCommand { get; set; }
        public ICommand SetSelectedItemRotateFactorCommand { get; set; }
        public ICommand OpenAdvanceSettingWindowCommand { get; set; }
        public ICommand CancelEditWizardCommand { get; set; }
        public ICommand ShowNameEditWindow { get; set; }
        public ICommand SetBorderSpotActiveCommand { get; set; }
        public ICommand SaveNewUserEditLEDSetup { get; set; }
        public ICommand ResetCountCommand { get; set; }
        public ICommand SetSpotPIDCommand { get; set; }
        public ICommand SetSpotVIDCommand { get; set; }
        public ICommand SetSelectedSpotCIDCommand { get; set; }
        public ICommand ResetMaxCountCommand { get; set; }
        public ICommand SetPIDNeutral { get; set; }
        public ICommand SetCurrentSelectedVID { get; set; }
        public ICommand SetCurrentSelectedVIDRange { get; set; }
        public ICommand SetPIDReverseNeutral { get; set; }
        public ICommand JumpToNextWizardStateCommand { get; set; }
        public ICommand JumpToNextAddDeviceWizardStateCommand { get; set; }
        public ICommand AddCurrentSelectedDeviceToDashboard { get; set; }
        public ICommand BackToPreviousAddDeviceWizardStateCommand { get; set; }
        public ICommand BackToPreviousWizardStateCommand { get; set; }
        public ICommand LaunchPositionEditWindowCommand { get; set; }
        public ICommand LaunchPIDEditWindowCommand { get; set; }
        public ICommand LaunchCompositionEditWindowCommand { get; set; }
        public ICommand LaunchVIDEditWindowCommand { get; set; }
        public ICommand LaunchMIDEditWindowCommand { get; set; }
        public ICommand LaunchCIDEditWindowCommand { get; set; }
        public ICommand EditSelectedPaletteCommand { get; set; }
        public ICommand AddNewSolidColorCommand { get; set; }
        public ICommand AddPickedSolidColorCommand { get; set; }
        public ICommand ImportPaletteCardFromFileCommand { get; set; }
        public ICommand ImportedGifFromFileCommand { get; set; }
        public ICommand ExportCurrentSelectedPaletteToFileCommand { get; set; }
        public ICommand EditSelectedPaletteSaveConfirmCommand { get; set; }
        public ICommand DeleteSelectedPaletteCommand { get; set; }
        public ICommand UploadSelectedPaletteCommand { get; set; }
        public ICommand DeleteSelectedGifCommand { get; set; }
        public ICommand CreateNewPaletteCommand { get; set; }
        public ICommand RequestingBetaChanelCommand { get; set; }
        public ICommand CreateNewAutomationCommand { get; set; }
        public ICommand SetIncreamentCommand { get; set; }
        public ICommand SetIncreamentCommandfromZero { get; set; }
        public ICommand UserInputIncreamentCommand { get; set; }

        public ICommand ImportEffectCommand { get; set; }
        public ICommand ChangeCurrentDeviceSelectedPalette { get; set; }
        public ICommand SelectMenuItem { get; set; }
        public ICommand BackCommand { get; set; }
        public ICommand DeviceRectDropCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand SetCustomColorCommand { get; set; }

        public ICommand AdjustPositionCommand { get; set; }
        public ICommand SnapshotCommand { get; set; }

        public ICommand SetSpotActiveCommand { get; set; }
        public ICommand SetAllSpotActiveCommand { get; set; }
        public ICommand SetZoneColorCommand { get; set; }
        public ICommand GetZoneColorCommand { get; set; }

        #endregion property

        private ObservableCollection<IDeviceSettings> _availableDevices;

        public ObservableCollection<IDeviceSettings> AvailableDevices {
            get { return _availableDevices; }
            set => Set(ref _availableDevices, value);
        }
        private ObservableCollection<object> _automationParamList;

        public ObservableCollection<object> AutomationParamList {
            get { return _automationParamList; }
            set => Set(ref _automationParamList, value);
        }
        private string _paramType;

        public string ParamType {
            get { return _paramType; }
            set => Set(ref _paramType, value);
        }

        private ObservableCollection<WLEDDevice> _availableWLEDDevices;

        public ObservableCollection<WLEDDevice> AvailableWLEDDevices {
            get { return _availableWLEDDevices; }
            set
            {
                if (_availableWLEDDevices == value) return;
                _availableWLEDDevices = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<IDeviceSettings> _availableOpenRGBDevices;

        public ObservableCollection<IDeviceSettings> AvailableOpenRGBDevices {
            get { return _availableOpenRGBDevices; }
            set
            {
                if (_availableOpenRGBDevices == value) return;
                _availableOpenRGBDevices = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<IDeviceSettings> _availableSerialDevices;

        public ObservableCollection<IDeviceSettings> AvailableSerialDevices {
            get { return _availableSerialDevices; }
            set
            {
                if (_availableSerialDevices == value) return;
                _availableSerialDevices = value;
                RaisePropertyChanged();
            }
        }

        private List<IOutputSettings> _availableOutputForSelectedDevice;

        public List<IOutputSettings> AvailableOutputForSelectedDevice {
            get { return _availableOutputForSelectedDevice; }
            set
            {
                if (_availableOutputForSelectedDevice == value) return;
                _availableOutputForSelectedDevice = value;
                RaisePropertyChanged();
            }
        }
        private IOutputSettings _selectedOutputForCurrentDevice;

        public IOutputSettings SelectedOutputForCurrentDevice {
            get { return _selectedOutputForCurrentDevice; }
            set
            {
                if (_selectedOutputForCurrentDevice == value) return;
                _selectedOutputForCurrentDevice = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<WLEDDevice> _selectedWLEDDevice;

        public ObservableCollection<WLEDDevice> SelectedWLEDDevice {
            get { return _selectedWLEDDevice; }
            set { _selectedWLEDDevice = value; }
        }

        private ObservableCollection<IDeviceProfile> _availableProfiles;

        public ObservableCollection<IDeviceProfile> AvailableProfiles {
            get { return _availableProfiles; }
            set
            {
                if (_availableProfiles == value) return;
                _availableProfiles = value;

                RaisePropertyChanged();
            }
        }

        private ObservableCollection<IDeviceFirmware> _availableFirmwareForCurrentDevice;

        public ObservableCollection<IDeviceFirmware> AvailableFirmwareForCurrentDevice {
            get { return _availableFirmwareForCurrentDevice; }
            set
            {
                if (_availableFirmwareForCurrentDevice == value) return;
                _availableFirmwareForCurrentDevice = value;

                RaisePropertyChanged();
            }
        }

        private ObservableCollection<IAutomationSettings> _availableAutomations;

        public ObservableCollection<IAutomationSettings> AvailableAutomations {
            get { return _availableAutomations; }
            set
            {
                if (_availableAutomations == value) return;
                _availableAutomations = value;

                RaisePropertyChanged();
            }
        }

        private IComputer _thisComputer;

        public IComputer ThisComputer {
            get { return _thisComputer; }
            set
            {
                _thisComputer = value;
                RaisePropertyChanged();
            }
        }

        public void RefreshHardwareStatus()
        {
            RaisePropertyChanged(nameof(ThisComputer));
        }

        private ObservableCollection<IDeviceProfile> _availableProfilesForCurrentDevice;

        public ObservableCollection<IDeviceProfile> AvailableProfilesForCurrentDevice {
            get { return _availableProfilesForCurrentDevice; }
            set
            {
                _availableProfilesForCurrentDevice = value;
                RaisePropertyChanged();
            }
        }

        public IDeviceProfile CurrentSelectedProfile {
            get { return AvailableProfilesForCurrentDevice.Where(p => p.ProfileUID == CurrentDevice.ActivatedProfileUID).FirstOrDefault(); }
            set
            {
                if (value != null)
                {
                    //_currentSelectedProfile = value;
                    if (value.ProfileUID != CurrentDevice.ActivatedProfileUID)// change profile
                        LoadProfile(value);
                    RaisePropertyChanged();
                }

                //RaisePropertyChanged(nameof(CurrentDevice.AvailableOutputs));
            }
        }

        private ObservableCollection<IDeviceSettings> _displayCards;

        public ObservableCollection<IDeviceSettings> DisplayCards {
            get { return _displayCards; }
            set
            {
                if (_displayCards == value) return;
                _displayCards = value;
                RaisePropertyChanged();
            }
        }




        public ICommand SelectCardCommand { get; set; }
        public ICommand SelectOnlineItemCommand { get; set; }
        public ICommand LightingModeSelection { get; set; }
        public ICommand ShowAddNewCommand { get; set; }
        public ICommand RefreshDeviceCommand { get; set; }

        private string JsonDeviceNameAndPath => Path.Combine(JsonPath, "adrilight-deviceInfos.json");
        public IList<String> _AvailableComPorts;

        public IList<String> AvailableComPorts {
            get
            {
                _AvailableComPorts = SerialPort.GetPortNames().Concat(new[] { "Không có" }).ToList();
                _AvailableComPorts.Remove("COM1");

                return _AvailableComPorts;
            }
        }
        public IList<double> _availableMotionSpeed;

        public IList<double> AvailableMotionSpeed {
            get
            {
                return _availableMotionSpeed;
            }
            set
            {
                _availableMotionSpeed = value;
                RaisePropertyChanged();
            }
        }
        public IList<string> _availableMotionDirrection;

        public IList<string> AvailableMotionDirrection {
            get
            {
                return _availableMotionDirrection;
            }
            set
            {
                _availableMotionDirrection = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<string> _caseEffects;

        public ObservableCollection<string> CaseEffects {
            get { return _caseEffects; }
            set
            {
                if (_caseEffects == value) return;
                _caseEffects = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<CollectionItem> _collectionItm;

        public ObservableCollection<CollectionItem> CollectionItems {
            get { return _collectionItm; }
            set
            {
                if (_collectionItm == value) return;
                _collectionItm = value;
                RaisePropertyChanged();
            }
        }



        private ObservableCollection<IColorPalette> _availablePallete;

        public ObservableCollection<IColorPalette> AvailablePallete {
            get { return _availablePallete; }
            set
            {
                if (_availablePallete == value) return;
                _availablePallete = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<IGifCard> _availableGifs;

        public ObservableCollection<IGifCard> AvailableGifs {
            get { return _availableGifs; }
            set
            {
                if (_availableGifs == value) return;
                _availableGifs = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<IGradientColorCard> _availableGradient;

        public ObservableCollection<IGradientColorCard> AvailableGradient {
            get { return _availableGradient; }
            set
            {
                if (_availableGradient == value) return;
                _availableGradient = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<Color> _availableSolidColors;

        public ObservableCollection<Color> AvailableSolidColors {
            get { return _availableSolidColors; }
            set
            {
                if (_availableSolidColors == value) return;
                _availableSolidColors = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<string> _availableLayout;

        public ObservableCollection<string> AvailableLayout {
            get { return _availableLayout; }
            set
            {
                if (_availableLayout == value) return;
                _availableLayout = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<string> _availableRotation;

        public ObservableCollection<string> AvailableRotation {
            get { return _availableRotation; }
            set
            {
                if (_availableRotation == value) return;
                _availableRotation = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<string> _availableMatrixOrientation;

        public ObservableCollection<string> AvailableMatrixOrientation {
            get { return _availableMatrixOrientation; }
            set
            {
                if (_availableMatrixOrientation == value) return;
                _availableMatrixOrientation = value;
                RaisePropertyChanged();
            }
        }



        private ObservableCollection<string> _availableMatrixStartPoint;

        public ObservableCollection<string> AvailableMatrixStartPoint {
            get { return _availableMatrixStartPoint; }
            set
            {
                if (_availableMatrixStartPoint == value) return;
                _availableMatrixStartPoint = value;
                RaisePropertyChanged();
            }
        }

        private IDeviceSpot[] _previewSpots;

        public IDeviceSpot[] PreviewSpots {
            get => _previewSpots;
            set
            {
                _previewSpots = value;
                RaisePropertyChanged();
            }
        }

        private ILEDSetup _currentLEDSetup;

        public ILEDSetup CurrentLEDSetup {
            get => _currentLEDSetup;
            set
            {
                _currentLEDSetup = value;
                RaisePropertyChanged();
            }
        }

        public WriteableBitmap _shaderBitmap;

        public WriteableBitmap ShaderBitmap {
            get => _shaderBitmap;
            set
            {
                _shaderBitmap = value;
                RaisePropertyChanged(nameof(ShaderBitmap));


            }
        }

        public WriteableBitmap _greyBitmap;

        public WriteableBitmap GreyBitmap {
            get => _greyBitmap;
            set
            {
                _greyBitmap = value;
                RaisePropertyChanged();
            }
        }

        private SeriesCollection _fanControlView;

        public SeriesCollection FanControlView {
            get => _fanControlView;
            set
            {
                _fanControlView = value;
                RaisePropertyChanged();
            }
        }

        public WriteableBitmap _gifxelationBitmap;

        public WriteableBitmap GifxelationBitmap {
            get => _gifxelationBitmap;
            set
            {
                _gifxelationBitmap = value;
                RaisePropertyChanged(nameof(GifxelationBitmap));

            }
        }

        private IVisualizerProgressBar[] _visualizerFFT;

        public IVisualizerProgressBar[] VisualizerFFT {
            get { return _visualizerFFT; }
            set
            {
                _visualizerFFT = value;
                RaisePropertyChanged(nameof(VisualizerFFT));
            }
        }

        private int _selectedDeviceCount;

        public int SelectedDeviceCount {
            get { return _selectedDeviceCount; }
            set
            {
                _selectedDeviceCount = value;
                RaisePropertyChanged(nameof(_selectedDeviceCount));
            }
        }
        private bool _toolbarVisible;

        public bool ToolbarVisible {
            get { return _toolbarVisible; }
            set
            {
                _toolbarVisible = value;
                RaisePropertyChanged(nameof(ToolbarVisible));
            }
        }


        private int _parrentLocation;

        public int ParrentLocation {
            get => _parrentLocation;
            set
            {
                _parrentLocation = value;
                RaisePropertyChanged();
            }
        }

        private string _setIDMode;

        public string SetIDMode {
            get => _setIDMode;
            set
            {
                _setIDMode = value;
                RaisePropertyChanged();
            }
        }

        private string _pickColorMode;

        public string PickColorMode {
            get => _pickColorMode;
            set
            {
                _pickColorMode = value;
                RaisePropertyChanged();
            }
        }

        public int CurrentSelectedCustomColorIndex {
            set
            {
                if (value >= 0)
                {
                    SetCustomColor(value);
                }
            }
        }

        //public int DeviceSpotX {
        //    get
        //    {
        //          CurrentDevice.SpotsX
        //    }
        //    set
        //    {
        //        if(CurrentDevice.SpotsY)
        //        CurrentDevice.SpotsX = value;
        //    }

        //}

        //public int DeviceSpotY {
        //    get => _deviceSpotY;
        //    set => _deviceSpotY = value;

        //}



        public IList<String> AvailableAudioDevice {
            get
            {

                var audioDevices = AudioFrame.AvailableAudioDevice;

                return audioDevices;

            }
        }


        private bool _deviceLightingModeCollection;

        public bool DeviceLightingModeCollection {
            get { return _deviceLightingModeCollection; }
            set { _deviceLightingModeCollection = value; }
        }

        private ObservableCollection<IDeviceCatergory> _availableDeviceCatergoryToAdd;

        public ObservableCollection<IDeviceCatergory> AvailableDeviceCatergoryToAdd { get => _availableDeviceCatergoryToAdd; set => Set(ref _availableDeviceCatergoryToAdd, value); }
        private IDeviceCatergory _currentSelectedCatergoryToAdd;

        public IDeviceCatergory CurrentSelectedCatergoryToAdd { get => _currentSelectedCatergoryToAdd; set => Set(ref _currentSelectedCatergoryToAdd, value); }

        private IDeviceSettings _currentSelectedDeviceToAdd;

        public IDeviceSettings CurrentSelectedDeviceToAdd { get => _currentSelectedDeviceToAdd; set => Set(ref _currentSelectedDeviceToAdd, value); }

        public ObservableCollection<string> AvailablePalette { get; private set; }
        public IContext Context { get; }
        private List<string> _availableDisplays;

        public List<string> AvailableDisplays {
            get
            {
                var listDisplay = new List<string>();
                foreach (var screen in System.Windows.Forms.Screen.AllScreens)
                {
                    listDisplay.Add(screen.DeviceName);
                }
                _availableDisplays = listDisplay;
                return _availableDisplays;
            }
        }





        public ObservableCollection<string> AvailableMenu { get; private set; }
        public ICommand SelectGif { get; set; }
        public BitmapImage gifimage;
        public Stream gifStreamSource;
        private BitmapSource _contentBitmap;

        public BitmapSource ContentBitmap {
            get { return _contentBitmap; }
            set
            {
                if (value != _contentBitmap)
                {
                    _contentBitmap = value;
                    RaisePropertyChanged(() => ContentBitmap);
                }
            }
        }
        public ResourceHelpers ResourceHlprs { get; private set; }
        public IGeneralSettings GeneralSettings { get; }

        public ISerialStream[] SerialStreams { get; }
        public IAmbinityClient AmbinityClient { get; set; }
        public IAudioFrame AudioFrame { get; set; }
        public ISerialDeviceDetection SerialDeviceDetection { get; set; }
        //public static IShaderEffect ShaderEffect { get; set; }

        private static ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();

        public MainViewViewModel(IContext context,
            IDeviceSettings[] devices,
            IGeneralSettings generalSettings,
            IAmbinityClient ambinityClient,
            ISerialDeviceDetection serialDeviceDetection,
            IAudioFrame audioFrame,
            ISerialStream[] serialStreams

           )
        {
            GeneralSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));
            SerialStreams = serialStreams ?? throw new ArgumentNullException(nameof(serialStreams));
            AvailableDevices = new ObservableCollection<IDeviceSettings>();
            AudioFrame = audioFrame ?? throw new ArgumentNullException(nameof(audioFrame));
            DisplayCards = new ObservableCollection<IDeviceSettings>();
            Context = context ?? throw new ArgumentNullException(nameof(context));
            AmbinityClient = ambinityClient ?? throw new ArgumentNullException(nameof(ambinityClient));
            SerialDeviceDetection = serialDeviceDetection ?? throw new ArgumentNullException(nameof(serialDeviceDetection));


            var settingsManager = new UserSettingsManager();
            foreach (IDeviceSettings device in devices)
            {
                device.PropertyChanged += (_, __) => WriteSingleDeviceInfoJson(device);
                AvailableDevices.Add(device);
            }



            AvailableAutomations = new ObservableCollection<IAutomationSettings>();
            foreach (var automation in LoadAutomationIfExist())
            {
                AvailableAutomations.Add(automation);
            }
            WriteAutomationCollectionJson();

            //register hotkey from loaded automation//


            //dummy device acts as add new button
            var addNewButton = new DeviceSettings {
                IsDummy = true
            };
            AvailableDevices.Add(addNewButton);

            GeneralSettings.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(GeneralSettings.ThemeIndex):
                        if (GeneralSettings.ThemeIndex == 0)
                            ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                        else
                        {
                            ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                        }
                        break;

                    case nameof(GeneralSettings.Autostart):
                        if (GeneralSettings.Autostart)
                        {
                            StartUpManager.AddApplicationToTaskScheduler(GeneralSettings.StartupDelaySecond);
                        }
                        else
                        {
                            StartUpManager.RemoveApplicationFromTaskScheduler("Ambinity Service");
                        }
                        break;
                    case nameof(GeneralSettings.StartupDelaySecond):
                        if (GeneralSettings.Autostart)
                        {
                            StartUpManager.AddApplicationToTaskScheduler(GeneralSettings.StartupDelaySecond);
                        }
                        else
                        {
                            StartUpManager.RemoveApplicationFromTaskScheduler("Ambinity Service");
                        }
                        break;

                    case nameof(GeneralSettings.HotkeyEnable):
                        if (GeneralSettings.HotkeyEnable)
                        {
                            KeyboardHookManagerSingleton.Instance.Start();
                            Register();
                        }
                        else
                        {
                            KeyboardHookManagerSingleton.Instance.Stop();
                            Unregister();
                        }
                        break;

                    default:
                        break;
                }
            };
            CreateFWToolsFolderAndFiles();

        }

        public void SetGifxelationPreviewImage(ByteFrame frame)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {
                if (GifxelationBitmap == null)
                {
                    GifxelationBitmap = new WriteableBitmap(frame.FrameWidth, frame.FrameHeight, 96, 96, PixelFormats.Bgra32, null);
                }
                else
                {
                    if (frame != null)
                    {
                        GifxelationBitmap.Lock();
                        IntPtr pixelAddress = GifxelationBitmap.BackBuffer;
                        Marshal.Copy(frame.Frame, 0, pixelAddress, frame.Frame.Length);

                        GifxelationBitmap.AddDirtyRect(new Int32Rect(0, 0, frame.FrameWidth, frame.FrameHeight));

                        GifxelationBitmap.Unlock();
                        //ShaderBitmap = MatrixBitmap;
                        //RaisePropertyChanged(() => DeviceRectWidthMax);
                        //RaisePropertyChanged(() => DeviceRectHeightMax);
                    }
                    else
                    {
                        //notify the UI show error message
                    }

                }
            });
        }
        private void WriteDeviceInfo(IDeviceSettings device)
        {
            // create directory based on device name and UID
            //check if device collection folder path is exist
            if (!Directory.Exists(DevicesCollectionFolderPath))
            {
                Directory.CreateDirectory(DevicesCollectionFolderPath);
            }
            var directory = Path.Combine(DevicesCollectionFolderPath, device.DeviceName + "-" + device.DeviceUID);
            Directory.CreateDirectory(directory);
            // write thumbnail
            //if this device is ambino legacy, search resource fo the thumbnail
            ResourceHlprs.CopyResource("adrilight.Resources.Thumbnails.ambino_basic_thumb.png", Path.Combine(directory, "thumbnail.png"));
            //set thumbnail
            device.DeviceThumbnail = Path.Combine(directory, "thumbnail.png");
            // finally write infojson
            WriteSingleDeviceInfoJson(device);




        }

        public void WriteSingleDeviceInfoJson(IDeviceSettings device)
        {
            var directory = Path.Combine(DevicesCollectionFolderPath, device.DeviceName + "-" + device.DeviceUID);
            var fileToWrite = Path.Combine(directory, "config.json");
            var json = JsonConvert.SerializeObject(device, new JsonSerializerSettings() {
                TypeNameHandling = TypeNameHandling.Auto
            });

            // Set Status to Locked
            _readWriteLock.EnterWriteLock();
            try
            {
                // Append text to the file
                using (StreamWriter sw = new StreamWriter(fileToWrite))
                {
                    sw.Write(json);
                    sw.Close();
                }
            }
            finally
            {
                // Release lock
                _readWriteLock.ExitWriteLock();
            }
        }
        public void FoundNewDevice(List<IDeviceSettings> newSerialDevices, List<IDeviceSettings> newOpenRGBDevices)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {

                if (newSerialDevices != null && newSerialDevices.Count > 0)
                {


                    foreach (var serialDevice in newSerialDevices)
                    {
                        //add available mode
                        foreach (var controller in serialDevice.AvailableControllers)
                        {

                            //in the future, if you want to disable some of the mode for specific device, just dont add to the list
                            //add controlable property, for fan hub, add speed control
                            //better implement a switch case
                            foreach (var output in controller.Outputs.Where(o => o.GetType() == typeof(LightingOutput)))
                            {
                                foreach (var zone in (output as LightingOutput).SlaveDevice.ControlableZones)
                                {
                                    zone.AvailableControlMode = new List<IControlMode>() { ColorPalette, MusicReactive };
                                }
                            }

                            foreach (var output in controller.Outputs.Where(o => o.GetType() == typeof(PWMOutput)))
                            {
                                foreach (var zone in (output as PWMOutput).SlaveDevice.ControlableZones)
                                {
                                    zone.AvailableControlMode = new List<IControlMode>() { FanSpeedAuto, FanSpeedManual };
                                }
                            }

                            //example of adding speed mode
                            //output.ControlableProperties.Add(new OutputControlableProperty() {
                            //    Name = "Speed",
                            //    Description = "Speed Control for Current Output",
                            //    Icon = "fanspeed",
                            //    Type = OutputControlablePropertyEnum.Speed,
                            //    AvailableControlMode = new List<IControlMode>() { FanSpeedAuto, FanSpeedManual },
                            //    CurrentActiveControlModeIndex = 0 // change this to change deffault lightingmode

                            //});
                        }
                        WriteDeviceInfo(serialDevice);
                        AvailableDevices.Insert(0, serialDevice);


                    }
                    if (newOpenRGBDevices != null)
                    {
                        foreach (var openRGBDevice in newOpenRGBDevices)
                        {
                            if (!AvailableDevices.Any(p => p.DeviceUID == openRGBDevice.DeviceUID)) // if device is already existed in the dashboard
                            {
                                AvailableDevices.Insert(0, openRGBDevice);
                            }
                        }
                    }

                    //WriteDeviceInfoJson();
                }
            });

        }
        public void OldDeviceReconnected(List<IDeviceSettings> oldSerialDevice, List<IDeviceSettings> oldOpenRGBDevices)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {

                if (oldSerialDevice != null && oldSerialDevice.Count > 0)
                {


                    foreach (var serialDevice in oldSerialDevice)
                    {
                        //set first device found active again since it's recconected

                        //var oldDevice = AvailableDevices.Where(p => p.OutputPort == serialDevice.OutputPort).FirstOrDefault();
                        //if (oldDevice.IsTransferActive == true)
                        //{
                        //    //try to poke it
                        //    oldDevice.IsTransferActive = false;
                        //}
                        //oldDevice.IsTransferActive = true;
                        //RaisePropertyChanged(nameof(oldDevice.IsTransferActive));

                        //restart it's serialstram by 
                        //setting the transferactive to true, no matter it;s active or not
                        // WriteSingleDeviceInfoJson(oldDevice);
                    }
                    //if (oldOpenRGBDevices != null)
                    //{
                    //    foreach (var openRGBDevice in oldOpenRGBDevices)
                    //    {
                    //        if (!AvailableDevices.Any(p => p.DeviceUID == openRGBDevice.DeviceUID)) // if device is already existed in the dashboard
                    //        {
                    //            AvailableDevices.Insert(0, openRGBDevice);
                    //        }
                    //    }
                    //}


                }
            });

        }
        public void ShaderImageUpdate(ByteFrame frame)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {
                if (ShaderBitmap == null)
                {
                    ShaderBitmap = new WriteableBitmap(frame.FrameWidth, frame.FrameHeight, 96, 96, PixelFormats.Bgra32, null);
                }
                else if (ShaderBitmap != null && (ShaderBitmap.Width != frame.FrameWidth || ShaderBitmap.Height != frame.FrameHeight))
                {
                    ShaderBitmap = new WriteableBitmap(frame.FrameWidth, frame.FrameHeight, 96, 96, PixelFormats.Bgra32, null);
                }
                else if (ShaderBitmap != null && ShaderBitmap.Width == frame.FrameWidth && ShaderBitmap.Height == frame.FrameHeight)
                {
                    if (frame != null)
                    {
                        ShaderBitmap.Lock();
                        IntPtr pixelAddress = ShaderBitmap.BackBuffer;
                        Marshal.Copy(frame.Frame, 0, pixelAddress, frame.Frame.Length);

                        ShaderBitmap.AddDirtyRect(new Int32Rect(0, 0, frame.FrameWidth, frame.FrameHeight));

                        ShaderBitmap.Unlock();
                        //ShaderBitmap = MatrixBitmap;
                        //RaisePropertyChanged(() => DeviceRectWidthMax);
                        //RaisePropertyChanged(() => DeviceRectHeightMax);
                    }
                    else
                    {
                        //notify the UI show error message
                    }

                }

            });
        }

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        public void SetPreviewImage(byte[] frame)
        {
            Context.Invoke(() =>
            {
                PreviewImageSource = new WriteableBitmap(120, 120, 96, 96, PixelFormats.Bgra32, null);
                PreviewImageSource.Lock();
                IntPtr pixelAddress = PreviewImageSource.BackBuffer;
                var CurrentFrame = frame;

                Marshal.Copy(CurrentFrame, 0, pixelAddress, CurrentFrame.Length);

                PreviewImageSource.AddDirtyRect(new Int32Rect(0, 0, 120, 120));
                RaisePropertyChanged(nameof(PreviewImageSource));

                PreviewImageSource.Unlock();
                // ShaderBitmap = MatrixBitmap;
            });
        }

        private WriteableBitmap _previewImageSource;

        public WriteableBitmap PreviewImageSource {
            get => _previewImageSource;
            set
            {
                // _log.Info("PreviewImageSource created.");
                Set(ref _previewImageSource, value);
                RaisePropertyChanged();
            }
        }

        private System.Windows.Media.Color _currentPickedColor;

        public System.Windows.Media.Color CurrentPickedColor {
            get => _currentPickedColor;
            set
            {
                // _log.Info("PreviewImageSource created.");
                Set(ref _currentPickedColor, value);
                RaisePropertyChanged();
            }
        }

        private int _currentLEDEditWizardState = 0;

        public int CurrentLEDEditWizardState {
            get => _currentLEDEditWizardState;
            set
            {
                // _log.Info("PreviewImageSource created.");
                Set(ref _currentLEDEditWizardState, value);
                RaisePropertyChanged();
            }
        }

        private int _currentAddDeviceWizardState = 0;

        public int CurrentAddDeviceWizardState {
            get => _currentAddDeviceWizardState;
            set
            {
                // _log.Info("PreviewImageSource created.");
                Set(ref _currentAddDeviceWizardState, value);
                RaisePropertyChanged();
            }
        }

        private IDeviceSpot[] _bufferSpots;

        public IDeviceSpot[] BufferSpots {
            get => _bufferSpots;
            set
            {
                // _log.Info("PreviewImageSource created.");
                Set(ref _bufferSpots, value);
                RaisePropertyChanged();
            }
        }
        private string _commonDialogQuestion;
        public string CommonDialogQuestion {
            get { return _commonDialogQuestion; }
            set
            {
                _commonDialogQuestion = value;
            }
        }


        private int _count = 0;

        public int Count {
            get => _count;
            set
            {
                Set(ref _count, value);
                RaisePropertyChanged(nameof(IsNextable));
            }
        }

        private int _countVID = 0;

        public int CountVID {
            get => _countVID;
            set
            {
                Set(ref _countVID, value);
                RaisePropertyChanged(nameof(IsNextable));
            }
        }
        private int _countPID = 0;

        public int CountPID {
            get => _countPID;
            set
            {
                Set(ref _countPID, value);
                RaisePropertyChanged(nameof(IsNextable));
            }
        }
        private int _gapVID = 5;

        public int GapVID {
            get => _gapVID;
            set
            {
                Set(ref _gapVID, value);
                RaisePropertyChanged(nameof(IsNextable));
            }
        }

        private int _maxLEDCount = 0;

        public int MaxLEDCount {
            get => _maxLEDCount;
            set
            {
                _maxLEDCount = value;
                RaisePropertyChanged(nameof(MaxLEDCount));
                RaisePropertyChanged(nameof(IsNextable));
            }
        }

        private Thickness _sensitivityThickness;

        public Thickness SensitivityThickness {
            get => _sensitivityThickness;
            set
            {
                Set(ref _sensitivityThickness, value);
                RaisePropertyChanged();
            }
        }

        private bool _isRichCanvasWindowOpen = false;

        public bool IsRichCanvasWindowOpen {
            get => _isRichCanvasWindowOpen;
            set
            {
                Set(ref _isRichCanvasWindowOpen, value);
                RaisePropertyChanged();
                // _log.Info($"IsSettingsWindowOpen is now {_isSettingsWindowOpen}");
            }
        }

        private bool _isSecondDesktopValid;

        public bool IsSecondDesktopValid {
            get => _isSecondDesktopValid;
            set
            {
                Set(ref _isSecondDesktopValid, value);
                // _log.Info($"IsSettingsWindowOpen is now {_isSettingsWindowOpen}");
            }
        }

        private bool _isThirdDesktopValid;

        public bool IsThirdDesktopValid {
            get => _isThirdDesktopValid;
            set
            {
                Set(ref _isThirdDesktopValid, value);
                // _log.Info($"IsSettingsWindowOpen is now {_isSettingsWindowOpen}");
            }
        }

        private bool _isVisualizerWindowOpen;

        public bool IsVisualizerWindowOpen {
            get => _isVisualizerWindowOpen;
            set
            {
                Set(ref _isVisualizerWindowOpen, value);
                // _log.Info($"IsSettingsWindowOpen is now {_isSettingsWindowOpen}");
            }
        }

        private bool _isSplitLightingWindowOpen;

        public bool IsSplitLightingWindowOpen {
            get => _isSplitLightingWindowOpen;
            set
            {
                Set(ref _isSplitLightingWindowOpen, value);
                // _log.Info($"IsSettingsWindowOpen is now {_isSettingsWindowOpen}");
            }
        }



        private IAutomationSettings _currentSelectedAutomation;

        public IAutomationSettings CurrentSelectedAutomation {
            get => _currentSelectedAutomation;
            set
            {
                Set(ref _currentSelectedAutomation, value);
                // _log.Info($"IsSettingsWindowOpen is now {_isSettingsWindowOpen}");
            }
        }

        //private IColorPalette _currentActivePalette;

        //public IColorPalette CurrentActivePalette {
        //    get { return _currentActivePalette; }
        //    set
        //    {
        //        if (value != null)
        //        {
        //            _currentActivePalette = value;
        //            CurrentOutput.OutputCurrentActivePalette = value;
        //            RaisePropertyChanged(nameof(CurrentOutput.OutputCurrentActivePalette));
        //        }

        //        //SetCurrentDeviceSelectedPalette(value);
        //    }
        //}

        //private IGifCard _currentActiveGif;

        //public IGifCard CurrentActiveGif {
        //    get { return _currentActiveGif; }
        //    set
        //    {
        //        if (value != null)
        //        {
        //            _currentActiveGif = value;
        //            CurrentOutput.OutputSelectedGif = value;
        //        }

        //        //SetCurrentDeviceSelectedPalette(value);
        //    }
        //}

        private ObservableCollection<Color> _currentEditingColors;

        public ObservableCollection<Color> CurrentEditingColors {
            get { return _currentEditingColors; }
            set
            {
                _currentEditingColors = value;

                //SetCurrentDeviceSelectedPalette(value);
            }
        }

        private BadgeStatus _isSettingsUnsaved = BadgeStatus.Dot;

        public BadgeStatus IsSettingsUnsaved {
            get
            {
                return _isSettingsUnsaved;
            }

            set
            {
                _isSettingsUnsaved = value;

                RaisePropertyChanged();
            }
        }

        public bool _isSpeedSettingUnsetted = false;

        public bool IsSpeedSettingUnsetted {
            get
            {
                return _isSpeedSettingUnsetted;
            }

            set
            {
                _isSpeedSettingUnsetted = value;

                RaisePropertyChanged();
            }
        }

        private string _nameToChange;

        public string NameToChange {
            get
            {
                return _nameToChange;
            }

            set
            {
                _nameToChange = value;

                RaisePropertyChanged();
            }
        }

        private string _newProfileName = "New Profile";

        public string NewProfileName {
            get
            {
                return _newProfileName;
            }

            set
            {
                _newProfileName = value;

                RaisePropertyChanged();
            }
        }

        private string _newProfileOwner;

        public string NewProfileOwner {
            get
            {
                return _newProfileOwner;
            }

            set
            {
                _newProfileOwner = value;

                RaisePropertyChanged();
            }
        }

        private string _newProfileDescription;

        public string NewProfileDescription {
            get
            {
                return _newProfileDescription;
            }

            set
            {
                _newProfileDescription = value;

                RaisePropertyChanged();
            }
        }

        private List<IDeviceSpot> _activatedSpots;

        public List<IDeviceSpot> ActivatedSpots {
            get
            {
                return _activatedSpots;
            }

            set
            {
                _activatedSpots = value;

                RaisePropertyChanged();
            }
        }

        private List<IDeviceSpot> _backupSpots;

        public List<IDeviceSpot> BackupSpots {
            get
            {
                return _backupSpots;
            }

            set
            {
                _backupSpots = value;

                RaisePropertyChanged();
            }
        }

        private Color _selectedSolidColor;

        public Color SelectedSolidColor {
            get
            {
                return _selectedSolidColor;
            }

            set
            {
                _selectedSolidColor = value;
                //CurrentOutput.OutputStaticColor = value;

                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// new Palette
        /// </summary>
        private string _newPaletteName;

        public string NewPaletteName {
            get { return _newPaletteName; }
            set
            {
                _newPaletteName = value;
            }
        }

        private string _newEffectName;

        public string NewEffectName {
            get { return _newEffectName; }
            set
            {
                _newEffectName = value;
            }
        }

        private string _newPaletteOwner;

        public string NewPaletteOwner {
            get { return _newPaletteOwner; }
            set
            {
                _newPaletteOwner = value;
            }
        }

        private string _newEffectOwner;

        public string NewEffectOwner {
            get { return _newEffectOwner; }
            set
            {
                _newEffectOwner = value;
            }
        }

        private string _newPaletteDescription;

        public string NewPaletteDescription {
            get { return _newPaletteDescription; }
            set
            {
                _newPaletteDescription = value;
            }
        }

        private string _newEffectDescription;

        public string NewEffectDescription {
            get { return _newEffectDescription; }
            set
            {
                _newEffectDescription = value;
            }
        }

        /// <summary>
        ///
        /// new Automation
        /// </summary>
        private string _newAutomationName = "Shortcut";

        public string NewAutomationName {
            get { return _newAutomationName; }
            set
            {
                _newAutomationName = value;
            }
        }

        private int _rangeMinValue = 0;

        public int RangeMinValue {
            get { return _rangeMinValue; }
            set
            {
                _rangeMinValue = value;
            }
        }

        private int _iDMinValue = 0;

        public int IDMinValue {
            get { return _iDMinValue; }
            set
            {
                _iDMinValue = value;
            }
        }

        private int _rangeMaxValue = 100;

        public int RangeMaxValue {
            get { return _rangeMaxValue; }
            set
            {
                _rangeMaxValue = value;
            }
        }

        //public int IDMaxValue {
        //    get
        //    {
        //        int totalLEDNum = 0;
        //        foreach (var device in AvailableDevices)
        //        {
        //            if (!device.IsDummy)
        //            {
        //                foreach (var output in device.AvailableOutputs)
        //                {
        //                    totalLEDNum += output.OutputLEDSetup.Spots.Count;
        //                }
        //            }
        //        }

        //        return totalLEDNum;
        //    }
        //}


        private bool _outputModeChangeEnable;

        public bool OutputModeChangeEnable {
            get { return _outputModeChangeEnable; }
            set
            {
                _outputModeChangeEnable = value;
                RaisePropertyChanged();
            }
        }

        private IGradientColorCard _currentSelectedGradient;

        public IGradientColorCard CurrentSelectedGradient {
            get { return _currentSelectedGradient; }
            set
            {
                if (value != null)
                {
                    _currentSelectedGradient = value;
                    //CurrentOutput.OutputSelectedGradient = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Color _currentStartColor;

        public Color CurrentStartColor {
            get { return _currentStartColor; }
            set
            {
                _currentStartColor = value;
                RaisePropertyChanged();
            }
        }

        private bool[] _selectedSpotData = new bool[4];

        public bool[] SelectedSpotData {
            get { return _selectedSpotData; }
            set
            {
                _selectedSpotData = value;
                RaisePropertyChanged();
            }
        }

        private Color _currentStopColor;

        public Color CurrentStopColor {
            get { return _currentStopColor; }
            set
            {
                _currentStopColor = value;
                RaisePropertyChanged();
            }
        }

        private int _adjustingRectangleWidth;

        public int AdjustingRectangleWidth {
            get { return _adjustingRectangleWidth; }
            set
            {
                _adjustingRectangleWidth = value;
                RaisePropertyChanged();
            }
        }

        private int _adjustingRectangleHeight;

        public int AdjustingRectangleHeight {
            get { return _adjustingRectangleHeight; }
            set
            {
                _adjustingRectangleHeight = value;
                RaisePropertyChanged();
            }
        }

        private int _adjustingRectangleTop;

        public int AdjustingRectangleTop {
            get { return _adjustingRectangleTop; }
            set
            {
                _adjustingRectangleTop = value;
                RaisePropertyChanged();
            }
        }

        private int _adjustingRectangleLeft;

        public int AdjustingRectangleLeft {
            get { return _adjustingRectangleLeft; }
            set
            {
                _adjustingRectangleLeft = value;
                RaisePropertyChanged();
            }
        }

        public bool IsNextable {
            get
            {
                if (CurrentLEDEditWizardState == 0)
                    return Count != 0;
                else if (CurrentLEDEditWizardState == 1)
                    return MaxLEDCount == 0;
                else
                    return true;
            }
        }

        public override void ReadData()
        {
            CurrentView = "dashboard";
            LoadContextMenu();
            LoadData();
            SetActivePaletteAllOutputsCommand = new RelayCommand<IColorPalette>((p) =>
            {
                return true;
            }, (p) =>
            {
                SetActivePaletteAllOutputs(p);
            });
            SetAllOutputSelectedModeCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                //foreach (var output in CurrentDevice.AvailableOutputs)
                //{
                //    //output.OutputSelectedMode = CurrentOutput.OutputSelectedMode;
                //}
            });
            SetAllOutputSelectedGifCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                //foreach (var output in CurrentDevice.AvailableOutputs)
                //{
                //    //output.OutputSelectedGif = CurrentOutput.OutputSelectedGif;
                //    //output.OutputSelectedGifIndex = CurrentOutput.OutputSelectedGifIndex;
                //}
            });

            SetAllDeviceSelectedModeCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                //foreach (var device in AvailableDevices.Where(p => !p.IsDummy))
                //{
                //    foreach (var output in device.AvailableOutputs)
                //    {
                //        //output.OutputSelectedMode = CurrentOutput.OutputSelectedMode;
                //    }

                //}
            });
            SetAllDeviceSelectedSolidColorCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                //foreach (var device in AvailableDevices.Where(p => !p.IsDummy))
                //{
                //    foreach (var output in device.AvailableOutputs)
                //    {
                //        //output.OutputStaticColor = CurrentOutput.OutputStaticColor;
                //    }

                //}
            });
            SetAllDeviceSelectedGradientColorCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                //foreach (var device in AvailableDevices.Where(p => !p.IsDummy))
                //{
                //    foreach (var output in device.AvailableOutputs)
                //    {
                //        //output.OutputSelectedGradient = CurrentOutput.OutputSelectedGradient;
                //    }

                //}
            });
            SelecFirmwareForCurrentDeviceCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                //set device hardware version with one selected in the list
                CurrentDevice.HardwareVersion = CurrentSelectedFirmware.TargetHardware;
                // lauch firmware upgrade
                UpgradeIfAvailable(CurrentDevice);
            });
            SetCurrentLEDSetupSentryColorCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                //foreach (var ledSetup in CurrentOutput.OutputLEDSetup)
                //{
                //    foreach (var spot in ledSetup.Spots)
                //    {
                //        spot.SetSentryColor(spot.Red, spot.Green, spot.Blue);
                //        foreach (var color in DefaultColorCollection.snap)
                //        {
                //            spot.SetColor(color.R, color.G, color.B, true);
                //        }
                //    }
                //}

                //show snap effect
            });
            SetAllOutputSelectedSolidColorCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                //foreach (var output in CurrentDevice.AvailableOutputs)
                //{
                //    //output.OutputStaticColor = CurrentOutput.OutputStaticColor;
                //}
            });
            SetAllOutputSelectedGradientColorCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                //foreach (var output in CurrentDevice.AvailableOutputs)
                //{
                //    //output.OutputSelectedGradient = CurrentOutput.OutputSelectedGradient;
                //}
            });
            UpdateCurrentSelectedDeviceFirmwareCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                UpgradeIfAvailable(CurrentDevice);
            });
            SetAllDeviceSelectedGifCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                foreach (var device in AvailableDevices.Where(p => !p.IsDummy))
                {
                    //foreach (var output in device.AvailableOutputs)
                    //{
                    //    //output.OutputSelectedGif = CurrentOutput.OutputSelectedGif;
                    //    //output.OutputSelectedGifIndex = CurrentOutput.OutputSelectedGifIndex;
                    //}

                }
            });
            SetActivePaletteAllDevicesCommand = new RelayCommand<IColorPalette>((p) =>
            {
                return true;
            }, (p) =>
            {
                SetActivePaletteAllDevices(p);
            });

            EditSelectedPaletteCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                EditCurrentPalette(p);
            });
            ResetAppCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                Reset();
            });
            OpenAboutWindowCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenAboutWindow();
            });
            OpenSurfaceEditorWindowCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenSurfaceEditorWindow();
            });
            OpenPasswordDialogCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenPasswordDialog();
            });
            AddNewGradientCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenAddNewGradientWindow();
            });
            SaveNewGradientCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                SaveNewGradient();
            });
            OpenAddNewAutomationCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenAddNewAutomationWindowCommand();
            });
            OpenAppSettingsWindowCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenAppSettingsWindow();
            });
            SaveAllAutomationCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                SaveAllAutomation();
            });
            SaveCurrentSelectedAutomationShortkeyCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                SaveCurrentSelectedAutomationShortkey();
            });
            AddNewSolidColorCommand = new RelayCommand<string>((p) =>
                 {
                     return true;
                 }, (p) =>
                 {
                     PickColorMode = p;
                     OpenColorPickerWindow();
                 });
            AddPickedSolidColorCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                switch (PickColorMode)
                {
                    case "solid":
                        AvailableSolidColors.Insert(0, CurrentPickedColor);
                        WriteSolidColorJson();
                        break;

                    case "gradientStart":
                        CurrentStartColor = CurrentPickedColor;

                        break;

                    case "gradientStop":
                        CurrentStopColor = CurrentPickedColor;
                        break;

                    case "accent":

                        ThemeManager.Current.AccentColor = new SolidColorBrush(CurrentPickedColor);
                        GeneralSettings.AccentColor = CurrentPickedColor;
                        break;
                    case "motion":
                        CurrentSelectedMotion.Color = CurrentPickedColor;
                        RaisePropertyChanged(nameof(CurrentSelectedMotion.Color));
                        //notify UI
                        break;
                }
            }
            );
            EditSelectedPaletteSaveConfirmCommand = new RelayCommand<string>((p) =>
                 {
                     return true;
                 }, (p) =>
                 {
                     SaveCurrentEditedPalette(p);
                 });
            ApplyCurrentOutputCapturingPositionCommand = new RelayCommand<string>((p) =>
           {
               return true;
           }, (p) =>
           {
               // ApplyCurrentOuputCapturingPosition();
           }
            );
            CreateNewPaletteCommand = new RelayCommand<string>((p) =>
                   {
                       return true;
                   }, (p) =>
                   {
                       //CreateNewPalette();
                   });
            RequestingBetaChanelCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                if (p == "@mb1n0b3t@")
                    TurnOnBetaChanel();
                else if (p == "abort")
                {
                    GeneralSettings.IsInBetaChanel = false;
                }
                else
                {
                    HandyControl.Controls.MessageBox.Show("Wrong Password", "Authentication", MessageBoxButton.OK, MessageBoxImage.Error);
                    GeneralSettings.IsInBetaChanel = false;
                }
            });
            CreateNewAutomationCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                CreateNewAutomation();
            });
            SaveCurrentProfileCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                SaveCurrentProfile(CurrentDevice.ActivatedProfileUID);
            }
            );
            DeleteAttachedProfileCommand = new RelayCommand<IDeviceProfile>((p) =>
            {
                return true;
            }, (p) =>
            {
                DeleteAttachedProfile(p);
            }
          );

            // SaveCurrentSelectedAutomationCommand = new RelayCommand<string>((p) =>
            // {
            //     return true;
            // }, (p) =>
            // {
            //     SaveCurrentSelectedAutomation();
            // }
            //);
            ExportCurrentProfileCommand = new RelayCommand<IDeviceProfile>((p) =>
            {
                return true;
            }, (p) =>
            {
                ExportCurrentProfile(p);
            }
          );

            //  ExportCurrentColorEffectCommand = new RelayCommand<string>((p) =>
            //  {
            //      return true;
            //  }, (p) =>
            //  {
            //      //ExportCurrentColorEffect();
            //  }
            //);
            OpenRectangleScaleCommand = new RelayCommand<ObservableCollection<IDrawable>>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenRectangleScaleWindow(p);
            }
          );
            SetSelectedMotionCommand = new RelayCommand<ITimeLineDataItem>((p) =>
            {
                return true;
            }, (p) =>
            {
                //show right property panel
                CurrentSelectedMotion = p;
            }
        );
            CompositionNextFrameCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                //show right property panel
                if (CurrentCompositionFrame < 9600)
                    CurrentCompositionFrame++;
            }
     );

            CutSelectedMotionCommand = new RelayCommand<ITimeLineDataItem>((p) =>
            {
                return true;
            }, (p) =>
            {
                //show right property panel
                CutSelectedMotion(p);
            }
     );
            ToggleCompositionPlayingStateCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                //show right property panel
                CurrentCompositionPlayingState = !CurrentCompositionPlayingState;
            }
);
            CompositionPreviousFrameCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                //show right property panel
                if (CurrentCompositionFrame > 0)
                    CurrentCompositionFrame--;
            }

);
            CompositionFrameStartOverCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                //show right property panel
                CurrentCompositionFrame = 0;
            }

);
            OpenAvailableLedSetupForCurrentDeviceWindowCommand = new RelayCommand<OutputSettings>((p) =>
            {
                return true;
            }, (p) =>
            {
                CurrentOutput = p;
                OpenAvailableLedSetupForCurrentDeviceWindow();
            }
          );
            PIDWindowClosingCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                //CurrentOutput.IsInSpotEditWizard = false;
            }
        );
            SurfaceEditorWindowClosingCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                IsRichCanvasWindowOpen = false;
            }
    );
            OpenRectangleRotateCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenRectangleRotateWindow();
            }
        );
            SetSelectedItemLiveViewCommand = new RelayCommand<IOutputSettings>((p) =>
            {
                return true;
            }, (p) =>
            {
                CurrentOutput = p;
            }
          );
            SetSelectedItemScaleFactorCommand = new RelayCommand<ObservableCollection<IDrawable>>((p) =>
            {
                return true;
            }, (p) =>
            {
                SetSelectedItemScaleFactor(p);
            }
            );
            SetSelectedItemRotateFactorCommand = new RelayCommand<ObservableCollection<IDrawable>>((p) =>
            {
                return true;
            }, (p) =>
            {
                SetSelectedItemRotateFactor(p);
            }
           );
            ResetToDefaultRectangleScaleCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                ResetToDefaultRectangleScale();
            }
            );
            AglignSelectedItemstoLeftCommand = new RelayCommand<ObservableCollection<IDrawable>>((p) =>
            {
                return true;
            }, (p) =>
            {
                AglignSelectedItemstoLeft(p);
            }
            );
            LockSelectedItemCommand = new RelayCommand<ObservableCollection<IDrawable>>((p) =>
            {
                return true;
            }, (p) =>
            {

                LockSelectedItem(p);

            }
            );
            OpenAddNewDrawableItemCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {

                OpenAddNewItemWindow();

            }
            );
            AddItemsToPIDCanvasCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {

                AddItemsToPIDCanvas();

            }
            );
            AddImageToPIDCanvasCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {

                AddImage();

            }
            );
            DeleteSelectedItemsCommand = new RelayCommand<ObservableCollection<IDrawable>>((p) =>
            {
                return true;
            }, (p) =>
            {

                DeleteSelectedItems(p);

            }
            );
            ClearPIDCanvasCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {

                ClearPIDCanvas();

            }
            );


            SetRandomOutputColorCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                SetRandomOutputColor();
            }
            );
            SaveCurretSurfaceLayoutCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                SaveCurrentSurfaceLayout();
            }
            );

            SaveCurrentLEDSetupLayoutCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                SaveCurrentLEDSetupLayout();
            }
            );

            UnlockSelectedItemCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                switch (p)
                {
                    case "PID":
                        UnlockSelectedItem(PIDEditWindowsRichCanvasItems);
                        break;
                    case "SUR":
                        UnlockSelectedItem(SurfaceEditorItems);
                        break;
                }
            }
       );
            SpreadItemRightHorizontalCommand = new RelayCommand<ObservableCollection<IDrawable>>((p) =>
            {
                return true;
            }, (p) =>
            {
                SpreadItemHorizontal(p, 0);
            }
            );
            SpreadItemLeftHorizontalCommand = new RelayCommand<ObservableCollection<IDrawable>>((p) =>
            {
                return true;
            }, (p) =>
            {
                SpreadItemHorizontal(p, 1);
            }
            );
            SpreadItemUpVerticalCommand = new RelayCommand<ObservableCollection<IDrawable>>((p) =>
            {
                return true;
            }, (p) =>
            {
                SpreadItemVertical(p, 1);
            }
           );
            SpreadItemDownVerticalCommand = new RelayCommand<ObservableCollection<IDrawable>>((p) =>
            {
                return true;
            }, (p) =>
            {
                SpreadItemVertical(p, 0);
            }
            );
            AglignSelectedItemstoTopCommand = new RelayCommand<ObservableCollection<IDrawable>>((p) =>
            {
                return true;
            }, (p) =>
            {
                AglignSelectedItemstoTop(p);
            }
            );
            AddSelectedItemToGroupCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                AddSelectedItemToGroup();
            }
          );
            ExitCurrentRunningAppCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                System.Windows.Application.Current.Shutdown();
            }
       );


            UpdateAppCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, async (p) =>
            {
                await UpdateApp();
            }
    );

            OpenLEDSteupSelectionWindowsCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {

                OpenLEDSteupSelectionWindows();
            }
          );
            SetCurrentSelectedOutputForCurrentSelectedOutputCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                if (SelectedOutputForCurrentDevice != null)
                    SetCurrentSelectedOutputForCurrentSelectedOutput();
            }

          );
            OpenExportNewColorEffectCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenExportNewEffectWindow();
            }
          );
            OpenFanSpeedPlotWindowsCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenFanSpeedPlotWindows();
            }
          );
            OpenAvailableUpdateListWindowCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, async (p) =>
            {
                IsCheckingForUpdate = true;
                UpdateButtonEnable = false;
                AvailableUpdates = null;
                await CheckForUpdate();
                IsCheckingForUpdate = false;
                UpdateButtonEnable = true;
                if (AvailableUpdates == null)
                {
                    return;
                }
                if (AvailableUpdates.ReleasesToApply.Any())
                {
                    OpenAvailableUpdateListWindow();
                }
                else
                {
                    HandyControl.Controls.MessageBox.Show("Không tìm thấy bản cập nhật mới nào", "No Update Available", MessageBoxButton.OK, MessageBoxImage.Information);
                }


            }
          );
            ImportProfileCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                ImportProfile();
            }
          );
            CreateNewProfileCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                CreateNewProfile();
            }
            );
            OpenProfileCreateCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenCreatenewProfileWindow();
            }
           );
            CoppyColorCodeCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                System.Windows.Clipboard.SetText(SelectedSolidColor.ToString());
                Growl.Success("Color code copied to clipboard!");
            }
         );
            DeleteSelectedSolidColorCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                AvailableSolidColors.Remove(SelectedSolidColor);
                WriteSolidColorJson();
            }
      );

            DeleteSelectedPaletteCommand = new RelayCommand<IColorPalette>((p) =>
            {
                return true;
            }, (p) =>
            {
                DeleteSelectedPalette(p);
            });
            UploadSelectedPaletteCommand = new RelayCommand<IColorPalette>((p) =>
           {
               return true;
           }, (p) =>
           {
               UploadSelectedPalette(p);
           });
            DeleteSelectedGradientCommand = new RelayCommand<IGradientColorCard>((p) =>
            {
                return true;
            }, (p) =>
            {
                DeleteSelectedGradient(p);
            });
            DeleteSelectedGifCommand = new RelayCommand<IGifCard>((p) =>
            {
                return true;
            }, (p) =>
            {
                DeleteSelectedGif(p);
            });

            DeleteSelectedAutomationCommand = new RelayCommand<IAutomationSettings>((p) =>
            {
                return true;
            }, (p) =>
            {
                DeleteSelectedAutomation(p);
            });

            SetIncreamentCommandfromZero = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                SetIncreament(0, 1, 0, PreviewSpots.Length - 1);
            });
            SetIncreamentCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                SetIncreament(10, 1, 0, PreviewSpots.Length - 1);
            });



            ImportEffectCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                ImportEffect();
            });



            DeleteSelectedDeviceCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                DeleteSelectedDevice();
            });
            DeleteSelectedDevicesCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                DeleteSelectedDevices();
            });

            ImportPaletteCardFromFileCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                ImportPaletteCardFromFile();
            });
            ImportedGifFromFileCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                ImportGif();
            });
            ExportCurrentSelectedPaletteToFileCommand = new RelayCommand<IColorPalette>((p) =>
            {
                return true;
            }, (p) =>
            {
                ExportCurrentSelectedPaletteToFile(p);
            });
            ExportPIDCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                //ExportCurrentOutputPID();

            });
            ImportLEDSetupCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                ImportPIDToCanvas();
            });
            AddNewZoneCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                AddNewZone();
            });
            UnZoneItemsCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                UnZone();
            });
            SnapshotCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                //SnapShot();
            });
            LaunchPIDEditWindowCommand = new RelayCommand<IDrawable>((p) =>
            {
                return true;
            }, (p) =>
            {
                LaunchPIDEditWindow(p);
            });
            LaunchCompositionEditWindowCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                LaunchCompositionEditWindow();
            });

            LaunchDeleteSelectedDeviceWindowCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                LaunchDeleteSelectedDeviceWindow();
            });
            OpenFFTPickerWindowCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                //OpenFFTPickerWindow();
            });
            LaunchAdrilightStoreItemCeatorWindowCommand = new RelayCommand<object>((p) =>
            {
                return true;
            }, (p) =>
            {
                CurrentOnlineItemToExport = p;
                LaunchAdrilightStoreItemCreatorWindow();
            });
            SelectLiveViewItemCommand = new RelayCommand<IDrawable>((p) =>
            {
                return true;
            }, (p) =>
            {
                //this selected live view item is an ledsetup, try to find it's parrent and set active
                // SelectedControlZone = p as IControlZone;
                //unselected other items
                foreach (var item in CurrentDevice.CurrentLiveViewZones)
                {
                    (item as IDrawable).IsSelected = false;
                }
                p.IsSelected = true;
                SelectedLiveViewItem = p;

            });

            SelectPIDCanvasItemCommand = new RelayCommand<IDrawable>((p) =>
            {
                return true;
            }, (p) =>
            {
                //this selected live view item is an ledsetup, try to find it's parrent and set active
                // SelectedControlZone = p as IControlZone;
                //unselected other items
                //if(p is LEDSetup)
                //(p as LEDSetup).FillSpotsColor(Color.FromRgb(255, 0, 0));

            });
            //ShowBrightnessAdjustmentPopupCommand = new RelayCommand<IOutputSettings>((p) =>
            //{
            //    return true;
            //}, (p) =>
            //{
            //    p.IsBrightnessPopupOpen = true;
            //});
            //SendCurrentDeviceSpeedCommand = new RelayCommand<string>((p) =>
            //{
            //    return true;
            //}, (p) =>
            //{
            //    if (CurrentDevice.IsTransferActive)
            //    {
            //        CurrentDevice.CurrentState = State.speed;
            //        IsSpeedSettingUnsetted = false;
            //    }
            //});
            OpenActionsManagerWindowCommand = new RelayCommand<IAutomationSettings>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenActionsManagerWindow(p);
            });
            OpenHotKeySelectionWindowCommand = new RelayCommand<IAutomationSettings>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenHotKeySelectionWindow(p);
            });
            OpenTargetDeviceSelectionWindowCommand = new RelayCommand<IActionSettings>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenTargetDeviceSelectionWindow(p);
            });
            OpenTargetActionSelectionWindowCommand = new RelayCommand<IActionSettings>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenTargetActionSelectionWindow(p);
            });
            OpenAutomationValuePickerWindowCommand = new RelayCommand<IActionSettings>((p) =>
            {
                return true;
            }, (p) =>
            {
                switch (p.ActionParameter.Type)
                {
                    case "color":
                        //open color picker windows
                        OpenAutomationColorPickerWindow(p);
                        break;
                }

            });
            OpenTargetParamSelectionWindowCommand = new RelayCommand<IActionSettings>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenTargetParamSelectionWindow(p);
            });

            OpenSpotMapWindowCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenSpotMapWindow();
            });
            OpenAutomationManagerWindowCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenAutomationManagerWindow();
            });
            OpenAmbinoStoreWindowCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenAmbinoStoreWindow();
            });
            DownloadSelectedPaletteCommand = new RelayCommand<IOnlineItemModel>((p) =>
            {
                return true;
            }, (p) =>
            {
                //DonwloadSelectedItem(p);
            });
            DownloadSelectedChasingPattern = new RelayCommand<Motion>((p) =>
            {
                return true;
            }, (p) =>
            {
                // DownloadSelectedPalette(p);
            });
            SetCurrentActionTypeForSelectedActionCommand = new RelayCommand<ActionType>((p) =>
            {
                return true;
            }, (p) =>
            {
                SetCurrentActionTypeForSelectedAction(p);
            });
            AddSelectedActionTypeToListCommand = new RelayCommand<ActionType>((p) =>
            {
                return true;
            }, (p) =>
            {
                AddSelectedActionTypeToList(p);
            });
            ExecuteAutomationFromManagerCommand = new RelayCommand<ObservableCollection<IActionSettings>>((p) =>
            {
                return true;
            }, (p) =>
            {
                ExecuteAutomationActions(p);
            });
            DeleteSelectedActionFromListCommand = new RelayCommand<IActionSettings>((p) =>
            {
                return true;
            }, (p) =>
            {
                DeleteSelectedActionFromList(p);
            });
            SetCurrentActionTargetDeviceForSelectedActionCommand = new RelayCommand<IDeviceSettings>((p) =>
            {
                return true;
            }, (p) =>
            {
                SetCurrentActionTargetDeviceForSelectedAction(p);
            });
            SetCurrentActionParamForSelectedActionCommand = new RelayCommand<IActionParameter>((p) =>
            {
                return true;
            }, (p) =>
            {
                SetCurrentActionParamForSelectedAction(p);
            });
            SetCurrentSelectedActionTypeColorValueCommand = new RelayCommand<Color>((p) =>
            {
                return true;
            }, (p) =>
            {
                SetCurrentSelectedActionTypeColorValue(p);
            });
            OpenHardwareMonitorWindowCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenHardwareMonitorWindow();
            });
            LaunchWBAdjustWindowCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                LaunchWBAdjustWindow();
            });
            LaunchVIDEditWindowCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                LaunchVIDEditWindow(p);

            });
            //LaunchMIDEditWindowCommand = new RelayCommand<string>((p) =>
            //{
            //    return true;
            //}, (p) =>
            //{
            //    //LaunchMIDEditWindow();
            //});
            //LaunchCIDEditWindowCommand = new RelayCommand<string>((p) =>
            //{
            //    return true;
            //}, (p) =>
            //{
            //    L/*aunchCIDEditWindow();*/
            //});
            DeviceRectDropCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                DeviceRectSavePosition();
            });
            SelectOnlineItemCommand = new RelayCommand<IOnlineItemModel>((p) =>
            {
                return true;
            }, async (p) =>
            {
                await gotoItemDetails(p);
            });
            SelectCardCommand = new RelayCommand<IDeviceSettings>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
                {

                    foreach (var device in AvailableDevices) //cancel any selection if it's active
                    {
                        if (device.IsSelected)
                        {
                            device.IsSelected = false;
                            if (SelectedDeviceCount > 0)
                                SelectedDeviceCount--;
                        }


                    }

                    CurrentDevice = p;

                    if (CurrentDevice.IsSizeNeedUserDefine) // this device has default ledsetup so we need to tell user to select the led setup
                    {
                        OOTBStage = 0;
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
                        {
                            OpenLEDSteupSelectionWindows();
                        });
                    }
                    else
                    {
                        GotoChild(p);
                    }

                }
                else if (Keyboard.IsKeyDown(Key.LeftCtrl)) // device is selected with ctrl key, start multiple select
                {
                    if (p.IsSelected)
                    {
                        p.IsSelected = false;
                        if (SelectedDeviceCount > 0)
                            SelectedDeviceCount--;
                    }

                    else
                    {
                        p.IsSelected = true;
                        SelectedDeviceCount++;

                    }


                }

                if (SelectedDeviceCount > 0)
                {
                    ToolbarVisible = true;
                }
                else
                {
                    ToolbarVisible = false;
                }
            });

            SaveNewUserEditLEDSetup = new RelayCommand<string>((p) =>
                  {
                      return p != null;
                  }, (p) =>
                  {
                      //WriteDeviceInfoJson();
                      RaisePropertyChanged(nameof(CurrentOutput));
                  });
            //JumpToNextWizardStateCommand = new RelayCommand<string>((p) =>
            //{
            //    return p != null;
            //}, (p) =>
            //{
            //    CurrentLEDEditWizardState++;

            //    if (CurrentLEDEditWizardState == 1)
            //    {
            //        GrabActivatedSpot();
            //    }
            //    else if (CurrentLEDEditWizardState == 2)
            //    {
            //        ReorderActivatedSpot();
            //        //CurrentOutput.IsInSpotEditWizard = false;
            //    }
            //});
            NextOOTBCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                if (OOTBStage < 2)
                    OOTBStage++;
            });

            OpenTutorialCommand = new RelayCommand<string>((p) =>
           {
               return p != null;
           }, (p) =>
{
    System.Diagnostics.Process.Start("https://github.com/Kaitoukid93/Ambinity-5.0/wiki/VID-l%C3%A0-g%C3%AC%3F-C%C3%A1ch-s%E1%BB%AD-d%E1%BB%A5ng");
});
            FinishOOTBCommand = new RelayCommand<string>((p) =>
                        {
                            return p != null;
                        }, (p) =>
                        {

                            CurrentDevice.IsSizeNeedUserDefine = false;
                            GotoChild(CurrentDevice);
                            OOTBWindows.Close();
                        });

            PrevioustOOTBCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                if (OOTBStage > 0)
                    OOTBStage--;
            });
            SkipOOTBCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                if (OOTBWindows.askAgain.IsChecked == true)
                    CurrentDevice.IsSizeNeedUserDefine = false;
                GotoChild(CurrentDevice);
                OOTBWindows.Close();

            });
            JumpToNextAddDeviceWizardStateCommand = new RelayCommand<string>((p) =>
                   {
                       return p != null;
                   }, (p) =>
                   {
                       CurrentAddDeviceWizardState++;
                   });
            AddCurrentSelectedDeviceToDashboard = new RelayCommand<string>((p) =>
           {
               return p != null;
           }, (p) =>
           {
               AddDevice();
           }
            );
            ApplySpotImportDataCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                //ApplySpotImportData();
            }
            );
            ApplyOutputImportDataCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                //ApplyOutputImportData();
            }
            );

            BackToPreviousAddDeviceWizardStateCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                CurrentAddDeviceWizardState--;
            });
            RenameSelectedItemCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                CurrentOutput.OutputName = NameToChange;
                RaisePropertyChanged(nameof(CurrentOutput.OutputName));
            });
            ShowNameEditWindow = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                OpenNameEditWindow();
            });

            OpenDeviceConnectionSettingsWindowCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                OpenDeviceConnectionSettingsWindow();
            });
            OpenDeviceFirmwareSettingsWindowCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                OpenDeviceFirmwareSettingsWindow();
            });
            OpenAdvanceSettingWindowCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                OpenAdvanceSettingWindow();
            });
            CancelEditWizardCommand = new RelayCommand<string>((p) =>
                 {
                     return p != null;
                 }, (p) =>
                 {
                 });
            //BackToPreviousWizardStateCommand = new RelayCommand<string>((p) =>
            //     {
            //         return p != null;
            //     }, (p) =>
            //     {
            //         CurrentOutput.IsInSpotEditWizard = true;
            //         CurrentLEDEditWizardState--;

            //         //if (CurrentLEDEditWizardState == 1)
            //         //{
            //         //    BufferSpots = new IDeviceSpot[MaxLEDCount];
            //         //    GrabActivatedSpot();

            //         //}
            //         //else if (CurrentLEDEditWizardState == 2)
            //         //{
            //         //    ReorderActivatedSpot();
            //         //}
            //         //else if (CurrentLEDEditWizardState == 3)
            //         //{
            //         //    RunTestSequence();
            //         //}
            //     });

            //SetSpotActiveCommand = new RelayCommand<IDeviceSpot>((p) =>
            //{
            //    return p != null;
            //}, (p) =>
            //{
            //    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            //    {
            //        foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
            //        {
            //            spot.SetStroke(0.5);
            //            spot.IsActivated = true;
            //            Count++;
            //        }
            //    }
            //    else
            //    {
            //        if (p.BorderThickness != 0)
            //        {
            //            p.SetStroke(0);
            //            Count--;
            //            p.IsActivated = false;
            //        }
            //        else
            //        {
            //            p.SetStroke(0.5);
            //            Count++;
            //            p.IsActivated = true;
            //        }
            //    }
            //});
            //SetAllSpotActiveCommand = new RelayCommand<string>((p) =>
            //{
            //    return p != null;
            //}, (p) =>
            //{
            //    Count = 0;
            //    foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
            //    {
            //        spot.SetStroke(0.5);
            //        spot.IsActivated = true;
            //        Count++;
            //    }
            //});
            SetBorderSpotActiveCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {

            });
            SetSpotPIDCommand = new RelayCommand<IDeviceSpot>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                SetSpotID(p);
            });
            SetSpotVIDCommand = new RelayCommand<IDeviceSpot>((p) =>
            {
                return p != null;
            }, (p) =>
            {

                SetSpotID(p);

            });
            //NextOutputCommand = new RelayCommand<string>((p) =>
            //{
            //    return p != null;
            //}, (p) =>
            //{
            //    foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
            //    {
            //        if (spot.BorderThickness != 0.0)
            //        {
            //            spot.BorderThickness = 0.0;
            //        }
            //    }
            //    int currentOutputID = CurrentOutput.OutputID;
            //    if (currentOutputID + 1 == CurrentDevice.AvailableOutputs.Count())
            //    {
            //        currentOutputID = 0;
            //        CurrentOutput = CurrentDevice.AvailableOutputs[currentOutputID];
            //        CurrentDevice.SelectedOutput = currentOutputID;
            //    }
            //    else
            //    {
            //        CurrentDevice.SelectedOutput = currentOutputID + 1;
            //        CurrentOutput = CurrentDevice.AvailableOutputs[currentOutputID + 1];
            //    }
            //});
            //ResetDefaultAspectRatioCommand = new RelayCommand<string>((p) =>
            //{
            //    return p != null;
            //}, (p) =>
            //{
            //    double ratio = CurrentOutput.OutputNumLEDX / CurrentOutput.OutputNumLEDY;
            //    int width = 100;
            //    int height = (int)(100d / ratio);
            //    CurrentOutput.SetRectangle(new Rectangle(0, 0, width, height));
            //    AdjustingRectangleHeight = CurrentOutput.OutputRectangle.Height;
            //    AdjustingRectangleWidth = CurrentOutput.OutputRectangle.Width;
            //    AdjustingRectangleTop = CurrentOutput.OutputRectangle.Top;
            //    AdjustingRectangleLeft = CurrentOutput.OutputRectangle.Left;
            //});
            //    PreviousOutputCommand = new RelayCommand<string>((p) =>
            //{
            //    return p != null;
            //}, (p) =>
            //{
            //    foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
            //    {
            //        if (spot.BorderThickness != 0.0)
            //        {
            //            spot.BorderThickness = 0.0;
            //        }
            //    }
            //    int currentOutputID = CurrentOutput.OutputID;
            //    if (currentOutputID == 0)
            //    {
            //        currentOutputID = CurrentDevice.AvailableOutputs.Count();
            //    }
            //    CurrentOutput = CurrentDevice.AvailableOutputs[currentOutputID - 1];
            //    CurrentDevice.SelectedOutput = currentOutputID - 1;
            //});

            //    ResetCountCommand = new RelayCommand<string>((p) =>
            //{
            //    return p != null;
            //}, (p) =>
            //{

            //    switch (CurrentIDType)
            //    {
            //        case "VID":
            //            foreach (var device in AvailableDevices.Where(d => !d.IsDummy))
            //            {
            //                foreach (var output in device.AvailableOutputs)
            //                {
            //                    foreach (var spot in output.OutputLEDSetup.Spots)
            //                    {

            //                        spot.SetVID(0);
            //                        spot.IsEnabled = false;
            //                    }
            //                }
            //            }
            //            break;
            //        case "FID":
            //            foreach (var device in AvailableDevices.Where(d => !d.IsDummy))
            //            {
            //                foreach (var output in device.AvailableOutputs)
            //                {
            //                    foreach (var spot in output.OutputLEDSetup.Spots)
            //                    {

            //                        spot.SetMID(0);
            //                        spot.IsEnabled = false;
            //                    }
            //                }
            //            }
            //            break;
            //        case "CID":
            //            foreach (var device in AvailableDevices.Where(d => !d.IsDummy))
            //            {
            //                foreach (var output in device.AvailableOutputs)
            //                {
            //                    foreach (var spot in output.OutputLEDSetup.Spots)
            //                    {

            //                        spot.SetCID(0);
            //                        spot.IsEnabled = false;
            //                    }
            //                }
            //            }
            //            break;
            //    }
            //});
            //SetCurrentSelectedVID = new RelayCommand<string>((p) =>
            //{
            //    return p != null;
            //}, (p) =>
            //{
            //    if (p != "")
            //        ProcessSelectedSpots(p);
            //});
            //SetSelectedSpotCIDCommand = new RelayCommand<string>((p) =>
            //{
            //    return p != null;
            //}, (p) =>
            //{
            //    if (p != "")
            //        ProcessSelectedSpots(p);
            //});
            SetSelectedSpotIDLeftToRightCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                // if (p != "")
                // ProcessSelectedSpotsID(p);
            });

            //SetCurrentSelectedVIDRange = new RelayCommand<string>((p) =>
            //{
            //    return p != null;
            //}, (p) =>
            //{
            //    ProcessSelectedSpotsWithRange(RangeMinValue, RangeMaxValue);
            //});
            //SetPIDNeutral = new RelayCommand<string>((p) =>
            //{
            //    return p != null;
            //}, (p) =>
            //{
            //    MaxLEDCount = ActivatedSpots.Count;
            //    foreach (var spot in ActivatedSpots)
            //    {
            //        spot.SetColor(0, 0, 0, true);
            //        spot.SetIDVissible(false);
            //    }
            //    foreach (var spot in ActivatedSpots)
            //    {
            //        spot.SetColor(100, 27, 0, true);
            //        spot.SetID(ActivatedSpots.Count() - MaxLEDCount--);
            //        spot.SetIDVissible(true);
            //    }
            //});
            //SetPIDReverseNeutral = new RelayCommand<string>((p) =>
            //{
            //    return p != null;
            //}, (p) =>
            //{
            //    MaxLEDCount = ActivatedSpots.Count;
            //    foreach (var spot in ActivatedSpots)
            //    {
            //        spot.SetColor(0, 0, 0, true);
            //        spot.SetIDVissible(false);
            //    }
            //    foreach (var spot in ActivatedSpots)
            //    {
            //        spot.SetColor(100, 27, 0, true);
            //        spot.SetID(MaxLEDCount--);
            //        spot.SetIDVissible(true);
            //    }
            //});
            //ResetMaxCountCommand = new RelayCommand<string>((p) =>
            //{
            //    return p != null;
            //}, (p) =>
            //{
            //    MaxLEDCount = ActivatedSpots.Count;
            //    foreach (var spot in ActivatedSpots)
            //    {
            //        spot.SetColor(0, 0, 0, true);
            //        spot.SetIDVissible(false);
            //    }
            //});

            SetZoneColorCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                //if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))

                //    CurrentPickedColor = CurrentDevice.CustomZone[CurrentSelectedCustomColorIndex];

                //else
                //    SetCustomColor(CurrentSelectedCustomColorIndex);
            });
            GetZoneColorCommand = new RelayCommand<System.Windows.Media.Color>((p) =>
            {
                return true;
            }, (p) =>
            {
            });

            //LightingModeSelection = new RelayCommand<string>((p) => {
            //    return p != null;
            //}, (p) =>
            //{
            //    switch(p)
            //    {
            //        case "Riêng Lẻ":
            //            //Enbale HUB output Navigation bar
            //            //Notify child devices to restart their own background service
            //            HubOutputNavigationEnable = true;
            //            RaisePropertyChanged(() => HubOutputNavigationEnable);
            //            break;
            //        case "Đồng Bộ":
            //            //disable navigation
            //            HubOutputNavigationEnable = false;
            //            //Notify child devices to end their own background services and expose their spotset for parrent HUB to take over the Lighting control
            //            RaisePropertyChanged(() => HubOutputNavigationEnable);
            //            break;
            //    }

            //});

            ShowAddNewCommand = new RelayCommand<IDeviceSettings>((p) =>
            {
                return true;
            }, (p) =>
            {
                ShowAddNewWindow();
            });

            BackCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                BackToDashboard();
            });
        }

        private void OpenPasswordDialog()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"PasswordDialog"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }

        private void TurnOnBetaChanel()
        {
            GeneralSettings.IsInBetaChanel = true;
        }

        //private static object _syncRoot = new object();

        //private async void ScanSerialDevice()
        //{
        //    ISerialDeviceDetection detector = new SerialDeviceDetection();
        //    var tokenSource = new CancellationTokenSource();
        //    CancellationToken token = tokenSource.Token;

        //    var jobTask = Task.Run(() =>
        //    {
        //        // Organize critical sections around logical serial port operations somehow.
        //        lock (_syncRoot)
        //        {
        //            return detector.DetectedDevices;
        //        }
        //    });
        //    if (jobTask != await Task.WhenAny(jobTask, Task.Delay(Timeout.Infinite, token)))
        //    {
        //        // Timeout;
        //        return;
        //    }
        //    var newDevices = await jobTask;
        //    if (newDevices.Count == 0)
        //    {
        //        HandyControl.Controls.MessageBox.Show("Unable to detect any supported device, try adding manually", "No Compatible Device Found", MessageBoxButton.OK, MessageBoxImage.Warning);
        //    }
        //    else
        //    {
        //        foreach (var device in newDevices)
        //        {
        //            Debug.WriteLine("Name: " + device.DeviceName);
        //            Debug.WriteLine("ID: " + device.DeviceSerial);
        //            Debug.WriteLine("Firmware Version: " + device.FirmwareVersion);
        //            Debug.WriteLine("---------------");
        //        }
        //        AvailableSerialDevices = new ObservableCollection<IDeviceSettings>();
        //        foreach (var device in newDevices)
        //        {
        //            AvailableSerialDevices.Add(device);
        //        }
        //        tokenSource.Cancel();
        //    }
        //}

        //private void OpenFFTPickerWindow()
        //{
        //    if (AssemblyHelper.CreateInternalInstance($"View.{"MIDEditWindow"}") is System.Windows.Window window)
        //    {
        //        VisualizerFFT = new VisualizerProgressBar[CurrentOutput.OutputLEDSetup.Spots.Count];
        //        for (int i = 0; i < VisualizerFFT.Length; i++)
        //        {
        //            VisualizerFFT[i] = new VisualizerProgressBar(0.0f, Color.FromRgb(0, 0, 0));
        //        }

        //        IsVisualizerWindowOpen = true;
        //        window.Owner = System.Windows.Application.Current.MainWindow;
        //        window.ShowDialog();
        //    }
        //}

        private void OpenActionsManagerWindow(IAutomationSettings selectedautomation)
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"ActionManagerWindow"}") is System.Windows.Window window)
            {
                CurrentSelectedAutomation = selectedautomation;
                AvailableActionsforCurrentDevice = new ObservableCollection<ActionType>();
                AvailableActionsforCurrentDevice.Add(new ActionType { Name = "Kích hoạt", Description = "Kích hoạt một Profile có sẵn", Geometry = "apply", Type = "Activate", LinkText = "Cho thiết bị", IsValueDisplayed = false });
                AvailableActionsforCurrentDevice.Add(new ActionType { Name = "Tăng", Description = "Tăng giá trị của một thuộc tính", Geometry = "apply", Type = "Increase", LinkText = "Của thiết bị", IsValueDisplayed = false });
                AvailableActionsforCurrentDevice.Add(new ActionType { Name = "Giảm", Description = "Giảm giá trị của một thuộc tính", Geometry = "apply", Type = "Decrease", LinkText = "Của thiết bị", IsValueDisplayed = false });
                AvailableActionsforCurrentDevice.Add(new ActionType { Name = "Bật", Description = "Bật một tính năng", Geometry = "apply", Type = "On", LinkText = "Của thiết bị", IsValueDisplayed = false });
                AvailableActionsforCurrentDevice.Add(new ActionType { Name = "Tắt", Description = "Tắt một tính năng", Geometry = "apply", Type = "Off", LinkText = "Của thiết bị", IsValueDisplayed = false });
                AvailableActionsforCurrentDevice.Add(new ActionType { Name = "Chuyển", Description = "Chuyển đổi đồng thời kích hoạt một tính năng", Geometry = "apply", Type = "Change", LinkText = "Của thiết bị", ToResultText = "thành", IsValueDisplayed = true });
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }
        private void OpenHotKeySelectionWindow(IAutomationSettings selectedautomation)
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"HotKeySelectionWindow"}") is System.Windows.Window window)
            {
                CurrentSelectedAutomation = selectedautomation;
                RaisePropertyChanged(nameof(CurrentSelectedAutomation));
                CurrentSelectedShortKeys = new ObservableCollection<KeyModel>();
                CurrentSelectedModifiers = new ObservableCollection<string>();


                //CurrentSelectedShortKeys.Add(KeyInterop.KeyFromVirtualKey(CurrentSelectedAutomation.Condition).ToString());

                //foreach (var modifier in CurrentSelectedAutomation.Modifiers)
                //{
                //    CurrentSelectedModifiers.Add(modifier.Name);
                //}

                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }
        private void OpenTargetDeviceSelectionWindow(IActionSettings selectedAction)
        {

            CurrentSelectedAction = selectedAction;
            RaisePropertyChanged(nameof(CurrentSelectedAction));
            ParamType = "device";
            AutomationParamList = new ObservableCollection<object>();
            foreach (var device in AvailableDevices)
            {
                if (!device.IsDummy)
                {
                    if (CurrentSelectedAction.ActionParameter.Type == "speed") // only filter hubfan
                    {
                        if (device.DeviceType == "ABFANHUB")
                            AutomationParamList.Add(device);
                    }
                    else
                    {
                        AutomationParamList.Add(device);
                    }

                }

            }

            if (AssemblyHelper.CreateInternalInstance($"View.{"TargetDeviceSelectionWindow"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }
        private void OpenTargetActionSelectionWindow(IActionSettings selectedAction)
        {



            CurrentSelectedAction = selectedAction;
            RaisePropertyChanged(nameof(CurrentSelectedAction));
            ParamType = "action";
            AutomationParamList = new ObservableCollection<object>();
            foreach (var actionType in AvailableActionsforCurrentDevice)
            {
                AutomationParamList.Add(actionType);
            }







            if (AssemblyHelper.CreateInternalInstance($"View.{"TargetDeviceSelectionWindow"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }

        private void OpenAutomationColorPickerWindow(IActionSettings selectedAction)
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"AutomationColorPickerWindow"}") is System.Windows.Window window)
            {
                CurrentSelectedAction = selectedAction;
                RaisePropertyChanged(nameof(CurrentSelectedAction));
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }

        private IActionParameter GetAutoMationParam(string paramType, string value)
        {
            var returnParam = new ActionParameter();
            switch (paramType)
            {
                case "brightness":
                    returnParam.Name = "Độ sáng";
                    returnParam.Geometry = "brightness";
                    returnParam.Type = "brightness";
                    returnParam.Value = value;
                    break;
                case "state":
                    returnParam.Name = "Tất cả LED";
                    returnParam.Geometry = "brightness";
                    returnParam.Type = "state";
                    returnParam.Value = value;
                    break;
                case "color":
                    returnParam.Name = "Màu";
                    returnParam.Geometry = "brightness";
                    returnParam.Type = "color";
                    returnParam.Value = Color.FromRgb(255, 255, 0);
                    break;
                case "speed":
                    returnParam.Name = "Tốc độ fan";
                    returnParam.Geometry = "brightness";
                    returnParam.Type = "speed";
                    returnParam.Value = value;
                    break;

            }
            return returnParam;
        }
        private void OpenTargetParamSelectionWindow(IActionSettings selectedAction)
        {


            CurrentSelectedAction = selectedAction;
            RaisePropertyChanged(nameof(CurrentSelectedAction));
            ParamType = "param";

            switch (CurrentSelectedAction.ActionType.Type)
            {
                case "Activate":
                    AutomationParamList = new ObservableCollection<object>();
                    var targetDevice = AvailableDevices.Where(x => x.DeviceUID == CurrentSelectedAction.TargetDeviceUID).FirstOrDefault();
                    foreach (var profile in AvailableProfiles)
                    {
                        if (profile.DeviceType == targetDevice.DeviceType)
                        {
                            AutomationParamList.Add(new ActionParameter { Geometry = profile.Geometry, Name = profile.Name, Type = "profile", Value = profile.ProfileUID });
                        }

                    }
                    break;
                case "Increase":
                    AutomationParamList = new ObservableCollection<object>();
                    AutomationParamList.Add(GetAutoMationParam("brightness", "up"));
                    AutomationParamList.Add(GetAutoMationParam("speed", "up"));
                    break;
                case "Decrease":
                    AutomationParamList = new ObservableCollection<object>();
                    AutomationParamList.Add(GetAutoMationParam("brightness", "down"));
                    AutomationParamList.Add(GetAutoMationParam("speed", "down"));
                    break;
                case "Off":
                    AutomationParamList = new ObservableCollection<object>();
                    AutomationParamList.Add(GetAutoMationParam("state", "off"));
                    break;
                case "On":
                    AutomationParamList = new ObservableCollection<object>();
                    AutomationParamList.Add(GetAutoMationParam("state", "on"));
                    break;
                case "Change":
                    AutomationParamList = new ObservableCollection<object>();
                    AutomationParamList.Add(GetAutoMationParam("color", "#ffff53c9"));
                    break;
            }






            if (AssemblyHelper.CreateInternalInstance($"View.{"TargetDeviceSelectionWindow"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }

        private void OpenSpotMapWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"SpotMapViewWindow"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.Show();
            }
        }


        private void SetCurrentActionTypeForSelectedAction(ActionType actionType)
        {

            CurrentSelectedAction.ActionType = actionType;
            //reset the param too
            CurrentSelectedAction.ActionParameter = new ActionParameter { Name = "Thuộc tính", Type = "unknown", Value = "none" };
            RaisePropertyChanged(nameof(CurrentSelectedAction.ActionType));
        }
        private void getTextForActionType(string type, out string linkTxt, out string resultTxt)
        {
            linkTxt = string.Empty;
            resultTxt = string.Empty;
            switch (type)
            {
                case "Activate":
                    linkTxt = "Cho thiết bị";
                    resultTxt = "";
                    break;
                case "Increase":
                    linkTxt = "Của thiết bị";
                    resultTxt = "";//could add more param
                    break;
                case "Decrease":
                    linkTxt = "Của thiết bị";
                    resultTxt = "";
                    break;
                case "On":
                    linkTxt = "Của thiết bị";
                    resultTxt = "";
                    break;
                case "Off":
                    linkTxt = "Của thiết bị";
                    resultTxt = "";
                    break;
                case "Change":
                    linkTxt = "Của thiết bị";
                    resultTxt = "thành";
                    break;
            }
        }
        private void AddSelectedActionTypeToList(ActionType actionType)
        {
            IActionSettings newBlankAction = new ActionSettings {
                ActionType = actionType,
                ActionParameter = new ActionParameter { Name = "Thuộc tính", Type = "unknown", Value = "none" },
            };
            string lnkText = string.Empty;
            string resultText = string.Empty;
            getTextForActionType(actionType.Type, out lnkText, out resultText);
            newBlankAction.ActionType.LinkText = lnkText;
            newBlankAction.ActionType.ToResultText = resultText;
            if (CurrentSelectedAutomation.Actions == null)
            {
                CurrentSelectedAutomation.Actions = new ObservableCollection<IActionSettings>();
            }
            CurrentSelectedAutomation.Actions.Add(newBlankAction);
            RaisePropertyChanged(nameof(CurrentSelectedAutomation.Actions));
        }
        private void DeleteSelectedActionFromList(IActionSettings action)
        {
            CurrentSelectedAutomation.Actions.Remove(action);
            RaisePropertyChanged(nameof(CurrentSelectedAutomation.Actions));
        }
        private void SetCurrentActionTargetDeviceForSelectedAction(IDeviceSettings targetDevice)
        {
            CurrentSelectedAction.TargetDeviceUID = targetDevice.DeviceUID;
            CurrentSelectedAction.TargetDeviceName = targetDevice.DeviceName;
            //after this step, the parameter has to be reset because the profile UID will return invalid profile for new device
            if (CurrentSelectedAction.ActionType.Type == "Activate")
                CurrentSelectedAction.ActionParameter = new ActionParameter { Name = "Thuộc tính", Type = "unknown", Value = "none" };
            RaisePropertyChanged(nameof(CurrentSelectedAction.ActionType));
        }
        private void SetCurrentActionParamForSelectedAction(IActionParameter param)
        {
            CurrentSelectedAction.ActionParameter = param;
            RaisePropertyChanged(nameof(CurrentSelectedAction.ActionType));
        }
        private void SetCurrentSelectedActionTypeColorValue(Color color)
        {
            CurrentSelectedAction.ActionParameter.Value = color;
            RaisePropertyChanged(nameof(CurrentSelectedAction.ActionParameter.Value));
        }
        private ObservableCollection<StoreCategory> _availableStoreCategories;
        public ObservableCollection<StoreCategory> AvailableStoreCategories {
            get { return _availableStoreCategories; }
            set { _availableStoreCategories = value; RaisePropertyChanged(); }
        }
        private StoreCategory _currentSelectedCategory;
        public StoreCategory CurrentSelectedCategory {
            get { return _currentSelectedCategory; }
            set
            {
                //clear collection
                if (AvailableOnlineItems != null)
                    AvailableOnlineItems.Clear();
                _currentSelectedCategory = value;
                RaisePropertyChanged();
                CurrentOnlineStoreView = "Collections";
                CarouselImageLoading = true;
                Task.Run(() => UpdateStoreView());


            }
        }
        private ObservableCollection<BitmapImage> _availableCarouselImage;
        public ObservableCollection<BitmapImage> AvailableCarouselImage {
            get { return _availableCarouselImage; }
            set { _availableCarouselImage = value; RaisePropertyChanged(); }
        }
        private bool _carouselImageLoading;
        public bool CarouselImageLoading {
            get { return _carouselImageLoading; }
            set { _carouselImageLoading = value; RaisePropertyChanged(); }
        }
        private ObservableCollection<IOnlineItemModel> _availableOnlineItems;
        public ObservableCollection<IOnlineItemModel> AvailableOnlineItems {
            get { return _availableOnlineItems; }
            set { _availableOnlineItems = value; RaisePropertyChanged(); }
        }
        public AmbinoOnlineStoreView StoreWindow { get; set; }
        private void DownloadSelectedPalette(ColorPalette selectedPalette)
        {
            //find the palette with the same name
            var sameNamePalette = AvailablePallete.Where(p => p.Name == selectedPalette.Name).FirstOrDefault();
            if (CheckEqualityObjects(selectedPalette, sameNamePalette))
                HandyControl.Controls.MessageBox.Show("ColorPaletteExisted", "File Existed", MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
                AvailablePallete.Add(selectedPalette);
                //save to local disk
                var paletteJson = JsonConvert.SerializeObject(selectedPalette, new JsonSerializerSettings() {
                    TypeNameHandling = TypeNameHandling.Auto
                });
                File.WriteAllText(Path.Combine(PalettesCollectionFolderPath, selectedPalette.Name + ".col"), paletteJson);

            }



        }
        private async void UpdateStoreView()
        {

            switch (CurrentSelectedCategory.Type)
            {
                case "Palette":
                    await GetStoreItem(paletteFolderpath);
                    CarouselImageLoading = false;
                    break;
                case "Pattern":
                    await GetStoreItem(chasingPatternsFolderPath);
                    CarouselImageLoading = false;
                    break;
                case "Gifxelation":
                    await GetStoreItem(gifxelationsFolderPath);
                    CarouselImageLoading = false;
                    break;
                case "LEDSetup":
                    await GetStoreItem(outputLEDSetupsFolderPath);
                    CarouselImageLoading = false;
                    break;


            }

        }
        private bool CheckEqualityObjects(object object1, object object2)
        {
            string object1String = JsonConvert.SerializeObject(object1, new JsonSerializerSettings() {
                TypeNameHandling = TypeNameHandling.Auto
            });
            string object2String = JsonConvert.SerializeObject(object2, new JsonSerializerSettings() {
                TypeNameHandling = TypeNameHandling.Auto
            });
            return (string.Equals(object2String, object1String));
        }
        private const string paletteFolderpath = "/home/adrilight_enduser/ftp/files/ColorPalettes";
        private const string chasingPatternsFolderPath = "/home/adrilight_enduser/ftp/files/ChasingPatterns";
        private const string gifxelationsFolderPath = "/home/adrilight_enduser/ftp/files/Gifxelations";
        private const string outputLEDSetupsFolderPath = "/home/adrilight_enduser/ftp/files/OutputLEDSetups";

        private async Task GetStoreItem(string itemFolderPath)
        {
            AvailableOnlineItems = new ObservableCollection<IOnlineItemModel>();

            //get carousel image
            if (FTPHlprs == null)
            {
                string host = @"103.148.57.184";
                string userName = "adrilight_publicuser";
                string password = @"@drilightPublic";
                FTPHlprs = new FTPServerHelpers();
                FTPHlprs.sFTP = new SftpClient(host, 1512, userName, password);


            }
            if (!FTPHlprs.sFTP.IsConnected)
            {
                try
                {
                    FTPHlprs.sFTP.Connect();
                }
                catch (Exception ex)
                {
                    HandyControl.Controls.MessageBox.Show("Server không khả dụng ở thời điểm hiện tại, vui lòng thử lại sau", "Server notfound", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            //get all available files
            var listItemAddress = await FTPHlprs.GetAllFilesAddressInFolder(itemFolderPath);
            //display all available files to the view 
            if (listItemAddress.Count > 0)
            {
                foreach (var address in listItemAddress)
                {

                    //var item = FTPHlprs.GetFiles<T>(address);
                    var thumbPath = address + "/thumb.png";
                    var infoPath = address + "/info.json";
                    var thumb = FTPHlprs.GetThumb(address + "/thumb.png").Result;
                    var info = FTPHlprs.GetFiles<OnlineItemModel>(infoPath).Result;
                    info.Path = address;

                    info.Thumb = thumb;
                    await System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
                    {

                        AvailableOnlineItems.Add(info);

                    });

                }
            }
            else
            {
                HandyControl.Controls.MessageBox.Show("Không có item nào cho mục này, vui lòng thử lại sau", "Item notfound", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }



        }
        private string _currentOnlineStoreView;
        public string CurrentOnlineStoreView {
            get { return _currentOnlineStoreView; }
            set { _currentOnlineStoreView = value; RaisePropertyChanged(); }
        }
        private void OpenAmbinoStoreWindow()
        {
            CurrentOnlineStoreView = "Collections";
            StoreWindow = new AmbinoOnlineStoreView();
            StoreWindow.Owner = System.Windows.Application.Current.MainWindow;
            StoreWindow.Show();
            if (AvailableStoreCategories == null)
            {
                AvailableStoreCategories = new ObservableCollection<StoreCategory>();
                var palettes = new StoreCategory() {
                    Name = "Color Palettes",
                    Type = "Palette",
                    Description = "All Color Palette created by Ambino and Contributed by Ambino Community",
                    Geometry = "colorpalette"
                };
                var gif = new StoreCategory() {
                    Name = "Gifxelations",
                    Type = "Gif",
                    Description = "All Color Palette created by Ambino and Contributed by Ambino Community",
                    Geometry = "colorpalette"
                };
                var gradient = new StoreCategory() {
                    Name = "Gradients",
                    Type = "Gradient",
                    Description = "All Color Palette created by Ambino and Contributed by Ambino Community",
                    Geometry = "colorpalette"
                };
                var chasingPatterns = new StoreCategory() {
                    Name = "Chasing Patterns",
                    Type = "Pattern",
                    Description = "All Color Palette created by Ambino and Contributed by Ambino Community",
                    Geometry = "colorpalette"
                };
                var outputLEDSetup = new StoreCategory() {
                    Name = "LED Setups",
                    Type = "LEDSetup",
                    Description = "All Color Palette created by Ambino and Contributed by Ambino Community",
                    Geometry = "colorpalette"
                };

                AvailableStoreCategories.Add(palettes);
                AvailableStoreCategories.Add(gif);
                AvailableStoreCategories.Add(gradient);
                AvailableStoreCategories.Add(chasingPatterns);
                AvailableStoreCategories.Add(outputLEDSetup);
            }

            AvailableCarouselImage = new ObservableCollection<BitmapImage>();


        }


        private void OpenAutomationManagerWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"AutomationManagerWindow"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }

        private void OpenOutputDataImportSelection()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"OutputDataImportSelection"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }

        private void OpenHardwareMonitorWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"ComputerHardwareInformationWindow"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }

        private void OpenAddNewGradientWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"GradientEditWindow"}") is System.Windows.Window window)
            {
                CurrentStartColor = CurrentSelectedGradient.StartColor;
                CurrentStopColor = CurrentSelectedGradient.StopColor;
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }

        private void OpenAddNewAutomationWindowCommand()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"AddNewAutomationWindow"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }

        private void OpenAppSettingsWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"AppSettingsWindow"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }

        private void LaunchWBAdjustWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"CustomWBWindow"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }



        private void ImportProfile()
        {
            OpenFileDialog Import = new OpenFileDialog();
            Import.Title = "Chọn Profile";
            Import.CheckFileExists = true;
            Import.CheckPathExists = true;
            Import.DefaultExt = "Pro";
            Import.Filter = "Text files (*.Pro)|*.Pro";
            Import.FilterIndex = 2;

            Import.ShowDialog();

            if (!string.IsNullOrEmpty(Import.FileName) && File.Exists(Import.FileName))
            {
                var json = File.ReadAllText(Import.FileName);

                var existedprofile = JsonConvert.DeserializeObject<DeviceProfile>(json, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });

                //ignore existed UID for posibility of conflicting with old UID
                if (existedprofile != null)
                {
                    try
                    {
                        existedprofile.ProfileUID = Guid.NewGuid().ToString();
                        AvailableProfiles.Add(existedprofile);
                        RaisePropertyChanged(nameof(AvailableProfiles));
                        WriteDeviceProfileCollection();
                        AvailableProfilesForCurrentDevice.Clear();
                        AvailableProfilesForCurrentDevice = ProfileFilter(CurrentDevice);
                    }
                    catch (Exception ex)
                    {
                        HandyControl.Controls.MessageBox.Show("Profile Corrupted or incompatible!!", "File Corrupted", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    HandyControl.Controls.MessageBox.Show("Profile Corrupted", "File Corrupted", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveNewGradient()
        {
            var gradient = new GradientColorCard("new gradient", "user", "unknown", "User created", CurrentStartColor, CurrentStopColor);
            AvailableGradient.Add(gradient);
            WriteGradientCollectionJson();
        }

        public void SaveCurrentProfile(string profileUID)
        {
            var currentProfile = AvailableProfiles.Where(p => p.ProfileUID == profileUID).FirstOrDefault();
            if (currentProfile != null)
                // currentProfile.SaveProfile(CurrentDevice.AvailableOutputs);

                WriteDeviceProfileCollection();

            //Growl.Success("Profile saved successfully!");
            IsSettingsUnsaved = BadgeStatus.Dot;
        }
        public void DeleteAttachedProfile(IDeviceProfile profile)
        {
            //check if profile is in used
            if (profile.ProfileUID == CurrentDevice.ActivatedProfileUID)
            {
                HandyControl.Controls.MessageBox.Show(profile.Name + " Không thể xóa, profile này đang được sử dụng!!!", "Profile is in used", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                AvailableProfilesForCurrentDevice.Remove(profile);
                AvailableProfiles.Remove(profile);

                WriteDeviceProfileCollection();
            }

        }

        private UpdateInfo _availableUpdates;
        public UpdateInfo AvailableUpdates {
            get { return _availableUpdates; }
            set
            {
                _availableUpdates = value;
                RaisePropertyChanged();
            }
        }
        private bool _isCheckingForUpdate = false;
        public bool IsCheckingForUpdate {
            get { return _isCheckingForUpdate; }
            set
            {
                _isCheckingForUpdate = value;
                RaisePropertyChanged();
            }
        }
        private bool _updateButtonEnable = true;
        public bool UpdateButtonEnable {
            get { return _updateButtonEnable; }
            set
            {
                _updateButtonEnable = value;
                RaisePropertyChanged();
            }
        }
        private string _updateErrorMessage = "";
        public string UpdateErrorMessage {
            get { return _updateErrorMessage; }
            set
            {
                _updateErrorMessage = value;
                RaisePropertyChanged();
            }
        }
        private Task<UpdateManager> mgr;
        private SplashScreen _splashScreen;


        private async Task CheckForUpdate()
        {

            UpdateErrorMessage = "";
            try
            {
                mgr = UpdateManager.GitHubUpdateManager(ADRILIGHT_RELEASES);
                using (var result = await mgr)
                {
                    AvailableUpdates = await mgr.Result.CheckForUpdate();
                    RaisePropertyChanged(nameof(AvailableUpdates));
                }
            }
            catch (Exception ex)
            {
                UpdateErrorMessage = ex.Message + "\n" + "Kiểm tra lại mạng hoặc liên hệ Ambino để được hỗ trợ";
            }

            //check once a day for updates


        }
        private async Task UpdateApp()
        {

            await System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {
                _splashScreen = new View.SplashScreen();
                _splashScreen.Header.Text = "Downloading Update";
                _splashScreen.status.Text = "UPDATING...";
                _splashScreen.Show();

            });


            // this.logger.Info("Downloading updates");
            var releaseEntry = await mgr.Result.UpdateApp();

            if (releaseEntry != null)
            {

                //restart adrilight if an update was installed
                //dispose locked WinRing0 file first
                //if (HWMonitor != null)
                //    HWMonitor.Dispose();
                if (AmbinityClient != null)
                    AmbinityClient.Dispose();
                //remember to dispose openrgbstream too!!!
                await System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    _splashScreen.status.Text = "RESTARTING...";
                });
                UpdateManager.RestartApp();
            }
            //this.logger.Info($"Download complete. Version {updateResult.Version} will take effect when App is restarted.");



        }


        //private void SaveCurrentSelectedAutomation()
        //{
        //    CurrentSelectedAutomation.Modifiers = new List<NonInvasiveKeyboardHookLibrary.ModifierKeys>();
        //    foreach (var modifier in AvailableModifiers)
        //    {
        //        if (modifier.IsChecked)
        //        {
        //            CurrentSelectedAutomation.Modifiers.Add(modifier);
        //        }
        //    }

        //    WriteAutomationCollectionJson();
        //    AvailableAutomations = new ObservableCollection<IAutomationSettings>();
        //    foreach (var automation in LoadAutomationIfExist())
        //    {
        //        AvailableAutomations.Add(automation);
        //    }
        //    if (GeneralSettings.HotkeyEnable)
        //    {
        //        Unregister();
        //        Register();
        //    }
        //}

        private void SaveAllAutomation()
        {
            WriteAutomationCollectionJson();
            AvailableAutomations = new ObservableCollection<IAutomationSettings>();
            foreach (var automation in LoadAutomationIfExist())
            {
                AvailableAutomations.Add(automation);
            }
            if (GeneralSettings.HotkeyEnable)
            {
                Unregister();
                Register();
            }
        }
        private NonInvasiveKeyboardHookLibrary.ModifierKeys ConvertStringtoModifier(string key)
        {
            NonInvasiveKeyboardHookLibrary.ModifierKeys returnKey = NonInvasiveKeyboardHookLibrary.ModifierKeys.WindowsKey;
            switch (key)
            {
                case "Shift":
                    returnKey = NonInvasiveKeyboardHookLibrary.ModifierKeys.Shift;
                    break;
                case "Ctrl":
                    returnKey = NonInvasiveKeyboardHookLibrary.ModifierKeys.Control;
                    break;
                case "Alt":
                    returnKey = NonInvasiveKeyboardHookLibrary.ModifierKeys.Alt;
                    break;

            }
            return returnKey;
        }

        private void SaveCurrentSelectedAutomationShortkey()
        {
            var modifiers = CurrentSelectedModifiers.ToArray();
            var key = CurrentSelectedShortKeys.ToArray();
            CurrentSelectedAutomation.Modifiers = new ObservableCollection<NonInvasiveKeyboardHookLibrary.ModifierKeys>();
            foreach (var modifier in modifiers)
            {

                CurrentSelectedAutomation.Modifiers.Add(ConvertStringtoModifier(modifier));

            }
            RaisePropertyChanged(nameof(CurrentSelectedAutomation.Modifiers));
            CurrentSelectedAutomation.StandardKey = key[0];
            WriteAutomationCollectionJson();
            //AvailableAutomations = new ObservableCollection<IAutomationSettings>();
            //foreach (var automation in LoadAutomationIfExist())
            //{
            //    AvailableAutomations.Add(automation);
            //}
            if (GeneralSettings.HotkeyEnable)
            {
                Unregister();
                Register();
            }
        }
        private void SetCurrentSelectedOutputForCurrentSelectedOutput()
        {
            // CurrentDevice.AvailableOutputs[0].IsInSpotEditWizard = true;
            CurrentDevice.SetOutput(SelectedOutputForCurrentDevice, CurrentOutput.OutputID);
            //CurrentDevice.IsSizeNeedUserDefine = false;
            // CurrentDevice.AvailableOutputs[0].IsInSpotEditWizard = false;
            //GotoChild(CurrentDevice);
        }
        //private void ExportCurrentColorEffect()
        //{
        //    SaveFileDialog Export = new SaveFileDialog();
        //    Export.CreatePrompt = true;
        //    Export.OverwritePrompt = true;

        //    Export.Title = "Xuất dữ liệu";
        //    Export.FileName = NewEffectName;
        //    Export.CheckFileExists = false;
        //    Export.CheckPathExists = true;
        //    Export.DefaultExt = "ACE";
        //    Export.Filter = "All files (*.*)|*.*";
        //    Export.InitialDirectory =
        //    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        //    Export.RestoreDirectory = true;
        //    //init new effect
        //    var listLEDSetup = new List<ILEDSetup>();

        //    foreach (var output in CurrentDevice.AvailableOutputs)
        //    {
        //        listLEDSetup.Add(output.OutputLEDSetup);
        //    }

        //    IAmbinoColorEffect effect = new AmbinoColorEffect {
        //        Name = NewEffectName,
        //        TargetType = CurrentDevice.DeviceType,
        //        ColorPalette = CurrentActivePalette,
        //        Description = _newEffectDescription,
        //        EffectVersion = "1.0.0",
        //        OutputLEDSetup = listLEDSetup.ToArray(),
        //    };

        //    var coloreffectjson = JsonConvert.SerializeObject(effect, new JsonSerializerSettings() {
        //        TypeNameHandling = TypeNameHandling.Auto
        //    });

        //    if (Export.ShowDialog() == DialogResult.OK)
        //    {
        //        Directory.CreateDirectory(JsonPath);
        //        File.WriteAllText(Export.FileName, coloreffectjson);
        //        Growl.Success("Profile exported successfully!");
        //    }
        //}

        private void ExportCurrentProfile(IDeviceProfile profile)
        {
            SaveFileDialog Export = new SaveFileDialog();
            Export.CreatePrompt = true;
            Export.OverwritePrompt = true;

            Export.Title = "Xuất dữ liệu";
            Export.FileName = profile.Name;
            Export.CheckFileExists = false;
            Export.CheckPathExists = true;
            Export.DefaultExt = "Pro";
            Export.Filter = "All files (*.*)|*.*";
            Export.InitialDirectory =
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            Export.RestoreDirectory = true;

            var profilejson = JsonConvert.SerializeObject(profile, new JsonSerializerSettings() {
                TypeNameHandling = TypeNameHandling.Auto
            });

            if (Export.ShowDialog() == DialogResult.OK)
            {
                Directory.CreateDirectory(JsonPath);
                File.WriteAllText(Export.FileName, profilejson);
                Growl.Success("Profile exported successfully!");
            }
        }

        private void LoadProfile(IDeviceProfile profileToLoad) // change to currentDevice.LoadProfile
        {
            GeneralSettings.IsProfileLoading = true;
            CurrentDevice.ActivateProfile(profileToLoad);

            //WriteDeviceInfoJson();

            //foreach (var output in CurrentDevice.AvailableOutputs)
            //{
            //    output.OutputIsBuildingLEDSetup = true;

            //}
            GeneralSettings.IsProfileLoading = false;
        }

        private void CreateNewProfile()
        {
            var newprofile = new DeviceProfile {
                Name = NewProfileName,
                Description = NewProfileDescription,
                Owner = NewProfileOwner,
                DeviceType = CurrentDevice.DeviceType,
                Geometry = "profile",
                ProfileUID = Guid.NewGuid().ToString(),
            };
            // newprofile.SaveProfile(CurrentDevice.AvailableOutputs);
            AvailableProfiles.Add(newprofile);

            WriteDeviceProfileCollection();
            AvailableProfiles.Clear();
            foreach (var profile in LoadDeviceProfileIfExist())
            {
                AvailableProfiles.Add(profile);
            }

            AvailableProfilesForCurrentDevice.Clear();
            AvailableProfilesForCurrentDevice = ProfileFilter(CurrentDevice);
            RaisePropertyChanged(nameof(AvailableProfiles));
            RaisePropertyChanged(nameof(AvailableProfilesForCurrentDevice));
        }

        private void OpenCreatenewProfileWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"AddNewProfileWindow"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }




        public void Register()
        {
            //_identifiers = new List<Guid?>();
            foreach (var automation in AvailableAutomations.Where(x => x.IsEnabled == true))
            {
                var modifierkeys = new List<NonInvasiveKeyboardHookLibrary.ModifierKeys>();
                if (automation.Modifiers != null)
                {
                    foreach (var modifier in automation.Modifiers)
                    {

                        modifierkeys.Add(modifier);
                    }
                }

                try
                {


                    switch (modifierkeys.Count)
                    {
                        case 0:
                            KeyboardHookManagerSingleton.Instance.RegisterHotkey(automation.StandardKey.KeyCode, () =>
                            {
                                Task.Run(() =>
                                   {
                                       ExecuteAutomationActions(automation.Actions);
                                   });

                                Debug.WriteLine(automation.Name + " excuted");
                                if (GeneralSettings.NotificationEnabled)
                                    SendNotification(automation.Name);
                            });
                            //_identifiers.Add(_identifier);
                            break;

                        case 1:
                            KeyboardHookManagerSingleton.Instance.RegisterHotkey(modifierkeys.First(), automation.StandardKey.KeyCode, () =>
                            {
                                Task.Run(() =>
                                {
                                    ExecuteAutomationActions(automation.Actions);
                                });
                                Debug.WriteLine(automation.Name + " excuted");
                                if (GeneralSettings.NotificationEnabled)
                                    SendNotification(automation.Name);
                            });
                            //_identifiers.Add(_identifier);
                            break;

                        default:
                            KeyboardHookManagerSingleton.Instance.RegisterHotkey(modifierkeys.ToArray(), automation.StandardKey.KeyCode, () =>
                            {
                                Task.Run(() =>
                                {
                                    ExecuteAutomationActions(automation.Actions);
                                });
                                Debug.WriteLine(automation.Name + " excuted");

                            });
                            //_identifiers.Add(_identifier);
                            break;
                    }
                }
                catch (NonInvasiveKeyboardHookLibrary.HotkeyAlreadyRegisteredException ex)
                {
                    HandyControl.Controls.MessageBox.Show(automation.Name + " Hotkey is being used by another automation!!!", "HotKey Already Registered", MessageBoxButton.OK, MessageBoxImage.Error);
                    //disable automation
                    automation.IsEnabled = false;
                    WriteAutomationCollectionJson();
                }
                catch (Exception ex)
                {

                }
            }
        }

        private void ExecuteAutomationActions(ObservableCollection<IActionSettings> actions)
        {
            if (actions == null)
                return;
            foreach (var action in actions)
            {
                var targetDevice = AvailableDevices.Where(x => x.DeviceUID == action.TargetDeviceUID).FirstOrDefault();
                if (targetDevice == null)
                {
                    HandyControl.Controls.MessageBox.Show(action.TargetDeviceName + " Không thể thiết lập automation, thiết bị đã bị xóa hoặc thay đổi UID!!!", "Device is not available", MessageBoxButton.OK, MessageBoxImage.Error);
                    actions.Remove(action);
                    WriteAutomationCollectionJson();
                    return;
                }
                switch (action.ActionType.Type)
                {
                    case "Activate":
                        var destinationProfile = AvailableProfiles.Where(x => x.ProfileUID == (string)action.ActionParameter.Value).FirstOrDefault();
                        if (destinationProfile != null)
                        {
                            targetDevice.ActivateProfile(destinationProfile);
                            //at this moment, selected profile for the target device changed but the UI know nothing about this
                            //because the porperty changed event didnt fire
                            //so we fire profile UID changed instead
                            targetDevice.ActivatedProfileUID = destinationProfile.ProfileUID;
                        }

                        break;

                    case "Increase":
                        switch (action.ActionParameter.Type)
                        {
                            case "brightness":
                                targetDevice.BrightnessUp(10);
                                break;
                            case "speed":
                                targetDevice.SpeedUp(10);
                                break;
                        }
                        break;
                    case "Decrease":
                        switch (action.ActionParameter.Type)
                        {
                            case "brightness":
                                targetDevice.BrightnessDown(10);
                                break;
                            case "speed":
                                targetDevice.SpeedDown(10);
                                break;
                        }
                        break;


                    case "Off":
                        // just turn off all leds for now
                        targetDevice.IsEnabled = false;
                        break;
                    case "On":
                        targetDevice.IsEnabled = true;
                        break;
                    case "Change":
                        //just change solid color and activate static mode
                        switch (action.ActionParameter.Type)
                        {
                            case "color":
                                //foreach (var output in targetDevice.AvailableOutputs)
                                //{
                                //    //output.OutputStaticColor = (Color)ColorConverter.ConvertFromString((string)action.ActionParameter.Value);
                                //    //output.OutputSelectedMode = 3;
                                //}

                                break;
                        }

                        break;

                }
            }
        }

        private MainView _mainForm;

        private void MainForm_FormClosed(object sender, EventArgs e)
        {
            if (_mainForm == null) return;

            //deregister to avoid memory leak
            _mainForm.Closed -= MainForm_FormClosed;
            _mainForm = null;
        }

        private void SendNotification(string content)
        {
            NotifyIcon.ShowBalloonTip("Ambinity | Automation excuted", content, NotifyIconInfoType.Info, "Ambinity");
        }

        private void Unregister()
        {
            KeyboardHookManagerSingleton.Instance.UnregisterAll();
        }

        private int _fwUploadPercent;

        public int FwUploadPercent {
            get { return _fwUploadPercent; }
            set
            {
                _fwUploadPercent = value;
                RaisePropertyChanged();
            }
        }

        private bool _fwUploadPercentVissible = false;

        public bool FwUploadPercentVissible {
            get { return _fwUploadPercentVissible; }
            set
            {
                _fwUploadPercentVissible = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<ITimeLineDataItem> _availableMotions;
        public ObservableCollection<ITimeLineDataItem> AvailableMotions {
            get { return _availableMotions; }
            set
            {
                _availableMotions = value;
                RaisePropertyChanged();
            }
        }

        private bool _reloadDeviceLoadingVissible = false;

        public bool ReloadDeviceLoadingVissible {
            get { return _reloadDeviceLoadingVissible; }
            set
            {
                _reloadDeviceLoadingVissible = value;
                RaisePropertyChanged();
            }
        }
        private bool _effectLoadingVissible = false;

        public bool EffectLoadingVissible {
            get { return _effectLoadingVissible; }
            set
            {
                _effectLoadingVissible = value;
                RaisePropertyChanged();
            }
        }
        private string _fwUploadOutputLog;

        public string FwUploadOutputLog {
            get { return _fwUploadOutputLog; }

            set
            {
                _fwUploadOutputLog = value;
                RaisePropertyChanged();
            }
        }

        private void UpgradeIfAvailable(IDeviceSettings device)
        {
            // set device to DFU first,
            // CurrentDevice.State = State.DFU;

            // CurrentDevice.TransferActive = false;
            // switch deviceProcessorType
            // case ch55x
            // Check if there is new version come with this app
            // Coppy corresponding firmware file for current device to firmware folder
            // get the file path
            // upload with CMD.exe
            //disable DeviceDiscovery first
            GeneralSettings.FrimwareUpgradeIsInProgress = true;
            if (device.DeviceType == "ABHUBV2")
            {
                MessageBoxResult result = HandyControl.Controls.MessageBox.Show("HUBV2 cần sử dụng FlyMCU để nạp firmware, nhấn [OK] để vào chế độ DFU", "External Software Required", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    device.CurrentState = State.dfu;

                    Thread.Sleep(5000);
                    device.CurrentState = State.normal;
                    HandyControl.Controls.MessageBox.Show("Đã gửi thông tin đến Device, mở FlyMCU để tiếp tục nạp firmware sau đó bật lại kết nối", "DFU", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                if (device.HardwareVersion == "unknown") // old firmware or not supported
                {
                    // show message box : unknown hardware version, please update firmware manually by chosing one of these firmware file in the list below
                    MessageBoxResult result = HandyControl.Controls.MessageBox.Show("Thiết bị đang ở firmware cũ hoặc phần cứng không hỗ trợ! bạn có muốn chọn firmware để cập nhật không?", "Unknown hardware version", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        //grab available firmware for current device type
                        var json = File.ReadAllText(JsonFWToolsFWListFileNameAndPath);
                        var availableFirmware = JsonConvert.DeserializeObject<List<DeviceFirmware>>(json, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
                        AvailableFirmwareForCurrentDevice = new ObservableCollection<IDeviceFirmware>();
                        foreach (var firmware in availableFirmware)
                        {
                            if (firmware.TargetDeviceType == device.DeviceType)
                                AvailableFirmwareForCurrentDevice.Add(firmware);
                        }

                        // show list selected firmware
                        OpenFirmwareSelectionWindow();
                    }
                }
                else
                {
                    // regconize this device, find the compatible firmware
                    var json = File.ReadAllText(JsonFWToolsFWListFileNameAndPath);
                    var requiredFwVersion = JsonConvert.DeserializeObject<List<DeviceFirmware>>(json, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });

                    var currentDeviceFirmwareInfo = requiredFwVersion.Where(p => p.TargetHardware == device.HardwareVersion).FirstOrDefault();
                    if (currentDeviceFirmwareInfo == null)
                    {
                        //not supported hardware

                        var result = HandyControl.Controls.MessageBox.Show("Phần cứng không còn được hỗ trợ hoặc không nhận ra: " + device.HardwareVersion + " Bạn có muốn chọn phần cứng được hỗ trợ?", "Firmware uploading", MessageBoxButton.YesNo, MessageBoxImage.Error);
                        if (result == MessageBoxResult.Yes)
                        {
                            var fwjson = File.ReadAllText(JsonFWToolsFWListFileNameAndPath);
                            var availableFirmware = JsonConvert.DeserializeObject<List<DeviceFirmware>>(fwjson, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
                            AvailableFirmwareForCurrentDevice = new ObservableCollection<IDeviceFirmware>();
                            foreach (var firmware in availableFirmware)
                            {
                                if (firmware.TargetDeviceType == device.DeviceType)
                                    AvailableFirmwareForCurrentDevice.Add(firmware);
                            }

                            // show list selected firmware
                            OpenFirmwareSelectionWindow();
                        }
                    }
                    else
                    {
                        if (device.FirmwareVersion != currentDeviceFirmwareInfo.Version)
                        {
                            //coppy hex file to FWTools folder
                            var fwOutputLocation = Path.Combine(JsonFWToolsFileNameAndPath, currentDeviceFirmwareInfo.Name);
                            try
                            {
                                ResourceHlprs.CopyResource(currentDeviceFirmwareInfo.ResourceName, fwOutputLocation);
                            }
                            catch (ArgumentException)
                            {
                                //show messagebox no firmware found for this device
                                return;
                            }

                            //check if specific driver for CH375 is instaled on this computer
                            if (GeneralSettings.DriverRequested)
                            {
                                //coppy resource
                                ResourceHlprs.CopyResource("adrilight.Tools.FWTools.CH372DRV.EXE", Path.Combine(JsonFWToolsFileNameAndPath, "CH372DRV.EXE"));
                                //launch driver installer
                                System.Diagnostics.Process.Start(Path.Combine(JsonFWToolsFileNameAndPath, "CH372DRV.EXE"));
                                GeneralSettings.DriverRequested = false;
                                return;
                            }
                            else
                            {
                                //put device in dfu state
                                device.CurrentState = State.dfu;
                                // wait for device to enter dfu
                                Thread.Sleep(1000);
                                FwUploadPercentVissible = true;
                                var startInfo = new System.Diagnostics.ProcessStartInfo {
                                    WorkingDirectory = JsonFWToolsFileNameAndPath,
                                    RedirectStandardOutput = true,
                                    RedirectStandardError = true,
                                    CreateNoWindow = true,
                                    UseShellExecute = false,
                                    FileName = "cmd.exe",
                                    Arguments = "/C vnproch55x " + fwOutputLocation
                                };
                                var proc = new Process() {
                                    StartInfo = startInfo,
                                    EnableRaisingEvents = true
                                };

                                // see below for output handler
                                proc.ErrorDataReceived += proc_DataReceived;
                                proc.OutputDataReceived += proc_DataReceived;

                                proc.Start();

                                proc.BeginErrorReadLine();
                                proc.BeginOutputReadLine();
                                proc.Exited += proc_FinishUploading;
                            }
                        }
                        else
                        {
                            HandyControl.Controls.MessageBox.Show("Không có phiên bản mới cho thiết bị này", "Firmware uploading", MessageBoxButton.OK, MessageBoxImage.Information);
                            GeneralSettings.FrimwareUpgradeIsInProgress = false;
                        }
                    }
                }
            }
        }

        private void proc_FinishUploading(object sender, System.EventArgs e)
        {
            //FwUploadPercent = 0;
            ////clear loading bar
            //FwUploadOutputLog = String.Empty;
            ////clear text box
            //percentCount = 0;
            ReloadDeviceLoadingVissible = true;

            Thread.Sleep(5000);
            CurrentDevice.CurrentState = State.normal;

            if (FwUploadOutputLog.Split('\n').Last() == "Found no CH55x USB")
            {
                //there is a chance of missing driver so first we install CH375 driver first
                //execute CH375 driver

                //try to restart uploading by resetting the state

                HandyControl.Controls.MessageBox.Show("Update firmware không thành công, Không tìm thấy thiết bị ở trạng thái DFU", "Firmware uploading", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                // check for current device actual firmware version
                CurrentDevice.RefreshFirmwareVersion();
                // reset loading bar
                percentCount = 0;
                FwUploadPercent = 0;
                FwUploadPercentVissible = false;
                FwUploadOutputLog = string.Empty;

                HandyControl.Controls.MessageBox.Show("Update firmware thành công - Phiên bản : " + CurrentDevice.FirmwareVersion, "Firmware uploading", MessageBoxButton.OK, MessageBoxImage.Information);
                ReloadDeviceLoadingVissible = false;
                GeneralSettings.FrimwareUpgradeIsInProgress = false;
            }
        }

        private int percentCount = 0;

        private void proc_DataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                if (e.Data.Contains("[2K"))//clear current line
                {
                    percentCount++;
                    FwUploadPercent = percentCount * 100 / 308;

                    //Dispatcher.BeginInvoke(new Action(() => Prog.Value = percent));
                    //Dispatcher.BeginInvoke(new Action(() => Output.Text += (Environment.NewLine + percentCount)));
                    //Dispatcher.BeginInvoke(new Action(() => Output.Text += (e.Data)));
                }
                else
                {
                    FwUploadOutputLog += Environment.NewLine + e.Data;
                }
            }
        }

        private ILEDSetup _selectedLEDSetup;
        private void UnZone()
        {
            var currentZone = PIDEditWindowsRichCanvasSelectedItem as LEDSetup;
            foreach (var spot in currentZone.Spots)
            {
                (spot as DeviceSpot).Left += currentZone.Left;
                (spot as DeviceSpot).Top += currentZone.Top;
                PIDEditWindowsRichCanvasItems.Add(spot as DeviceSpot);
            }
            currentZone.IsSelected = false;
            PIDEditWindowsRichCanvasItems.Remove(currentZone);
        }
        private void AddNewZone()
        {
            var newZone = new LEDSetup();
            foreach (var spot in PIDEditWindowsRichCanvasItems.Where(s => s is DeviceSpot && s.IsSelected))
            {
                //add new ledsetup and add all spots
                newZone.Spots.Add(spot as DeviceSpot);


            }
            newZone.UpdateSizeByChild();
            foreach (var spot in newZone.Spots)
            {
                //spot.ScaleWidth = (spot as DeviceSpot).Width / newZone.Width;
                //spot.ScaleHeight = (spot as DeviceSpot).Height / newZone.Height;
                //spot.ScaleLeft = ((spot as DeviceSpot).Left - newZone.Left) / newBoundRect.Width;
                //spot.ScaleTop = ((spot as DeviceSpot).Top - newZone.Top) / newBoundRect.Height;
                (spot as DeviceSpot).Left -= newZone.Left;
                (spot as DeviceSpot).Top -= newZone.Top;
                //rebuild spot
                //(spot as DeviceSpot).RebuildSpot(newBoundRect.Width, newBoundRect.Height);
            }
            PIDEditWindowsRichCanvasItems.Add(newZone);
            foreach (var spot in newZone.Spots)
            {
                //add new ledsetup and add all spots
                (spot as IDrawable).IsSelected = false;
                PIDEditWindowsRichCanvasItems.Remove(spot as IDrawable);
            }


        }
        private void ImportPIDToCanvas()
        {
            OpenFileDialog Import = new OpenFileDialog();
            Import.Title = "Chọn LED Setup file";
            Import.CheckFileExists = true;
            Import.CheckPathExists = true;
            Import.DefaultExt = "json";
            Import.Filter = "Text files (*.json)|*.json";
            Import.FilterIndex = 2;

            Import.ShowDialog();

            if (!string.IsNullOrEmpty(Import.FileName) && File.Exists(Import.FileName))
            {
                var json = File.ReadAllText(Import.FileName);

                try
                {
                    var ledSetup = JsonConvert.DeserializeObject<LEDSetup>(json, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
                    foreach (var spot in ledSetup.Spots)
                    {
                        PIDEditWindowsRichCanvasItems.Add(spot as IDrawable);
                    }

                    //prompt selection
                    //OpenSpotDataSelectionWindow();
                }
                catch (Exception)
                {
                    HandyControl.Controls.MessageBox.Show("Corrupted or incompatible data File!!!", "LEDSetup Import", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        //private void ExportCurrentOutputPID()
        //{
        //    SaveFileDialog Export = new SaveFileDialog();
        //    Export.CreatePrompt = true;
        //    Export.OverwritePrompt = true;

        //    Export.Title = "Xuất dữ liệu";
        //    Export.FileName = CurrentOutput.OutputName + " LEDSetup";
        //    Export.CheckFileExists = false;
        //    Export.CheckPathExists = true;
        //    Export.InitialDirectory =
        //    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        //    Export.RestoreDirectory = true;

        //    var ledsetupJson = JsonConvert.SerializeObject(CurrentOutput.OutputLEDSetup, new JsonSerializerSettings() {
        //        TypeNameHandling = TypeNameHandling.Auto
        //    });

        //    if (Export.ShowDialog() == DialogResult.OK)
        //    {
        //        //create directory with same name
        //        var newFolder = Directory.CreateDirectory(Export.FileName);
        //        var contentFolder = Directory.CreateDirectory(Path.Combine(Export.FileName, "content")).ToString();
        //        //create main content 
        //        File.WriteAllText(Path.Combine(Export.FileName, "content", CurrentOutput.OutputLEDSetup.Name + ".json"), ledsetupJson);
        //        //create info
        //        var info = new OnlineItemModel() {
        //            Name = CurrentOutput.OutputLEDSetup.Name,
        //            Owner = CurrentOutput.OutputLEDSetup.Owner,
        //            Description = CurrentOutput.OutputLEDSetup.Description,
        //            Type = "LEDSetup",
        //            SubType = CurrentOutput.OutputLEDSetup.TargetType
        //        };
        //        var infoJson = JsonConvert.SerializeObject(info, new JsonSerializerSettings() {
        //            TypeNameHandling = TypeNameHandling.Auto
        //        });
        //        File.WriteAllText(Path.Combine(Export.FileName, "info.json"), infoJson);
        //        //create image, require user input later thumb.png???


        //    }
        //}

        //private void ProcessSelectedSpotsWithRange(int rangeMinValue, int rangeMaxValue)
        //{
        //    int counter = 0;

        //    foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
        //    {
        //        if (spot.BorderThickness != 0)//spots is selecteds
        //        {
        //            counter++;
        //        }
        //    }
        //    var spacing = (rangeMaxValue - rangeMinValue) / counter;
        //    if (spacing < 1)
        //        spacing = 1;
        //    int offset = rangeMinValue;
        //    foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
        //    {
        //        if (spot.BorderThickness != 0)//spots is selected
        //        {
        //            switch (SetIDMode)
        //            {
        //                case "VID":
        //                    spot.SetVID(offset);
        //                    offset += spacing;
        //                    spot.IsEnabled = true;
        //                    break;

        //                case "MID":
        //                    spot.SetMID(offset);
        //                    offset += spacing;
        //                    break;
        //            }
        //        }
        //    }
        //}

        //private void ProcessSelectedSpotsID(string mode, string dirrection)
        //{
        //    switch (mode)
        //    {
        //        case "VID":
        //            switch (dirrection)
        //            {
        //                case "lefttoright":
        //                    int min = RangeMinValue;
        //                    int max = RangeMaxValue;
        //                    var selectedSpots = new List<IDeviceSpot>();
        //                    foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
        //                    {
        //                        if (spot.BorderThickness != 0)//spots is selected
        //                        {
        //                            selectedSpots.Add(spot);
        //                        }
        //                    }
        //                    //re-arrange selectedSpots as X and Y to a list of increment order to set ID
        //                    //could implement drag move?
        //                    break;
        //            }

        //            break;

        //        case "MID":
        //            break;

        //        case "CID":
        //            break;
        //    }
        //}

        //private void ProcessSelectedSpots(string userInput)
        //{
        //    switch (SetIDMode)
        //    {
        //        case "VID":
        //            foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
        //            {
        //                if (spot.BorderThickness != 0)//spots is selected
        //                {
        //                    int mIDToSet = Int32.Parse(userInput);
        //                    if (mIDToSet > 1023)
        //                        mIDToSet = 0;
        //                    spot.SetVID(mIDToSet);
        //                }
        //            }
        //            break;

        //        case "MID":
        //            foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
        //            {
        //                if (spot.BorderThickness != 0)//spots is selected
        //                {
        //                    int mIDToSet = Int32.Parse(userInput);
        //                    if (mIDToSet > 1023)
        //                        mIDToSet = 0;
        //                    spot.SetMID(mIDToSet);
        //                }
        //            }
        //            break;

        //        case "CID":
        //            foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
        //            {
        //                int mIDToSet = Int32.Parse(userInput);
        //                if (mIDToSet > 32)
        //                    mIDToSet = 0;
        //                if (spot.BorderThickness != 0)//spots is selected
        //                {
        //                    spot.SetCID(mIDToSet);
        //                }
        //            }
        //            break;
        //    }
        //    RaisePropertyChanged(nameof(CurrentOutput.OutputLEDSetup));
        //}

        private void OpenFirmwareSelectionWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"FirmwareSelectionWindow"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }

        private void OpenSpotDataSelectionWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"SpotDataSelection"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }

        private void OpenDeviceConnectionSettingsWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"DeviceConnectionSettingsWindow"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }

        private void OpenDeviceFirmwareSettingsWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"DeviceFirmwareSettingsWindow"}") is System.Windows.Window window)
            {
                //reset progress and log display
                FwUploadPercentVissible = false;
                percentCount = 0;
                FwUploadPercent = 0;
                FwUploadOutputLog = String.Empty;
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }

        private void OpenNameEditWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"RenameItemWindow"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }

        private void OpenAdvanceSettingWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"OutputAdvanceSettingsWindow"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }

        private void launchSizeSelectionWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"SizeSelectionWindow"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }

        //private void ApplySpotImportData()
        //{
        //    int count = 0;
        //    if (SelectedSpotData[0]) // overwrite current spot data
        //    {
        //        //ignore all other spotdata because this will erase all
        //        CurrentOutput.OutputLEDSetup = _selectedLEDSetup;
        //        RaisePropertyChanged(nameof(CurrentOutput.OutputLEDSetup));
        //    }
        //    if (SelectedSpotData[1])
        //    {
        //        //set ID by spot's matrix position

        //        foreach (var spot in _selectedLEDSetup.Spots)
        //        {
        //            var targerSpot = CurrentOutput.OutputLEDSetup.Spots.Where(item => item.YIndex == spot.YIndex && item.XIndex == spot.XIndex).FirstOrDefault();
        //            if (targerSpot == null)
        //            {
        //                count++;
        //            }
        //            else
        //            {
        //                targerSpot.SetVID(spot.VID);
        //            }
        //        }
        //    }
        //    if (SelectedSpotData[2])
        //    {
        //        foreach (var spot in _selectedLEDSetup.Spots)
        //        {
        //            var targerSpot = CurrentOutput.OutputLEDSetup.Spots.Where(item => item.YIndex == spot.YIndex && item.XIndex == spot.XIndex).FirstOrDefault();
        //            if (targerSpot == null)
        //            {
        //                count++;
        //            }
        //            else
        //            {
        //                targerSpot.SetMID(spot.MID);
        //            }
        //        }
        //    }
        //    if (SelectedSpotData[3])
        //    {
        //        foreach (var spot in _selectedLEDSetup.Spots)
        //        {
        //            var targerSpot = CurrentOutput.OutputLEDSetup.Spots.Where(item => item.YIndex == spot.YIndex && item.XIndex == spot.XIndex).FirstOrDefault();
        //            if (targerSpot == null)
        //            {
        //                count++;
        //            }
        //            else
        //            {
        //                targerSpot.SetCID(spot.CID);
        //            }
        //        }
        //    }
        //    if (count > 0)
        //    {
        //        HandyControl.Controls.MessageBox.Show("Một số LED không thể lấy vị trí do hình dạng LED khác nhau giữa file và thiết bị", "LEDSetup Import", MessageBoxButton.OK, MessageBoxImage.Warning);
        //    }
        //    else
        //    {
        //        HandyControl.Controls.MessageBox.Show("Import thành công", "LEDSetup Import", MessageBoxButton.OK, MessageBoxImage.Information);
        //    }
        //}

        private void AddDevice()
        {
            CurrentSelectedDeviceToAdd.DeviceID = AvailableDevices.Count + 1;
            CurrentSelectedDeviceToAdd.DeviceUID = Guid.NewGuid().ToString();
            AvailableDevices.Insert(0, CurrentSelectedDeviceToAdd);
            //WriteDeviceInfoJson();
            //  OpenRGBStream.Dispose();
            // System.Windows.Forms.Application.Restart();
            //  Process.GetCurrentProcess().Kill();
        }




        //private void ReorderActivatedSpot()
        //{
        //    BackupSpots.Clear();
        //    BackupSpots = ActivatedSpots.OrderBy(o => o.id).ToList();
        //    // CurrentOutput.OutputLEDSetup.Spots = BackupSpots;
        //}

        //private void GrabActivatedSpot()
        //{
        //    foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
        //    {
        //        if (spot.IsActivated)
        //        {
        //            MaxLEDCount++;
        //            spot.SetID(1000);
        //            ActivatedSpots.Add(spot);
        //            spot.SetStroke(0);
        //        }
        //    }
        //}

        private void LaunchDeleteSelectedDeviceWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"DeleteSelectedDeviceWindow"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }

        private ObservableCollection<IDrawable> _pIDEditWindowSelectedItems;
        public ObservableCollection<IDrawable> PIDEditWindowSelectedItems {
            get { return _pIDEditWindowSelectedItems; }
            set { _pIDEditWindowSelectedItems = value; RaisePropertyChanged(); }
        }
        private ObservableCollection<IDrawable> _pIDEditWindowsRichCanvasItems;
        public ObservableCollection<IDrawable> PIDEditWindowsRichCanvasItems {
            get { return _pIDEditWindowsRichCanvasItems; }
            set { _pIDEditWindowsRichCanvasItems = value; RaisePropertyChanged(); }
        }
        private IDrawable _pIDEditWindowsRichCanvasSelectedItem;
        public IDrawable PIDEditWindowsRichCanvasSelectedItem {
            get { return _pIDEditWindowsRichCanvasSelectedItem; }
            set { _pIDEditWindowsRichCanvasSelectedItem = value; RaisePropertyChanged(); }
        }
        private ObservableCollection<IDrawable> _vIDEditWindowsRichCanvasItems;
        public ObservableCollection<IDrawable> VIDEditWindowsRichCanvasItems {
            get { return _vIDEditWindowsRichCanvasItems; }
            set { _vIDEditWindowsRichCanvasItems = value; RaisePropertyChanged(); }
        }
        private bool _showAllDevicePID = false;
        private bool _showAllOutputPID = false;
        public bool ShowAllDevicePID {
            get { return _showAllDevicePID; }
            set
            {
                _showAllDevicePID = value;
                if (value)
                    ShowAllOutputPID = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(ShowAllOutputPID));
                switch (value)
                {
                    case true:
                        //clear canvas anyway
                        //show all output and device
                        VIDEditWindowsRichCanvasItems.Clear();
                        foreach (var device in AvailableDevices.Where(d => !d.IsDummy))
                        {
                            //foreach (var output in device.AvailableOutputs)
                            //{
                            //    foreach (var ledSetup in output.OutputLEDSetup)
                            //    {
                            //        VIDEditWindowsRichCanvasItems.Add(ledSetup as LEDSetup);
                            //    }


                            //}
                        }
                        break;
                    case false:
                        VIDEditWindowsRichCanvasItems.Clear();


                        //foreach (var ledSetup in CurrentOutput.OutputLEDSetup)
                        //{
                        //    VIDEditWindowsRichCanvasItems.Add(ledSetup as LEDSetup);
                        //}

                        //remove all other output, just keep current selected output
                        break;
                }
            }
        }
        public bool ShowAllOutputPID {
            get { return _showAllOutputPID; }
            set
            {
                _showAllOutputPID = value;
                if (value)
                {
                    //foreach (var output in CurrentDevice.AvailableOutputs)
                    //{
                    //    foreach (var ledSetup in output.OutputLEDSetup)
                    //    {
                    //        VIDEditWindowsRichCanvasItems.Add(ledSetup as LEDSetup);
                    //    }


                    //}
                }
                else
                {
                    VIDEditWindowsRichCanvasItems.Clear();


                    //foreach (var ledSetup in CurrentOutput.OutputLEDSetup)
                    //{
                    //    VIDEditWindowsRichCanvasItems.Add(ledSetup as LEDSetup);
                    //}
                }
                RaisePropertyChanged();
            }
        }

        private string _currentIDType;
        public string CurrentIDType {
            get { return _currentIDType; }
            set { _currentIDType = value; }
        }
        //private ObservableCollection<IDrawable> _currentDeviceLiveViewItems;
        //public ObservableCollection<IDrawable> CurrentDeviceLiveViewItems {
        //    get { return _currentDeviceLiveViewItems; }
        //    set
        //    {
        //        _currentDeviceLiveViewItems = value;
        //        RaisePropertyChanged();
        //    }
        //}
        public ObservableCollection<IDrawable> SelectedLiveViewItems { get; set; }
        private IDrawable _selectedLiveViewItem;

        public IDrawable SelectedLiveViewItem {
            get { return _selectedLiveViewItem; }
            set { _selectedLiveViewItem = value; RaisePropertyChanged(); SelectedControlZone = value as IControlZone; }
        }
        private IControlZone _selectedControlZone;
        public IControlZone SelectedControlZone {
            get { return _selectedControlZone; }
            set { _selectedControlZone = value; RaisePropertyChanged(); }
        }
        private double _currentLiveViewScale;
        public double CurrentLiveViewScale {
            get { return _currentLiveViewScale; }
            set { _currentLiveViewScale = value; RaisePropertyChanged(); }
        }
        private double _currentLiveViewWidth;
        public double CurrentLiveViewWidth {
            get { return _currentLiveViewWidth; }
            set { _currentLiveViewWidth = value; RaisePropertyChanged(); UpdateLiveView(); }
        }
        private double _currentLiveViewHeight;
        public double CurrentLiveViewHeight {
            get { return _currentLiveViewHeight; }
            set { _currentLiveViewHeight = value; RaisePropertyChanged(); UpdateLiveView(); }
        }
        private Point _currentLiveViewOffset;
        public Point CurrentLiveViewOffset {
            get { return _currentLiveViewOffset; }
            set { _currentLiveViewOffset = value; RaisePropertyChanged(); }
        }
        private void UpdateLiveView()
        {
            //simple just set the scale
            CurrentDevice.UpdateChildSize();
            var widthScale = (CurrentLiveViewWidth - 50) / CurrentDevice.CurrentLivewItemsBound.Width;
            var scaleHeight = (CurrentLiveViewHeight - 50) / CurrentDevice.CurrentLivewItemsBound.Height;
            CurrentLiveViewScale = Math.Min(widthScale, scaleHeight);
            var currentWidth = CurrentDevice.CurrentLivewItemsBound.Width * CurrentLiveViewScale;
            var currentHeight = CurrentDevice.CurrentLivewItemsBound.Height * CurrentLiveViewScale;

            //set current device offset
            CurrentLiveViewOffset = new Point((CurrentLiveViewWidth - currentWidth) / 2, (CurrentLiveViewHeight - currentHeight) / 2);
            //foreach(var zone in CurrentDevice.CurrentLiveViewZones)
            //{
            //    (zone as IDrawable).ShouldBringIntoView=false;
            //    (zone as IDrawable).ShouldBringIntoView = true;

            //} 

        }
        //private void UpdateLiveView()
        //{
        //    //update live view when size changed

        //    //bring items into view

        //    //get device bound
        //    var boundWidth = (double)CurrentDevice.DeviceBoundRectangle.Width;
        //    var boundHeight = (double)CurrentDevice.DeviceBoundRectangle.Height;
        //    //move to center
        //    //keep smaller side
        //    var liveViewSmallerSide = Math.Min(CurrentLiveViewWidth, CurrentLiveViewHeight);
        //    if (liveViewSmallerSide == CurrentLiveViewWidth) //verticalview
        //    {
        //        var scale = liveViewSmallerSide / boundWidth;
        //        boundWidth = liveViewSmallerSide;
        //        boundHeight *= scale;
        //        //move to center
        //        var newLeft = (CurrentLiveViewWidth - boundWidth) / 2;
        //        var moveLeft = newLeft - (double)CurrentDevice.DeviceBoundRectangle.Left;
        //        var newTop = (CurrentLiveViewHeight - boundHeight) / 2;
        //        var moveTop = newTop - (double)CurrentDevice.DeviceBoundRectangle.Top;
        //        foreach (var zone in CurrentDevice.CurrentLiveViewZones)
        //        {
        //            (zone as IDrawable).Left *= scale;
        //            (zone as IDrawable).Top *= scale;
        //            (zone as IDrawable).SetScale(scale);
        //            (zone as IDrawable).Left += moveLeft;
        //            (zone as IDrawable).Top += moveTop;
        //        }

        //    }
        //    else if (liveViewSmallerSide == CurrentLiveViewHeight)
        //    {
        //        var scale = liveViewSmallerSide / boundHeight;
        //        boundHeight = liveViewSmallerSide;
        //        boundWidth *= scale;
        //        var newLeft = (CurrentLiveViewWidth - boundWidth) / 2;
        //        var moveLeft = newLeft - (double)CurrentDevice.DeviceBoundRectangle.Left;
        //        var newTop = (CurrentLiveViewHeight - boundHeight) / 2;
        //        var moveTop = newTop - (double)CurrentDevice.DeviceBoundRectangle.Top;
        //        foreach (var zone in CurrentDevice.CurrentLiveViewZones)
        //        {
        //            (zone as IDrawable).Left *= scale;
        //            (zone as IDrawable).Top *= scale;
        //            (zone as IDrawable).SetScale(scale);
        //            (zone as IDrawable).Left += moveLeft;
        //            (zone as IDrawable).Top += moveTop;
        //        }

        //    }
        //    //get max liveView size



        //}
        private void GetItemsForLiveView(IDeviceSettings device)
        {

            SelectedLiveViewItems = new ObservableCollection<IDrawable>();
            //set live view scale to bring zone into view
            UpdateLiveView();

            //scale control zone
            //foreach (var controller in device.AvailableControllers)
            //{
            //    { //only add lighting zone for now
            //        foreach (var output in controller.Outputs.Where(o => o.GetType() == typeof(LightingOutput)))
            //        {
            //            foreach(var zone in output.SlaveDevice.ControlableZones)
            //            {
            //                CurrentDeviceLiveViewItems.Add(zone as LEDSetup);
            //            }

            //        }
            //    }
            //}
        }
        private ObservableCollection<IDrawable> _vIDEditWindowSelectedItems;
        public ObservableCollection<IDrawable> VIDEditWindowSelectedItems {
            get { return _vIDEditWindowSelectedItems; }
            set { _vIDEditWindowSelectedItems = value; RaisePropertyChanged(); }
        }
        private void LaunchVIDEditWindow(string idType)
        {
            CurrentIDType = idType;
            PIDEditWindowSelectedItems = new ObservableCollection<IDrawable>();
            //reset all count
            CountVID = 0;
            GapVID = 0;
            //add output border rect
            VIDEditWindowsRichCanvasItems = new ObservableCollection<IDrawable>();
            if (ShowAllDevicePID)
            {
                //foreach (var device in AvailableDevices.Where(d => !d.IsDummy))
                //{
                //    foreach (var output in device.AvailableOutputs)
                //    {
                //        foreach (var ledSetup in output.OutputLEDSetup)
                //        {
                //            VIDEditWindowsRichCanvasItems.Add(ledSetup as LEDSetup);
                //        }


                //    }
                //}
            }

            else
            {
                //if (ShowAllOutputPID)
                //{
                //    foreach (var output in CurrentDevice.AvailableOutputs)
                //    {
                //        foreach (var ledSetup in output.OutputLEDSetup)
                //        {
                //            VIDEditWindowsRichCanvasItems.Add(ledSetup as LEDSetup);

                //        }

                //    }
                //}
                //else
                //    foreach (var ledSetup in CurrentOutput.OutputLEDSetup)
                //    {
                //        VIDEditWindowsRichCanvasItems.Add(ledSetup as LEDSetup);
                //    }

            }


            //this is maximum border(physical screen size) that device chose to capture from
            //BackupSpots = new List<IDeviceSpot>();
            //foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
            //{
            //    BackupSpots.Add(spot);
            //}
            // CurrentOutput.IsInSpotEditWizard = true;
            ActivatedSpots = new List<IDeviceSpot>();
            // RaisePropertyChanged(nameof(CurrentOutput.IsInSpotEditWizard));

            IsRichCanvasWindowOpen = true;
            vidEditCanvasWindow = new VIDEditCanvasWindow();
            vidEditCanvasWindow.Owner = System.Windows.Application.Current.MainWindow;
            vidEditCanvasWindow.ShowDialog();

        }
        CompositionEditWindow compositionEditWindow { get; set; }
        private Composition _currentSelectedComposition;
        public Composition CurrentSelectedComposition {
            get { return _currentSelectedComposition; }
            set
            {
                _currentSelectedComposition = value;
                RaisePropertyChanged();
            }
        }
        private ITimeLineDataItem _currentSelectedMotion;
        public ITimeLineDataItem CurrentSelectedMotion {
            get { return _currentSelectedMotion; }
            set
            {
                _currentSelectedMotion = value;
                RaisePropertyChanged();
            }
        }
        private bool _currentCompositionPlayingState;
        public bool CurrentCompositionPlayingState {
            get { return _currentCompositionPlayingState; }
            set { _currentCompositionPlayingState = value; RaisePropertyChanged(); }
        }
        private double _currentCompositionFrame;

        public double CurrentCompositionFrame {
            get { return _currentCompositionFrame; }
            set { _currentCompositionFrame = value; RaisePropertyChanged(); }
        }
        private double _timeLineHeight = 30;
        public double TimeLineHeight {
            get { return _timeLineHeight; }
            set { _timeLineHeight = value; RaisePropertyChanged(); }
        }
        private double _unitSize = 1;
        public double UnitSize {
            get { return _unitSize; }
            set { _unitSize = value; RaisePropertyChanged(); }
        }
        private void LaunchCompositionEditWindow()
        {
            ////////// testing binding logic/////////
            CurrentSelectedComposition = new Composition();
            CurrentSelectedComposition.Layers = new ObservableCollection<MotionLayer>();
            var layer1 = new MotionLayer();
            var layer2 = new MotionLayer();
            var emptyLayer = new MotionLayer();
            var emptyLayer1 = new MotionLayer();
            var emptyLayer2 = new MotionLayer();
            var emptyLayer3 = new MotionLayer();
            var emptyLayer4 = new MotionLayer();
            var bouncing = new TempDataType() {
                Name = "Bouncing",
                StartFrame = 5,
                EndFrame = 125,
                TrimEnd = 0,
                TrimStart = 0,
                OriginalDuration = 120,
                Color = Color.FromRgb(255, 255, 0)
            };
            var bouncing2 = new TempDataType() {
                Name = "Chasing",
                StartFrame = 20,
                EndFrame = 300,
                TrimEnd = 0,
                TrimStart = 0,
                OriginalDuration = 280,
                Color = Color.FromRgb(255, 0, 0)
            };
            layer1.Motions.Add(bouncing);
            layer2.Motions.Add(bouncing2);
            CurrentSelectedComposition.Layers.Add(layer1);
            CurrentSelectedComposition.Layers.Add(layer2);
            CurrentSelectedComposition.Layers.Add(emptyLayer);
            CurrentSelectedComposition.Layers.Add(emptyLayer1);
            CurrentSelectedComposition.Layers.Add(emptyLayer2);
            CurrentSelectedComposition.Layers.Add(emptyLayer3);
            CurrentSelectedComposition.Layers.Add(emptyLayer4);

            //catch collection changed to resize motions from source (path)
            foreach (var layer in CurrentSelectedComposition.Layers)
            {
                layer.Motions.CollectionChanged += (s, e) =>
                {
                    switch (e.Action)
                    {
                        case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                            var newMotions = e.NewItems;
                            foreach (TempDataType temp in newMotions)
                            {
                                //load motion from disk
                                var motion = ReadMotionFromResource(testMotionPath); // load test
                                //the resize state is implemeting inside render state
                                //ResizeMotion(motion, CurrentOutput.OutputLEDSetup.Spots.Count());
                            }
                            break;
                    }
                };
            }

            ///////
            compositionEditWindow = new CompositionEditWindow();
            compositionEditWindow.Owner = System.Windows.Application.Current.MainWindow;
            compositionEditWindow.ShowDialog();
        }
        private string testMotionPath = "adrilight.AmbinoFactoryValue.NewBouncing.AML";
        private static Motion ReadMotionFromResource(string resourceName)
        {
            Motion motion = new Motion(256);
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream resource = assembly.GetManifestResourceStream(resourceName))
            {
                if (resource == null)
                {
                    throw new ArgumentException("No such resource", "resourceName");
                }
                using (StreamReader reader = new StreamReader(resource))
                {
                    string json = reader.ReadToEnd(); //Make string equal to full file
                    try
                    {
                        motion = JsonConvert.DeserializeObject<Motion>(json, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
                    }
                    catch (Exception)
                    {
                        HandyControl.Controls.MessageBox.Show("Corrupted or incompatible data File!!!", "LEDSetup Import", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            return motion;
        }
        public void CutSelectedMotion(ITimeLineDataItem motion)
        {
            if (motion.StartFrame < CurrentCompositionFrame && CurrentCompositionFrame < motion.EndFrame)
            {
                var totalDuration = motion.OriginalDuration;
                motion.EndFrame = CurrentCompositionFrame;
                motion.OriginalDuration = motion.EndFrame - motion.StartFrame;
                //create new motion with the cutted out part
                var newMotion = new TempDataType() {
                    StartFrame = CurrentCompositionFrame,
                    OriginalDuration = totalDuration - motion.OriginalDuration,
                    Color = motion.Color,
                    Name = motion.Name + "_1",
                    EndFrame = CurrentCompositionFrame + totalDuration - motion.OriginalDuration
                };

                var currentLayer = CurrentSelectedComposition.Layers.Where(l => l.Motions.Contains(motion)).FirstOrDefault();
                currentLayer.Motions.Insert(currentLayer.Motions.IndexOf(motion) + 1, newMotion);


            }
        }
        public Motion ResizeMotion(Motion input, int framesize)
        {
            return null;
        }
        private void LaunchPIDEditWindow(IDrawable p)
        {
            CurrentIDType = "PID";
            var currentSlaveDevice = p as ARGBLEDSlaveDevice;
            CountPID = 0;
            PIDEditWindowSelectedItems = new ObservableCollection<IDrawable>();
            //add output border rect
            PIDEditWindowsRichCanvasItems = new ObservableCollection<IDrawable>();
            //ad zone border

            foreach (var zone in currentSlaveDevice.ControlableZones)
            {

                var ledSetup = zone as LEDSetup;
                //var border = new Border();
                //border.Width = ledSetup.Width;
                //border.Height = ledSetup.Height;
                //border.Left = ledSetup.Left;
                //border.Top = ledSetup.Top;
                //border.IsSelectable = true;
                //border.IsResizeable = true;
                //border.IsDraggable = true;
                PIDEditWindowsRichCanvasItems.Add(ledSetup as IDrawable);
                
             
                
            }

            //foreach (var ledSetup in CurrentOutput.OutputLEDSetup)
            //{
            //    foreach (var spot in ledSetup.Spots)
            //    {
            //        (spot as IDrawable).IsResizeable = false;
            //        (spot as IDrawable).IsDeleteable = true;
            //        PIDEditWindowsRichCanvasItems.Add(spot as DeviceSpot);
            //    }
            //}

            //Border border = new Border() {
            //    Width = (CurrentOutput as OutputSettings).Width,
            //    Height = (CurrentOutput as OutputSettings).Height,
            //    IsDraggable = true,
            //    IsSelectable = true,
            //    IsResizeable = true,
            //    IsDeleteable = false,
            //    ShouldBringIntoView = true

            //};
            //this is maximum border(physical screen size) that device chose to capture from


            ScreenBound screen = new ScreenBound();
            screen.Width = Screen.AllScreens[0].Bounds.Width;
            screen.Height = Screen.AllScreens[0].Bounds.Height;
            screen.Top = Screen.AllScreens[0].Bounds.Top;
            screen.Left = Screen.AllScreens[0].Bounds.Left;
            PIDEditWindowsRichCanvasItems.Insert(0, screen);





            PathGuide pathGuide = new PathGuide() {
                Width = 200,
                Height = 200,
                IsDraggable = true,
                IsSelectable = true,
                IsResizeable = false,
                Geometry = "AmbinoA1Guide"
            };
            // PIDEditWindowsRichCanvasItems.Insert(0, pathGuide);


            //BackupSpots = new List<IDeviceSpot>();
            //foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
            //{
            //    BackupSpots.Add(spot);
            //}
            //CurrentOutput.IsInSpotEditWizard = true;
            ActivatedSpots = new List<IDeviceSpot>();
            // RaisePropertyChanged(nameof(CurrentOutput.IsInSpotEditWizard));
            //foreach (var ledSetup in CurrentOutput.OutputLEDSetup)
            //{
            //    foreach (var spot in ledSetup.Spots)
            //    {
            //        spot.SetColor(0, 0, 0, true);
            //    }
            //}

            CurrentLEDEditWizardState = 0;
            pidEditCanvasWindow = new PIDEditCanvasWindow();
            pidEditCanvasWindow.Owner = System.Windows.Application.Current.MainWindow;
            pidEditCanvasWindow.ShowDialog();

        }



        //private void LaunchMIDEditWindow()
        //{
        //    if (AssemblyHelper.CreateInternalInstance($"View.{"MIDEditWindow"}") is System.Windows.Window window)
        //    {
        //        BackupSpots = new List<IDeviceSpot>();
        //        foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
        //        {
        //            BackupSpots.Add(spot);
        //        }

        //        window.Owner = System.Windows.Application.Current.MainWindow;
        //        window.ShowDialog();
        //    }
        //}

        //private void LaunchCIDEditWindow()
        //{
        //    if (AssemblyHelper.CreateInternalInstance($"View.{"CIDEditWindow"}") is System.Windows.Window window)
        //    {
        //        BackupSpots = new List<IDeviceSpot>();
        //        foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
        //        {
        //            BackupSpots.Add(spot);
        //        }

        //        window.Owner = System.Windows.Application.Current.MainWindow;
        //        window.ShowDialog();
        //    }
        //}

        private void OpenColorPickerWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"ColorPickerWindow"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }


        private void OpenAboutWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"AboutAppWindow"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }
        private ObservableCollection<IDrawable> _surfaceEditorItems;
        public ObservableCollection<IDrawable> SurfaceEditorItems {
            get { return _surfaceEditorItems; }
            set
            {
                _surfaceEditorItems = value;
                RaisePropertyChanged(nameof(SurfaceEditorItems));
            }
        }

        public ObservableCollection<IDrawable> SurfaceEditorSelectedItems { get; set; }
        private IDrawable _surfaceEditorSelectedItem;
        public IDrawable SurfaceEditorSelectedItem {
            get { return _surfaceEditorSelectedItem; }
            set
            {
                _surfaceEditorSelectedItem = value;
                RaisePropertyChanged(nameof(SurfaceEditorSelectedItem));
            }
        }
        private bool _canSelectMultipleItems;
        public bool CanSelectMultipleItems {
            get { return _canSelectMultipleItems; }
            set
            {
                _canSelectMultipleItems = value;
                RaisePropertyChanged(nameof(CanSelectMultipleItems));
            }
        }
        private void SelectedItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (CanSelectMultipleItems)
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    SurfaceEditorSelectedItem = SurfaceEditorSelectedItems.Count == 1 ? SurfaceEditorSelectedItems[0] : null;
                }
                else if (e.Action == NotifyCollectionChangedAction.Reset)
                {
                    if (SurfaceEditorSelectedItems.Count == 0 || SurfaceEditorSelectedItems.Count > 1)
                    {
                        SurfaceEditorSelectedItem = null;
                    }
                }
            }
        }
        private void OpenRectangleScaleWindow(ObservableCollection<IDrawable> itemSource) // set scale for all surface editor selected items 
        {


            var window = new ScaleSelectionWindow(itemSource);
            window.Owner = System.Windows.Application.Current.MainWindow;
            window.ShowDialog();


        }
        private void OpenAvailableLedSetupForCurrentDeviceWindow()
        {
            AvailableOutputForSelectedDevice = new List<IOutputSettings>();

            //foreach (var defaultOutput in DefaulOutputCollection.AvailableDefaultOutputsForAmbinoDevices.Where(o => o.TargetDevice == CurrentDevice.DeviceType))
            //{
            //    if (defaultOutput.TargetDevice == CurrentDevice.DeviceType)
            //    {
            //        AvailableOutputForSelectedDevice.Add(defaultOutput);
            //    }
            //}
            var window = new LEDSetupSelectionWindows();
            window.Owner = System.Windows.Application.Current.MainWindow;
            window.ShowDialog();
        }
        private void OpenRectangleRotateWindow() // set scale for all surface editor selected items 
        {


            if (AssemblyHelper.CreateInternalInstance($"View.{"RotateSelectionWindow"}") is System.Windows.Window window)
            {

                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }

        }
        private double _itemScaleValue = 1.0;
        public double ItemScaleValue {
            get { return _itemScaleValue; }
            set { _itemScaleValue = value; RaisePropertyChanged(nameof(ItemScaleValue)); }
        }
        private double _itemRotateValue = 15.0;
        public double ItemRotateValue {
            get { return _itemRotateValue; }
            set { _itemRotateValue = value; RaisePropertyChanged(nameof(ItemRotateValue)); }
        }
        private void SetSelectedItemScaleFactor(ObservableCollection<IDrawable> itemSource)
        {
            foreach (var item in itemSource.OfType<IDrawable>().Where(d => d.IsSelected))
            {
                item.SetScale(ItemScaleValue);
            }
        }
        private void SetSelectedItemRotateFactor(ObservableCollection<IDrawable> itemSource)
        {

            var selectedItems = itemSource.OfType<IDrawable>().Where(d => d.IsSelected && d is DeviceSpot);
            ////find center point of selected item(s)
            //var maxX = selectedItems.MaxBy(d => (d.Width + d.Left)).FirstOrDefault();
            //var maxY = selectedItems.MaxBy(d => (d.Height + d.Top)).FirstOrDefault();
            //var minX = selectedItems.MinBy(d => (d.Left)).FirstOrDefault();
            //var minY = selectedItems.MinBy(d => (d.Top)).FirstOrDefault();
            //var x = minX.Left + (maxX.Width + maxX.Left - minX.Left) / 2;
            //var y = minY.Top + (maxY.Height + maxY.Top - minY.Top) / 2;
            //System.Windows.Point centerPoint = new System.Windows.Point(x, y);
            foreach (var item in selectedItems)
            {
                //item.CenterX = x;
                //item.CenterY = y;
                item.Angle = ItemRotateValue;
            }
        }
        private void ResetToDefaultRectangleScale()
        {
            foreach (var item in SurfaceEditorItems.OfType<IDrawable>().Where(d => d.IsSelected))
            {
                item.Width = 100.0;
                item.Height = 100.0;
                item.SetScale(1.0);
            }
        }
        private void AglignSelectedItemstoLeft(ObservableCollection<IDrawable> itemSource)
        {
            double minLeft = itemSource.OfType<IDrawable>().Where(d => d.IsSelected).Min(x => x.Left);
            foreach (var item in itemSource.OfType<IDrawable>().Where(d => d.IsSelected))
            {
                item.Left = minLeft;
            }
        }
        private void SpreadItemHorizontal(ObservableCollection<IDrawable> itemSource, int dirrection)
        {
            //get min X
            double spacing = 10.0;
            double minLeft = itemSource.OfType<IDrawable>().Where(d => d.IsSelected).Min(x => x.Left);
            var selectedItems = itemSource.OfType<IDrawable>().Where(d => d.IsSelected).ToArray();
            switch (dirrection)
            {
                case 0:
                    for (int i = 0; i < selectedItems.Count(); i++)
                    {
                        if (i == 0)
                            selectedItems[i].Left = minLeft;
                        else
                        {
                            var previousLeft = selectedItems[i - 1].Left;
                            selectedItems[i].Left = selectedItems[i - 1].Width + previousLeft + spacing;
                        }

                    }
                    AglignSelectedItemstoTop(itemSource);
                    break;
                case 1:
                    for (int i = 0; i < selectedItems.Count(); i++)
                    {
                        if (i == 0)
                            selectedItems[i].Left = minLeft;
                        else
                        {
                            var previousLeft = selectedItems[i - 1].Left;
                            selectedItems[i].Left = previousLeft - (selectedItems[i].Width + spacing);
                        }

                    }
                    AglignSelectedItemstoTop(itemSource);
                    break;

            }
        }
        private void SpreadItemVertical(ObservableCollection<IDrawable> itemSource, int dirrection)
        {
            //get min Y
            double spacing = 10.0;
            double minTop = itemSource.OfType<IDrawable>().Where(d => d.IsSelected).Min(x => x.Top);
            var selectedItems = itemSource.OfType<IDrawable>().Where(d => d.IsSelected).ToArray();
            switch (dirrection)
            {
                case 0:
                    for (int i = 0; i < selectedItems.Count(); i++)
                    {
                        if (i == 0)
                            selectedItems[i].Top = minTop;
                        else
                        {
                            var previousTop = selectedItems[i - 1].Top;
                            selectedItems[i].Top = selectedItems[i - 1].Height + previousTop + spacing;
                        }

                    }
                    AglignSelectedItemstoLeft(itemSource);
                    break;
                case 1:
                    for (int i = 0; i < selectedItems.Count(); i++)
                    {
                        if (i == 0)
                            selectedItems[i].Top = minTop;
                        else
                        {
                            var previousTop = selectedItems[i - 1].Top;
                            selectedItems[i].Top = previousTop - (selectedItems[i].Height + spacing);
                        }

                    }
                    AglignSelectedItemstoLeft(itemSource);
                    break;

            }

        }
        private void AglignSelectedItemstoTop(ObservableCollection<IDrawable> itemSource)
        {
            double minTop = itemSource.OfType<IDrawable>().Where(d => d.IsSelected).Min(x => x.Top);
            foreach (var item in itemSource.OfType<IDrawable>().Where(d => d.IsSelected))
            {
                item.Top = minTop;
            }
        }
        public class DrawableShape
        {
            public string Geometry { get; set; }
            public string Name { get; set; }
        }
        public DrawableShape SelectedShape { get; set; }
        public double NewItemWidth { get; set; }
        public double NewItemHeight { get; set; }
        public int ItemNumber { get; set; }
        public List<DrawableShape> AvailableShapeToAdd { get; set; }
        private void OpenAddNewItemWindow()
        {
            AvailableShapeToAdd = new List<DrawableShape>();
            var circle = new DrawableShape() {
                Geometry = "genericCircle",
                Name = "Round"
            };
            var square = new DrawableShape() {
                Geometry = "genericSquare",
                Name = "Square"
            };
            //var ambinoA1Outer = new DrawableShape() {
            //    Geometry = "ambinoFanA1OuteringGeometry",
            //    Name = "AmbinoA1Outer"
            //};
            //var ambinoA1Inner = new DrawableShape() {
            //    Geometry = "ambinoFanA1InnerringGeometry",
            //    Name = "AmbinoA1Inner"
            //};
            AvailableShapeToAdd.Add(square);
            AvailableShapeToAdd.Add(circle);
            //AvailableShapeToAdd.Add(ambinoA1Inner);
            //AvailableShapeToAdd.Add(ambinoA1Outer);
            if (AssemblyHelper.CreateInternalInstance($"View.{"NewItemParametersWindow"}") is System.Windows.Window window)
            {

                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }


        }
        private Point _mousePosition;
        public Point MousePosition {
            get { return _mousePosition; }
            set { _mousePosition = value; RaisePropertyChanged(nameof(MousePosition)); }
        }
        private void AddImage()
        {
            OpenFileDialog addImage = new OpenFileDialog();
            addImage.Title = "Chọn Ảnh";
            addImage.CheckFileExists = true;
            addImage.CheckPathExists = true;
            addImage.DefaultExt = "Png";
            addImage.Filter = "Image files (*.png;*.jpeg;*jpg)|*.png;*.jpeg;*jpg|All files (*.*)|*.*";
            addImage.FilterIndex = 2;

            addImage.ShowDialog();

            if (!string.IsNullOrEmpty(addImage.FileName) && File.Exists(addImage.FileName))
            {

                var image = new ImageVisual {
                    Top = MousePosition.Y,
                    Left = MousePosition.X,
                    ImagePath = addImage.FileName,
                    Height = 500,
                    Width = 500,
                    IsResizeable = true
                };
                PIDEditWindowsRichCanvasItems.Insert(0, image);
            }
        }

        private void DeleteSelectedItems(ObservableCollection<IDrawable> itemSource)
        {
            var selectedItems = itemSource.OfType<IDrawable>().Where(d => d.IsSelected).ToArray();
            for (var i = 0; i < selectedItems.Count(); i++)
            {
                selectedItems[i].IsSelected = false;
                if (selectedItems[i].IsDeleteable)
                    itemSource.Remove(selectedItems[i]);
                //foreach ()
                //    if (CurrentOutput.OutputLEDSetup.Spots.Contains(selectedItems[i] as DeviceSpot))
                //        CurrentOutput.OutputLEDSetup.Spots.Remove(selectedItems[i] as DeviceSpot);

            }
        }
        private void AddItemsToPIDCanvas()
        {
            var lastSpotID = PIDEditWindowsRichCanvasItems.Where(s => s is DeviceSpot).Count();
            var newItems = new ObservableCollection<IDrawable>();
            if (ItemNumber == 0)
            {
                ItemNumber = 1;
            }
            for (int i = 0; i < ItemNumber; i++)
            {
                var newItem = new DeviceSpot(
                    MousePosition.Y,
                    MousePosition.X,
                    NewItemWidth > 0 ? NewItemWidth : 20.0,
                    NewItemHeight > 0 ? NewItemHeight : 20.0,
                    1,
                    1,
                    1,
                    1,
                    lastSpotID + i,
                    lastSpotID + i,
                    lastSpotID + i,
                    lastSpotID + i,
                    lastSpotID + i,
                    false,
                    SelectedShape.Geometry);
                newItem.IsSelected = true;
                newItem.IsDeleteable = true;
                newItems.Add(newItem);

            }
            SpreadItemHorizontal(newItems, 0);
            foreach (var item in newItems)
            {
                PIDEditWindowsRichCanvasItems.Add(item);
                //CurrentOutput.OutputLEDSetup.Spots.Add(item as DeviceSpot);
            }

        }
        private void ClearPIDCanvas()
        {
            CountPID = 0;
            var itemToRemove = new List<IDrawable>();
            foreach (var item in PIDEditWindowsRichCanvasItems.Where(s => s is DeviceSpot))
            {
                item.IsSelected = false;
                itemToRemove.Add(item);
            }
            foreach (var item in PIDEditWindowsRichCanvasItems.Where(b => b is Border))
            {
                item.IsSelected = false;
                itemToRemove.Add(item);
            }
            foreach (var item in itemToRemove)
            {
                PIDEditWindowsRichCanvasItems.Remove(item);
                //if (CurrentOutput.OutputLEDSetup.Spots.Contains(item as DeviceSpot))
                //    CurrentOutput.OutputLEDSetup.Spots.Remove(item as DeviceSpot);
            }

        }
        private void LockSelectedItem(ObservableCollection<IDrawable> itemSource)
        {

            foreach (var item in itemSource.OfType<IDrawable>().Where(d => d.IsSelected))
            {
                item.IsDraggable = false;
            }
        }
        private void SetRandomOutputColor()
        {
            foreach (var device in AvailableDevices.Where(x => !x.IsDummy))
            {
                device.CurrentState = State.surfaceEditor;
            }
        }
        private void UnlockSelectedItem(ObservableCollection<IDrawable> items)
        {

            foreach (var item in items.OfType<IDrawable>().Where(d => d.IsSelected))
            {
                item.IsDraggable = true;
                item.IsSelectable = true;

            }
        }
        private DrawableHelpers DrawableHlprs { get; set; }

        private void SaveCurrentLEDSetupLayout()
        {
            //get width and height of current border
            var spotList = PIDEditWindowsRichCanvasItems.OfType<IDrawable>().Where(d => d is DeviceSpot);
            if (spotList.Count() == 0)
            {
                HandyControl.Controls.MessageBox.Show("Bạn phải thêm ít nhất 1 LED", "Invalid LED number", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (DrawableHlprs == null)
            {
                DrawableHlprs = new DrawableHelpers();
            }
            var newBoundRect = DrawableHlprs.GetBound(spotList.ToList());

            var screen = PIDEditWindowsRichCanvasItems.OfType<IDrawable>().Where(d => d is ScreenBound && !d.IsResizeable).FirstOrDefault();
            var screenRect = new Rectangle((int)screen.Left, (int)screen.Top, (int)screen.Width, (int)screen.Height);
            if (Rectangle.Intersect(screenRect, newBoundRect).Equals(newBoundRect))
            {
                //check if there is any zone exist
                //if not, create a single zone
                //if there is, foreach zone, put them back to slave device
                //update slave deivce size
                foreach (var spot in spotList)
                {
                    (spot as DeviceSpot).ScaleWidth = spot.Width / newBoundRect.Width;
                    (spot as DeviceSpot).ScaleHeight = spot.Height / newBoundRect.Height;
                    (spot as DeviceSpot).ScaleLeft = (spot.Left - newBoundRect.Left) / newBoundRect.Width;
                    (spot as DeviceSpot).ScaleTop = (spot.Top - newBoundRect.Top) / newBoundRect.Height;
                    //rebuild spot
                    (spot as DeviceSpot).RebuildSpot(newBoundRect.Width, newBoundRect.Height);
                }

                var currentSlaveDevice = SurfaceEditorSelectedItem as ARGBLEDSlaveDevice;
                //new bound rect
                currentSlaveDevice.ScaleLeft = (newBoundRect.Left - screen.Left) / screen.Width;
                currentSlaveDevice.ScaleTop = (newBoundRect.Top - screen.Top) / screen.Height;
                currentSlaveDevice.ScaleWidth = newBoundRect.Width / screen.Width;
                currentSlaveDevice.ScaleHeight = newBoundRect.Height / screen.Height;
                currentSlaveDevice.Left = newBoundRect.Left;
                currentSlaveDevice.Top = newBoundRect.Top;
                currentSlaveDevice.UpdateSizeByChild();
                //(SurfaceEditorSelectedItem as IDrawable).Width = newBoundRect.Width;
                //(SurfaceEditorSelectedItem as IDrawable).Height = newBoundRect.Height ;
                //check if liveviewopen and update it
                // UpdateLiveView();
            }
            else
            {
                HandyControl.Controls.MessageBox.Show("Vị trí hoặc kích thước của thiết bị vượt ra ngoài giới hạn màn hình", "Out of range", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //if ((maxX.Width + maxX.Left) > (border.Width + border.Left) ||
            //    (maxY.Height + maxX.Top) > (border.Height + border.Top) ||
            //    minX.Left < border.Left ||
            //    minY.Top < border.Top
            //    )
            //{
            //    //this indicate there is atleast one of the item jumping out of the border
            //    HandyControl.Controls.MessageBox.Show("Kiểm tra lại vị trí, có LED vượt ra ngoài ranh giới", "Out of range", MessageBoxButton.OK, MessageBoxImage.Error);
            //    return;
            //}
            //check if the border is out of the screen




            //change the scale too

            //CurrentOutput.IsInSpotEditWizard = false;
            pidEditCanvasWindow.Close();

        }
        private void SaveCurrentSurfaceLayout()
        {
            //get width and height of current border
            var screens = SurfaceEditorItems.OfType<IDrawable>().Where(d => d is ScreenBound);
            //var maxX = SurfaceEditorItems.MaxBy(d => (d.Width + d.Left)).FirstOrDefault();
            //var maxY = SurfaceEditorItems.MaxBy(d => (d.Height + d.Top)).FirstOrDefault();
            //var minX = SurfaceEditorItems.MinBy(d => (d.Left)).FirstOrDefault();
            //var minY = SurfaceEditorItems.MinBy(d => (d.Top)).FirstOrDefault();
            //if ((maxX.Width + maxX.Left) > (border.Width + border.Left) ||
            //    (maxY.Height + maxX.Top) > (border.Height + border.Top) ||
            //    minX.Left < border.Left ||
            //    minY.Top < border.Top
            //    )
            foreach (var item in SurfaceEditorItems.Where(i => i is ARGBLEDSlaveDevice))
            {
                var ledDevice = item as ARGBLEDSlaveDevice;
                var itemRect = new Rectangle(
                        (int)ledDevice.Left,
                        (int)ledDevice.Top,
                        (int)ledDevice.Width,
                        (int)ledDevice.Height);
                int screenCount = 0;
                foreach (var screen in screens)
                {
                    var screenRect = new Rectangle(
                        (int)screen.Left,
                        (int)screen.Top,
                        (int)screen.Width,
                        (int)screen.Height);
                    if (Rectangle.Intersect(screenRect, itemRect).Equals(itemRect))
                    {
                        //this is the pool our current item is in
                        //set output selected display for this item
                        //(item as LEDSetup).OutputSelectedDisplay = (screen as ScreenBound).Index;
                        //set scale base on this pool width and height
                        //set scale for each led setup
                        //foreach(var zone in ledDevice.ControlableZones)
                        //{
                        //    (zone as LEDSetup).RefreshSizeAndPosition();
                        //}
                        (item as ARGBLEDSlaveDevice).ScaleWidth = item.Width / screen.Width;
                        (item as ARGBLEDSlaveDevice).ScaleHeight = item.Height / screen.Height;
                        (item as ARGBLEDSlaveDevice).ScaleLeft = (item.Left - screen.Left) / screen.Width;
                        (item as ARGBLEDSlaveDevice).ScaleTop = (item.Top - screen.Top) / screen.Height;
                        //slave device data will be used for capturing information
                        item.IsSelected = false;

                        //(item as LEDSetup).IsScreenCaptureEnabled = true;
                        screenCount++;
                    }
                }
                if (screenCount == 0)
                {
                    //we not save this position for now
                    //this item is no in any pool
                    //we disable screen capture for this device
                    // (item as LEDSetup).IsScreenCaptureEnabled = false;
                    HandyControl.Controls.MessageBox.Show("Những thiết bị nằm ngoài ranh giới sẽ không sử dụng được tính năng sáng theo màn hình", "Out of range", MessageBoxButton.OK, MessageBoxImage.Warning);
                    //return;
                }


            }

            //since we got multiple screen support, we need to define dynamic border
            //we need to define if any part of current item is outside of the border
            //in ordinary way, we just need a trip through all item to check but it's overkill and not smart
            //to be able to do this, using intersect
            //foreach (var item in SurfaceEditorItems.OfType<IDrawable>().Where(d => !(d is Border)))
            //{
            //    (item as LEDSetup).ScaleWidth = item.Width / border.Width;
            //    (item as LEDSetup).ScaleHeight = item.Height / border.Height;
            //    (item as LEDSetup).ScaleLeft = item.Left / border.Width;
            //    (item as LEDSetup).ScaleTop = item.Top / border.Height;
            //    item.IsSelected = false;
            //}
            surfaceeditorWindow.Close();
            IsRichCanvasWindowOpen = false;
        }
        private void SetSpotID(IDeviceSpot spot)
        {
            if (Keyboard.IsKeyDown(Key.B) || Keyboard.IsKeyDown(Key.RightCtrl) || Mouse.LeftButton == MouseButtonState.Pressed)
            {
                switch (CurrentIDType)
                {
                    case "VID":
                        spot.SetVID(CountVID += GapVID);
                        spot.IsEnabled = true;
                        if (CountVID > 1023)
                            CountVID = 0;
                        break;
                    case "FID":
                        spot.SetMID(CountVID += GapVID);
                        spot.IsEnabled = true;
                        if (CountVID > 1023)
                            CountVID = 0;
                        break;
                    case "CID":
                        spot.SetCID(CountVID += GapVID);
                        spot.IsEnabled = true;
                        if (CountVID > 32)
                            CountVID = 0;
                        break;
                    case "PID":
                        spot.SetID(CountPID++);
                        spot.IsEnabled = true;
                        spot.SetColor(0, 0, 255, true);
                        break;

                }

            }
            else if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                switch (CurrentIDType)
                {
                    case "VID":
                        spot.SetVID(0);
                        spot.IsEnabled = false;
                        if (CountVID >= GapVID)
                        {
                            CountVID -= GapVID;
                        }
                        else
                            CountVID = 0;
                        break;
                    case "FID":
                        spot.SetMID(0);
                        spot.IsEnabled = false;
                        if (CountVID >= GapVID)
                        {
                            CountVID -= GapVID;
                        }
                        else
                            CountVID = 0;
                        break;
                    case "CID":
                        spot.SetCID(0);
                        spot.IsEnabled = false;
                        if (CountVID >= GapVID)
                        {
                            CountVID -= GapVID;
                        }
                        else
                            CountVID = 0;
                        break;
                    case "PID":
                        spot.SetID(0);
                        spot.IsEnabled = false;
                        spot.SetColor(0, 0, 0, true);
                        if (CountPID >= 1)
                        {
                            CountPID -= 1;
                        }
                        else
                            CountPID = 0;
                        break;

                }

            }

        }
        private void OpenSurfaceEditorWindow()
        {
            SurfaceEditorItems = new ObservableCollection<IDrawable>();
            SurfaceEditorSelectedItems = new ObservableCollection<IDrawable>();
            SurfaceEditorSelectedItems.CollectionChanged += SelectedItemsChanged;
            foreach (var device in AvailableDevices)
            {
                if (!device.IsDummy)
                {
                    var slaveDevices = device.AvailableLightingDevices;
                    foreach (var slaveDevice in slaveDevices)
                    {
                        SurfaceEditorItems.Add(slaveDevice as IDrawable);
                    }




                }
            }
            //add virtual borders
            if (GeneralSettings.IsMultipleScreenEnable)
            {
                for (int i = 0; i < Screen.AllScreens.Length; i++)
                {
                    ScreenBound screen = new ScreenBound();
                    screen.Width = Screen.AllScreens[i].Bounds.Width;
                    screen.Height = Screen.AllScreens[i].Bounds.Height;
                    screen.Top = Screen.AllScreens[i].Bounds.Top;
                    screen.Left = Screen.AllScreens[i].Bounds.Left;
                    screen.Index = i;
                    SurfaceEditorItems.Add(screen);
                }
            }
            else
            {
                ScreenBound screen = new ScreenBound();
                SurfaceEditorItems.Add(screen);
            }


            surfaceeditorWindow = new SurfaceEditorWindow();
            IsRichCanvasWindowOpen = true;
            surfaceeditorWindow.Owner = System.Windows.Application.Current.MainWindow;
            surfaceeditorWindow.ShowDialog();


        }
        SurfaceEditorWindow surfaceeditorWindow { get; set; }
        PIDEditCanvasWindow pidEditCanvasWindow { get; set; }
        VIDEditCanvasWindow vidEditCanvasWindow { get; set; }
        private void AddSelectedItemToGroup()
        {
            var newGroup = new Group();
            newGroup.SetGroupedElements(SurfaceEditorItems.OfType<IDrawable>().Where(d => !(d is Group) && d is IGroupable && d.IsSelected).ToArray());
            newGroup.SetGroupSize();
            SurfaceEditorItems.Add(newGroup);

        }
        OnlineItemExporterView itemExporter { get; set; }
        public object CurrentOnlineItemToExport { get; set; }
        private void LaunchAdrilightStoreItemCreatorWindow()
        {
            //set current selected online item
            CurrentSelectedOnlineItem = new OnlineItemModel() {
                Name = "Change Name here",
                Description = "Edit Description here",
                Owner = "Enter the Name of the owner(ex ambino, corsair)",
            };
            itemExporter = new OnlineItemExporterView();
            itemExporter.ShowDialog();

        }
        private void ExportCurrentItemToAdrilightStore<T>(T item)
        {
            //
        }
        private void ExportCurrentSelectedPaletteToFile(IColorPalette palette)
        {
            SaveFileDialog Export = new SaveFileDialog();
            Export.CreatePrompt = true;
            Export.OverwritePrompt = true;

            Export.Title = "Xuất dữ liệu";
            Export.FileName = palette.Name + " Color Palette";
            Export.CheckFileExists = false;
            Export.CheckPathExists = true;
            Export.InitialDirectory =
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            Export.RestoreDirectory = true;

            var paletteJson = JsonConvert.SerializeObject(palette, new JsonSerializerSettings() {
                TypeNameHandling = TypeNameHandling.Auto
            });

            if (Export.ShowDialog() == DialogResult.OK)
            {
                //create directory with same name
                var newFolder = Directory.CreateDirectory(Export.FileName);
                var contentFolder = Directory.CreateDirectory(Path.Combine(Export.FileName, "content")).ToString();
                //create main content 
                File.WriteAllText(Path.Combine(Export.FileName, "content", palette.Name + ".col"), paletteJson);
                //create info
                var info = new OnlineItemModel() {
                    Name = palette.Name,
                    Owner = palette.Owner,
                    Description = palette.Description,
                    Type = "Palette",
                    SubType = "RGBPalette16"
                };
                var infoJson = JsonConvert.SerializeObject(info, new JsonSerializerSettings() {
                    TypeNameHandling = TypeNameHandling.Auto
                });
                File.WriteAllText(Path.Combine(Export.FileName, "info.json"), infoJson);
                //create image, require user input later thumb.png???


            }

        }

        public void SetPreviewVisualizerFFT(float[] fft, Color[] previewStrip)
        {
            for (int i = 0; i < fft.Length; i++)
            {
                VisualizerFFT[i].SetValue(fft[i]);
                VisualizerFFT[i].SetColor(previewStrip[i]);
            }
        }

        //private void CreateNewPalette()
        //{
        //    var lastSelectedPaletteIndex = CurrentOutput.OutputSelectedChasingPalette;
        //    var name = NewPaletteName;
        //    var owner = NewPaletteOwner;
        //    var description = NewPaletteDescription;
        //    var colors = new System.Windows.Media.Color[16];
        //    int counter = 0;
        //    foreach (var color in CurrentEditingColors)
        //    {
        //        colors[counter++] = color;
        //    }
        //    AvailablePallete.Clear();
        //    foreach (var palette in LoadPaletteIfExists())
        //    {
        //        AvailablePallete.Add(palette);
        //    }
        //    IColorPalette newpalette = new ColorPalette(name, owner, "RGBPalette16", description, colors);
        //    AvailablePallete.Add(newpalette);

        //    //WritePaletteCollectionJson();
        //    var json = JsonConvert.SerializeObject(newpalette, new JsonSerializerSettings() {
        //        TypeNameHandling = TypeNameHandling.Auto
        //    });
        //    File.WriteAllText(Path.Combine(PalettesCollectionFolderPath, newpalette.Name + ".col"), json);

        //}

        private void CreateNewAutomation()
        {
            var name = NewAutomationName;
            IAutomationSettings newAutomation = new AutomationSettings { Name = name };

            //var doAbsolutelyNothing = new List<IActionSettings>(); // doing nothing to all devices
            //if (AvailableDevices.Count > 0 && AvailableProfiles.Count > 0)
            //{
            //    foreach (var device in AvailableDevices.Where(x => x.IsDummy == false))
            //    {
            //        IActionSettings doNothing = new ActionSettings {
            //            ActionType = "Hành động",
            //            TargetDeviceUID = device.DeviceUID,
            //            TargetDeviceName = device.DeviceName,
            //            TargetDeviceType = device.DeviceType,
            //            ActionParameter = new ActionParameter { Name = "Biến", Type = "unknown", Value = "none" },
            //        };

            //        doAbsolutelyNothing.Add(doNothing);
            //    }
            //}

            ////newAutomation.Actions = doAbsolutelyNothing;
            //IActionSettings dummy = new ActionSettings {
            //    ActionType = "Hành động",
            //    TargetDeviceUID = device.DeviceUID,
            //    TargetDeviceName = device.DeviceName,
            //    TargetDeviceType = device.DeviceType,
            //    ActionParameter = new ActionParameter { Name = "Biến", Type = "unknown", Value = "none" },
            //};
            AvailableAutomations.Add(newAutomation);
            WriteAutomationCollectionJson();
        }

        private void EditCurrentPalette(string param)
        {
            OpenEditPaletteDialog(param);
        }

        private void SetActivePaletteAllOutputs(IColorPalette palette)
        {
            //foreach (var output in CurrentDevice.AvailableOutputs)
            //{
            //    //output.OutputCurrentActivePalette = palette;
            //    //output.OutputSelectedChasingPalette = CurrentOutput.OutputSelectedChasingPalette;
            //}
        }

        private void SetActivePaletteAllDevices(IColorPalette palette)
        {
            foreach (var device in AvailableDevices.Where(p => !p.IsDummy))
            {
                //foreach (var output in device.AvailableOutputs)
                //{
                //    //output.OutputCurrentActivePalette = palette;
                //    //output.OutputSelectedChasingPalette = CurrentOutput.OutputSelectedChasingPalette;
                //}

            }
        }

        private void Reset()
        {
            //disable all device
            if (File.Exists(JsonDeviceFileNameAndPath))
            {
                File.Delete(JsonDeviceFileNameAndPath);
            }
            if (File.Exists(JsonDeviceProfileFileNameAndPath))
            {
                File.Delete(JsonDeviceProfileFileNameAndPath);
            }
            if (File.Exists(JsonGeneralFileNameAndPath))
            {
                File.Delete(JsonGeneralFileNameAndPath);
            }
            if (AmbinityClient != null)
                AmbinityClient.Dispose();
            System.Windows.Forms.Application.Restart();
            Process.GetCurrentProcess().Kill();
        }

        private void DeleteSelectedAutomation(IAutomationSettings automation)
        {
            AvailableAutomations.Remove(automation);
            WriteAutomationCollectionJson();
        }
        private FTPServerHelpers FTPHlprs { get; set; }
        private void UploadSelectedPalette(IColorPalette selectedpalette)
        {
            if (selectedpalette != null)
            {
                //serialize current selected palette
                var palette = JsonConvert.SerializeObject(selectedpalette, new JsonSerializerSettings() {
                    TypeNameHandling = TypeNameHandling.Auto
                });


            }
        }
        private void DeleteSelectedPalette(IColorPalette selectedpalette)
        {
            if (AvailablePallete.Count == 1)
            {
                var result = HandyControl.Controls.MessageBox.Show(new MessageBoxInfo {
                    Message = " Please don't delete all Color Palette in this section, atleast keep one left!!!",
                    Caption = "Xóa dải màu",
                    Button = MessageBoxButton.OK,
                    IconBrushKey = ResourceToken.AccentBrush,
                    IconKey = ResourceToken.WarningGeometry,
                    StyleKey = "MessageBoxCustom"
                });

                return;
            }
            AvailablePallete.Remove(selectedpalette);
            //WritePaletteCollectionJson();
            AvailablePallete.Clear();
            foreach (var palette in LoadPaletteIfExists())
            {
                AvailablePallete.Add(palette);
            }
            //CurrentOutput.OutputSelectedChasingPalette = 0;
            //CurrentActivePalette = AvailablePallete.First();
        }

        private void DeleteSelectedGif(IGifCard gif)
        {
            if (AvailableGifs.Count == 1)
            {
                var result = HandyControl.Controls.MessageBox.Show(new MessageBoxInfo {
                    Message = " You have nothing left if you delete this lat bit of goodness!!!",
                    Caption = "Xóa Gif",
                    Button = MessageBoxButton.OK,
                    IconBrushKey = ResourceToken.AccentBrush,
                    IconKey = ResourceToken.WarningGeometry,
                    StyleKey = "MessageBoxCustom"
                });

                return;
            }
            AvailableGifs.Remove(gif);
            //CurrentActiveGif = AvailableGifs.Last();
            WriteGifCollectionJson();

            //AvailableGifs.Clear();
            //foreach (var palette in LoadPaletteIfExists())
            //{
            //    AvailablePallete.Add(palette);
            //}
            //CurrentOutput.OutputSelectedChasingPalette = 0;
            //CurrentActivePalette = AvailablePallete.First();
        }

        private void DeleteSelectedGradient(IGradientColorCard gradient)
        {
            if (AvailableGradient.Count == 1)
            {
                var result = HandyControl.Controls.MessageBox.Show(new MessageBoxInfo {
                    Message = " You have nothing left if you delete this lat bit of goodness!!!",
                    Caption = "Xóa Gradient",
                    Button = MessageBoxButton.OK,
                    IconBrushKey = ResourceToken.AccentBrush,
                    IconKey = ResourceToken.WarningGeometry,
                    StyleKey = "MessageBoxCustom"
                });

                return;
            }
            AvailableGradient.Remove(gradient);
            CurrentSelectedGradient = AvailableGradient.Last();
            WriteGradientCollectionJson();

            //AvailableGifs.Clear();
            //foreach (var palette in LoadPaletteIfExists())
            //{
            //    AvailablePallete.Add(palette);
            //}
            //CurrentOutput.OutputSelectedChasingPalette = 0;
            //CurrentActivePalette = AvailablePallete.First();
        }

        private void SaveCurrentEditedPalette(string param)
        {
            ////AvailablePallete[CurrentOutput.OutputSelectedChasingPalette] = CurrentActivePalette;
            //var lastSelectedPaletteIndex = CurrentOutput.OutputSelectedChasingPalette;
            //if (param == "save")
            //{
            //    //WritePaletteCollectionJson();
            //}

            ////reload all available palette;
            //AvailablePallete.Clear();

            //foreach (var palette in LoadPaletteIfExists())
            //{
            //    AvailablePallete.Add(palette);
            //}
            //CurrentOutput.OutputSelectedChasingPalette = lastSelectedPaletteIndex;
            ////CurrentActivePalette = AvailablePallete[CurrentOutput.OutputSelectedChasingPalette];
            ////var result = HandyControl.Controls.MessageBox.Show(new MessageBoxInfo {
            ////    Message = "Bạn có muốn ghi đè lên dải màu hiện tại?",
            ////    Caption = "Lưu dải màu",
            ////    Button = MessageBoxButton.YesNo,
            ////    IconBrushKey = ResourceToken.AccentBrush,
            ////    IconKey = ResourceToken.AskGeometry,
            ////    StyleKey = "MessageBoxCustom"
            ////});
            ////if (result == MessageBoxResult.Yes) // overwrite current palette
            ////{
            ////    var activePalette = CurrentOutput.OutputCurrentActivePalette;
            ////    CurrentActivePaletteCard.Thumbnail = activePalette;
            ////    SetCurrentDeviceSelectedPalette(CurrentActivePaletteCard);
            ////    WritePaletteCollectionJson();
            ////    //reload all available palette;
            ////    AvailablePallete.Clear();
            ////    foreach (var palette in LoadPaletteIfExists())
            ////    {
            ////        AvailablePallete.Add(palette);
            ////    }

            ////    CurrentCustomZoneColor.Clear();
            ////    foreach (var color in CurrentOutput.OutputCurrentActivePalette)
            ////    {
            ////        CurrentCustomZoneColor.Add(color);
            ////    }
            ////    RaisePropertyChanged(nameof(CurrentCustomZoneColor));
            ////}

            ////else // open create new dialog
            ////{
            ////OpenCreateNewDialog();
            ////}
        }

        public void OpenEditPaletteDialog(string praram)
        {
            var window = new PaletteEditWindow(praram);
            window.Owner = System.Windows.Application.Current.MainWindow;
            CurrentEditingColors = new ObservableCollection<Color>();
            //foreach (var color in CurrentOutput.OutputCurrentActivePalette.Colors)
            //{
            //    CurrentEditingColors.Add(color);
            //}
            //CurrentCustomZoneColor = palette;

            RaisePropertyChanged(nameof(CurrentEditingColors));
            window.ShowDialog();
        }

        public void OpenCreateNewDialog()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"AddNewPaletteWindow"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }

        public void OpenExportNewEffectWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"ExportNewColorEffectWindow"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }

        public void OpenFanSpeedPlotWindows()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"FanSpeedPlotWindow"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }
        public void OpenAvailableUpdateListWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"UpdateSelectionWindow"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }
        private int _oOTBStage;
        public int OOTBStage {
            get { return _oOTBStage; }
            set { _oOTBStage = value; RaisePropertyChanged(); }
        }
        private OOTBWindow OOTBWindows { get; set; }
        public void OpenLEDSteupSelectionWindows()
        {
            OOTBWindows = new OOTBWindow();
            OOTBWindows.Owner = System.Windows.Application.Current.MainWindow;
            OOTBWindows.ShowDialog();
        }


        //public void SetCurrentDeviceSelectedPalette(IColorPaletteCard palette)
        //{
        //    if (palette != null)
        //    {
        //        for (var i = 0; i < CurrentOutput.OutputCurrentActivePalette.Length; i++)
        //        {
        //            CurrentOutput.OutputCurrentActivePalette[i] = palette.Thumbnail[i];

        //        }

        //        RaisePropertyChanged(nameof(AvailablePallete));
        //        RaisePropertyChanged(nameof(CurrentOutput.OutputCurrentActivePalette));
        //        WriteDeviceInfoJson();
        //    }

        //}

        //public void SnapShot()
        //{
        //    int counter = 0;
        //    byte[] snapshot = new byte[256];
        //    foreach (IDeviceSpot spot in PreviewSpots)
        //    {
        //        snapshot[counter++] = spot.Red;
        //        snapshot[counter++] = spot.Green;
        //        snapshot[counter++] = spot.Blue;
        //        // counter++;
        //    }
        //    CurrentOutput.SnapShot = snapshot;
        //    RaisePropertyChanged(() => CurrentDevice.SnapShot);
        //}
        public void SetCustomColor(int index)
        {
            //if (CurrentActivePalette == null)
            //{
            //    CurrentActivePalette = new ColorPalette();
            //}
            //CurrentEditingColors[index] = CurrentPickedColor;
            //CurrentActivePalette.SetColor(index, CurrentPickedColor);
            //CurrentOutput.OutputCurrentActivePalette.SetColor(index, CurrentPickedColor);

            //RaisePropertyChanged(nameof(CurrentActivePalette));
            //RaisePropertyChanged(nameof(CurrentOutput.OutputCurrentActivePalette));
        }

        public void CurrentSpotSetVIDChanged()
        {
            //foreach (var spot in PreviewSpots)
            //{
            //    CurrentDevice.VirtualIndex[spot.id] = spot.VID;

            //}

            //WriteDeviceInfoJson();
        }

        public void DeviceRectSavePosition()
        {
            //save current device rect position to json database
            //CurrentDevice.DeviceRectLeft = DeviceRectX;
            //CurrentDevice.DeviceRectTop = DeviceRectY;
            // CurrentDevice.DeviceRectWidth = DeviceRectWidth;
            //CurrentDevice.DeviceRectHeight = DeviceRectHeight;
            // CurrentDevice.DeviceScale = DeviceScale;
            //RaisePropertyChanged(() => CurrentDevice.DeviceRectLeft);
            //RaisePropertyChanged(() => CurrentDevice.DeviceRectTop);
            // RaisePropertyChanged(() => CurrentDevice.DeviceRectWidth);
            // RaisePropertyChanged(() => CurrentDevice.DeviceRectHeight);
            // RaisePropertyChanged(() => CurrentDevice.DeviceScale);
        }

        public void DFU()
        {
            foreach (var serialStream in SerialStreams)
            {
                //if (CurrentDevice.ParrentLocation != 151293) // device is in hub object
                //{
                //    if (serialStream.ID == CurrentDevice.ParrentLocation)
                //        serialStream.DFU();
                //}
                //else
                //{
                //    if (serialStream.ID == CurrentDevice.DeviceID)
                //        serialStream.DFU();
                //}
            }
        }

        private int _dFUProgress;

        public int DFUProgress {
            get { return _dFUProgress; }
            set
            {
                _dFUProgress = value;
                if (value == 75)
                {
                    DFU();
                }
            }
        }

        private void CreateFWToolsFolderAndFiles()
        {
            if (!Directory.Exists(JsonFWToolsFileNameAndPath))
            {
                Directory.CreateDirectory(JsonFWToolsFileNameAndPath);
                ResourceHlprs.CopyResource("adrilight.Tools.FWTools.busybox.exe", Path.Combine(JsonFWToolsFileNameAndPath, "busybox.exe"));
                ResourceHlprs.CopyResource("adrilight.Tools.FWTools.CH375DLL.dll", Path.Combine(JsonFWToolsFileNameAndPath, "CH375DLL.dll"));
                ResourceHlprs.CopyResource("adrilight.Tools.FWTools.libgcc_s_sjlj-1.dll", Path.Combine(JsonFWToolsFileNameAndPath, "libgcc_s_sjlj-1.dll"));
                ResourceHlprs.CopyResource("adrilight.Tools.FWTools.libusb-1.0.dll", Path.Combine(JsonFWToolsFileNameAndPath, "libusb-1.0.dll"));
                ResourceHlprs.CopyResource("adrilight.Tools.FWTools.libusbK.dll", Path.Combine(JsonFWToolsFileNameAndPath, "libusbK.dll"));
                ResourceHlprs.CopyResource("adrilight.Tools.FWTools.vnproch55x.exe", Path.Combine(JsonFWToolsFileNameAndPath, "vnproch55x.exe"));
                //required fw version
            }
            CreateRequiredFwVersionJson();
        }

        private void CreateRequiredFwVersionJson()
        {
            IDeviceFirmware ABR1p = new DeviceFirmware() {
                Name = "ABR1p.hex",
                Version = "1.0.6",
                TargetHardware = "ABR1p",
                TargetDeviceType = "ABBASIC",
                Geometry = "binary",
                ResourceName = "adrilight.DeviceFirmware.ABR1p.hex"
            };
            //IDeviceFirmware ABR1e = new DeviceFirmware() {
            //    Name = "ABR1e.hex",
            //    Version = "1.0.5",
            //    TargetHardware = "ABR1e",
            //    TargetDeviceType = "ABBASIC",
            //    Geometry = "binary",
            //    ResourceName = "adrilight.DeviceFirmware.ABR1e.hex"
            //};

            //IDeviceFirmware ABR2p = new DeviceFirmware() {
            //    Name = "ABR2p.hex",
            //    Version = "1.0.5",
            //    TargetHardware = "ABR2p",
            //    TargetDeviceType = "ABBASIC",
            //    Geometry = "binary",
            //    ResourceName = "adrilight.DeviceFirmware.ABR2p.hex"
            //};
            IDeviceFirmware ABR2e = new DeviceFirmware() {
                Name = "ABR2e.hex",
                Version = "1.0.6",
                TargetHardware = "ABR2e",
                TargetDeviceType = "ABBASIC",
                Geometry = "binary",
                ResourceName = "adrilight.DeviceFirmware.ABR2e.hex"
            };
            IDeviceFirmware AER1e = new DeviceFirmware() {
                Name = "AER1e.hex",
                Version = "1.0.4",
                TargetHardware = "AER1e",
                TargetDeviceType = "ABEDGE",
                Geometry = "binary",
                ResourceName = "adrilight.DeviceFirmware.AER1e.hex"
            };
            IDeviceFirmware AER2e = new DeviceFirmware() {
                Name = "AER2e.hex",
                Version = "1.0.4",
                TargetHardware = "AER2e",
                TargetDeviceType = "ABEDGE",
                Geometry = "binary",
                ResourceName = "adrilight.DeviceFirmware.AER2e.hex"
            };
            //IDeviceFirmware AER2p = new DeviceFirmware() {
            //    Name = "AER2p.hex",
            //    Version = "1.0.3",
            //    TargetHardware = "AER2p",
            //    TargetDeviceType = "ABEDGE",
            //    Geometry = "binary",
            //    ResourceName = "adrilight.DeviceFirmware.AER2p.hex"
            //};
            IDeviceFirmware AFR1g = new DeviceFirmware() {
                Name = "AFR1g.hex",
                Version = "1.0.3",
                TargetHardware = "AFR1g",
                TargetDeviceType = "ABFANHUB",
                Geometry = "binary",
                ResourceName = "adrilight.DeviceFirmware.AFR1g.hex"
            };
            IDeviceFirmware AFR2g = new DeviceFirmware() {
                Name = "AFR2g.hex",
                Version = "1.0.5",
                TargetHardware = "AFR2g",
                TargetDeviceType = "ABFANHUB",
                Geometry = "binary",
                ResourceName = "adrilight.DeviceFirmware.AFR2g.hex"
            };
            IDeviceFirmware AFR3g = new DeviceFirmware() {
                Name = "AFR3g.hex",
                Version = "1.0.6",
                TargetHardware = "AFR3g",
                TargetDeviceType = "ABFANHUB",
                Geometry = "binary",
                ResourceName = "adrilight.DeviceFirmware.AFR3g.hex"
            };
            IDeviceFirmware AHR2g = new DeviceFirmware() {
                Name = "AHR2g.hex",
                Version = "1.0.1",
                TargetHardware = "AHR1g",
                TargetDeviceType = "ABHUBV3",
                Geometry = "binary",
                ResourceName = "adrilight.DeviceFirmware.AHR2g.hex"
            };
            IDeviceFirmware ARR1p = new DeviceFirmware() {
                Name = "ARR1p.hex",
                Version = "1.0.2",
                TargetHardware = "ARR1p",
                TargetDeviceType = "ABRP",
                Geometry = "binary",
                ResourceName = "adrilight.DeviceFirmware.ARR1p.hex"
            };
            var firmwareList = new List<IDeviceFirmware>();
            firmwareList.Add(ABR1p);
            //firmwareList.Add(ABR2p);
            //firmwareList.Add(ABR1e);
            firmwareList.Add(ABR2e);
            firmwareList.Add(AER1e);
            firmwareList.Add(AER2e);
            firmwareList.Add(AFR1g);
            firmwareList.Add(AHR2g);
            firmwareList.Add(ARR1p);
            firmwareList.Add(AFR2g);
            firmwareList.Add(AFR3g);
            //+-------------------------------------------------+--------+----+----+-------+
            //| Ambino Basic CH552P without PowerLED Support    | CH552P | 32 | 14 | ABR1p |
            //+-------------------------------------------------+--------+----+----+-------+
            //| Ambino Basic CH552E Without PowerLED Support    | CH552E | 15 | 17 | ABR1e |
            //+-------------------------------------------------+--------+----+----+-------+
            //| Ambino Basic CH552E(rev2) With PowerLED Support | CH552e | 14 | 15 | ARR2e |
            //+-------------------------------------------------+--------+----+----+-------+
            //| Ambino EDGE CH552E Without PowerLED Support     | CH552E | 15 | 17 | AER1e |
            //+-------------------------------------------------+--------+----+----+-------+
            //| Ambino Basic CH552P(rev2) With PowerLED Support | CH552P |    |    | ABR2p |
            //+-------------------------------------------------+--------+----+----+-------+
            //| Ambino EDGE CH552P (rev2) With PowerLED Support | CH552P |    |    | AER2p |
            //+-------------------------------------------------+--------+----+----+-------+
            //| Ambino FanHUB CH552G rev1                       | CH552G |    |    | AFR1g |
            //+-------------------------------------------------+--------+----+----+-------+
            //| Ambino FanHUB CH552G rev2                       | CH552G |    |    | AFR2g |
            //+-------------------------------------------------+--------+----+----+-------+
            //| Ambino HUBV3 CH552G rev1                        | CH552G |    |    | AHR1g |
            //+-------------------------------------------------+--------+----+----+-------+
            //| Ambino RainPow CH552P rev1                      | CH552P |    |    | ARR1p |
            //+-------------------------------------------------+--------+----+----+-------+
            //| Ambino HUBV3 CH552G rev2                        | CH552G |    |    | AHR2g |
            //+-------------------------------------------------+--------+----+----+-------+

            //foreach(var device in DefaultDeviceCollection.AvailableDefaultDevice())
            //{
            //    string requiredFwVersion = "1.0.0";
            //    switch (device.HardwareVersion)
            //    {
            //        case "ABR1p":
            //            requiredFwVersion = "1.0.5";
            //            break;
            //        case "ABR2p":
            //            requiredFwVersion = "1.0.5";
            //            break;
            //        case "ABR1e":
            //            requiredFwVersion = "1.0.5";
            //            break;
            //        case "AER1e":
            //            requiredFwVersion = "1.0.3";
            //            break;
            //        case "AER2p":
            //            requiredFwVersion = "1.0.3";
            //            break;
            //        case "AFR1g":
            //            requiredFwVersion = "1.0.2";
            //            break;
            //        case "AHR1g":
            //            requiredFwVersion = "1.0.1";
            //            break;
            //        case "ARR1p":
            //            requiredFwVersion = "1.0.1";
            //            break;
            //    }

            //}
            var requiredFwVersionjson = JsonConvert.SerializeObject(firmwareList, Formatting.Indented);
            File.WriteAllText(JsonFWToolsFWListFileNameAndPath, requiredFwVersionjson);
        }

        private List<IGifCard> LoadGifIfExist()
        {
            var loadedGifs = new List<IGifCard>();
            AvailableGifs = new ObservableCollection<IGifCard>();

            if (Directory.Exists(JsonGifsFileNameAndPath))
            {
                //check if gif exist
                if (File.Exists(JsonGifsCollectionFileNameAndPath))
                {
                    var json = File.ReadAllText(JsonGifsCollectionFileNameAndPath);

                    var existedGif = JsonConvert.DeserializeObject<List<GifCard>>(json, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
                    foreach (var gif in existedGif)
                    {
                        loadedGifs.Add(gif);
                    }
                }
            }
            else
            {    //create gif dirrectory
                Directory.CreateDirectory(JsonGifsFileNameAndPath);
                //coppy embeded gif to adrilight folder in appdata

                ResourceHlprs.CopyResource("adrilight.Gif.Rainbow.gif", Path.Combine(JsonGifsFileNameAndPath, "Rainbow.gif"));
                ResourceHlprs.CopyResource("adrilight.Gif.BitOcean.gif", Path.Combine(JsonGifsFileNameAndPath, "BitOcean.gif"));
                ResourceHlprs.CopyResource("adrilight.Gif.DigitalBlue.gif", Path.Combine(JsonGifsFileNameAndPath, "DigitalBlue.gif"));
                ResourceHlprs.CopyResource("adrilight.Gif.HexWave.gif", Path.Combine(JsonGifsFileNameAndPath, "HexWave.gif"));
                ResourceHlprs.CopyResource("adrilight.Gif.Hypnotic.gif", Path.Combine(JsonGifsFileNameAndPath, "Hypnotic.gif"));
                ResourceHlprs.CopyResource("adrilight.Gif.Plasma.gif", Path.Combine(JsonGifsFileNameAndPath, "Plasma.gif"));
                ResourceHlprs.CopyResource("adrilight.Gif.Blob.gif", Path.Combine(JsonGifsFileNameAndPath, "Blob.gif"));

                return new()
            {
            new GifCard()
            {
                Name = "Rainbow",
                Source = Path.Combine(JsonGifsFileNameAndPath,"Rainbow.gif"),
                Description = "Rainbow Spiral Gif",
                Owner = "Dribble",
            },
              new GifCard()
            {
                Name = "BitOcean",
                Source = Path.Combine(JsonGifsFileNameAndPath,"BitOcean.gif"),
                Description = "Application Resources",
                Owner = "GIFER",
            },
                 new GifCard()
            {
                Name = "DigitalBlue",
                Source = Path.Combine(JsonGifsFileNameAndPath,"DigitalBlue.gif"),
                Description = "Application Resources",
                Owner = "GIFER",
            },
                    new GifCard()
            {
                Name = "HexWave",
                Source = Path.Combine(JsonGifsFileNameAndPath,"HexWave.gif"),
                Description = "Application Resources",
                Owner = "GIFER",
            },
                       new GifCard()
            {
                Name = "Hypnotic",
                Source = Path.Combine(JsonGifsFileNameAndPath,"Hypnotic.gif"),
                Description ="Application Resources",
                Owner = "GIFER",
            },
                          new GifCard()
            {
                Name = "Plasma",
                Source = Path.Combine(JsonGifsFileNameAndPath,"Plasma.gif"),
                Description = "Application Resources",
                Owner = "GIFER",
            },
                                         new GifCard()
            {
                Name = "Blob",
                Source = Path.Combine(JsonGifsFileNameAndPath,"Blob.gif"),
                Description = "Application Resources",
                Owner = "GIFER",
            },
        };
            }
            return loadedGifs;
        }




        public void LoadAvailableLightingMode()
        {
            //AvailableLightingMode = new ObservableCollection<ILightingMode>();
            //var screencapture = new LightingMode { Name = "Screen Capture", Geometry = "ambilight", Description = "Screen sampling to LED" };
            //var palette = new LightingMode { Name = "Color Palette", Geometry = "colorpalette", Description = "Screen sampling to LED" };
            //var music = new LightingMode { Name = "Music Reactive", Geometry = "music", Description = "Screen sampling to LED" };
            //var staticcolor = new LightingMode { Name = "Static Color", Geometry = "static", Description = "Screen sampling to LED" };
            //var gifxelation = new LightingMode { Name = "Gifxelation", Geometry = "gifxelation", Description = "Screen sampling to LED" };
            //var composition = new LightingMode { Name = "Composition", Geometry = "gifxelation", Description = "Screen sampling to LED" };
            //AvailableLightingMode.Add(screencapture);
            //AvailableLightingMode.Add(palette);
            //AvailableLightingMode.Add(music);
            //AvailableLightingMode.Add(staticcolor);
            //AvailableLightingMode.Add(gifxelation);
        }
        private void LoadAvailableBaudRate()
        {
            AvailableBaudrates = new ObservableCollection<int> {
                9600,
                19200,
                38400,
                57600,
                115200,
                230400,
                460800,
                576000,
                921600,
                1000000,
                1500000,
                2000000,
                2500000,
            };
        }
        private void LoadAvailableChasingPatterns()
        {
            AvailableMotionDirrection = new List<string>();
            AvailableMotionDirrection.Add("Normal");
            AvailableMotionDirrection.Add("Reverse");
            AvailableMotionSpeed = new List<double>();
            AvailableMotionSpeed.Add(0.25);
            AvailableMotionSpeed.Add(0.50);
            AvailableMotionSpeed.Add(1.00);
            AvailableMotionSpeed.Add(1.25);
            AvailableMotionSpeed.Add(1.50);
            AvailableMotionSpeed.Add(2.00);
            AvailableMotions = new ObservableCollection<ITimeLineDataItem>();
            var bouncing = new MotionCard() {
                Name = "Bouncing",
                StartFrame = 5,
                EndFrame = 125,
                TrimEnd = 0,
                TrimStart = 0,
                OriginalDuration = 120,
                Color = Color.FromRgb(0, 0, 255)

            };
            var bouncing2 = new MotionCard() {
                Name = "Chasing",
                StartFrame = 20,
                EndFrame = 300,
                TrimEnd = 0,
                TrimStart = 0,
                OriginalDuration = 280,
                Color = Color.FromRgb(0, 255, 255)
            };
            AvailableMotions.Add(bouncing);
            AvailableMotions.Add(bouncing2);
        }
        private void LoadAvailablePalettes()
        {
            AvailablePallete = new ObservableCollection<IColorPalette>();
            foreach (var loadedPalette in LoadPaletteIfExists())
            {
                AvailablePallete.Add(loadedPalette);
            }
        }
        private void LoadAvailableAnimations()
        {
            AvailableGifs = new ObservableCollection<IGifCard>();
            foreach (var loadedGif in LoadGifIfExist())
            {
                AvailableGifs.Add(loadedGif);
            }
            WriteGifCollectionJson();
        }
        private void LoadAvailableGradients()
        {
            AvailableGradient = new ObservableCollection<IGradientColorCard>();
            foreach (var gradient in LoadGradientIfExists())
            {
                AvailableGradient.Add(gradient);
            }
        }
        private void LoadAvailableSolidColors()
        {
            AvailableSolidColors = new ObservableCollection<Color>();
            foreach (var color in LoadSolidColorIfExists())
            {
                AvailableSolidColors.Add(color);
            }
        }
        private void LoadAvailableProfiles()
        {
            AvailableProfiles = new ObservableCollection<IDeviceProfile>();
            foreach (var profile in LoadDeviceProfileIfExist())
            {
                AvailableProfiles.Add(profile);
            }
            WriteDeviceProfileCollection();
        }

        public void LoadData()
        {
            if (ResourceHlprs == null)
                ResourceHlprs = new ResourceHelpers();
            LoadAvailableLightingMode();
            LoadAvailablePalettes();
            LoadAvailableAnimations();
            LoadAvailableBaudRate();
            LoadAvailableChasingPatterns();
            LoadAvailableGradients();
            LoadAvailableProfiles();
            LoadAvailableSolidColors();


        }

        public List<IColorPalette> LoadPaletteIfExists()
        {
            if (!Directory.Exists(PalettesCollectionFolderPath))
            {

                //create default palette
                var palettes = new List<IColorPalette>();
                IColorPalette rainbow = new ColorPalette("Full Rainbow", "Zooey", "RGBPalette16", "Default Color Palette by Ambino", DefaultColorCollection.rainbow);
                IColorPalette police = new ColorPalette("Police", "Zooey", "RGBPalette16", "Default Color Palette by Ambino", DefaultColorCollection.police);
                IColorPalette forest = new ColorPalette("Forest", "Zooey", "RGBPalette16", "Default Color Palette by Ambino", DefaultColorCollection.forest);
                IColorPalette aurora = new ColorPalette("Aurora", "Zooey", "RGBPalette16", "Default Color Palette by Ambino", DefaultColorCollection.aurora);
                IColorPalette iceandfire = new ColorPalette("Ice and Fire", "Zooey", "RGBPalette16", "Default Color Palette by Ambino", DefaultColorCollection.iceandfire);
                IColorPalette scarlet = new ColorPalette("Scarlet", "Zooey", "RGBPalette16", "Default Color Palette by Ambino", DefaultColorCollection.scarlet);
                IColorPalette party = new ColorPalette("Party", "Zooey", "RGBPalette16", "Default Color Palette by Ambino", DefaultColorCollection.party);
                IColorPalette cloud = new ColorPalette("Cloud", "Zooey", "RGBPalette16", "Default Color Palette by Ambino", DefaultColorCollection.cloud);
                IColorPalette france = new ColorPalette("France", "Zooey", "RGBPalette16", "Default Color Palette by Ambino", DefaultColorCollection.france);
                IColorPalette badtrip = new ColorPalette("Bad Trip", "Zooey", "RGBPalette16", "Default Color Palette by Ambino", DefaultColorCollection.badtrip);
                IColorPalette lemon = new ColorPalette("Lemon", "Zooey", "RGBPalette16", "Default Color Palette by Ambino", DefaultColorCollection.lemon);

                palettes.Add(rainbow);
                palettes.Add(police);
                palettes.Add(forest);
                palettes.Add(aurora);
                palettes.Add(iceandfire);
                palettes.Add(scarlet);
                palettes.Add(party);
                palettes.Add(cloud);
                palettes.Add(france);
                palettes.Add(badtrip);
                palettes.Add(lemon);
                //create colorPalette directory
                Directory.CreateDirectory(PalettesCollectionFolderPath);
                foreach (var palette in palettes)
                {
                    var json = JsonConvert.SerializeObject(palette, new JsonSerializerSettings() {
                        TypeNameHandling = TypeNameHandling.Auto
                    });
                    File.WriteAllText(Path.Combine(PalettesCollectionFolderPath, palette.Name + ".col"), json);
                }


                return palettes;
                //coppy all internal palettes to local 

            }
            string[] exisetedPalettes = Directory.GetFiles(PalettesCollectionFolderPath);
            var loadedPaletteCard = new List<IColorPalette>();
            foreach (var paletteFile in exisetedPalettes)
            {
                var json = File.ReadAllText(paletteFile);

                var existPaletteCard = JsonConvert.DeserializeObject<ColorPalette>(json);

                loadedPaletteCard.Add(existPaletteCard);

            }


            return loadedPaletteCard;
        }

        public List<IDeviceProfile> LoadDeviceProfileIfExist()
        {
            var availableDefaultDevice = new List<IDeviceSettings>();
            var loadedProfiles = new List<IDeviceProfile>();
            if (!File.Exists(JsonDeviceProfileFileNameAndPath))
            {
                var defaultFanHubProfile = new DeviceProfile {
                    Name = "Fan HUB Default",
                    Owner = "Ambino",
                    ProfileUID = Guid.NewGuid().ToString(),
                    Geometry = "profile",
                    DeviceType = "ABFANHUB",
                    Description = "Default Profile for Ambino Fan HUB",
                    //OutputSettings = availableDefaultDevice.ambinoFanHub.AvailableOutputs
                };
                var defaultAmbinobasic24 = new DeviceProfile {
                    Name = "Ambino Basic 24inch Default",
                    Owner = "Ambino",
                    ProfileUID = Guid.NewGuid().ToString(),
                    Geometry = "profile",
                    DeviceType = "ABBASIC24",
                    Description = "Default Profile for Ambino Basic",
                    //OutputSettings = availableDefaultDevice.AmbinoBasic24Inch.AvailableOutputs
                };
                var defaultAmbinobasic27 = new DeviceProfile {
                    Name = "Ambino Basic 27inch Default",
                    Owner = "Ambino",
                    ProfileUID = Guid.NewGuid().ToString(),
                    Geometry = "profile",
                    DeviceType = "ABBASIC27",
                    Description = "Default Profile for Ambino Basic",
                    //OutputSettings = availableDefaultDevice.AmbinoBasic27Inch.AvailableOutputs
                };
                var defaultAmbinobasic29 = new DeviceProfile {
                    Name = "Ambino Basic 29inch Default",
                    Owner = "Ambino",
                    ProfileUID = Guid.NewGuid().ToString(),
                    Geometry = "profile",
                    DeviceType = "ABBASIC29",
                    Description = "Default Profile for Ambino Basic",
                    //OutputSettings = availableDefaultDevice.AmbinoBasic29Inch.AvailableOutputs
                };
                var defaultAmbinobasic32 = new DeviceProfile {
                    Name = "Ambino Basic 32inch Default",
                    Owner = "Ambino",
                    ProfileUID = Guid.NewGuid().ToString(),
                    Geometry = "profile",
                    DeviceType = "ABBASIC32",
                    Description = "Default Profile for Ambino Basic",
                    //OutputSettings = availableDefaultDevice.AmbinoBasic32Inch.AvailableOutputs
                };
                var defaultAmbinobasic34 = new DeviceProfile {
                    Name = "Ambino Basic 34inch Default",
                    Owner = "Ambino",
                    ProfileUID = Guid.NewGuid().ToString(),
                    Geometry = "profile",
                    DeviceType = "ABBASIC34",
                    Description = "Default Profile for Ambino Basic",
                    //OutputSettings = availableDefaultDevice.AmbinoBasic34Inch.AvailableOutputs
                };
                var defaultAmbinoedge1m2 = new DeviceProfile {
                    Name = "Ambino EDGE 1m2 Default",
                    Owner = "Ambino",
                    ProfileUID = Guid.NewGuid().ToString(),
                    Geometry = "profile",
                    DeviceType = "ABEDGE1.2",
                    Description = "Default Profile for Ambino EDGE",
                    //OutputSettings = availableDefaultDevice.AmbinoEDGE1M2.AvailableOutputs
                };
                var defaultAmbinoedge2m = new DeviceProfile {
                    Name = "Ambino EDGE 2m Default",
                    Owner = "Ambino",
                    ProfileUID = Guid.NewGuid().ToString(),
                    Geometry = "profile",
                    DeviceType = "ABEDGE2.0",
                    Description = "Default Profile for Ambino EDGE",
                    //OutputSettings = availableDefaultDevice.AmbinoEDGE2M.AvailableOutputs
                };
                loadedProfiles.Add(defaultFanHubProfile);
                loadedProfiles.Add(defaultAmbinobasic24);
                loadedProfiles.Add(defaultAmbinobasic27);
                loadedProfiles.Add(defaultAmbinobasic29);
                loadedProfiles.Add(defaultAmbinobasic32);
                loadedProfiles.Add(defaultAmbinobasic34);
                loadedProfiles.Add(defaultAmbinoedge1m2);
                loadedProfiles.Add(defaultAmbinoedge2m);
            }
            else
            {
                var json = File.ReadAllText(JsonDeviceProfileFileNameAndPath);

                var existedProfile = JsonConvert.DeserializeObject<List<DeviceProfile>>(json, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
                if (existedProfile != null)
                {
                    foreach (var profile in existedProfile)
                    {
                        loadedProfiles.Add(profile);
                    }
                }

            }

            return loadedProfiles;
        }

        public List<IAutomationSettings> LoadAutomationIfExist()
        {
            var loadedAutomations = new List<IAutomationSettings>();

            //if (!File.Exists(JsonAutomationFileNameAndPath))
            //{
            //    var brightnessUpAllDevices = new List<IActionSettings>(); // load all profile 0 on the list apply to all devices
            //    var brightnessDownAllDevices = new List<IActionSettings>(); // load all profile 0 on the list apply to all devices
            //    if (AvailableDevices.Count > 0 && AvailableProfiles.Count > 0)
            //    {
            //        foreach (var device in AvailableDevices)
            //        {
            //            IActionSettings brightnessUp = new ActionSettings {
            //                ActionType = "Brightness Control",
            //                TargetDeviceUID = device.DeviceUID,
            //                TargetDeviceName = device.DeviceName,
            //                TargetDeviceType = device.DeviceType,
            //                ActionParameter = new ActionParameter { Name= "Brightness Up", Type="brightness",Value="up"  },

            //            };
            //            IActionSettings brightnessDown = new ActionSettings {
            //                ActionType = "Brightness Control",
            //                TargetDeviceUID = device.DeviceUID,
            //                TargetDeviceName = device.DeviceName,
            //                TargetDeviceType = device.DeviceType,
            //                ActionParameter = new ActionParameter { Name = "Brightness Down", Type = "brightness", Value = "down" },

            //            };
            //            brightnessUpAllDevices.Add(brightnessUp);
            //            brightnessDownAllDevices.Add(brightnessDown);
            //        }

            //    }

            //    var BrightnessUpAllDevicesAutomation = new AutomationSettings {
            //        Name = "Brightness Up",
            //        Actions = brightnessUpAllDevices,
            //        ConditionTypeIndex = 0,//key stroke detection
            //        Modifiers = new List<IModifiersType> { new ModifiersType { Name ="CTRL", ModifierKey=NonInvasiveKeyboardHookLibrary.ModifierKeys.Control,IsChecked=true }, new ModifiersType { Name = "SHIFT", ModifierKey = NonInvasiveKeyboardHookLibrary.ModifierKeys.Shift, IsChecked = true } },
            //        Condition = 120// F9

            //    };
            //    var BrightnessDownAllDevicesAutomation = new AutomationSettings {
            //        Name = "Brightness Down",
            //        Actions = brightnessDownAllDevices,
            //        Modifiers = new List<IModifiersType> { new ModifiersType { Name = "CTRL", ModifierKey = NonInvasiveKeyboardHookLibrary.ModifierKeys.Control, IsChecked = true }, new ModifiersType { Name = "SHIFT", ModifierKey = NonInvasiveKeyboardHookLibrary.ModifierKeys.Shift, IsChecked = true } },
            //        ConditionTypeIndex = 0,//key stroke detection
            //        Condition = 121// F10

            //    };
            //    loadedAutomations.Add(BrightnessUpAllDevicesAutomation);
            //    loadedAutomations.Add(BrightnessDownAllDevicesAutomation);

            //}
            //else
            //{
            if (File.Exists(JsonAutomationFileNameAndPath))
            {
                var json = File.ReadAllText(JsonAutomationFileNameAndPath);

                var existedAutomation = JsonConvert.DeserializeObject<List<AutomationSettings>>(json, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
                foreach (var automation in existedAutomation)
                {
                    loadedAutomations.Add(automation);
                }
            }

            return loadedAutomations;
        }

        public Color[] LoadSolidColorIfExists()
        {
            if (!File.Exists(JsonSolidColorFileNameAndPath))
            {
                Color[] colors =
            {
        (Color)ColorConverter.ConvertFromString("Red"),
        Color.FromArgb(255, 255, 192, 192),
        Color.FromArgb(255, 255, 224, 192),
        Color.FromArgb(255, 255, 255, 192),
        Color.FromArgb(255, 192, 255, 192),
        Color.FromArgb(255, 192, 255, 255),
        Color.FromArgb(255, 192, 192, 255),
        Color.FromArgb(255, 255, 192, 255),
        Color.FromArgb(255, 224, 224, 224),
        Color.FromArgb(255, 255, 128, 128),
        Color.FromArgb(255, 255, 192, 128),
        Color.FromArgb(255, 255, 255, 128),
        Color.FromArgb(255, 128, 255, 128),
        Color.FromArgb(255, 128, 255, 255),
        Color.FromArgb(255, 128, 128, 255),
        Color.FromArgb(255, 255, 128, 255),
        (Color)ColorConverter.ConvertFromString("Silver"),
        (Color)ColorConverter.ConvertFromString("Red"),
        Color.FromArgb(255, 255, 128, 0),
        (Color)ColorConverter.ConvertFromString("Yellow"),
        (Color)ColorConverter.ConvertFromString("Lime"),
        (Color)ColorConverter.ConvertFromString("Cyan"),
        (Color)ColorConverter.ConvertFromString("Blue"),
        (Color)ColorConverter.ConvertFromString("Fuchsia"),
        (Color)ColorConverter.ConvertFromString("Gray"),
        Color.FromArgb(255, 192, 0, 0),
        Color.FromArgb(255, 192, 64, 0),
        Color.FromArgb(255, 192, 192, 0),
        Color.FromArgb(255, 0, 192, 0),
        Color.FromArgb(255, 0, 192, 192),
        Color.FromArgb(255, 0, 0, 192),
        Color.FromArgb(255, 192, 0, 192),
        Color.FromArgb(255, 64, 64, 64),
        (Color)ColorConverter.ConvertFromString("Maroon"),
        Color.FromArgb(255, 128, 64, 0),
        (Color)ColorConverter.ConvertFromString("Olive"),
        (Color)ColorConverter.ConvertFromString("Green"),
        (Color)ColorConverter.ConvertFromString("Teal"),
        (Color)ColorConverter.ConvertFromString("Navy"),
        (Color)ColorConverter.ConvertFromString("Purple"),
        (Color)ColorConverter.ConvertFromString("Black"),
        Color.FromArgb(255, 64, 0, 0),
        Color.FromArgb(255, 128, 64, 64),
        Color.FromArgb(255, 64, 64, 0),
        Color.FromArgb(255, 0, 64, 0),
        Color.FromArgb(255, 0, 64, 64),
        Color.FromArgb(255, 0, 0, 64),
        Color.FromArgb(255, 64, 0, 64),
    };
                return colors;
                WriteSolidColorJson();
            }
            else
            {
                var json = File.ReadAllText(JsonSolidColorFileNameAndPath);
                var existedSolidColor = JsonConvert.DeserializeObject<List<Color>>(json, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
                Color[] colors = new Color[existedSolidColor.Count];
                colors = existedSolidColor.ToArray();
                return colors;
            }
        }

        public List<IGradientColorCard> LoadGradientIfExists()
        {
            var gradientCards = new List<IGradientColorCard>();
            if (!File.Exists(JsonGradientFileNameAndPath))
            {
                //create default palette

                IGradientColorCard a = new GradientColorCard("Pastel 1", "Zooey", "RGBGradient", "Default Gradient Color by Ambino", System.Windows.Media.Color.FromRgb(254, 141, 198), System.Windows.Media.Color.FromRgb(254, 209, 199));
                IGradientColorCard b = new GradientColorCard("Pastel 2", "Zooey", "RGBGradient", "Default Gradient Color by Ambino", System.Windows.Media.Color.FromRgb(127, 0, 255), System.Windows.Media.Color.FromRgb(255, 0, 255));
                IGradientColorCard c = new GradientColorCard("Pastel 3", "Zooey", "RGBGradient", "Default Gradient Color by Ambino", System.Windows.Media.Color.FromRgb(251, 176, 64), System.Windows.Media.Color.FromRgb(249, 237, 50));
                IGradientColorCard d = new GradientColorCard("Pastel 4", "Zooey", "RGBGradient", "Default Gradient Color by Ambino", System.Windows.Media.Color.FromRgb(0, 161, 255), System.Windows.Media.Color.FromRgb(0, 255, 143));
                IGradientColorCard e = new GradientColorCard("Pastel 5", "Zooey", "RGBGradient", "Default Gradient Color by Ambino", System.Windows.Media.Color.FromRgb(238, 42, 123), System.Windows.Media.Color.FromRgb(255, 125, 184));
                IGradientColorCard f = new GradientColorCard("Pastel 6", "Zooey", "RGBGradient", "Default Gradient Color by Ambino", System.Windows.Media.Color.FromRgb(255, 0, 212), System.Windows.Media.Color.FromRgb(0, 221, 255));

                gradientCards.Add(a);
                gradientCards.Add(b);
                gradientCards.Add(c);
                gradientCards.Add(d);
                gradientCards.Add(e);
                gradientCards.Add(f);
            }
            else
            {
                var json = File.ReadAllText(JsonGradientFileNameAndPath);

                var existedGradient = JsonConvert.DeserializeObject<List<GradientColorCard>>(json);
                foreach (var gradient in existedGradient)
                {
                    gradientCards.Add(gradient);
                }
            }

            return gradientCards;

            //}

            //var json = File.ReadAllText(JsonPaletteFileNameAndPath);
            //var loadedPaletteCard = new List<IColorPaletteCard>();
            //var existPaletteCard = JsonConvert.DeserializeObject<List<ColorPaletteCard>>(json);
            //foreach (var paletteCard in existPaletteCard)
            //{
            //    loadedPaletteCard.Add(paletteCard);
            //}
        }

        public async void ShowAddNewWindow()
        {/*
           
            //Device Catergory to group type of device in the add new window, this could be moved to add new window open code
            var availableDefaultDevice = new List<IDeviceSettings>();
            AvailableDeviceCatergoryToAdd = new ObservableCollection<IDeviceCatergory>();
            IDeviceCatergory ambinoBasic = new DeviceCatergory {
                Description = "Ambino Ambient Lighting Kit",
                Name = "AMBINO BASIC",
                Geometry = "ambinobasic",
                Devices = new DeviceSettings[] { availableDefaultDevice.AmbinoBasic24Inch,
                                                 availableDefaultDevice.AmbinoBasic27Inch,
                                                 availableDefaultDevice.AmbinoBasic29Inch,
                                                 availableDefaultDevice.AmbinoBasic32Inch,
                                                 availableDefaultDevice.AmbinoBasic34Inch
                }
            };
            IDeviceCatergory ambinoEDGE = new DeviceCatergory {
                Description = "Ambino Ambient Lighting Kit",
                Name = "AMBINO EDGE",
                Geometry = "ambinoedge",
                Devices = new DeviceSettings[] { availableDefaultDevice.AmbinoEDGE1M2,
                                                 availableDefaultDevice.AmbinoEDGE2M
                }
            };
            IDeviceCatergory ambinoHUB = new DeviceCatergory {
                Description = "Ambino HUB Collection",
                Name = "AMBINO HUB",
                Geometry = "ambinohub",
                Devices = new DeviceSettings[] { availableDefaultDevice.ambinoHUBV2,
                                                 availableDefaultDevice.ambinoFanHub,
                                                 availableDefaultDevice.ambinoHUBV3
                }
            };
            IDeviceCatergory ambinoRainPow = new DeviceCatergory {
                Description = "Ambino RainPow",
                Name = "AMBINO RAINPOW",
                Geometry = "generaldevice",
                Devices = new DeviceSettings[] { availableDefaultDevice.ambinoRainPow
                }
            };

            AvailableDeviceCatergoryToAdd.Add(ambinoBasic);
            AvailableDeviceCatergoryToAdd.Add(ambinoEDGE);
            AvailableDeviceCatergoryToAdd.Add(ambinoHUB);
            AvailableDeviceCatergoryToAdd.Add(ambinoRainPow);

            if (AssemblyHelper.CreateInternalInstance($"View.{"AddNewDeviceWindow"}") is System.Windows.Window window)
            {
                CurrentAddDeviceWizardState = 0;
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }

            //System.Windows.Forms.Application.Restart();
            //    Process.GetCurrentProcess().Kill();
            */
        }

        private void DeleteSelectedDevice()
        {
            AvailableDevices.Remove(CurrentDevice);
            //WriteDeviceInfoJson();
            AmbinityClient.Dispose();
            System.Windows.Forms.Application.Restart();
            Process.GetCurrentProcess().Kill();
        }
        private void DeleteSelectedDevices()
        {
            foreach (var device in AvailableDevices.Where(p => p.IsSelected == true).ToList())
            {
                SelectedDeviceCount--;
                AvailableDevices.Remove(device);
            }
            ToolbarVisible = false;
            //WriteDeviceInfoJson();

        }








        /// <summary>
        /// Change View
        /// </summary>
        /// <param name="menuItem"></param>


        //public void WriteDeviceInfoJson()
        //{
        //    var devices = new List<IDeviceSettings>();
        //    foreach (var item in AvailableDevices)
        //    {
        //        if (!item.IsDummy)
        //            devices.Add(item);
        //    }

        //    var json = JsonConvert.SerializeObject(devices, new JsonSerializerSettings() {
        //        TypeNameHandling = TypeNameHandling.Auto
        //    });

        //    // Set Status to Locked
        //    _readWriteLock.EnterWriteLock();
        //    try
        //    {
        //        // Append text to the file
        //        using (StreamWriter sw = new StreamWriter(JsonDeviceFileNameAndPath))
        //        {
        //            sw.Write(json);
        //            sw.Close();
        //        }
        //    }
        //    finally
        //    {
        //        // Release lock
        //        _readWriteLock.ExitWriteLock();
        //    }
        //    //Directory.CreateDirectory(JsonPath);
        //    //await File.WriteAllTextAsync(JsonDeviceFileNameAndPath, json);
        //}

        ////public void WriteOpenRGBDeviceInfoJson()
        ////{
        ////    var devices = new List<Device>();
        ////    foreach (var item in AvailableOpenRGBDevices)
        ////    {
        ////        devices.Add(item);
        ////    }

        ////    var json = JsonConvert.SerializeObject(devices, new JsonSerializerSettings() {
        ////        TypeNameHandling = TypeNameHandling.Auto
        ////    });
        ////    Directory.CreateDirectory(JsonPath);
        ////    File.WriteAllText(JsonOpenRGBDevicesFileNameAndPath, json);
        ////}

        public void WriteGifCollectionJson()
        {
            var gifs = new List<IGifCard>();
            foreach (var gif in AvailableGifs)
            {
                gifs.Add(gif);
            }

            var json = JsonConvert.SerializeObject(gifs, new JsonSerializerSettings() {
                TypeNameHandling = TypeNameHandling.Auto
            });
            Directory.CreateDirectory(JsonPath);
            File.WriteAllText(JsonGifsCollectionFileNameAndPath, json);
        }

        public void WriteGradientCollectionJson()
        {
            var gradients = new List<IGradientColorCard>();
            foreach (var gradient in AvailableGradient)
            {
                gradients.Add(gradient);
            }

            var json = JsonConvert.SerializeObject(gradients, new JsonSerializerSettings() {
                TypeNameHandling = TypeNameHandling.Auto
            });
            Directory.CreateDirectory(JsonPath);
            File.WriteAllText(JsonGradientFileNameAndPath, json);
        }

        public void WriteGenralSettingsJson()
        {
            var json = JsonConvert.SerializeObject(GeneralSettings, new JsonSerializerSettings() {
                TypeNameHandling = TypeNameHandling.Auto
            });
            Directory.CreateDirectory(JsonPath);
            File.WriteAllText(JsonGeneralFileNameAndPath, json);
        }

        public void WritePaletteCollectionJson()
        {
            var palettes = new List<IColorPalette>();
            foreach (var palette in AvailablePallete)
            {
                palettes.Add(palette);
            }
            var json = JsonConvert.SerializeObject(palettes, Formatting.Indented);
            Directory.CreateDirectory(JsonPath);
            File.WriteAllText(PalettesCollectionFolderPath, json);
        }

        public void WriteAutomationCollectionJson()
        {
            var automations = new List<IAutomationSettings>();
            foreach (var automation in AvailableAutomations)
            {
                automations.Add(automation);
            }
            var json = JsonConvert.SerializeObject(automations, new JsonSerializerSettings() {
                TypeNameHandling = TypeNameHandling.Auto
            });
            File.WriteAllText(JsonAutomationFileNameAndPath, json);
        }

        public void WriteSolidColorJson()
        {
            var colors = new List<Color>();
            foreach (var color in AvailableSolidColors)
            {
                colors.Add(color);
            }
            var json = JsonConvert.SerializeObject(colors, new JsonSerializerSettings() {
                TypeNameHandling = TypeNameHandling.Auto
            });
            Directory.CreateDirectory(JsonPath);
            File.WriteAllText(JsonSolidColorFileNameAndPath, json);
        }

        public void WriteDeviceProfileCollection()
        {
            var profiles = new List<IDeviceProfile>();
            foreach (var profile in AvailableProfiles)
            {
                profiles.Add(profile);
            }
            var json = JsonConvert.SerializeObject(profiles, new JsonSerializerSettings() {
                TypeNameHandling = TypeNameHandling.Auto
            });
            Directory.CreateDirectory(JsonPath);
            File.WriteAllText(JsonDeviceProfileFileNameAndPath, json);
        }

        private IAmbinoColorEffect currentImportedEffect { get; set; }

        public void ImportEffect()
        {
            OpenFileDialog Import = new OpenFileDialog();
            Import.Title = "Chọn ACE file";
            Import.CheckFileExists = true;
            Import.CheckPathExists = true;
            Import.DefaultExt = "ACE";
            Import.Filter = "Text files (*.ACE)|*.ACE";
            Import.FilterIndex = 2;

            Import.ShowDialog();

            if (!string.IsNullOrEmpty(Import.FileName) && File.Exists(Import.FileName))
            {
                try
                {
                    var json = File.ReadAllText(Import.FileName);

                    currentImportedEffect = JsonConvert.DeserializeObject<AmbinoColorEffect>(json, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });

                    //open outputdataimportselection window
                    if (currentImportedEffect.TargetType == CurrentDevice.DeviceType)
                    {
                        OpenOutputDataImportSelection();
                    }
                    else
                    {
                        HandyControl.Controls.MessageBox.Show("Hiệu ứng vừa chọn không dành cho thiết bị này", "LEDSetup Import", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception)
                {
                    HandyControl.Controls.MessageBox.Show("Corrupted effect data File!!!");
                }
            }
        }

        //public void ApplyOutputImportData()
        //{
        //    int count = 0;

        //    for (var i = 0; i < currentImportedEffect.OutputLEDSetup.Length; i++)
        //    {
        //        foreach (var spot in currentImportedEffect.OutputLEDSetup[i].Spots)
        //        {
        //            var targerSpot = CurrentDevice.AvailableOutputs[i].OutputLEDSetup.Spots.Where(item => item.YIndex == spot.YIndex && item.XIndex == spot.XIndex).FirstOrDefault();
        //            if (targerSpot == null)
        //            {
        //                count++;
        //            }
        //            else
        //            {
        //                if (CurrentDevice.AvailableOutputs[i].OutputIsSelected)
        //                {
        //                    targerSpot.SetVID(spot.VID);
        //                    targerSpot.IsEnabled = spot.IsEnabled;
        //                }
        //            }
        //        }
        //    }

        //    if (count > 0)
        //    {
        //        HandyControl.Controls.MessageBox.Show("Một số LED không thể lấy vị trí do hình dạng LED khác nhau giữa file và thiết bị", "LEDSetup Import", MessageBoxButton.OK, MessageBoxImage.Warning);
        //    }
        //    else
        //    {
        //        HandyControl.Controls.MessageBox.Show("Import thành công", "Effect Import", MessageBoxButton.OK, MessageBoxImage.Information);
        //    }
        //}

        public void ImportPaletteCardFromFile()
        {
            OpenFileDialog Import = new OpenFileDialog();
            Import.Title = "Chọn col file";
            Import.CheckFileExists = true;
            Import.CheckPathExists = true;
            Import.DefaultExt = "col";
            Import.Filter = "Text files (*.col)|*.col|All files (*.*)|*.*";
            Import.FilterIndex = 2;

            Import.ShowDialog();

            if (!string.IsNullOrEmpty(Import.FileName) && File.Exists(Import.FileName))
            {
                var json = File.ReadAllText(Import.FileName);

                try
                {
                    var importedPaletteCard = JsonConvert.DeserializeObject<ColorPalette>(json, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
                    var lines = File.ReadAllLines(Import.FileName);


                    AvailablePallete.Add(importedPaletteCard);
                    RaisePropertyChanged(nameof(AvailablePallete));
                    //WritePaletteCollectionJson();
                }


                catch (Exception ex)
                {
                    HandyControl.Controls.MessageBox.Show("Corrupted Color Palette data File!!!");
                }
            }
        }
        private bool _isRenderingVideo;
        public bool IsRenderingVideo {
            get { return _isRenderingVideo; }
            set
            {
                _isRenderingVideo = value;
                RaisePropertyChanged();
            }
        }
        public void ImportGif()
        {
            OpenFileDialog Import = new OpenFileDialog();
            Import.Title = "Chọn gif file";
            Import.CheckFileExists = true;
            Import.CheckPathExists = true;
            Import.DefaultExt = "gif";
            Import.Multiselect = false;
            Import.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.tif;...|All files (*.*)|*.*";
            Import.FilterIndex = 2;

            Import.ShowDialog();

            if (!string.IsNullOrEmpty(Import.FileName) && File.Exists(Import.FileName))
            {

                //rendering new video or gif in background thread, turn the is rendering flag on
                //but what do we do when the resolution changed, because all the devices are now scale based size
                //simple answer, we make gif frame scale based too,or just using scale of device respect to gif border
                //but first scale to 100*100 frame size
                IsRenderingVideo = true;
                var result = LoadFrameData(Import.FileName, out ByteFrame[] renderedFrame);
                if (!result)
                {
                    //show error message
                }
                else
                {
                    //save byte frame to disk,get the path and put in to new gifcard model
                    var renderedFrames = JsonConvert.SerializeObject(renderedFrame, new JsonSerializerSettings() {
                        TypeNameHandling = TypeNameHandling.Auto
                    });
                    var source = Import.FileNames;
                    var name = System.IO.Path.GetFileNameWithoutExtension(source.FirstOrDefault());
                    Directory.CreateDirectory(JsonPath);
                    var path = Path.Combine(JsonPath, name + "rendered");
                    File.WriteAllText(path, renderedFrames);
                    var owner = "User";
                    var description = "User Import";
                    var importedGif = new GifCard { Name = name, Source = path, Owner = owner, Description = description };
                    AvailableGifs.Add(importedGif);
                    WriteGifCollectionJson();
                    Growl.Success("Animation imported successfully!");
                }

            }
        }
        public bool LoadFrameData(string path, out ByteFrame[] renderedFrames)
        {
            renderedFrames = null;
            var fileType = Path.GetExtension(path);
            switch (fileType)
            {
                case ".gif":
                    renderedFrames = LoadGifFromDisk(path);
                    break;
                case ".avi":
                    renderedFrames = LoadVideoFileFromDisk(path);
                    break;
            }
            return renderedFrames != null;
        }
        public ByteFrame[] LoadVideoFileFromDisk(string path)
        {
            try
            {
                VideoCapture capture = new VideoCapture(path);
                Mat m = new Mat();
                var totalFrame = (int)capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameCount);
                var renderedFrames = new ByteFrame[totalFrame];
                for (int i = 0; i < totalFrame; i++)
                {
                    //get frame from position
                    capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.PosFrames, i);
                    capture.Read(m);
                    var currentFrame = m.ToBitmap();
                    var resizedBmp = new Bitmap(currentFrame, 100, 100);
                    var rect = new Rectangle(0, 0, resizedBmp.Width, resizedBmp.Height);
                    BitmapData bmpData = resizedBmp.LockBits(rect, ImageLockMode.ReadWrite, resizedBmp.PixelFormat);

                    // Get the address of the first line.
                    IntPtr ptr = bmpData.Scan0;
                    // Declare an array to hold the bytes of the bitmap.
                    int bytes = Math.Abs(bmpData.Stride) * resizedBmp.Height;
                    byte[] rgbValues = new byte[bytes];

                    // Copy the RGB values into the array.
                    System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);
                    var frame = new ByteFrame();
                    frame.Frame = rgbValues;
                    frame.FrameWidth = resizedBmp.Width;
                    frame.FrameHeight = resizedBmp.Height;


                    renderedFrames[i] = frame;
                    resizedBmp.UnlockBits(bmpData);
                }
                capture.Dispose();
                GC.Collect();
                return renderedFrames;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public ByteFrame[] LoadGifFromDisk(string path)
        {
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    using (System.Drawing.Image imageToLoad = System.Drawing.Image.FromStream(fs))
                    {
                        var frameDim = new FrameDimension(imageToLoad.FrameDimensionsList[0]);
                        var frameCount = imageToLoad.GetFrameCount(frameDim);
                        var renderedFrames = new ByteFrame[frameCount];
                        for (int i = 0; i < frameCount; i++)
                        {
                            imageToLoad.SelectActiveFrame(frameDim, i);

                            var resizedBmp = new Bitmap(imageToLoad, 100, 100);

                            var rect = new System.Drawing.Rectangle(0, 0, resizedBmp.Width, resizedBmp.Height);
                            System.Drawing.Imaging.BitmapData bmpData =
                                resizedBmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                                resizedBmp.PixelFormat);

                            // Get the address of the first line.
                            IntPtr ptr = bmpData.Scan0;

                            // Declare an array to hold the bytes of the bitmap.
                            int bytes = Math.Abs(bmpData.Stride) * resizedBmp.Height;
                            byte[] rgbValues = new byte[bytes];

                            // Copy the RGB values into the array.
                            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);
                            var frame = new ByteFrame();
                            frame.Frame = rgbValues;
                            frame.FrameWidth = resizedBmp.Width;
                            frame.FrameHeight = resizedBmp.Height;


                            renderedFrames[i] = frame;
                            resizedBmp.UnlockBits(bmpData);

                        }
                        imageToLoad.Dispose();
                        fs.Close();
                        GC.Collect();

                        return renderedFrames;
                    }

                }

            }
            catch (Exception)
            {
                return null;
            }
        }
        public void IncreaseVID(IDeviceSpot spot)
        {
            spot.VID = 5;
        }

        //VIDs function
        public void SetZerotoAll(int number)
        {
            foreach (var spot in PreviewSpots)
            {
                spot.SetVID(number);
            }
            CurrentSpotSetVIDChanged();
        }

        public void SetIncreament(int startIndex, int spacing, int startPoint, int endPoint)
        {
            int counter = startPoint;
            for (var i = startIndex; i <= startIndex + (endPoint - startPoint) * spacing; i += spacing)
            {
                PreviewSpots[counter++].SetVID(i);
            }

            CurrentSpotSetVIDChanged();
        }

        private ObservableCollection<IDeviceProfile> ProfileFilter(IDeviceSettings device)
        {
            var filteredProfiles = new ObservableCollection<IDeviceProfile>();
            foreach (var profile in AvailableProfiles)
            {
                if (profile.DeviceType == device.DeviceType)
                {
                    filteredProfiles.Add(profile);
                }
            }
            return filteredProfiles;
        }
        private string _currentSelectedOnlineItemType;
        public string CurrentSelectedOnlineItemType {
            get { return _currentSelectedOnlineItemType; }
            set { _currentSelectedOnlineItemType = value; RaisePropertyChanged(); }
        }
        private IOnlineItemModel _currentSelectedOnlineItem;
        public IOnlineItemModel CurrentSelectedOnlineItem {
            get { return _currentSelectedOnlineItem; }
            set { _currentSelectedOnlineItem = value; RaisePropertyChanged(); }
        }
        public async Task gotoItemDetails(IOnlineItemModel item)
        {
            //CurrentSelectedOnlineItemType = item.GetType().ToString();
            CurrentSelectedOnlineItem = item;
            var screenshotsPath = item.Path + "/screenshots";
            var descriptionPath = item.Path + "/description.md";
            var screenshotsListAddress = await FTPHlprs.GetAllFilesAddressInFolder(screenshotsPath);
            item.Screenshots = new List<BitmapImage>();
            if (screenshotsListAddress == null)
                return;
            foreach (var screenshotAddress in screenshotsListAddress)
            {
                var screenshot = FTPHlprs.GetThumb(screenshotAddress).Result;
                item.Screenshots.Add(screenshot);
            }
            var description = await FTPHlprs.GetStringContent(descriptionPath);
            item.MarkDownDescription = description;
            CurrentOnlineStoreView = "Details";

        }
        private string _currentView;
        public string CurrentView {
            get { return _currentView; }
            set { _currentView = value; RaisePropertyChanged(); }
        }



        public void GotoChild(IDeviceSettings selectedDevice)
        {
            CurrentDevice = selectedDevice;
            CurrentView = "details";
            GetItemsForLiveView(selectedDevice);
            SelectedLiveViewItem = CurrentDevice.CurrentLiveViewZones[0] as IDrawable;
            UpdateLiveView();
            //foreach (var output in CurrentDevice.AvailableOutputs)
            //{
            //    output.OutputUniqueID = CurrentDevice.DeviceUID.ToString() + output.OutputID.ToString();
            //}
            AvailableProfilesForCurrentDevice = new ObservableCollection<IDeviceProfile>();
            AvailableProfilesForCurrentDevice = ProfileFilter(CurrentDevice);

            //CurrentSelectedProfile = null;
            //if (CurrentDevice.ActivatedProfileUID != null)
            //{
            //    CurrentSelectedProfile = AvailableProfilesForCurrentDevice.Where(p => p.ProfileUID == CurrentDevice.ActivatedProfileUID).FirstOrDefault();
            //}
            //if (CurrentSelectedProfile == null || CurrentDevice.ActivatedProfileUID == null)
            //{   //create new profile for this device
            //    IDeviceProfile unsavedProfile = new DeviceProfile {
            //        Name = "Unsaved Profile",
            //        Geometry = "profile",
            //        Owner = "Auto Created",
            //        DeviceType = CurrentDevice.DeviceType,
            //        ProfileUID = Guid.NewGuid().ToString()
            //    };
            //    //get output data
            //    unsavedProfile.SaveProfile(CurrentDevice.AvailableOutputs);
            //    AvailableProfiles.Add(unsavedProfile);
            //    WriteDeviceProfileCollection();
            //    Task.Run(() =>
            //    {
            //        CurrentDevice.ActivateProfile(unsavedProfile);
            //    });

            //    CurrentSelectedProfile = unsavedProfile;
            //}

            //if (CurrentDevice.AvailableOutputs.Length > 1)
            //{
            //    OutputModeChangeEnable = true;
            //}
            //else
            //{
            //    OutputModeChangeEnable = false;
            //}
            //CurrentDevice.SelectedOutput = 0;
            //CurrentOutput = CurrentDevice.AvailableOutputs[CurrentDevice.SelectedOutput];
            ////CurrentSelectedGradient = CurrentOutput.OutputSelectedGradient;
            //CurrentLEDSetup = CurrentOutput.OutputLEDSetup;
            RaisePropertyChanged(nameof(CurrentDevice.SelectedOutput));
            IsSplitLightingWindowOpen = true;


            //WriteDeviceInfoJson();

            CurrentDevice.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(CurrentDevice.SelectedOutput):
                        if (CurrentDevice.SelectedOutput >= 0)
                        {
                            //CurrentOutput = CurrentDevice.AvailableOutputs[CurrentDevice.SelectedOutput];
                            //RaisePropertyChanged(nameof(CurrentOutput));
                        }
                        else
                        {
                            //CurrentOutput = CurrentDevice.AvailableOutputs[0];
                        }

                        break;
                    case nameof(CurrentDevice.IsTransferActive):
                        {
                            CurrentDevice.IsLoadingSpeed = false;
                        }
                        break;
                    case nameof(CurrentDevice.ActivatedProfileUID):
                        RaisePropertyChanged(() => CurrentSelectedProfile);
                        break;
                    case nameof(CurrentDevice.CurrentActiveController):
                        SelectedLiveViewItem = CurrentDevice.CurrentLiveViewZones[0] as IDrawable;
                        UpdateLiveView();
                        break;
                }
            };


        }

        public void BackToDashboard()
        {
            SaveCurrentProfile(CurrentDevice.ActivatedProfileUID);
            CurrentView = "dashboard";
        }



        /// <summary>
        /// Load vertical menu
        /// </summary>

        public void LoadContextMenu()
        {
            AvailableContextMenus = new ObservableCollection<SystemTrayContextMenu>();
            //exit button
            var exit = new SystemTrayContextMenu() {
                Header = "Exit",
                Geometry = "exit",
                Action = ExitCurrentRunningAppCommand

            };
            var dashboard = new SystemTrayContextMenu() {
                Header = "Dashboard",
                Geometry = "dashboard",
                Action = ExitCurrentRunningAppCommand,
                SubMenus = new ObservableCollection<SystemTrayContextMenu>() {
                    exit,exit
                }

            };
            AvailableContextMenus.Add(exit);
            AvailableContextMenus.Add(dashboard);
        }

    }
}