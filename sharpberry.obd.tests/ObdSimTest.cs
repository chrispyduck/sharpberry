﻿using System;
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
    [TestFixture]
    public class ObdSimTest
    {
        /// <summary>
        /// Not decorated with SetUpAttribute because this is a one-time thing
        /// </summary>
        public ObdSimTest()
        {
            ObdCommands.LoadDefaults();

            obd = new ObdInterface("COM5", 115200);
            obd.Connect().Wait();
        }

        private ObdInterface obd;

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
            Assert.AreEqual(ResponseStatus.Valid, task.Result.Status);

            var rpm = cmd.Evaluate(task.Result.Response.DataBytes);
            Trace.TraceInformation("RPM: {0}", rpm);
            Assert.Greater(rpm, 0);
        }
    }
}
