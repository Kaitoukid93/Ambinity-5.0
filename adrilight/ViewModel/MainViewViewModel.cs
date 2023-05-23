using adrilight.DesktopDuplication;
using adrilight.Settings;
using adrilight.Spots;
using adrilight.Util;
using adrilight.View;
using adrilight_effect_analyzer.Model;
using Emgu.CV;
using FTPServer;
using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Themes;
using MoreLinq;
using Newtonsoft.Json;
using Renci.SshNet;
using Squirrel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TimeLineTool;
using Bitmap = System.Drawing.Bitmap;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using ColorPalette = adrilight.Util.ColorPalette;
using NotifyIcon = HandyControl.Controls.NotifyIcon;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using Point = System.Windows.Point;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;
using SplashScreen = adrilight.View.SplashScreen;
using adrilight.Helpers;
using IComputer = adrilight.Util.IComputer;
using Group = adrilight.Settings.Group;
using MoreLinq.Extensions;
using SelectionMode = System.Windows.Controls.SelectionMode;
using Border = adrilight.Settings.Border;
using Un4seen.BassWasapi;
using adrilight.Util.ModeParameters;
using adrilight.Settings.Automation;
using System.Xml;
using System.Windows.Markup;
using Formatting = Newtonsoft.Json.Formatting;

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



        #region database local folder paths
        private string PalettesCollectionFolderPath => Path.Combine(JsonPath, "ColorPalettes");
        private string AnimationsCollectionFolderPath => Path.Combine(JsonPath, "Animations");
        private string ChasingPatternsCollectionFolderPath => Path.Combine(JsonPath, "ChasingPatterns");
        private string AutomationsCollectionFolderPath => Path.Combine(JsonPath, "Automations");
        private string DevicesCollectionFolderPath => Path.Combine(JsonPath, "Devices");
        private string SupportedDeviceCollectionFolderPath => Path.Combine(JsonPath, "SupportedDevices");
        private string ColorsCollectionFolderPath => Path.Combine(JsonPath, "Colors");
        private string VIDCollectionFolderPath => Path.Combine(JsonPath, "VID");
        private string MIDCollectionFolderPath => Path.Combine(JsonPath, "MID");
        private string ResourceFolderPath => Path.Combine(JsonPath, "Resource");
        private string ProfileCollectionFolderPath => Path.Combine(JsonPath, "Profiles");

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



        private List<ActionParameter> _availableParametersforCurrentAction;

        public List<ActionParameter> AvailableParametersforCurrentAction {
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

        private List<ActionParameter> _allAvailableParametersforCurrentAction;

        public List<ActionParameter> AllAvailableParametersforCurrentAction {
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

        private ActionSettings _currentSelectedAction;

        public ActionSettings CurrentSelectedAction {
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

        private DeviceFirmware _currentSelectedFirmware;

        public DeviceFirmware CurrentSelectedFirmware {
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

        private ActionParameter _selectedParameter;

        public ActionParameter SelectedParameter {
            get { return _selectedParameter; }
            set { _selectedParameter = value; RaisePropertyChanged(); }
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
        public ICommand ExportItemForOnlineStoreCommand { get; set; }
        public ICommand GroupSelectedZoneForMaskedControlCommand { get; set; }
        public ICommand UnselectAllLiveiewItemCommand { get; set; }
        public ICommand LiveViewMouseButtonUpCommand { get; set; }
        public ICommand UnselectAllSurfaceEditorItemCommand { get; set; }
        public ICommand IsolateSelectedItemsCommand { get; set; }
        public ICommand OpenSpotPIDQuickEDitWindowCommand { get; set; }
        public ICommand RotateSelectedSurfaceEditorItemCommand { get; set; }
        public ICommand ReflectSelectedSurfaceEditorItemCommand { get; set; }
        public ICommand SaveCurrentPIDCommand { get; set; }
        public ICommand CancelCurrentPIDCommand { get; set; }
        public ICommand ExitIsolateModeCommand { get; set; }
        public ICommand UnGroupZoneCommand { get; set; }
        public ICommand ExportCurrentOnlineItemToFilesCommand { get; set; }
        public ICommand OpenImageSelectorCommand { get; set; }
        public ICommand DownloadCurrentItemCommand { get; set; }
        public ICommand CompositionNextFrameCommand { get; set; }
        public ICommand SelectSlaveDeviceForCurrentOutputCommand { get; set; }
        public ICommand OpenSlaveDeviceSelectorCommand { get; set; }
        public ICommand SelectPIDCanvasItemCommand { get; set; }
        public ICommand SelectLiveViewItemCommand { get; set; }
        public ICommand SelectSurfaceEditorItemCommand { get; set; }
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
        public ICommand AddSpotGeometryCommand { get; set; }
        public ICommand AddSpotLayoutCommand { get; set; }
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
        public ICommand CloseIDSetupCommand { get; set; }
        public ICommand ResetAllItemsIDCommand { get; set; }
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
        public ICommand ActivateSelectedProfileCommmand { get; set; }
        public ICommand DeleteAttachedProfileCommand { get; set; }
        public ICommand SaveCurrentSelectedActionCommand { get; set; }
        public ICommand CreateNewProfileCommand { get; set; }
        public ICommand OpenProfileCreateCommand { get; set; }
        public ICommand ExportPIDCommand { get; set; }
        public ICommand ImportLEDSetupCommand { get; set; }
        public ICommand UnZoneItemsCommand { get; set; }
        public ICommand RenameZoneCommand { get; set; }
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
        public ICommand SetZoneMIDCommand { get; set; }
        public ICommand ResetSpotIDCommand { get; set; }
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
        public ICommand EditSelectedPaletteCommand { get; set; }
        public ICommand AddNewItemToCollectionCommand { get; set; }
        public ICommand AddPickedSolidColorCommand { get; set; }
        public ICommand OpenExternalWindowsFromModeParameterCommand { get; set; }
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
        public ICommand BackToCollectionViewCommand { get; set; }
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

        private ObservableCollection<AppProfile> _availableProfiles;

        public ObservableCollection<AppProfile> AvailableProfiles {
            get { return _availableProfiles; }
            set
            {
                if (_availableProfiles == value) return;
                _availableProfiles = value;

                RaisePropertyChanged();
            }
        }

        private ObservableCollection<DeviceFirmware> _availableFirmwareForCurrentDevice;

        public ObservableCollection<DeviceFirmware> AvailableFirmwareForCurrentDevice {
            get { return _availableFirmwareForCurrentDevice; }
            set
            {
                if (_availableFirmwareForCurrentDevice == value) return;
                _availableFirmwareForCurrentDevice = value;

                RaisePropertyChanged();
            }
        }

        private ObservableCollection<AutomationSettings> _availableAutomations;

        public ObservableCollection<AutomationSettings> AvailableAutomations {
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

        private ObservableCollection<DeviceProfile> _availableProfilesForCurrentDevice;

        public ObservableCollection<DeviceProfile> AvailableProfilesForCurrentDevice {
            get { return _availableProfilesForCurrentDevice; }
            set
            {
                _availableProfilesForCurrentDevice = value;
                RaisePropertyChanged();
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

        private WriteableBitmap _shaderBitmap;

        public WriteableBitmap ShaderBitmap {
            get => _shaderBitmap;
            set
            {
                _shaderBitmap = value;
                RaisePropertyChanged(nameof(ShaderBitmap));


            }
        }
        private VisualizerDataModel _audioVisualizers;
        public VisualizerDataModel AudioVisualizers {
            get { return _audioVisualizers; }
            set { _audioVisualizers = value; RaisePropertyChanged(); }
        }
        private ObservableCollection<WriteableBitmap> _desktops;
        public ObservableCollection<WriteableBitmap> Desktops {
            get { return _desktops; }
            set { _desktops = value; RaisePropertyChanged(); }
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



        //public IList<String> AvailableAudioDevice {
        //    get
        //    {

        //        var audioDevices = AudioFrame.AvailableAudioDevice;

        //        return audioDevices;

        //    }
        //}


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


        public ResourceHelpers ResourceHlprs { get; private set; }
        public LocalFileHelpers LocalFileHlprs { get; private set; }
        public IGeneralSettings GeneralSettings { get; }

        public ISerialStream[] SerialStreams { get; }
        //public IAmbinityClient AmbinityClient { get; set; }
        private static ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();
        public object this[string propertyName] {
            get
            {
                // probably faster without reflection:
                // like:  return Properties.Settings.Default.PropertyValues[propertyName] 
                // instead of the following
                Type myType = typeof(MainViewViewModel);
                PropertyInfo myPropInfo = myType.GetProperty(propertyName);
                return myPropInfo.GetValue(this, null);
            }
            set
            {
                Type myType = typeof(MainViewViewModel);
                PropertyInfo myPropInfo = myType.GetProperty(propertyName);
                myPropInfo.SetValue(this, value, null);
            }
        }
        public MainViewViewModel(IGeneralSettings generalSettings, IList<ISelectableViewPart> selectableViewParts
            )
        {
            #region load Params
            GeneralSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));
            SelectableViewParts = selectableViewParts.OrderBy(p => p.Order)
                .ToList();
            SelectedViewPart = SelectableViewParts.First();
            DisplayCards = new ObservableCollection<IDeviceSettings>();
            //AmbinityClient = ambinityClient ?? throw new ArgumentNullException(nameof(ambinityClient));
            #endregion


            #region Registering devices
            AvailableDevices = new ObservableCollection<IDeviceSettings>();
            AvailableDevices.CollectionChanged += (s, e) =>
            {
                switch (e.Action)
                {
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                        var newDevices = e.NewItems;
                        // create new IDeviceSettings with new Name
                        //Get ID:
                        foreach (IDeviceSettings device in newDevices)
                        {
                            RegisterDevice(device);
                        }
                        break;
                }
            };


            #endregion

            #region registering General settings
            RegisterGeneralSettings(GeneralSettings);
            #endregion


            #region load Automation
            AvailableAutomations = new ObservableCollection<AutomationSettings>();
            foreach (var automation in LoadAutomationIfExist())
            {
                AvailableAutomations.Add(automation);
            }
            WriteAutomationCollectionJson();
            #endregion


            #region Create Resource and Collections Folder
            CreateFWToolsFolderAndFiles();
            #endregion



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
            var thumbnailResourcePath = "adrilight.Resources.Thumbnails." + device.DeviceType.Type.ToString() + "_thumb.png";
            var outputmapResourcePath = "adrilight.Resources.OutputMaps." + device.DeviceType.Type.ToString() + "_outputmap.png";
            ResourceHlprs.CopyResource(thumbnailResourcePath, Path.Combine(directory, "thumbnail.png"));
            ResourceHlprs.CopyResource(outputmapResourcePath, Path.Combine(directory, "outputmap.png"));
            //write output info
            var lightingOutputDirectory = Path.Combine(directory, "LightingOutputs"); //contains lighting controller info
            foreach (var output in device.AvailableLightingOutputs)
            {
                var outputDirectory = Path.Combine(lightingOutputDirectory, output.OutputName + "_" + output.OutputID.ToString());
                Directory.CreateDirectory(outputDirectory);
                //write output infojson
                WriteSingleOutputInfoJson(output, device);

                //write slave device config
                var slaveDeviceDirectory = Path.Combine(outputDirectory, "AttachedDevice");
                Directory.CreateDirectory(slaveDeviceDirectory);
                //write slave device thumb as undefined device
                if (device.IsSizeNeedUserDefine)
                {
                    var undefinedThumbnailResourcePath = "adrilight.Resources.Thumbnails.Undefined_thumb.png";
                    ResourceHlprs.CopyResource(undefinedThumbnailResourcePath, Path.Combine(slaveDeviceDirectory, "thumbnail.png"));
                    //output.SlaveDevice.Thumbnail = Path.Combine(slaveDeviceDirectory, "thumbnail.png");
                }

                WriteSingleSlaveDeviceInfoJson(output.SlaveDevice, output, device);
            }
            var pwmController = device.AvailableControllers.Where(c => c.Type == ControllerTypeEnum.PWMController).FirstOrDefault();
            if (pwmController != null)
            {
                var pwmOutputDirectory = Path.Combine(directory, "PWMOutputs"); //contains pwm controller info
                foreach (var output in device.AvailablePWMOutputs)
                {
                    var outputDirectory = Path.Combine(pwmOutputDirectory, output.OutputName + "_" + output.OutputID.ToString());
                    Directory.CreateDirectory(outputDirectory);
                    //write output infojson
                    WriteSingleOutputInfoJson(output, device);

                    //write slave device config
                    var slaveDeviceDirectory = Path.Combine(outputDirectory, "AttachedDevice");
                    Directory.CreateDirectory(slaveDeviceDirectory);
                    //write slave device thumb as undefined device
                    if (device.IsSizeNeedUserDefine)
                    {
                        var undefinedThumbnailResourcePath = "adrilight.Resources.Thumbnails.Undefined_thumb.png";
                        ResourceHlprs.CopyResource(undefinedThumbnailResourcePath, Path.Combine(slaveDeviceDirectory, "thumbnail.png"));
                        //output.SlaveDevice.Thumbnail = Path.Combine(slaveDeviceDirectory, "thumbnail.png");
                    }

                    WriteSingleSlaveDeviceInfoJson(output.SlaveDevice, output, device);
                }
            }

            // finally write infojson
            WriteSingleDeviceInfoJson(device);




        }
        #region component register
        /// <summary>
        /// this region contains all function need to call when a new component is added
        /// </summary>
        /// 
        private void RegisterGeneralSettings(IGeneralSettings generalSettings)

        {
            generalSettings.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(generalSettings.ThemeIndex):
                        if (generalSettings.ThemeIndex == 0)
                            ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                        else
                        {
                            ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                        }
                        break;

                    case nameof(generalSettings.Autostart):
                        if (generalSettings.Autostart)
                        {
                            StartUpManager.AddApplicationToTaskScheduler(generalSettings.StartupDelaySecond);
                        }
                        else
                        {
                            StartUpManager.RemoveApplicationFromTaskScheduler("Ambinity Service");
                        }
                        break;
                    case nameof(generalSettings.StartupDelaySecond):
                        if (generalSettings.Autostart)
                        {
                            StartUpManager.AddApplicationToTaskScheduler(generalSettings.StartupDelaySecond);
                        }
                        else
                        {
                            StartUpManager.RemoveApplicationFromTaskScheduler("Ambinity Service");
                        }
                        break;

                    case nameof(generalSettings.HotkeyEnable):
                        if (generalSettings.HotkeyEnable)
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
        }
        private void RegisterDevice(IDeviceSettings device)
        {
            device.PropertyChanged += (_, __) =>
            {
                WriteSingleDeviceInfoJson(device);
                switch (__.PropertyName)
                {
                    case nameof(device.CurrentActiveControlerIndex):
                        {
                            //update liveview
                            GetItemsForLiveView();
                            UpdateLiveView();
                        }
                        break;
                }
            };

            foreach (var controller in device.AvailableControllers)
            {
                foreach (var output in controller.Outputs)
                {
                    RegisterOutput(output, device);
                }

            }
            if (device.ControlZoneGroups != null)
            {

                foreach (var group in device.ControlZoneGroups)
                {

                    var childItems = GetGroupChildItems(group, device);
                    switch (group.Type)
                    {
                        case ControllerTypeEnum.LightingController:
                            RegisterGroupItem(childItems.Cast<LEDSetup>().ToList(), group);
                            break;
                        case ControllerTypeEnum.PWMController:
                            RegisterGroupItem(childItems.Cast<FanMotor>().ToList(), group);
                            break;
                    }

                }

            }
        }
        private void RegisterOutput(IOutputSettings output, IDeviceSettings device)
        {


            output.PropertyChanged += (_, __) =>
            {
                WriteSingleOutputInfoJson(output, device);

                switch (__.PropertyName)
                {
                    case nameof(output.SlaveDevice): // in case slave device changed to completely new one
                        WriteSingleSlaveDeviceInfoJson(output.SlaveDevice, output, device);
                        RegisterSlaveDevice(output.SlaveDevice, output, device);
                        break;

                }
            };
            RegisterSlaveDevice(output.SlaveDevice, output, device);

        }
        private void RegisterSlaveDevice(ISlaveDevice slaveDevice, IOutputSettings output, IDeviceSettings device)
        {
            slaveDevice.PropertyChanged += (_, __) =>
            {
                WriteSingleSlaveDeviceInfoJson(output.SlaveDevice, output, device);
                //this master change in position reflect other output type of the device
                if (output.OutputType == OutputTypeEnum.ARGBLEDOutput)
                {
                    var ledSlaveDevice = output.SlaveDevice as IDrawable;
                    if (device.AvailablePWMOutputs.Count() > 0)
                    {
                        var pwmSlaveDevice = device.AvailablePWMOutputs.Where(o => o.OutputID == output.OutputID).FirstOrDefault().SlaveDevice;
                        switch (__.PropertyName)
                        {
                            case nameof(ledSlaveDevice.Left):
                            case nameof(ledSlaveDevice.Top):
                            case nameof(ledSlaveDevice.Width):
                            case nameof(ledSlaveDevice.Height):
                                if (pwmSlaveDevice != null)
                                {
                                    (pwmSlaveDevice as IDrawable).Left = ledSlaveDevice.Left;
                                    (pwmSlaveDevice as IDrawable).Top = ledSlaveDevice.Top;
                                    (pwmSlaveDevice as IDrawable).Width = ledSlaveDevice.Width;
                                    (pwmSlaveDevice as IDrawable).Height = ledSlaveDevice.Height;
                                }
                                break;
                        }
                    }

                }




            };
            foreach (var zone in output.SlaveDevice.ControlableZones)
            {
                RegisterZone(zone, slaveDevice, output, device);
            }
        }
        private void RegisterZone(IControlZone zone, ISlaveDevice slaveDevice, IOutputSettings output, IDeviceSettings device)
        {
            zone.PropertyChanged += (_, __) => WriteSingleSlaveDeviceInfoJson(output.SlaveDevice, output, device);
            foreach (var mode in zone.AvailableControlMode)
            {
                mode.PropertyChanged += (_, __) => WriteSingleSlaveDeviceInfoJson(output.SlaveDevice, output, device);
                foreach (var param in mode.Parameters)
                {
                    param.PropertyChanged += (_, __) => WriteSingleSlaveDeviceInfoJson(output.SlaveDevice, output, device);
                    if (param.SubParams != null)
                    {
                        foreach (var subParam in param.SubParams)
                        {
                            subParam.PropertyChanged += (_, __) => WriteSingleSlaveDeviceInfoJson(output.SlaveDevice, output, device);
                        }
                    }

                }
            }
        }

        #endregion

        #region writing database
        public void WriteSettingJson(IGeneralSettings generalSettings)
        {
            var json = JsonConvert.SerializeObject(generalSettings, Formatting.Indented);
            Directory.CreateDirectory(JsonPath);
            File.WriteAllText(JsonGeneralFileNameAndPath, json);
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
        public void WriteSingleSlaveDeviceInfoJson(ISlaveDevice slaveDevice, IOutputSettings output, IDeviceSettings parrent)
        {
            var parrentDirectory = Path.Combine(DevicesCollectionFolderPath, parrent.DeviceName + "-" + parrent.DeviceUID);
            string outputDirectory = " ";
            switch (output.OutputType)
            {
                case (OutputTypeEnum.PWMOutput):
                    outputDirectory = Path.Combine(parrentDirectory, "PWmOutputs", output.OutputName + "_" + output.OutputID.ToString());
                    break;
                case (OutputTypeEnum.ARGBLEDOutput):
                    outputDirectory = Path.Combine(parrentDirectory, "LightingOutputs", output.OutputName + "_" + output.OutputID.ToString());
                    break;
            }

            var fileToWrite = Path.Combine(outputDirectory, "AttachedDevice", "config.json");
            var json = JsonConvert.SerializeObject(slaveDevice, new JsonSerializerSettings() {
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
            //change thumb path
        }
        public void WriteSingleOutputInfoJson(IOutputSettings output, IDeviceSettings parrent)
        {
            var parrentDirectory = Path.Combine(DevicesCollectionFolderPath, parrent.DeviceName + "-" + parrent.DeviceUID);
            string childDirectory = " ";
            switch (output.OutputType)
            {
                case (OutputTypeEnum.PWMOutput):
                    childDirectory = Path.Combine(parrentDirectory, "PWmOutputs", output.OutputName + "_" + output.OutputID.ToString());
                    break;
                case (OutputTypeEnum.ARGBLEDOutput):
                    childDirectory = Path.Combine(parrentDirectory, "LightingOutputs", output.OutputName + "_" + output.OutputID.ToString());
                    break;
            }
            var fileToWrite = Path.Combine(childDirectory, "config.json");
            var json = JsonConvert.SerializeObject(output, new JsonSerializerSettings() {
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
        #endregion
        private bool _searchingForDevices;
        public bool SearchingForDevices {
            get { return _searchingForDevices; }
            set
            {
                _searchingForDevices = value;
                RaisePropertyChanged();
            }
        }
        public void SetDashboardStatusText(string text, bool isLoading)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {
                DashboardLoadingText = text;
                SearchingForDevices = isLoading;
            });
        }
        private string _dashboardLoadingText = "Searching for supported device...";
        public string DashboardLoadingText {
            get { return _dashboardLoadingText; }
            set
            {
                _dashboardLoadingText = value;
                RaisePropertyChanged();
            }
        }
        public async Task FoundNewDevice(List<IDeviceSettings> newDevices)
        {
            if (_isloadingProfile)
                return;
            await System.Windows.Application.Current.Dispatcher.BeginInvoke(async () =>
            {

                if (newDevices != null && newDevices.Count > 0)
                {
                    foreach (var device in newDevices)
                    {
                        device.IsTransferActive = true;
                        if (device.DeviceType.Type == DeviceTypeEnum.AmbinoBasic)
                        {
                            GeneralSettings.IsOpenRGBEnabled = true;
                        }
                        WriteDeviceInfo(device);
                        var thumbPath = await FTPHlprs.GetFileByName(device.DeviceName + ".png", thumbResourceFolderPath);
                        if (thumbPath != null)
                        {
                            FTPHlprs.DownloadFile(thumbPath, Path.Combine(ResourceFolderPath, device.DeviceName + "_thumb.png"));
                        }
                        lock (AvailableDeviceLock)
                        {
                            AvailableDevices.Insert(0, device);

                        }
                    }
                    SearchingForDevices = false;
                }

            });

        }
        public void OldDeviceReconnected(List<string> oldDevices)
        {
            if (_isloadingProfile)
                return;
            System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {

                if (oldDevices != null && oldDevices.Count > 0)
                {


                    foreach (var port in oldDevices)
                    {
                        //set first device found active again since it's recconected

                        var oldDevice = AvailableDevices.Where(p => p.OutputPort == port).FirstOrDefault();
                        if (oldDevice.IsTransferActive == false)
                        {
                            //try to poke it
                            oldDevice.IsTransferActive = true;
                            if (oldDevice.DeviceType.Type == DeviceTypeEnum.AmbinoBasic)
                            {
                                GeneralSettings.IsOpenRGBEnabled = true;
                            }
                        }

                        WriteSingleDeviceInfoJson(oldDevice);
                    }

                    SearchingForDevices = false;
                }
            });

        }
        public object AudioUpdateLock { get; } = new object();
        public object AvailableDeviceLock { get; } = new object();
        public void AudioVisualizerUpdate(ByteFrame frame)
        {
            if (AudioVisualizers == null)
            {
                AudioVisualizers = new VisualizerDataModel(32);

            }

            System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {

                foreach (var column in AudioVisualizers.Columns)
                {
                    column.SetValue(frame.Frame[column.Index]);
                }

            });
        }
        private ObservableCollection<AudioDevice> _availableAudioDevices;
        public ObservableCollection<AudioDevice> AvailableAudioDevices {
            get { return _availableAudioDevices; }
            set
            {
                _availableAudioDevices = value;
                RaisePropertyChanged();
            }
        }
        public List<AudioDevice> GetAvailableAudioDevices()
        {
            var availableDevices = new List<AudioDevice>();
            int devicecount = BassWasapi.BASS_WASAPI_GetDeviceCount();

            for (int i = 0; i < devicecount; i++)
            {

                var device = BassWasapi.BASS_WASAPI_GetDeviceInfo(i);

                if (device.IsEnabled && device.IsLoopback)
                {
                    var audioDevice = new AudioDevice() { Name = device.name, Index = i };

                    availableDevices.Add(audioDevice);
                }

            }
            return availableDevices;
        }
        public void DesktopsPreviewUpdate(ByteFrame frame, int index)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {

                if (Desktops[index] == null)
                {
                    Desktops[index] = new WriteableBitmap(frame.FrameWidth, frame.FrameHeight, 96, 96, PixelFormats.Bgra32, null);
                }
                else if (Desktops[index] != null && (Desktops[index].Width != frame.FrameWidth || Desktops[index].Height != frame.FrameHeight))
                {
                    Desktops[index] = new WriteableBitmap(frame.FrameWidth, frame.FrameHeight, 96, 96, PixelFormats.Bgra32, null);
                }
                else if (Desktops[index] != null && Desktops[index].Width == frame.FrameWidth && Desktops[index].Height == frame.FrameHeight)
                {
                    if (frame != null)
                    {
                        Desktops[index].Lock();
                        IntPtr pixelAddress = Desktops[index].BackBuffer;
                        Marshal.Copy(frame.Frame, 0, pixelAddress, frame.Frame.Length);

                        Desktops[index].AddDirtyRect(new Int32Rect(0, 0, frame.FrameWidth, frame.FrameHeight));

                        Desktops[index].Unlock();

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


        private int _count = 0;
        public int Count {
            get => _count;
            set
            {
                Set(ref _count, value);
                RaisePropertyChanged(nameof(IsNextable));
            }
        }
        private int _countMID = 0;

        public int CountMID {
            get => _countMID;
            set
            {
                Set(ref _countMID, value);

            }
        }
        private int _countVID = 0;

        public int CountVID {
            get => _countVID;
            set
            {
                Set(ref _countVID, value);

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




        private bool _isSplitLightingWindowOpen;

        public bool IsSplitLightingWindowOpen {
            get => _isSplitLightingWindowOpen;
            set
            {
                Set(ref _isSplitLightingWindowOpen, value);
                // _log.Info($"IsSettingsWindowOpen is now {_isSettingsWindowOpen}");
            }
        }



        private AutomationSettings _currentSelectedAutomation;

        public AutomationSettings CurrentSelectedAutomation {
            get => _currentSelectedAutomation;
            set
            {
                Set(ref _currentSelectedAutomation, value);
                // _log.Info($"IsSettingsWindowOpen is now {_isSettingsWindowOpen}");
            }
        }



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
        private string _newPaletteName = "New Palette" + DateTime.Now.ToString("yyyyMMdd");

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


        private bool _outputModeChangeEnable;

        public bool OutputModeChangeEnable {
            get { return _outputModeChangeEnable; }
            set
            {
                _outputModeChangeEnable = value;
                RaisePropertyChanged();
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
            LoadContextMenu();
            LoadData();
            #region Command setup







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
            CloseIDSetupCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                if (LiveViewItems.Contains(EraserBrush))
                    LiveViewItems.Remove(EraserBrush);
                if (LiveViewItems.Contains(IDEditBrush))
                    LiveViewItems.Remove(IDEditBrush);
                foreach (var item in LiveViewItems)
                {
                    item.IsSelectable = true;
                    item.IsSelected = false;
                }
                IsInIDEditStage = false;
            });
            ResetAllItemsIDCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                VIDCount = 0;
                if (Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    foreach (var item in LiveViewItems.Where(i => i is LEDSetup))
                    {
                        (item as LEDSetup).ResetVIDStage();
                    }
                }

            });
            AddNewItemToCollectionCommand = new RelayCommand<string>((p) =>
                 {
                     return true;
                 }, (p) =>
                 {
                     switch (p)
                     {
                         case "Add Color":
                             OpenColorPickerWindow(2);
                             break;
                         case "Add VID":
                             IdEditMode = IDMode.VID;
                             IsInIDEditStage = true;
                             break;
                         case "Add FID":
                             IdEditMode = IDMode.FID;
                             IsInIDEditStage = true;
                             break;
                         case "Add Palette":
                             OpenPaletteEditorWindow(16);
                             break;
                         case "Import Palette":
                             ImportPaletteFromFile();
                             break;
                     }
                 });
            OpenExternalWindowsFromModeParameterCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                switch (p)
                {
                    case "surfaceEditor":
                        OpenSurfaceEditorWindow();
                        break;
                    case "audioDevice":
                        OpenAudioSelectorWindow();
                        break;

                }
            });
            AddPickedSolidColorCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                switch (ColorList.Count)
                {
                    case 2:
                        AddNewColorToCollection();
                        break;
                    case 16:
                        OpenCreateNewPaletteDialog();
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
                       var newPalette = new ColorPalette(16);
                       newPalette.Name = NewPaletteName;
                       newPalette.Description = NewPaletteDescription;
                       newPalette.Owner = NewPaletteOwner;
                       for (int i = 0; i < 16; i++)
                       {
                           newPalette.Colors[i] = ColorList[i].Color;
                       }
                       AddNewPaletteToCollection(newPalette);
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
            ActivateSelectedProfileCommmand = new RelayCommand<AppProfile>((p) =>
            {
                return true;
            }, async (p) =>
            {
                await Task.Run(() => { System.Windows.Application.Current.Dispatcher.InvokeAsync(() => ActivateSelectedProfile(p)); });

            }
          );
            DeleteAttachedProfileCommand = new RelayCommand<AppProfile>((p) =>
            {
                return true;
            }, (p) =>
            {
                DeleteAttachedProfile(p);
            }
          );

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
            AddSpotGeometryCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {

                AddSpotGeometry();

            }
            );
            AddSpotLayoutCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {

                AddSpotLayout();

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
            OpenImageSelectorCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenImageSelector(p);
            });
            ExportCurrentOnlineItemToFilesCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                ExportCurrentOnlineItemToFiles();
            });
            ExportItemForOnlineStoreCommand = new RelayCommand<object>((p) =>
            {
                return true;
            }, (p) =>
            {
                ExportItemForOnlineStore(p);
            });

            ClearPIDCanvasCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {

                ClearPIDCanvas();

            }
            );
            DownloadCurrentItemCommand = new RelayCommand<IOnlineItemModel>((p) =>
            {
                return true;
            }, async (p) =>
            {
                await Task.Run(() => DownloadCurrentOnlineItem(p));
            });

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
                SaveCurrentLEDSetupLayout(p);
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

            DeleteSelectedPaletteCommand = new RelayCommand<ColorPalette>((p) =>
            {
                return true;
            }, (p) =>
            {
                //DeleteSelectedPalette(p);
            });
            UploadSelectedPaletteCommand = new RelayCommand<ColorPalette>((p) =>
           {
               return true;
           }, (p) =>
           {
               //UploadSelectedPalette(p);
           });

            DeleteSelectedGifCommand = new RelayCommand<IGifCard>((p) =>
            {
                return true;
            }, (p) =>
            {
                DeleteSelectedGif(p);
            });

            DeleteSelectedAutomationCommand = new RelayCommand<AutomationSettings>((p) =>
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

            ImportedGifFromFileCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                ImportGif();
            });
            ExportCurrentSelectedPaletteToFileCommand = new RelayCommand<ColorPalette>((p) =>
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
                AddNewZone(PIDEditWindowsRichCanvasItems, PIDEditWindowSelectedItems);
            });
            UnZoneItemsCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                UnZone();
            });
            RenameZoneCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                RenameZone();
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
                if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    if ((p as ARGBLEDSlaveDevice).ControlableZones.Any(z => z.IsInControlGroup))
                    {
                        HandyControl.Controls.MessageBox.Show("Không thể chỉnh sửa cấu hình LED khi thiết bị nằm trong Group. Ungroup trước khi chỉnh sửa", "Device is in group", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    LaunchPIDEditWindow(p);
                }
                else
                {
                    ExportItemForOnlineStore(p as ARGBLEDSlaveDevice);
                }
            });
            OpenSpotPIDQuickEDitWindowCommand = new RelayCommand<ObservableCollection<IDrawable>>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenSpotPIDQuickEDitWindow(p);
            });
            RotateSelectedSurfaceEditorItemCommand = new RelayCommand<ObservableCollection<IDrawable>>((p) =>
            {
                return true;
            }, (p) =>
            {
                RotateSelectedSurfaceEditorItem(p);
            });
            ReflectSelectedSurfaceEditorItemCommand = new RelayCommand<ObservableCollection<IDrawable>>((p) =>
            {
                return true;
            }, (p) =>
            {
                ReflectSelectedSurfaceEditorItem(p);
            });
            SaveCurrentPIDCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                SaveCurrentPID();
            });
            CancelCurrentPIDCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                CancelCurrentPID();
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

            UnselectAllLiveiewItemCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                if (!Keyboard.IsKeyDown(Key.LeftCtrl)) // user is draging or holding ctrl
                {
                    foreach (var item in LiveViewItems)
                    {
                        item.IsSelected = false;
                    }
                    ShowSelectedItemToolbar = false;
                    SelectedSlaveDevice = null;
                }

            });

            LiveViewMouseButtonUpCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                if (LiveViewItems.Where(p => p.IsSelected).Count() > 0)
                    ShowSelectedItemToolbar = true;
                CanUnGroup = false;
                CanGroup = false;
                if (LiveViewItems.Where(p => p.IsSelected).Count() == 1)
                {

                    if (LiveViewItems.Where(p => p.IsSelected).FirstOrDefault() is Border)
                    {
                        CanUnGroup = true;
                    }

                }

                else
                {
                    CanGroup = true;
                    CanUnGroup = false;
                }

            });

            UnselectAllSurfaceEditorItemCommand = new RelayCommand<IDrawable>((p) =>
            {
                return true;
            }, (p) =>
            {
                if (!Keyboard.IsKeyDown(Key.LeftCtrl)) // user is draging or holding ctrl
                {
                    foreach (var item in SurfaceEditorItems.Where(i => i != p))
                    {
                        item.IsSelected = false;
                    }
                }

            });
            IsolateSelectedItemsCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                IsolateSelectedItems();
                UpdateLiveView();

            });
            ExitIsolateModeCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                GetItemsForLiveView();
                UpdateLiveView();

            });
            UnGroupZoneCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {

                UngroupZone();

            });

            SelectLiveViewItemCommand = new RelayCommand<IDrawable>((p) =>
            {
                return true;
            }, (p) =>
            {
                LiveViewSelectedItemChanged(p);

            });
            SelectSurfaceEditorItemCommand = new RelayCommand<IDrawable>((p) =>
            {
                return true;
            }, (p) =>
            {
                if (p.IsSelectable)
                    p.IsSelected = true;

            });
            GroupSelectedZoneForMaskedControlCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                AddZoneToGroup();


            });
            SelectSlaveDeviceForCurrentOutputCommand = new RelayCommand<ARGBLEDSlaveDevice>((p) =>
            {
                return true;
            }, (p) =>
            {
                //change uid to prevent injection conflict
                foreach (var zone in p.ControlableZones)
                {
                    zone.ZoneUID = Guid.NewGuid().ToString();
                    if (CtrlHlprs == null)
                        CtrlHlprs = new ControlModeHelpers();
                    CtrlHlprs.MakeZoneControlable(zone);
                }
                p.ParrentID = CurrentSelectedOutputMap.OutputID;
                CurrentSelectedOutputMap.SlaveDevice = p;
                SlaveDeviceSelection.Close();

            });
            OpenSlaveDeviceSelectorCommand = new RelayCommand<IDrawable>((p) =>
            {
                return true;
            }, (p) =>
            {
                var currentOutput = p as OutputSettings;
                OpenSlaveDeviceSelector(currentOutput);

                //open slave device selection
                //browse online store for items that match this target device
                //add items to collection and display

            });
            SelectPIDCanvasItemCommand = new RelayCommand<IDrawable>((p) =>
            {
                return true;
            }, (p) =>
            {
                if (p is LEDSetup)
                {
                    var zone = p as LEDSetup;
                    foreach (var spot in zone.Spots)
                    {
                        spot.SetColor(0, 0, 255, true);
                    }
                }

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
            OpenActionsManagerWindowCommand = new RelayCommand<AutomationSettings>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenActionsManagerWindow(p);
            });
            OpenHotKeySelectionWindowCommand = new RelayCommand<AutomationSettings>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenHotKeySelectionWindow(p);
            });
            OpenTargetDeviceSelectionWindowCommand = new RelayCommand<ActionSettings>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenTargetDeviceSelectionWindow(p);
            });
            OpenTargetActionSelectionWindowCommand = new RelayCommand<ActionSettings>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenTargetActionSelectionWindow(p);
            });
            OpenAutomationValuePickerWindowCommand = new RelayCommand<ActionSettings>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenAutomationValuePickerWindow(p);
            });
            OpenTargetParamSelectionWindowCommand = new RelayCommand<ActionSettings>((p) =>
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
            ExecuteAutomationFromManagerCommand = new RelayCommand<ObservableCollection<ActionSettings>>((p) =>
            {
                return true;
            }, (p) =>
            {
                ExecuteAutomationActions(p);
            });
            DeleteSelectedActionFromListCommand = new RelayCommand<ActionSettings>((p) =>
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
            SetCurrentActionParamForSelectedActionCommand = new RelayCommand<ActionParameter>((p) =>
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
                OOTBItems.Clear();
                OOTBSelectedItems.Clear();
                if (OOTBWindows.askAgain.IsChecked == true)
                    CurrentDevice.IsSizeNeedUserDefine = false;
                GotoChild(CurrentDevice);
                OOTBWindows.Close();

            });
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
                if (NameChangingSelectedItem != null)
                {
                    if (NameChangingSelectedItem is IDrawable)
                    {
                        (NameChangingSelectedItem as IDrawable).Name = NameToChange;
                    }
                }
            });
            ShowNameEditWindow = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                //OpenNameEditWindow();
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
            SetZoneMIDCommand = new RelayCommand<int>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                SetZoneMID(p);

            });
            ResetSpotIDCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {

                ResetPID();

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
            //SetSelectedSpotIDLeftToRightCommand = new RelayCommand<string>((p) =>
            //{
            //    return p != null;
            //}, (p) =>
            //{
            //    // if (p != "")
            //    // ProcessSelectedSpotsID(p);
            //});

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
            BackToCollectionViewCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                BackToCollectionView();
            });
            #endregion
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

        private void OpenActionsManagerWindow(AutomationSettings selectedautomation)
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"ActionManagerWindow"}") is System.Windows.Window window)
            {
                CurrentSelectedAutomation = selectedautomation;
                AvailableActionsforCurrentDevice = new ObservableCollection<ActionType>();
                AvailableActionsforCurrentDevice.Add(new ActionType { Name = "Kích hoạt", Description = "Kích hoạt một Profile có sẵn", Geometry = "apply", Type = "Activate", LinkText = "", IsValueDisplayed = false, IsTargetDeviceDisplayed = false });
                AvailableActionsforCurrentDevice.Add(new ActionType { Name = "Tăng", Description = "Tăng giá trị của một thuộc tính", Geometry = "apply", Type = "Increase", LinkText = "Của thiết bị", IsValueDisplayed = false, IsTargetDeviceDisplayed = true });
                AvailableActionsforCurrentDevice.Add(new ActionType { Name = "Giảm", Description = "Giảm giá trị của một thuộc tính", Geometry = "apply", Type = "Decrease", LinkText = "Của thiết bị", IsValueDisplayed = false, IsTargetDeviceDisplayed = true });
                AvailableActionsforCurrentDevice.Add(new ActionType { Name = "Bật", Description = "Bật một tính năng", Geometry = "apply", Type = "On", LinkText = "Của thiết bị", IsValueDisplayed = false, IsTargetDeviceDisplayed = true });
                AvailableActionsforCurrentDevice.Add(new ActionType { Name = "Tắt", Description = "Tắt một tính năng", Geometry = "apply", Type = "Off", LinkText = "Của thiết bị", IsValueDisplayed = false, IsTargetDeviceDisplayed = true });
                AvailableActionsforCurrentDevice.Add(new ActionType { Name = "Chuyển", Description = "Chuyển đổi đồng thời kích hoạt một tính năng", Geometry = "apply", Type = "Change", LinkText = "Của thiết bị", ToResultText = "thành", IsValueDisplayed = true, IsTargetDeviceDisplayed = true });
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }
        private void OpenHotKeySelectionWindow(AutomationSettings selectedautomation)
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
        private void OpenTargetDeviceSelectionWindow(ActionSettings selectedAction)
        {

            CurrentSelectedAction = selectedAction;
            RaisePropertyChanged(nameof(CurrentSelectedAction));
            ParamType = "device";
            AutomationParamList = new ObservableCollection<object>();
            foreach (var device in AvailableDevices)
            {
                if (CurrentSelectedAction.ActionParameter.Type == "speed") // only filter hubfan
                {
                    if (device.DeviceType.Type == DeviceTypeEnum.AmbinoFanHub)
                        AutomationParamList.Add(device);
                }
                else
                {
                    AutomationParamList.Add(device);
                }

            }

            if (AssemblyHelper.CreateInternalInstance($"View.{"TargetDeviceSelectionWindow"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }
        private void OpenTargetActionSelectionWindow(ActionSettings selectedAction)
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
        private ObservableCollection<object> _automationValuePickerList;
        public ObservableCollection<object> AutomationValuePickerList {
            get { return _automationValuePickerList; }
            set
            {
                _automationValuePickerList = value;
                RaisePropertyChanged();
            }
        }
        private void OpenAutomationValuePickerWindow(ActionSettings selectedAction)
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"AutomationValuePickerWindow"}") is System.Windows.Window window)
            {
                CurrentSelectedAction = selectedAction;
                var targetDevice = AvailableDevices.Where(x => x.DeviceUID == CurrentSelectedAction.TargetDeviceUID).FirstOrDefault();
                if (targetDevice == null)
                    return;
                CurrentSelectedAction = selectedAction;
                RaisePropertyChanged(nameof(CurrentSelectedAction));
                AutomationValuePickerList = new ObservableCollection<object>();
                switch (selectedAction.ActionParameter.Type)
                {
                    case "color":
                        (targetDevice.AvailableLightingDevices[0].ControlableZones[0] as LEDSetup).GetStaticColorDataSource().ForEach(c => AutomationValuePickerList.Add(c));
                        break;
                    case "mode":
                        targetDevice.AvailableLightingDevices[0].ControlableZones[0].AvailableControlMode.ForEach(m => AutomationValuePickerList.Add((m as LightingMode).BasedOn));
                        break;
                }
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }

        private ActionParameter GetAutoMationParam(string paramType, string value)
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
                case "mode":
                    returnParam.Name = "Chế độ";
                    returnParam.Geometry = "brightness";
                    returnParam.Type = "mode";
                    returnParam.Value = value;
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
        private void OpenTargetParamSelectionWindow(ActionSettings selectedAction)
        {


            CurrentSelectedAction = selectedAction;
            RaisePropertyChanged(nameof(CurrentSelectedAction));
            ParamType = "param";
            var targetDevice = AvailableDevices.Where(x => x.DeviceUID == CurrentSelectedAction.TargetDeviceUID).FirstOrDefault();

            switch (CurrentSelectedAction.ActionType.Type)
            {
                case "Activate":
                    AutomationParamList = new ObservableCollection<object>();
                    foreach (var profile in AvailableProfiles)
                    {
                        AutomationParamList.Add(new ActionParameter { Geometry = profile.Geometry, Name = profile.Name, Type = "profile", Value = profile.Name });
                    }
                    break;
                case "Increase":
                    if (targetDevice == null)
                        return;
                    AutomationParamList = new ObservableCollection<object>();
                    AutomationParamList.Add(GetAutoMationParam("brightness", "up"));
                    AutomationParamList.Add(GetAutoMationParam("speed", "up"));
                    break;
                case "Decrease":
                    if (targetDevice == null)
                        return;
                    AutomationParamList = new ObservableCollection<object>();
                    AutomationParamList.Add(GetAutoMationParam("brightness", "down"));
                    AutomationParamList.Add(GetAutoMationParam("speed", "down"));
                    break;
                case "Off":
                    if (targetDevice == null)
                        return;
                    AutomationParamList = new ObservableCollection<object>();
                    AutomationParamList.Add(GetAutoMationParam("state", "off"));
                    break;
                case "On":
                    if (targetDevice == null)
                        return;
                    AutomationParamList = new ObservableCollection<object>();
                    AutomationParamList.Add(GetAutoMationParam("state", "on"));
                    break;
                case "Change":
                    if (targetDevice == null)
                        return;
                    AutomationParamList = new ObservableCollection<object>();
                    AutomationParamList.Add(GetAutoMationParam("color", "#ffff53c9"));
                    if (targetDevice != null)
                        AutomationParamList.Add(GetAutoMationParam("mode", targetDevice.AvailableLightingDevices[0].ControlableZones[0].CurrentActiveControlMode.Name));
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
            ActionSettings newBlankAction = new ActionSettings {
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
                CurrentSelectedAutomation.Actions = new ObservableCollection<ActionSettings>();
            }
            CurrentSelectedAutomation.Actions.Add(newBlankAction);
            RaisePropertyChanged(nameof(CurrentSelectedAutomation.Actions));
        }
        private void DeleteSelectedActionFromList(ActionSettings action)
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
        private void SetCurrentActionParamForSelectedAction(ActionParameter param)
        {
            CurrentSelectedAction.ActionParameter = param;
            RaisePropertyChanged(nameof(CurrentSelectedAction.ActionType));
        }
        private void SetCurrentSelectedActionTypeColorValue(Color color)
        {
            CurrentSelectedAction.ActionParameter.Value = color;
            RaisePropertyChanged(nameof(CurrentSelectedAction.ActionParameter.Value));
        }

        #region Online Item Exporter

        public OnlineItemExporterView onlineExportWindow;
        public IOnlineItemModel CurrentItemForExport { get; set; }
        public object CurrentContentForExport { get; set; }
        public ObservableCollection<string> OnlineItemScreenShotCollection { get; set; }
        private string _onlineItemAvatar;
        public string OnlineItemAvatar {
            get { return _onlineItemAvatar; }
            set { _onlineItemAvatar = value; RaisePropertyChanged(); }
        }
        private List<DeviceType> _onlineItemSelectedTargetTypes;
        public List<DeviceType> OnlineItemSelectedTargetTypes {
            get { return _onlineItemSelectedTargetTypes; }
            set { _onlineItemSelectedTargetTypes = value; RaisePropertyChanged(); }
        }
        private string _onlineItemMarkdownDescription;
        public string OnlineItemMarkdownDescription {
            get { return _onlineItemMarkdownDescription; }
            set { _onlineItemMarkdownDescription = value; RaisePropertyChanged(); }
        }
        public ObservableCollection<DeviceType> OnlineItemSelectableTargetType { get; set; }
        private void ExportCurrentOnlineItemToFiles()
        {
            //creat all needed path
            SaveFileDialog Export = new SaveFileDialog();
            Export.CreatePrompt = true;
            Export.OverwritePrompt = true;
            Export.Title = "Xuất dữ liệu";
            Export.FileName = CurrentItemForExport.Name;
            Export.CheckFileExists = false;
            Export.CheckPathExists = true;
            Export.InitialDirectory =
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            Export.RestoreDirectory = true;
            string contentjson = "";
            var content = CurrentContentForExport;
            contentjson = JsonConvert.SerializeObject(content, new JsonSerializerSettings() {
                TypeNameHandling = TypeNameHandling.Auto
            });
            if (Export.ShowDialog() == DialogResult.OK)
            {
                //create directory with same name
                var newFolder = Directory.CreateDirectory(Export.FileName);
                var contentFolder = Directory.CreateDirectory(Path.Combine(Export.FileName, "content")).ToString();
                //create main content 
                File.WriteAllText(Path.Combine(Export.FileName, "content", "config.json"), contentjson);
                //create info
                var info = new OnlineItemModel() {
                    Name = CurrentItemForExport.Name,
                    Owner = CurrentItemForExport.Owner,
                    Description = CurrentItemForExport.Description,
                    Type = CurrentItemForExport.Type,
                    TargetDevices = OnlineItemSelectedTargetTypes.ToList()

                };
                var infoJson = JsonConvert.SerializeObject(info, new JsonSerializerSettings() {
                    TypeNameHandling = TypeNameHandling.Auto
                });
                File.WriteAllText(Path.Combine(Export.FileName, "info.json"), infoJson);
                File.WriteAllText(Path.Combine(Export.FileName, "description.md"), OnlineItemMarkdownDescription);
                //coppy image data
                File.Copy(OnlineItemAvatar, Path.Combine(Export.FileName, "thumb.png"));
                //copy thumbnail also
                File.Copy(OnlineItemAvatar, Path.Combine(Export.FileName, "content", "thumbnail.png"));
                Directory.CreateDirectory(Path.Combine(Export.FileName, "screenshots"));
                int count = 0;
                foreach (var file in OnlineItemScreenShotCollection)
                {
                    File.Copy(file, Path.Combine(Export.FileName, "screenshots", "screenshot" + "_" + count++.ToString() + ".png"));
                }
            }
        }
        private void OpenImageSelector(string type)
        {
            System.Windows.Forms.OpenFileDialog Import = new System.Windows.Forms.OpenFileDialog();
            Import.Title = "Chọn Screenshot";
            Import.CheckFileExists = true;
            Import.CheckPathExists = true;
            Import.DefaultExt = "Pro";
            Import.Filter = "Image files (*.png)|*.Png";
            Import.FilterIndex = 2;
            Import.Multiselect = type == "screenshot";
            Import.ShowDialog();
            switch (type)
            {
                case "screenshot":
                    for (int i = 0; i < Import.FileNames.Length; i++)
                    {
                        //add string path to list
                        OnlineItemScreenShotCollection.Add(Import.FileNames[i].ToString());
                    }
                    break;
                case "avatar":
                    OnlineItemAvatar = Import.FileName;
                    break;
            }
        }

        private void ExportItemForOnlineStore(object p)
        {
            CurrentContentForExport = p;
            CurrentItemForExport = new OnlineItemModel() { Name = "Change Name", Description = "Change Description", Type = p.GetType().Name.ToString(), Owner = "Change Owner" };
            OnlineItemScreenShotCollection = new ObservableCollection<string>();
            OnlineItemSelectableTargetType = new ObservableCollection<DeviceType>();
            OnlineItemSelectedTargetTypes = new List<DeviceType>();

            if (p.GetType() == typeof(ARGBLEDSlaveDevice))
            {
                CurrentItemForExport.Name = (p as ARGBLEDSlaveDevice).Name;
                foreach (DeviceTypeEnum type in Enum.GetValues(typeof(DeviceTypeEnum)))
                {
                    OnlineItemSelectableTargetType.Add(new DeviceType(type));
                }
            }
            else if (p.GetType() == typeof(ColorPalette))
            {
                CurrentItemForExport.Name = (p as ColorPalette).Name;
                foreach (DeviceTypeEnum type in Enum.GetValues(typeof(DeviceTypeEnum)))
                {
                    OnlineItemSelectableTargetType.Add(new DeviceType(type));
                }
            }
            else if (p.GetType() == typeof(AppProfile))
            {
                CurrentItemForExport.Name = (p as AppProfile).Name;
                foreach (var deviceProfile in (p as AppProfile).DeviceProfiles)
                {
                    OnlineItemSelectableTargetType.Add(deviceProfile.DeviceType);
                }
            }
            onlineExportWindow = new OnlineItemExporterView();
            onlineExportWindow.Show();
        }
        #endregion

        #region Online Item downloader
        private async Task SaveItemToLocalCollection<T>(string collectionPath, DeserializeMethodEnum mode, IOnlineItemModel item, string extension)
        {
            if (!Directory.Exists(collectionPath))
                Directory.CreateDirectory(collectionPath);
            //check for duplicate item
            string[] subdirs = Directory.GetDirectories(collectionPath)
                             .Select(Path.GetFileName)
                             .ToArray();
            var sameNameItem = subdirs.Where(p => p == item.Name).FirstOrDefault();
            if (sameNameItem != null)
                HandyControl.Controls.MessageBox.Show("File đã tồn tại", "File Existed", MessageBoxButton.OK, MessageBoxImage.Error);

            if (FTPHlprs != null)
            {
                //get list of files
                var listofFiles = await FTPHlprs.GetAllFilesInFolder(item.Path + "/content");
                if (listofFiles == null)
                {
                    HandyControl.Controls.MessageBox.Show("Không tìm thấy file, vui lòng chọn nội dung khác!!", "File not found", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                switch (mode)
                {
                    case DeserializeMethodEnum.MultiJson:
                        //save to local folder , we only need json config file
                        var data = await FTPHlprs.GetFiles<T>(item.Path + "/content" + "/" + "config.json");
                        WriteSimpleJson(data, Path.Combine(Path.Combine(ProfileCollectionFolderPath, "collection"), item.Name + extension));
                        break;
                    case DeserializeMethodEnum.FolderStructure:
                        var localPath = Path.Combine(collectionPath, item.Name);
                        Directory.CreateDirectory(localPath);
                        foreach (var file in listofFiles)
                        {
                            //save to local folder
                            FTPHlprs.DownloadFile(item.Path + "/content" + "/" + file.Name, localPath + "/" + file.Name);
                        }
                        break;
                }

            }



        }

        private async Task DownloadCurrentOnlineItem(IOnlineItemModel o)
        {
            //create needed info 
            string localFolder = string.Empty; // the resource folder this item will be saved to
            switch (o.Type)
            {
                case "ColorPalette":
                    await SaveItemToLocalCollection<ColorPalette>(PalettesCollectionFolderPath, DeserializeMethodEnum.MultiJson, o, ".col");
                    break;
                case "ARGBLEDSlaveDevice":
                    await SaveItemToLocalCollection<ARGBLEDSlaveDevice>(SupportedDeviceCollectionFolderPath, DeserializeMethodEnum.FolderStructure, o, string.Empty);
                    break;
                case "AppProfile":
                    await SaveItemToLocalCollection<AppProfile>(ProfileCollectionFolderPath, DeserializeMethodEnum.MultiJson, o, ".aap");
                    break;
            }


        }
        #endregion















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
                if (value != null)
                {
                    CurrentOnlineStoreView = "Collections";
                    CarouselImageLoading = true;
                    Task.Run(() => UpdateStoreView());
                }



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
            //var sameNamePalette = AvailablePallete.Where(p => p.Name == selectedPalette.Name).FirstOrDefault();
            //if (CheckEqualityObjects(selectedPalette, sameNamePalette))
            //    HandyControl.Controls.MessageBox.Show("ColorPaletteExisted", "File Existed", MessageBoxButton.OK, MessageBoxImage.Error);
            //else
            //{
            //    AvailablePallete.Add(selectedPalette);
            //    //save to local disk
            //    var paletteJson = JsonConvert.SerializeObject(selectedPalette, new JsonSerializerSettings() {
            //        TypeNameHandling = TypeNameHandling.Auto
            //    });
            //    File.WriteAllText(Path.Combine(PalettesCollectionFolderPath, selectedPalette.Name + ".col"), paletteJson);

            //}



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
                case "SupportedDevice":
                    await GetStoreItem(SupportedDevicesFolderPath);
                    CarouselImageLoading = false;
                    break;
                case "Profiles":
                    await GetStoreItem(ProfilesFolderPath);
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
        #region Store Folder Path
        private const string paletteFolderpath = "/home/adrilight_enduser/ftp/files/ColorPalettes";
        private const string chasingPatternsFolderPath = "/home/adrilight_enduser/ftp/files/ChasingPatterns";
        private const string gifxelationsFolderPath = "/home/adrilight_enduser/ftp/files/Gifxelations";
        private const string outputLEDSetupsFolderPath = "/home/adrilight_enduser/ftp/files/OutputLEDSetups";
        private const string SupportedDevicesFolderPath = "/home/adrilight_enduser/ftp/files/SupportedDevices";
        private const string ProfilesFolderPath = "/home/adrilight_enduser/ftp/files/Profiles";
        #endregion
        #region Resource Folder Path
        private const string thumbResourceFolderPath = "/home/adrilight_enduser/ftp/files/Resources/Thumbs";
        #endregion
        private async Task GetStoreItem(string itemFolderPath)
        {
            AvailableOnlineItems = new ObservableCollection<IOnlineItemModel>();

            //get carousel image
            //get all available files
            var listItemAddress = await FTPHlprs.GetAllFilesAddressInFolder(itemFolderPath);
            //display all available files to the view 
            if (listItemAddress != null && listItemAddress.Count > 0)
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
                    Name = "Colors",
                    Type = "Palette",
                    Description = "All Color Palette created by Ambino and Contributed by Ambino Community",
                    Geometry = "palette"
                };
                var chasingPatterns = new StoreCategory() {
                    Name = "Animations",
                    Type = "Pattern",
                    Description = "All Color Palette created by Ambino and Contributed by Ambino Community",
                    Geometry = "chasingPattern"
                };
                var supportedDevices = new StoreCategory() {
                    Name = "Devices",
                    Type = "SupportedDevice",
                    Description = "All Color Palette created by Ambino and Contributed by Ambino Community",
                    Geometry = "slaveDevice"
                };
                var profiles = new StoreCategory() {
                    Name = "Profiles",
                    Type = "Profiles",
                    Description = "Color Lighting and Setup profile by Ambino and Ambino User",
                    Geometry = "appProfile"
                };

                AvailableStoreCategories.Add(palettes);
                AvailableStoreCategories.Add(chasingPatterns);
                AvailableStoreCategories.Add(supportedDevices);
                AvailableStoreCategories.Add(profiles);
            }
            if (CurrentSelectedCategory == null)
            {
                CurrentSelectedCategory = AvailableStoreCategories.First();
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



        private void OpenAddNewAutomationWindowCommand()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"AddNewAutomationWindow"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }
        private ListSelectionParameter _accentColorSelection;
        public ListSelectionParameter AccentColorSelection {
            get { return _accentColorSelection; }
            set
            {
                _accentColorSelection = value;
                RaisePropertyChanged();
            }
        }
        private void OpenAppSettingsWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"AppSettingsWindow"}") is System.Windows.Window window)
            {
                //init app acent color selection
                AccentColorSelection = new ListSelectionParameter(ModeParameterEnum.Color) {

                    Name = "Màu chủ đề",
                    Description = "Chọn màu chủ đạo cho ứng dụng, Sau khi chọn màu, bạn nên khởi động lại ứng dụng",
                    DataSourceLocaFolderNames = new List<string>() { "Colors" }
                };
                AccentColorSelection.LoadAvailableValues();
                AccentColorSelection.SelectedValue = new ColorCard(GeneralSettings.AccentColor, GeneralSettings.AccentColor);
                AccentColorSelection.PropertyChanged += (_, __) =>
                {
                    switch (__.PropertyName)
                    {
                        case nameof(AccentColorSelection.SelectedValue):
                            ThemeManager.Current.AccentColor = new SolidColorBrush((AccentColorSelection.SelectedValue as ColorCard).StartColor);
                            GeneralSettings.AccentColor = (AccentColorSelection.SelectedValue as ColorCard).StartColor;

                            break;
                    }
                };
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
                        //AvailableProfiles.Add(existedprofile);
                        RaisePropertyChanged(nameof(AvailableProfiles));
                        // WriteDeviceProfileCollection();
                        AvailableProfilesForCurrentDevice.Clear();

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


        public void SaveCurrentProfile(string profileUID)
        {
            //var currentProfile = AvailableProfiles.Where(p => p.ProfileUID == profileUID).FirstOrDefault();
            //if (currentProfile != null)
            //    // currentProfile.SaveProfile(CurrentDevice.AvailableOutputs);

            //    // WriteDeviceProfileCollection();

            //    //Growl.Success("Profile saved successfully!");
            //    IsSettingsUnsaved = BadgeStatus.Dot;
        }
        public void DeleteAttachedProfile(AppProfile profile)
        {
            //check if profile is in used
            if (profile.IsActivated)
            {
                HandyControl.Controls.MessageBox.Show(profile.Name + " Không thể xóa, profile này đang được sử dụng!!!", "Profile is in used", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                AvailableProfiles.Remove(profile);
                //delete from disk
            }

        }
        private AppProfile _currentActivatedProfile;
        public AppProfile CurrentActivatedProfile {
            get { return _currentActivatedProfile; }
            set { _currentActivatedProfile = value; RaisePropertyChanged(); }
        }
        private bool _isloadingProfile = false;
        public void ActivateSelectedProfile(AppProfile profile)
        {
            //construct devices from profile data
            //check if any connected device has same type and hardware version
            List<IDeviceSettings> compatibleDevices = new List<IDeviceSettings>();
            lock (AvailableDeviceLock)
            {
                _isloadingProfile = true;
                int counter = 0;
                foreach (var deviceProfile in profile.DeviceProfiles)
                {
                    var targetDevice = AvailableDevices.Where(d => d.DeviceType.Type == deviceProfile.DeviceSettings.DeviceType.Type).FirstOrDefault();
                    if (targetDevice != null)
                    {
                        //get uid for local writing
                        var device = ObjectHelpers.Clone<DeviceSettings>(deviceProfile.DeviceSettings as DeviceSettings);
                        device.DeviceUID = targetDevice.DeviceUID;
                        device.DeviceName = targetDevice.DeviceName;
                        device.IsLoadingProfile = false;
                        device.OutputPort = targetDevice.OutputPort;
                        compatibleDevices.Add(device);
                        AvailableDevices.Remove(targetDevice);
                        counter++;
                    }
                }
                //takes what left 
                compatibleDevices.AddRange(AvailableDevices.ToList());
                AvailableDevices.Clear();//reset the collection so kernel can be unbind
                compatibleDevices.ForEach(d => AvailableDevices.Add(d));
                SelectedViewPart = SelectableViewParts[0];
                if (counter == 0)
                {
                    HandyControl.Controls.MessageBox.Show("Profile không thích hợp với bất kỳ thiết bị nào hiện có", "No Device Compatible", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    HandyControl.Controls.MessageBox.Show("Áp dụng thành công Profile" + profile.Name, "Profile Activated", MessageBoxButton.OK, MessageBoxImage.Information);
                    CurrentActivatedProfile = profile;
                }
                _isloadingProfile = false;

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
                //if (AmbinityClient != null)
                //    AmbinityClient.Dispose();
                //remember to dispose openrgbstream too!!!
                await System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    _splashScreen.status.Text = "RESTARTING...";
                });
                UpdateManager.RestartApp();
            }
            //this.logger.Info($"Download complete. Version {updateResult.Version} will take effect when App is restarted.");



        }

        private void SaveAllAutomation()
        {
            WriteAutomationCollectionJson();
            AvailableAutomations = new ObservableCollection<AutomationSettings>();
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
        public ObservableCollection<IDeviceSettings> SelectedDevicesForCurrentProfile { get; set; }
        private void CreateNewProfile()
        {
            if (SelectedDevicesForCurrentProfile.Count == 0)
            {
                HandyControl.Controls.MessageBox.Show("Bạn phải chọn ít nhất một thiết bị tham gia vào profile này", "No device selected!!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var newAppProfile = new AppProfile();
            newAppProfile.Name = NewProfileName;
            newAppProfile.Owner = NewProfileOwner;
            newAppProfile.Description = NewProfileDescription;
            foreach (var device in SelectedDevicesForCurrentProfile)
            {
                var newprofile = new DeviceProfile {
                    Name = device.DeviceName + "Profile",
                    Description = "UserCreated profile for" + device.DeviceName,
                    Geometry = "profile",
                    ProfileUID = device.DeviceUID
                };
                newprofile.SaveProfile(device);
                newAppProfile.DeviceProfiles.Add(newprofile);
            }

            //AvailableProfiles.Add(newprofile);
            AvailableProfiles.Add(newAppProfile);
            WriteSimpleJson(newAppProfile, Path.Combine(Path.Combine(ProfileCollectionFolderPath, "collection"), newAppProfile.Name + ".aap")); // .aap stands for adrilight application profile ...
            addNewProfileWindow.Close();
            //WriteDeviceProfileCollection();
        }
        private AddNewProfileWindow addNewProfileWindow { get; set; }
        private void OpenCreatenewProfileWindow()
        {
            addNewProfileWindow = new AddNewProfileWindow();
            addNewProfileWindow.Owner = System.Windows.Application.Current.MainWindow;
            SelectedDevicesForCurrentProfile = new ObservableCollection<IDeviceSettings>();
            addNewProfileWindow.ShowDialog();

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

        private void ExecuteAutomationActions(ObservableCollection<ActionSettings> actions)
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
                    //case "Activate":
                    //    var destinationProfile = AvailableProfiles.Where(x => x.ProfileUID == (string)action.ActionParameter.Value).FirstOrDefault();
                    //    if (destinationProfile != null)
                    //    {
                    //        targetDevice.ActivateProfile(destinationProfile);
                    //        //at this moment, selected profile for the target device changed but the UI know nothing about this
                    //        //because the porperty changed event didnt fire
                    //        //so we fire profile UID changed instead
                    //        targetDevice.ActivatedProfileUID = destinationProfile.ProfileUID;
                    //    }

                    //    break;

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
            if (device.DeviceType.Type == DeviceTypeEnum.AmbinoHUBV2)
            {
                MessageBoxResult result = HandyControl.Controls.MessageBox.Show("HUBV2 cần sử dụng FlyMCU để nạp firmware, nhấn [OK] để vào chế độ DFU", "External Software Required", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    device.DeviceState = DeviceStateEnum.DFU;

                    Thread.Sleep(5000);
                    device.DeviceState = DeviceStateEnum.Normal;
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
                        AvailableFirmwareForCurrentDevice = new ObservableCollection<DeviceFirmware>();
                        foreach (var firmware in availableFirmware)
                        {
                            if (firmware.TargetDeviceType == device.DeviceType.Type)
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
                            AvailableFirmwareForCurrentDevice = new ObservableCollection<DeviceFirmware>();
                            foreach (var firmware in availableFirmware)
                            {
                                if (firmware.TargetDeviceType == device.DeviceType.Type)
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
                                device.DeviceState = DeviceStateEnum.DFU;
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
            CurrentDevice.DeviceState = DeviceStateEnum.Normal;

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


        private void UnZone()
        {

            var zoneList = PIDEditWindowsRichCanvasItems.Where(z => z is LEDSetup && z.IsSelected).ToList();
            //zoneList.ForEach(zone => zone.IsSelected = false);
            foreach (var zone in zoneList)
            {

                var currentZone = zone as LEDSetup;
                //(SurfaceEditorSelectedItem as ARGBLEDSlaveDevice).ControlableZones.Remove(currentZone);
                //add spot to temp bag
                var spotTempBag = new List<DeviceSpot>();
                foreach (var spot in currentZone.Spots)
                {
                    var clonedSpot = ObjectHelpers.Clone<DeviceSpot>(spot as DeviceSpot);
                    spotTempBag.Add(clonedSpot);
                }
                //remove zone from canvas

                foreach (var spot in spotTempBag)
                {
                    (spot as DeviceSpot).Left += currentZone.Left;
                    (spot as DeviceSpot).Top += currentZone.Top;
                    PIDEditWindowsRichCanvasItems.Add(spot as IDrawable);
                }
            }
            PIDEditWindowSelectedItems.Clear();
            zoneList.ForEach(zone => PIDEditWindowsRichCanvasItems.Remove(zone));

            //now add item from temp bag



        }
        private object NameChangingSelectedItem { get; set; }
        private void RenameZone()
        {
            NameChangingSelectedItem = PIDEditWindowsRichCanvasItems.Where(z => z is LEDSetup && z.IsSelected).FirstOrDefault();
            OpenNameEditWindow(pidEditCanvasWindow);

            //now add item from temp bag
        }
        private void AddNewZone(ObservableCollection<IDrawable> itemSource, ObservableCollection<IDrawable> selectedItemsSource) // create new zone from selected spot fomr wellknown itemsource
        {

            var newZone = new LEDSetup();
            var spotList = itemSource.Where(s => s is DeviceSpot && s.IsSelected).ToList();
            //spotList.ForEach(spot => spot.IsSelected = false);
            if (spotList.Count < 1)
                return;
            foreach (var spot in spotList)
            {
                var clonedSpot = ObjectHelpers.Clone<DeviceSpot>(spot as DeviceSpot);
                newZone.Spots.Add(clonedSpot);

            }
            newZone.UpdateSizeByChild(true);
            spotList.ForEach(spot => itemSource.Remove(spot));
            foreach (var spot in newZone.Spots)
            {

                (spot as DeviceSpot).Left -= newZone.Left;
                (spot as DeviceSpot).Top -= newZone.Top;
                (spot as DeviceSpot).IsSelected = false;

            }
            selectedItemsSource.Clear();
            itemSource.Add(newZone);

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
                        (spot as IDrawable).IsSelected = false;
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

        private void OpenNameEditWindow(System.Windows.Window owner)
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"RenameItemWindow"}") is System.Windows.Window window)
            {
                window.Owner = owner;
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





        private string _currentIDType;
        public string CurrentIDType {
            get { return _currentIDType; }
            set { _currentIDType = value; }
        }

        private bool _isRegisteringGroup;
        public bool IsRegisteringGroup {
            get { return _isRegisteringGroup; }
            set { _isRegisteringGroup = value; RaisePropertyChanged(); }
        }
        private void RegisterGroupItem(List<LEDSetup> childItems, ControlZoneGroup group)
        {
            IsRegisteringGroup = true;
            foreach (var item in childItems)
            {
                item.IsSelected = false;
                item.MaskedControlMode = group.MaskedControlZone.CurrentActiveControlMode;
                item.IsSelectable = false;
                item.IsInControlGroup = true;
                item.GroupID = group.GroupUID;

            }
            group.MaskedControlZone.PropertyChanged += (_, __) =>
            {
                WriteSingleDeviceInfoJson(CurrentDevice);
                switch (__.PropertyName)
                {
                    case nameof(group.MaskedControlZone.CurrentActiveControlModeIndex):
                        foreach (var item in childItems)
                        {
                            item.MaskedControlMode = group.MaskedControlZone.CurrentActiveControlMode;
                        }
                        break;
                }
            };
            IsRegisteringGroup = false;

        }
        private void RegisterGroupItem(List<FanMotor> childItems, ControlZoneGroup group)
        {
            IsRegisteringGroup = true;
            foreach (var item in childItems)
            {
                item.IsSelected = false;
                item.MaskedControlMode = group.MaskedControlZone.CurrentActiveControlMode;
                item.IsSelectable = false;
                item.IsInControlGroup = true;
                item.GroupID = group.GroupUID;

            }
            group.MaskedControlZone.PropertyChanged += (_, __) =>
            {
                WriteSingleDeviceInfoJson(CurrentDevice);
                switch (__.PropertyName)
                {
                    case nameof(group.MaskedControlZone.CurrentActiveControlModeIndex):
                        foreach (var item in childItems)
                        {
                            item.MaskedControlMode = group.MaskedControlZone.CurrentActiveControlMode;
                        }
                        break;
                }
            };
            IsRegisteringGroup = false;

        }
        private void RemoveGroup(ControlZoneGroup group)
        {
            //turn on Isregistering group
            IsRegisteringGroup = true;
            //remove border from canvas
            if (group == null)
                return;
            LiveViewItems.Remove(LiveViewItems.Where(b => b.Name == group.Border.Name).FirstOrDefault());
            //set each zone selectable
            foreach (var zone in CurrentDevice.CurrentLiveViewZones)
            {
                if (group.GroupUID == zone.GroupID)
                {
                    //set each zone IsIngroup to false
                    //set maskedControl to null
                    zone.GroupID = null;
                    zone.IsInControlGroup = false;
                    (zone as IDrawable).IsSelectable = true;
                    (zone as IControlZone).MaskedControlMode = null;
                }
            }
            //SelectedSlaveDevice = null;
            CurrentDevice.ControlZoneGroups.Remove(group);
            //
            //remove group
            //turn off IsRegisteringGroup
            IsRegisteringGroup = false;
        }
        private void UngroupZone()
        {
            //turn on Isregistering group
            IsRegisteringGroup = true;
            //remove border from canvas
            var borderToRemove = LiveViewItems.Where(p => p.IsSelected && p is Border).FirstOrDefault();
            if (borderToRemove == null)
                return;
            LiveViewItems.Remove(borderToRemove);
            var currentGroup = CurrentDevice.ControlZoneGroups.Where(g => g.Border.Name == borderToRemove.Name && g.Type == CurrentDevice.CurrentActiveController.Type).FirstOrDefault();
            //set each zone selectable
            foreach (var zone in CurrentDevice.CurrentLiveViewZones)
            {
                if (currentGroup.GroupUID == zone.GroupID)
                {
                    //set each zone IsIngroup to false
                    //set maskedControl to null
                    zone.GroupID = null;
                    zone.IsInControlGroup = false;
                    (zone as IDrawable).IsSelectable = true;
                    (zone as IControlZone).MaskedControlMode = null;
                }
            }
            SelectedSlaveDevice = null;
            CurrentDevice.ControlZoneGroups.Remove(currentGroup);
            //
            //remove group
            //turn off IsRegisteringGroup
            IsRegisteringGroup = false;
        }
        private List<IControlZone> GetGroupChildItems(ControlZoneGroup group, IDeviceSettings device)
        {
            List<IControlZone> childItems = new List<IControlZone>();
            foreach (var zone in device.AvailableControlZones)
            {
                if (zone.GroupID == group.GroupUID)
                {
                    childItems.Add(zone);
                }
            }
            return childItems;
        }
        private Border GetGroupBorder(List<IControlZone> groupItems, ControlZoneGroup group)
        {
            if (DrawableHlprs == null)
            {
                DrawableHlprs = new DrawableHelpers();
            }
            if (groupItems.Count == 0)
            {
                //this could happen due to ledsetup changing
                //remove this group
                RemoveGroup(group);
                return null;

            }

            var left = DrawableHlprs.GetRealBound(groupItems.ToArray()).Left;
            var top = DrawableHlprs.GetRealBound(groupItems.ToArray()).Top;
            var width = DrawableHlprs.GetRealBound(groupItems.ToArray()).Width;
            var height = DrawableHlprs.GetRealBound(groupItems.ToArray()).Height;
            var border = new Border() {
                Name = group.Name,
                Left = left,
                Top = top,
                Width = width,
                Height = height,
                IsSelectable = true,
            };
            if (group.Border != null)
            {
                group.Border.Name = border.Name;
                group.Border.Left = border.Left;
                group.Border.Top = border.Top;
                group.Border.Width = border.Width;
                group.Border.Height = border.Height;
            }
            else
            {
                group.Border = border;
            }
            foreach (var item in groupItems)
            {
                if (item is LEDSetup)
                {
                    var zone = item as LEDSetup;
                    zone.VIDSpace = new Rect(group.Border.Left, group.Border.Top, group.Border.Width, group.Border.Height);
                }

            }
            (group.MaskedControlZone as IDrawable).Left = group.Border.Left;
            (group.MaskedControlZone as IDrawable).Top = group.Border.Top;
            (group.MaskedControlZone as IDrawable).Width = group.Border.Width;
            (group.MaskedControlZone as IDrawable).Height = group.Border.Height;
            return border;
        }
        private void AddZoneToGroup()
        {
            if (!File.Exists(Path.Combine(ResourceFolderPath, "Group_thumb.png")))
            {
                Directory.CreateDirectory(ResourceFolderPath);
                ResourceHlprs.CopyResource("adrilight.Resources.Thumbnails.Group_thumb.png", Path.Combine(ResourceFolderPath, "Group_thumb.png"));
            }
            if (CurrentDevice.ControlZoneGroups == null)
            {
                CurrentDevice.ControlZoneGroups = new ObservableCollection<ControlZoneGroup>();
            }
            var selectedItems = CurrentDevice.CurrentLiveViewZones.Where(z => (z as IDrawable).IsSelected).ToList();
            if (selectedItems != null && selectedItems.Count > 1)
            {
                //more than 1 items get selected
                var newGroup = new ControlZoneGroup();
                newGroup.Name = "Group" + " " + (CurrentDevice.ControlZoneGroups.Count + 1).ToString();
                newGroup.GroupUID = Guid.NewGuid().ToString();
                //init masked control for multi zone
                if (selectedItems.First() is FanMotor)
                {
                    newGroup.Type = ControllerTypeEnum.PWMController;
                    newGroup.MaskedControlZone = ObjectHelpers.Clone<FanMotor>(selectedItems.First() as FanMotor);
                    newGroup.MaskedControlZone.Name = newGroup.Name + " - " + "MultiFan";
                    newGroup.MaskedControlZone.Description = "Masked Control for multiple fans selected";
                    newGroup.MaskedSlaveDevice = new PWMMotorSlaveDevice() {
                        Name = "Union Device",
                        Description = "Thiết bị đại diện cho " + newGroup.Name,
                        Owner = "System",
                        //Thumbnail = Path.Combine(ResourceFolderPath, "Group_thumb.png")
                    };
                }
                else if (selectedItems.First() is LEDSetup)
                {
                    newGroup.Type = ControllerTypeEnum.LightingController;
                    newGroup.MaskedControlZone = ObjectHelpers.Clone<LEDSetup>(selectedItems.First() as LEDSetup);
                    newGroup.MaskedControlZone.Name = newGroup.Name + " - " + "MultiZone";
                    newGroup.MaskedControlZone.Description = "Masked Control for multizone selected";
                    newGroup.MaskedSlaveDevice = new ARGBLEDSlaveDevice() {
                        Name = "Union Device",
                        Description = "Thiết bị đại diện cho " + newGroup.Name,
                        Owner = "System",
                        //Thumbnail = Path.Combine(ResourceFolderPath, "Group_thumb.png")
                    };
                }
                //add group border
                GetGroupBorder(selectedItems, newGroup);
                newGroup.Border.IsSelected = true;
                LiveViewItems.Add(newGroup.Border);
                switch (newGroup.Type)
                {
                    case ControllerTypeEnum.LightingController:
                        RegisterGroupItem(selectedItems.Cast<LEDSetup>().ToList(), newGroup);
                        break;
                    case ControllerTypeEnum.PWMController:
                        RegisterGroupItem(selectedItems.Cast<FanMotor>().ToList(), newGroup);
                        break;
                }


                SelectedControlZone = newGroup.MaskedControlZone;
                //set display slave device
                SelectedSlaveDevice = newGroup.MaskedSlaveDevice;
                CanUnGroup = true;
                CanGroup = false;
                CurrentDevice.ControlZoneGroups.Add(newGroup);

            }
        }


        private IControlZone _selectedControlZone;
        public IControlZone SelectedControlZone {
            get { return _selectedControlZone; }
            set { _selectedControlZone = value; RaisePropertyChanged(); }
        }
        private ISlaveDevice _selectedSlaveDevice;
        public ISlaveDevice SelectedSlaveDevice {
            get { return _selectedSlaveDevice; }
            set { _selectedSlaveDevice = value; RaisePropertyChanged(); }
        }
        private double _currentLiveViewScale;
        public double CurrentLiveViewScale {
            get { return _currentLiveViewScale; }
            set { _currentLiveViewScale = value; RaisePropertyChanged(); }
        }
        private double _currentLiveViewWidth;
        public double CurrentLiveViewWidth {
            get { return _currentLiveViewWidth; }
            set { _currentLiveViewWidth = value; RaisePropertyChanged(); if (value > 0) UpdateLiveView(); }
        }
        private double _currentLiveViewHeight;
        public double CurrentLiveViewHeight {
            get { return _currentLiveViewHeight; }
            set { _currentLiveViewHeight = value; RaisePropertyChanged(); if (value > 0) UpdateLiveView(); }
        }
        private Point _currentLiveViewOffset;
        public Point CurrentLiveViewOffset {
            get { return _currentLiveViewOffset; }
            set { _currentLiveViewOffset = value; RaisePropertyChanged(); }
        }
        private void UpdateLiveView()
        {
            if (!IsLiveViewOpen)
                return;
            //simple just set the scale
            CurrentDevice.UpdateChildSize();
            if (DrawableHlprs == null)
                DrawableHlprs = new DrawableHelpers();
            var liveViewItemsBound = DrawableHlprs.GetRealBound(LiveViewItems.Where(i => i is not PathGuide).ToArray());
            var widthScale = (CurrentLiveViewWidth - 50) / liveViewItemsBound.Width;
            var scaleHeight = (CurrentLiveViewHeight - 50) / liveViewItemsBound.Height;
            CurrentLiveViewScale = Math.Min(widthScale, scaleHeight);
            var currentWidth = liveViewItemsBound.Width * CurrentLiveViewScale;
            var currentHeight = liveViewItemsBound.Height * CurrentLiveViewScale;
            //set current device offset
            CurrentLiveViewOffset = new Point(0 - liveViewItemsBound.Left * CurrentLiveViewScale + (CurrentLiveViewWidth - currentWidth) / 2, 0 - liveViewItemsBound.Top * CurrentLiveViewScale + (CurrentLiveViewHeight - currentHeight) / 2);
        }
        private bool _showSelectedItemToolbar;
        public bool ShowSelectedItemToolbar {
            get { return _showSelectedItemToolbar; }
            set { _showSelectedItemToolbar = value; RaisePropertyChanged(); }
        }
        private bool _canGroup = false;
        public bool CanGroup {
            get { return _canGroup; }
            set { _canGroup = value; RaisePropertyChanged(); }
        }
        private bool _canUnGroup = false;
        public bool CanUnGroup {
            get { return _canUnGroup; }
            set { _canUnGroup = value; RaisePropertyChanged(); }
        }
        private ObservableCollection<IDrawable> _liveViewSelectedItems;
        public ObservableCollection<IDrawable> LiveViewSelectedItems {
            get { return _liveViewSelectedItems; }
            set
            {
                _liveViewSelectedItems = value;
                RaisePropertyChanged();
            }
        }
        private void LiveViewSelectedItemChanged(IDrawable item)
        {
            var shouldBeSelected = IsInIDEditStage ? true : item.IsSelectable;
            if (shouldBeSelected)
            {
                item.IsSelected = true;
                ShowSelectedItemToolbar = true;
                if (item is Border && LiveViewItems.Where(p => p.IsSelected).Count() == 1)
                {
                    CanUnGroup = true;
                }
                else
                {
                    CanUnGroup = false;
                }
                if (item is LEDSetup)
                {
                    SelectedControlZone = item as IControlZone;
                    SelectedSlaveDevice = CurrentDevice.AvailableLightingDevices.Where(d => d.ControlableZones.Contains(item as IControlZone)).FirstOrDefault();
                }
                else if (item is FanMotor)
                {
                    SelectedControlZone = item as IControlZone;
                    SelectedSlaveDevice = CurrentDevice.AvailablePWMDevices.Where(d => d.ControlableZones.Contains(item as IControlZone)).FirstOrDefault();
                }
                else if (item is Border)
                {
                    foreach (var group in CurrentDevice.ControlZoneGroups)
                    {
                        if (group.Name == item.Name && group.Type == CurrentDevice.CurrentActiveController.Type)
                        {
                            //item.IsSelected = true;
                            //group.Border.IsSelected = true;
                            SelectedControlZone = group.MaskedControlZone;
                            //set display slave device
                            SelectedSlaveDevice = group.MaskedSlaveDevice;

                        }
                    }

                }


            }
        }

        private ObservableCollection<IDrawable> _liveViewItems;
        public ObservableCollection<IDrawable> LiveViewItems {
            get { return _liveViewItems; }
            set
            {
                _liveViewItems = value;
                RaisePropertyChanged();
            }
        }
        private int _vidCount;
        public int VIDCount {
            get { return _vidCount; }
            set
            {
                _vidCount = value;
                RaisePropertyChanged();
            }
        }
        private double _lastBrushX;
        private double _lastBrushY;
        private Double CalculateDelta(double lastX, double lastY, double currentX, double currentY)
        {
            var deltaX = currentX - lastX;
            var deltaY = currentY - lastY;
            return Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY));
        }
        private void LiveViewSelectedItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {

            if (e.Action == NotifyCollectionChangedAction.Add)
            {

                //PIDEditWindowsRichCanvasSelectedItem = PIDEditWindowSelectedItems.Count == 1 ? PIDEditWindowSelectedItems[0] : null;
                ShowSelectedItemToolbar = true;
                //if (LiveViewItems.Where(p => p.IsSelected).Any(p => p is Border))
                //{
                //    CanGroup = false;
                //}
                //else
                //{
                //    CanGroup = true;
                //}
            }


        }

        private void GetItemsForLiveView()
        {
            IsInIsolateMode = false;
            if (LiveViewItems == null)
            {
                LiveViewItems = new ObservableCollection<IDrawable>();
            }
            else
            {
                LiveViewItems.Clear();
            }
            LiveViewSelectedItems = new ObservableCollection<IDrawable>();

            IDrawable lastSelectedItem = null;
            foreach (var item in CurrentDevice.CurrentLiveViewZones)
            {
                (item as IDrawable).IsSelectable = true;
                LiveViewItems.Add(item as IDrawable);
                if ((item as IDrawable).IsSelected)
                    lastSelectedItem = item as IDrawable;

            }
            if (CurrentDevice.ControlZoneGroups != null)
            {
                foreach (var group in CurrentDevice.CurrentLiveViewGroup)
                {
                    //register group child to masked control 
                    var groupItems = GetGroupChildItems(group, CurrentDevice);
                    if (GetGroupBorder(groupItems, group) != null)
                    {
                        LiveViewItems.Add(group.Border);
                        if ((group.Border.IsSelected))
                            lastSelectedItem = group.Border;
                        //disable selection items in group
                        foreach (var item in groupItems)
                        {
                            (item as IDrawable).IsSelectable = false;
                        }

                    }
                    //switch (group.Type)
                    //{
                    //    case ControllerTypeEnum.LightingController:
                    //        RegisterGroupItem(groupItems.Cast<LEDSetup>().ToList(), group);
                    //        break;
                    //    case ControllerTypeEnum.PWMController:
                    //        RegisterGroupItem(groupItems.Cast<FanMotor>().ToList(), group);
                    //        break;
                    //}



                }
            }
            //reset toolbard width
            ToolBarWidth = 450;
            if (IsInIDEditStage)
            {
                GetThingsReadyFroIDEdit();
            }
            if (lastSelectedItem != null)
            {
                SelectLiveViewItemCommand.Execute(lastSelectedItem);
            }
            else
                SelectLiveViewItemCommand.Execute(LiveViewItems.Where(i => i.IsSelectable).First());


            ShowSelectedItemToolbar = true;

        }

        private void GetThingsReadyFroIDEdit()
        {
            switch (IdEditMode)
            {
                case IDMode.VID:
                    GetToolsForIDEdit();
                    GetBrushForIDEdit();
                    ToolBarWidth = 52;
                    break;
                case IDMode.FID:
                    ToolBarWidth = 450;
                    break;
            }

        }
        private ObservableCollection<IDrawable> _vIDEditWindowSelectedItems;
        public ObservableCollection<IDrawable> VIDEditWindowSelectedItems {
            get { return _vIDEditWindowSelectedItems; }
            set { _vIDEditWindowSelectedItems = value; RaisePropertyChanged(); }
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
            motion.GUID = Guid.NewGuid().ToString();
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
        private ARGBLEDSlaveDevice CurrentEditingDevice { get; set; }
        private void PIDEditSelectedItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {

            if (e.Action == NotifyCollectionChangedAction.Add)
            {

                //PIDEditWindowsRichCanvasSelectedItem = PIDEditWindowSelectedItems.Count == 1 ? PIDEditWindowSelectedItems[0] : null;
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {

                if (PIDEditWindowSelectedItems.Count == 0 || PIDEditWindowSelectedItems.Count > 1)
                {
                    PIDEditWindowsRichCanvasSelectedItem = null;
                }
            }

        }

        public ISlaveDevice CurrentEditingPIDItem { get; set; }
        private PIDQuickEditWindow pidQuickEditWindow { get; set; }
        private void OpenSpotPIDQuickEDitWindow(ObservableCollection<IDrawable> p)
        {
            CurrentIDType = "PID";
            CountPID = 0;
            CurrentEditingPIDItem = p.Where(p => p.IsSelected).FirstOrDefault() as ARGBLEDSlaveDevice;
            foreach (var zone in CurrentEditingPIDItem.ControlableZones)
            {
                (zone as LEDSetup).BackupSpots();
                (zone as LEDSetup).FillSpotsColor(Color.FromRgb(0, 0, 0));
            }
            pidQuickEditWindow = new PIDQuickEditWindow();
            pidQuickEditWindow.Owner = surfaceeditorWindow;
            pidQuickEditWindow.ShowDialog();

        }
        private void RotateSelectedSurfaceEditorItem(ObservableCollection<IDrawable> p)
        {
            CurrentEditingPIDItem = p.Where(p => p.IsSelected).FirstOrDefault() as ARGBLEDSlaveDevice;
            CurrentEditingPIDItem.RotateLEDSetup(90.0);

        }
        private void ReflectSelectedSurfaceEditorItem(ObservableCollection<IDrawable> p)
        {
            CurrentEditingPIDItem = p.Where(p => p.IsSelected).FirstOrDefault() as ARGBLEDSlaveDevice;
            CurrentEditingPIDItem.ReflectLEDSetupVertical();

        }
        private void SaveCurrentPID()
        {
            //check if any error
            var unfinishedZoneCount = 0;
            foreach (var zone in CurrentEditingPIDItem.ControlableZones)
            {

                if ((zone as LEDSetup).Spots.Any(s => !s.IsEnabled))
                {
                    unfinishedZoneCount++;
                }

            }
            if (unfinishedZoneCount > 0)
            {
                HandyControl.Controls.MessageBox.Show("Chưa set hết ID, vui lòng kiểm tra lại", "Spot ID error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            else
            {
                pidQuickEditWindow.Close();
            }

        }
        private void CancelCurrentPID()
        {
            foreach (var zone in CurrentEditingPIDItem.ControlableZones)
            {
                (zone as LEDSetup).RestoreSpots();
                (zone as LEDSetup).FillSpotsColor(Color.FromRgb(0, 0, 0));
                pidQuickEditWindow.Close();
            }
        }

        public bool IsAudioSelectionOpen { get; set; }
        private void OpenAudioSelectorWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"AudioDeviceSelectionWindow"}") is System.Windows.Window window)
            {
                IsAudioSelectionOpen = true;
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.Closing += (_, __) => IsAudioSelectionOpen = false;
                window.ShowDialog();
            }
        }
        private void LaunchPIDEditWindow(IDrawable p)
        {

            CurrentEditingDevice = p as ARGBLEDSlaveDevice;
            CurrentIDType = "PID";
            //Clone this slave device
            var temp = ObjectHelpers.Clone<ARGBLEDSlaveDevice>(p as ARGBLEDSlaveDevice);


            CountPID = 0;

            //add output border rect
            PIDEditWindowsRichCanvasItems = new ObservableCollection<IDrawable>();
            // PIDEditWindowsRichCanvasItems.CollectionChanged += PIDEditItemsChanged;
            //ad zone border
            PIDEditWindowSelectedItems = new ObservableCollection<IDrawable>();
            PIDEditWindowSelectedItems.CollectionChanged += PIDEditSelectedItemsChanged;
            // PIDEditWindowsRichCanvasSelectedItem = null;
            foreach (var zone in temp.ControlableZones)
            {

                var ledSetup = zone as LEDSetup;
                //deselect any selected zone
                ledSetup.IsSelected = false;
                ledSetup.Left = ledSetup.GetRect.Left;
                ledSetup.Top = ledSetup.GetRect.Top;
                PIDEditWindowsRichCanvasItems.Add(ledSetup);



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
                    PIDEditWindowsRichCanvasItems.Insert(0, screen);
                }
            }
            else
            {
                ScreenBound screen = new ScreenBound();
                PIDEditWindowsRichCanvasItems.Insert(0, screen);
            }





            PathGuide pathGuide = new PathGuide() {
                Width = 200,
                Height = 200,
                IsDraggable = true,
                IsSelectable = true,
                IsResizeable = false,
                Geometry = "AmbinoA1Guide"
            };

            surfaceeditorWindow.Close();
            IsRichCanvasWindowOpen = true;
            pidEditCanvasWindow = new PIDEditCanvasWindow();
            pidEditCanvasWindow.ShowDialog();

        }

        #region color and palette edit properties
        /// <summary>
        /// contains list of color that user can edit
        /// </summary>
        private ObservableCollection<ColorEditingObject> _colorList;
        public ObservableCollection<ColorEditingObject> ColorList {
            get { return _colorList; }
            set
            {
                _colorList = value;
                RaisePropertyChanged();
            }
        }
        /// <summary>
        /// this property store current color picked by hc:colorpicker
        /// </summary>
        private Color _currentPickedColor;
        public Color CurrentPickedColor {
            get => _currentPickedColor;
            set
            {
                Set(ref _currentPickedColor, value);
                foreach (var item in ColorList)
                {
                    if (item.IsSelected)
                    {
                        item.Color = value;
                    }
                }
                RaisePropertyChanged();
            }
        }
        /// <summary>
        /// this property store current selected color on the color list
        /// </summary>
        private ColorEditingObject _currentSelectedColor;
        public ColorEditingObject CurrentSelectedColor {
            get { return _currentSelectedColor; }
            set
            {
                _currentSelectedColor = value;
                if (CurrentPickedColor != null)
                    CurrentPickedColor = value != null ? value.Color : Color.FromRgb(255, 255, 255);
                RaisePropertyChanged();
            }
        }
        private SelectionMode _colorListBoxSelectionMode = SelectionMode.Single;
        public SelectionMode ColorListBoxSelectionMode {
            get { return _colorListBoxSelectionMode; }
            set
            {
                _colorListBoxSelectionMode = value;
                RaisePropertyChanged();
            }
        }
        private bool _isSolidMode;
        public bool IsSolidMode {
            get { return _isSolidMode; }
            set
            {
                _isSolidMode = value;
                RaisePropertyChanged();
                if (value)
                {
                    ColorListBoxSelectionMode = SelectionMode.Multiple;
                    foreach (var color in ColorList)
                    {
                        color.IsSelected = true;
                        color.Color = Color.FromRgb(255, 0, 0);
                    }
                }
                else
                {
                    ColorListBoxSelectionMode = SelectionMode.Single;
                    foreach (var color in ColorList)
                    {
                        color.IsSelected = false;
                    }
                    ColorList[0].IsSelected = true;
                }
            }
        }
        #endregion
        private void OpenColorPickerWindow(int numColor)
        {
            ColorList = new ObservableCollection<ColorEditingObject>();
            var currentColorControl = SelectedControlZone.CurrentActiveControlMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.Color).FirstOrDefault() as ListSelectionParameter;
            var currentSelectedColorCard = currentColorControl.SelectedValue as ColorCard;
            ColorList.Add(new ColorEditingObject() { Color = currentSelectedColorCard.StartColor });
            ColorList.Add(new ColorEditingObject() { Color = currentSelectedColorCard.StopColor });
            ColorListBoxSelectionMode = SelectionMode.Single;
            foreach (var color in ColorList)
            {
                color.IsSelected = true;
                color.Color = ColorList[0].Color;
            }
            if (AssemblyHelper.CreateInternalInstance($"View.{"ColorPickerWindow"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }
        private void ImportPaletteFromFile()
        {
            var palette = LocalFileHlprs.OpenImportFileDialog<ColorPalette>("col", "col files (*.col)|*.Col|Text files (*.txt)|*.Txt");
            if (palette != null)
            {
                AddNewPaletteToCollection(palette);
            }
        }
        private void OpenPaletteEditorWindow(int numColor)
        {
            ColorList = new ObservableCollection<ColorEditingObject>();
            ColorListBoxSelectionMode = SelectionMode.Extended;
            var currentColorControl = SelectedControlZone.CurrentActiveControlMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.Palette).FirstOrDefault() as ListSelectionParameter;
            var currentSelectedPalette = currentColorControl.SelectedValue as ColorPalette;
            foreach (var color in currentSelectedPalette.Colors)
            {
                ColorList.Add(new ColorEditingObject() { Color = color });
            }

            if (AssemblyHelper.CreateInternalInstance($"View.{"ColorPickerWindow"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }
        private void AddNewColorToCollection()
        {
            var newColor = new ColorCard();
            newColor.StartColor = ColorList[0].Color;
            newColor.StopColor = ColorList[1].Color;
            //read current collection '
            var currentParam = SelectedControlZone.CurrentActiveControlMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.Color).FirstOrDefault() as ListSelectionParameter;
            currentParam.AddItemToCollection(newColor);
            //var currentCollectionPath = currentParam.SelectedValueLocalPath.Path;
            //var currentCollection = currentParam.AvailableValue;
            //currentCollection.Insert(0, newColor);
            //WriteSimpleJson(currentCollection, Path.Combine(currentCollectionPath, "collection.json"));
            //currentParam.RefreshCollection();
            //SelectedControlZone.CurrentActiveControlMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.Color).FirstOrDefault().Value = 0;
        }
        private void AddNewPaletteToCollection(ColorPalette source)
        {
            //read current collection
            var currentParam = SelectedControlZone.CurrentActiveControlMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.Palette).FirstOrDefault() as ListSelectionParameter;
            currentParam.AddItemToCollection(source);
            //var currentCollectionPath = currentParam.SelectedValueLocalPath.Path;
            //var currentCollection = currentParam.AvailableValue;
            //if (currentCollection.Any(p => p.Name == source.Name))
            //{
            //    HandyControl.Controls.MessageBox.Show("File đã tồn tại, " + source.Name + "vui lòng chọn tên khác!", "file already exists", MessageBoxButton.OK, MessageBoxImage.Warning);
            //    return;
            //}
            //currentCollection.Insert(0, source);
            //var collectionFolderPath = Path.Combine(currentCollectionPath, "collection");
            //WriteSimpleJson(source, Path.Combine(collectionFolderPath, source.Name + ".col"));
            //currentParam.RefreshCollection();
            //SelectedControlZone.CurrentActiveControlMode.Parameters.Where(p => p.ParamType == ModeParameterEnum.Palette).FirstOrDefault().Value = 0;
            AddNewPaletteDialog?.Close();
        }
        private void WriteSimpleJson(object obj, string path)
        {
            try
            {
                var json = JsonConvert.SerializeObject(obj, new JsonSerializerSettings() {
                    TypeNameHandling = TypeNameHandling.Auto
                });
                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                //log
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
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<IDrawable> SurfaceEditorSelectedItems { get; set; }



        private bool _canSelectMultipleItems;
        public bool CanSelectMultipleItems {
            get { return _canSelectMultipleItems; }
            set
            {
                _canSelectMultipleItems = value;
                RaisePropertyChanged(nameof(CanSelectMultipleItems));
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
                //find centerPoint
                Point center = new Point(item.CenterX, item.CenterY);
                //scale
                if (item.SetScale(ItemScaleValue, ItemScaleValue, false))
                {
                    //move back to center
                    item.Left = center.X - item.Width / 2;
                    item.Top = center.Y - item.Height / 2;
                }
                else
                {
                    HandyControl.Controls.MessageBox.Show("kích thước scale quá nhỏ", "Zone size too small", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

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
                item.SetScale(1.0, 1.0, true);
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
            public Geometry Geometry { get; set; }
            public string Name { get; set; }
        }
        public DrawableShape SelectedShape { get; set; }
        public double NewItemWidth { get; set; }
        public double NewItemHeight { get; set; }
        public int ItemNumber { get; set; }
        private ObservableCollection<DrawableShape> _availableShapeToAdd;
        public ObservableCollection<DrawableShape> AvailableShapeToAdd {
            get { return _availableShapeToAdd; }
            set
            {
                _availableShapeToAdd = value;
                RaisePropertyChanged();
            }
        }
        private void OpenAddNewItemWindow()
        {
            AvailableShapeToAdd = new ObservableCollection<DrawableShape>();
            var circleShape = new DrawableShape() {
                Geometry = Geometry.Parse("M100 50C100 77.6142 77.6142 100 50 100C22.3858 100 0 77.6142 0 50C0 22.3858 22.3858 0 50 0C77.6142 0 100 22.3858 100 50Z"),
                Name = "Round"
            };
            var squareShape = new DrawableShape() {
                Geometry = Geometry.Parse("M0 0H100V100H0V0Z"),
                Name = "Square"
            };
            AvailableShapeToAdd.Add(squareShape);
            AvailableShapeToAdd.Add(circleShape);
            //AvailableShapeToAdd.Add(ambinoA1Inner);
            //AvailableShapeToAdd.Add(ambinoA1Outer);
            if (AssemblyHelper.CreateInternalInstance($"View.{"NewItemParametersWindow"}") is System.Windows.Window window)
            {

                window.Owner = pidEditCanvasWindow;
                window.ShowDialog();
            }


        }
        private Point _mousePosition;
        public Point MousePosition {
            get { return _mousePosition; }
            set
            {
                _mousePosition = value; RaisePropertyChanged();
                if (IsInIDEditStage)
                {
                    if (SelectedToolIndex == 1)
                    {
                        IDEditBrush.Left = MousePosition.X - IDEditBrush.Width / 2;
                        IDEditBrush.Top = MousePosition.Y - IDEditBrush.Height / 2;
                    }
                    else if (SelectedToolIndex == 2)
                    {
                        EraserBrush.Left = MousePosition.X - EraserBrush.Width / 2;
                        EraserBrush.Top = MousePosition.Y - EraserBrush.Height / 2;
                    }

                }
            }
        }
        #region pid canvas viewmodel
        private void AddSpotLayout()
        {
            System.Windows.Forms.OpenFileDialog Import = new System.Windows.Forms.OpenFileDialog();
            Import.Title = "Chọn GeometryGroup files";
            Import.CheckFileExists = true;
            Import.CheckPathExists = true;
            Import.DefaultExt = "Pro";
            Import.Filter = "Text files (*.txt)|*.TXT";
            Import.FilterIndex = 2;
            Import.Multiselect = false;

            Import.ShowDialog();

            var text = System.IO.File.ReadAllText(Import.FileName);
            try
            {
                StringReader sr = new StringReader(text);

                XmlReader reader = XmlReader.Create(sr);

                GeometryGroup gr = (GeometryGroup)XamlReader.Load(reader);


                var lastSpotID = PIDEditWindowsRichCanvasItems.Where(s => s is DeviceSpot).Count();
                var newItems = new ObservableCollection<IDrawable>();
                for (int i = 0; i < gr.Children.Count; i++)
                {
                    var newItem = new DeviceSpot(
                        gr.Children[i].Bounds.Top,
                        gr.Children[i].Bounds.Left,
                        gr.Children[i].Bounds.Width,
                        gr.Children[i].Bounds.Height,
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
                        gr.Children[i]);
                    newItem.IsSelected = true;
                    newItem.IsDeleteable = true;
                    newItems.Add(newItem);

                }
                foreach (var item in newItems)
                {
                    PIDEditWindowsRichCanvasItems.Add(item);
                    //CurrentOutput.OutputLEDSetup.Spots.Add(item as DeviceSpot);
                }
            }
            catch (Exception ex)
            {

            }

        }
        private void AddSpotGeometry()
        {
            System.Windows.Forms.OpenFileDialog Import = new System.Windows.Forms.OpenFileDialog();
            Import.Title = "Chọn GeometryGroup files";
            Import.CheckFileExists = true;
            Import.CheckPathExists = true;
            Import.DefaultExt = "Pro";
            Import.Filter = "Text files (*.txt)|*.TXT";
            Import.FilterIndex = 2;
            Import.Multiselect = false;

            Import.ShowDialog();

            var text = System.IO.File.ReadAllText(Import.FileName);
            try
            {
                StringReader sr = new StringReader(text);

                XmlReader reader = XmlReader.Create(sr);

                Geometry geo = (Geometry)XamlReader.Load(reader);


                var lastSpotID = PIDEditWindowsRichCanvasItems.Where(s => s is DeviceSpot).Count();
                var newItem = new DrawableShape() {
                    Geometry = geo,
                };
                AvailableShapeToAdd.Add(newItem);
                
            }
            catch (Exception ex)
            {

            }

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
                Bitmap bitmap = (Bitmap)Bitmap.FromFile(addImage.FileName, true);               
                var image = new ImageVisual {
                    Top = MousePosition.Y,
                    Left = MousePosition.X,
                    ImagePath = addImage.FileName,
                    Height = bitmap.Height,
                    Width = bitmap.Width,
                    IsResizeable = true,
                    IsDeleteable = true
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
            var itemToRemove = PIDEditWindowsRichCanvasItems.Where(s => s is DeviceSpot || s is LEDSetup).ToList();
            PIDEditWindowSelectedItems.Clear();
            itemToRemove.ForEach(item => PIDEditWindowsRichCanvasItems.Remove(item));

        }
        private void LockSelectedItem(ObservableCollection<IDrawable> itemSource)
        {

            foreach (var item in itemSource.OfType<IDrawable>().Where(d => d.IsSelected))
            {
                item.IsDraggable = false;
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
        #endregion
        private DrawableHelpers DrawableHlprs { get; set; }
        private ControlModeHelpers CtrlHlprs { get; set; }
        private void SaveCurrentLEDSetupLayout(string mode)
        {

            if (mode == "save")
            {
                if (DrawableHlprs == null)
                    DrawableHlprs = new DrawableHelpers();
                if (CtrlHlprs == null)
                    CtrlHlprs = new ControlModeHelpers();

                var usableItems = PIDEditWindowsRichCanvasItems.OfType<IDrawable>().Where(d => d is DeviceSpot || d is LEDSetup);
                if (usableItems.Count() == 0)
                {
                    HandyControl.Controls.MessageBox.Show("Bạn phải thêm ít nhất 1 LED", "Invalid LED number", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                //check if there is any zone exist
                if (PIDEditWindowsRichCanvasItems.Where(d => d is LEDSetup).Count() == 0)//no zone at all so we add all device spot in to single zone
                {
                    //select all spot
                    var availableSpot = PIDEditWindowsRichCanvasItems.Where(s => s is DeviceSpot).ToList();
                    availableSpot.ForEach(s => s.IsSelected = true);

                    //add single zone grab all available spot to canvas
                    AddNewZone(PIDEditWindowsRichCanvasItems, PIDEditWindowSelectedItems);

                }
                var usableZone = PIDEditWindowsRichCanvasItems.Where(z => z is LEDSetup).ToList();
                //now update zone size by its own child
                foreach (var zone in usableZone)
                {
                    //get left and top also, set true
                    var ledSetup = zone as LEDSetup;
                }
                //get new boundRect of all usable zone
                var newBoundRect = DrawableHlprs.GetBound(usableZone);

                //get current screen size
                var screen = PIDEditWindowsRichCanvasItems.OfType<IDrawable>().Where(d => d is ScreenBound && !d.IsResizeable).FirstOrDefault();
                var screenRect = new Rectangle((int)screen.Left, (int)screen.Top, (int)screen.Width, (int)screen.Height);


                foreach (var zone in usableZone)
                {
                    //reset origin for each zone
                    var ledSetup = zone as LEDSetup;
                    ledSetup.Left -= newBoundRect.Left;
                    ledSetup.Top -= newBoundRect.Top;
                    ledSetup.ZoneUID = Guid.NewGuid().ToString();
                    //make this zone controlable
                    CtrlHlprs.MakeZoneControlable(ledSetup);

                }

                CurrentEditingDevice.ControlableZones.Clear();
                usableZone.ForEach(zone => CurrentEditingDevice.ControlableZones.Add(zone as LEDSetup));
                CurrentEditingDevice.UpdateSizeByChild(true);
                CurrentEditingDevice.Left = newBoundRect.Left;
                CurrentEditingDevice.Top = newBoundRect.Top;
            }

            if (IsLiveViewOpen)
                UpdateLiveView();
            pidEditCanvasWindow.Close();
            OpenSurfaceEditorWindow();

        }
        private void SaveCurrentSurfaceLayout()
        {
            var screens = SurfaceEditorItems.OfType<IDrawable>().Where(d => d is ScreenBound);
            foreach (var item in SurfaceEditorItems.Where(i => i is ARGBLEDSlaveDevice))
            {
                var ledDevice = item as ARGBLEDSlaveDevice;

                foreach (var zone in ledDevice.ControlableZones)
                {

                    var ledZone = zone as LEDSetup;
                    ledZone.OffsetX = ledDevice.Left;
                    ledZone.OffsetY = ledDevice.Top;

                }
                item.IsSelected = false;

            }

            if (IsLiveViewOpen)
            {
                GetItemsForLiveView();
                UpdateLiveView();
            }

            surfaceeditorWindow.Close();
            IsRichCanvasWindowOpen = false;
        }
        private void ResetPID()
        {
            CountPID = 0;
            foreach (var zone in CurrentEditingPIDItem.ControlableZones)
            {
                var ledZone = zone as LEDSetup;
                foreach (var spot in ledZone.Spots)
                {
                    spot.SetColor(0, 0, 0, true);
                    spot.IsEnabled = false;
                    spot.SetID(0);

                }
                // ledZone.ReorderSpots();
            }
        }

        private void SetZoneMID(int id)
        {
            var selectedZone = SelectedControlZone as LEDSetup;
            foreach (var spot in selectedZone.Spots)
            {
                spot.SetMID(id);
            }
        }
        private void SetSpotID(IDeviceSpot spot)
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
                    if (spot.IsEnabled)
                    {

                        spot.SetColor(0, 0, 0, true);
                        spot.IsEnabled = false;
                        if (CountPID >= 1)
                        {
                            CountPID -= 1;
                        }
                        else
                            CountPID = 0;
                    }
                    else
                    {
                        spot.SetID(CountPID++);
                        spot.IsEnabled = true;
                        spot.SetColor(0, 0, 255, true);

                    }
                    //foreach(var zone in CurrentEditingPIDItem.ControlableZones)
                    //{
                    //    (zone as LEDSetup).ReorderSpots();
                    //}
                    break;

            }
        }


        private void OpenSurfaceEditorWindow()
        {
            SurfaceEditorItems = new ObservableCollection<IDrawable>();
            SurfaceEditorSelectedItems = new ObservableCollection<IDrawable>();
            foreach (var device in AvailableDevices)
            {
                var slaveDevices = device.AvailableLightingDevices;
                foreach (var slaveDevice in slaveDevices)
                {
                    SurfaceEditorItems.Add(slaveDevice as ARGBLEDSlaveDevice);
                }
            }
            //add virtual borders
            Desktops = new ObservableCollection<WriteableBitmap>();
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
                    screen.ShouldBringIntoView = true;
                    var source = new WriteableBitmap((int)(screen.Width / 8.0), (int)(screen.Height / 8.0), 96, 96, PixelFormats.Bgra32, null);
                    Desktops.Add(source);
                    //screen.Source = Desktops[i];
                    SurfaceEditorItems.Insert(0, screen);
                }
            }
            else
            {
                ScreenBound screen = new ScreenBound();
                SurfaceEditorItems.Insert(0, screen);
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
        private void ExportCurrentSelectedPaletteToFile(ColorPalette palette)
        {
            //SaveFileDialog Export = new SaveFileDialog();
            //Export.CreatePrompt = true;
            //Export.OverwritePrompt = true;

            //Export.Title = "Xuất dữ liệu";
            //Export.FileName = palette.Name + " Color Palette";
            //Export.CheckFileExists = false;
            //Export.CheckPathExists = true;
            //Export.InitialDirectory =
            //Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            //Export.RestoreDirectory = true;

            //var paletteJson = JsonConvert.SerializeObject(palette, new JsonSerializerSettings() {
            //    TypeNameHandling = TypeNameHandling.Auto
            //});

            //if (Export.ShowDialog() == DialogResult.OK)
            //{
            //    //create directory with same name
            //    var newFolder = Directory.CreateDirectory(Export.FileName);
            //    var contentFolder = Directory.CreateDirectory(Path.Combine(Export.FileName, "content")).ToString();
            //    //create main content 
            //    File.WriteAllText(Path.Combine(Export.FileName, "content", palette.Name + ".col"), paletteJson);
            //    //create info
            //    var info = new OnlineItemModel() {
            //        Name = palette.Name,
            //        Owner = palette.Owner,
            //        Description = palette.Description,
            //        Type = "Palette",
            //        TargetDevices =  newnew DeviceTypeDataEnum("Others", DeviceTypeEnum.Unknown);
            //    };
            //    var infoJson = JsonConvert.SerializeObject(info, new JsonSerializerSettings() {
            //        TypeNameHandling = TypeNameHandling.Auto
            //    });
            //    File.WriteAllText(Path.Combine(Export.FileName, "info.json"), infoJson);
            //    //create image, require user input later thumb.png???


            //}

        }

        private ObservableCollection<ISlaveDevice> _availableARGBSlaveDevices;
        public ObservableCollection<ISlaveDevice> AvailableARGBSlaveDevices {
            get { return _availableARGBSlaveDevices; }
            set
            {
                _availableARGBSlaveDevices = value;
                RaisePropertyChanged();
            }
        }
        private SlaveDeviceCollectionView SlaveDeviceSelection { get; set; }
        public IOutputSettings CurrentSelectedOutputMap { get; set; }
        private void OpenSlaveDeviceSelector(OutputSettings o)
        {
            //get items ready
            //load local folder
            CurrentSelectedOutputMap = o;
            string[] existedSlaveDeviceFolder = Directory.GetDirectories(SupportedDeviceCollectionFolderPath);
            AvailableARGBSlaveDevices = new ObservableCollection<ISlaveDevice>();
            foreach (var deviceFolder in existedSlaveDeviceFolder)
            {
                if (File.Exists(Path.Combine(deviceFolder, "config.json")))
                {
                    var json = File.ReadAllText(Path.Combine(deviceFolder, "config.json"));
                    var existedDevice = JsonConvert.DeserializeObject<ARGBLEDSlaveDevice>(json, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
                    // existedDevice.Thumbnail = Path.Combine(deviceFolder, "thumbnail.png");
                    AvailableARGBSlaveDevices.Add(existedDevice);
                }

            }

            //open dialog
            SlaveDeviceSelection = new SlaveDeviceCollectionView();
            SlaveDeviceSelection.Owner = OOTBWindows;
            SlaveDeviceSelection.Show();


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
            AutomationSettings newAutomation = new AutomationSettings { Name = name };

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
            //if (AmbinityClient != null)
            //    AmbinityClient.Dispose();
            System.Windows.Forms.Application.Restart();
            Process.GetCurrentProcess().Kill();
        }

        private void DeleteSelectedAutomation(AutomationSettings automation)
        {
            AvailableAutomations.Remove(automation);
            WriteAutomationCollectionJson();
        }
        private FTPServerHelpers FTPHlprs { get; set; }



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
        private AddNewPaletteWindow AddNewPaletteDialog;
        public void OpenCreateNewPaletteDialog()
        {
            AddNewPaletteDialog = new AddNewPaletteWindow();
            AddNewPaletteDialog.Owner = System.Windows.Application.Current.MainWindow;
            AddNewPaletteDialog.ShowDialog();

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
        private ObservableCollection<IDrawable> _ootbItems;
        public ObservableCollection<IDrawable> OOTBItems {
            get { return _ootbItems; }
            set { _ootbItems = value; RaisePropertyChanged(); }
        }
        public ObservableCollection<IDrawable> OOTBSelectedItems { get; set; }
        public IDrawable OOTBSelectedItem { get; set; }
        public void OpenLEDSteupSelectionWindows()
        {
            GetItemReadyForOOTB();
            OOTBWindows = new OOTBWindow();
            OOTBWindows.Owner = System.Windows.Application.Current.MainWindow;
            OOTBWindows.ShowDialog();
        }

        private void GetItemReadyForOOTB()
        {
            OOTBItems = new ObservableCollection<IDrawable>();
            OOTBSelectedItems = new ObservableCollection<IDrawable>();
            //get image ready
            var outputMapImage = new ImageVisual() {
                Name = "Output Map",
                ImagePath = CurrentDevice.DeviceOutputMap,
                Width = 500,
                Height = 500,
                Left = 0,
                Top = 0,
                IsDraggable = false,
                IsSelectable = false,
                IsResizeable = false
            };
            OOTBItems.Add(outputMapImage);
            //output map
            foreach (var output in CurrentDevice.AvailableLightingOutputs)
            {
                OOTBItems.Add(output as OutputSettings);
            }
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
        private void CreateColorCollectionFolder()
        {
            if (!Directory.Exists(ColorsCollectionFolderPath))
            {
                Directory.CreateDirectory(ColorsCollectionFolderPath);
                //get data from resource file and copy to local folder
                var colorCollectionResourcePath = "adrilight.Resources.Colors.ColorCollection.json";
                ResourceHlprs.CopyResource(colorCollectionResourcePath, Path.Combine(ColorsCollectionFolderPath, "collection.json"));
                //Create deserialize config
                var config = new ResourceLoaderConfig(nameof(ColorCard), DeserializeMethodEnum.SingleJson);
                var configJson = JsonConvert.SerializeObject(config, new JsonSerializerSettings() {
                    TypeNameHandling = TypeNameHandling.Auto
                });
                File.WriteAllText(Path.Combine(ColorsCollectionFolderPath, "config.json"), configJson);
            }
            //deserialize and store colorcollection

        }
        private void CreateVIDCollectionFolder()
        {
            if (!Directory.Exists(VIDCollectionFolderPath))
            {
                Directory.CreateDirectory(VIDCollectionFolderPath);
                var collectionFolder = Path.Combine(VIDCollectionFolderPath, "collection");
                Directory.CreateDirectory(collectionFolder);
                var vidCollection = new List<VIDDataModel>();
                var lef2Right = new VIDDataModel() {
                    Name = "Trái sang phải",
                    Description = "Màu chạy từ trái sang phải",
                    ExecutionType = VIDType.PositonGeneratedID,
                    Dirrection = VIDDirrection.left2right
                };
                var right2Left = new VIDDataModel() {
                    Name = "Phải sang trái",
                    Description = "Màu chạy từ phải sang trái",
                    ExecutionType = VIDType.PositonGeneratedID,
                    Dirrection = VIDDirrection.right2left
                };
                var up2Down = new VIDDataModel() {
                    Name = "Trên xuống dưới",
                    Description = "Màu chạy từ trên xuống dưới",
                    ExecutionType = VIDType.PositonGeneratedID,
                    Dirrection = VIDDirrection.top2bot
                };
                var down2Up = new VIDDataModel() {
                    Name = "Dưới lên trên",
                    Description = "Màu chạy từ dưới lên trên",
                    ExecutionType = VIDType.PositonGeneratedID,
                    Dirrection = VIDDirrection.bot2top
                };
                var linear = new VIDDataModel() {
                    Name = "Tuyến tính",
                    Description = "Màu chạy theo thứ tự LED",
                    ExecutionType = VIDType.PositonGeneratedID,
                    Dirrection = VIDDirrection.linear
                };
                var custom = new VIDDataModel() {
                    Name = "Custom",
                    Description = "Drawing Custom Path",
                    ExecutionType = VIDType.PredefinedID
                };
                vidCollection.Add(lef2Right);
                vidCollection.Add(right2Left);
                vidCollection.Add(up2Down);
                vidCollection.Add(down2Up);
                vidCollection.Add(custom);
                vidCollection.Add(linear);
                foreach (var vid in vidCollection)
                {
                    var json = JsonConvert.SerializeObject(vid, new JsonSerializerSettings() {
                        TypeNameHandling = TypeNameHandling.Auto
                    });
                    File.WriteAllText(Path.Combine(collectionFolder, vid.Name + ".json"), json);
                }
                //coppy all internal palettes to local 
                var config = new ResourceLoaderConfig(nameof(VIDDataModel), DeserializeMethodEnum.MultiJson);
                var configJson = JsonConvert.SerializeObject(config, new JsonSerializerSettings() {
                    TypeNameHandling = TypeNameHandling.Auto
                });
                File.WriteAllText(Path.Combine(VIDCollectionFolderPath, "config.json"), configJson);
            }
            //deserialize and store colorcollection

        }

        private void CreateMIDCollectionFolder()
        {
            if (!Directory.Exists(MIDCollectionFolderPath))
            {
                Directory.CreateDirectory(MIDCollectionFolderPath);
                var collectionFolder = Path.Combine(MIDCollectionFolderPath, "collection");
                Directory.CreateDirectory(collectionFolder);
                var midCollection = new List<MIDDataModel>();
                var low = new MIDDataModel() {
                    Name = "Low",
                    Description = "Trung bình của các tần số thấp (Bass)",
                    ExecutionType = VIDType.PositonGeneratedID,
                    Frequency = MIDFrequency.Low
                };
                var middle = new MIDDataModel() {
                    Name = "Mid",
                    Description = "Trung bình của các tần số trung (Mid)",
                    ExecutionType = VIDType.PositonGeneratedID,
                    Frequency = MIDFrequency.Middle
                };
                var high = new MIDDataModel() {
                    Name = "High",
                    Description = "Trung bình của các tần số cao (Treble)",
                    ExecutionType = VIDType.PositonGeneratedID,
                    Frequency = MIDFrequency.High
                };

                var custom = new MIDDataModel() {
                    Name = "Custom",
                    Description = "Chọn Tần số bằng tay",
                    ExecutionType = VIDType.PredefinedID
                };
                midCollection.Add(low);
                midCollection.Add(middle);
                midCollection.Add(high);
                midCollection.Add(custom);
                foreach (var mid in midCollection)
                {
                    var json = JsonConvert.SerializeObject(mid, new JsonSerializerSettings() {
                        TypeNameHandling = TypeNameHandling.Auto
                    });
                    File.WriteAllText(Path.Combine(collectionFolder, mid.Name + ".json"), json);
                }
                //coppy all internal palettes to local 
                var config = new ResourceLoaderConfig(nameof(MIDDataModel), DeserializeMethodEnum.MultiJson);
                var configJson = JsonConvert.SerializeObject(config, new JsonSerializerSettings() {
                    TypeNameHandling = TypeNameHandling.Auto
                });
                File.WriteAllText(Path.Combine(MIDCollectionFolderPath, "config.json"), configJson);
            }
            //deserialize and store colorcollection

        }
        private void CreateRequiredFwVersionJson()
        {
            var ABR1p = new DeviceFirmware() {
                Name = "ABR1p.hex",
                Version = "1.0.6",
                TargetHardware = "ABR1p",
                TargetDeviceType = DeviceTypeEnum.AmbinoBasic,
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
            var ABR2e = new DeviceFirmware() {
                Name = "ABR2e.hex",
                Version = "1.0.6",
                TargetHardware = "ABR2e",
                TargetDeviceType = DeviceTypeEnum.AmbinoBasic,
                Geometry = "binary",
                ResourceName = "adrilight.DeviceFirmware.ABR2e.hex"
            };
            var AER1e = new DeviceFirmware() {
                Name = "AER1e.hex",
                Version = "1.0.4",
                TargetHardware = "AER1e",
                TargetDeviceType = DeviceTypeEnum.AmbinoEDGE,
                Geometry = "binary",
                ResourceName = "adrilight.DeviceFirmware.AER1e.hex"
            };
            var AER2e = new DeviceFirmware() {
                Name = "AER2e.hex",
                Version = "1.0.4",
                TargetHardware = "AER2e",
                TargetDeviceType = DeviceTypeEnum.AmbinoEDGE,
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
            var AFR1g = new DeviceFirmware() {
                Name = "AFR1g.hex",
                Version = "1.0.3",
                TargetHardware = "AFR1g",
                TargetDeviceType = DeviceTypeEnum.AmbinoFanHub,
                Geometry = "binary",
                ResourceName = "adrilight.DeviceFirmware.AFR1g.hex"
            };
            var AFR2g = new DeviceFirmware() {
                Name = "AFR2g.hex",
                Version = "1.0.5",
                TargetHardware = "AFR2g",
                TargetDeviceType = DeviceTypeEnum.AmbinoFanHub,
                Geometry = "binary",
                ResourceName = "adrilight.DeviceFirmware.AFR2g.hex"
            };
            var AFR3g = new DeviceFirmware() {
                Name = "AFR3g.hex",
                Version = "1.0.6",
                TargetHardware = "AFR3g",
                TargetDeviceType = DeviceTypeEnum.AmbinoFanHub,
                Geometry = "binary",
                ResourceName = "adrilight.DeviceFirmware.AFR3g.hex"
            };
            var AHR2g = new DeviceFirmware() {
                Name = "AHR2g.hex",
                Version = "1.0.1",
                TargetHardware = "AHR1g",
                TargetDeviceType = DeviceTypeEnum.AmbinoHUBV3,
                Geometry = "binary",
                ResourceName = "adrilight.DeviceFirmware.AHR2g.hex"
            };
            var ARR1p = new DeviceFirmware() {
                Name = "ARR1p.hex",
                Version = "1.0.2",
                TargetHardware = "ARR1p",
                TargetDeviceType = DeviceTypeEnum.AmbinoRainPowPro,
                Geometry = "binary",
                ResourceName = "adrilight.DeviceFirmware.ARR1p.hex"
            };
            var firmwareList = new List<DeviceFirmware>();
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

        private void LoadAvailableAnimations()
        {
            AvailableGifs = new ObservableCollection<IGifCard>();
            foreach (var loadedGif in LoadGifIfExist())
            {
                AvailableGifs.Add(loadedGif);
            }
            WriteGifCollectionJson();
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
            AvailableProfiles = new ObservableCollection<AppProfile>();
            foreach (var profile in LoadAppProfileIfExist())
            {
                AvailableProfiles.Add(profile);
            }
            // WriteDeviceProfileCollection();
        }

        public void LoadData()
        {
            if (ResourceHlprs == null)
                ResourceHlprs = new ResourceHelpers();
            if (LocalFileHlprs == null)
                LocalFileHlprs = new LocalFileHelpers();
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
            SearchingForDevices = true;
            #region checking and creating resource folder path if not exist
            CreateColorCollectionFolder();
            CreatePaletteCollectionFolder();
            CreateChasingPatternCollectionFolder();
            CreateVIDCollectionFolder();
            CreateMIDCollectionFolder();
            CreateAudioDevicesCollection();
            CreateProfileCollectionFolder();
            CreateSupportedDevicesCollectionFolder();
            #endregion

            LoadAvailableLightingMode();
            LoadAvailableAnimations();
            LoadAvailableBaudRate();
            LoadAvailableChasingPatterns();
            LoadAvailableProfiles();
            LoadAvailableSolidColors();


        }
        private void CreateProfileCollectionFolder()
        {
            if (!Directory.Exists(ProfileCollectionFolderPath))
                Directory.CreateDirectory(ProfileCollectionFolderPath);
            var collectionFolder = Path.Combine(ProfileCollectionFolderPath, "collection");
            Directory.CreateDirectory(collectionFolder);
            var config = new ResourceLoaderConfig(nameof(AppProfile), DeserializeMethodEnum.MultiJson);
            var configJson = JsonConvert.SerializeObject(config, new JsonSerializerSettings() {
                TypeNameHandling = TypeNameHandling.Auto
            });
            File.WriteAllText(Path.Combine(ProfileCollectionFolderPath, "config.json"), configJson);

        }
        private void CreateAudioDevicesCollection()
        {
            AvailableAudioDevices = new ObservableCollection<AudioDevice>();
            foreach (var device in GetAvailableAudioDevices())
            {
                AvailableAudioDevices.Add(device);
            }
        }
        public void CreateChasingPatternCollectionFolder()
        {
            if (!Directory.Exists(ChasingPatternsCollectionFolderPath))
            {
                Directory.CreateDirectory(ChasingPatternsCollectionFolderPath);
                var collectionFolder = Path.Combine(ChasingPatternsCollectionFolderPath, "collection");
                Directory.CreateDirectory(collectionFolder);
                //var allResourceNames = "adrilight.Resources.Colors.ChasingPatterns.json";
                var allResourceNames = ResourceHlprs.GetResourceFileName();
                foreach (var resourceName in allResourceNames.Where(r => r.EndsWith(".AML")))
                {
                    ResourceHlprs.CopyResource(resourceName, Path.Combine(collectionFolder, resourceName));
                }
                var config = new ResourceLoaderConfig(nameof(ChasingPattern), DeserializeMethodEnum.MultiJson);
                var configJson = JsonConvert.SerializeObject(config, new JsonSerializerSettings() {
                    TypeNameHandling = TypeNameHandling.Auto
                });
                File.WriteAllText(Path.Combine(ChasingPatternsCollectionFolderPath, "config.json"), configJson);
                //copy default chasing pattern from resource

            }
        }
        public void CreateSupportedDevicesCollectionFolder()
        {
            if (!Directory.Exists(SupportedDeviceCollectionFolderPath))
            {
                Directory.CreateDirectory(SupportedDeviceCollectionFolderPath);
                //create generic device
                var undefinedThumbnailResourcePath = "adrilight.Resources.Thumbnails.Undefined_thumb.png";
                var genericDeviceDirectory = Path.Combine(SupportedDeviceCollectionFolderPath, "GenericDevice");
                Directory.CreateDirectory(genericDeviceDirectory);
                ResourceHlprs.CopyResource(undefinedThumbnailResourcePath, Path.Combine(genericDeviceDirectory, "thumbnail.png"));
            }
        }
        public void CreatePaletteCollectionFolder()
        {
            if (!Directory.Exists(PalettesCollectionFolderPath))
            {

                //create default palette
                var palettes = new List<ColorPalette>();
                var rainbow = new ColorPalette("Full Rainbow", "Zooey", "RGBPalette16", "Default Color Palette by Ambino", DefaultColorCollection.rainbow);
                var police = new ColorPalette("Police", "Zooey", "RGBPalette16", "Default Color Palette by Ambino", DefaultColorCollection.police);
                var forest = new ColorPalette("Forest", "Zooey", "RGBPalette16", "Default Color Palette by Ambino", DefaultColorCollection.forest);
                var aurora = new ColorPalette("Aurora", "Zooey", "RGBPalette16", "Default Color Palette by Ambino", DefaultColorCollection.aurora);
                var iceandfire = new ColorPalette("Ice and Fire", "Zooey", "RGBPalette16", "Default Color Palette by Ambino", DefaultColorCollection.iceandfire);
                var scarlet = new ColorPalette("Scarlet", "Zooey", "RGBPalette16", "Default Color Palette by Ambino", DefaultColorCollection.scarlet);
                var party = new ColorPalette("Party", "Zooey", "RGBPalette16", "Default Color Palette by Ambino", DefaultColorCollection.party);
                var cloud = new ColorPalette("Cloud", "Zooey", "RGBPalette16", "Default Color Palette by Ambino", DefaultColorCollection.cloud);
                var france = new ColorPalette("France", "Zooey", "RGBPalette16", "Default Color Palette by Ambino", DefaultColorCollection.france);
                var badtrip = new ColorPalette("Bad Trip", "Zooey", "RGBPalette16", "Default Color Palette by Ambino", DefaultColorCollection.badtrip);
                var lemon = new ColorPalette("Lemon", "Zooey", "RGBPalette16", "Default Color Palette by Ambino", DefaultColorCollection.lemon);

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
                Directory.CreateDirectory(PalettesCollectionFolderPath);
                var collectionFolder = Path.Combine(PalettesCollectionFolderPath, "collection");
                Directory.CreateDirectory(collectionFolder);
                foreach (var palette in palettes)
                {
                    var json = JsonConvert.SerializeObject(palette, new JsonSerializerSettings() {
                        TypeNameHandling = TypeNameHandling.Auto
                    });
                    File.WriteAllText(Path.Combine(collectionFolder, palette.Name + ".col"), json);
                }

                //coppy all internal palettes to local 
                var config = new ResourceLoaderConfig(nameof(ColorPalette), DeserializeMethodEnum.MultiJson);
                var configJson = JsonConvert.SerializeObject(config, new JsonSerializerSettings() {
                    TypeNameHandling = TypeNameHandling.Auto
                });
                File.WriteAllText(Path.Combine(PalettesCollectionFolderPath, "config.json"), configJson);

            }

        }

        public List<AppProfile> LoadAppProfileIfExist()
        {
            var existedProfile = new List<AppProfile>();
            string[] files = Directory.GetFiles(Path.Combine(ProfileCollectionFolderPath, "collection"));
            foreach (var file in files)
            {
                var jsonData = File.ReadAllText(file);
                var profile = JsonConvert.DeserializeObject<AppProfile>(jsonData, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
                existedProfile.Add(profile);
            }
            return existedProfile;

        }

        public List<AutomationSettings> LoadAutomationIfExist()
        {
            var loadedAutomations = new List<AutomationSettings>();
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
            // AmbinityClient.Dispose();
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

        public void WriteGenralSettingsJson()
        {
            var json = JsonConvert.SerializeObject(GeneralSettings, new JsonSerializerSettings() {
                TypeNameHandling = TypeNameHandling.Auto
            });
            Directory.CreateDirectory(JsonPath);
            File.WriteAllText(JsonGeneralFileNameAndPath, json);
        }



        public void WriteAutomationCollectionJson()
        {
            var automations = new List<AutomationSettings>();
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

        //private ObservableCollection<DeviceProfile> ProfileFilter(IDeviceSettings device)
        //{
        //    var filteredProfiles = new ObservableCollection<DeviceProfile>();
        //    foreach (var profile in AvailableProfiles)
        //    {
        //        if (profile.DeviceType == device.DeviceType)
        //        {
        //            filteredProfiles.Add(profile);
        //        }
        //    }
        //    return filteredProfiles;
        //}
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



        public bool IsLiveViewOpen => SelectedViewPart == SelectableViewParts[1];
        private bool _isAppActivated;
        public bool IsAppActivated {
            get { return _isAppActivated; }
            set { _isAppActivated = value; RaisePropertyChanged(); }
        }
        private int _toolBarWidth = 450;
        public int ToolBarWidth {
            get { return _toolBarWidth; }
            set { _toolBarWidth = value; RaisePropertyChanged(); }
        }

        private bool _isInIDEditStage;
        public bool IsInIDEditStage {
            get { return _isInIDEditStage; }
            set
            {
                _isInIDEditStage = value;
                GetItemsForLiveView();
                UpdateLiveView();
                RaisePropertyChanged();
            }
        }
        public enum IDMode
        {
            VID = 0,
            FID = 1
        }
        private IDMode _idEditMode;
        public IDMode IdEditMode {
            get { return _idEditMode; }
            set
            {
                _idEditMode = value;
                RaisePropertyChanged();

            }
        }
        public ObservableCollection<VIDToolBarDataModel> AvailableTools { get; set; }
        private int _selectedToolIndex = 0;
        public int SelectedToolIndex {
            get { return _selectedToolIndex; }
            set
            {
                _selectedToolIndex = value;
                RaisePropertyChanged();
                if (SelectedToolIndex == 0)
                {
                    //selection mode
                    if (IDEditBrush != null)
                    {
                        if (LiveViewItems.Contains(IDEditBrush))
                            LiveViewItems.Remove(IDEditBrush);
                        if (LiveViewItems.Contains(EraserBrush))
                            LiveViewItems.Remove(EraserBrush);
                    }

                }
                else if (SelectedToolIndex == 1)
                {
                    if (IDEditBrush != null)
                    {
                        if (LiveViewItems.Contains(EraserBrush))
                            LiveViewItems.Remove(EraserBrush);
                        if (!LiveViewItems.Contains(IDEditBrush))
                            LiveViewItems.Add(IDEditBrush);
                    }

                }
                else if (SelectedToolIndex == 2)
                {
                    if (IDEditBrush != null)
                    {
                        if (LiveViewItems.Contains(IDEditBrush))
                            LiveViewItems.Remove(IDEditBrush);
                        if (!LiveViewItems.Contains(EraserBrush))
                            LiveViewItems.Add(EraserBrush);
                    }

                }
            }
        }
        private int _brushIntensity = 5;
        public int BrushIntensity {
            get { return _brushIntensity; }
            set
            {
                _brushIntensity = value;
                RaisePropertyChanged();
            }
        }

        private PathGuide _iDEditBrush;
        public PathGuide IDEditBrush {
            get { return _iDEditBrush; }
            set
            {
                _iDEditBrush = value;
                RaisePropertyChanged();
            }
        }
        private PathGuide _eraserBrush;
        public PathGuide EraserBrush {
            get { return _eraserBrush; }
            set
            {
                _eraserBrush = value;
                RaisePropertyChanged();
            }
        }
        private int _brushSize;
        public int BrushSize {
            get { return _brushSize; }
            set
            {
                _brushSize = value;
                if (SelectedToolIndex == 1)
                {
                    IDEditBrush.Width = value;
                    IDEditBrush.Height = value;
                    IDEditBrush.Left = MousePosition.X - value / 2;
                    IDEditBrush.Top = MousePosition.Y - value / 2;
                }
                else if (SelectedToolIndex == 2)
                {
                    EraserBrush.Width = value;
                    EraserBrush.Height = value;
                    EraserBrush.Left = MousePosition.X - value / 2;
                    EraserBrush.Top = MousePosition.Y - value / 2;

                }

                RaisePropertyChanged();
            }
        }
        public void IncreaseBrushSize()
        {

            if (SelectedToolIndex == 1)
            {
                if (IDEditBrush.Width < 1000)
                {

                    IDEditBrush.Width += 5;
                    IDEditBrush.Height += 5;
                    IDEditBrush.Left = MousePosition.X - IDEditBrush.Width / 2;
                    IDEditBrush.Top = MousePosition.Y - IDEditBrush.Height / 2;
                }
            }
            else if (SelectedToolIndex == 2)
            {
                if (EraserBrush.Width < 1000)
                {

                    EraserBrush.Width += 5;
                    EraserBrush.Height += 5;
                    EraserBrush.Left = MousePosition.X - EraserBrush.Width / 2;
                    EraserBrush.Top = MousePosition.Y - EraserBrush.Height / 2;
                }

            }
        }
        public void DecreaseBrushSize()
        {
            if (SelectedToolIndex == 1)
            {
                if (IDEditBrush.Width > 10)
                {

                    IDEditBrush.Width -= 5;
                    IDEditBrush.Height -= 5;
                    IDEditBrush.Left = MousePosition.X - IDEditBrush.Width / 2;
                    IDEditBrush.Top = MousePosition.Y - IDEditBrush.Height / 2;
                }
            }
            else if (SelectedToolIndex == 2)
            {
                if (EraserBrush.Width < 1000)
                {

                    EraserBrush.Width -= 5;
                    EraserBrush.Height -= 5;
                    EraserBrush.Left = MousePosition.X - EraserBrush.Width / 2;
                    EraserBrush.Top = MousePosition.Y - EraserBrush.Height / 2;
                }

            }

        }
        private void GetToolsForIDEdit()
        {
            if (AvailableTools == null)
            {
                AvailableTools = new ObservableCollection<VIDToolBarDataModel>() {
                                 new VIDToolBarDataModel(){Name = "Selection Tool",Geometry= "selectionTool"},
                                 new VIDToolBarDataModel(){Name = "Brush Tool",Geometry= "brushTool"},
                                 new VIDToolBarDataModel(){Name = "Eraser Tool",Geometry= "eraserTool"},
                     };
            }
            //add brush
            SelectedToolIndex = 0;
        }
        private void GetBrushForIDEdit()
        {

            IDEditBrush = new PathGuide() {
                Width = 200,
                Height = 200,
                IsSelectable = false,
                VisualProperties = new VisualProperties() { FillColor = Color.FromRgb(255, 255, 255) },
                Geometry = "genericCircle"

            };
            EraserBrush = new PathGuide() {
                Width = 200,
                Height = 200,
                IsSelectable = false,
                VisualProperties = new VisualProperties() { FillColor = Color.FromRgb(255, 0, 0) },
                Geometry = "genericCircle"

            };
            BrushSize = 200;
            VIDCount = 0;
            _lastBrushX = 0;
            _lastBrushY = 0;
            RegisterBrush(IDEditBrush);
            RegisterBrush(EraserBrush);
        }
        private void DrawVID(IDrawable brush)
        {

            //check if brush has moved more than 10 unit, consider as update interval
            if (CalculateDelta(_lastBrushX, _lastBrushY, brush.Left, brush.Top) > 10)
            {

                _lastBrushX = brush.Left;
                _lastBrushY = brush.Top;
                //check if this position intersect any zone
                int settedVIDCount = 0;

                foreach (var zone in LiveViewItems.Where(z => z is LEDSetup))
                {
                    if (!Rectangle.Intersect(zone.GetRect, brush.GetRect).IsEmpty)
                    {
                        //this zone is being touch by the brush
                        //check if this brush touch any spot inside this zone
                        var ledSetup = zone as LEDSetup;
                        var intersectRect = Rectangle.Intersect(ledSetup.GetRect, brush.GetRect);
                        intersectRect.Offset(new System.Drawing.Point(0 - (int)ledSetup.GetRect.Left, 0 - (int)ledSetup.GetRect.Top));

                        foreach (var spot in ledSetup.Spots)
                        {
                            switch (SelectedToolIndex)
                            {
                                case 1:
                                    if (spot.GetVIDIfNeeded(VIDCount, intersectRect, 0))
                                        settedVIDCount++;
                                    break;
                                case 2:
                                    if (spot.GetVIDIfNeeded(VIDCount, intersectRect, 1))
                                        settedVIDCount++;
                                    break;
                            }

                        }

                    }
                }
                if (settedVIDCount > 0)
                {
                    if (SelectedToolIndex == 1)
                    {
                        VIDCount += BrushIntensity;
                        if (VIDCount > 1023)
                            VIDCount = 0;
                    }
                    else if (SelectedToolIndex == 2)
                    {
                        VIDCount -= BrushIntensity;
                        if (VIDCount < 0)
                            VIDCount = 1023;
                    }
                }





            }

        }
        private void RegisterBrush(IDrawable brush)
        {
            brush.PropertyChanged += async (_, __) =>
            {
                switch (__.PropertyName)
                {
                    case nameof(brush.Left):
                    case nameof(brush.Top):
                        if (Keyboard.IsKeyDown(Key.B))
                        {
                            await Task.Run(() => DrawVID(brush));
                        }

                        break;
                }
            };
        }
        private bool _isInIsolateMode;
        public bool IsInIsolateMode {
            get { return _isInIsolateMode; }
            set
            {
                _isInIsolateMode = value;
                RaisePropertyChanged();
            }
        }
        private void IsolateSelectedItems()
        {

            var selectedItems = LiveViewItems.Where(i => i.IsSelected).ToList();
            if (selectedItems.Count == 0)
            {
                HandyControl.Controls.MessageBox.Show("Bạn phải chọn một vùng LED hoặc Group trước", "No item selected", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            IsInIsolateMode = true;
            LiveViewItems.Clear();
            foreach (var item in selectedItems)
            {
                if (item is LEDSetup)
                {
                    LiveViewItems.Add(item);


                }
                else if (item is Border)
                {
                    var group = CurrentDevice.ControlZoneGroups.Where(g => g.Border.Name == item.Name).FirstOrDefault();
                    var childZone = GetGroupChildItems(group, CurrentDevice);
                    foreach (var zone in childZone)
                    {
                        if (zone is LEDSetup)
                            LiveViewItems.Add(zone as LEDSetup);
                        else if (zone is FanMotor)
                            LiveViewItems.Add(zone as FanMotor);
                    }
                    LiveViewItems.Add(item);



                }
                else if (item is FanMotor)
                {
                    LiveViewItems.Add(item);


                }

            }
            if (IsInIDEditStage)
            {
                GetBrushForIDEdit();
            }
            SelectLiveViewItemCommand.Execute(selectedItems.First());

        }

        public void GotoChild(IDeviceSettings selectedDevice)
        {
            SelectedViewPart = SelectableViewParts[1];
            CurrentDevice = selectedDevice;
            GetItemsForLiveView();
            UpdateLiveView();
            IsSplitLightingWindowOpen = true;


        }
        public IList<ISelectableViewPart> SelectableViewParts { get; }
        public ISelectableViewPart _selectedViewPart;
        public ISelectableViewPart SelectedViewPart {
            get => _selectedViewPart;
            set
            {
                Set(ref _selectedViewPart, value);

            }
        }
        public void BackToDashboard()
        {
            SelectedViewPart = SelectableViewParts[0];
        }

        public void BackToCollectionView()
        {

            CurrentOnlineStoreView = "Collections";
            CarouselImageLoading = true;
            Task.Run(() => UpdateStoreView());
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