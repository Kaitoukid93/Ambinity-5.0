using Serilog;
using System;
using System.Collections.Generic;
using Un4seen.Bass;
using Un4seen.BassWasapi;

namespace adrilight.Util.CaptureEngine
{
    public class AudioCaptureBasic
    {
        public static float[] _fft;
        public WASAPIPROC _process;
        private float _speed1 = 1.0F, _speed2 = 0.20F;
        private int _lastlevel;             //last output level
        private int _hanctr;
        private AudioDevice _device;
        public AudioCaptureBasic(AudioDevice device)
        {
            _process = new WASAPIPROC(Process);
            _fft = new float[1024];
            _device = device;
        }
        private int Process(IntPtr buffer, int length, IntPtr user)
        {
            return length;
        }
        public void FreeBassWasapi()
        {
            BassWasapi.BASS_WASAPI_Free();
        }
        public void StartBassWasapi()
        {
            bool result = BassWasapi.BASS_WASAPI_Init(_device.Index, 0, 0, BASSWASAPIInit.BASS_WASAPI_BUFFER, 1f, 0.05f, _process, IntPtr.Zero); // this is the function to init the device according to device index
            if (!result)
            {
                var error = Bass.BASS_ErrorGetCode();
                Log.Error(error.ToString());
                // MessageBox.Show(error.ToString());
            }
            bool dev = BassWasapi.BASS_WASAPI_SetDevice(_device.Index);
            if (!dev)
            {
                var err = Bass.BASS_ErrorGetCode();
            }

            else
            {


            }
            BassWasapi.BASS_WASAPI_Start();
        }
        public bool GetCurrentFFTFrame(int numFreq, byte[] lastSpectrumData)
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
    }
}
