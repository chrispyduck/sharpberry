using System;
using System.Threading;
using sharpberry.obd.Responses;

namespace sharpberry.obd.Commands
{
    internal class QueuedCommand : IDisposable 
    {
        public QueuedCommand(Command command, QueueItemType type)
        {
            this.command = command;
            this.Type = type;
            this.Sent = false;
            this.ResponseValidity = ResponseStatus.Incomplete;
        }

        public Command Command { get { return this.command; } }
        public QueueItemType Type { get; private set; }
        public bool Sent { get; private set; }
        public string Response { get; private set; }
        public ResponseStatus ResponseValidity { get; private set; }
        public WaitHandle WaitHandle { get { return this.waitHandle; } }

        private readonly Command command;
        private readonly ManualResetEvent waitHandle = new ManualResetEvent(false);

        public void MarkSent()
        {
            this.Sent = true;
        }

        public void CheckResponseValidity()
        {
            this.ResponseValidity = this.Command.ExpectedResponse.CheckInput(this.Response);
        }

        public void SetIntermediateResponse(string response)
        {
            this.Response = response;
        }

        public void SetFinalResponse()
        {
            this.waitHandle.Set();
        }

        public void Abort()
        {
            // don't do anything if we've already signaled the end of this command
            if (this.waitHandle.WaitOne(0, false))
                return;

            // invalidate the response and mark the command complete
            this.ResponseValidity = ResponseStatus.Indeterminate;
            this.Response = null;
            this.waitHandle.Set();
        }

        public void Dispose()
        {
            this.waitHandle.Dispose();
        }
    }
}
