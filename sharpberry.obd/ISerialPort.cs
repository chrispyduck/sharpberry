using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sharpberry.obd
{
    public interface ISerialPort : IDisposable 
    {
        string PortName { get; set; }
        int BaudRate { get; set; }

        void Open();
        void Close();

        void Send(string input);
        string Receive();

        event EventHandler DataReceived;
        event EventHandler Error;
    }
}
