using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using sharpberry.configuration;
using sharpberry.controllers;
using sharpberry.gpio;

namespace sharpberry.controller.tests
{
    [TestFixture]
    public class LedControllerTests
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
        public void TestInitialize()
        {
            var controller = new LedController(mock.Object, config);
            Thread.Sleep(config.Display.InitializingDelay * 6);
            mock.VerifySet(x => x.PowerLed = true, Moq.Times.Between(3, 4, Range.Inclusive));
            mock.VerifySet(x => x.ActivityLed = true, Moq.Times.Between(3, 4, Range.Inclusive));
            mock.VerifySet(x => x.PowerLed = !x.ActivityLed, Moq.Times.Between(3, 4, Range.Inclusive));
        }

        [Test]
        public void TestStandby()
        {
            var controller = new LedController(mock.Object, config);
            controller.HandleEvent(this, new ControllerEventArgs(ControllerEventType.RequestEnterPowerSave));
            mock.ResetCalls();
            Thread.Sleep((int)(config.Display.StandbyDelay*3.5));
            mock.VerifySet(x => x.PowerLed = true, Moq.Times.Between(3, 4, Range.Inclusive));
            mock.VerifySet(x => x.ActivityLed = true, Moq.Times.Between(3, 4, Range.Inclusive));
            mock.VerifySet(x => x.ActivityLed = x.PowerLed, Moq.Times.Between(3, 4, Range.Inclusive));
        }

        [Test]
        public void TestOn()
        {
            var controller = new LedController(mock.Object, config);
            mock.Object.ActivityLed = true;
            mock.ResetCalls();
            controller.HandleEvent(this, new ControllerEventArgs(ControllerEventType.RequestExitPowerSave));
            Thread.Sleep((int)(config.Display.StandbyDelay * 3));
            mock.VerifySet(x => x.PowerLed = true, Moq.Times.AtLeastOnce());
            mock.VerifySet(x => x.ActivityLed = true, Moq.Times.Never());
            mock.VerifySet(x => x.ActivityLed = false, Moq.Times.Once());
            Assert.IsTrue(mock.Object.PowerLed, "Power LED");
            Assert.IsFalse(mock.Object.ActivityLed, "Activity LED");
        }
    }
}
