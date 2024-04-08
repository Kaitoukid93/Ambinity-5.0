using adrilight_shared.Enums;
using adrilight_shared.Helpers;
using adrilight_shared.Models.Device;
using Microsoft.Win32;
using Semver;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Xml.Linq;
using Windows.Devices.Input;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http.Headers;

namespace adrilight.Util
{
    internal class SerialDeviceDetection
    {


        private static byte[] requestCommand = { (byte)'d', (byte)'i', (byte)'r' };
        private static byte[] expectedValidHeader = { 15, 12, 93 };
        private static byte[] settingCommand = { (byte)'h', (byte)'s', (byte)'d' };
        private static byte[] unexpectedValidHeader = { (byte)'A', (byte)'b', (byte)'n' };
        private static CancellationToken cancellationtoken;
        private static bool isNoRespondingMessageShowed = false;
        private static List<IDeviceSettings> ExistedSerialDevice { get; set; }
        //private static List<DeviceHUB> ExistedDeviceHUB { get; set; }

        public SerialDeviceDetection(List<IDeviceSettings> existedSerialDevice)
        {
            ExistedSerialDevice = existedSerialDevice;
        }



        static List<string> GetComPortByID(String VID, String PID)
        {
            String pattern = String.Format("^VID_{0}.PID_{1}", VID, PID);
            Regex _rx = new Regex(pattern, RegexOptions.IgnoreCase);
            List<string> comports = new List<string>();
            RegistryKey rk1 = Registry.LocalMachine;
            RegistryKey rk2 = rk1.OpenSubKey("SYSTEM\\CurrentControlSet\\Enum");
            foreach (String s3 in rk2.GetSubKeyNames())
            {
                RegistryKey rk3 = rk2.OpenSubKey(s3);
                foreach (String s in rk3.GetSubKeyNames())
                {
                    if (_rx.Match(s).Success)
                    {
                        RegistryKey rk4 = rk3.OpenSubKey(s);
                        foreach (String s2 in rk4.GetSubKeyNames())
                        {
                            RegistryKey rk5 = rk4.OpenSubKey(s2);
                            RegistryKey rk6 = rk5.OpenSubKey("Device Parameters");
                            string portName = (string)rk6.GetValue("PortName");
                            if (!String.IsNullOrEmpty(portName) && SerialPort.GetPortNames().Contains(portName))
                            {
                                comports.Add((string)rk6.GetValue("PortName"));
                            }
                        }
                    }
                }

            }
            return comports;
        }
        /// <summary>
        /// this return any availabe port that is not in use by the app
        /// </summary>
        /// <returns></returns>
        public static List<string> GetValidDevice()
        {
            List<string> CH55X = GetComPortByID("1209", "c550");
            List<string> CH340 = GetComPortByID("1A86", "7522");
            List<string> ada = GetComPortByID("239A", "CAFE");
            var devices = new List<string>();
            List<string> sd = GetComPortByID("1209", "c55c");
            if (CH55X.Count > 0 || CH340.Count > 0 || ada.Count > 0)
            {
                foreach (var port in CH55X)
                {
                    if (ExistedSerialDevice.Any(d => d.OutputPort == port && d.IsTransferActive == true))
                        continue;
                    devices.Add(port);
                }
                foreach (var port in CH340)
                {
                    if (ExistedSerialDevice.Any(d => d.OutputPort == port && d.IsTransferActive == true))
                        continue;
                    devices.Add(port);
                }
                foreach (var port in ada)
                {
                    if (ExistedSerialDevice.Any(d => d.OutputPort == port && d.IsTransferActive == true))
                        continue;
                    devices.Add(port);
                }
            }
            else
            {
                Log.Warning("No Compatible Device Detected");
            }
            return devices;
        }
        /// <summary>
        /// this return a new constructed device with the information read from comPort
        /// </summary>
        /// <param name="comPort"></param>
        /// <returns></returns>
        private static DeviceSettings GetDeviceInformation(string comPort)
        {
            var newDevice = new DeviceSettings();
            string id = "000000";
            string name = "unknown";
            string fw = "unknown";
            string hw = "unknown";
            bool isValid = true;
            var _serialPort = new SerialPort(comPort, 1000000);
            _serialPort.ReadTimeout = 5000;
            _serialPort.WriteTimeout = 1000;
            _serialPort.DtrEnable = true;
            try
            {
                _serialPort.Open();
            }
            catch (Exception ex)
            {
                // Log.Error(ex,"AcessDenied " + _serialPort.PortName);
                Log.Error(_serialPort.PortName + " is removed");
                Thread.Sleep(2000);
                return null;
            }

        //write request info command
        retry:
            try
            {
                _serialPort.Write(requestCommand, 0, 3);
                _serialPort.WriteLine("\r\n");
            }

            catch (System.IO.IOException ex)// retry until received valid header
            {
                Log.Warning("This Device seems to have Ambino PID/VID but not an USB device " + _serialPort.PortName);
                return null;
            }
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
                    else if (header == unexpectedValidHeader[offset])
                    {
                        offset++;
                        if (offset == 3)
                        {
                            Log.Information("Old Ambino Device at" + _serialPort.PortName + ". Restarting Firmware Request");
                            goto retry;
                        }
                    }
                }
                catch (TimeoutException ex)// retry until received valid header
                {
                    _serialPort.Write(requestCommand, 0, 3);
                    _serialPort.WriteLine("\r\n");
                    retryCount++;
                    if (retryCount == 3)
                    {
                        Log.Warning("timeout waiting for respond on serialport " + _serialPort.PortName);
                        if (!isNoRespondingMessageShowed)
                        {
                            isNoRespondingMessageShowed = true;
                            HandyControl.Controls.MessageBox.Show("Device at " + _serialPort.PortName + " is not responding, try adding it manually", "Device is not responding", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        _serialPort.Close();
                        _serialPort.Dispose();
                        return null;
                    }
                }
                catch (System.IO.IOException ex)// retry until received valid header
                {
                    return null;
                }

            }
            if (offset == 3) //3 bytes header are valid
            {
                idLength = (byte)_serialPort.ReadByte();
                int count = idLength;
                var d = new byte[count];
                while (count > 0)
                {
                    var readCount = _serialPort.Read(d, 0, count);
                    offset += readCount;
                    count -= readCount;
                }
                id = BitConverter.ToString(d);
            }

            if (offset == 3 + idLength) //3 bytes header are valid
            {
                nameLength = (byte)_serialPort.ReadByte();
                int count = nameLength;
                var n = new byte[count];
                while (count > 0)
                {
                    var readCount = _serialPort.Read(n, 0, count);
                    offset += readCount;
                    count -= readCount;
                }
                name = Encoding.ASCII.GetString(n, 0, n.Length);

            }
            if (offset == 3 + idLength + nameLength) //3 bytes header are valid
            {
                fwLength = (byte)_serialPort.ReadByte();
                int count = fwLength;
                var f = new byte[count];
                while (count > 0)
                {
                    var readCount = _serialPort.Read(f, 0, count);
                    offset += readCount;
                    count -= readCount;
                }
                fw = Encoding.ASCII.GetString(f, 0, f.Length);
            }
            if (offset == 3 + idLength + nameLength + fwLength) //3 bytes header are valid
            {
                try
                {
                    hwLength = (byte)_serialPort.ReadByte();
                    int count = hwLength;
                    var h = new byte[count];
                    while (count > 0)
                    {
                        var readCount = _serialPort.Read(h, 0, count);
                        offset += readCount;
                        count -= readCount;
                    }
                    hw = Encoding.ASCII.GetString(h, 0, h.Length);
                }
                catch (TimeoutException)
                {
                    Log.Information(name.ToString(), "Unknown Hardware Version");
                }

            }
            //construct new device
            var dev = DeviceConstruct(name, id, fw, hw, comPort);
            _serialPort.Close();
            _serialPort.Dispose();
            return dev;
        }
        /// <summary>
        /// Construct device from basic information
        /// </summary>
        /// <param name="name"></param>
        /// <param name="deviceSerial"></param>
        /// <param name="fwVersion"></param>
        /// <param name="hwVersion"></param>
        /// <param name="outputPort"></param>
        /// <returns></returns>
        private static DeviceSettings DeviceConstruct(string name, string deviceSerial, string fwVersion, string hwVersion, string outputPort)
        {
            var ledSetupHlprs = new LEDSetupHelpers();
            var newDevice = new DeviceSettings();
            newDevice.DeviceName = name;
            switch (newDevice.DeviceName)
            {
                case "Ambino Basic":// General Ambino Basic USB Device
                                    //newDevice = availableDefaultDevice.AmbinoBasic24Inch;

                    newDevice = new SlaveDeviceHelpers().DefaultCreatedAmbinoDevice(
                        new DeviceType(DeviceTypeEnum.AmbinoBasic),
                        newDevice.DeviceName,
                        outputPort,
                        false,
                        true,
                        1);
                    newDevice.DashboardWidth = 230;
                    newDevice.DashboardHeight = 270;
                    break;
                case "Ambino EDGE":// General Ambino Edge USB Device
                    newDevice = new SlaveDeviceHelpers().DefaultCreatedAmbinoDevice(
                     new DeviceType(DeviceTypeEnum.AmbinoEDGE),
                     newDevice.DeviceName,
                     outputPort,
                     false,
                     true,
                     1);
                    newDevice.DashboardWidth = 230;
                    newDevice.DashboardHeight = 270;

                    break;
                case "Ambino FanHub":
                    newDevice = new SlaveDeviceHelpers().DefaultCreatedAmbinoDevice(
                       new DeviceType(DeviceTypeEnum.AmbinoFanHub),
                    newDevice.DeviceName,
                       outputPort,
                       true,
                       true,
                       10);
                    newDevice.DashboardWidth = 472;
                    newDevice.DashboardHeight = 270;
                    break;
                case "Ambino HubV2":
                    newDevice = new SlaveDeviceHelpers().DefaultCreatedAmbinoDevice(
                     new DeviceType(DeviceTypeEnum.AmbinoHUBV2),
                  newDevice.DeviceName,
                     outputPort,
                     false,
                     true,
                     7);
                    newDevice.DashboardWidth = 320;
                    newDevice.DashboardHeight = 270;
                    break;
                case "Ambino HubV3":
                    newDevice = new SlaveDeviceHelpers().DefaultCreatedAmbinoDevice(
                      new DeviceType(DeviceTypeEnum.AmbinoHUBV3),
                   newDevice.DeviceName,
                      outputPort,
                      false,
                      true,
                      4);
                    newDevice.DashboardWidth = 320;
                    newDevice.DashboardHeight = 270;
                    break;
                case "Ambino RainPow":
                    //newDevice = availableDefaultDevice.ambinoRainPow;
                    //newDevice.DeviceType = DeviceTypeEnum.AmbinoRainPowPro;
                    //newDevice.DeviceConnectionType = "wired";
                    //newDevice.OutputPort = device;
                    //newDevice.IsSizeNeedUserDefine = true;
                    break;
            }

            newDevice.DeviceSerial = deviceSerial;
            newDevice.FirmwareVersion = fwVersion;
            newDevice.HardwareVersion = hwVersion;
            newDevice.UpdateChildSize();
            return newDevice;
        }
        /// <summary>
        /// this return list of new devices and old devices
        /// </summary>
        /// <returns></returns>
        public (List<IDeviceSettings>, List<IDeviceSettings>) DetectedDevices()
        {
            List<IDeviceSettings> newDevices = new List<IDeviceSettings>();
            List<IDeviceSettings> existedDevices = new List<IDeviceSettings>();
            var validDevices = GetValidDevice();
            foreach (var comPort in validDevices)
            {

                var dev = GetDeviceInformation(comPort);
                if (dev != null)
                {
                    //check if device exist
                    var serial = dev.DeviceSerial;
                    var altSerial = dev.DeviceSerial.Replace("-", " ");
                    var type = dev.DeviceType.Type;
                    var matchDev = ExistedSerialDevice.Where(d => d.DeviceSerial == serial || d.DeviceSerial == altSerial).FirstOrDefault();
                    if (matchDev != null)
                    {
                        if (matchDev.OutputPort != comPort)
                        {
                            Log.Warning("Device COMPort changed from " + matchDev.OutputPort + " to " + comPort);
                            matchDev.OutputPort = comPort;

                        }
                        if (matchDev.DeviceType.Type != type)
                        {
                            Log.Warning("Device Type changed from " + matchDev.DeviceType.Name + " to " + dev.DeviceType.Name);
                            matchDev.DeviceSerial = null;
                            newDevices.Add(dev);
                        }
                        else
                            existedDevices.Add(matchDev);
                    }
                    else
                    {
                        newDevices.Add(dev);
                    }
                }
                else
                {
                    Log.Error("Can not get device information: " + comPort);
                    continue;
                }


            }
            return (newDevices, existedDevices);

        }
    }
}
//+-------------------------------------------------+--------+----+----+-------+
//| Ambino Basic CH552P without PowerLED Support    | CH552P | 32 | 14 | ABR1p |
//+-------------------------------------------------+--------+----+----+-------+
//| Ambino Basic CH552E Without PowerLED Support    | CH552E | 15 | 17 | ABR1e |
//+-------------------------------------------------+--------+----+----+-------+
//| Ambino EDGE CH552E Without PowerLED Support     | CH552E | 15 | 17 | AER1e |
//+-------------------------------------------------+--------+----+----+-------+
//| Ambino Basic CH552P(rev2) With PowerLED Support | CH552P |    |    | ABR2p |
//+-------------------------------------------------+--------+----+----+-------+
//| Ambino EDGE CH552P (rev2) With PowerLED Support | CH552P |    |    | AER2p |
//+-------------------------------------------------+--------+----+----+-------+
//| Ambino FanHUB CH552G rev1                       | CH552G |    |    | AFR1g |
//+-------------------------------------------------+--------+----+----+-------+
//| Ambino HUBV3 CH552G rev1                        | CH552G |    |    | AHR1g |
//+-------------------------------------------------+--------+----+----+-------+
//| Ambino RainPow CH552P rev1                      | CH552P |    |    | ARR1p |
//+-------------------------------------------------+--------+----+----+-------+