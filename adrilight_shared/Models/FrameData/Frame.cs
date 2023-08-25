namespace adrilight_shared.Models.FrameData
{
    public class Frame
    {
        public Frame(int numPixel)
        {
            BrightnessData = new byte[numPixel];
        }
        public Frame()
        {
            BrightnessData = new byte[256];
        }
        public byte[] BrightnessData { get; set; }

    }
}
