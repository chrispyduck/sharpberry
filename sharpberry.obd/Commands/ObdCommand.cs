using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using SimpleExpressionEvaluator;
using sharpberry.obd.Responses;

namespace sharpberry.obd.Commands
{
    [Serializable]
    public class ObdCommand : Command, IEquatable<ObdCommand>
    {
        private static readonly SimpleExpressionEvaluator.ExpressionEvaluator Evaluator = new ExpressionEvaluator();

        public ObdCommand(string name, int mode, int pid, int expectedBytes, int? minValue, int? maxValue, string units, string formula)
            : this(name, mode, pid, expectedBytes)
        {
            this.MinValue = minValue;
            this.MaxValue = maxValue;
            this.Units = units;
            this.Formula = formula;
        }

        public ObdCommand(string name, int mode, int pid, int expectedBytes)
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
        private Func<object, decimal> compiledFormula;

        public override string GetCommandString()
        {
            if (commandString == null)
            {
                var cmd = new byte[2];
                cmd[0] = (byte)this.Mode;
                cmd[1] = (byte)this.Pid;
                this.commandString = cmd.ToHexString();
            }

            return commandString;
        }

        public bool Equals(ObdCommand other)
        {
            return other.Mode == this.Mode
                   && other.Pid == this.Pid;
        }

        public decimal Evaluate(byte[] data)
        {
            if (this.compiledFormula == null)
            {
                if (string.IsNullOrWhiteSpace(this.Formula))
                    throw new NotSupportedException("No formula provided");
                try
                {
                    this.compiledFormula = Evaluator.Compile(this.Formula);
                }
                catch (Exception e)
                {
                    throw new NotSupportedException("Unable to evaluate string expression '" + this.Formula + "': " + e.Message);
                }
            }

            object args;
            if (data.Length == 1)
                args = new { A = data[0] };
            else if (data.Length == 2)
                args = new { A = data[0], B = data[1] };
            else if (data.Length == 3)
                args = new { A = data[0], B = data[1], C = data[2] };
            else if (data.Length == 4)
                args = new { A = data[0], B = data[1], C = data[2], D = data[3] };
            else if (data.Length == 5)
                args = new { A = data[0], B = data[1], C = data[2], D = data[3], E = data[4] };
            else
                throw new NotSupportedException("Too many or too few arguments");

            return this.compiledFormula(args);
        }
    }
}
