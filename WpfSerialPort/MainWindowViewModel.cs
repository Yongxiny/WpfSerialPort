﻿using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WpfSerialPort
{
    public class MainWindowViewModel : BindableBase
    {
        public MainWindowViewModel(MySerialPort mySerialPort)
        {
            this.mySerialPort = mySerialPort;
            OpenOrClose = "打开";
            IsReadOnly = true;
            OperateCommand = new DelegateCommand<string>(Operate);
            OperateContentCommand = new DelegateCommand<string>(OperateContent);
            ConvertCommand = new DelegateCommand<object>(ConvertData);
            StopDisplayCheckedCommand = new DelegateCommand(StopDisplayChecked);

            ComNames = new ObservableCollection<ComboxInfo>();
            MessageList = new ObservableCollection<string>();
            foreach (var item in mySerialPort.GetALLCOM())
            {
                ComNames.Add(item);
            }
            mySerialPort.ReceiveClientDataEvent += MySerialPort_ReceiveClientDataEvent;
            mySerialPort.SendClientDataEvent += MySerialPort_SendClientDataEvent;

        }



        #region 属性

        private readonly MySerialPort mySerialPort;
        private SerialPortConfiguration serial;
        public SerialPortConfiguration SerialPortInfo
        {
            get { return serial; }
            set { serial = value; RaisePropertyChanged(); }
        }

        public DelegateCommand<string> OperateCommand { get; private set; }

        public DelegateCommand<string> OperateContentCommand { get; private set; }

        public DelegateCommand<object> ConvertCommand { get; private set; }

        public DelegateCommand StopDisplayCheckedCommand { get; private set; }

        private ObservableCollection<ComboxInfo> comNames;

        public ObservableCollection<ComboxInfo> ComNames
        {
            get { return comNames; }
            set { comNames = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<string> messageList;
        public ObservableCollection<string> MessageList
        {
            get { return messageList; }
            set { messageList = value; RaisePropertyChanged(); }
        }

        private string openOrClose;

        public string OpenOrClose
        {
            get { return openOrClose; }
            set { openOrClose = value; RaisePropertyChanged(); }
        }

        private ComboxInfo cmbCom;

        public ComboxInfo CmbCom
        {
            get { return cmbCom; }
            set { cmbCom = value; RaisePropertyChanged(); }
        }

        private ComboBoxItem cmbBaudRate;

        public ComboBoxItem CmbBaudRate
        {
            get { return cmbBaudRate; }
            set { cmbBaudRate = value; RaisePropertyChanged(); }
        }

        private ComboBoxItem cmbParity;

        public ComboBoxItem CmbParity
        {
            get { return cmbParity; }
            set { cmbParity = value; RaisePropertyChanged(); }
        }

        private ComboBoxItem cmbdataBits;

        public ComboBoxItem CmbDataBits
        {
            get { return cmbdataBits; }
            set { cmbdataBits = value; RaisePropertyChanged(); }
        }

        private ComboBoxItem cmbstopBits;


        public ComboBoxItem CmbStopBits
        {
            get { return cmbstopBits; }
            set { cmbstopBits = value; RaisePropertyChanged(); }
        }

        private string receivedData;

        public string ReceivedData
        {
            get { return receivedData; }
            set { receivedData = value; RaisePropertyChanged(); }
        }

        private string sendData;

        public string SendData
        {
            get { return sendData; }
            set { sendData = value; RaisePropertyChanged(); }
        }

        private string textMessage;

        public string TextMessage
        {
            get { return textMessage; }
            set { textMessage = value; RaisePropertyChanged(); }
        }

        private bool isReadOnly;

        public bool IsReadOnly
        {
            get { return isReadOnly; }
            set { isReadOnly = value; RaisePropertyChanged(); }
        }

        private bool sendChecked;

        public bool SendChecked
        {
            get { return sendChecked; }
            set { sendChecked = value; RaisePropertyChanged(); }
        }

        private bool receiveChecked;

        public bool ReceiveChecked
        {
            get { return receiveChecked; }
            set { receiveChecked = value; RaisePropertyChanged(); }
        }

        private bool displayChecked;

        public bool DisplayChecked
        {
            get { return displayChecked; }
            set { displayChecked = value; RaisePropertyChanged(); }
        }
        private bool receiveIsEnable;

        public bool ReceiveIsEnable
        {
            get { return receiveIsEnable; }
            set { receiveIsEnable = value; RaisePropertyChanged(); }
        }


        #endregion
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private bool MySerialPort_SendClientDataEvent(string data)
        {
            if (data == "#HEX")
                return SendChecked;
            else
                return false;
        }
        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private bool MySerialPort_ReceiveClientDataEvent(string data)
        {
            if (data == "#HEX")
                return ReceiveChecked;

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!ReceSerialPort(data))
                    return;
                if (DisplayChecked)
                    ReceivedData += DateTime.Now + ":" + data + "\r\n";
                else
                    ReceivedData += data;
            }));
            return true;
        }

        private bool ReceSerialPort(string strIn)
        {
            var strCompare = strIn.Split('-');
            if (strCompare.Count() > 1)
            {
                if (strCompare[0] == "#Info000")
                {
                    TextMessage = SerialPortInfo.PortName + "已连接";
                    MessageList.Add(string.Format("{0}:{1}", DateTime.Now.ToString("T"), TextMessage));
                    return false;
                }
                if (strCompare[0] == "#Info001")
                {
                    TextMessage = SerialPortInfo.PortName + "已断开";
                    MessageList.Add(string.Format("{0}:{1}", DateTime.Now.ToString("T"), TextMessage));
                    return false;
                }
                if (strCompare[0] == "#Error000")
                {
                    TextMessage = strIn;
                    MessageList.Add(string.Format("{0}:{1}", DateTime.Now.ToString("T"), TextMessage));
                    return false;
                }
                if (strCompare[0] == "#Error001")
                {
                    TextMessage = strIn;
                    MessageList.Add(string.Format("{0}:{1}", DateTime.Now.ToString("T"), TextMessage));
                    return false;
                }
            }
            return true;
        }

        private void OperateContent(string obj)
        {
            try
            {
                switch (obj)
                {
                    case "Send":
                        if (mySerialPort.SendSerialData(SendData))
                            ReceiveIsEnable = true;
                        else
                            ReceiveIsEnable = false;
                        break;
                    case "ClearSend":
                        SendData = string.Empty;
                        break;
                    case "ClearReceived":
                        ReceivedData = string.Empty;
                        break;
                }
            }
            catch (Exception ex)
            {
                TextMessage = string.Format("#Error002-{0}", ex.Message);
                MessageList.Add(string.Format("{0}:{1}", DateTime.Now.ToString("T"), TextMessage));
            }
        }

        private void StopDisplayChecked()
        {
            if (ReceiveIsEnable)
                ReceiveIsEnable = false;
            else
                ReceiveIsEnable = true;
        }

        private void Operate(string obj)
        {
            try
            {
                if (obj == "打开")
                {

                    SerialPortInfo = new SerialPortConfiguration()
                    {
                        PortName = CmbCom.Name,
                        BaudRate = Convert.ToInt32(CmbBaudRate.Content),
                        DataBits = Convert.ToInt32(CmbDataBits.Content),
                        Parity = (Parity)Enum.Parse(typeof(Parity), CmbParity.Content.ToString()),
                        StopBits = (StopBits)Enum.Parse(typeof(StopBits), CmbStopBits.Content.ToString()),
                    };
                    OpenOrClose = "关闭";
                    IsReadOnly = false;
                    mySerialPort.OpenSerialPort(SerialPortInfo);
                }
                else
                {
                    mySerialPort.CloseSerialPort();
                    IsReadOnly = true;
                    OpenOrClose = "打开";
                }
            }
            catch (Exception ex)
            {
                TextMessage = string.Format("#Error002-{0}", ex.Message);
                MessageList.Add(string.Format("{0}:{1}", DateTime.Now.ToString("T"), TextMessage));
            }
        }

        private void ConvertData(object obj)
        {
            if ((bool)obj)
                SendData = mySerialPort.StrToHex(SendData);
            else
                SendData = mySerialPort.HexToStr(SendData);
        }
    }
}
