using adrilight_shared.Helpers;
using adrilight_shared.Settings;
using GalaSoft.MvvmLight;
using OpenRGB.NET;
using OpenRGB.NET.Models;
using Polly;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace adrilight.Services.OpenRGBService
{
    internal sealed class
        AmbinityClient : ViewModelBase, IDisposable, IAmbinityClient
    {
        private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "adrilight\\");
        private string ORGBJsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OpenRGB\\");
        private string ORGBPath => Path.Combine(JsonPath, "ORGB\\");
        private string ORGBExeFolderNameAndPath => Path.Combine(ORGBPath, "OpenRGB\\");
        private string ORGBExeFileNameAndPath => Path.Combine(ORGBExeFolderNameAndPath, "OpenRGB Windows 64-bit\\OpenRGB.exe");

        public AmbinityClient(IGeneralSettings generalSettings)
        {
            GeneralSettings = generalSettings ?? throw new ArgumentException(nameof(generalSettings));
            _retryPolicy = Policy
           .Handle<Exception>()
           .WaitAndRetryAsync(10, _ => TimeSpan.FromSeconds(5));
            GeneralSettings.PropertyChanged += UserSettings_PropertyChangedAsync;
            if (ResourceHlprs == null)
                ResourceHlprs = new ResourceHelpers();
            //RefreshTransferState();
        }
        //Dependency Injection//
        private IGeneralSettings GeneralSettings { get; }
        public ResourceHelpers ResourceHlprs { get; set; }
        private Process _oRGBProcess;
        public Process ORGBProcess {
            get { return _oRGBProcess; }
            set { _oRGBProcess = value; }
        }
        private bool _isInitialize;
        public bool IsInitialized {
            get
            {
                return _isInitialize;
            }
            set
            {
                _isInitialize = value; RaisePropertyChanged();
            }
        }
        private bool _isInitializing;
        public bool IsInitializing {
            get
            {
                return _isInitializing;
            }
            private set
            {
                _isInitializing = value; RaisePropertyChanged();
            }
        }
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

        public void LoadDefaultProfile()
        {
            Client.LoadProfile("default");
        }
        public void SaveDefaultProfile()
        {
            Client.SaveProfile("default");
        }
        private static bool isRunning(string name)
        {
            try
            {
                var processes = Process.GetProcessesByName(name);
                if (processes.Count() == 0 || processes == null)
                    return false;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }

            return true;
        }
        public async Task RefreshTransferState()
        {
            IsInitializing = true;

            if (!IsInitialized && GeneralSettings.IsOpenRGBEnabled && GeneralSettings.UsingOpenRGB) // Only run OpenRGB Stream if User enable OpenRGB Utilities in General Settings
            {
                //check if OpenRGB process is alive
                if (!isRunning("OpenRGB"))
                {
                    LaunchOpenRGBProcess();
                    await Task.Delay(5000);
                }

                try
                {
                    if (Client != null)
                        Client.Dispose();
                    var result = await _retryPolicy.ExecuteAsync(async () => await RefreshOpenRGBDeviceState());
                    if (result)
                    {
                        IsInitialized = true;
                        IsInitializing = false;
                    }

                }
                catch (TimeoutException)
                {
                    HandyControl.Controls.MessageBox.Show("Không tìm thấy Server OpenRGB, Hãy thử thoát ứng dụng và mở lại");
                    IsInitialized = false;
                    IsInitializing = false;
                    //IsAvailable= false;

                }
                catch (System.Net.Sockets.SocketException)
                {
                    HandyControl.Controls.MessageBox.Show("Mất kết nối ứng dụng OpenRGB, vui lòng không thoát OpenRGB khi đang sử dụng");
                    IsInitialized = false;
                    IsInitializing = false;
                    //IsAvailable= false;

                }
                catch (Exception ex)
                {
                    Log.Error(ex, "OpenRGB External Exception");
                    IsInitialized = false;
                    IsInitializing = false;
                }
            }

        }
        private void LaunchOpenRGBProcess()
        {
            var currentVersion = new Version(GeneralSettings.OpenRGBVersion);
            var packedVersion = new Version("0.91");
            if (!File.Exists(ORGBExeFileNameAndPath) || currentVersion < packedVersion)
            {
                try
                {
                    if (!Directory.Exists(ORGBPath))
                        Directory.CreateDirectory(ORGBPath);
                    else
                    {
                        LocalFileHelpers.ClearFolderContent(ORGBPath);
                    }
                    ResourceHlprs.CopyResource("adrilight_shared.Tools.OpenRGB.OpenRGB.zip", Path.Combine(ORGBPath, "OpenRGB.zip"));
                    //Create directory to extract
                    Directory.CreateDirectory(ORGBExeFolderNameAndPath);
                    //then extract
                    ZipFile.ExtractToDirectory(Path.Combine(ORGBPath, "OpenRGB.zip"), ORGBExeFolderNameAndPath);
                    //then delete the zip to prevent further conflict
                    File.Delete(Path.Combine(ORGBPath, "OpenRGB.zip"));
                    GeneralSettings.OpenRGBVersion = packedVersion.ToString();
                }
                catch (ArgumentException)
                {
                    //show messagebox no firmware found for this device
                    return;
                }

            }
            if (GeneralSettings.ShowOpenRGB)
            {
                ORGBProcess = Process.Start(ORGBExeFileNameAndPath, "--server --startminimized --gui");
            }
            else
            {
                ORGBProcess = Process.Start(ORGBExeFileNameAndPath, "--server");
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
                    var devices = Client.GetAllControllerData();
                    //set all existed device mode to dirrect if supported
                    var index = 0;
                    foreach (var device in devices)
                    {
                        for (var i = 0; i < device.Modes.Length; i++)
                        {
                            Log.Information(device.Modes[i].Name.ToString());
                            if (device.Modes[i].Name == "Direct")
                            {
                                Client.SetMode(index, i);
                                break;
                            }
                        }
                        index++;
                        Log.Information($"Device found : " + device.Name.ToString() + "At index: " + index);
                        AvailableOpenRGBDevices.Add(device);
                    }
                }

            }
            catch (Exception ex)
            {
                //something could happen to openRGB client
                Client.Dispose();
                IsInitialized = false;

            }
            return AvailableOpenRGBDevices;
        }


        public async Task<bool> RefreshOpenRGBDeviceState()//init
        {

            if (Client != null)
                Client.Dispose();
            Client = new OpenRGBClient("127.0.0.1", 6742, name: "Ambinity", autoconnect: true, timeout: 1000, protocolVersion: 2);
            if (Client != null)
            {
                //check if we get any device from Openrgb
                //create default profile if not exist
                if (Client.GetControllerCount() > 0)
                {
                    return await Task.FromResult(true);
                }
                else // this could happen due to device scanning is in progress
                {
                    //dispose the client
                    Client.Dispose();
                    throw new Exception("ORGB busy");
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
            //if (ORGBProcess != null)
            //{
            //    try { ORGBProcess.Kill(); }

            //    catch
            //    {

            //    }

            //}
            IsInitialized = false;
            GC.SuppressFinalize(this);
        }


    }
}
