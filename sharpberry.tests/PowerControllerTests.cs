using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using sharpberry.configuration;
using sharpberry.controllers;
using sharpberry.gpio;

namespace sharpberry.controller.tests
{
    [TestFixture]
    public class PowerControllerTests
    {
        [SetUp]
        public void Setup()
        {
            mock = new Mock<IGpioProxy>();
            mock.SetupAllProperties();
        }

        private Mock<IGpioProxy> mock;
        private Config config = new Config();

        [Test]
        public void RaisePowerOnEvent()
        {
            var c = new PowerController(mock.Object);
            var raised = false;
            c.Event += (sender, args) =>
                {
                    if (raised == true)
                        Assert.Fail("Power on event raised more than once");
                    if (args.EventType == ControllerEventType.RequestExitPowerSave)
                        raised = true;
                    else
                        Assert.Fail("Unexpected event was raised: {0}", args.EventType);
                };
            Assert.IsFalse(raised, "Test not started yet");
            mock.Object.vAcc = true;
            mock.Raise(x => x.PinChanged += null, mock.Object, new PinChangeEventArgs("vAcc", true));
            Assert.IsTrue(raised, "Event was raised");
        }

        [Test]
        public void RaisePowerOffEvent()
        {
            mock.Object.vAcc = true;
            var c = new PowerController(mock.Object);
            var raised = false;
            c.Event += (sender, args) =>
            {
                if (raised == true)
                    Assert.Fail("Power off event raised more than once");
                if (args.EventType == ControllerEventType.RequestEnterPowerSave)
                    raised = true;
                else
                    Assert.Fail("Unexpected event was raised: {0}", args.EventType);
            };
            Assert.IsFalse(raised, "Test not started yet");
            mock.Object.vAcc = false;
            mock.Raise(x => x.PinChanged += null, mock.Object, new PinChangeEventArgs("vAcc", false));
            Assert.IsTrue(raised, "Event was raised");
        }
    }
}
