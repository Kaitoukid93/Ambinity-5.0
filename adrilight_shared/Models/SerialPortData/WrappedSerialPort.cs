using System;
using System.IO.Ports;

namespace adrilight_shared.Models.SerialPortData
{
    public class WrappedSerialPort : ISerialPortWrapper
    {
        public WrappedSerialPort(SerialPort serialPort)
        {
            SerialPort = serialPort ?? throw new ArgumentNullException(nameof(serialPort));
        }

        public SerialPort SerialPort { get; }

        public bool IsOpen => SerialPort.IsOpen;

        public void Close() => SerialPort.Close();

        public void Open() => SerialPort.Open();

        public void Write(byte[] outputBuffer, int v, int streamLength) => SerialPort.BaseStream.Write(outputBuffer, v, streamLength);
        public void Print(string outputBuffer) => SerialPort.Write(outputBuffer);
        public void Read(byte[] inputBuffer, int v, int streamLength) => SerialPort.Read(inputBuffer, v, streamLength);


        public int ReadByte()

        {
            int touchvalue = SerialPort.ReadByte();
            return touchvalue;
        }
        public int BytesToRead => SerialPort.BytesToRead;
        public void Dispose() => SerialPort.Dispose();

    }
}
