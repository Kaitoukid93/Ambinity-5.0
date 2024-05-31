using adrilight_shared.Enums;
using adrilight_shared.Helpers;
using adrilight_shared.Models.ControlMode.ModeParameters.ParameterValues;
using adrilight_shared.Models.Device.SlaveDevice;
using adrilight_shared.ViewModel;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO.Ports;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using adrilight_shared.Services;

namespace adrilight_shared.Models.Device
{
    public class DeviceHardwareSettings
    {
        public DeviceHardwareSettings(DialogService service)
        {
            _dialogService = service;
        }
        public async Task Init(IDeviceSettings device)
        {
            var result = await GetHardwareSettings(false,device);
            if (!result)
                return;
            //show not supported dialog
        }
        private DialogService _dialogService;
        private static byte[] requestCommand = { (byte)'d', (byte)'i', (byte)'r', (byte)'\n' };
        private static byte[] sendCommand = { (byte)'h', (byte)'s', (byte)'d' };
        private static byte[] expectedValidHeader = { 15, 12, 93 };


        #region Hardware Related Properties and Method
        private byte[] GetSettingOutputStream(IDeviceSettings device)
        {
            var outputStream = new byte[48];
            Buffer.BlockCopy(sendCommand, 0, outputStream, 0, sendCommand.Length);
            int counter = sendCommand.Length;
            outputStream[counter++] = (byte)(device.HWL_enable == true ? 1 : 0);
            outputStream[counter++] = (byte)(device.StatusLEDEnable == true ? 1 : 0);
            outputStream[counter++] = (byte)device.HWL_returnafter;
            outputStream[counter++] = (byte)device.HWL_effectMode;
            outputStream[counter++] = (byte)device.HWL_effectSpeed;
            outputStream[counter++] = (byte)device.HWL_brightness;
            outputStream[counter++] = (byte)device.HWL_singleColor.R;
            outputStream[counter++] = (byte)device.HWL_singleColor.G;
            outputStream[counter++] = (byte)device.HWL_singleColor.B;
            for (int i = 0; i < 8; i++)
            {
                outputStream[counter++] = (byte)device.HWL_palette[i].R;
                outputStream[counter++] = (byte)device.HWL_palette[i].G;
                outputStream[counter++] = (byte)device.HWL_palette[i].B;
            }
            outputStream[counter++] = (byte)device.HWL_effectIntensity;
            outputStream[counter++] = (byte)device.NoSignalFanSpeed;
            outputStream[counter++] = (byte)device.HWL_MaxLEDPerOutput;
            return outputStream;
        }
        private byte[] GetEEPRomDataOutputStream()
        {
            var outputStream = new byte[48];
            Buffer.BlockCopy(sendCommand, 0, outputStream, 0, sendCommand.Length);
            outputStream[3] = 255;
            return outputStream;
        }
        private bool IsFirmwareValid(IDeviceSettings device)
        {
            if (device.DeviceType.Type == DeviceTypeEnum.AmbinoBasic ||
                device.DeviceType.Type == DeviceTypeEnum.AmbinoEDGE ||
                device.DeviceType.Type == DeviceTypeEnum.AmbinoFanHub ||
                device.DeviceType.Type == DeviceTypeEnum.AmbinoHUBV3)
            {
                string fwversion = device.FirmwareVersion;
                if (fwversion == "unknown" || fwversion == string.Empty || fwversion == null)
                    fwversion = "1.0.0";
                var deviceFWVersion = new Version(fwversion);
                var requiredVersion = new Version();
                if (device.DeviceType.Type == DeviceTypeEnum.AmbinoBasic)
                {
                    requiredVersion = new Version("1.0.8");
                }
                else if (device.DeviceType.Type == DeviceTypeEnum.AmbinoEDGE)
                {
                    requiredVersion = new Version("1.0.5");
                }
                else if (device.DeviceType.Type == DeviceTypeEnum.AmbinoFanHub)
                {
                    requiredVersion = new Version("1.0.8");
                }
                if (deviceFWVersion >= requiredVersion)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            { return false; }
        }
        public async Task<bool> GetHardwareSettings(bool skipDeviceType, IDeviceSettings device)
        {
            Thread.Sleep(500);
            if (!skipDeviceType)
            {
                device.IsTransferActive = false; // stop current serial stream attached to this device
                string deviceName = null;
                string deviceID = null;
                string deviceFirmware = null;
                string deviceHardware = null;
                int deviceHWL = 0;
                var result = RefreshDeviceInfo(device.OutputPort, out deviceName, out deviceID, out deviceFirmware, out deviceHardware, out deviceHWL);

                if (!result)
                {
                    return false;
                }
                if (!IsFirmwareValid(device))
                {
                    return false;
                }
                device.DeviceName = deviceName;
                device.FirmwareVersion = deviceFirmware;
                device.HardwareVersion = deviceHardware;
                device.DeviceSerial = deviceID;
                device.HWL_version = deviceHWL;
                if (device.HWL_version < 1)
                {
                    //request firmware update and hide device settings
                    HandyControl.Controls.MessageBox.Show(adrilight_shared.Properties.Resources.DeviceAdvanceSettingsViewModel_GetHardwareSettings_OldFirmware, adrilight_shared.Properties.Resources.DeviceAdvanceSettingsViewModel_GetHardwareSettings_ProtocolOutdated, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }

            device.IsTransferActive = false; // stop current serial stream attached to this device
            if (!SerialPort.GetPortNames().Contains(device.OutputPort))
                return false;
            var _serialPort = new SerialPort(device.OutputPort, 1000000);
            _serialPort.DtrEnable = true;
            _serialPort.ReadTimeout = 5000;
            _serialPort.WriteTimeout = 1000;
            try
            {
                _serialPort.Open();
            }
            catch (UnauthorizedAccessException)
            {
                return await Task.FromResult(false);
            }
            catch (System.IO.IOException ex)
            {
                return await Task.FromResult(false);
            }

            var outputStream = GetEEPRomDataOutputStream();
            _serialPort.Write(outputStream, 0, outputStream.Length);
            _serialPort.WriteLine("\r\n");
            int retryCount = 0;
            int offset = 0;
            //searching for header
            while (offset < 3)
            {


                try
                {
                    byte header = (byte)_serialPort.ReadByte();
                    if (header == expectedValidHeader[offset])
                    {
                        offset++;
                    }
                }
                catch (TimeoutException)// retry until received valid header
                {
                    _serialPort.Write(outputStream, 0, outputStream.Length);
                    retryCount++;
                    if (retryCount == 3)
                    {
                        Log.Warning("timeout waiting for respond on serialport " + _serialPort.PortName);
                        _serialPort.Close();
                        _serialPort.Dispose();
                        return await Task.FromResult(false);
                    }
                    Debug.WriteLine("no respond, retrying...");
                }


            }
            //3 bytes header are valid continue to read next 45 byte of data
            if (offset == 3)
            {
                try
                {
                    ReadDeviceEEPROM(_serialPort, device);
                    //discard buffer
                    _serialPort.DiscardInBuffer();
                }
                catch (TimeoutException ex)
                {
                    //discard buffer
                    _serialPort.DiscardInBuffer();
                    _serialPort.Close();
                    _serialPort.Dispose();
                    return await Task.FromResult(true);
                }


            }
            //discard buffer
            _serialPort.DiscardInBuffer();
            _serialPort.Close();
            _serialPort.Dispose();
            return await Task.FromResult(true);
        }
        public void ReadDeviceEEPROM(SerialPort _serialPort, IDeviceSettings device)
        {
            /// Hardware Lighting Protocol version 1
            //+----------+---------------------+------------+---------------+
            //| Position | Name                | Size(byte) | Default Value |
            //+----------+---------------------+------------+---------------+
            //| 0        | HWL_enable          | 1          | 1             |
            //| 1        | StatusLEDEnable     | 1          | 0             |
            //| 2        | HWL_returnafter     | 1          | 3             |
            //| 3        | HWL_effectMode      | 1          | 0             |
            //| 4        | HWL_effectSpeed     | 1          | 20            |
            //| 5        | HWL_brightness      | 1          | 80            |
            //| 6 - 8    | HWL_singlecolor     | 3          | 255,0,0       |
            //| 9-32     | HWL_palette         | 24         |               |
            //| 33       | HWL_effectIntensity | 1          | 16            |
            //| 34       | HWF_FanSpeed        |            | 127           |
            //| 35-44    | HWF_PerFanSpeed     | 10         | 127           |
            //+----------+---------------------+------------+---------------+


            device.HWL_enable = _serialPort.ReadByte() == 1 ? true : false;
            Log.Information("HWL_enable: " + device.HWL_enable);
            device.StatusLEDEnable = _serialPort.ReadByte() == 1 ? true : false;
            Log.Information("StatusLEDEnable: " + device.StatusLEDEnable);
            device.HWL_returnafter = (byte)_serialPort.ReadByte();
            Log.Information("HWL_returnafter: " + device.HWL_returnafter);
            device.HWL_effectMode = (byte)_serialPort.ReadByte();
            Log.Information("HWL_effectMode: " + device.HWL_effectMode);
            device.HWL_effectSpeed = (byte)_serialPort.ReadByte();
            Log.Information("HWL_effectSpeed: " + device.HWL_effectSpeed);
            device.HWL_brightness = (byte)_serialPort.ReadByte();
            Log.Information("HWL_brightness: " + device.HWL_brightness);
            device.HWL_singleColor = Color.FromRgb((byte)_serialPort.ReadByte(), (byte)_serialPort.ReadByte(), (byte)_serialPort.ReadByte());
            Log.Information("HWL_singleColor: " + device.HWL_singleColor);
            if (device.HWL_palette == null)
            {
                device.HWL_palette = new Color[8];
            }
            for (int i = 0; i < 8; i++)
            {
                device.HWL_palette[i] = Color.FromRgb((byte)_serialPort.ReadByte(), (byte)_serialPort.ReadByte(), (byte)_serialPort.ReadByte());
                Log.Information("HWL_palette " + i + ": " + device.HWL_palette[i]);
            }

            device.HWL_effectIntensity = (byte)_serialPort.ReadByte();
            Log.Information("HWL_effectIntensity: " + device.HWL_effectIntensity);

            var noSignalFanSpeed = _serialPort.ReadByte();
            device.NoSignalFanSpeed = noSignalFanSpeed < 20 ? 20 : noSignalFanSpeed;
            Log.Information("NoSignalFanSpeed: " + noSignalFanSpeed);
            device.HWL_MaxLEDPerOutput = (byte)_serialPort.ReadByte();
            Log.Information("HWL_MaxLEDPerOutput: " + device.HWL_MaxLEDPerOutput);

        }
        private void ShowUpdateSuccessMessage(ProgressDialogViewModel vm, IDeviceSettings device)
        {
            vm.Value = 0;
            vm.ProgressBarVisibility = Visibility.Collapsed;
            vm.Header = "Done";
            vm.SuccessMessage = "Applied " + " HWL Version " + device.HWL_version;
            vm.SuccessMesageVisibility = Visibility.Visible;
            vm.SecondaryActionButtonContent = "Close";
            vm.PrimaryActionButtonContent = "Show Log";
        }
        public async Task ShowHWSettingsDialog(IDeviceSettings device)
        {
            var vm = new ProgressDialogViewModel("Applying settings", "123", "usbIcon");
            vm.ProgressBarVisibility = Visibility.Visible;
            vm.CurrentProgressHeader = "Sending to device";
            vm.ProgressBarVisibility = Visibility.Visible;
            Task.Run(() => SendHardwareSettings(vm,device));
            _dialogService.ShowDialog<ProgressDialogViewModel>(result =>
            {
                //try update the view if needed

            }, vm);

        }
        public async Task<bool> SendHardwareSettings(ProgressDialogViewModel vm, IDeviceSettings device)
        {

            // start fwtool
            ///////////////////// Hardware settings data table, will be wirte to device EEPRom /////////
            /// [h,s,d,Led on/off,Signal LED On/off,Connection Type,Max Brightness,Show Welcome LED,Serial Timeout,0,0,0,0,0,0,0] ///////

            device.IsTransferActive = false; // stop current serial stream attached to this device
            var _serialPort = new SerialPort(device.OutputPort, 1000000);
            _serialPort.DtrEnable = true;
            _serialPort.ReadTimeout = 5000;
            _serialPort.WriteTimeout = 1000;
            try
            {
                _serialPort.Open();
            }
            catch (UnauthorizedAccessException)
            {
                return await Task.FromResult(false);
            }

            var outputStream = GetSettingOutputStream(device);
            vm.Value = 25;
            await Task.Delay(1000);
            _serialPort.Write(outputStream, 0, outputStream.Length);
            _serialPort.WriteLine("\r\n");
            int retryCount = 0;
            int offset = 0;
            while (offset < 3)
            {


                try
                {
                    byte header = (byte)_serialPort.ReadByte();
                    if (header == expectedValidHeader[offset])
                    {
                        offset++;
                    }
                }
                catch (TimeoutException)// retry until received valid header
                {
                    _serialPort.Write(outputStream, 0, outputStream.Length);
                    retryCount++;
                    if (retryCount == 3)
                    {
                        Console.WriteLine("timeout waiting for respond on serialport " + _serialPort.PortName);
                        _serialPort.Close();
                        _serialPort.Dispose();
                        return await Task.FromResult(false);
                    }
                    Debug.WriteLine("no respond, retrying...");
                }


            }
            if (offset == 3) //3 bytes header are valid continue to read next 13 byte of data
            {
                ReadDeviceEEPROM(_serialPort, device);
                //discard buffer
                _serialPort.DiscardInBuffer();
            }

            _serialPort.Close();
            _serialPort.Dispose();
            for (int i = 0; i < 10; i++)
            {
                vm.Value += 10;
                if (vm.Value > 100)
                    vm.Value = 100;
                await Task.Delay(200);
            }
            ShowUpdateSuccessMessage(vm, device);
            return await Task.FromResult(true);
        }

        public bool RefreshDeviceInfo(string comPort,
            out string deviceName,
            out string deviceID,
            out string deviceFirmware,
            out string deviceHardware,
            out int deviceHWL)
        {
            byte[] id = new byte[256];
            byte[] name = new byte[256];
            byte[] fw = new byte[256];
            byte[] hw = new byte[256];
            deviceName = null;
            deviceID = null;
            deviceFirmware = null;
            deviceHardware = null;
            deviceHWL = 0;
            var _serialPort = new SerialPort(comPort, 1000000);
            _serialPort.DtrEnable = true;
            _serialPort.ReadTimeout = 5000;
            _serialPort.WriteTimeout = 1000;
            try
            {
                _serialPort.Open();
            }
            catch (UnauthorizedAccessException)
            {
                return false;

            }
            catch (Exception ex)
            {
                HandyControl.Controls.MessageBox.Show(adrilight_shared.Properties.Resources.DeviceAdvanceSettingsViewModel_RefreshFirmwareVersion_Disconnect,
                    adrilight_shared.Properties.Resources.DeviceAdvanceSettingsViewModel_RefreshFirmwareVersion_DeviceDisconnect_header,
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            //write request info command
            _serialPort.Write(requestCommand, 0, 4);
            int retryCount = 0;
            int offset = 0;
            int idLength = 0; // Expected response length of valid deviceID 
            int nameLength = 0; // Expected response length of valid deviceName 
            int fwLength = 0;
            int hwLength = 0;
            while (offset < 3)
            {


                try
                {
                    byte header = (byte)_serialPort.ReadByte();
                    if (header == expectedValidHeader[offset])
                    {
                        offset++;
                    }
                }
                catch (TimeoutException)// retry until received valid header
                {
                    _serialPort.Write(requestCommand, 0, 4);
                    retryCount++;
                    if (retryCount == 3)
                    {
                        Console.WriteLine("timeout waiting for respond on serialport " + _serialPort.PortName);
                        HandyControl.Controls.MessageBox.Show("Thiết bị ở " + _serialPort.PortName + "Không có thông tin về Firmware, vui lòng liên hệ Ambino trước khi cập nhật firmware thủ công", "Device is not responding", MessageBoxButton.OK, MessageBoxImage.Warning);
                        break;
                    }
                    Debug.WriteLine("no respond, retrying...");
                    return false;
                }


            }
            if (offset == 3) //3 bytes header are valid
            {
                idLength = (byte)_serialPort.ReadByte();
                int count = idLength;
                id = new byte[count];
                while (count > 0)
                {
                    var readCount = _serialPort.Read(id, 0, count);
                    offset += readCount;
                    count -= readCount;
                }


                deviceID = BitConverter.ToString(id).Replace('-', ' ');
            }
            if (offset == 3 + idLength) //3 bytes header are valid
            {
                nameLength = (byte)_serialPort.ReadByte();
                int count = nameLength;
                name = new byte[count];
                while (count > 0)
                {
                    var readCount = _serialPort.Read(name, 0, count);
                    offset += readCount;
                    count -= readCount;
                }
                deviceName = Encoding.ASCII.GetString(name, 0, name.Length);
            }
            if (offset == 3 + idLength + nameLength) //3 bytes header are valid
            {
                fwLength = (byte)_serialPort.ReadByte();
                int count = fwLength;
                fw = new byte[count];
                while (count > 0)
                {
                    var readCount = _serialPort.Read(fw, 0, count);
                    offset += readCount;
                    count -= readCount;
                }
                deviceFirmware = Encoding.ASCII.GetString(fw, 0, fw.Length);
            }
            if (offset == 3 + idLength + nameLength + fwLength) //3 bytes header are valid
            {
                try
                {
                    hwLength = (byte)_serialPort.ReadByte();
                    int count = hwLength;
                    hw = new byte[count];
                    while (count > 0)
                    {
                        var readCount = _serialPort.Read(hw, 0, count);
                        offset += readCount;
                        count -= readCount;
                    }
                    deviceHardware = Encoding.ASCII.GetString(hw, 0, hw.Length);
                }
                catch (TimeoutException)
                {
                    Log.Information(deviceName, "Unknown Firmware Version");
                    deviceHardware = "unknown";
                }

            }
            if (offset == 3 + idLength + nameLength + fwLength + hwLength) //3 bytes header are valid
            {
                try
                {
                    deviceHWL = _serialPort.ReadByte();
                }
                catch (TimeoutException)
                {
                    Log.Information(name.ToString(), "Unknown Hardware Lighting Version");
                }

            }

            _serialPort.Close();
            _serialPort.Dispose();
            return true;
        }
        #endregion
        #region Ambino Protocol
        /// Hardware Lighting Protocol version 0
        //+----------+---------------------+------------+---------------+
        //| Position | Name                | Size(byte) | Default Value |
        //+----------+---------------------+------------+---------------+
        //| 0        | HWL_enable          | 1          | 1             |
        //| 1        | StatusLEDEnable     | 1          | 0             |
        //| 2        | HWL_tbd             | 1          | 255           |
        //| 3        | HWL_tbd             | 1          | 255           |
        //| 4        | HWL_tbd             | 1          | 255           |
        //| 5        | HWL_tbd             | 1          | 255           |
        //| 6        | HWF_FanSpeed        | 1          | 127           |
        //| 7        | HWL_tbd             | 1          | 255           |
        //| 8        | HWL_tbd             | 1          | 255           |
        //| 9        | HWL_tbd             | 1          | 255           |
        //| 10       | HWL_tbd             | 1          | 255           |
        //| 11       | HWL_tbd             | 1          | 255           |
        //| 12       | HWL_tbd             | 1          | 255           |
        //+----------+---------------------+------------+---------------+
        #endregion
    }
}
