using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sharpberry.gpio
{
    public interface IGpioProxy 
    {
        bool PowerLed { get; set; }
        bool ActivityLed { get; set; }

        bool vAcc { get; set; }
        bool vBatt { get; set; }

        event EventHandler<PinChangeEventArgs> PinChanged;
    }
}
