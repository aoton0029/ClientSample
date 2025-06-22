using MultimeterCtrl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InspectionApp
{
    public interface IMultimeter
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
        event EventHandler<MultimeterErrorEventArgs> ErrorOccurred;

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
        /// 電圧測定を実行します
        /// </summary>
        /// <returns>測定された電圧値</returns>
        Task<double> MeasureVoltageAsync();

        /// <summary>
        /// 電流測定を実行します
        /// </summary>
        /// <returns>測定された電流値</returns>
        Task<double> MeasureCurrentAsync();

        /// <summary>
        /// 測定機能を設定します
        /// </summary>
        /// <param name="function">測定機能（"VOLT:DC", "VOLT:AC", "CURR:DC", "CURR:AC", "RES", "FRES", "TEMP"など）</param>
        Task SetFunctionAsync(string function);

        /// <summary>
        /// 現在の測定機能を取得します
        /// </summary>
        /// <returns>現在の測定機能</returns>
        Task<string> GetFunctionAsync();

        /// <summary>
        /// デバイスのエラーメッセージを取得します
        /// </summary>
        /// <returns>エラーメッセージ（エラーがない場合は空文字）</returns>
        Task<string> GetErrorMessageAsync();
    }

    /// <summary>
    /// マルチメーターエラーイベントの引数クラス
    /// </summary>
    public class MultimeterErrorEventArgs : EventArgs
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
        public MultimeterErrorEventArgs(Exception exception, int errorCode, string message)
        {
            Exception = exception;
            ErrorCode = errorCode;
            Message = message;
        }
    }
}
