using System;
using System.Collections.Generic;
using DBus;

namespace sharpberry.dbus
{
    [Interface("org.freedesktop.DBus.ObjectManager")]
    public interface IObjectManager
    {
        [return: Argument("objpath_interfaces_and_properties")]
        Dictionary<ObjectPath, Dictionary<string, object>> GetManagedObjects();

        event Action<ObjectPath, Dictionary<string, object>> InterfacesAdded;
        event Action<ObjectPath, String[]> InterfacesRemoved;

    }
}
