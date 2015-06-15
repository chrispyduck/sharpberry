using System;
using System.Collections.Generic;
using DBus;

namespace sharpberry.dbus
{
    public interface IProperties
    {
        [return: Argument("value")]
        object Get(string interface_name, string property_name);

        void Set(string interface_name, string property_name, object value);

        [return: Argument("props")]
        Dictionary<string, object> GetAll(string interface_name);

        event Action<string, Dictionary<string, object>, string[]> PropertiesChanged;
    }
}
