using Castle.Core.Logging;
using NLog;
using OpenRGB.NET;
using OpenRGB.NET.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.Util
{
    internal class OpenRGBClientDevice : IOpenRGBClientDevice
    {
        private NLog.ILogger _log = LogManager.GetCurrentClassLogger();
        public OpenRGBClientDevice()
        {
            //GeneralSettings = generalSettings ?? throw new ArgumentException(nameof(generalSettings));
            //DeviceSettings = deviceSettings ?? throw new ArgumentNullException(nameof(deviceSettings));
           
           // DeviceSettings.PropertyChanged += UserSettings_PropertyChanged;
            //RefreshOpenRGBDeviceState();

            //_log.Info($"SerialStream created.");


        }
        public OpenRGBClient AmbinityClient { get; set; }
        public bool IsInitialized { get; set; }
        public bool RefreshOpenRGBDeviceState()//init
        {
            IsInitialized = false;
            try
            {


                AmbinityClient = new OpenRGBClient( "127.0.0.1",6742,name: "Ambinity", autoconnect: true, timeout: 1000);
            //AmbinityClient = client;
                if(AmbinityClient != null)
                {
                    var deviceCount = AmbinityClient.GetControllerCount();
                    var devices = AmbinityClient.GetAllControllerData();
                    DeviceList = devices;
                    IsAvailable = true;
                    foreach (var device in devices)
                    {
                        _log.Info($"Device found : " + device.Name.ToString());
                    }

                }


                //for (int i = 0; i < devices.Length; i++)
                //{
                //    var leds = Enumerable.Range(0, devices[i].Colors.Length)
                //        .Select(_ => new Color(255, 0, 255))
                //        .ToArray();
                //    client.UpdateLeds(i, leds);
                //}
                IsInitialized = true;
            }
            catch(TimeoutException)
            {
                HandyControl.Controls.MessageBox.Show("OpenRGB server Không khả dụng, hãy start server trong app OpenRGB (SDK Server)");
                //IsAvailable= false;
                
            }
            return true;
        }

        private Device[] _deviceList;
        public Device[] DeviceList 
        {
            get { return _deviceList; }
            set
            {
                _deviceList = value;
            }

        }
         private bool _isAvailable;
        public bool IsAvailable 
        {
            get { return _isAvailable; }
            set
            {
                _isAvailable = value;
            }

        }
        //private IDeviceSettings DeviceSettings { get; }
        //private IGeneralSettings GeneralSettings { get; }
    }
}
