namespace adrilight.Util
{
    public class Tick
    {
        public Tick() { }
        public double CurrentTick { get; set; }
        public TickEnum TickType { get; set; }
        public int TickRate { get; set; } = 1;
        public int MaxTick { get; set; }
        public double TickSpeed { get; set; }
        public bool IsRunning { get; set; }
        public string TickUID { get; set; }
    }
}
