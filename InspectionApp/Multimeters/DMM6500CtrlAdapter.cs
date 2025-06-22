using MultimeterCtrl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InspectionApp
{
    public class DMM6500CtrlAdapter : IMultimeter
    {
        private readonly MMCtrl _mmCtrl;

        /// <summary>
        /// 接続状態を取得します
        /// </summary>
        public bool IsConnected => _mmCtrl?.IsConnected ?? false;

        /// <summary>
        /// 初期化状態を取得します
        /// </summary>
        public bool IsInitialized => _mmCtrl?.IsInitialized ?? false;

        /// <summary>
        /// 接続状態が変化したときに発生するイベント
        /// </summary>
        public event EventHandler<ConnectionStatusChangedEventArgs> ConnectionStatusChanged;

        /// <summary>
        /// エラーが発生したときに発生するイベント
        /// </summary>
        public event EventHandler<MultimeterErrorEventArgs> ErrorOccurred;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="connection">使用する接続インターフェース</param>
        public DMM6500CtrlAdapter(IConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            _mmCtrl = new MMCtrl(connection);
            _mmCtrl.ConnectionStatusChanged += MmCtrl_ConnectionStatusChanged;
            _mmCtrl.ErrorOccurred += MmCtrl_ErrorOccurred;
        }

        /// <summary>
        /// デバイスに接続します
        /// </summary>
        /// <returns>接続結果</returns>
        public async Task<bool> ConnectAsync()
        {
            return await _mmCtrl.ConnectAsync();
        }

        /// <summary>
        /// デバイスから切断します
        /// </summary>
        public async Task DisconnectAsync()
        {
            await _mmCtrl.DisconnectAsync();
        }

        /// <summary>
        /// デバイスを初期化します
        /// </summary>
        public async Task InitializeAsync()
        {
            await _mmCtrl.InitializeAsync();
        }

        /// <summary>
        /// 電圧測定を実行します
        /// </summary>
        /// <returns>測定された電圧値</returns>
        public async Task<double> MeasureVoltageAsync()
        {
            return await _mmCtrl.MeasureVoltageAsync();
        }

        /// <summary>
        /// 電流測定を実行します
        /// </summary>
        /// <returns>測定された電流値</returns>
        public async Task<double> MeasureCurrentAsync()
        {
            return await _mmCtrl.MeasureCurrentAsync();
        }

        /// <summary>
        /// 測定機能を設定します
        /// </summary>
        /// <param name="function">測定機能（"VOLT:DC", "VOLT:AC", "CURR:DC", "CURR:AC", "RES", "FRES", "TEMP"など）</param>
        public async Task SetFunctionAsync(string function)
        {
            await _mmCtrl.SetFunctionAsync(function);
        }

        /// <summary>
        /// 現在の測定機能を取得します
        /// </summary>
        /// <returns>現在の測定機能</returns>
        public async Task<string> GetFunctionAsync()
        {
            return await _mmCtrl.GetFunctionAsync();
        }

        /// <summary>
        /// デバイスのエラーメッセージを取得します
        /// </summary>
        /// <returns>エラーメッセージ（エラーがない場合は空文字）</returns>
        public async Task<string> GetErrorMessageAsync()
        {
            return await _mmCtrl.GetErrorMessageAsync();
        }

        /// <summary>
        /// MMCtrlからのエラーイベントをハンドリングします
        /// </summary>
        private void MmCtrl_ErrorOccurred(object sender, MultimeterCtrl.MultimeterErrorEventArgs e)
        {
            // MultimeterCtrlのErrorCodeをInspectionAppのErrorCodeに変換
            int errorCode = (int)e.ErrorCode;
            ErrorOccurred?.Invoke(this, new MultimeterErrorEventArgs(e.Exception, errorCode, e.Message));
        }

        /// <summary>
        /// MMCtrlからの接続状態変更イベントをハンドリングします
        /// </summary>
        private void MmCtrl_ConnectionStatusChanged(object sender, ConnectionEventArgs e)
        {
            // ConnectionEventArgsをConnectionStatusChangedEventArgsに変換
            ConnectionStatusChanged?.Invoke(this, new ConnectionStatusChangedEventArgs(e.IsConnected));
        }
    }

}
