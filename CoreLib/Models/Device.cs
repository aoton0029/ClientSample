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
    public class Device : IDisposable, IDevice
    {
        public IConnection Connection { get; protected set; }

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

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Connection?.Dispose();
            }
        }

        public async Task SendCmdAsync(string command)
        {
            await Connection.WriteString(command, true);
        }

        public async Task<string> QueryAsync(string command, CancellationToken cancellationToken = default)
        {
            await Connection.WriteString(command, true, cancellationToken);
            string response = await Connection.ReadString(0, cancellationToken);
            return response;
        }
    }
}
