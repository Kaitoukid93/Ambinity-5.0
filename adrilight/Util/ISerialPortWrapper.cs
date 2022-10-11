﻿using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.Util
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
