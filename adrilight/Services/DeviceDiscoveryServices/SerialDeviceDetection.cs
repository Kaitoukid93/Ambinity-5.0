using adrilight_shared.Enums;
using adrilight_shared.Helpers;
using adrilight_shared.Models.Device;
using Microsoft.Win32;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;

namespace adrilight.Util
{
    internal class SerialDeviceDetection
    {


        private static byte[] requestCommand = { (byte)'d', (byte)'i', (byte)'r' };
        private static byte[] expectedValidHeader = { 15, 12, 93 };
        private static byte[] unexpectedValidHeader = { (byte)'A', (byte)'b', (byte)'n' };
        private static CancellationToken cancellationtoken;
        private static bool isNoRespondingMessageShowed = false;
        private static List<IDeviceSettings> ExistedSerialDevice { get; set; }

        public SerialDeviceDetection(List<IDeviceSettings> existedSerialDevice)
        {
            ExistedSerialDevice = existedSerialDevice;
            Log.Information("SerialDetection");
        }


        public static List<string> ValidDevice()
        {
            List<string> CH55X = ComPortNames("1209", "c550");
            List<string> CH340 = ComPortNames("1A86", "7522");
            List<string> devices = new List<string>();
            if (CH55X.Count > 0 || CH340.Count > 0)
            {
                foreach (var port in CH55X)
                {
                    devices.Add(port);
                }
                foreach (var port in CH340)
                {
                    devices.Add(port);
                }
                //int counter = 0;
                //foreach (String s in SerialPort.GetPortNames())
                //{
                //    if (CH55X.Contains(s) || CH340.Contains(s))
                //    {
                //        counter++;
                //        devices.Add(s);
                //    }
                //}
            }
            else
            {
                Log.Warning("No Compatible Device Detected");
            }
            return devices;
        }

        public static ResourceHelpers ResourceHlprs { get; private set; }
        public (List<IDeviceSettings>, List<string>) DetectedDevices {
            get
            {
                return RequestDeviceInformation();
            }
        }
        static (List<IDeviceSettings>, List<string>) RequestDeviceInformation()
        {

            if (ResourceHlprs == null)
            {
                ResourceHlprs = new ResourceHelpers();
            }
            // Assume serial port timeouts are set.
            byte[] id = new byte[8];
            byte[] name;
            byte[] fw;
            byte[] hw;
            List<IDeviceSettings> newDevices = new List<IDeviceSettings>();
            List<string> existedDevices = new List<string>();
            foreach (var device in ValidDevice())
            {
                if (ExistedSerialDevice.Any(d => d.OutputPort == device))
                {
                    var sp = new SerialPort(device, 1000000, Parity.Odd, 8, StopBits.One);
                    try
                    {
                        sp.Open();
                        Thread.Sleep(500);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        //Log.Error("device is in use found : " + device);
                        continue;
                    }
                    catch (IOException)
                    {
                        Log.Error("Device is disconnected : " + device);
                        continue;
                    }
                    finally
                    {
                        sp.Close();
                    }
                    existedDevices.Add(device);
                    continue;
                }

                bool isValid = true;
                var _serialPort = new SerialPort(device, 1000000);
                _serialPort.DtrEnable = true;
                _serialPort.ReadTimeout = 5000;
                _serialPort.WriteTimeout = 1000;
                try
                {
                    _serialPort.Open();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "AcessDenied" + _serialPort.PortName);
                    continue;
                }

            //write request info command
            retry:
                try
                {
                    _serialPort.Write(requestCommand, 0, 3);
                }

                catch (System.IO.IOException ex)// retry until received valid header
                {
                    Log.Warning("This Device seems to have Ambino PID/VID but not an USB device " + _serialPort.PortName);
                    break;
                }
                int retryCount = 0;
                int offset = 0;
                int idLength = 0; // Expected response length of valid deviceID 
                int nameLength = 0; // Expected response length of valid deviceName 
                int fwLength = 0;
                int hwLength = 0;
                IDeviceSettings newDevice = new DeviceSettings();
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
                        retryCount++;
                        if (retryCount == 3)
                        {
                            Log.Warning("timeout waiting for respond on serialport " + _serialPort.PortName);
                            if (!isNoRespondingMessageShowed)
                            {
                                isNoRespondingMessageShowed = true;
                                HandyControl.Controls.MessageBox.Show("Device at " + _serialPort.PortName + " is not responding, try adding it manually", "Device is not responding", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }

                            isValid = false;
                            break;
                        }
                    }
                    catch (System.IO.IOException ex)// retry until received valid header
                    {
                        Log.Warning("This Device seems to have Ambino PID/VID but not an USB device " + _serialPort.PortName);
                        break;
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
                    newDevice.DeviceName = Encoding.ASCII.GetString(name, 0, name.Length);
                    //get new instance of ledsetuphelpers
                    var ledSetupHlprs = new LEDSetupHelpers();
                    switch (newDevice.DeviceName)
                    {
                        case "Ambino Basic":// General Ambino Basic USB Device
                                            //newDevice = availableDefaultDevice.AmbinoBasic24Inch;

                            newDevice = new SlaveDeviceHelpers().DefaultCreatedAmbinoDevice(
                                new DeviceType(DeviceTypeEnum.AmbinoBasic),
                                newDevice.DeviceName,
                                device,
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
                             device,
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
                               device,
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
                             device,
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
                              device,
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
                    newDevice.UpdateChildSize();
                }
                newDevice.DeviceSerial = BitConverter.ToString(id);
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
                    newDevice.FirmwareVersion = Encoding.ASCII.GetString(fw, 0, fw.Length);
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
                        newDevice.HardwareVersion = Encoding.ASCII.GetString(hw, 0, hw.Length);
                    }
                    catch (TimeoutException)
                    {
                        Log.Information(newDevice.DeviceName, "Unknown Firmware Version");
                        newDevice.HardwareVersion = "unknown";
                    }

                }

                _serialPort.Close();
                _serialPort.Dispose();
                if (isValid)
                    newDevices.Add(newDevice);

            }

            return (newDevices, existedDevices);

        }
        static List<string> ComPortNames(String VID, String PID)
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