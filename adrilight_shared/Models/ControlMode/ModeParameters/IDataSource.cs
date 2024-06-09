
using System.ComponentModel;

namespace adrilight_shared.Models.ControlMode.ModeParameters
{
    public interface IDataSource :INotifyPropertyChanged
    {
        public string Name { get; set; }
    }
}