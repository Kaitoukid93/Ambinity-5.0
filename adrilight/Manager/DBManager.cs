using adrilight.Services.OpenRGBService;
using adrilight.ViewModel;
using adrilight_shared.Models;
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
        public DBmanager(MainViewViewModel mainViewViewModel, CollectionItemStore collectionStore)
        {

            MainViewViewModel = mainViewViewModel ?? throw new ArgumentNullException(nameof(mainViewViewModel));
            _collectionStore = collectionStore ?? throw new ArgumentNullException(nameof(collectionStore));
            _collectionStore.ItemsRemoved += ItemsRemoved;
            FilesQToRemove = new ObservableCollection<string>();
            StartThread();

        }

        private void ItemsRemoved(List<IGenericCollectionItem> list)
        {
            foreach (var item in list)
            {
                if (File.Exists(item.LocalPath))
                {
                    try
                    {
                        File.Delete(item.LocalPath);
                    }

                    catch (Exception ex)
                    {

                    }
                }
            }
        }

        private Thread _workerThread;
        private CancellationTokenSource _cancellationTokenSource;
        private CollectionItemStore _collectionStore;
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
        public MainViewViewModel MainViewViewModel { get; set; }
        private AmbinityClient AmbinityClient { get; }
        private async void StartDiscovery(CancellationToken token)
        {

            while (!token.IsCancellationRequested)
            {
                try
                {
                    SaveFile();
                    if (MainViewViewModel.IsAppActivated)
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
        public void SaveFile()
        {
            if (MainViewViewModel.DeviceHlprs == null)
                return;
            lock (MainViewViewModel.AvailableDevices)
            {
                foreach (var device in MainViewViewModel.AvailableDevices)
                {
                    lock (device)
                        MainViewViewModel.DeviceHlprs.WriteSingleDeviceInfoJson(device);
                }
            }
            MainViewViewModel.LightingProfileManagerViewModel.SaveData();
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