﻿using System.IO.Ports;

namespace WpfSerialPort
{
    public class SerialPortConfiguration
    {
        public string PortName { get; set; }

        public int BaudRate { get; set; }

        public Parity Parity { get; set; }

        public int DataBits { get; set; }

        public StopBits StopBits { get; set; }

    }
}
