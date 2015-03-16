using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using sharpberry.obd.Commands;
using sharpberry.obd.Responses;

namespace sharpberry.obd.tests
{
    [TestFixture]
    public class ObdCommandTests
    {
        [Test]
        public void StandardCommand()
        {
            var sc = new StandardCommand("test", 1, 15, 4);
            Assert.AreEqual("test", sc.Name, "Name");
            Assert.AreEqual(1, sc.Mode, "mode");
            Assert.AreEqual(15, sc.Pid, "pid");
            Assert.IsNotNull(sc.ExpectedResponse as ByteCountExpectedResponse);
            Assert.AreEqual(4, ((ByteCountExpectedResponse)sc.ExpectedResponse).NumberOfBytes);
            Assert.AreEqual("010F", sc.GetCommandString());
            Assert.AreEqual("test (010F)", sc.ToString());
        }

        [Test]
        public void CustomCommand()
        {
            var cc = new CustomCommand("custom", "BLARG!", ExpectedResponse.Any);
            Assert.AreEqual("custom", cc.Name, "Name");
            Assert.AreEqual(ExpectedResponse.Any, cc.ExpectedResponse);
            Assert.AreEqual("BLARG!", cc.GetCommandString());
            Assert.AreEqual("custom (BLARG!)", cc.ToString());
        }

        [Test]
        public void LoadStadardCommands()
        {
            Assert.AreEqual(ObdCommands.All.Count, 0);
            Assert.IsNull(ObdCommands.GetCommand(1, 0));
            Assert.IsNull(ObdCommands.GetCommand(1, 1));
            Assert.IsNull(ObdCommands.GetCommand(1, 2));
            ObdCommands.LoadDefaults();
            Assert.Greater(ObdCommands.All.Count, 50);
            Assert.IsNotNull(ObdCommands.GetCommand(1, 0));
            Assert.AreEqual(4, ((ByteCountExpectedResponse)ObdCommands.GetCommand(1, 0).ExpectedResponse).NumberOfBytes);
            Assert.IsNotNull(ObdCommands.GetCommand(1, 1));
            Assert.IsNotNull(ObdCommands.GetCommand(1, 2));
        }
    }
}
