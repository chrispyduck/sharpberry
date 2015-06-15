using System;
using System.Collections.Generic;
using sharpberry.obd.Responses;

namespace sharpberry.obd.Commands
{
    public class CommandCompletedEventArgs : EventArgs
    {
        public CommandCompletedEventArgs(Command command, List<ParsedResponse> responses)
        {
            this.Command = command;
            this.Responses = responses;
        }

        public Command Command { get; private set; }
        public List<ParsedResponse> Responses { get; private set; }
    }
}
