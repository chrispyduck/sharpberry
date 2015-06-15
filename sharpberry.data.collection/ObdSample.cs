using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace sharpberry.data.collection
{
    [ProtoContract]
    public struct ObdSample
    {
        public ObdSample(ushort mode, ushort pid, byte[] value)
        {
            this.Ticks = DateTime.Now.Ticks;
            this.Mode = mode;
            this.Pid = pid;
            this.Value = value;
            this.Checksum = ComputeChecksum(this.Ticks, this.Mode, this.Pid, this.Value);
        }


        [ProtoMember(0)] public long Ticks; //8
        [ProtoMember(1)] public ushort Mode; //2
        [ProtoMember(2)] public ushort Pid; //2
        [ProtoMember(3)] public byte[] Value; //1
        [ProtoMember(4)] public int Checksum; //4

        //total size: 17

        public object Serialize()
        {
            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, this);
                return ms.ToArray();
            }
        }

        public bool IsChecksumOk()
        {
            var cs = ComputeChecksum(this.Ticks, this.Mode, this.Pid, this.Value);
            return cs == this.Checksum;
        }

        private static int ComputeChecksum(long ticks, ushort mode, ushort pid, byte[] value)
        {
            unchecked
            {
                var checksum = (int) (ticks & uint.MaxValue);
                checksum += (int) (ticks >> 32);
                checksum += mode;
                checksum += pid;
                checksum += value.Length;
                int sum = 0;
                foreach (byte b in value)
                    sum += b;
                checksum += sum;
                return checksum;
            }
        }
    }
}
