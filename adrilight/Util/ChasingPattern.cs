using GalaSoft.MvvmLight;

namespace adrilight.Util
{
    public class ChasingPattern : ViewModelBase, IParameterValue
    {

        public ChasingPattern()
        {

        }
        private Tick _tick;
        public string Name { get; set; }
        public string Owner { get; set; }
        public ChasingPatternTypeEnum Type { get; set; }
        public string Description { get; set; }
        public string Path { get; set; }
        public Tick Tick { get => _tick; set { Set(() => Tick, ref _tick, value); } }
        private string _version = "1.0.0";
        public string Version { get => _version; set { Set(() => Version, ref _version, value); } }
    }
}

