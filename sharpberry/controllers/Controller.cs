using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sharpberry.controllers
{
    public abstract class Controller : IController
    {
        protected Controller(StartupPriority startupPriority)
        {
            this.startupPriority = startupPriority;
        }

        private StartupPriority startupPriority;
        public StartupPriority StartupPriority { get { return startupPriority; } }

        public void HandleEvent(object sender, ControllerEventArgs args)
        {
            switch (args.EventType)
            {
                case ControllerEventType.RequestEnterPowerSave:
                    this.OnEnterPowerSave();
                    break;

                case ControllerEventType.RequestExitPowerSave:
                    this.OnExitPowerSave();
                    break;

                case ControllerEventType.Initialize:
                    this.OnInitialize();
                    break;

                case ControllerEventType.Shutdown:
                    this.OnShutdown();
                    break;

                case ControllerEventType.ReceiveData:
                    this.OnReceiveData();
                    break;

                case ControllerEventType.Custom:
                    this.OnCustomEvent(args.Argument);
                    break;
            }
        }

        public event EventHandler<ControllerEventArgs> Event;

        protected abstract void OnEnterPowerSave();
        protected abstract void OnExitPowerSave();
        protected virtual void OnInitialize() { }
        protected virtual void OnShutdown() { }
        protected virtual void OnReceiveData() { }
        protected virtual void OnCustomEvent(object sender) { }

        protected void RaiseEvent(ControllerEventType type)
        {
            var x = Event;
            if (x != null)
                x.Invoke(this, new ControllerEventArgs(type));
        }
        protected void RaiseEvent(object data)
        {
            var x = Event;
            if (x != null)
                x.Invoke(this, new ControllerEventArgs(ControllerEventType.Custom, data));
        }
    }
}
