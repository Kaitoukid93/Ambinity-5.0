using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
using NLog;
using System.Buffers;
using adrilight.Util;
using System.Linq;
using Newtonsoft.Json;
using System.Windows;
using adrilight.Spots;
using OpenRGB.NET;
using System.Collections.Generic;
using Polly;
using System.IO;
using System.Reflection;
using System.IO.Compression;
using OpenRGB.NET.Models;
using System.Threading.Tasks;
using adrilight.View;
using adrilight.ViewModel;

namespace adrilight
{
    internal sealed class
        AmbinityClient : IDisposable, IAmbinityClient
    {
        private ILogger _log = LogManager.GetCurrentClassLogger();
        private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "adrilight\\");
        private string ORGBJsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OpenRGB\\");
        private string ORGBPath => Path.Combine(JsonPath, "ORGB\\");
        private string ORGBExeFolderNameAndPath => Path.Combine(ORGBPath, "OpenRGB\\");
        private string ORGBExeFileNameAndPath => Path.Combine(ORGBExeFolderNameAndPath, "OpenRGB Windows 64-bit\\OpenRGB.exe");

        public AmbinityClient(IGeneralSettings generalSettings,MainViewViewModel mainViewViewModel)
        {
            GeneralSettings = generalSettings ?? throw new ArgumentException(nameof(generalSettings));
           MainViewViewModel = mainViewViewModel ?? throw new ArgumentException(nameof(mainViewViewModel));
            _retryPolicy = Policy
           .Handle<Exception>()
           .WaitAndRetryAsync(10, _ => TimeSpan.FromSeconds(1));
            GeneralSettings.PropertyChanged += UserSettings_PropertyChangedAsync;
            //RefreshTransferState();
            _log.Info($"SerialStream created.");
        }
        //Dependency Injection//
        private IGeneralSettings GeneralSettings { get;}
        private MainViewViewModel MainViewViewModel { get; }
        private System.Diagnostics.Process _oRGBProcess;
        public System.Diagnostics.Process ORGBProcess {
            get { return _oRGBProcess; }
            set { _oRGBProcess = value; }
        }
        public bool IsInitialized { get; set; }
        private OpenRGBClient _client;
        public OpenRGBClient Client {
            get { return _client; }
            set { _client = value; }
        }


        private readonly Policy _retryPolicy;

        private async void UserSettings_PropertyChangedAsync(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(GeneralSettings.IsOpenRGBEnabled):
                    await RefreshTransferState();
                    break;
            }
        }


        public async Task RefreshTransferState()
        {

            if (!IsInitialized && GeneralSettings.IsOpenRGBEnabled) // Only run OpenRGB Stream if User enable OpenRGB Utilities in General Settings
            {
                MainViewViewModel.SetDashboardStatusText("Starting OpenRGB service...", true);
                LaunchOpenRGBProcess();
                await Task.Delay(3000);
                try
                {
                    if (Client != null)
                        Client.Dispose();
                    MainViewViewModel.SetDashboardStatusText("Searching for OpenRGB devices...", true);
                    var result = await _retryPolicy.ExecuteAsync(async () => await RefreshOpenRGBDeviceState());
                    if (result)
                    {
                        MainViewViewModel.SetDashboardStatusText("Done!", false);
                        IsInitialized = true;
                    }
                        
                }
                catch (TimeoutException)
                {
                    HandyControl.Controls.MessageBox.Show("Không tìm thấy Server OpenRGB, Hãy thử thoát ứng dụng và mở lại");
                    IsInitialized = false;
                    //IsAvailable= false;

                }
                catch (System.Net.Sockets.SocketException)
                {
                    HandyControl.Controls.MessageBox.Show("Mất kết nối ứng dụng OpenRGB, vui lòng không thoát OpenRGB khi đang sử dụng");
                    IsInitialized = false;
                    //IsAvailable= false;

                }
                catch (Exception ex)
                {
                    //if (ex.Message == "ORGB busy")
                    //{
                    //    // no device available
                    //    HandyControl.Controls.MessageBox.Show("Không có thiết bị bên thứ ba nào được tìm thấy");
                    //}
                }
            }

        }
        private void LaunchOpenRGBProcess()
        {
            if (!File.Exists(ORGBExeFileNameAndPath))
            {

                try
                {
                    Directory.CreateDirectory(ORGBPath);

                    CopyResource("adrilight.Tools.OpenRGB.OpenRGB.zip", Path.Combine(ORGBPath, "OpenRGB.zip"));
                    //Create directory to extract
                    Directory.CreateDirectory(ORGBExeFolderNameAndPath);
                    //then extract
                    ZipFile.ExtractToDirectory(Path.Combine(ORGBPath, "OpenRGB.zip"), ORGBExeFolderNameAndPath);
                    //then delete the zip to prevent further conflict
                    File.Delete(Path.Combine(ORGBPath, "OpenRGB.zip"));
                }
                catch (ArgumentException)
                {
                    //show messagebox no firmware found for this device
                    return;
                }

            }
            ORGBProcess = System.Diagnostics.Process.Start(ORGBExeFileNameAndPath, "--server --startminimized --gui");


        }
        private void CopyResource(string resourceName, string file)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream resource = assembly.GetManifestResourceStream(resourceName))
            {
                if (resource == null)
                {
                    throw new ArgumentException("No such resource", "resourceName");
                }
                using (Stream output = File.OpenWrite(file))
                {
                    resource.CopyTo(output);
                }
            }
        }
        public object Lock { get; } = new object();
        public List<OpenRGB.NET.Models.Device> ScanNewDevice()
        {
            var AvailableOpenRGBDevices = new List<Device>();
            try
            {

                if (Client != null && Client.Connected == true)
                {

                    var newOpenRGBDevices = Client.GetAllControllerData();

                    foreach (var device in newOpenRGBDevices)
                    {
                        AvailableOpenRGBDevices.Add(device);
                    }
                }

            }
            catch (Exception ex)
            {
                //something could happen to openRGB client

            }
            return AvailableOpenRGBDevices;
        }


        public async Task<bool> RefreshOpenRGBDeviceState()//init
        {

            if (Client != null)
                Client.Dispose();
            Client = new OpenRGBClient("127.0.0.1", 6742, name: "Ambinity", autoconnect: true, timeout: 1000);

            if (Client != null)
            {
                //check if we get any device from Openrgb
                if (Client.GetControllerCount() > 0)
                {

                    var devices = Client.GetAllControllerData();
                    int index = 0;
                    foreach (var device in devices)
                    {
                        for (var i = 0; i < device.Modes.Length; i++)
                        {
                            Debug.WriteLine(device.Modes[i].Name.ToString());
                            if (device.Modes[i].Name == "Direct")
                            {
                                Client.SetMode(index, i);
                            }
                        }
                        index++;

                        _log.Info($"Device found : " + device.Name.ToString() + "At index: " + index);
                    }
                    return await Task.FromResult(true);
                }
                else // this could happen due to device scanning is in progress
                {
                    //dispose the client
                    Client.Dispose();
                    throw (new Exception("ORGB busy"));
                    // throw some type of exception , now retry policy will catch the exception and retry
                    // the thing is, how many retrying is enough before throwing message box "no device detected"??


                }
            }
            return await Task.FromResult(false);
        }

        public void Dispose()
        {
            if (Client != null)
                Client.Dispose();
            if (ORGBProcess != null)
            {
                try { ORGBProcess.Kill(); }

                catch
                {

                }

            }

            GC.SuppressFinalize(this);
        }


    }
}
