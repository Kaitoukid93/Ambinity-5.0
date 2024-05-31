using adrilight_shared.Enums;
using adrilight_shared.Helpers;
using adrilight_shared.Models.ControlMode.ModeParameters;
using adrilight_shared.Models.ControlMode.ModeParameters.ParameterValues;
using adrilight_shared.Models.Device;
using adrilight_shared.Models.RelayCommand;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using adrilight_shared.Models.ItemsCollection;

namespace adrilight.ViewModel
{
    public class DeviceAdvanceSettingsViewModel : ViewModelBase
    {
        #region Construct
        private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "adrilight\\");
        private string BackupFolder => Path.Combine(JsonPath, "Backup");

        public DeviceAdvanceSettingsViewModel(DeviceDBManager dbManager,
            DeviceHardwareSettings deviceHardwareSettings,
            DeviceFirmwareUpdater firmwareUpdater)
        {
            _deviceDBManager = dbManager ?? throw new ArgumentNullException(nameof(dbManager));
            _deviceHardwareSettings = deviceHardwareSettings ?? throw new ArgumentNullException(nameof(deviceHardwareSettings));
            _deviceFirmwareUpdater = firmwareUpdater ?? throw new ArgumentNullException(nameof(firmwareUpdater));
            LocalFileHlprs = new LocalFileHelpers();
            CommandSetup();

        }
        #endregion
        #region Events
        private void Device_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Device.HWL_effectMode):
                    HardwareLightingControlInit();
                    break;
            }
        }
        #endregion
        #region Properties
        private IDeviceSettings _device;
        public IDeviceSettings Device {
            get
            {
                return _device;
            }
            set
            {
                _device = value;
                RaisePropertyChanged();
            }
        }
        private LocalFileHelpers LocalFileHlprs;
        private DeviceDBManager _deviceDBManager;
        private DeviceHardwareSettings _deviceHardwareSettings;
        private DeviceFirmwareUpdater _deviceFirmwareUpdater;
        private bool _hwl_HasIntensityControl = false;
        private bool _hwl_HasSpeedControl = false;
        private string _selectedCustomFirmwarePath;
        private string _selectedDeviceBackupFile;
        private string _updateButtonContent = "Check for update";
        private string _updateInstructionContent = adrilight_shared.Properties.Resources.CheckForUpdate_content;
        private string _newFirmwareVersionContent;
        private bool _updateAvailable;
        private Visibility _hardwareSettingsEnable;
        public bool HWL_HasSpeedControl {
            get
            {
                return _hwl_HasSpeedControl;
            }
            set
            {
                _hwl_HasSpeedControl = value;
                RaisePropertyChanged();
            }
        }
        public bool HWL_HasIntensityControl {
            get
            {
                return _hwl_HasIntensityControl;
            }
            set
            {
                _hwl_HasIntensityControl = value;
                RaisePropertyChanged();
            }
        }
        private ListSelectionParameter _hardwareLightingColorSelection;
        public ListSelectionParameter HardwareLightingColorSelection {
            get
            {
                return _hardwareLightingColorSelection;
            }
            set
            {
                _hardwareLightingColorSelection = value;
                RaisePropertyChanged();
            }
        }
        public string SelectedCustomFirmwarePath {
            get
            {
                return _selectedCustomFirmwarePath;
            }

            set
            {
                _selectedCustomFirmwarePath = value;
                RaisePropertyChanged();
            }
        }
        public string SelectedDevicebackupFile {
            get
            {
                return _selectedDeviceBackupFile;
            }

            set
            {
                _selectedDeviceBackupFile = value;
                RaisePropertyChanged();
            }
        }
        public string UpdateButtonContent {
            get
            {
                return _updateButtonContent;
            }

            set
            {
                _updateButtonContent = value;
                RaisePropertyChanged();
            }
        }
        public string UpdateInstructionContent {
            get
            {
                return _updateInstructionContent;
            }

            set
            {
                _updateInstructionContent = value;
                RaisePropertyChanged();
            }
        }
        public string NewFirmwareVersionContent {
            get
            {
                return _newFirmwareVersionContent;
            }
            set
            {
                _newFirmwareVersionContent = value;
                RaisePropertyChanged();
            }
        }
        public bool UpdateAvailable {
            get
            {
                return _updateAvailable;
            }
            set
            {
                _updateAvailable = value;
                RaisePropertyChanged();
            }
        }
        public Visibility HardwareSettingsEnable {
            get
            {
                return _hardwareSettingsEnable;
            }

            set
            {
                _hardwareSettingsEnable = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Methods
        public async Task Init(IDeviceSettings device)
        {
            if (device == null)
                return;
            Device = device;
            Device.PropertyChanged += Device_PropertyChanged;
            _deviceDBManager.Init(Device);
            _deviceFirmwareUpdater.Init(Device);
            await _deviceHardwareSettings.Init(Device);
            //ambino hubV2 is not supported in-app downloading
            if (Device.DeviceType.Type == DeviceTypeEnum.AmbinoHUBV2)
            {
                UpdateButtonContent = adrilight_shared.Properties.Resources.EnterDFU_ButtonContent;
                UpdateInstructionContent = adrilight_shared.Properties.Resources.HUBV_Checkforupdate_content;
            }
            HardwareLightingControlInit();
        }
        private void CommandSetup()
        {
            ShowDeviceBackupFolderCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                var backupPath = Path.Combine(BackupFolder, "Device");
                if (Directory.Exists(backupPath))
                    Process.Start("explorer.exe", backupPath);

            });
            BackupDeviceCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, async (p) =>
            {
                await _deviceDBManager.BackupDevice();

            });
            RestoreDeviceCommand = new RelayCommand<string>((p) =>
                {
                    return true;
                }, async (p) =>
                {
                    await _deviceDBManager.RestoreDeviceFromFile(SelectedDevicebackupFile);

                });
            SelecFirmwareForCurrentDeviceCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                string openFileDialogDescription = "hex Files (.HEX)|*.hex";
                switch (Device.DeviceFirmwareExtension)
                {
                    case ".hex":
                        openFileDialogDescription = "hex Files (.HEX)|*.hex";
                        break;
                    case ".uf2":
                        openFileDialogDescription = "uf2 Files (.UF2)|*.uf2";
                        break;
                }
                var file = LocalFileHlprs.OpenImportFileDialog(Device.DeviceFirmwareExtension, openFileDialogDescription);
                if (file != null)
                {
                    SelectedCustomFirmwarePath = file;
                }

            });
            SelectBackupFileCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {

                var file = LocalFileHlprs.OpenImportFileDialog(adrilight_shared.Properties.Resources.DeviceAdvanceSettingsViewModel_ChoseBackupFile_header, "zip Files (.ZIP)|*.zip");
                if (file != null)
                {
                    SelectedDevicebackupFile = file;
                }

            });
            ApplyDeviceHardwareSettingsCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, async (p) =>
            {
                await _deviceHardwareSettings.ShowHWSettingsDialog(Device);
            });
            UpdateDeviceFirmwareCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, async (p) =>
            {
                await _deviceFirmwareUpdater.Update();

            });
            DownloadCustomFirmwareCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, async (p) =>
            {
                await _deviceFirmwareUpdater.Update(SelectedCustomFirmwarePath);

            });

        }
        private void ColorSelectionInit(List<string> source)
        {
            HardwareLightingColorSelection = new ListSelectionParameter(ModeParameterEnum.Color);
            HardwareLightingColorSelection.DataSourceLocaFolderNames = source;
            HardwareLightingColorSelection.Name = adrilight_shared.Properties.Resources.LightingEngine_ColorControl_header;
            HardwareLightingColorSelection.Description = adrilight_shared.Properties.Resources.DeviceAdvanceSettingsViewModel_ColorSelectionInit_SelectColors;
            HardwareLightingColorSelection.LoadAvailableValues();
            UpdateColorSelectionSelectedValue();
            HardwareLightingColorSelection.PropertyChanged += (_, __) =>
            {
                switch (__.PropertyName)
                {
                    case nameof(HardwareLightingColorSelection.SelectedValue):
                        //update offline palette here
                        if (HardwareLightingColorSelection.SelectedValue is ColorPalette)
                        {
                            //resize color palette o 8 colors
                            var palette = HardwareLightingColorSelection.SelectedValue as ColorPalette;
                            palette.Resize(8);
                            Device.HWL_palette = palette.Colors;
                        }
                        else if (HardwareLightingColorSelection.SelectedValue is ColorCard)
                        {
                            var selectedColor = HardwareLightingColorSelection.SelectedValue as ColorCard;
                            if (selectedColor != null)
                            {
                                var usableColor = selectedColor.StartColor;
                                Device.HWL_singleColor = usableColor;
                            }
                        }
                        break;
                }
            };
        }
        private void HardwareLightingControlInit()
        {
            //if has color control
            List<string> source = new List<string>();
            if (Device.HWL_effectMode == 0 || Device.HWL_effectMode == 2)
                source = new List<string>() { "Colors" };
            else
            {
                source = new List<string>() { "ColorPalettes" };
            }
            ColorSelectionInit(source);
            if (Device.HWL_effectMode == 1 || Device.HWL_effectMode == 2)
                HWL_HasSpeedControl = true;
            else
                HWL_HasSpeedControl = false;
            if (Device.HWL_effectMode == 1)
                HWL_HasIntensityControl = true;
            else
                HWL_HasIntensityControl = false;

        }
        public async Task RefreshDeviceHardwareInfo()
        {
            var rslt = await Task.Run(() => _deviceHardwareSettings.GetHardwareSettings(false,Device));
            if (!rslt)
            {
                HardwareSettingsEnable = Visibility.Collapsed;
            }
            else
            {
                HardwareSettingsEnable = Visibility.Visible;
            }

        }
        private void UpdateColorSelectionSelectedValue()
        {
            if (Device.HWL_effectMode == 0 || Device.HWL_effectMode == 2)
            {
                HardwareLightingColorSelection.SelectedValue = new ColorCard() { StartColor = Device.HWL_singleColor, StopColor = Device.HWL_singleColor };
            }
            else if (Device.HWL_effectMode == 1)
            {
                HardwareLightingColorSelection.SelectedValue = new ColorPalette() { Colors = Device.HWL_palette };
            }
        }
        public void Dispose()
        {
            if (Device != null)
                Device.PropertyChanged -= Device_PropertyChanged;
            Device = null;
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Commands
        public ICommand BackupDeviceCommand { get; set; }
        public ICommand RestoreDeviceCommand { get; set; }
        public ICommand ShowDeviceBackupFolderCommand { get; set; }
        public ICommand SelecFirmwareForCurrentDeviceCommand { get; set; }
        public ICommand SelectBackupFileCommand { get; set; }
        public ICommand UpdateDeviceFirmwareCommand { get; set; }
        public ICommand ApplyDeviceHardwareSettingsCommand { get; set; }
        public ICommand DownloadCustomFirmwareCommand { get; set; }
        #endregion
    }
}

