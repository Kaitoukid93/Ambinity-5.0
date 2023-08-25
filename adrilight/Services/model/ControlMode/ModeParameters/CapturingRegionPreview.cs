using System.Windows;

namespace adrilight.Util.ModeParameters
{
    internal class CapturingRegionPreview : PreviewableContent
    {
        public CapturingRegionPreview()
        {
            Type = PreviewableContentEnum.CapturingRegion;
        }
        public PreviewableContentEnum Type { get; set; }
        public Rect Region { get; set; }
        public Rect Canvas { get; set; }
        public string SourceName { get; set; }
    }
}