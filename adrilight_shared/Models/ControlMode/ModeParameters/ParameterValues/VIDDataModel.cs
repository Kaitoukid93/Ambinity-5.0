using GalaSoft.MvvmLight;
using Newtonsoft.Json;

namespace adrilight_shared.Models.ControlMode.ModeParameters.ParameterValues
{
    public class VIDDataModel : ViewModelBase, IParameterValue
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public VIDType ExecutionType { get; set; }
        public BrushData[] DrawingPath { get; set; }
        private string _geometry = "binary";
        public VIDDirrection Dirrection { get; set; }
        private bool _isChecked = false;
        private bool _isDeleteable = true;
        public string Geometry { get => _geometry; set { Set(() => Geometry, ref _geometry, value); } }
        [JsonIgnore]
        public bool IsDeleteable { get => _isDeleteable; set { Set(() => IsChecked, ref _isDeleteable, value); } }
        [JsonIgnore]
        public bool IsChecked { get => _isChecked; set { Set(() => IsChecked, ref _isChecked, value); } }
        [JsonIgnore]
        public string LocalPath { get; set; }
        [JsonIgnore]
        public string InfoPath { get; set; }
    }
}
