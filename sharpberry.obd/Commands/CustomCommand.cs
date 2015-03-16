using System;
using sharpberry.obd.Responses;

namespace sharpberry.obd.Commands
{
    [Serializable]
    public class CustomCommand : Command
    {
        public CustomCommand(string command, ExpectedResponse expectedResponse)
            : base("Custom command", expectedResponse)
        {
            this.Command = command;
        }

        public CustomCommand(string name, string command, ExpectedResponse expectedResponse)
            : base(name, expectedResponse)
        {
            this.Command = command;
        }

        public string Command { get; private set; }

        public override string GetCommandString()
        {
            return this.Command;
        }
    }
}
