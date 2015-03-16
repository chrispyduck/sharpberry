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
    public class ObdInterfaceTests
    {
        static string GetCommandResponse(string command)
        {
            command = command.Trim('\r', '\n');
            if (command == "ATZ")
                return "ELM mockup";
            if (command.StartsWith("AT"))
                return "OK";
            return "ERROR";
        }

        void PrepareMockCommandResponse(Mock<ISerialPort> mock, string command)
        {
            Thread.Sleep(10);
            response = GetCommandResponse(command);
            mock.Raise(s => s.DataReceived += null, EventArgs.Empty);
        }

        string ReturnResponse()
        {
            Thread.Sleep(10);
            var r = response;
            response = null;
            return r;
        }

        private string response;

        [Test]
        public void TestConnectUnexpectedResponse()
        {
            // ATZ shouldn't return OK, so this should fault
            var serial = new Mock<ISerialPort>();
            serial.Setup(s => s.PortName).Returns("mock");
            serial.Setup(s => s.Send(It.IsAny<string>())).Raises(s => s.DataReceived += null, EventArgs.Empty);
            serial.Setup(s => s.Receive()).Returns("OK\r\n>");

            // quick test of the mock
            Assert.AreEqual("OK\r\n>", serial.Object.Receive());

            var obd = new ObdInterface(serial.Object);
            var task = obd.Connect();
            Assert.Throws<AggregateException>(task.Wait);

            Assert.AreEqual(true, task.IsFaulted);
            var obdEx = (ObdException) task.Exception.InnerExceptions[0];
            Assert.AreEqual(ElmCommands.Reset, obdEx.Command);
        }

        [Test]
        public void TestConnect()
        {
            var serial = new Mock<ISerialPort>();
            serial.Setup(s => s.PortName).Returns("mock");
            serial.Setup(s => s.Send(It.IsAny<string>())).Callback<string>(s => this.PrepareMockCommandResponse(serial, s));
            serial.Setup(s => s.Receive()).Returns(this.ReturnResponse);

            var obd = new ObdInterface(serial.Object);
            obd.Connect().Wait();
            Assert.AreEqual(ObdInterfaceState.Connected, obd.State);

            obd.Dispose();
            Assert.AreEqual(ObdInterfaceState.Disposed, obd.State);
        }

        private void SetupDelayedWrite(Mock<ISerialPort> serial, int minDelay = 750, int maxDelay = 1250)
        {
            var rnd = new Random();
            serial.Setup(s => s
                .Send(It.IsAny<string>()))
                .Callback<string>(s => Task
                    .Delay(rnd.Next(minDelay, maxDelay))
                    .ContinueWith(_ => this.PrepareMockCommandResponse(serial, s)));
        }

        private void SetupDelayedRead(Mock<ISerialPort> serial, int minDelay = 750, int maxDelay = 1250)
        {
            var rnd = new Random();
            serial.Setup(s => s.Receive())
                .Returns(() => Task
                    .Delay(rnd.Next(minDelay, maxDelay))
                    .ContinueWith(_ => this.ReturnResponse())
                    .Result);
        }

        private ObdInterface SetupQueueAbortTestCase()
        {
            var serial = new Mock<ISerialPort>();
            serial.Setup(s => s.PortName).Returns("mock");
            SetupDelayedWrite(serial);
            SetupDelayedRead(serial);

            // start the connection and wait until it's underway
            var obd = new ObdInterface(serial.Object);
            obd.IsReadTimerEnabled = false; // prevent undesired port polling to make this test more predictable
            return obd;
        }
            
        [Test]
        public void DisconnectWithPendingInitCommands()
        {
            var obd = SetupQueueAbortTestCase();
            obd.Connect().Wait(10);

            // everything should be queued. validate and copy the queue.
            var queue = obd.writeQueue.ToArray();
            Assert.AreEqual(ObdInterface.InitializationCommands.Length, queue.Length);
            Assert.IsTrue(queue[0].Sent, "Command at top of queue should have been sent by now");
            Assert.IsFalse(queue.Skip(1).Any(c => c.Sent), "No other commands should have been sent");
            for (var i = 0; i < queue.Length; i++)
                Assert.IsFalse(queue[i].WaitHandle.WaitOne(0, false), "Wait handle on queue item idx #{0} should not have been set yet", i);

            obd.Disconnect();
            for (var i = 0; i < queue.Length; i++)
                Assert.IsTrue(queue[i].WaitHandle.WaitOne(0, false), "Wait handle on queue item idx #{0} should have been set", i);
            Assert.AreEqual(ObdInterfaceState.NotConnected, obd.State);
        }

        [Test]
        public void DisposeWithPendingInitCommands()
        {
            var obd = SetupQueueAbortTestCase();
            obd.Connect().Wait(10);

            // everything should be queued. validate and copy the queue.
            var queue = obd.writeQueue.ToArray();
            Assert.AreEqual(ObdInterface.InitializationCommands.Length, queue.Length);
            Assert.IsTrue(queue[0].Sent, "Command at top of queue should have been sent by now");
            Assert.IsFalse(queue.Skip(1).Any(c => c.Sent), "No other commands should have been sent");
            for (var i = 0; i < queue.Length; i++)
                Assert.IsFalse(queue[i].WaitHandle.WaitOne(0, false), "Wait handle on queue item idx #{0} should not have been set yet", i);

            obd.Dispose();
            for (var i = 0; i < queue.Length; i++)
                Assert.IsTrue(queue[i].WaitHandle.WaitOne(0, false), "Wait handle on queue item idx #{0} should have been set", i);
            Assert.AreEqual(ObdInterfaceState.Disposed, obd.State);
        }

        /*[Test]
        public void IssueCommand()
        {
            if (obd == null || obd.State != ObdInterfaceState.Connected)
            {
                Assert.Inconclusive("OBD interface is not connected");
                return;
            }

            var task = obd.ExecuteCommand(new CustomObdCommand("0100", ExpectedResponse.ByteCount(8)));
            task.Wait();
            Assert.AreEqual(ResponseStatus.Valid, task.Result.Status);
        }*/
    }
}
