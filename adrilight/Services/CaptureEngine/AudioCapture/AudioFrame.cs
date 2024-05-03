using adrilight.Services.CaptureEngine;
using adrilight.Util.CaptureEngine;
using adrilight.ViewModel;
using adrilight_shared.Models.Audio;
using adrilight_shared.Models.FrameData;
using adrilight_shared.Settings;
using GalaSoft.MvvmLight;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Un4seen.Bass;
using Un4seen.BassWasapi;

namespace adrilight
{
    internal class AudioFrame : ViewModelBase, IDisposable, ICaptureEngine
    {
        public AudioFrame(IGeneralSettings generalSettings, MainViewViewModel mainViewModel)
        {

            GeneralSettings = generalSettings ?? throw new ArgumentException(nameof(generalSettings));
            MainViewModel = mainViewModel ?? throw new ArgumentException(nameof(mainViewModel));
            MainViewModel.AvailableAudioDevices.CollectionChanged += async (s, e) =>
            {
                switch (e.Action)
                {
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                        await Task.Run(() => AudioDeviceChanged());
                        break;
                }
            };
            //Init();
            RefreshCapturingState();

        }
        #region private field
        private CancellationTokenSource _cancellationTokenSource;
        private bool _bassInitialized;
        public object Lock { get; } = new object();
        #endregion

        #region dependency injection
        private IGeneralSettings GeneralSettings { get; }
        private MainViewViewModel MainViewModel { get; }
        #endregion

        #region public field
        public bool IsRunning { get; private set; } = false;
        public ByteFrame[] Frames { get; set; }
        public ByteFrame Frame { get; set; }
        public string DeviceName { get; set; }
        private List<Thread> _workerThreads;
        private AudioCaptureBasic[] _audioCaptures;
        private enum RunningState { Capturing, Waiting, Canceling };
        private RunningState _state = RunningState.Canceling;
        private int _serviceRequired;
        public int ServiceRequired {
            get { return _serviceRequired; }
            set
            {
                if ((_serviceRequired == 0 && value == 1) || (_serviceRequired == 1 && value == 0))
                {
                    _serviceRequired = value;
                    RefreshCapturingState();
                }
                _serviceRequired = value;
            }
        }
        #endregion

        private void AudioDeviceChanged()
        {
            Stop();
            foreach (var capture in _audioCaptures)
            {
                capture.FreeBassWasapi();
            }
            Log.Information("starting BassAudioCapturing");
            List<AudioDevice> audioDevices = GetAvailableAudioDevices();
            _cancellationTokenSource = new CancellationTokenSource();
            _workerThreads = new List<Thread>();
            Frames = new ByteFrame[audioDevices.Count()];
            _audioCaptures = new AudioCaptureBasic[audioDevices.Count()];
            int index = 0;
            foreach (var device in audioDevices)
            {
                Thread workerThread = new Thread(() => Run(device, index++, _cancellationTokenSource.Token)) {
                    IsBackground = true,
                    Priority = ThreadPriority.BelowNormal,
                    Name = "AudioCapture" + device.Name
                };
                _state = RunningState.Capturing;
                workerThread.Start();
                _workerThreads.Add(workerThread);
            }
        }
        public void RefreshCapturingState()
        {
            var isRunning = _state != RunningState.Canceling;
            var shouldBeRunning = GeneralSettings.AudioCapturingEnabled && ServiceRequired > 0;
            if (isRunning && !shouldBeRunning)
            {
                //stop it!
                Log.Information("AudioFrame is disabled");
                _state = RunningState.Waiting;

            }
            else if (!isRunning && shouldBeRunning)
            {
                Init();
                Log.Information("starting BassAudioCapturing");
                List<AudioDevice> audioDevices = GetAvailableAudioDevices();
                _cancellationTokenSource = new CancellationTokenSource();
                _workerThreads = new List<Thread>();
                Frames = new ByteFrame[audioDevices.Count()];
                _audioCaptures = new AudioCaptureBasic[audioDevices.Count()];
                int index = 0;
                foreach (var device in audioDevices)
                {
                    Thread workerThread = new Thread(() => Run(device, index++, _cancellationTokenSource.Token)) {
                        IsBackground = true,
                        Priority = ThreadPriority.BelowNormal,
                        Name = "AudioCapture" + device.Name
                    };
                    _state = RunningState.Capturing;
                    workerThread.Start();
                    _workerThreads.Add(workerThread);
                }
            }
            else if (isRunning && shouldBeRunning)
            {
                _state = RunningState.Capturing;
            }
        }




