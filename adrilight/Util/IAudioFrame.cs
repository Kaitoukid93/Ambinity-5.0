using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.Util
{
    public interface IAudioFrame : INotifyPropertyChanged
    {
        float[] FFT { get; set; }
        IList<string> AvailableAudioDevice { get; }
    }
}
