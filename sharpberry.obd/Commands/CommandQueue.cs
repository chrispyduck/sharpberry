using System.Collections.Generic;
using PostSharp.Patterns.Threading;

namespace sharpberry.obd.Commands
{
    [PrivateThreadAware]
    internal class CommandQueue : Queue<QueuedCommand>
    {
    }
}
