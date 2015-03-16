using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace sharpberry.obd.tests
{
    [SetUpFixture]
    public class Setup
    {
        [SetUp]
        public void ConfigureLogging()
        {
            var hierarchy = (Hierarchy) log4net.LogManager.GetRepository();
            hierarchy.Root.Level = Level.All;
            hierarchy.Root.AddAppender(new TraceAppender
            {
                Threshold = Level.All,
                Layout = new PatternLayout("%date{HH:mm:ss,fff} [%thread] %level: %message%newline")
            });
            hierarchy.Configured = true;

            var logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
            logger.Debug("Logging started");
            logger.Info("Logging Started");
        }
    }
}
