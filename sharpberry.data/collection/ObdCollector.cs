using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sharpberry.data;
using sharpberry.data.storage;
using sharpberry.data.types;
using sharpberry.obd;
using sharpberry.obd.Commands;
using System.Diagnostics;
using log4net;

namespace sharpberry.data.collection
{
    public class ObdCollector : DataCollector<ObdSample>
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(MongoDataProvider));

        public ObdCollector(ObdInterface obd)
        {
            this.Obd = obd;
        }

        public override void Initialize()
        {
            blackBoxCommands = new ObdCommand[]
                {
                    ObdCommands.EngineRpm,
                    ObdCommands.RelativeAcceleratorPedalPosition,
                    ObdCommands.ThrottlePosition,
                    ObdCommands.VehicleSpeed,
                    ObdCommands.MassAirFlow
                };
            diagCommands = new ObdCommand[]
                {
                    ObdCommands.CoolantTemp,
                    ObdCommands.EngineOilTemp,
                    ObdCommands.IntakeAirTemp
                };
        }

        private ObdCommand[] blackBoxCommands;
        private ObdCommand[] diagCommands;
        
        // set by DI
        public ObdInterface Obd { get; private set; }

        private int DiagCollectionMinInterval = 15;

        private DateTime lastDiagCollection = DateTime.MinValue;

        internal override void AddSampleToTrip(Trip trip, ObdSample sample, IDataProvider dataProvider)
        {
            dataProvider.WriteObdSample(trip, sample);
        }

        internal override IEnumerable<ObdSample> GetSamples()
        {
            var sw = new Stopwatch();
            sw.Start();

            var commands = new List<ObdCommand>(blackBoxCommands.Length + diagCommands.Length);
            commands.AddRange(blackBoxCommands);

            if (DateTime.Now.Subtract(lastDiagCollection).TotalSeconds > DiagCollectionMinInterval)
            {
                commands.AddRange(diagCommands);
                lastDiagCollection = DateTime.Now;
            }

            // enqueue all commands and wait for all results
            var tasks = commands.Select(c => this.Obd.ExecuteCommand(c)).ToArray();
            Task.WaitAll(tasks);

            sw.Stop();
            logger.Debug($"ObdCollector.GetSamples() took {sw.ElapsedMilliseconds}ms");

            return tasks
                .Where(t => t.Status == TaskStatus.RanToCompletion)
                .SelectMany(t =>
                    {
                        var cmd = (ObdCommand) t.Result.Command;
                        return t.Result.Responses.Select(r => new ObdSample(cmd.Mode, cmd.Pid, r.DataBytes));
                    });
        }
    }
}
