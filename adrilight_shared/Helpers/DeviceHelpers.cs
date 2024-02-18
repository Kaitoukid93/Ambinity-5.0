using adrilight_shared.Enums;
using adrilight_shared.Models.Device;
using adrilight_shared.Models.Device.Output;
using adrilight_shared.Models.Device.SlaveDevice;
using Newtonsoft.Json;
using Serilog;
using System;
using System.IO;
using System.Threading;

namespace adrilight_shared.Helpers
{
    public class DeviceHelpers
    {
        private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "adrilight\\");
        private string DevicesCollectionFolderPath => Path.Combine(JsonPath, "Devices");
        private static ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();
        public void WriteSingleDeviceInfoJson(IDeviceSettings device)
        {
            foreach (var controller in device.AvailableControllers)
            {
                foreach (var output in controller.Outputs)
                {
                    WriteSingleOutputInfoJson(output, device);
                    WriteSingleSlaveDeviceInfoJson(output.SlaveDevice, output, device);

                }
            }
            var directory = Path.Combine(DevicesCollectionFolderPath, device.DeviceName + "-" + device.DeviceUID);
            if (!Directory.Exists(directory))
                return;
            var fileToWrite = Path.Combine(directory, "config.json");
            var json = JsonConvert.SerializeObject(device);

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
                case OutputTypeEnum.PWMOutput:
                    outputDirectory = Path.Combine(parrentDirectory, "PWmOutputs", output.OutputName + "_" + output.OutputID.ToString());
                    break;

                case OutputTypeEnum.ARGBLEDOutput:
                    outputDirectory = Path.Combine(parrentDirectory, "LightingOutputs", output.OutputName + "_" + output.OutputID.ToString());
                    break;
            }
            if (!Directory.Exists(outputDirectory))
                return;
            var fileToWrite = Path.Combine(outputDirectory, "AttachedDevice", "config.json");
            var json = JsonConvert.SerializeObject(slaveDevice);

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
        public void RemoveDeviceLocalData(IDeviceSettings device)
        {
            var directory = Path.Combine(DevicesCollectionFolderPath, device.DeviceName + "-" + device.DeviceUID);
            if (!Directory.Exists(directory))
                return;
            try
            {
                Directory.Delete(directory, true);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
            
        }
        public void WriteSingleOutputInfoJson(IOutputSettings output, IDeviceSettings parrent)
        {
            var parrentDirectory = Path.Combine(DevicesCollectionFolderPath, parrent.DeviceName + "-" + parrent.DeviceUID);
            string childDirectory = " ";
            switch (output.OutputType)
            {
                case OutputTypeEnum.PWMOutput:
                    childDirectory = Path.Combine(parrentDirectory, "PWmOutputs", output.OutputName + "_" + output.OutputID.ToString());
                    break;

                case OutputTypeEnum.ARGBLEDOutput:
                    childDirectory = Path.Combine(parrentDirectory, "LightingOutputs", output.OutputName + "_" + output.OutputID.ToString());
                    break;
            }
            if (!Directory.Exists(childDirectory))
                return;
            var fileToWrite = Path.Combine(childDirectory, "config.json");
            var json = JsonConvert.SerializeObject(output);

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
    }

}