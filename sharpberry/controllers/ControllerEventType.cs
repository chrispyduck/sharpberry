using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sharpberry.controllers
{
    public enum ControllerEventType
    {
        RequestEnterPowerSave,
        RequestExitPowerSave,
        ReceiveData,
        Initialize,
        Shutdown,
        Custom
    }
}
