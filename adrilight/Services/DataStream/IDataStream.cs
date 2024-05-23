using adrilight_shared.Models.Device;

namespace adrilight.Services.DataStream
{

    public interface IDataStream
    {
        bool IsRunning { get; }
        void Init(IDeviceSettings device);
        void Start();
        void Stop();
        bool IsValid();

    }
}