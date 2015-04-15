using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Raspberry.IO.GeneralPurpose;

namespace sharpberry.controller.tests
{
    [TestFixture]
    public class GpioControllerTests
    {
        [Test]
        public void LedBehavior_Standby()
        {
            const int blinks = 3;
            var driver = new Moq.Mock<IGpioConnectionDriver>();
            var setPower = 0;
            var setAct = 0;
            driver.Setup(x => x.Write(It.IsAny<ProcessorPin>(), It.IsAny<bool>()))
                .Callback((ProcessorPin pin, bool value) =>
                    {
                        var con = pin.ToConnector();
                        if (con == configuration.Config.Gpio.ActivityLed)
                        {
                            if (value)
                                setAct++;
                            Trace.TraceInformation("Activity LED: {0}", value);
                        }
                        else if (con == configuration.Config.Gpio.PowerLed)
                        {
                            if (value)
                                setPower++;
                            Trace.TraceInformation("Power LED: {0}", value);
                        }
                    });

            var sw = new Stopwatch();
            using (var gpio = new Gpio(driver.Object))
            {
                sw.Start();
                Trace.TraceInformation("Starting count");
                setPower = 0;
                setAct = 0;
                gpio.Behavior = LedBehavior.Standby;
                Thread.Sleep((Gpio.StandbyDelay + Gpio.BlinkDelay)*blinks);
            }

            sw.Stop();
            var expectedBlinks = sw.ElapsedMilliseconds/(Gpio.StandbyDelay + Gpio.BlinkDelay);
            Assert.AreEqual(expectedBlinks, setPower);
            Assert.AreEqual(expectedBlinks, setAct);
        }
    }
}
