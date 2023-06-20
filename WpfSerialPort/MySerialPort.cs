using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;

namespace WpfSerialPort
{
    public class MySerialPort
    {
        private SerialPort? serialPort = null;
        public delegate bool ReceiveClientData(string data);
        public event ReceiveClientData ReceiveClientDataEvent;
        public event ReceiveClientData SendClientDataEvent;
        /// <summary>
        /// 获取该设备所有COM
        /// </summary>
        /// <returns></returns>
        public List<ComboxInfo> GetALLCOM()
        {
            var Arrstr = SerialPort.GetPortNames();
            var list = new List<ComboxInfo>();
            for (int i = 0; i < Arrstr.Length; i++)
            {
                list.Add(new ComboxInfo
                {
                    Id = i,
                    Name = Arrstr[i]
                });
            }
            return list;
        }

        public bool OpenSerialPort(SerialPortConfiguration serialPortConfiguration)
        {
            serialPort = new SerialPort
            {
                PortName = serialPortConfiguration.PortName,
                BaudRate = serialPortConfiguration.BaudRate,
                Parity = serialPortConfiguration.Parity,
                DataBits = serialPortConfiguration.DataBits,
                StopBits = serialPortConfiguration.StopBits
            };
            serialPort.DataReceived += new SerialDataReceivedEventHandler(SerialDataReceived);
            if (!serialPort.IsOpen)
                serialPort.Open();
            ReceiveClientDataEvent(string.Format("#Sp000-{0}", serialPortConfiguration.PortName));
            return true;
        }

        public bool CloseSerialPort()
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Close();
                ReceiveClientDataEvent(string.Format("#Sp001-{0}", serialPort.PortName));
                serialPort = new SerialPort();
                return true;
            }
            return false;
        }

        private void SerialDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (ReceiveClientDataEvent("#HEX"))
                {
                    int length = serialPort.BytesToRead;
                    byte[] bytes = new byte[serialPort.BytesToRead];
                    serialPort.Write(bytes, 0, length);
                    ReceiveClientDataEvent(ConverToString(bytes));
                }
                else
                {
                    string ReceiveData = serialPort.ReadExisting();
                    ReceiveClientDataEvent(ReceiveData);
                }
            }));
        }

        public bool SendSerialData(string data)
        {
            try
            {
                if (serialPort != null && serialPort.IsOpen)
                {
                    if (SendClientDataEvent("#HEX"))
                    {
                        byte[] sendbytes = StringToConver(data);
                        serialPort.Write(sendbytes, 0, sendbytes.Length);
                    }
                    else
                    {
                        serialPort.Write(data);
                    }
                    return true;
                }

            }
            catch (Exception ex)
            {

            }
            return false;
        }


        private byte[] StringToConver(string stringData)
        {
            String[] SendArr = stringData.Split(' ');
            byte[] decBytes = new byte[SendArr.Length];
            for (int i = 0; i < SendArr.Length; i++)
                decBytes[i] = Convert.ToByte(SendArr[i], 16);
            return decBytes;
        }

        private string ConverToString(byte[] bytesData)
        {
            StringBuilder stb = new StringBuilder();
            for (int i = 0; i < bytesData.Length; i++)
            {
                if ((int)bytesData[i] > 15)
                {
                    stb.Append(Convert.ToString(bytesData[i], 16).ToUpper());
                }
                else
                {
                    stb.Append("0" + Convert.ToString(bytesData[i], 16).ToUpper());
                }
                if (i != bytesData.Length - 1)
                    stb.Append(" ");
            }
            return stb.ToString();
        }

        /// <summary>
        /// 16进制转字符串
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public string HexToStr(string hex)
        {
            var hexString = hex.Replace(" ", "");
            byte[] buffer = new byte[hexString.Length / 2];
            string result = string.Empty;
            for (int i = 0; i < hexString.Length / 2; i++)
            {
                result = hexString.Substring(i * 2, 2);
                buffer[i] = Convert.ToByte(result, 16);
            }
            return Encoding.Default.GetString(buffer);
        }

        /// <summary>
        /// 字节转16进制
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public string StrToHex(string str)
        {
            byte[] buffer = Encoding.Default.GetBytes(str);
            string result = string.Empty;
            foreach (char c in buffer)
            {
                result += Convert.ToString(c, 16) + " ";
            }
            return result.TrimEnd().ToUpper();
        }


    }
    public class ComboxInfo : BindableBase
    {
        private int id;

        public int Id
        {
            get { return id; }
            set { id = value; RaisePropertyChanged(); }
        }

        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; RaisePropertyChanged(); }
        }

    }
}
