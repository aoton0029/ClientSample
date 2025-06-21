using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Device1
{
    /// <summary>
    /// Device1を制御するためのクラス
    /// </summary>
    public class Device1Ctrl
    {
        private readonly CommandHandler _commandHandler;
        private readonly IConnection _connection;
        private bool _isInitialized = false;

        /// <summary>
        /// Device1Ctrlを初期化します
        /// </summary>
        /// <param name="connection">使用する接続インターフェース</param>
        public Device1Ctrl(IConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _commandHandler = new CommandHandler(connection);

            // 接続状態の変更イベントをハンドリング
            _connection.ConnectionStatusChanged += OnConnectionStatusChanged;
        }

        /// <summary>
        /// デバイスの接続状態が変化したときに発生するイベント
        /// </summary>
        public event EventHandler<ConnectionEventArgs> ConnectionStatusChanged;

        /// <summary>
        /// デバイスが現在接続されているかどうかを取得します
        /// </summary>
        public bool IsConnected => _connection?.IsConnected ?? false;

        /// <summary>
        /// デバイスが初期化されているかどうかを取得します
        /// </summary>
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// デバイスに接続します
        /// </summary>
        /// <returns>接続に成功した場合はtrue</returns>
        public async Task<bool> ConnectAsync()
        {
            return await _connection.ConnectAsync();
        }

        /// <summary>
        /// デバイスから切断します
        /// </summary>
        public async Task DisconnectAsync()
        {
            await _connection.DisconnectAsync();
        }

        /// <summary>
        /// デバイスを初期化します
        /// </summary>
        public async Task InitializeAsync()
        {
            if (!IsConnected)
                throw new InvalidOperationException("デバイスに接続されていません。");

            // デバイスをリセット
            await _commandHandler.ResetInstrumentAsync();

            // リモートモードに設定
            await _commandHandler.SetRemoteModeAsync();

            // デバイスID情報を取得して確認
            string idInfo = await _commandHandler.GetIdentificationAsync();
            if (string.IsNullOrEmpty(idInfo))
                throw new InvalidOperationException("デバイスからの応答がありません。");

            _isInitialized = true;
        }

        /// <summary>
        /// 出力状態を設定します
        /// </summary>
        /// <param name="enable">出力を有効にする場合はtrue</param>
        /// <param name="channel">チャンネル番号（省略可）</param>
        public async Task SetOutputStateAsync(bool enable, int? channel = null)
        {
            await _commandHandler.SetOutputStateAsync(enable, channel);
        }

        /// <summary>
        /// 出力状態を取得します
        /// </summary>
        /// <param name="channel">チャンネル番号（省略可）</param>
        /// <returns>出力状態（true=ON, false=OFF）</returns>
        public async Task<bool> GetOutputStateAsync(int? channel = null)
        {
            return await _commandHandler.GetOutputStateAsync(channel);
        }

        /// <summary>
        /// 電圧測定値を取得します
        /// </summary>
        /// <returns>電圧測定値</returns>
        public async Task<double> GetVoltageAsync()
        {
            return await _commandHandler.GetVolt();
        }

        /// <summary>
        /// 電流測定値を取得します
        /// </summary>
        /// <returns>電流測定値</returns>
        public async Task<double> GetCurrentAsync()
        {
            return await _commandHandler.GetCurrentAsync();
        }

        /// <summary>
        /// 抵抗測定値を取得します
        /// </summary>
        /// <returns>抵抗測定値</returns>
        public async Task<double> GetResistanceAsync()
        {
            return await _commandHandler.GetResistanceAsync();
        }

        /// <summary>
        /// 温度測定値を取得します
        /// </summary>
        /// <returns>温度測定値</returns>
        public async Task<double> GetTemperatureAsync()
        {
            return await _commandHandler.GetTemperatureAsync();
        }

        /// <summary>
        /// エラーメッセージを取得します
        /// </summary>
        /// <returns>エラーメッセージ（エラーがない場合は空文字）</returns>
        public async Task<string> GetErrorMessageAsync()
        {
            return await _commandHandler.GetErrorMessageAsync();
        }

        /// <summary>
        /// デバイスをローカル操作モードに設定します
        /// </summary>
        public async Task SetLocalModeAsync()
        {
            await _commandHandler.SetLocalModeAsync();
        }

        private void OnConnectionStatusChanged(object sender, ConnectionEventArgs e)
        {
            if (!e.IsConnected)
                _isInitialized = false;

            ConnectionStatusChanged?.Invoke(this, e);
        }
    }
}
