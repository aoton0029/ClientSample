using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PowersupplyCtrl2
{
    public class TcpConnection : IConnection
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private readonly string _ipAddress;
        private readonly int _port;
        private bool _isConnected;

        public TcpConnection(string ipAddress, int port)
        {
            _ipAddress = ipAddress;
            _port = port;
            _client = new TcpClient();
        }

        public bool IsConnected => _isConnected;

        public event EventHandler<ConnectionEventArgs> ConnectionStatusChanged;

        public async Task<bool> ConnectAsync()
        {
            try
            {
                if (_isConnected)
                    return true;

                _client = new TcpClient();
                await _client.ConnectAsync(_ipAddress, _port);
                _stream = _client.GetStream();
                _isConnected = true;

                OnConnectionStatusChanged(true, "TCP接続が確立されました");
                return true;
            }
            catch (Exception ex)
            {
                OnConnectionStatusChanged(false, $"TCP接続に失敗しました: {ex.Message}");
                return false;
            }
        }

        public async Task DisconnectAsync()
        {
            if (!_isConnected)
                return;

            _stream?.Close();
            _client?.Close();
            _isConnected = false;

            OnConnectionStatusChanged(false, "TCP接続が切断されました");
            await Task.CompletedTask;
        }

        public async Task SendAsync(byte[] data)
        {
            if (!_isConnected || _stream == null)
                throw new InvalidOperationException("接続が確立されていません");

            await _stream.WriteAsync(data, 0, data.Length);
        }

        public async Task<byte[]> ReceiveAsync(int timeout = 1000)
        {
            if (!_isConnected || _stream == null)
                throw new InvalidOperationException("接続が確立されていません");

            using var cts = new System.Threading.CancellationTokenSource(timeout);
            var buffer = new byte[4096]; // バッファサイズは必要に応じて調整

            try
            {
                var bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length, cts.Token);
                if (bytesRead == 0)
                    return Array.Empty<byte>();

                var result = new byte[bytesRead];
                Array.Copy(buffer, result, bytesRead);
                return result;
            }
            catch (TaskCanceledException)
            {
                throw new TimeoutException("受信タイムアウトが発生しました");
            }
        }

        private void OnConnectionStatusChanged(bool isConnected, string message)
        {
            ConnectionStatusChanged?.Invoke(this, new ConnectionEventArgs(isConnected, message));
        }
    }
}
