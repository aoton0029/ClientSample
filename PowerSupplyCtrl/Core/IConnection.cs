using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowersupplyCtrl
{
    public interface IConnection
    {
        // 接続を確立するメソッド
        Task<bool> ConnectAsync();

        // 接続を切断するメソッド
        Task DisconnectAsync();

        // 現在の接続状態を確認するプロパティ
        bool IsConnected { get; }

        // データを送信するメソッド
        Task SendAsync(byte[] data);

        // データを受信するメソッド
        Task<byte[]> ReceiveAsync(int timeout = 1000);

        // 接続イベント
        event EventHandler<ConnectionEventArgs> ConnectionStatusChanged;
    }

    // 接続イベント用の引数クラス
    public class ConnectionEventArgs : EventArgs
    {
        public bool IsConnected { get; }
        public string Message { get; }

        public ConnectionEventArgs(bool isConnected, string message = "")
        {
            IsConnected = isConnected;
            Message = message;
        }
    }
}
