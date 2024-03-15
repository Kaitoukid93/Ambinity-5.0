namespace adrilight_shared.Models.Device
{
    public interface ISubDeviceSettings
    {
        int SubDeviceAddress { get; set; }
        string SubDeviceName { get; set; }
        int SubDeviceMaxOutputs { get; set; } 
        string ParrentID { get; set; }
    }
}