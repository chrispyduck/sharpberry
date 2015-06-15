using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using sharpberry.obd.Commands;
using sharpberry.obd.Responses;

namespace sharpberry.obd.tests
{
    public abstract class ObdInterfaceTestFixture
    {
        [SetUp]
        public virtual void CreateObdInterface()
        {
            serial = Setup();
            obd = new ObdInterface(serial.Object);
        }

        protected Mock<ISerialPort> serial;
        protected ObdInterface obd;
        protected static readonly Random rnd = new Random();

        protected string GetCommandResponse(string command)
        {
            command = command.Trim('\r', '\n');
            if (command == "ATZ")
                return "ELM mockup";
            if (command.StartsWith("AT"))
                return "OK";

            if (command.Length == 4)
            {
                // assume it's a 4 digit hex command and parse into mode and pid
                var mode = Convert.ToInt32(command.Substring(0, 2), 16);
                var pid = Convert.ToInt32(command.Substring(2, 2), 16);
                var cmd = ObdCommands.GetCommand(mode, pid);
                if (cmd != null)
                {
                    // figure out how many bytes are expected, and return that many random bytes
                    var expectedByteCount = ((ByteCountExpectedResponse)cmd.ExpectedResponse).NumberOfBytes;
                    var bytes = new byte[expectedByteCount + 2];
                    rnd.NextBytes(bytes);
                    bytes[0] = (byte)(mode + 0x40);
                    bytes[1] = (byte)pid;

                    // compile message with headers and checksum, if needed
                    var message = new StringBuilder();
                    if (obd.Features.HeadersEnabled)
                        message.Append("43210");
                    message.Append(bytes.ToHexString());
                    if (obd.Features.HeadersEnabled)
                        unchecked
                        {
                            var checksum = bytes.Sum(b => (int)b);
                            message.Append(new byte[] { (byte)checksum }.ToHexString());
                        }
                    return message.ToString();
                }
            }

            return "ERROR";
        }

        protected Mock<ISerialPort> Setup(int minDelay = 10, int maxDelay = 100)
        {
            var serial = new Mock<ISerialPort>();
            string cmd = null;
            serial.Setup(s => s.PortName).Returns("mock");
            serial.Setup(s => s.Receive())
                .Returns(() => Task
                    .Delay(rnd.Next(minDelay, maxDelay))
                    .ContinueWith(_ =>
                    {
                        Console.WriteLine("serial: <-- {0}\\r>", cmd.Replace("\r", "\\r").Replace("\n", "\\n"));
                        return cmd + "\r>";
                    })
                    .Result);
            serial.Setup(s => s
                .Send(It.IsAny<string>()))
                .Callback<string>(s => Task
                    .Delay(rnd.Next(minDelay, maxDelay))
                    .ContinueWith(_ =>
                    {
                        cmd = GetCommandResponse(s);
                        Console.WriteLine("serial: --> {0}", s.Replace("\r", "\\r").Replace("\n", "\\n"));
                    })
                    .ContinueWith(_ => serial.Raise(sx => sx.DataReceived += null, EventArgs.Empty)));
            return serial;
        }
    }
}
