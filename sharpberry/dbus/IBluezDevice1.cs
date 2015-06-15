using System;
using System.Collections.Generic;

namespace sharpberry.dbus
{
    public interface IBluezDevice1
    {
        void Connect();
        void Disconnect();
        void ConnectProfile(string uuid);
        void Pair();
        void CancelPairing();

        string Address { get; }
        string Name { get; }
        string Icon { get; }
        UInt32 Class { get; }
        UInt16 Appearance { get; }
        string[] UUIDs { get; }
        bool Paired { get; }
        bool Connected { get; }
        bool Trusted { get; set; }
        bool Blocked { get; set; }
        string Alias { get; set; }
        object Adapter { get; }
        bool LegacyPairing { get; }
        string Modalias { get; }
        Int16 RSSI { get; }
        Int16 TxPower { get; }
        Dictionary<string, byte[]> ManufacturerData { get; }
        Dictionary<string, byte[]> ServiceData { get; } 
    }
}
