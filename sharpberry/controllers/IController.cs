using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sharpberry.controllers
{
    public interface IController
    {

        /// <summary>
        /// Higher values indicate higher priority. Controllers with higher priority values exit power save first and enter power save last.
        /// </summary>
        StartupPriority StartupPriority { get; }

        void HandleEvent(object sender, ControllerEventArgs args);
        event EventHandler<ControllerEventArgs> Event;
    }
}
