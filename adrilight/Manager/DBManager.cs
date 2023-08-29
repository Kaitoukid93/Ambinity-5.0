using adrilight.Services.OpenRGBService;
using adrilight.ViewModel;
using Serilog;
using System;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace adrilight.Manager
{
    internal class DBmanager
    {
        public DBmanager(MainViewViewModel mainViewViewModel)
        {

            MainViewViewModel = mainViewViewModel ?? throw new ArgumentNullException(nameof(mainViewViewModel));
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


        public MainViewViewModel MainViewViewModel { get; set; }
        private AmbinityClient AmbinityClient { get; }
        private async void StartDiscovery(CancellationToken token)
        {

            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (MainViewViewModel.DeviceHlprs == null)
                        return;
                    foreach (var device in MainViewViewModel.AvailableDevices)
                    {
                        MainViewViewModel.DeviceHlprs.WriteSingleDeviceInfoJson(device);
                    }
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