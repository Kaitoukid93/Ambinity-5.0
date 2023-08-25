namespace adrilight_shared.Models.FrameData
{
    public class Motion
    {
        public Motion(int FrameCount)
        {
            Frames = new Frame[FrameCount];
        }
        public Motion(string name)
        {
            Name = name;
        }
        public Motion()
        {

        }
        public Frame[] Frames { get; set; }
        public string GUID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Owner { get; set; }

    }
}
