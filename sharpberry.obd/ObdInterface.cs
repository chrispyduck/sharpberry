using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PostSharp.Patterns.Model;
using PostSharp.Patterns.Threading;
using log4net;
using sharpberry.obd.Commands;
using sharpberry.obd.Responses;
using Timer = System.Timers.Timer;

namespace sharpberry.obd
{
    //[ReaderWriterSynchronized]
    public class ObdInterface : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        internal static readonly Command[] InitializationCommands = new[]
            {
                ElmCommands.Reset,
                ElmCommands.DisableEcho,
                ElmCommands.DisableExtraCrLf,
                ElmCommands.EnableHeaders,
                ElmCommands.SetAdaptiveTiming2,
                ElmCommands.DisableSpaces
            };

        /// <summary>
        /// the maximum amount of time to wait for a command to be completed before giving up
        /// </summary>
        internal const int GlobalReadHoldTime = 1250;

        public ObdInterface(string portName, int baudRate)
            : this(new SerialPort
                {
                    PortName = portName,
                    BaudRate = baudRate
                })
        { }

        public ObdInterface(ISerialPort port)
        {
            this.port = port;
            this.port.DataReceived += this.DataReceived;
            this.port.Error += this.PortError;
            this.Features = new ObdFeatures();

            this.ReadHoldTime = GlobalReadHoldTime;
            this.IsReadTimerEnabled = true;
        }

        internal int ReadHoldTime { get; set; }
        internal bool IsReadTimerEnabled { get; set; }
        public string PortName { get { return this.port.PortName; } }
        public int BaudRate { get { return this.port.BaudRate; } }
        public ObdInterfaceState State { [Reader] get; private set; }
        public ObdFeatures Features { get; private set; }
        public event EventHandler<CommandCompletedEventArgs> CommandCompleted;

        [Child]
        private readonly ISerialPort port;
        [Child]
        internal readonly CommandQueue writeQueue = new CommandQueue();
        private Timer readTimer = null;

        #region Control
        [Writer]
        public async Task Connect()
        {
            Logger.InfoFormat("Opening serial port {0}@{1}", this.PortName, this.BaudRate);
            this.State = ObdInterfaceState.Initializing;

            // open port
            try
            {
                this.port.Open();
            }
            catch (Exception e)
            {
                this.State = ObdInterfaceState.Error;
                throw new ObdException("Unexpected error opening serial port", e);
            }

            // queue negotiation commands
            Logger.InfoFormat("Port {0} open. Establishing protocol format.", this.PortName);
            var initTasks = InitializationCommands
                .Select(cmd => this.ExecuteCommandInternal(cmd, QueueItemType.Initialization))
                .ToArray();
            await Task.WhenAll(initTasks);

            var failed = initTasks.Where(t => !t.IsCompleted || t.IsFaulted).ToArray();
            if (failed.Length > 0)
            {
                throw new ObdException("Initialization did not complete as expected");
            }

            if (initTasks.Any(t => t.Result.Responses.Any(r => r.Status == ResponseStatus.Indeterminate)))
                Logger.Info("OBD connection aborted prior to completion of initialization commands");
            else if (initTasks.All(t => t.Result.Responses.All(r => r.Status == ResponseStatus.Valid)))
            {
                Logger.Info("OBD connection established");
                this.State = ObdInterfaceState.Connected;
            }
        }

        public void Disconnect()
        {
            lock (this)
            {
                Logger.InfoFormat("Closing port {0}@{1}", this.PortName, this.BaudRate);
                this.State = ObdInterfaceState.NotConnected;
                while (this.writeQueue.Count > 0)
                    this.writeQueue.Dequeue().Abort();
                this.writeQueue.Clear();
                if (this.readTimer != null)
                {
                    this.readTimer.Dispose();
                    this.readTimer = null;
                }
                this.port.Close();
                Logger.Info("Port closed");
            }
        }

        protected void PortError(object sender, EventArgs args)
        {
            var error = args as SerialErrorReceivedEventArgs;
            Logger.ErrorFormat("Serial port fault: {0}", error == null ? "unknown" : error.EventType.ToString());
            this.State = ObdInterfaceState.Error;
            this.port.Dispose();
        }

