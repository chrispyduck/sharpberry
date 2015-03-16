using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using sharpberry.obd.Responses;

namespace sharpberry.obd.Commands
{
    [Serializable]
    public class StandardCommand : Command, IEquatable<StandardCommand>
    {
        public StandardCommand(string name, int mode, int pid, int expectedBytes, int? minValue, int? maxValue, string units, string formula)
            : this(name, mode, pid, expectedBytes)
        {
            this.MinValue = minValue;
            this.MaxValue = maxValue;
            this.Units = units;
            this.Formula = formula;
        }

        public StandardCommand(string name, int mode, int pid, int expectedBytes)
            : base(name, ExpectedResponse.ByteCount(expectedBytes))
        {
            this.Mode = mode;
            this.Pid = pid;
        }

        public int Mode { get; private set; }
        public int Pid { get; private set; }
        public int? MinValue { get; private set; }
        public int? MaxValue { get; private set; }
        public string Units { get; private set; }
        public string Formula { get; private set; }
        
        private string commandString = null;

        public override string GetCommandString()
        {
            if (commandString == null)
            {
                var cmd = new byte[2];
                cmd[0] = (byte) this.Mode;
                cmd[1] = (byte) this.Pid;
                this.commandString = cmd.ToHexString();
            }

            return commandString;
        }

        public bool Equals(StandardCommand other)
        {
            return other.Mode == this.Mode
                   && other.Pid == this.Pid;
        }
    }
}
