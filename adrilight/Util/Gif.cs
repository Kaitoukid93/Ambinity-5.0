using GalaSoft.MvvmLight;

namespace adrilight.Util
{
    internal class Gif : ViewModelBase, IParameterValue
    {

        public Gif(string path)
        {
            Path = path;
        }
        public Gif()
        {

        }
        public string Name { get; set; }
        public string Owner { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string Path { get; set; }
    }
}
