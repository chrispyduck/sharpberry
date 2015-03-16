using System;
using sharpberry.obd.Responses;

namespace sharpberry.obd.Commands
{
    public static class ElmCommands
    {
        public static Command Reset = new CustomCommand("Reset ELM", "ATZ", ExpectedResponse.String("ELM", true));
        public static Command DisableEcho = new CustomCommand("Disable Echo", "ATE0", ExpectedResponse.Ok);
        public static Command DisableExtraCrLf = new CustomCommand("Disable Extra CrLf", "ATL0", ExpectedResponse.Ok);
        public static Command DisableSpaces = new CustomCommand("Disable Spaces", "ATS0", ExpectedResponse.Ok);
        public static Command DisableHeaders = new CustomCommand("Disable Headers", "ATH0", ExpectedResponse.Ok);
        public static Command EnableHeaders = new CustomCommand("Enable Headers", "ATH1", ExpectedResponse.Ok);
        public static Command SetAdaptiveTiming1 = new CustomCommand("Set Adaptive Timing 1", "ATAT1", ExpectedResponse.Ok);
        public static Command SetAdaptiveTiming2 = new CustomCommand("Set Adaptive Timing 2", "ATAT2", ExpectedResponse.Ok);
    }
}
