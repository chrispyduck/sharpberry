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
    public class ObdInterfaceInitializationTests : ObdInterfaceTestFixture
    {
        [Test]
        public void TestConnectUnexpectedResponse()
        {
            // ATZ shouldn't return OK, so this should fault
            serial.Setup(s => s.Receive()).Returns("OK\r\n>");

            // quick test of the mock
            Assert.AreEqual("OK\r\n>", serial.Object.Receive());

            var task = obd.Connect();
            Assert.Throws<AggregateException>(task.Wait);

            Assert.AreEqual(true, task.IsFaulted);
            var obdEx = (ObdException)task.Exception.InnerExceptions[0];
            Assert.AreEqual(ElmCommands.Reset, obdEx.Command);
        }

        [Test]
        public void TestConnect()
        {
            obd.Connect().Wait();
            Assert.AreEqual(ObdInterfaceState.Connected, obd.State);

            obd.Dispose();
            Assert.AreEqual(ObdInterfaceState.Disposed, obd.State);
        }

        private ObdInterface SetupQueueAbortTestCase()
        {
            serial = Setup(750, 1250);
            obd = new ObdInterface(serial.Object);
            obd.IsReadTimerEnabled = false; // prevent undesired port polling to make this test more predictable
            return obd;
        }

        [Test]
        public void DisconnectWithPendingInitCommands()
        {
            SetupQueueAbortTestCase();
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
            SetupQueueAbortTestCase();
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
    }
}
