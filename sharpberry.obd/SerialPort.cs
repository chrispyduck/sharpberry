using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace sharpberry.obd
{
    public class SerialPort : ISerialPort
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public SerialPort()
        {
            this.serialPort = new System.IO.Ports.SerialPort();
            this.serialPort.Encoding = System.Text.Encoding.ASCII;
            this.serialPort.DiscardNull = true;
            this.serialPort.Handshake = System.IO.Ports.Handshake.None;
            this.serialPort.DataReceived += (sender, args) => Fire(this.DataReceived, args);
            this.serialPort.ErrorReceived += (sender, args) => Fire(this.Error, args);
        }

        private readonly System.IO.Ports.SerialPort serialPort;

        void Fire<T>(EventHandler handler, T args = null)
            where T : EventArgs
        {
            if (handler == null)
                return;

            handler.Invoke(this, args);
        }

        public void Dispose()
        {
            this.serialPort.Dispose();
        }

        public string PortName 
        {
            get { return this.serialPort.PortName; }
            set { this.serialPort.PortName = value; }
        }
        
        public int BaudRate
        {
            get { return this.serialPort.BaudRate; }
            set { this.serialPort.BaudRate = value; }
        }
        
        public void Open()
        {
            this.serialPort.Open();
        }

        public void Close()
        {
            this.serialPort.Close();
        }

        public void Send(string input)
        {
            Logger.Debug("--> " + input.Replace("\r", "\\r").Replace("\n", "\\n"));
            this.serialPort.Write(input);
        }

        public string Receive()
        {
            var str = this.serialPort.ReadExisting();
            Logger.Debug("<-- " + str.Replace("\r", "\\r").Replace("\n", "\\n"));
            return str;
        }

        public event EventHandler DataReceived;
        public event EventHandler Error;
    }
}
