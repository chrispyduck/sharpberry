using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sharpberry.controllers
{
    public enum StartupPriority
    {
        DevicePower = 1000,
        DeviceConfiguration = 500,
        DataCollection = 10,
        UserInterface = 0
    }
}
