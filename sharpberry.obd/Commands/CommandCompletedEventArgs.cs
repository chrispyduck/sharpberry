using System;
using sharpberry.obd.Responses;

namespace sharpberry.obd.Commands
{
    public class CommandCompletedEventArgs : EventArgs
    {
        public CommandCompletedEventArgs(Command command, string response, ResponseStatus status)
        {
            this.Command = command;
            this.Response = response;
            this.Status = status;
        }

        public Command Command { get; private set; }
        public string Response { get; private set; }
        public ResponseStatus Status { get; private set; }
    }
}