        public void Run(AudioDevice device, int index, CancellationToken token)

        {
            // if (IsRunning) throw new Exception(" AudioFrame is already running!");

            IsRunning = true;

            Log.Information("Started Audio Capture for: " + device.Name);
            Frames[index] = new ByteFrame();
            try
            {
                Frames[index].Frame = new byte[32];
                _audioCaptures[index] = new AudioCaptureBasic(device);
                _audioCaptures[index].StartBassWasapi();
                while (!token.IsCancellationRequested)
                {
                    if (_state == RunningState.Capturing)
                    {
                        var isPreviewWindowOpen = MainViewModel.IsInIDEditStage || MainViewModel.IsAudioSelectionOpen;
                        //var result = GetCurrentFFTFrame(32, Frames[index].Frame);
                        lock (Lock)
                        {
                            var result = _audioCaptures[index].GetCurrentFFTFrame(32, Frames[index].Frame);
                            if (!result)
                            {
                                Frames[index].Frame = new byte[32];
                            }
                        }
                        if (isPreviewWindowOpen)
                        {
                            lock (MainViewModel.AudioUpdateLock)
                            {
                                MainViewModel.AudioVisualizerUpdate(Frames[index], index);
                            }

                        }
                        Thread.Sleep(25); // take 100 sample per second
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }



                }
            }

            catch (OperationCanceledException)
            {
                Log.Error("OperationCanceledException catched. returning.");

                // return;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Exception catched.");

                //allow the system some time to recover
                Thread.Sleep(500);
            }
            finally
            {

                Log.Information("Stopped AudioFrame.");
                IsRunning = false;
            }



        }



        private List<AudioDevice> GetAvailableAudioDevices()
        {
            var availableDevices = new List<AudioDevice>();
            int devicecount = BassWasapi.BASS_WASAPI_GetDeviceCount();

            for (int i = 0; i < devicecount; i++)
            {

                var device = BassWasapi.BASS_WASAPI_GetDeviceInfo(i);

                if (device.IsEnabled && device.IsLoopback)
                {
                    var audioDevice = new AudioDevice() { Name = device.name, Index = i };

                    availableDevices.Add(audioDevice);
                }

            }
            return availableDevices;
        }

        private void Init()
        {
            //BassWasapi.BASS_WASAPI_Free();
            //Bass.BASS_Free();
            if (_bassInitialized)
                return;
            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_UPDATETHREADS, false);
            var result = Bass.BASS_Init(0, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
            if (result)
            {
                _bassInitialized = true;
            }
            if (!result) throw new Exception("Init Error");
        }


        public void Free()
        {
            Bass.BASS_Free();
        }


        public bool IsDisposed { get; private set; }
        public void Dispose()
        {
            IsDisposed = true;
            Free();
            GC.Collect();
        }

        public void Stop()
        {
            if (_workerThreads == null)
                return;
            Log.Information("Stop called for Audio Capture");
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource = null;
            }
            _state = RunningState.Canceling;
            for (int i = 0; i < _workerThreads.Count(); i++)
            {
                if (_workerThreads[i] == null) return;
                //_captures[i]?.Dispose();
                GC.Collect();
                _workerThreads[i]?.Join();
                _workerThreads[i] = null;
            }
        }
    }
}
