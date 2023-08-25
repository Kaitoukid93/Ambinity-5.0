using System;
using System.IO.Ports;

namespace adrilight_shared.Models.SerialPortData
{
    interface ISerialPortWrapper : IDisposable
    {
        bool IsOpen { get; }
        SerialPort SerialPort { get; }

        void Close();
        void Open();
        void Write(byte[] outputBuffer, int v, int streamLength);
        void Read(byte[] inputBuffer, int v, int streamLength);
        void Print(string outputBuffer);
        int BytesToRead { get; }
        int ReadByte();

        // void Print(byte[] outputBuffer);
    }
}
