﻿using System;
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
using SharpDX.DXGI;
using adrilight.DesktopDuplication;

namespace adrilight
{
    internal class AudioFrame : ViewModelBase, IDisposable, ICaptureEngine
    {




        private readonly NLog.ILogger _log = LogManager.GetCurrentClassLogger();

        public AudioFrame(IGeneralSettings generalSettings, MainViewViewModel mainViewModel)
        {

            GeneralSettings = generalSettings ?? throw new ArgumentException(nameof(generalSettings));
            GeneralSettings.PropertyChanged += PropertyChanged;
            MainViewModel = mainViewModel ?? throw new ArgumentException(nameof(mainViewModel));
            BassNet.Registration("saorihara93@gmail.com", "2X2831021152222");
            _process = new WASAPIPROC(Process);
            _fft = new float[1024];
            RefreshCapturingState();

        }
        #region private field
        private int _deviceIndex;
        private Thread _workerThread;
        private CancellationTokenSource _cancellationTokenSource;
        public static float[] _fft;
        private byte[] _lastSpectrumData;
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
        public ByteFrame Frame { get; set; }
        public string DeviceName { get; set; }
        #endregion

         
        #region properties changed event

        #endregion
        private void PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {

                case nameof(GeneralSettings.SelectedAudioDevice):
                    StartBassWasapi();
                    break;
            }

        }
        public void RefreshCapturingState()
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
            Frame = new ByteFrame();
            try
            {
                _lastSpectrumData = new byte[32];
                Frame.Frame = new byte[32];
                StartBassWasapi();
                while (!token.IsCancellationRequested)
                {
                    var isPreviewWindowOpen = MainViewModel.IsInIDEditStage && MainViewModel.IdEditMode == MainViewViewModel.IDMode.FID|| MainViewModel.IsAudioSelectionOpen;
                    GetCurrentFFTFrame(32);
                    lock(Lock)
                    {
                        Frame.Frame = _lastSpectrumData;
                    }
                    
                    if (isPreviewWindowOpen)
                    {
                        lock (MainViewModel.AudioUpdateLock)
                        {
                            MainViewModel.AudioVisualizerUpdate(Frame);
                        }

                    }
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


        private byte[] GetCurrentFFTFrame(int numFreq)
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
                if (spectrumdata[i] > _lastSpectrumData[i])
                {
                    _lastSpectrumData[i] += (byte)(_speed1 * (spectrumdata[i] - _lastSpectrumData[i]));


                }

                if (spectrumdata[i] < _lastSpectrumData[i])
                {
                    _lastSpectrumData[i] -= (byte)(_speed2 * (_lastSpectrumData[i] - spectrumdata[i]));


                }
            }
            return _lastSpectrumData;


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
        private void StartBassWasapi()
        {
            var selectedIndex = GeneralSettings.SelectedAudioDevice > 0 ? GeneralSettings.SelectedAudioDevice : 0;
            var selectedAudioDevice = GetAvailableAudioDevices()[selectedIndex];
            _deviceIndex = selectedAudioDevice.Index;
            Init();
            bool result = BassWasapi.BASS_WASAPI_Init(_deviceIndex, 0, 0, BASSWASAPIInit.BASS_WASAPI_BUFFER, 1f, 0.05f, _process, IntPtr.Zero); // this is the function to init the device according to device index
            if (!result)
            {
                var error = Bass.BASS_ErrorGetCode();
                //MessageBox.Show(error.ToString());
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


        public bool IsDisposed { get; private set; }
        public void Dispose()
        {
            IsDisposed = true;
            Free();
            GC.Collect();
        }

        public void Stop()
        {
            _log.Debug("Stop called for audio frame");
            if (_workerThread == null) return;
            Free();
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
            _workerThread?.Join();
            _workerThread = null;

        }
    }
}