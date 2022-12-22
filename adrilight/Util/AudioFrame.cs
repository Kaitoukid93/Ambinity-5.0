using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Color = System.Windows.Media.Color;
using adrilight.Util;
using System.Threading;
using NLog;
using Un4seen.BassWasapi;
using Un4seen.Bass;
using System.Windows;
using System.Diagnostics;
using adrilight.Spots;
using adrilight.ViewModel;
using GalaSoft.MvvmLight;
using adrilight.View;
using System.Threading.Tasks;

namespace adrilight
{
    internal class AudioFrame : ViewModelBase, IAudioFrame
    {


        public static float[] _fft;
        public static int _lastlevel;
        public static int _hanctr;
        public static int volumeLeft;
        public static int volumeRight;
        public static int height = 0;
        public static int heightL = 0;
        public static int heightR = 0;
        private float[] lastSpectrumData;
        public WASAPIPROC _process;
        public static byte lastvolume = 0;
        public static byte volume = 0;
        public static int lastheight = 0;
        private float speed1 = 1.0F, speed2 = 0.20F, lightTime = 5.0F;
        public static bool bump = false;

        private readonly NLog.ILogger _log = LogManager.GetCurrentClassLogger();

        public AudioFrame(IGeneralSettings generalSettings)
        {

            GeneralSettings = generalSettings ?? throw new ArgumentException(nameof(generalSettings));
            GeneralSettings.PropertyChanged += PropertyChanged;
            BassNet.Registration("saorihara93@gmail.com", "2X2831021152222");
            _process = new WASAPIPROC(Process);
            _fft = new float[1024];
            _lastlevel = 0;
            _hanctr = 0;
            AvailableAudioDevice = GetAvailableAudioDevices();
            RefreshAudioState();
            _log.Info($"MusicColor Created");

        }
        //Dependency Injection//




        private IGeneralSettings GeneralSettings { get; }
        private bool inSync { get; set; }

        public bool IsRunning { get; private set; } = false;
        private CancellationTokenSource _cancellationTokenSource;
        public float[] FFT { get; set; }

        private void PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {

                case nameof(GeneralSettings.SelectedAudioDevice):

                    RefreshAudioState();
                    break;


            }

        }
        private void RefreshAudioState()
        {

            var isRunning = _cancellationTokenSource != null && IsRunning;
            var shouldBeRunning = true;

            if (isRunning && !shouldBeRunning)
            {
                //stop it!
                _log.Debug("stopping the Audio Frame");
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;
                // Free();
            }

            else if (!isRunning && shouldBeRunning)
            {
                //start it
                _log.Debug("starting the Audio Frame");
                Init();
                bool result = BassWasapi.BASS_WASAPI_Init(BassAudioDeviceID, 0, 0, BASSWASAPIInit.BASS_WASAPI_BUFFER, 1f, 0.05f, _process, IntPtr.Zero); // this is the function to init the device according to device index
                if (!result)
                {
                    var error = Bass.BASS_ErrorGetCode();
                    //MessageBox.Show(error.ToString());
                }
                else
                {
                    //_initialized = true;
                    //  Bassbox.IsEnabled = false;

                }
                BassWasapi.BASS_WASAPI_Start();
                //BassWasapi.BASS_WASAPI_Init(-3, 0, 0, BASSWASAPIInit.BASS_WASAPI_BUFFER, 1f, 0.05f, _process, IntPtr.Zero);
                _cancellationTokenSource = new CancellationTokenSource();
                var thread = new Thread(() => Run(_cancellationTokenSource.Token)) {
                    IsBackground = true,
                    Priority = ThreadPriority.BelowNormal,
                    Name = "AudioFrame"
                };
                thread.Start();
            }
            else if (isRunning && shouldBeRunning) // something changed but not affects the running state
            {
                //start it
                Free();
                IsRunning = false;
                Init();
                bool result = BassWasapi.BASS_WASAPI_Init(BassAudioDeviceID, 0, 0, BASSWASAPIInit.BASS_WASAPI_BUFFER, 1f, 0.05f, _process, IntPtr.Zero); // this is the function to init the device according to device index
                if (!result)
                {
                    var error = Bass.BASS_ErrorGetCode();
                    //MessageBox.Show(error.ToString());
                }
                else
                {
                    //_initialized = true;
                    //  Bassbox.IsEnabled = false;

                }
                BassWasapi.BASS_WASAPI_Start();
                //BassWasapi.BASS_WASAPI_Init(-3, 0, 0, BASSWASAPIInit.BASS_WASAPI_BUFFER, 1f, 0.05f, _process, IntPtr.Zero);
                _cancellationTokenSource = new CancellationTokenSource();
                var thread = new Thread(() => Run(_cancellationTokenSource.Token)) {
                    IsBackground = true,
                    Priority = ThreadPriority.BelowNormal,
                    Name = "AudioFrame"
                };
                thread.Start();
            }
        }



