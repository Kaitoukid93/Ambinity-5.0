namespace adrilight_shared.Models.Automation
{
    public interface ITriggerCondition
    {
        string Name { get; set; }
        string Description { get; set; }
        string Geometry { get; set; }
    }
}