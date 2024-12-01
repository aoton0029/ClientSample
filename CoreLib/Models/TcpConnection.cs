using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CoreLib.Models
{
    public class TcpConnection : IConnection
    {
        private TcpClient _Client;
        public int Timeout { get; set; }
        public string DevicePath { get; }
        public string Host { get; }
        public int Port { get; }
        public const int DefaultTimeout = 500;
        protected ILogger<TcpConnection> Logger { get; }
        public bool IsOpen => _Client?.Connected == true;

        public TcpConnection(string host, int port, int timeout = DefaultTimeout, ILogger<TcpConnection> logger = null)
        {
            Host = host;
            Port = port;
            Timeout = timeout;
            Logger = logger;
            DevicePath = $"tcp://{host}:{port}";
        }

        public async Task Open(CancellationToken cancellationToken = default)
        {
            // Create a new TCP client instance:
            _Client?.Dispose();
            _Client = new TcpClient
            {
                // This forces the socket to be immediately closed when Close method is called.
                LingerState = new LingerOption(true, 0)
            };

            // Start asynchronous connection and connection timeout task:
            Logger?.LogInformation($"Creating TCP connection to {Host}:{Port}...", Host, Port);
            Task timeoutTask = Task.Delay(Timeout, cancellationToken);
            Task connTask = _Client.ConnectAsync(Host, Port);

            // Wait for either successful connection or timeout:
            await Task.WhenAny(connTask, timeoutTask);

            // Check for cancelling:
            if (timeoutTask.IsCanceled)
            {
                Logger?.LogWarning("Connection to the remote device has been cancelled.");
                _Client.Dispose();
                _Client = null;
                throw new OperationCanceledException();
            }

            // Check for timeout:
            if (timeoutTask.IsCompleted)
            {
                Logger?.LogError($"Connection to the remote device {Host}:{Port} timed out.");
                _Client.Dispose();
                _Client = null;
                throw new TimeoutException($"Connection to the remote device {Host}:{Port} timed out.");
            }

            Logger?.LogInformation("Connection succeeded.");
        }


        public void Close()
        {
            Logger?.LogInformation("Closing the TCP connection.");
            _Client?.Dispose();
            _Client = null;
        }


        public async Task<ReadResult> Read(byte[] buffer, int readLength = -1, int specialTimeout = 0, CancellationToken cancellationToken = default)
        {
            if (!IsOpen)
            {
                throw new InvalidOperationException("Cannot read data, the connection is not open.");
            }

            // Check/fix the reading length:
            if (readLength < 0)
            {
                readLength = buffer.Length;
            }
            else if (readLength > buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(readLength), "Read length cannot be greater than the buffer size.");
            }

            // Start reading operation:
            NetworkStream stream = _Client.GetStream();
            Task<int> readTask = stream.ReadAsync(buffer, 0, readLength, cancellationToken);

            // Wait until the read task is finished or timeout is reached:
            int readTimeout = specialTimeout > 0 ? specialTimeout : Timeout;
            if (await Task.WhenAny(readTask, Task.Delay(readTimeout, cancellationToken)) != readTask)
            {
                throw new TimeoutException("Reading from the device timed out.");
            }

            // The TCP protocol does not have EOF flag like the USB TMC protocol, but all SCPI messages (including curve data)
            // end with the new line character which can be used as the EOF flag. Correct EOF detection is very important because
            // some oscilloscopes (MDO3024) sometimes fragment the response into multiple packets and the above reading returns
            // only the first packet content.
            bool eof = readTask.Result <= 0 || buffer[readTask.Result - 1] == 0x0a;
            return new ReadResult(readTask.Result, eof, buffer);
        }

        public async Task Write(byte[] data, CancellationToken cancellationToken = default)
        {
            if (!IsOpen)
            {
                throw new InvalidOperationException("Cannot write data, the connection is not open.");
            }

            // Start write operation:
            Task writeTask = _Client.GetStream().WriteAsync(data, 0, data.Length, cancellationToken);

            // Wait until the read task is finished or timeout is reached:
            if (await Task.WhenAny(writeTask, Task.Delay(Timeout, cancellationToken)) != writeTask)
            {
                throw new TimeoutException("Write to the device timed out.");
            }
        }

        public async Task ClearBuffers(CancellationToken cancellationToken = default)
        {
            byte[] buffer = new byte[4096];

            if (!IsOpen)
            {
                throw new InvalidOperationException("Cannot clear buffers, the connection is not open.");
            }

            Logger?.LogDebug("Clearing input buffer.");
            NetworkStream stream = _Client.GetStream();
            while (stream.DataAvailable)
            {
                await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Close();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
