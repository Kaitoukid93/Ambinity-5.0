﻿using adrilight.DesktopDuplication;
using adrilight.Util.CaptureEngine;
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
            MainViewModel.AvailableAudioDevices.CollectionChanged += (s, e) =>
            {
                switch (e.Action)
                {
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                        AudioDeviceChanged();
                        break;
                }
            };
            //Init();
            RefreshCapturingState();

        }
        #region private field
        private CancellationTokenSource _cancellationTokenSource;

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
        private void AudioDeviceChanged()
        {
            Stop();
            foreach (var capture in _audioCaptures)
            {
                capture.FreeBassWasapi();
            }
            Log.Information("starting BassAudioCapturing");
            List<AudioDevice> audioDevices = GetAvailableAudioDevices();
            _workerThreads = new List<Thread>();
            Frames = new ByteFrame[audioDevices.Count()];
            _audioCaptures = new AudioCaptureBasic[audioDevices.Count()];
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
        public void RefreshCapturingState()
        {

            Init();
            Log.Information("starting BassAudioCapturing");
            List<AudioDevice> audioDevices = GetAvailableAudioDevices();
            _workerThreads = new List<Thread>();
            Frames = new ByteFrame[audioDevices.Count()];
            _audioCaptures = new AudioCaptureBasic[audioDevices.Count()];
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




        public void Run(AudioDevice device, int index)

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
                while (_state == RunningState.Capturing)
                {
                    var isPreviewWindowOpen = MainViewModel.IsInIDEditStage && MainViewModel.IdEditMode == MainViewViewModel.IDMode.FID || MainViewModel.IsAudioSelectionOpen;
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
            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_UPDATETHREADS, false);
            var result = Bass.BASS_Init(0, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
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
