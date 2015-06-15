using System;

namespace sharpberry.configuration
{
    public class DataCollection
    {
        public DataCollection()
        {
            this.CollectionInterval = TimeSpan.FromSeconds(1.5);
            this.ObdDiagnosticCollectionInterval = TimeSpan.FromSeconds(30);
            this.ObdTelemetryCounters = new[] {"EngineRpm", "RelativeAcceleratorPedalPosition", "ThrottlePosition", "VehicleSpeed"};
            this.ObdDiagnosticCounters = new[] {"CoolantTemp", "EngineOilTemp", "IntakeAirTemp"};
        }

        public TimeSpan CollectionInterval { get; set; }
        public string[] ObdTelemetryCounters { get; set; }
        public string[] ObdDiagnosticCounters { get; set; }
        public TimeSpan ObdDiagnosticCollectionInterval { get; set; }
    }
}
