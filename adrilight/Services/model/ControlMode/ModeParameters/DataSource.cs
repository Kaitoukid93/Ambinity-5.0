using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using System.Windows.Forms.VisualStyles;

namespace adrilight.Util.ModeParameters
{
    public class DataSource : ViewModelBase
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public ObservableCollection<IParameterValue> AvailableValues { get; set; }
    }
}