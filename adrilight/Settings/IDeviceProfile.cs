using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace adrilight.Settings
{
    public interface IDeviceProfile : INotifyPropertyChanged
    {
        string Name { get; set; }
        string DeviceType { get; set; }
        string Owner { get; set; }
        string Description { get; set; }
        string Geometry { get; set; }
        string ProfileUID { get; set; }
        IOutputSettings UnionOutput { get; set; }
        IOutputSettings[] OutputSettings { get; set; }
        void SaveProfile(IOutputSettings unionOutput, IOutputSettings[] availableOutputs);
    }
}
