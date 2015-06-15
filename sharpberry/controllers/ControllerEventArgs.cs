using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sharpberry.controllers
{
    public class ControllerEventArgs : EventArgs
    {
        public ControllerEventArgs(ControllerEventType type, object argument = null)
        {
            this.EventType = type;
            this.Argument = argument;
        }

        public ControllerEventType EventType { get; private set; }
        public object Argument { get; private set; }
    }
}
