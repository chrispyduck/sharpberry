using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sharpberry.obd
{
    public enum ObdInterfaceState
    {
        NotConnected,
        Initializing,
        Connected,
        Error,
        Disposed
    }
}
