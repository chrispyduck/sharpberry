using System;
using DBus;

namespace sharpberry.dbus
{
    [Interface("org.bluez.Adapter1")]
    public interface IBluezAdapter1
    {
        void StartDiscovery();
        void StopDiscovery();
        void RemoveDevice(object device);

        string Address { get; }
        string Name { get; }
        string Alias { get; }
        UInt32 Class { get; }
        bool Powered { get; set; }
        bool Discoverable { get; set; }
        bool Pairable { get; set; }
        UInt32 PairableTimeout { get; set; }
        UInt32 DiscoverableTimeout { get; set; }
        bool Discovering { get; }
        string[] UUIDs { get; }
    }
}
