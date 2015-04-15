using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Raspberry.IO.GeneralPurpose;

namespace sharpberry.controller.tests
{
    [SetUpFixture]
    public class Setup
    {
        [SetUp]
        public void Configure()
        {
            configuration.Config.Gpio.vAcc = ConnectorPin.P1Pin5;
            configuration.Config.Gpio.vBatt = ConnectorPin.P1Pin7;
            configuration.Config.Gpio.PowerLed = ConnectorPin.P1Pin08;
            configuration.Config.Gpio.ActivityLed = ConnectorPin.P1Pin10;
        }
    }
}
