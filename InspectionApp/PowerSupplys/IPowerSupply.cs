using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InspectionApp
{
    /// <summary>
    /// 電源装置の共通インターフェース
    /// </summary>
    public interface IPowerSupply
    {
        /// <summary>
        /// 接続状態を取得します
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// 初期化状態を取得します
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// 接続状態が変化したときに発生するイベント
        /// </summary>
        event EventHandler<ConnectionStatusChangedEventArgs> ConnectionStatusChanged;

        /// <summary>
        /// エラーが発生したときに発生するイベント
        /// </summary>
        event EventHandler<PowerSupplyErrorEventArgs> ErrorOccurred;

        /// <summary>
        /// デバイスに接続します
        /// </summary>
        /// <returns>接続結果</returns>
        Task<bool> ConnectAsync();

        /// <summary>
        /// デバイスから切断します
        /// </summary>
        Task DisconnectAsync();

        /// <summary>
        /// デバイスを初期化します
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// 電圧値を設定します
        /// </summary>
        /// <param name="voltage">設定電圧値（単位: V）</param>
        /// <param name="channel">チャンネル番号（省略可）</param>
        Task SetVoltageAsync(double voltage, int? channel = null);

        /// <summary>
        /// 設定されている電圧値を取得します
        /// </summary>
        /// <param name="channel">チャンネル番号（省略可）</param>
        /// <returns>設定電圧値（単位: V）</returns>
        Task<double> GetVoltageSettingAsync(int? channel = null);

        /// <summary>
        /// 電流値を設定します
        /// </summary>
        /// <param name="current">設定電流値（単位: A）</param>
        /// <param name="channel">チャンネル番号（省略可）</param>
        Task SetCurrentAsync(double current, int? channel = null);

        /// <summary>
        /// 設定されている電流値を取得します
        /// </summary>
        /// <param name="channel">チャンネル番号（省略可）</param>
        /// <returns>設定電流値（単位: A）</returns>
        Task<double> GetCurrentSettingAsync(int? channel = null);

        /// <summary>
        /// 出力状態を設定します
        /// </summary>
        /// <param name="enable">出力を有効にする場合はtrue</param>
        /// <param name="channel">チャンネル番号（省略可）</param>
        Task SetOutputStateAsync(bool enable, int? channel = null);

        /// <summary>
        /// 出力状態を取得します
        /// </summary>
        /// <param name="channel">チャンネル番号（省略可）</param>
        /// <returns>出力状態（true=ON, false=OFF）</returns>
        Task<bool> GetOutputStateAsync(int? channel = null);

        /// <summary>
        /// 実測電圧値を取得します
        /// </summary>
        /// <param name="channel">チャンネル番号（省略可）</param>
        /// <returns>実測電圧値（単位: V）</returns>
        Task<double> MeasureVoltageAsync(int? channel = null);

        /// <summary>
        /// 実測電流値を取得します
        /// </summary>
        /// <param name="channel">チャンネル番号（省略可）</param>
        /// <returns>実測電流値（単位: A）</returns>
        Task<double> MeasureCurrentAsync(int? channel = null);

        /// <summary>
        /// デバイスのエラーメッセージを取得します
        /// </summary>
        /// <returns>エラーメッセージ（エラーがない場合は空文字）</returns>
        Task<string> GetErrorMessageAsync();
    }

    /// <summary>
    /// 接続状態変更イベントの引数クラス
    /// </summary>
    public class ConnectionStatusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 接続状態
        /// </summary>
        public bool IsConnected { get; }

        /// <summary>
        /// メッセージ
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="isConnected">接続状態</param>
        /// <param name="message">メッセージ</param>
        public ConnectionStatusChangedEventArgs(bool isConnected, string message = "")
        {
            IsConnected = isConnected;
            Message = message;
        }
    }

    /// <summary>
    /// 電源装置エラーイベントの引数クラス
    /// </summary>
    public class PowerSupplyErrorEventArgs : EventArgs
    {
        /// <summary>
        /// 例外
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// エラーコード
        /// </summary>
        public int ErrorCode { get; }

        /// <summary>
        /// エラーメッセージ
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="exception">例外</param>
        /// <param name="errorCode">エラーコード</param>
        /// <param name="message">エラーメッセージ</param>
        public PowerSupplyErrorEventArgs(Exception exception, int errorCode, string message)
        {
            Exception = exception;
            ErrorCode = errorCode;
            Message = message;
        }
    }
}
