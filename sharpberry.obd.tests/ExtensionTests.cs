using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using sharpberry.obd;

namespace sharpberry.obd.tests
{
    [TestFixture]
    public class ExtensionTests
    {
        [Test]
        public void WaitHandlePositive()
        {
            var wh = new ManualResetEvent(false);
            var task = wh.AsTask(TimeSpan.FromSeconds(5));
            wh.Set();
            Thread.Sleep(500);
            Assert.IsTrue(task.IsCompleted, "Task should be completed");
            wh.Dispose();
        }

        [Test]
        public void WaitHandleNegative()
        {
            var wh = new ManualResetEvent(false);
            var task = wh.AsTask(TimeSpan.FromSeconds(1));
            Thread.Sleep(2000);
            Assert.IsTrue(task.IsCompleted, "Task should be completed");
            wh.Dispose();
        }

        [Test]
        public void ConvertFromHexString()
        {
            Assert.AreEqual(new byte[] { 0xE }, "e".GetBytesFromHexString());
            Assert.AreEqual(new byte[] { 0, 0x10 }, "010".GetBytesFromHexString());
            Assert.AreEqual(new byte[] { 1, 2 }, "0102".GetBytesFromHexString());
            Assert.AreEqual(new byte[0], "".GetBytesFromHexString());
            Assert.AreEqual(new byte[0], ((string)null).GetBytesFromHexString());
        }

        [Test]
        public void ConvertToHexString()
        {
            Assert.AreEqual("0101", new byte[] { 1, 1 }.ToHexString());
            Assert.AreEqual(null, null);
            Assert.AreEqual("FEAD", new byte[] { 0xfe, 0xad }.ToHexString());
            Assert.AreEqual("", new byte[] { }.ToHexString());
        }
    }
}
