using System.Windows;

namespace adrilight_shared.Models.ControlMode.ModeParameters
{
    public class CapturingRegionPreview : PreviewableContent
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