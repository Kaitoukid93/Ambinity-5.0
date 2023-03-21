﻿using adrilight.Settings;
using adrilight.Spots;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace adrilight.Util
{
    internal class SerialDeviceDetection : ISerialDeviceDetection
    {


        private static byte[] requestCommand = { (byte)'d', (byte)'i', (byte)'r' };
        private static byte[] expectedValidHeader = { 15, 12, 93 };
        private static byte[] unexpectedValidHeader = { (byte)'A', (byte)'b', (byte)'n' };
        private static CancellationToken cancellationtoken;
        private static List<IDeviceSettings> ExistedSerialDevice { get; set; }

        public SerialDeviceDetection(List<IDeviceSettings> existedSerialDevice)
        {
            ExistedSerialDevice = existedSerialDevice;
        }


        public static List<string> ValidDevice()
        {
            List<string> CH55X = ComPortNames("1209", "c550");
            List<string> CH340 = ComPortNames("1A86", "7522");
            List<string> devices = new List<string>();
            if (CH55X.Count > 0 || CH340.Count > 0)
            {
                int counter = 0;
                foreach (String s in SerialPort.GetPortNames())
                {
                    if (CH55X.Contains(s) || CH340.Contains(s))
                    {
                        //if (!ExistedSerialDevice.Any(p => p.OutputPort == s)) // if this comport is not used by any of the existed serial device
                        //{
                        counter++;
                        devices.Add(s);
                        //}


                    }


                }
            }
            else
            {
                Console.WriteLine("Không tìm thấy thiết bị nào của Ambino, hãy thêm thiết bị theo cách thủ công");
                // return null;
            }
            return devices;
        }



        //static async Task SearchingForDevice(CancellationToken cancellationToken)
        //{
        //    var jobTask = Task.Run(() => {
        //        // Organize critical sections around logical serial port operations somehow.
        //        lock (_syncRoot)
        //        {
        //            return RequestDeviceInformation();
        //        }
        //    });
        //    if (jobTask != await Task.WhenAny(jobTask, Task.Delay(Timeout.Infinite, cancellationToken)))
        //    {
        //        // Timeout;
        //        return;
        //    }
        //    var newDevices = await jobTask;
        //    foreach(var device in newDevices)
        //    {
        //        Console.WriteLine("Name: " + device.DeviceName);
        //        Console.WriteLine("ID: " + device.DeviceSerial);
        //        Console.WriteLine("Firmware Version: " + device.FirmwareVersion);
        //        Console.WriteLine();
        //        Console.WriteLine();
        //        Console.WriteLine();
        //    }



        //    // Process response.
        //}
        public List<IDeviceSettings> DetectedDevices {
            get
            {
                return RequestDeviceInformation();
            }
        }
        static List<IDeviceSettings> RequestDeviceInformation()
        {
            // Assume serial port timeouts are set.
            byte[] id;
            byte[] name;
            byte[] fw;
            byte[] hw;
            List<IDeviceSettings> newDevices = new List<IDeviceSettings>();


            foreach (var device in ValidDevice())
            {

                bool isValid = true;
                var _serialPort = new SerialPort(device, 1000000);
                _serialPort.DtrEnable = true;
                _serialPort.ReadTimeout = 5000;
                _serialPort.WriteTimeout = 1000;
                try
                {
                    _serialPort.Open();
                }
                catch (Exception)
                {
                    continue;
                }

                //write request info command
                _serialPort.Write(requestCommand, 0, 3);
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
                                Console.WriteLine("Old Ambino Device at" + _serialPort.PortName);
                                HandyControl.Controls.MessageBox.Show("Thiết bị ở " + _serialPort.PortName + " đang chạy firmware cũ, vui lòng cập nhật firmware để sử dụng ổn định hơn", "Old Device detected", MessageBoxButton.OK, MessageBoxImage.Warning);
                                isValid = false;
                                break;
                            }
                        }
                    }
                    catch (TimeoutException)// retry until received valid header
                    {
                        _serialPort.Write(requestCommand, 0, 3);
                        retryCount++;
                        if (retryCount == 3)
                        {
                            Console.WriteLine("timeout waiting for respond on serialport " + _serialPort.PortName);
                            HandyControl.Controls.MessageBox.Show("Device at " + _serialPort.PortName + "is not responding, try adding it manually", "Device is not responding", MessageBoxButton.OK, MessageBoxImage.Warning);
                            isValid = false;
                            break;
                        }
                        Debug.WriteLine("no respond, retrying...");
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


                    newDevice.DeviceSerial = BitConverter.ToString(id).Replace('-', ' ');
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
                            newDevice.DeviceName = "Ambino Basic";
                            newDevice.DeviceUID = Guid.NewGuid().ToString();
                            newDevice.DeviceType = "ABBASIC";
                            newDevice.DeviceConnectionType = "wired";
                            newDevice.OutputPort = device;
                            newDevice.IsSizeNeedUserDefine = true;
                            newDevice.AvailableControllers = new DeviceController[2];
                            newDevice.AvailableControllers[0] = new DeviceController() { Description = "LED Controller", Name = "Lighting",Geometry="brightness", Executioner = "CH552G", Type = ControllerTypeEnum.LightingController };
                            newDevice.AvailableControllers[0].Outputs = new LightingOutput[1];
                            newDevice.AvailableControllers[0].Outputs[0] = new LightingOutput() { OutputID = 0, SlaveDevice = new ARGBLEDSlaveDevice() {Name="Ambino Basic Default",ControlableZones = new LEDSetup[4] },OutputName ="Ambino 3P 2.54mm", };
                            //set a default output for this device , simply plugin 4 LED strips with led number of 5-12
                            //build leds for zone 0
                            newDevice.AvailableControllers[0].Outputs[0].SlaveDevice.ControlableZones[0] = ledSetupHlprs.BuildLEDSetup(1, 5, "Cạnh phải", 50.0, 250.0);
                            newDevice.AvailableControllers[0].Outputs[0].SlaveDevice.ControlableZones[1] = ledSetupHlprs.BuildLEDSetup(12, 1, "Cạnh trên", 600.0, 50.0);
                            newDevice.AvailableControllers[0].Outputs[0].SlaveDevice.ControlableZones[2] = ledSetupHlprs.BuildLEDSetup(1, 5, "Cạnh trái", 50.0,250.0);
                            newDevice.AvailableControllers[0].Outputs[0].SlaveDevice.ControlableZones[3] = ledSetupHlprs.BuildLEDSetup(12, 1, "Cạnh dưới", 600, 50.0);
                            newDevice.AvailableControllers[1] = new DeviceController() { Description = "PWM Controller", Name = "Fan Speed", Geometry = "fanspeed", Executioner = "CH552G", Type = ControllerTypeEnum.PWMController };
                            newDevice.AvailableControllers[1].Outputs = new PWMOutput[1];
                            newDevice.AvailableControllers[1].Outputs[0] = new PWMOutput() { OutputID = 0, SlaveDevice = new PWMMotorSlaveDevice() { Name = "Ambino A1", ControlableZones = new FanMotor[1] }, OutputName = "Ambino 6p 2.54mm" };
                            newDevice.AvailableControllers[1].Outputs[0].SlaveDevice.ControlableZones[0] = new FanMotor() { Name = "AmbinoA1", FanSpiningAnimationPath = "I:\\Ambinity\\AmbinityWPF\\adrilight\\adrilight\\Animation\\splashLoading.json", Width=300,Height=300 };
                            newDevice.UpdateChildSize();
                            //this device contains 4 zones

                            break;
                        case "Ambino EDGE":// General Ambino Edge USB Device
                            //newDevice = availableDefaultDevice.AmbinoEDGE1M2;
                            newDevice.DeviceName = "Ambino EDGE";
                            newDevice.DeviceConnectionType = "wired";
                            newDevice.DeviceType = "ABEDGE";
                            newDevice.OutputPort = device;
                            newDevice.IsSizeNeedUserDefine = true;

                            break;
                        case "Ambino FanHub":
                            //newDevice = availableDefaultDevice.ambinoFanHub;
                            newDevice.DeviceType = "ABFANHUB";
                            newDevice.DeviceConnectionType = "wired";
                            newDevice.OutputPort = device;
                            newDevice.IsSizeNeedUserDefine = true;
                            //newDevice.AvailableOutputs = new IOutputSettings[10];
                            ////set a default output for this device , simply plugin LED strip with led number of 16
                            //newDevice.AvailableOutputs[0] = new LightingOutput() { OutputID = 0, ControlableZone = new LEDSetup[1] };
                            //newDevice.AvailableOutputs[1] = new LightingOutput() { OutputID = 1, ControlableZone = new LEDSetup[1] };
                            //newDevice.AvailableOutputs[2] = new LightingOutput() { OutputID = 2, ControlableZone = new LEDSetup[1] };
                            //newDevice.AvailableOutputs[3] = new LightingOutput() { OutputID = 3, ControlableZone = new LEDSetup[1] };
                            //newDevice.AvailableOutputs[4] = new LightingOutput() { OutputID = 4, ControlableZone = new LEDSetup[1] };
                            //newDevice.AvailableOutputs[5] = new LightingOutput() { OutputID = 5, ControlableZone = new LEDSetup[1] };
                            //newDevice.AvailableOutputs[6] = new LightingOutput() { OutputID = 6, ControlableZone = new LEDSetup[1] };
                            //newDevice.AvailableOutputs[7] = new LightingOutput() { OutputID = 7, ControlableZone = new LEDSetup[1] };
                            //newDevice.AvailableOutputs[8] = new LightingOutput() { OutputID = 8, ControlableZone = new LEDSetup[1] };
                            //newDevice.AvailableOutputs[9] = new LightingOutput() { OutputID = 9, ControlableZone = new LEDSetup[1] };
                            ////build leds for zones
                            //newDevice.AvailableOutputs[0].ControlableZone[0] = ledSetupHlprs.BuildLEDSetup(16, 1, "Fan", 160.0, 10.0);
                            //newDevice.AvailableOutputs[1].ControlableZone[0] = ledSetupHlprs.BuildLEDSetup(16, 1, "Fan", 160.0, 10.0);
                            //newDevice.AvailableOutputs[2].ControlableZone[0] = ledSetupHlprs.BuildLEDSetup(16, 1, "Fan", 160.0, 10.0);
                            //newDevice.AvailableOutputs[3].ControlableZone[0] = ledSetupHlprs.BuildLEDSetup(16, 1, "Fan", 160.0, 10.0);
                            //newDevice.AvailableOutputs[4].ControlableZone[0] = ledSetupHlprs.BuildLEDSetup(16, 1, "Fan", 160.0, 10.0);
                            //newDevice.AvailableOutputs[5].ControlableZone[0] = ledSetupHlprs.BuildLEDSetup(16, 1, "Fan", 160.0, 10.0);
                            //newDevice.AvailableOutputs[6].ControlableZone[0] = ledSetupHlprs.BuildLEDSetup(16, 1, "Fan", 160.0, 10.0);
                            //newDevice.AvailableOutputs[7].ControlableZone[0] = ledSetupHlprs.BuildLEDSetup(16, 1, "Fan", 160.0, 10.0);
                            //newDevice.AvailableOutputs[8].ControlableZone[0] = ledSetupHlprs.BuildLEDSetup(16, 1, "Fan", 160.0, 10.0);
                            //newDevice.AvailableOutputs[9].ControlableZone[0] = ledSetupHlprs.BuildLEDSetup(16, 1, "Fan", 160.0, 10.0);




                            break;
                        case "Ambino HubV2":
                            //newDevice = availableDefaultDevice.ambinoHUBV2;
                            newDevice.DeviceType = "ABHUBV2";
                            newDevice.DeviceConnectionType = "wired";
                            newDevice.OutputPort = device;
                            newDevice.IsSizeNeedUserDefine = true;
                            break;
                        case "Ambino HubV3":
                            //newDevice = availableDefaultDevice.ambinoHUBV3;
                            newDevice.DeviceType = "ABHUBV3";
                            newDevice.DeviceConnectionType = "wired";
                            newDevice.OutputPort = device;
                            newDevice.IsSizeNeedUserDefine = true;
                            break;
                        case "Ambino RainPow":
                            //newDevice = availableDefaultDevice.ambinoRainPow;
                            newDevice.DeviceType = "ABRP";
                            newDevice.DeviceConnectionType = "wired";
                            newDevice.OutputPort = device;
                            newDevice.IsSizeNeedUserDefine = true;
                            break;
                    }

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
                        newDevice.HardwareVersion = "unknown";
                    }

                }
                _serialPort.Close();
                _serialPort.Dispose();
                if (isValid)
                    newDevices.Add(newDevice);

            }

            return newDevices;

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
                            comports.Add((string)rk6.GetValue("PortName"));
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