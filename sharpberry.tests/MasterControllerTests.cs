using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using Moq;
using NUnit.Framework;
using sharpberry.controllers;
using sharpberry.data.collection;
using sharpberry.gpio;
using sharpberry.obd;

namespace sharpberry.controller.tests
{
    [TestFixture]
    public class MasterControllerTests
    {
        [Test]
        public void EventPropogation()
        {
            var c1 = new Mock<IController>();
            var c2 = new Mock<IController>();
            var c3 = new Mock<IController>();

            var container = new UnityContainer();
            container.RegisterInstance("c1", c1.Object);
            container.RegisterInstance("c2", c2.Object);
            container.RegisterInstance("c3", c3.Object);
            var mc = new MasterController(container);

            c1.Raise(c => c.Event += null, c1.Object, new ControllerEventArgs(ControllerEventType.Custom, "data"));
            c1.Verify(c => c.HandleEvent(It.IsAny<object>(), It.IsAny<ControllerEventArgs>()), Times.Never());
            c2.Verify(c => c.HandleEvent(It.IsAny<object>(), It.IsAny<ControllerEventArgs>()), Times.Once());
            c3.Verify(c => c.HandleEvent(It.IsAny<object>(), It.IsAny<ControllerEventArgs>()), Times.Once());

            c1.ResetCalls();
            c2.ResetCalls();
            c3.ResetCalls();
            c2.Raise(c => c.Event += null, c2.Object, new ControllerEventArgs(ControllerEventType.ReceiveData));
            c1.Verify(c => c.HandleEvent(It.IsAny<object>(), It.IsAny<ControllerEventArgs>()), Times.Once());
            c2.Verify(c => c.HandleEvent(It.IsAny<object>(), It.IsAny<ControllerEventArgs>()), Times.Never());
            c3.Verify(c => c.HandleEvent(It.IsAny<object>(), It.IsAny<ControllerEventArgs>()), Times.Once());
        }

        [Test]
        public void StartupPriority()
        {
            var c1 = new Mock<IController>();
            var c2 = new Mock<IController>();
            var c3 = new Mock<IController>();
            c1.SetupGet(x => x.StartupPriority).Returns(controllers.StartupPriority.DevicePower);
            c2.SetupGet(x => x.StartupPriority).Returns(controllers.StartupPriority.DeviceConfiguration);
            c3.SetupGet(x => x.StartupPriority).Returns(controllers.StartupPriority.DataCollection);

            var container = new UnityContainer();
            container.RegisterInstance("c1", c1.Object);
            container.RegisterInstance("c2", c2.Object);
            container.RegisterInstance("c3", c3.Object);
            var mc = new MasterController(container);
        }
    }
}
