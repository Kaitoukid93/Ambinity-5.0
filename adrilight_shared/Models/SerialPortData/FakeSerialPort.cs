using Serilog;
using System.IO.Ports;

namespace adrilight_shared.Models.SerialPortData
{
    public class FakeSerialPort : ISerialPortWrapper
    {
        public FakeSerialPort() => Log.Warning("FakeSerialPort created!");

        public bool IsOpen { get; private set; }
        public void Open() => IsOpen = true;
        public void Close() => IsOpen = false;
        public SerialPort SerialPort { get; set; }
        private static readonly FpsLogger fpsLogger = new FpsLogger("FakeSerialPort");
        public void Dispose()
        {
            fpsLogger?.Dispose();
        }

        public void Write(byte[] outputBuffer, int v, int streamLength)
        {
            //_log.Warn($"Faking writing of {streamLength} bytes to the serial port");
            fpsLogger.TrackSingleFrame();
        }

        public void Print(string outputBuffer)
        {
            //_log.Warn($"Faking writing of {streamLength} bytes to the serial port");
            fpsLogger.TrackSingleFrame();
        }
        public void Read(byte[] inputBuffer, int v, int streamLength)
        {
            //_log.Warn($"Faking writing of {streamLength} bytes to the serial port");
            fpsLogger.TrackSingleFrame();
        }
        public int BytesToRead { get; }



        public int ReadByte()
        {

            return 0;

        }
    }
}
