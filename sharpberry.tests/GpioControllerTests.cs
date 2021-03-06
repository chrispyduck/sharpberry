﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Raspberry.IO.GeneralPurpose;
using sharpberry.gpio;

namespace sharpberry.controller.tests
{
    [TestFixture]
    public class GpioControllerTests
    {

        [Test]
        public void PowerStateChange_vAcc()
        {
            /*
            var driver = new Moq.Mock<IGpioConnectionDriver>();
            var flag = false;
            var flagChange = false;
            var testPin = (ProcessorPin)configuration.Config.Gpio.vAcc;
            driver.Setup(x => x.Read(testPin))
                .Returns(() => flag);
            var gpio = new GpioController(driver.Object);
            var calls = 0;
            gpio.PowerEvent += (sender, args) =>
                {
                    calls++;
                    Assert.AreEqual(flag, args.vAcc);
                    Assert.AreEqual(flagChange, args.vAccChanged);
                };
            var checkFn = typeof (GpioConnection).GetMethod("CheckInputPins", BindingFlags.NonPublic | BindingFlags.Instance);
            
            // wait for PinStatusChanged event
            Thread.Sleep(200);
            checkFn.Invoke(gpio.connection, new object[] { });
            flag = true;
            flagChange = false;

            // wait for PinStatusChanged event
            checkFn.Invoke(gpio.connection, new object[] { });
            flag = false;
            flagChange = true;

            // wait for PinStatusChanged event
            checkFn.Invoke(gpio.connection, new object[] { });
            flag = true;
            flagChange = true;

            Assert.AreEqual(3, calls, "Number of calls to PowerEvent event handler");
             */
        }

        [Test]
        public void LedBehavior_Standby()
        {/*
            const int blinks = 3;
            var proxy = new Moq.Mock<IGpioProxy>();
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
            using (var gpio = new GpioController(driver.Object))
            {
                sw.Start();
                Trace.TraceInformation("Starting count");
                setPower = 0;
                setAct = 0;
                gpio.Behavior = LedBehavior.Standby;
                Thread.Sleep((GpioController.StandbyDelay + GpioController.BlinkDelay)*blinks);
            }

            sw.Stop();
            var expectedBlinks = sw.ElapsedMilliseconds/(GpioController.StandbyDelay + GpioController.BlinkDelay);
            Assert.AreEqual(expectedBlinks, setPower);
            Assert.AreEqual(expectedBlinks, setAct);*/
        }
    }
}
