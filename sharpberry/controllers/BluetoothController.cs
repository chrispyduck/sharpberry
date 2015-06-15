using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DBus;
using sharpberry.dbus;

namespace sharpberry.controllers
{
    public class BluetoothController : Controller 
    {
        public BluetoothController()
            : base(StartupPriority.DeviceConfiguration)
        { }

        private const string AdapterInterface = "org.bluez.Adapter1";
        private const string DeviceInterface = "org.bluez.Device1";
        private Thread dbus;
        private dbus.IObjectManager objectManager;
        private readonly Dictionary<ObjectPath, dbus.IBluezAdapter1> adapters = new Dictionary<ObjectPath, IBluezAdapter1>();
        private readonly Dictionary<ObjectPath, dbus.IBluezDevice1> devices = new Dictionary<ObjectPath, IBluezDevice1>();

        protected override void OnInitialize()
        {
            dbus = new Thread(() =>
            {
                try
                {
                    while (true)
                        DBus.Bus.System.Iterate();
                }
                catch (ThreadAbortException)
                { }
            });
            dbus.Start();

            this.objectManager = DBus.Bus.System.GetObject<dbus.IObjectManager>("org.bluez", new ObjectPath("/"));
            this.objectManager.InterfacesAdded += AddInterfaces;
            this.objectManager.InterfacesRemoved += RemoveInterfaces;
            this.FindDevices();
            this.ConnectDevices();
        }

        protected override void OnShutdown()
        {
            this.DisconnectDevices();
            this.objectManager = null;
            this.adapters.Clear();
            this.devices.Clear();
            this.dbus.Abort();
        }

        void FindDevices()
        {
            var managedObjects = objectManager.GetManagedObjects();
            foreach (var obj in managedObjects)
                this.AddInterfaces(obj.Key, obj.Value);
        }

        void AddInterfaces(ObjectPath path, Dictionary<string, object> data)
        {
            if (data.ContainsKey(AdapterInterface))
            {
                var adapter = DBus.Bus.System.GetObject<dbus.IBluezAdapter1>("org.bluez", path);
                adapter.StartDiscovery();
                this.adapters.Add(path, adapter);
            }
            else if (data.ContainsKey(DeviceInterface))
            {
                this.devices.Add(path, DBus.Bus.System.GetObject<dbus.IBluezDevice1>("org.bluez", path));
            }
        }

        void RemoveInterfaces(ObjectPath path, string[] interfaces)
        {
            if (interfaces.Contains(AdapterInterface))
                this.adapters.Remove(path);
            else if (interfaces.Contains(DeviceInterface))
                this.devices.Remove(path);
        }

        public void ConnectDevices()
        {
            foreach (var device in this.devices.Values)
                if (!device.Connected)
                    device.Connect(); // this throws
        }

        public void DisconnectDevices()
        {
            foreach (var device in this.devices.Values)
                if (device.Connected)
                    device.Disconnect();
        }

        protected override void OnEnterPowerSave()
        {
            this.OnShutdown();
        }

        protected override void OnExitPowerSave()
        {
            this.OnInitialize();
        }
    }
}
