using adrilight_shared.Models.Device;

namespace adrilight.Services.DataStream
{

    public interface IDataStream
    {
        bool IsRunning { get; }
        string ID { get; set; }
        void Init(IDeviceSettings device);
        void Start();
        void Stop();
        bool IsValid();

    }
}