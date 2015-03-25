using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using sharpberry.obd.Commands;

namespace sharpberry.obd.tests
{
    [TestFixture]
    public class ObdSimTest
    {
        [Test]
        public void ObdSim()
        {
            ObdCommands.LoadDefaults();

            var obd = new ObdInterface("COM5", 115200);
            obd.Connect().Wait();
            obd.QuerySupportedCommands().Wait();
        }
    }
}
