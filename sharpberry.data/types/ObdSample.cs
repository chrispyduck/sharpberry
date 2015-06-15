using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using ProtoBuf;

namespace sharpberry.data.types
{
    public struct ObdSample
    {
        public ObdSample(ushort mode, ushort pid, byte[] value)
        {
            this.Ticks = DateTime.Now.Ticks;
            this.Mode = mode;
            this.Pid = pid;
            this.Value = value;
            //this.Checksum = ComputeChecksum(this.Ticks, this.Mode, this.Pid, this.Value);
        }

        public long Ticks; //8
        public ushort Mode; //2
        public ushort Pid; //2
        public byte[] Value; //1
        //public byte Checksum; //4

        //total size: 17
        
        /*public bool IsChecksumOk()
        {
            var cs = ComputeChecksum(this.Ticks, this.Mode, this.Pid, this.Value);
            return cs == this.Checksum;
        }

        private static byte ComputeChecksum(long ticks, ushort mode, ushort pid, byte[] value)
        {
            unchecked
            {
                byte checksum = 0;
                foreach (var b in BitConverter.GetBytes(ticks)
                                              .Union(BitConverter.GetBytes(mode))
                                              .Union(BitConverter.GetBytes(pid))
                                              .Union(value))
                    checksum += b;
                return checksum;
            }
        }*/
    }
}
