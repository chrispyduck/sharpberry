using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using sharpberry.obd.Commands;
using sharpberry.obd.Responses;

namespace sharpberry.obd.tests
{
    [TestFixture(Category="Integration")]
    public class ObdSimTest
    {
        /// <summary>
        /// Not decorated with SetUpAttribute because this is a one-time thing
        /// </summary>
        public ObdSimTest()
        {
            ObdCommands.LoadDefaults();

            obd = new ObdInterface("COM5", 38400);
            obd.Connect().Wait();
        }

        private ObdInterface obd;

        [Test]
        public void QueryDtcs()
        {
            var task = obd.GetDtcCount();
            task.Wait();
            var results = task.Result;
            Assert.AreEqual(1, results.Length);
            Assert.IsTrue(results[0].IsCheckEngineLightOn);
        }

        [Test]
        public void QuerySupportedCommands()
        {
            obd.QuerySupportedCommands().Wait();
        }

        [Test]
        public void QueryRpm()
        {
            var cmd = ObdCommands.GetCommand(0x01, 0x0C);
            var task = obd.ExecuteCommand(cmd);
            task.Wait();

            var response = task.Result.Responses.FirstOrDefault();
            Assert.NotNull(response);
            Assert.AreEqual(ResponseStatus.Valid, response.Status);

            var rpm = cmd.Evaluate(response.DataBytes);
            Trace.TraceInformation("RPM: {0}", rpm);
            Assert.Greater(rpm, 0);
        }
    }
}
