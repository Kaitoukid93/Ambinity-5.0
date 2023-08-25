namespace adrilight_shared.Models.ControlMode.ModeParameters
{
    public class CapturingRegion
    {
        public CapturingRegion()
        {

        }
        public CapturingRegion(double scaleX, double scaleY, double scaleWidth, double scaleHeight)
        {
            ScaleX = scaleX;
            ScaleY = scaleY;
            ScaleWidth = scaleWidth;
            ScaleHeight = scaleHeight;
        }
        public double ScaleX { get; set; }
        public double ScaleY { get; set; }
        public double ScaleWidth { get; set; }
        public double ScaleHeight { get; set; }
    }
}
