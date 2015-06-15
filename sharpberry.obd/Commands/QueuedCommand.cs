using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            this.RawResponse = string.Empty;
            this.UnmatchedResponse = string.Empty;
            this.Responses = new List<ParsedResponse>();
        }

        public Command Command { get { return this.command; } }
        public QueueItemType Type { get; private set; }
        public bool Sent { get; private set; }
        public string RawResponse { get; set; }
        public string UnmatchedResponse { get; set; }
        public List<ParsedResponse> Responses { get; private set; }
        public WaitHandle WaitHandle { get { return this.waitHandle; } }
        public decimal Time { get { return timer.ElapsedMilliseconds; } }

        private readonly Stopwatch timer = new Stopwatch();
        private readonly Command command;
        private readonly ManualResetEvent waitHandle = new ManualResetEvent(false);

        public void MarkSent()
        {
            this.Sent = true;
            this.timer.Start();
        }

        public void MarkComplete()
        {
            this.timer.Stop();
            this.waitHandle.Set();
        }

        /// <summary>
        /// Determines whether the given response is complete. If it is, it is added to the <see cref="Responses"/> collection. 
        /// Otherwise, the <paramref name="response"/> parameter value is assigned to <see cref="UnmatchedResponse"/>.
        /// </summary>
        /// <param name="response">The response received so far</param>
        /// <param name="features"></param>
        public void TryAddResponse(string response, ObdFeatures features)
        {
            var parsedResponse = ResponseParser.Parse(response, this.command, features);
            switch (parsedResponse.Status)
            {
                case ResponseStatus.Incomplete:
                case ResponseStatus.Indeterminate:
                    this.UnmatchedResponse = response;
                    break;
                    
                case ResponseStatus.Invalid:
                case ResponseStatus.NoData:
                case ResponseStatus.Valid:
                    this.UnmatchedResponse = string.Empty;
                    this.Responses.Add(parsedResponse);
                    break;
            }
        }

        public void Abort()
        {
            // don't do anything if we've already signaled the end of this command
            if (this.waitHandle.WaitOne(0, false))
                return;

            // invalidate the response and mark the command complete
            this.Responses.Add(new ParsedResponse
                {
                    Status = ResponseStatus.Invalid,
                    IsError = true
                });
            this.waitHandle.Set();
        }

        public void Dispose()
        {
            this.waitHandle.Dispose();
        }
    }
}
