using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sharpberry.obd.Commands;

namespace sharpberry.obd
{
    public class ObdFeatures
    {
        public ObdFeatures()
        {
            this.ExtraCrLf = true;
            this.EchoEnabled = true;
            this.HeadersEnabled = true;
            this.SupportedCommands = new List<Command>();
        }

        public bool ExtraCrLf { get; set; }
        public bool EchoEnabled { get; set; }
        public bool HeadersEnabled { get; set; }

        public List<Command> SupportedCommands { get; private set; }
    }
}
