using System.Windows.Media;

namespace adrilight.Services.NetworkStream
{
    class XmlApiResponse
    {
        public byte Brightness { get; set; } = 128;
        public bool IsOn { get; set; } = false;
        public Color LightColor { get; set; }
        public string Name { get; set; } = "";
    }
}
