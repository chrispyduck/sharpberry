using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using sharpberry.obd.Commands;

namespace sharpberry.obd
{
    [Serializable]
    public class ObdException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public ObdException(string message, Exception innerException = null, Command command = null)
            : base(message, innerException)
        {
            this.Command = command;
        }

        public Command Command { get; private set; }

        protected ObdException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
            this.Command = (Command)info.GetValue("Command", typeof (Command));
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            info.AddValue("Command", this.Command);
        }
    }
}
