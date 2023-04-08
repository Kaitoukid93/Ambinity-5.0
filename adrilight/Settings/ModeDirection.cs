namespace adrilight.Settings
{
    public class ModeDirection
    {
        public ModeDirection(string name, int dirrection)
        {

            Name = name;
            Direction = dirrection;

        }
        public string Name { get; set; }
        public int Direction { get; set; }
    }
}