        private int Process(IntPtr buffer, int length, IntPtr user)
        {
            return length;
        }


        public void Run(CancellationToken token)

        {
            if (IsRunning) throw new Exception(" AudioFrame is already running!");

            IsRunning = true;

            _log.Debug("Started Audio Frame.");
            FFT = new float[32]; // create 32 frequency
            //BassWasapi.BASS_WASAPI_SetDevice(BassAudioDeviceID);

            try
            {

                lastSpectrumData = new float[32];
                while (!token.IsCancellationRequested)
                {

                    GetCurrentFFTFrame(32);
                    FFT = lastSpectrumData;
                    Thread.Sleep(10); // take 100 sample per second
                }
            }

            catch (OperationCanceledException)
            {
                _log.Debug("OperationCanceledException catched. returning.");

                // return;
            }
            catch (Exception ex)
            {
                _log.Debug(ex, "Exception catched.");

                //allow the system some time to recover
                Thread.Sleep(500);
            }
            finally
            {

                _log.Debug("Stopped AudioFrame.");
                IsRunning = false;
            }



        }


        private float[] GetCurrentFFTFrame(int numFreq)
        {
            List<byte> spectrumdata = new List<byte>();
            int ret = BassWasapi.BASS_WASAPI_GetData(_fft, (int)BASSData.BASS_DATA_FFT2048);// get channel fft data
            if (ret < -1) return null;
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
                    lastSpectrumData[i] += speed1 * (spectrumdata[i] - lastSpectrumData[i]);


                }

                if (spectrumdata[i] < lastSpectrumData[i])
                {
                    lastSpectrumData[i] -= speed2 * (lastSpectrumData[i] - spectrumdata[i]);


                }
            }
            return lastSpectrumData;


        }

        private IList<string> _availableAudioDevice;
        public IList<string> AvailableAudioDevice {
            set
            {
                _availableAudioDevice = value;
            }
            get
            {

                return _availableAudioDevice;
            }

        }
        private List<string> GetAvailableAudioDevices()
        {
            var availableDevices = new List<string>();
            int devicecount = BassWasapi.BASS_WASAPI_GetDeviceCount();

            for (int i = 0; i < devicecount; i++)
            {

                var devices = BassWasapi.BASS_WASAPI_GetDeviceInfo(i);

                if (devices.IsEnabled && devices.IsLoopback)
                {
                    var device = string.Format("{0} - {1}", i, devices.name);

                    availableDevices.Add(device);
                }

            }
            return availableDevices;
        }



        public int BassAudioDeviceID {
            get
            {
                string currentDevice;
                if (AvailableAudioDevice.Count >= 1)
                {
                    if (GeneralSettings.SelectedAudioDevice >= AvailableAudioDevice.Count())
                    {
                        if (GeneralSettings.AudioDeviceAskAgain)
                            Application.Current.Dispatcher.Invoke<bool>(AskUserForAudioDeviceError);
                        currentDevice = AvailableAudioDevice.ElementAt(0);
                    }
                    else
                        currentDevice = AvailableAudioDevice.ElementAt(GeneralSettings.SelectedAudioDevice);

                    var array = currentDevice.Split(' ');
                    return Convert.ToInt32(array[0]);

                }
                else
                {
                    if (GeneralSettings.AudioDeviceAskAgain)
                        Application.Current.Dispatcher.Invoke<bool>(AskUserForAudioDeviceError);
                    return -1;
                }

            }
        }


        public bool AskUserForAudioDeviceError()
        {
            var dialog = new CommonInfoDialog();
            //dialog.header.Text = "OpenRGB is disabled"
            dialog.question.Text = "Có sự thay đổ về đầu ra âm thanh, vui lòng chọn lại bên trong cài đặt";
            var result = dialog.ShowDialog();
            if (result == false)
            {
                if (dialog.askagaincheckbox.IsChecked == true)
                {
                    GeneralSettings.AudioDeviceAskAgain = false;
                }
                else
                {
                    GeneralSettings.AudioDeviceAskAgain = true;
                }
                return false;

            }
            else
            {
                return true;
            }
        }


        private void Init()
        {
            BassWasapi.BASS_WASAPI_Free();
            Bass.BASS_Free();
            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_UPDATETHREADS, false);
            var result = Bass.BASS_Init(0, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
            if (!result) throw new Exception("Init Error");
        }


        public void Free()
        {
            BassWasapi.BASS_WASAPI_Free();
            Bass.BASS_Free();
        }



    }
}