        protected void SetErrorState(Command command, string response)
        {
            this.State = ObdInterfaceState.Error;
            while (this.writeQueue.Count > 0)
            {
                var item = this.writeQueue.Dequeue();
                item.Abort();
            }
            throw new ObdException(
                string.Format(
                    "Connection faulted due to unexpected response, '{0}'. Expecting {{{1}}}.",
                    response,
                    command.ExpectedResponse),
                command: command);
        }

        [Writer]
        public void Dispose()
        {
            this.State = ObdInterfaceState.Disposed;

            if (this.readTimer != null)
                this.readTimer.Dispose();

            while (this.writeQueue.Count > 0)
            {
                var item = this.writeQueue.Dequeue();
                item.Abort();
            }

            if (this.port != null)
            {
                this.port.Close();
                this.port.Dispose();
            }
        }
        #endregion

        #region Command Queueing
        protected async Task<CommandCompletedEventArgs> ExecuteCommandInternal(Command command, QueueItemType type)
        {
            if (command == null)
                throw new ArgumentNullException("command");
            var item = new QueuedCommand(command, type);
            this.writeQueue.Enqueue(item);
            Logger.DebugFormat("Enqueued command: {0}", item.Command);
            // this sequencing is because i'm neurotic
            var task = item.WaitHandle.AsTask();
            ProcessWriteQueue();
            await task;
            for (var i = 0; i < item.Responses.Count; i++)
            {
                Logger.DebugFormat("Queue item '{0}' received response[{1}] with status '{2}' in {3}ms", item.Command, i, item.Responses[i].Status, item.Time);
            }
            if (item.Type == QueueItemType.Initialization && item.Responses.Any(r => r.Status == ResponseStatus.Invalid))
                this.SetErrorState(item.Command, item.RawResponse);
            return new CommandCompletedEventArgs(item.Command, item.Responses);
        }

        [Writer]
        public async Task<CommandCompletedEventArgs> ExecuteCommand(Command command)
        {
            if (this.State != ObdInterfaceState.Connected)
                throw new InvalidOperationException("The OBD interface is not online; the current state is: " + this.State);
            return await this.ExecuteCommandInternal(command, QueueItemType.User);
        }

        protected void ProcessWriteQueue()
        {
            if (this.State == ObdInterfaceState.Disposed || this.State == ObdInterfaceState.NotConnected)
                return;
            if (this.writeQueue.Count == 0)
                return;

            var item = this.writeQueue.Peek();
            if (item.Sent)
                return;

            Logger.Debug("--> " + item.Command);
            this.port.Send(item.Command.GetCommandString() + "\r\n");
            item.MarkSent();
            RestartReadTimer();
        }
        #endregion

        #region Reading
        protected void DataReceived(object sender, EventArgs data)
        {
            this.ReceiveAndParse();
        }

        protected void RestartReadTimer()
        {
            RestartReadTimer(this.ReadHoldTime);
        }
        protected void RestartReadTimer(int delay)
        {
            if (!this.IsReadTimerEnabled)
                return;

            if (this.readTimer == null)
            {
                this.readTimer = new Timer();
                this.readTimer.Elapsed += this.ReadTimerElapsed;
            }
            else
                this.readTimer.Stop();

            this.readTimer.Interval = delay;
            this.readTimer.Start();
        }

        protected void ReadTimerElapsed(object sender, EventArgs e)
        {
            if (this.State == ObdInterfaceState.Disposed)
                return;
            this.readTimer.Stop();
            this.ReceiveAndParse(true);
        }

        protected void ReceiveAndParse(bool fromTimer = false)
        {
            lock (this)
            {
                var newlyReceivedData = this.port.Receive();

                var currentCommand = this.writeQueue.Count > 0 ? this.writeQueue.Peek() : null;
                if (currentCommand == null)
                {
                    //Logger.WarnFormat("Received data from OBD, but no data was expected. Data received: {0}", newlyReceivedData);
                    return;
                }

                currentCommand.RawResponse += newlyReceivedData;

                // 
                var data = currentCommand.UnmatchedResponse + newlyReceivedData;
                currentCommand.UnmatchedResponse = string.Empty;
                foreach (var part in ResponseParser.GetParts(data))
                {
                    if (string.IsNullOrWhiteSpace(part.Text))
                        continue;
                    if (part.Text == ">")
                    {
                        // received prompt; ELM is done returning data
                        if (this.readTimer != null)
                            this.readTimer.Stop();
                        this.TryFinishRead(true);
                        return;
                    }
                    if (!part.IsComplete)
                    {
                        currentCommand.UnmatchedResponse = part.Text;
                        continue;
                    }
                    if (this.Features.EchoEnabled && part.Text == currentCommand.Command.GetCommandString())
                        continue;

                    if (!fromTimer && part.Text.Equals("SEARCHING...", StringComparison.InvariantCultureIgnoreCase))
                    {
                        RestartReadTimer(5000);
                        continue;
                    }

                    currentCommand.TryAddResponse(part.Text, this.Features);
                }
            }

            if (fromTimer)
                this.TryFinishRead(true);
        }

