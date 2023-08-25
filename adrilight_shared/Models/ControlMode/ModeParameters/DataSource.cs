using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;

namespace adrilight_shared.Models.ControlMode.ModeParameters
{
    public class DataSource : ViewModelBase
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public ObservableCollection<IParameterValue> AvailableValues { get; set; }
    }
}