using adrilight.Services.OpenRGBService;
using adrilight.ViewModel;
using adrilight_shared.Models;
using adrilight_shared.Models.Device;
using adrilight_shared.Models.Stores;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace adrilight.Manager
{
    public class DBmanager
    {
        public DBmanager()
        {

            FilesQToRemove = new ObservableCollection<string>();
            StartThread();

        }

        private Thread _workerThread;
        private CancellationTokenSource _cancellationTokenSource;
        public void StartThread()
        {
            //if (App.IsPrivateBuild) return;
            _cancellationTokenSource = new CancellationTokenSource();
            _workerThread = new Thread(() => StartDiscovery(_cancellationTokenSource.Token)) {
                Name = "DBManager",
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal
            };
            _workerThread.Start();

        }

        public ObservableCollection<string> FilesQToRemove { get; set; }
        private AmbinityClient AmbinityClient { get; }
        private async void StartDiscovery(CancellationToken token)
        {

            while (!token.IsCancellationRequested)
            {
                try
                {
                    await SaveFile();
                        Log.Information("Periodically App Data Saved!");

                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"error when saving devices data : {ex.GetType().FullName}: {ex.Message}");
                }

                //check once a second for updates
                await Task.Delay(TimeSpan.FromSeconds(30));
            }
        }
        public Task SaveFile()
        {
            //if (MainViewViewModel.DeviceHlprs == null)
            //    return Task.FromResult(false);
            //lock (MainViewViewModel.AvailableDevices)
            //{
            //    foreach (var device in MainViewViewModel.DeviceManagerViewModel.AvailableDevices.Items)
            //    {
            //        lock (device)
            //            MainViewViewModel.DeviceHlprs.WriteSingleDeviceInfoJson(device as DeviceSettings);
            //    }
            //}
            //MainViewViewModel.LightingProfileManagerViewModel.SaveData();
            return Task.FromResult(true);
        }
        private static object _syncRoot = new object();
        public void Stop()
        {
            Log.Information("Stop called for DBManager");
            if (_workerThread == null) return;
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
            _workerThread?.Join();
            _workerThread = null;
        }

    }
}