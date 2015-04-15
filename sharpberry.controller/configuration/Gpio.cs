using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raspberry.IO.GeneralPurpose;

namespace sharpberry.controller.configuration
{
    public class Gpio
    {
        public ConnectorPin vAcc { get; set; }
        public ConnectorPin vBatt { get; set; }
        public ConnectorPin PowerLed { get; set; }
        public ConnectorPin ActivityLed { get; set; }
    }
}
