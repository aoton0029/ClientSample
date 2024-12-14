using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CoreLib.Models
{
    public class Device : IDisposable
    {
        public IConnection Connection { get; protected set; }

        public const NumberStyles NumberStyle = NumberStyles.Float;

        protected int EventPollingIntervalMs { get; set; } = 250;

        protected int ExtendedPollingCommandTimeoutMs { get; set; } = 1500;

        public const int DefaultPollingTimeoutMs = 10000;

        public string InstrumentId { get; }

        public Device(IConnection connection, string deviceId)
        {
            Connection = connection;
            InstrumentId = deviceId;
        }

        public virtual Task Close()
        {
            Dispose();
            return Task.CompletedTask;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Connection?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected async Task SendCmd(string command)
        {
            await Connection.WriteString(command, true);
        }

        protected async Task<string> Query(string command, bool stripHeader = true, CancellationToken cancellationToken = default)
        {
            // Send command to the instrument:
            await Connection.WriteString(command, true, cancellationToken);

            // Read the response:
            string response = await Connection.ReadString(0, cancellationToken);

            return response;
        }


        protected async Task<double> QueryDouble(string command)
        {
            string doubleStr = await Query(command);
            return double.Parse(doubleStr, NumberStyle, CultureInfo.InvariantCulture);
        }

        protected async Task PollQuery(string command, string response, int timeoutMs = DefaultPollingTimeoutMs, CancellationToken cancellationToken = default)
        {
            DateTime deadline = DateTime.Now.AddMilliseconds(timeoutMs);

            while (!cancellationToken.IsCancellationRequested)
            {
                // Query the device state. We will not use the standard Query method because we don't want to spoil the log output
                // with the polling queries:
                await Connection.WriteString(command, true);

                // Wait for the response with extended timeout:
                string responseWithHeader = await Connection.ReadString(ExtendedPollingCommandTimeoutMs, cancellationToken);

                // Wait for a while and then try again:
                await Task.Delay(EventPollingIntervalMs, cancellationToken);
                if (timeoutMs > 0 && DateTime.Now > deadline)
                {
                    throw new TimeoutException($"Polling of '{command}' timed out.");
                }
            }
        }
    }
}
