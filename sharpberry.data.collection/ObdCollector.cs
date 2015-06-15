using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sharpberry.data;
using sharpberry.obd;
using sharpberry.obd.Commands;

namespace sharpberry.data.collection
{
    public class ObdCollector : DataCollector<ObdSample>
    {
        public ObdCollector(ITemporaryDataStore<ObdSample> temporaryStorage)
            : base(temporaryStorage)
        { }

        public override void Initialize()
        {
            blackBoxCommands = new ObdCommand[]
                {
                    ObdCommands.EngineRpm,
                    ObdCommands.RelativeAcceleratorPedalPosition,
                    ObdCommands.ThrottlePosition,
                    ObdCommands.VehicleSpeed
                };
            diagCommands = new ObdCommand[]
                {
                    ObdCommands.CoolantTemp,
                    ObdCommands.EngineOilTemp,
                    ObdCommands.IntakeAirTemp
                };
            if (obd == null)
                obd = new ObdInterface("...", 115200);
        }

        private ObdCommand[] blackBoxCommands;
        private ObdCommand[] diagCommands;
        private ObdInterface obd;

        private int DiagCollectionMinInterval = 10;

        private DateTime lastDiagCollection = DateTime.MinValue;
        protected override IEnumerable<ObdSample> GetSamples()
        {
            var commands = new List<ObdCommand>(blackBoxCommands.Length + diagCommands.Length);
            commands.AddRange(blackBoxCommands);

            if (DateTime.Now.Subtract(lastDiagCollection).TotalSeconds > DiagCollectionMinInterval)
            {
                commands.AddRange(diagCommands);
                lastDiagCollection = DateTime.Now;
            }

            // enqueue all commands and wait for all results
            var tasks = commands.Select(c => obd.ExecuteCommand(c)).ToArray();
            Task.WaitAll(tasks);
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
