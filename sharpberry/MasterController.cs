using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using sharpberry.configuration;
using sharpberry.controllers;
using sharpberry.data;
using sharpberry.data.collection;

namespace sharpberry
{
    public class MasterController : ServiceBase 
    {
        public static int Main(string[] args)
        {
            var container = new UnityContainer();
            container.LoadConfiguration();

            var config = new Config();
            config.Load();
            container.RegisterInstance(config);

            var mc = new MasterController(container);
            ServiceBase.Run(mc);
            
            return 0;
        }

        public MasterController(UnityContainer container)
        {
            this.container = container;
            this.controllers = container.ResolveAll<IController>().ToArray();
            foreach (var controller in this.controllers)
                controller.Event += ControllerEventHandler;

            base.CanHandlePowerEvent = false; // we do our own power monitoring
            base.CanHandleSessionChangeEvent = false;
            base.CanPauseAndContinue = false;
            base.CanStop = true;
        }

        private readonly UnityContainer container;
        private readonly IController[] controllers;

        protected override void OnStart(string[] args)
        {
            foreach (var controller in this.controllers.OrderByDescending(c => c.StartupPriority))
                controller.HandleEvent(this, new ControllerEventArgs(ControllerEventType.Initialize));
        }

        protected override void OnStop()
        {
            foreach (var controller in this.controllers.OrderBy(c => c.StartupPriority))
                controller.HandleEvent(this, new ControllerEventArgs(ControllerEventType.Shutdown));
        }

        void ControllerEventHandler(object sender, ControllerEventArgs args)
        {
            controllers.Where(c => c != sender)
                       .AsParallel()
                       .ForAll(c => c.HandleEvent(sender, args));
        }
    }
}
