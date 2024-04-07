namespace adrilight.Services.SerialStream
{

    public interface ISerialStream
    {
        bool IsRunning { get; }

        void Start();
        void Stop();
        bool IsValid();
        void DFU();



    }
}