using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using sharpberry.obd.Commands;
using sharpberry.obd.Responses;

namespace sharpberry.obd.tests
{
    [TestFixture]
    public class ObdInterfaceTests : ObdInterfaceTestFixture
    {
        [TestFixtureSetUp]
        public void LoadCommands()
        {
            ObdCommands.LoadDefaults();
            Assert.Greater(ObdCommands.All.Count, 0, "Commands have been loaded");
        }

        [SetUp]
        public override void CreateObdInterface()
        {
            Console.WriteLine("---------- BEGIN STARTUP ----------");
            base.CreateObdInterface();
            obd.Connect().Wait();
            Assert.AreEqual(ObdInterfaceState.Connected, obd.State);
            Console.WriteLine("---------- END STARTUP ----------");
        }

        [TearDown]
        public void CloseConnection()
        {
            obd.Dispose();
            Assert.AreEqual(ObdInterfaceState.Disposed, obd.State);
        }
        
        [Test]
        public void TestGetSupportedCommands()
        {
            obd.QuerySupportedCommands().Wait();
            Assert.Greater(obd.Features.SupportedCommands.Count, 0, "There should be at least one supported command");
        }

        [Test]
        public void TestMultipleInterfaces()
        {
            var responses = new List<string>();

            // start returning extra responses
            serial.Setup(s => s.PortName).Returns("mock");
            serial.Setup(s => s.Send(It.IsAny<string>())).Callback<string>(s =>
            {
                responses.Clear();
                responses.Add(base.GetCommandResponse(s));
                responses.Add(base.GetCommandResponse(s));
                responses.Add(base.GetCommandResponse(s));
                serial.Raise(x => x.DataReceived += null, EventArgs.Empty);
            });
            serial.Setup(s => s.Receive()).Returns(() => string.Join("\r", responses) + "\r>");

            var task = obd.ExecuteCommand(ObdCommands.EngineRpm);
            task.Wait();
            Assert.AreEqual(3, task.Result.Responses.Count);
        }
    }
}
