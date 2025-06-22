using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultimeterCtrl
{
    public class SerialPortConnection : IConnection
    {
        private SerialPort _serialPort;
        private readonly string _portName;
        private readonly int _baudRate;
        private readonly Parity _parity;
        private readonly int _dataBits;
        private readonly StopBits _stopBits;
        private bool _isConnected;

        public SerialPortConnection(string portName, int baudRate = 9600, Parity parity = Parity.None,
                                   int dataBits = 8, StopBits stopBits = StopBits.One)
        {
            _portName = portName;
            _baudRate = baudRate;
            _parity = parity;
            _dataBits = dataBits;
            _stopBits = stopBits;

            _serialPort = new SerialPort(_portName, _baudRate, _parity, _dataBits, _stopBits)
            {
                ReadTimeout = 1000,
                WriteTimeout = 1000
            };
        }

        public bool IsConnected => _isConnected;

        public event EventHandler<ConnectionEventArgs> ConnectionStatusChanged;

        public async Task<bool> ConnectAsync()
        {
            try
            {
                if (_isConnected)
                    return true;

                if (_serialPort == null || !_serialPort.IsOpen)
                {
                    _serialPort = new SerialPort(_portName, _baudRate, _parity, _dataBits, _stopBits);
                    _serialPort.Open();
                    _isConnected = true;
                    OnConnectionStatusChanged(true, "シリアル接続が確立されました");
                }

                return true;
            }
            catch (Exception ex)
            {
                OnConnectionStatusChanged(false, $"シリアル接続に失敗しました: {ex.Message}");
                return false;
            }

            // SerialPort.OpenAsync()が存在しないため、Task.CompletedTaskを返します
            return await Task.FromResult(_isConnected);
        }

        public async Task DisconnectAsync()
        {
            if (!_isConnected)
                return;

            try
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    _serialPort.Close();
                    _serialPort.Dispose();
                    _isConnected = false;
                    OnConnectionStatusChanged(false, "シリアル接続が切断されました");
                }
            }
            catch (Exception ex)
            {
                OnConnectionStatusChanged(false, $"シリアル切断中にエラーが発生しました: {ex.Message}");
            }

            await Task.CompletedTask;
        }

        public async Task SendAsync(byte[] data)
        {
            if (!_isConnected || _serialPort == null || !_serialPort.IsOpen)
                throw new InvalidOperationException("シリアル接続が確立されていません");

            await Task.Run(() => _serialPort.Write(data, 0, data.Length));
        }

        public async Task<byte[]> ReceiveAsync(int timeout = 1000)
        {
            if (!_isConnected || _serialPort == null || !_serialPort.IsOpen)
                throw new InvalidOperationException("シリアル接続が確立されていません");

            // タイムアウト設定
            _serialPort.ReadTimeout = timeout;

            return await Task.Run(() =>
            {
                try
                {
                    // 利用可能なバイト数を確認
                    int bytesToRead = _serialPort.BytesToRead;
                    if (bytesToRead == 0)
                    {
                        // データが来るまで一定時間待機
                        System.Threading.Thread.Sleep(100);
                        bytesToRead = _serialPort.BytesToRead;

                        if (bytesToRead == 0)
                            return Array.Empty<byte>();
                    }

                    byte[] buffer = new byte[bytesToRead];
                    _serialPort.Read(buffer, 0, bytesToRead);
                    return buffer;
                }
                catch (TimeoutException)
                {
                    return Array.Empty<byte>();
                }
            });
        }

        private void OnConnectionStatusChanged(bool isConnected, string message)
        {
            ConnectionStatusChanged?.Invoke(this, new ConnectionEventArgs(isConnected, message));
        }
    }
}

