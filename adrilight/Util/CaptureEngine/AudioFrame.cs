using adrilight.DesktopDuplication;
using adrilight.ViewModel;
using GalaSoft.MvvmLight;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Un4seen.Bass;
using Un4seen.BassWasapi;

namespace adrilight
{
    internal class AudioFrame : ViewModelBase, IDisposable, ICaptureEngine
    {
        public AudioFrame(IGeneralSettings generalSettings, MainViewViewModel mainViewModel)
        {

            GeneralSettings = generalSettings ?? throw new ArgumentException(nameof(generalSettings));
            GeneralSettings.PropertyChanged += PropertyChanged;
            MainViewModel = mainViewModel ?? throw new ArgumentException(nameof(mainViewModel));
            _process = new WASAPIPROC(Process);
            _fft = new float[1024];
            //Init();
            RefreshCapturingState();

        }
        #region private field
        private int _deviceIndex;
        private Thread _workerThread;
        private int _lastlevel;             //last output level
        private int _hanctr;                //last output level counter
        private CancellationTokenSource _cancellationTokenSource;
        public static float[] _fft;
        public WASAPIPROC _process;
        private float _speed1 = 1.0F, _speed2 = 0.20F;
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
        private enum RunningState { Capturing, Waiting, Canceling };
        private RunningState _state = RunningState.Waiting;
        #endregion

        private void PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {

                // case nameof(GeneralSettings.SelectedAudioDevice):
                //   Log.Information("Audio Device Changing to: " + GeneralSettings.SelectedAudioDevice);
                //  RefreshCapturingState();
                //break;
            }

        }
        public void RefreshCapturingState()
        {

            Stop();
            Free();
            Init();
            _workerThreads = new List<Thread>();
            Log.Information("starting BassAudioCapturing");
            List<AudioDevice> audioDevices = GetAvailableAudioDevices();
            Frames = new ByteFrame[audioDevices.Count()];
            int index = 0;
            foreach (var device in audioDevices)
            {
                Thread workerThread = new Thread(() => Run(device, index++)) {
                    IsBackground = true,
                    Priority = ThreadPriority.BelowNormal,
                    Name = "AudioCapture" + device.Name
                };
                _state = RunningState.Capturing;
                workerThread.Start();
                _workerThreads.Add(workerThread);
            }
        }


        private int Process(IntPtr buffer, int length, IntPtr user)
        {
            return length;
        }


        public void Run(AudioDevice device, int index)

        {
            // if (IsRunning) throw new Exception(" AudioFrame is already running!");

            IsRunning = true;

            Log.Information("Started Audio Capture for: " + device.Name);
            Frames[index] = new ByteFrame();
            try
            {
                Frames[index].Frame = new byte[32];
                StartBassWasapi(device);
                while (_state == RunningState.Capturing)
                {
                    var isPreviewWindowOpen = MainViewModel.IsInIDEditStage && MainViewModel.IdEditMode == MainViewViewModel.IDMode.FID || MainViewModel.IsAudioSelectionOpen;
                    //var result = GetCurrentFFTFrame(32, Frames[index].Frame);

                    lock (Lock)
                    {
                        var result = GetCurrentFFTFrame(32, Frames[index].Frame);
                        if (!result)
                        {
                            Frames[index].Frame = new byte[32];
                        }
                    }



                    if (isPreviewWindowOpen)
                    {
                        lock (MainViewModel.AudioUpdateLock)
                        {
                            MainViewModel.AudioVisualizerUpdate(Frames[0]);
                        }

                    }
                    Thread.Sleep(25); // take 100 sample per second
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


        private bool GetCurrentFFTFrame(int numFreq, byte[] lastSpectrumData)
        {
            List<byte> spectrumdata = new List<byte>();
            int ret = BassWasapi.BASS_WASAPI_GetData(_fft, (int)BASSData.BASS_DATA_FFT2048);// get channel fft data
            if (ret < 0) return false;
            int x, y;
            int b0 = 0;
            //computes the spectrum data, the code is taken from a bass_wasapi sample.
            for (x = 0; x < numFreq; x++)
            {
                float peak = 0;
                int b1 = (int)Math.Pow(2, x * 10.0 / (numFreq - 1));
                if (b1 > 1023) b1 = 1023;
                if (b1 <= b0) b1 = b0 + 1;
                for (; b0 < b1; b0++)
                {
                    if (peak < _fft[1 + b0]) peak = _fft[1 + b0];
                }
                y = (int)(Math.Sqrt(peak) * 3 * 250 - 4);
                if (y > 255) y = 255;
                if (y < 10) y = 0;
                spectrumdata.Add((byte)y);
            }


            for (int i = 0; i < numFreq; i++)
            {
                if (spectrumdata[i] > lastSpectrumData[i])
                {
                    lastSpectrumData[i] += (byte)(_speed1 * (spectrumdata[i] - lastSpectrumData[i]));


                }

                if (spectrumdata[i] < lastSpectrumData[i])
                {
                    lastSpectrumData[i] -= (byte)(_speed2 * (lastSpectrumData[i] - spectrumdata[i]));
                }
            }
            int level = BassWasapi.BASS_WASAPI_GetLevel();
            if (level == _lastlevel && level != 0) _hanctr++;
            _lastlevel = level;
            //Required, because some programs hang the output. If the output hangs for a 75ms
            //this piece of code re initializes the output
            //so it doesn't make a gliched sound for long.
            //if (_hanctr > 3)
            //{
            //    _hanctr = 0;
            //    StartBassWasapi();
            //}
            return true;


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
        private void StartBassWasapi(AudioDevice device)
        {
            bool result = BassWasapi.BASS_WASAPI_Init(device.Index, 0, 0, BASSWASAPIInit.BASS_WASAPI_BUFFER, 1f, 0.05f, _process, IntPtr.Zero); // this is the function to init the device according to device index
            if (!result)
            {
                var error = Bass.BASS_ErrorGetCode();
                Log.Error(error.ToString());
                // MessageBox.Show(error.ToString());
            }
            bool dev = BassWasapi.BASS_WASAPI_SetDevice(device.Index);
            if (!dev)
            {
                var err = Bass.BASS_ErrorGetCode();
            }

            else
            {


            }
            BassWasapi.BASS_WASAPI_Start();
        }





        //public bool AskUserForAudioDeviceError()
        //{
        //    var dialog = new CommonInfoDialog();
        //    //dialog.header.Text = "OpenRGB is disabled"
        //    dialog.question.Text = "Có sự thay đổ về đầu ra âm thanh, vui lòng chọn lại bên trong cài đặt";
        //    var result = dialog.ShowDialog();
        //    if (result == false)
        //    {
        //        if (dialog.askagaincheckbox.IsChecked == true)
        //        {
        //            GeneralSettings.AudioDeviceAskAgain = false;
        //        }
        //        else
        //        {
        //            GeneralSettings.AudioDeviceAskAgain = true;
        //        }
        //        return false;

        //    }
        //    else
        //    {
        //        return true;
        //    }
        //}


        private void Init()
        {
            //BassWasapi.BASS_WASAPI_Free();
            //Bass.BASS_Free();
            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_UPDATETHREADS, false);
            var result = Bass.BASS_Init(0, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
            if (!result) throw new Exception("Init Error");
        }


        public void Free()
        {
            BassWasapi.BASS_WASAPI_Free();
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
            Log.Error("Stop called for Audio Capture");
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
