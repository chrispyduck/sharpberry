using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sharpberry.controller
{
    public class PowerEventArgs : EventArgs
    {
        public PowerEventArgs(bool vAcc, bool vBatt)
        {
            this.vAcc = vAcc;
            this.vBatt = vBatt;
        }

        public bool vAcc { get; private set; }
        public bool vBatt { get; private set; }
    }
}