        protected void TryFinishRead(bool fromTimer = false)
        {
            lock (this)
            {
                // see if we've received everything we're expecting
                if (this.writeQueue.Count == 0)
                    return;
                var queueItem = this.writeQueue.Peek();

                if (queueItem.UnmatchedResponse != string.Empty && !fromTimer)
                {
                    // keep waiting
                    RestartReadTimer();
                    return;
                }

                foreach (var response in queueItem.Responses)
                {
                    Logger.Debug("<-- " + response);
                    if (response.Status == ResponseStatus.Incomplete)
                        Logger.WarnFormat("Received incomplete response to command '{0}': {1}", queueItem.Command, response);
                }

                this.writeQueue.Dequeue();
                queueItem.MarkComplete();
                this.ProcessWriteQueue();

                switch (queueItem.Type)
                {
                    case QueueItemType.User:
                        var x = this.CommandCompleted;
                        if (x != null)
                            x.Invoke(this, new CommandCompletedEventArgs(queueItem.Command, queueItem.Responses));
                        break;

                    case QueueItemType.Initialization:
                        if (queueItem.Responses.Count == 0)
                            this.SetErrorState(queueItem.Command, string.Empty);
                        else
                            this.SetFeature(queueItem.Command, queueItem.Responses[0]);
                        break;
                }

            }
        }
        #endregion

        #region Interface Features
        [Writer]
        void SetFeature(Command command, ParsedResponse response)
        {
            if (response.Status != ResponseStatus.Valid)
                return;

            if (command == ElmCommands.DisableEcho)
                this.Features.EchoEnabled = false;
            else if (command == ElmCommands.EnableHeaders)
                this.Features.HeadersEnabled = true;
            else if (command == ElmCommands.DisableHeaders)
                this.Features.HeadersEnabled = false;
            else if (command == ElmCommands.DisableExtraCrLf)
                this.Features.ExtraCrLf = false;
        }

        public async Task QuerySupportedCommands()
        {
            var todo = new[]
                {
                    // mode, pid, number of commands described
                    new[] { 0x01, 0x00, 19 },
                    new[] { 0x01, 0x20, 19 },
                    new[] { 0x01, 0x40, 19 },
                    new[] { 0x01, 0x60, 19 },
                    new[] { 0x05, 0x100, 19 },
                    new[] { 0x09, 0x00, 19 }
                };
            foreach (var task in todo)
            {
                // give these things friendly names
                var mode = task[0];
                var pid = task[1];
                var numberOfCommands = task[2];

                // execute the query command and interpret the response
                var command = ObdCommands.GetCommand(mode, pid);
                if (command == null)
                {
                    Logger.WarnFormat("Missing feature detection command for mode 0x{0:X}, pid 0x{1:X}", mode, pid);
                    continue;
                }
                var result = await this.ExecuteCommandInternal(command, QueueItemType.FeatureDetection);
                foreach (var response in result.Responses)
                {
                    if (response.Status != ResponseStatus.Valid)
                    {
                        Logger.WarnFormat("Feature detection failed while identifying supported commands in mode {0}, range 0x{1:X}-0x{2:X}", mode,
                                          pid + 1, pid + 20);
                        continue;
                    }
                    var intValue = Convert.ToInt32(response.Data, 16);
                    var bitVector = new BitVector32(intValue);

                    for (var i = 1; i <= numberOfCommands; i++)
                    {
                        var cmd = ObdCommands.GetCommand(mode, pid + i);
                        if (this.Features.SupportedCommands.Contains(cmd) && !bitVector[i - 1])
                            this.Features.SupportedCommands.Remove(cmd);
                        else if (!this.Features.SupportedCommands.Contains(cmd) && bitVector[i - 1])
                            this.Features.SupportedCommands.Add(cmd);
                    }
                }
            }
        }
        #endregion
    }
}
